/*********************************************************************
*
* ANSI C Example program:
*    ContWriteDigPort-ExtClk.c
*
* Example Category:
*    DO
*
* Description:
*    This example demonstrates how to output a continuous digital
*    pattern using an external clock.
*
* Instructions for Running:
*    1. Select the Physical Channel to correspond to where your
*       signal is output on the DAQ device.
*    2. Select the Clock Source for the generation.
*    3. Specify the Rate of the output digital pattern.
*    4. Enter the digital pattern data.
*
* Steps:
*    1. Create a task.
*    2. Create an Digital Output Channel.
*    3. Call the DAQmxCfgSampClkTiming function which sets the sample
*       clock rate. Additionally, set the sample mode to continuous.
*    4. Write the data to the output buffer.
*    5. Call the Start function to start the task.
*    6. Wait until the user presses the Stop button.
*    7. Call the Clear Task function to clear the Task.
*    8. Display an error if any.
*
* I/O Connections Overview:
*    Make sure your signal output terminal matches the Physical
*    Channel I/O Control. Also, make sure your external clock
*    terminal matches the Clock Source Control. For further
*    connection information, refer to your hardware reference manual.
*
*********************************************************************/

#include <stdio.h>
#include <conio.h>
#include <ctype.h>
#include <string.h>
#include <stdlib.h>
#include <NIDAQmx.h>
#include <omp.h>
#include <math.h>
#include <time.h>
#include <windows.h>
#include <winbase.h>
#include <C:\Users\greinerlab\Documents\GitHub\RbRepository\software\exp_control\NI_653x_routines\exp_def.h>
#include <C:\Users\greinerlab\Documents\GitHub\RbRepository\software\exp_control\NI_653x_routines\exp_var.h>
//#include <C:\Users\greinerlab\Documents\GitHub\RbRepository\software\exp_control\NI_653x_routines\Dio64.h>

#define OPENMP_FOR 0
#define NTHREADS 4
//#define DEBUG_DIO 0
#define DEBUG_NI653X 0
//#define NIVISA_PXI

typedef TaskHandle (__stdcall* configure_653x)(char* dev, char *trigger, char *clock, int total_cycles);
typedef void (__stdcall* initialize_data)(uInt32* NI_waveform, int total_cycles);
typedef void (__stdcall* transpose_data)(uInt32* NI_waveform);
typedef int (__stdcall* write_to_653x)(TaskHandle taskHandle, uInt32* NI_waveform);
typedef void (__stdcall* release_data)(uInt32 *NI_waveform);
typedef void (__stdcall* insert_ramp)(double t_start, double t_stop, double y_start, double y_stop, 
                        int dat_chan, uInt32 NI_waveform[]);
typedef void (__stdcall* insert_sine)(double offset, double amp, double freq,
						double t_start, double t_stop, 
                        int dat_chan, uInt32 NI_waveform[]);


typedef unsigned int (_cdecl *DIO64_OpenResource)(const char *resourceName, WORD board, WORD baseio);
typedef unsigned int (_cdecl *DIO64_Load)(WORD board, char* rbfFile, int inputHint, int outputHint);
//typedef unsigned int (_stdcall *DIO64_OpenResource)(WORD board, WORD baseio);
typedef int (_stdcall *DIO64_Open)(unsigned short board, unsigned short baseio);


int main(int argc, char *argv[]) {
	/* importing DIO64_Visa32.dll */
	BOOL freeResult, runTimeLinkSuccess = FALSE; 
	HINSTANCE dllHandle = NULL;
	configure_653x configure_653xPtr = NULL;
	initialize_data initialize_dataPtr = NULL;
	transpose_data transpose_dataPtr = NULL;
	write_to_653x write_to_653xPtr = NULL;
	release_data release_dataPtr = NULL;
	insert_ramp insert_rampPtr = NULL;
	insert_sine insert_sinePtr = NULL;
	int lastError;
	int retVal = 0;

#ifdef DEBUG_DIO
	HINSTANCE DIOdllHandle = NULL;
	DIO64_OpenResource DIO64_OpenResourcePtr = NULL;
	DIO64_Open DIO64_OpenPtr = NULL;
	DIO64_Load DIO64_LoadPtr = NULL;
	const char resourceName[] = "PXI4::0::INSTR";
	char *rbfFile = "C:\\DIO64Visa\\DIO64.CAT";
	unsigned short board = 0;
	unsigned short baseio = 384;
	int ihint = -1;
	int ohint = -1;
#endif

    int total_cycles = 40000000 - 2600;
	int nsamples = total_cycles + NUMVCOCALCYCLES + 2*50;
    TaskHandle taskHandle = NULL;

	//int nsamples = n_offset + 31500000;

	/* Allocate memory for the data to be set to PCIe 653x card */
    uInt32 *NI_waveform;
	
	printf("allocation NI_waveform\n");
    /* Allocate memory and initialize 'NI_waveform' matrix */
	NI_waveform = (uInt32 *)malloc(BPW * nsamples * sizeof(uInt32));
    if (NI_waveform == NULL) {
        fprintf(stderr, "out of memory\n");
    }
    memset(&NI_waveform[0], 0, BPW * nsamples * sizeof(uInt32));
    

	/************** DIO64 *********************/
	printf("loading library\n");
	///* Load the dll and keep the handle to it */
	//dllHandle = LoadLibrary("ni653x.dll");

#ifdef DEBUG_DIO
	printf("Attempting to load the 'DIO64_Visa32.dll' library.\n");
	retVal = SetDllDirectory("C:\\DIO64Visa\\");
	DIOdllHandle = LoadLibrary("DIO64_Visa32.dll");
	//DIOdllHandle = LoadLibraryEx("DIO64_Visa32.dll", NULL, DONT_RESOLVE_DLL_REFERENCES);
	/* If the DIO handle is valid, try to get the function address. */
    if (NULL != DIOdllHandle) {
		printf("Got the handle!\n");
		DIO64_OpenResourcePtr = (DIO64_OpenResource)GetProcAddress(DIOdllHandle, "DIO64_OpenResource");
		DIO64_LoadPtr = (DIO64_Load)GetProcAddress(DIOdllHandle, "DIO64_Load");
		if (runTimeLinkSuccess = (NULL != DIO64_OpenResourcePtr )) {
            printf("Found the 'DIO64_OpenResource' function!\n");
	        retVal = (*DIO64_OpenResourcePtr)("PXI4::0::INSTR", 0, 384);
            printf("'DIO64_OpenResource' successfully ran? retVal = %d.\n", retVal);
        } else {
			lastError = GetLastError();
            printf("Got the handle, but wasn't able to find the function. Error (%d).\n", lastError);
        }
		if (runTimeLinkSuccess = (NULL != DIO64_LoadPtr )) {
            printf("Found the 'DIO64_Load' function!\n");
	        retVal = (*DIO64_LoadPtr)(board, rbfFile, ihint, ohint);
            printf("'DIO64_Load' successfully ran? retVal = %d.\n", retVal);
        } else {
			lastError = GetLastError();
            printf("Got the handle, but wasn't able to find the function. Error (%d).\n", lastError);
        }
	} else {
        lastError = GetLastError();
        printf("Unable to get a handle for the 'DIO64_Visa32.dll' driver. Error (%d).\n", lastError);
    }
#endif

#ifdef DEBUG_NI653X
	retVal = SetDllDirectory(L"C:\\Users\\greinerlab\\Documents\\GitHub\\RbRepository\\software\\exp_control\\NI_653x_routines\\ni653x\\x64\\Debug\\");
	dllHandle = LoadLibrary(L"ni653x.dll");
	printf("retVal = %d \n", retVal);
	/* If the handle is valid, try to get the function address. */
    if (NULL != dllHandle) {
		/* Get pointer to our function using GetProcAddress: */
		configure_653xPtr = (configure_653x)GetProcAddress(dllHandle, "configure_653x");
		/* If the function address is valid, call the function. */
		if (runTimeLinkSuccess = (NULL != configure_653xPtr)) {
        } else {
			printf("got the handle, but couldn't find \"configure_653x\"...\n");
			lastError = GetLastError();
			printf("failed in loading dio64_visa32.dll (%d)\n", lastError );
			getchar();
			return 0;
		}

		initialize_dataPtr = (initialize_data)GetProcAddress(dllHandle, "initialize_data");
		/* If the function address is valid, call the function. */
		if (runTimeLinkSuccess = (NULL != initialize_dataPtr)) {
        } else {
			printf("Got the handle, but couldn't find \"initialize_data\"...\n");
			lastError = GetLastError();
			printf("Failed in loading DIO64_Visa32.dll (%d)\n", lastError );
			getchar();
			return 0;
		}

		transpose_dataPtr = (transpose_data)GetProcAddress(dllHandle, "transpose_data");
		/* If the function address is valid, call the function. */
		if (runTimeLinkSuccess = (NULL != transpose_dataPtr)) {
        } else {
			printf("Got the handle, but couldn't find \"transpose_data\"...\n");
			lastError = GetLastError();
			printf("Failed in loading DIO64_Visa32.dll (%d)\n", lastError );
			getchar();
			return 0;
		}

		write_to_653xPtr = (write_to_653x)GetProcAddress(dllHandle, "write_to_653x");
		/* If the function address is valid, call the function. */
		if (runTimeLinkSuccess = (NULL != write_to_653xPtr)) {
        } else {
			printf("Got the handle, but couldn't find \"write_data\"...\n");
			lastError = GetLastError();
			printf("Failed in loading DIO64_Visa32.dll (%d)\n", lastError );
			getchar();
			return 0;
		}

		release_dataPtr = (release_data)GetProcAddress(dllHandle, "release_data");
		/* If the function address is valid, call the function. */
		if (runTimeLinkSuccess = (NULL != release_dataPtr)) {
        } else {
			printf("Got the handle, but couldn't find \"release_data\"...\n");
			lastError = GetLastError();
			printf("Failed in loading DIO64_Visa32.dll (%d)\n", lastError );
			getchar();
			return 0;
		}

		insert_rampPtr = (insert_ramp)GetProcAddress(dllHandle, "insert_ramp");
		/* If the function address is valid, call the function. */
		if (runTimeLinkSuccess = (NULL != insert_rampPtr)) {
        } else {
			printf("Got the handle, but couldn't find \"insert_linear_ramp\"...\n");
			lastError = GetLastError();
			printf("Failed in loading DIO64_Visa32.dll (%d)\n", lastError );
			getchar();
			return 0;
		}

		insert_sinePtr = (insert_sine)GetProcAddress(dllHandle, "insert_sine");
		/* If the function address is valid, call the function. */
		if (runTimeLinkSuccess = (NULL != insert_sinePtr)) {
        } else {
			printf("Got the handle, but couldn't find \"insert_sine\"...\n");
			lastError = GetLastError();
			printf("Failed in loading DIO64_Visa32.dll (%d)\n", lastError );
			getchar();
			return 0;
		}
	} else {
		printf("No handle\n");
		lastError = GetLastError();
	    printf("Failed in loading DIO64_Visa32.dll (%d)\n", lastError );
        getchar();
        return 0;
	}

	taskHandle = (*configure_653xPtr)("/dev1/line0:31", NULL, "OnboardClock", nsamples);
	(*initialize_dataPtr)(NI_waveform, total_cycles);
	printf("------finished initializing----------\n");
	(*insert_rampPtr)(0, 5, 0, 54000, DIO23, NI_waveform);
	(*insert_rampPtr)(5, 0, 0, 54000, DIO27, NI_waveform);
	printf("------done with linear ramps---------\n");
	(*insert_sinePtr)(2.5, 2, 10, 0, 42002, DIO30, NI_waveform);
	(*insert_sinePtr)(2.5, 2, 10, 0, 40020, DIO26, NI_waveform);
	printf("------now transposing----------------\n");
	(*transpose_dataPtr)(NI_waveform);
	printf("------ let's write-------------------\n");
	retVal = (*write_to_653xPtr)(taskHandle, NI_waveform);
	printf("------ done writing------------------\n");
	getchar();
	(*release_dataPtr)(NI_waveform);
#endif
	getchar();
	printf("Done.");

	/* Free the library: */
	freeResult = FreeLibrary(dllHandle);       
	/************** BEGIN WRITING *************/
	return 0;
}
