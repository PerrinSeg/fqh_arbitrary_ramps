/*********************************************************************
*
* Driver for NI PCI-653x
*
*********************************************************************/

#include <windows.h>
#include <ctype.h>
#include <stdio.h>
#include <C:\Program Files (x86)\National Instruments\NI-DAQ\DAQmx ANSI C Dev\include\NIDAQmx.h>
#include <string.h>
#include <stdlib.h>
#include <omp.h>
#include <math.h>
#include <time.h>
/* #include <C:\Users\greinerlab\Documents\RbRepository\software\exp_control\NI_653x_routines\exp_def.h> */
#include <C:\Users\Rb Lab\Documents\NI_653x_routines\ni653x\ni653x\exp_def.h>
//#include <exp_def.h>

//#define DEBUG_CHANNELS 0
//#define DEBUG_TRANSPOSE 0
//#define DEBUG_REG_EDITS 0
//#define DEBUGCS 0
//#define SINGLETHREAD 0
#define OPENMP_FOR 0
#define NTHREADS 8
//#define BLOCKSIZE 1480000 
#define BLOCKSIZE 7650000
/* data is streamed to the 653x in chunks of size BLOCKSIZE*4 bytes. The '4 bytes' 
 * is because each slice of time is 32 bits wide. Hence, a 'BLOCK' represents
 * 'BLOCKSIZE' number of 40 ns (= 1/25 MHz) width time slices. If transposing is done
 * between streaming of individual chunks, this number must be a multiple of BPW. Thus,
 * the 'transpose32c' routine will be run an integer number of times. (however,
 * transposing between streaming of blocks seems to cause timing errors as of
 * 06/25/2014. the code to do this can be activated by defining
 * 'TRANSPOSE_DREAMS;) */
//#define TRANSPOSE_DREAMS
#define PIPE_DREAMS

#define DAQmxErrChk(functionCall) if( DAQmxFailed(error=(functionCall)) ) goto Error; else
#define swap(a0, a1, j, m) t = (a0 ^ (a1 >>j)) & m; \
                           a0 = a0 ^ t; \
                           a1 = a1 ^ (t << j);

int parse_dev_types(char* dev, const char* filedir);
void parse_reg_edits(const char* ad_registry, unsigned int** AD_reg_edits);
void initialize_NI_waveform(int card_number, uInt32 *NI_waveform);
void insert_reg_edits(uInt32 NI_waveform[], unsigned int AD_reg_edits[][2], int nedits);
void raise_line(uInt32 NI_waveform[], int chan);
void insert_clock(uInt32 NI_waveform[]);
void interleave_zero(unsigned int *x);
void transpose32c(uInt32 A[32], uInt32 B[32]);
void print_bit_matrix(uInt32 array[], int sample_number);
void print_bit_matrixT(uInt32 array[], int sample_number);
unsigned int AD_convert(double voltage);
unsigned int AD_convert_bipolar(double voltage);
unsigned int (*get_convert_func(unsigned int dat_chan))(double);
double DA_convert(unsigned int dig_volt);
int digitize_time(double real_time);
double tunnel_from_depth(double d);
double volt_from_tunnel(double d);
double volt_from_tunnel_shallow(double d);
double depth_from_tunnel(double t);
double axialdepth_from_2d_depth(double d);
double mod_start_voltage(double offset_depth, double rel_amp, double phase, double calib_volt, double slope);
double mod_end_voltage(double offset_depth, double rel_amp, double phase, double freq_Hz, double duration, double calib_volt, double slope);
double InteractionRatio(double v);
double BlueDeconfinement(double v);
double round(double val);
double depth_from_tunnel_calibrated(double a[11], double t);
double gauge_volt_from_tunnel_calibrated(double a[7], double t);
double gauge_depth_from_tunnel_calibrated(double a[4], double t);

int32 CVICALLBACK DoneCallback(TaskHandle taskHandle, int32 status, void *callbackData);

BOOL WINAPI  DllMain (
            HANDLE    hModule,
            DWORD     dwFunction,
            LPVOID    lpNot)
{
    return TRUE;
}

int tot_words;
int n_offset;
int j_last;
unsigned int devtype[128];
unsigned int validNIchannel;
/* support for up to (4 cards) * 32 devices/card = 128 devices. this can be
 * easily increased by just adding entries. */

_declspec (dllexport) TaskHandle configure_653x(char* dev, char *trigger, char *clock, int words) {
	int32 error = 0;
	//char *errBuff;
	char errBuff[2048] = {'\0'};
    TaskHandle taskHandle = 0;

    //errBuff = (char *)malloc(2048 * sizeof(char));

	tot_words = words; 
    /* sets the number of samples to a number that matches the experiment, not
     * simply the 'max_words' as it had been set by expcontrol when that
     * program invokes 'initialize_data' */

	/*********************************************/
	// DAQmx Configure Code
	/*********************************************/
	DAQmxErrChk (DAQmxCreateTask("",&taskHandle));
	DAQmxErrChk (DAQmxCreateDOChan(taskHandle,dev,"",DAQmx_Val_ChanForAllLines));
#ifndef PIPE_DREAMS
	DAQmxErrChk (DAQmxCfgSampClkTiming(taskHandle, 
                                       clock, 
                                       SMP_CLK, 
                                       DAQmx_Val_Rising, 
                                       DAQmx_Val_FiniteSamps,
                                       words*BPW));
	//DAQmxErrChk (DAQmxCfgOutputBuffer (taskHandle, 10000000) );
	//DAQmxErrChk (DAQmxCfgOutputBuffer (taskHandle, words*BPW) );
#endif 

#ifdef PIPE_DREAMS
	if (clock != NULL) {
		/* If clock is specified, use that clock and export it on "RTSI7" */
		DAQmxErrChk (DAQmxCfgPipelinedSampClkTiming(taskHandle, 
													clock, 
													SMP_CLK,
													DAQmx_Val_Rising,
													DAQmx_Val_ContSamps,
			 										words*BPW));
		
		if (strcmp(clock, "OnboardClock") != 0) {
			printf("exported clock = %s\n", clock);
			DAQmxErrChk (DAQmxExportSignal (taskHandle, DAQmx_Val_SampleClock , "RTSI7") );
			//DAQmxErrChk (DAQmxSetExportedSampClkOutputTerm  (taskHandle, "RTSI7") );
			printf("Expect clock input from %s, which will be exported on RTSI7.\n", clock);
		}
				
	} else {
		/* If no clock is specified, assume clock is on "RTSI7" */
		DAQmxErrChk (DAQmxCfgPipelinedSampClkTiming(taskHandle, 
													"RTSI7", 
													SMP_CLK,
													DAQmx_Val_Rising,
													DAQmx_Val_ContSamps,
													words*BPW));
		printf("Clock sourced from RTSI7.\n");
	}
	
	DAQmxErrChk (DAQmxSetWriteWaitMode(taskHandle, DAQmx_Val_Poll));
	if (words*BPW < 25*BLOCKSIZE) {
		/* large buffer for interactive mode. there is no crazy streaming for interactive mode. */
		DAQmxErrChk (DAQmxCfgOutputBuffer (taskHandle, 20*BLOCKSIZE) );
	} else {
		DAQmxErrChk (DAQmxCfgOutputBuffer (taskHandle, 2*BLOCKSIZE) );
	}

	DAQmxErrChk (DAQmxSetSampClkUnderflowBehavior(taskHandle,DAQmx_Val_PauseUntilDataAvailable));
#endif

	DAQmxErrChk (DAQmxSetWriteRegenMode (taskHandle, DAQmx_Val_DoNotAllowRegen) );

	if (trigger != NULL) {
        if (strcmp(trigger, "RTSI2") == 0) {
            printf("Setting up trigger on %s.\n", trigger);
            DAQmxErrChk (DAQmxCfgDigEdgeStartTrig(taskHandle, trigger, DAQmx_Val_Rising));
            //DAQmxErrChk (DAQmxExportSignal (taskHandle, DAQmx_Val_StartTrigger, "RTSI3") );
        } else if (strcmp(trigger, "RTSI3") == 0) {
            printf("Setting up trigger on %s.\n", trigger);
            DAQmxErrChk (DAQmxCfgDigEdgeStartTrig(taskHandle, trigger, DAQmx_Val_Rising));
            //DAQmxErrChk (DAQmxExportSignal (taskHandle, DAQmx_Val_StartTrigger, "RTSI4") );
        }
	////} else if (trigger != NULL) {
	////	printf("Setting up trigger on %s.\n", trigger);
	////	DAQmxErrChk (DAQmxCfgDigEdgeStartTrig(taskHandle, trigger, DAQmx_Val_Rising));
	////	//DAQmxErrChk (DAQmxExportSignal (taskHandle, DAQmx_Val_StartTrigger, "RTSI2") );
    } else {
		/* if no trigger is specified, then automatically start. */
		printf("Start trigger for %s exported on RTSI2\n", dev);
		DAQmxErrChk (DAQmxExportSignal (taskHandle, DAQmx_Val_StartTrigger, "RTSI2") );
	}

	DAQmxErrChk (DAQmxRegisterDoneEvent(taskHandle, 0, DoneCallback, NULL));
	//free(errBuff);
	return taskHandle;

Error:
	if( DAQmxFailed(error) ) {
		DAQmxGetExtendedErrorInfo(errBuff,2048);
	    //free(errBuff);
    }
	if( DAQmxFailed(error) ) {
		printf("DAQmx Error: %s\n",errBuff);
	    //free(errBuff);
    }
	return taskHandle;
}

_declspec (dllexport) void initialize_data(char* dev, uInt32* NI_waveform, int max_words) {
    /****** Variable Declarations ******/
    int i;
	int nedits = NEDITS;
	int reg_width = 2;
	unsigned int **AD_reg_edits;
        
	/* Variables needed for parsing 'reg_edits.txt' file */
	const char *ad_registry = REGEDITSFILE;
	const char *devtypefiledir = DEVTYPEFILEDIR;
    int card_number;
	time_t  t0, t1; /* time_t is defined on <time.h> and <sys/types.h> as long */
	clock_t c0, c1; /* clock_t is defined on <time.h> and <sys/types.h> as int */

	/* Memory allocation and setting up array 'AD_reg_edits' */
	AD_reg_edits = (unsigned int **)malloc(NEDITS * sizeof(unsigned int *));
    printf("finished AD_reg_edits (from intialize_data)\n ");
	if(AD_reg_edits == NULL) {
		fprintf(stderr, "out of memory\n");
	}
	for(i = 0; i < NEDITS; i++)	{
		AD_reg_edits[i] = (unsigned int *)malloc(reg_width * sizeof(unsigned int));
		if(AD_reg_edits[i] == NULL) {
			fprintf(stderr, "out of memory\n");
		}
	}

	validNIchannel = ((1 << DIO18) 
            + (1 << DIO16)
            + (1 << DIO22)
            + (1 << DIO20)
            + (1 << DIO26)
            + (1 << DIO24)
            + (1 << DIO30)
            + (1 << DIO28)
            + (1 << DIO27)
            + (1 << DIO29)
            + (1 << DIO23)
            + (1 << DIO25)
            + (1 << DIO07)
            + (1 << DIO09)
            + (1 << DIO03)
            + (1 << DIO05)
            + (1 << DIO02)
            + (1 << DIO00)
            + (1 << DIO06)
            + (1 << DIO04)
			+ (1 << DIO10)
            + (1 << DIO08)
            + (1 << DIO14)
            + (1 << DIO12));

    /* 'n_offset' is the number of words needed for programming the AD9522
     * clock chip and the number of digital words needed for VCO calibration. the
     * experiment starts after 'n_offset' words ('BPW' bits per word) */
	n_offset = 2*nedits + NUMVCOCALCYCLES;
    j_last = n_offset;
	tot_words = max_words;

    /* populate the 'devtype' array with the appropriate device IDs for this
     * card */
    card_number = parse_dev_types(dev, devtypefiledir);
    printf("finished \"parse_dev_types\": \n\tdev %c \ndirectory: %c \ncard number: %d\n", *dev, *devtypefiledir, card_number);
    /* Raise the appropriate active-low lines, insert the clock signals on the
     * REFIN line, and set default voltage on DAs to 0V (correctly, according to
     * their corresponding device type). This must occur before
     * 'insert_reg_edits', which will overwrite portions of the initialized
     * 'NI_waveform' array. */
    printf("starting initialize_NI_waveform...\n");
    initialize_NI_waveform(card_number, NI_waveform);
    printf("...finished initialize_NI_waveform\n");
    /* Registry Modifications */
    /* Parse 'reg_edits.txt' file to find the appropriate registry (address,
     * value) data. */
    printf("starting parse_reg_edits...\n");
    parse_reg_edits(ad_registry, AD_reg_edits);
    printf("...finished parse_reg_edits\n");
    #ifdef DEBUG_REG_EDITS
	for (i = 0; i < NEDITS; i++) {
		printf("(%x, %d)\n", AD_reg_edits[i][0], AD_reg_edits[i][1]);
	}
    #endif
	
    /* Now that 'AD_reg_edits' has the appropriate information, insert the
     * correct data into 'NI_waveform' using 'insert_reg_edits'. Note that 
     * 'insert_reg_edits' is call by reference, hence it will change
     * the 'AD_reg_edits' array */
	printf("Inserting registry edits of the AD9522.\n");
    insert_reg_edits(NI_waveform, AD_reg_edits, nedits);
	printf("Done registry edits of the AD9522.\n");

    // /* Beginning Trigger */
	// for (i = 0; i < NUMVCOCALCYCLES/2; i++) {
	// 	NI_waveform[BPW*(n_offset - i) + CSTop] = 0xFFFFFFFF;
	// 	NI_waveform[BPW*(n_offset - i) + CSBot] = 0xFFFFFFFF;
	// }
	// for (i = NUMVCOCALCYCLES/2; i < NUMVCOCALCYCLES; i++) {
	// 	NI_waveform[BPW*(n_offset - i) + CSTop] = 0xFFFFFFFF;
	// 	NI_waveform[BPW*(n_offset - i) + CSBot] = 0xFFFFFFFF;
	// }

    /* Beginning Trigger */
    //printf("creating trigger");
	for (i = 0; i < NUMVCOCALCYCLES; i++) {
		NI_waveform[BPW*(n_offset - i) + CSTop] = 0xFFFFFFFF;
		NI_waveform[BPW*(n_offset - i) + CSBot] = 0xFFFFFFFF;
    }
    //printf("finished trigger");

    //for (i = 0; i < BPW; i++) {
    //    printf("NI_waveform[%d] = %d.\n", i, NI_waveform[68 + i]);
    //}
	for (i = 0; i < NEDITS; i++) {
		free(AD_reg_edits[i]);
	}
	free(AD_reg_edits);
}

_declspec (dllexport) void transpose_data(uInt32* NI_waveform) {
	int word_counter;
	#ifdef OPENMP_FOR
		int chunksize;
	#endif
    clock_t begin, end;
    double time_spent;
			
    begin = clock();

    #ifndef TRANSPOSE_DREAMS
	printf("Start transpose operation...\n");
    #ifdef SINGLETHREAD
        for (word_counter = 0; word_counter < tot_words; word_counter++) {
            transpose32c(&NI_waveform[word_counter*BPW], &NI_waveform[word_counter*BPW]);
    		}
    #endif

    #ifdef OPENMP_FOR
        chunksize = tot_words/8;
        #pragma omp parallel shared(NI_waveform, chunksize) private(word_counter) num_threads(NTHREADS)
        {
        	#pragma omp for schedule(static, chunksize)
        	for (word_counter = 0; word_counter < tot_words; word_counter++) {
        		transpose32c(&NI_waveform[word_counter*BPW], &NI_waveform[word_counter*BPW]);
        	}
        }
    printf("Done transposing\n");
    #endif
    #endif
    end = clock();
    time_spent = (double)(end - begin) / CLOCKS_PER_SEC;
    printf("transposing NI_waveform: %f seconds\n", time_spent);
}

_declspec (dllexport) int write_to_653x(TaskHandle taskHandle, uInt32* NI_waveform) {
	int32 error = 0;
    int i = 0;
	//char *errBuff;
	char errBuff[2048] = {'\0'};
	uInt32 *buffer_temp;
#ifdef TRANPOSE_DREAMS
	int word_counter;
	#ifdef OPENMP_FOR
		int chunksize;
	#endif
#endif
	int wordsPerBlock = BLOCKSIZE/BPW;

	//errBuff = (char *)malloc(2048 * sizeof(char));
	buffer_temp = (uInt32 *)malloc(BLOCKSIZE*sizeof(uInt32));

	printf("Transferring %d samples to PCIe-653x buffer...\n", BPW*tot_words);

	/*********************************************/
	// DAQmx Write Code
	/*********************************************/
#ifdef PIPE_DREAMS
	if (tot_words*BPW < 20*BLOCKSIZE) {
        #ifdef TRANSPOSE_DREAMS
		#ifdef OPENMP_FOR
		chunksize = tot_words/8;
        #pragma omp parallel shared(NI_waveform, chunksize) private(word_counter) num_threads(NTHREADS)
		{
        	#pragma omp for schedule(static, chunksize)
        	for (word_counter = 0; word_counter < tot_words; word_counter++) {
        		transpose32c(&NI_waveform[word_counter*BPW], &NI_waveform[word_counter*BPW]);
        	}
		}
		#endif
		#endif
		printf("Done transposing\n");
		/* for short sequences, do not bother with crazy streaming */
		DAQmxErrChk (DAQmxWriteDigitalU32(taskHandle, /* the task to write samples to */
										tot_words*BPW, /* number of samples to write */
										0, /* autoStart */
										10.0, /* timeout in seconds */
										DAQmx_Val_GroupByScanNumber, /* interleaved or not */
										NI_waveform, /* data to write to buffer */
										NULL, /* Must be null */
										NULL)); /* API honestly doesn't even say this arg exists */

		//*********************************************/
		//// DAQmx Start Code
		//*********************************************/
		printf("Starting the task...\n");
		DAQmxErrChk (DAQmxStartTask(taskHandle));
	} else {
		/* for long sequences, stream the data in blocks of size 'BLOCKSIZE*sizeof(uInt32)' bytes */
		/* transpose in the dead time between streaming blocks */
        #ifdef TRANSPOSE_DREAMS
		#ifdef OPENMP_FOR
		chunksize = wordsPerBlock/8;
		#pragma omp parallel shared(NI_waveform, chunksize) private(word_counter) num_threads(NTHREADS)
		{
        	#pragma omp for schedule(static, chunksize)
        	for (word_counter = 0; word_counter < wordsPerBlock; word_counter++) {
        		transpose32c(&NI_waveform[word_counter*BPW], &NI_waveform[word_counter*BPW]);
        	}
		}
		#endif
		#endif
		memcpy(buffer_temp, NI_waveform, BLOCKSIZE*sizeof(uInt32));
		DAQmxErrChk (DAQmxWriteDigitalU32(taskHandle, /* the task to write samples to */
											BLOCKSIZE, /* number of samples to write */
											0, /* autoStart */
											10.0, /* timeout in seconds */
											DAQmx_Val_GroupByScanNumber, /* interleaved or not */
											buffer_temp, /* data to write to buffer */
											NULL, /* Must be null */
											NULL)); /* API honestly doesn't even say this arg exists */

		//*********************************************/
		//// DAQmx Start Code
		//*********************************************/
		printf("Starting the task...\n");
		DAQmxErrChk (DAQmxStartTask(taskHandle));

		printf("tot_words*BPW/BLOCKSIZE = %d\n", tot_words*BPW/BLOCKSIZE);
	
		for (i = 1; i < tot_words*BPW/BLOCKSIZE; i++) {
			/* first transpose all the words associated with the block */
            #ifdef TRANSPOSE_DREAMS
			#ifdef OPENMP_FOR
			chunksize = wordsPerBlock/8;
			#pragma omp parallel shared(NI_waveform, chunksize) private(word_counter) num_threads(NTHREADS)
			{
        		#pragma omp for schedule(static, chunksize)
        		for (word_counter = i*wordsPerBlock; word_counter < (i+1)*wordsPerBlock; word_counter++) {
        			transpose32c(&NI_waveform[word_counter*BPW], &NI_waveform[word_counter*BPW]);
        		}
			}
			#endif
            #endif

			/* transfer the transposed region into the buffer memory */
			memcpy(buffer_temp, &NI_waveform[i*BLOCKSIZE], BLOCKSIZE*sizeof(uInt32));

			/* write the buffer to the 653x */
			DAQmxErrChk (DAQmxWriteDigitalU32(taskHandle, /* the task to write samples to */
											BLOCKSIZE, /* number of samples to write */
											0, /* autoStart */
											10.0, /* timeout in seconds */
											DAQmx_Val_GroupByScanNumber, /* interleaved or not */
											buffer_temp, /* data to write to buffer */
											NULL, /* Must be null */
											NULL)); /* API honestly doesn't even say this arg exists */
		}
	}
#endif

	//DAQmxErrChk (DAQmxWaitUntilTaskDone(taskHandle, BLOCKSIZE/SMP_CLK + 2));
	printf("Generating digital output continuously. Press Enter to interrupt\n");
	//getchar();

Error:
	if( DAQmxFailed(error) ) {
		DAQmxGetExtendedErrorInfo(errBuff,2048);
        //free(errBuff);
    }
	if( DAQmxFailed(error) ) {
		printf("DAQmx Error: %s\n",errBuff);
		printf("failed on loop iteration i = %d\n", i);
        //free(errBuff);
	}
	printf("End of program.\n");
    free(buffer_temp);
    //free(errBuff);
	return 0;
}

_declspec (dllexport) void release_data() {
	/* For potential future use? */
	printf("Data freed.\n");
}

_declspec (dllexport) void release_task(TaskHandle taskHandle) {
	printf("============ Task Closed =============.\n");
	if( taskHandle!=0 ) {
		/*********************************************/
		// DAQmx Stop Code
		/*********************************************/
		DAQmxStopTask(taskHandle);
		DAQmxClearTask(taskHandle);
	}
}

int32 CVICALLBACK DoneCallback(TaskHandle taskHandle, int32 status, void *callbackData) {
	int32   error=0;
	//char *errBuff;
	char errBuff[2048] = {'\0'};

    //errBuff = (char *)malloc(2048 * sizeof(char));

	// Check to see if an error stopped the task.
	DAQmxErrChk (status);

Error:
	if( DAQmxFailed(error) ) {
		DAQmxGetExtendedErrorInfo(errBuff,2048);
		DAQmxClearTask(taskHandle);
		printf("DAQmx Error: %s\n",errBuff);
        //free(errBuff);
	}
    //free(errBuff);
	return 0;
}

_declspec (dllexport) double get_total_time(void) {
    int last_time_j = j_last + n_offset;
    double last_time;

    last_time = last_time_j*BPW/SMP_CLK*1000;
    return last_time;
}

_declspec (dllexport) void disable_clk_dist(double t_start, double t_stop, uInt32 *NI_waveform) {
    int j;                      /* loop index */
	int i;                      /* loop index */
	int j_start = digitize_time(t_start);
    int j_stop = digitize_time(t_stop);
	int j_times[2] = {j_start, j_stop - 4};
    int nedits = 5;
    int reg_width = 2;
	unsigned int **AD_reg_edits;
	AD_reg_edits = (unsigned int **)malloc(nedits * sizeof(unsigned int *));

    if (j_stop > j_last) {
        j_last = j_stop;
    }
	
	if(AD_reg_edits == NULL) {
		fprintf(stderr, "out of memory\n");
	}
	for(i = 0; i < nedits; i++)	{
		AD_reg_edits[i] = (unsigned int *)malloc(reg_width * sizeof(unsigned int));
		if(AD_reg_edits[i] == NULL) {
			fprintf(stderr, "out of memory\n");
		}
	}

	for (j = 0; j < 4; j++) {
		AD_reg_edits[j][0] = 0x191 + j*3;
		AD_reg_edits[j][1] = 0x70;
	}
	AD_reg_edits[4][0] = 0x232;
	AD_reg_edits[4][1] = 0x1;
	
	for (i = 0; i < 2; i++) {
		for (j = 0; j < 5; j++) {
			interleave_zero(&(AD_reg_edits[j][0]));
			interleave_zero(&(AD_reg_edits[j][1]));
			NI_waveform[BPW*(n_offset + j_times[i] + 2*j) + ADDAT] = AD_reg_edits[j][0];
			NI_waveform[BPW*(n_offset + j_times[i] + 2*j + 1) + ADDAT] = AD_reg_edits[j][1];
		
			/* Insert clock: full clock required for 16-bit instruction byte. For
				* the 8-bit value byte, only need clocks in the second half of the
				* digital world */
			/* 0x5 = 0101 */
			NI_waveform[BPW*(n_offset + j_times[i] + 2*j) + ADCLK] = 0x55555555;
			NI_waveform[BPW*(n_offset + j_times[i] + 2*j + 1) + ADCLK] = 0x00005555;
		}

		/* reset 'AD_reg_edits' with powered on values */
		for (j = 0; j < 4; j++) {
			AD_reg_edits[j][0] = 0x191 + j*3;
			AD_reg_edits[j][1] = 0;
		}
		AD_reg_edits[4][0] = 0x232;
		AD_reg_edits[4][1] = 0x1; 
	}
	for (i = 0; i < nedits; i++) {
		free(AD_reg_edits[i]);
	}

	for (j = j_start; j < j_stop; j++) {
		NI_waveform[BPW*(n_offset + j) + CSTop] = 0xFFFFFFFF;
		NI_waveform[BPW*(n_offset + j) + CSBot] = 0xFFFFFFFF;
	}
	free(AD_reg_edits);
}

_declspec (dllexport) void set_dds_freq(int dds_chan, double freq, 
                                       int dat_chan, uInt32 NI_waveform[]) {
    /* Function description */
    /* This function is for programming the Analog Devices AD9959 eval boards.
     * It programs a frequency 'freq' [MHz] on 'dds_chan' [zero-indexed] */

	unsigned int int0;
    unsigned int int1;
    unsigned int int2;
    unsigned int int3;
    unsigned int FTW;
    unsigned int sync_chan;
    int i;
	uInt32 *CSblock;

    /* Initialize 'CSblock' */
	CSblock = (uInt32 *)malloc(2 * sizeof(*CSblock));
	memset(&CSblock[0], 0, 2 * sizeof(uInt32));
    /* 'CSblock' consists of 2 bytes which represent the 2 clock cycles
     * that occur between each 32-clock-cycle wide section of actual
     * data. During this CSblock, the CS lines need to go high (during
     * the actual data, CS is low). Additionally, we set 'SYNCBAR' and
     * 'RESETBAR' high to keep the AD9522 in normal operation. */
	CSblock[0] = CSblock[0] | (1 << (31 - ADCS));
	CSblock[0] = CSblock[0] | (1 << (31 - RESETBAR));
	CSblock[0] = CSblock[0] | (1 << (31 - CSTop));
	CSblock[0] = CSblock[0] | (1 << (31 - CSBot));
	CSblock[0] = CSblock[0] | (1 << (31 - SYNCBAR));

	CSblock[1] = CSblock[1] | (1 << (31 - ADCS));
	CSblock[1] = CSblock[1] | (1 << (31 - RESETBAR));
	CSblock[1] = CSblock[1] | (1 << (31 - CSTop));
	CSblock[1] = CSblock[1] | (1 << (31 - CSBot));
	CSblock[1] = CSblock[1] | (1 << (31 - SYNCBAR));

    /* the 'REFIN' line should always be cycling between high and low, 
     * so that the PLL remains locked. So, we set it high for the first
     * part of CSblock. */
	CSblock[0] = CSblock[0] | (1 << (31 - REFIN));


    sync_chan = 8;

    FTW = (unsigned int)(4*freq/SYSCLK*(long double)(1 << 30));
	//printf("FTW = %f\n", (long double)freq/SYSCLK*(1 << 31));
	//printf("FTW = %u\n", (unsigned int)freq/SYSCLK*(1 << 31));
	//printf("test = %u\n", (unsigned int)(1 << 8));
	//printf("test2 = %u\n", (unsigned int)(1 << 32));
	//printf("FTW = %f\n", FTW);
	printf("FTW = %u\n", (unsigned int)FTW);

    /* write to channel select register (CSR) twice */
    int0 = (unsigned int) ((0 << 24)
        //+ ((1 << (dds_chan + 4)) << 16)   /* control bits = 0 = 0b00 */
        + ((15 << 4) << 16)   /* control bits = 0 = 0b00 */
        + (0 << 8)
        //+ ((1 << (dds_chan + 4)) << 0));  /* control bits = 0 = 0b00 */
        + ((15 << 4) << 0));  /* control bits = 0 = 0b00 */

    /* write to FR1 register to set PLL divider ratio and VCO gain control */
	int1 = (unsigned int)((1 << 24)
		+ (1 << 23)
		+ (20 << 18));

    /* read from FR2 register (to fill up 32-bit segment) and then instruction 
     * byte for CFTW register */
    int2 = (unsigned int) (((1 << 7) << 24)
        + (2 << 24)
        + (0 << 8)
        + 4);

    int3 = (unsigned int) (FTW);

	printf("int0 = %u\n", int0);
	printf("int1 = %u\n", int1);
	printf("int2 = %u\n", int2);
	printf("int3 = %u\n", int3);
	
	NI_waveform[BPW*(n_offset + 5) + (dat_chan % NUMCHANNELS)] = (unsigned int)int0;
    NI_waveform[BPW*(n_offset + 6) + (dat_chan % NUMCHANNELS)] = (unsigned int)int1;
    NI_waveform[BPW*(n_offset + 7) + (dat_chan % NUMCHANNELS)] = (unsigned int)int2;
    NI_waveform[BPW*(n_offset + 8) + (dat_chan % NUMCHANNELS)] = (unsigned int)int3;

    // for (i = 1; i < 5000; i++) {
	//     NI_waveform[BPW*(n_offset + i) + (dat_chan % NUMCHANNELS)] = AD_convert(1.55);
    // }

    printf("dat_chan: %d \n", dat_chan);

    for (i = 5; i < 9; i++) {
        NI_waveform[BPW*(n_offset + i + 1) - 2] = CSblock[0];
        NI_waveform[BPW*(n_offset + i + 1) - 1] = CSblock[1];

        printf("csblock modification: %d \n", i);
    }
    // NI_waveform[BPW*(n_offset + 3) + sync_chan] = (unsigned int) 0;
    NI_waveform[BPW*(n_offset + 5) + sync_chan] = (unsigned int) 0;
    NI_waveform[BPW*(n_offset + 6) + sync_chan] = (unsigned int) 0;
    NI_waveform[BPW*(n_offset + 7) + sync_chan] = (unsigned int) 0;
    NI_waveform[BPW*(n_offset + 8) + sync_chan] = (unsigned int) 0;

    // NI_waveform[BPW*(n_offset + 10) + sync_chan] = (unsigned int) 0;
}

_declspec (dllexport) void set_dds_freq_sweep(int dds_chan, 
                                    double start_freq, double stop_freq,
                                    double sweep_duration,
                                    int dat_chan, uInt32 NI_waveform[]) {
    /* Function description */
    /* This function is for programming the Analog Devices AD9959 eval boards.
     * It programs a slow frequency sweep. */

    unsigned int int1;
    unsigned int int2;
    unsigned int int3;
    unsigned int int4;
    unsigned int int5;
    unsigned int int6;
    unsigned int int7;
    unsigned int int8;
    unsigned int CFR;
    unsigned int RDW;
    unsigned int RSRR;
    double SFTW;
    double EFTW;
    double delta_freq;

	delta_freq = fabs(stop_freq - start_freq);

	if (start_freq <= stop_freq) {
	    SFTW = start_freq*(1 << 32)/SYSCLK;
	    EFTW = stop_freq*(1 << 32)/SYSCLK;
    } else {
	    SFTW = (SYSCLK - start_freq)*(1 << 32)/SYSCLK;
	    EFTW = (SYSCLK - stop_freq)*(1 << 32)/SYSCLK;
    }

	CFR = (unsigned int)(2 << 22
            + (0 << 15) 
            + (1 << 14)
            + (3 << 8));

	RDW = 1;
	RSRR = (unsigned int)((1 << 8) 
        + sweep_duration/1000*SYSCLK/(1 << 32)/delta_freq*SYSCLK/4*1000000); 

    /* write to channel select register (CSR) twice */
    int1 = (unsigned int) ((0 << 24)
            + ((1 << (dds_chan + 4)) << 16)
            + (0 << 8)
            + ((1 << (dds_chan + 4)) << 0)); 

    /* write to LSRR register and instruction byte for CFTW register */
    int2 = (unsigned int) ((7 << 24)          /* 0x07 is LSRR address*/
            + (RSRR << 8)
            + 4);                           /* 0x04 is CFTW address */

    /* write SFTW to CFTW register */
    int3 = (unsigned int) (SFTW);

    /* write CFR to CFR register (enabling linear sweep) */
    int4 = (unsigned int) ((3 << 24)          /* 0x03 is CFR address */
            + CFR);

    /* write to LSRR register (again, just to fill up the 32-bit segment)
     * and then instruction byte for RDW register */
    int5 = (unsigned int) ((7 << 24)          /* 0x07 is LSRR address */
            + RSRR << 8
            + 8);                           /* 0x08 is RDW address */

    /* write RDW to RDW register */
    int6 = (unsigned int) (RDW);

    /* write to LSRR register (again*2, just to fill up the 32-bit segment)
     * and then instruction byte for CW1 register */
    int7 = (unsigned int) ((7 << 24)          /* 0x07 is LSRR address */
            + (RSRR << 8)
            + 0xA);                         /* 0x0A is CW1 address */

    /* write EFTW to CW1 register */
    int8 = (unsigned int) (RDW);

    NI_waveform[BPW*(tot_words - 8) + dat_chan] = int1;
    NI_waveform[BPW*(tot_words - 7) + dat_chan] = int2;
    NI_waveform[BPW*(tot_words - 6) + dat_chan] = int3;
    NI_waveform[BPW*(tot_words - 5) + dat_chan] = int4;
    NI_waveform[BPW*(tot_words - 4) + dat_chan] = int5;
    NI_waveform[BPW*(tot_words - 3) + dat_chan] = int6;
    NI_waveform[BPW*(tot_words - 2) + dat_chan] = int7;
    NI_waveform[BPW*(tot_words - 1) + dat_chan] = int8;
}

_declspec (dllexport) void set_dds_freq_sweep_fast(int dds_chan, 
                                    double start_freq, double stop_freq,
                                    double sweep_duration,
                                    int dat_chan, uInt32 NI_waveform[]) {
    /* Function description */
    /* This function is for programming the Analog Devices AD9959 eval boards.
     * It programs a fast frequency sweep, e.g. for the compressed MOT. */
    unsigned int int1;
    unsigned int int2;
    unsigned int int3;
    unsigned int int4;
    unsigned int int5;
    unsigned int int6;
    unsigned int int7;
    unsigned int int8;
    unsigned int CFR;
    unsigned int RDW;
    unsigned int RSRR;
    double SFTW;
    double EFTW;
    double delta_freq;

	delta_freq = fabs(stop_freq - start_freq);

	if (start_freq <= stop_freq) {
	    SFTW = start_freq*(1 << 32)/SYSCLK;
	    EFTW = stop_freq*(1 << 32)/SYSCLK;
    } else {
	    SFTW = (SYSCLK - start_freq)*(1 << 32)/SYSCLK;
	    EFTW = (SYSCLK - stop_freq)*(1 << 32)/SYSCLK;
    }

	CFR = (unsigned int)(2 << 22
            + (0 << 15) 
            + (1 << 14)
            + (3 << 8));

	RDW = (unsigned int)delta_freq/(sweep_duration/1000)*((1 << 32)/(SYSCLK*1000000))*(4/SYSCLK);
	RSRR = (unsigned int)((1 << 8) + 1);

    /* write to channel select register (CSR) twice */
    int1 = (unsigned int) (0 << 24
            + (1 << (dds_chan + 4)) << 16   
            + 0 << 8
            + (1 << (dds_chan + 4)) << 0); 

    /* write to LSRR register and instruction byte for CFTW register */
    int2 = (unsigned int) (7 << 24          /* 0x07 is LSRR address*/
            + RSRR << 8
            + 4);                           /* 0x04 is CFTW address */

    /* write SFTW to CFTW register */
    int3 = (unsigned int) (SFTW);

    /* write CFR to CFR register (enabling linear sweep) */
    int4 = (unsigned int) ((3 << 24)          /* 0x03 is CFR address */
            + CFR);

    /* write to LSRR register (again, just to fill up the 32-bit segment)
     * and then instruction byte for RDW register */
    int5 = (unsigned int) ((7 << 24)          /* 0x07 is LSRR address */
            + (RSRR << 8)
            + 8);                           /* 0x08 is RDW address */

    /* write RDW to RDW register */
    int6 = (unsigned int) (RDW);

    /* write to LSRR register (again*2, just to fill up the 32-bit segment)
     * and then instruction byte for CW1 register */
    int7 = (unsigned int) ((7 << 24)          /* 0x07 is LSRR address */
            + (RSRR << 8)
            + 0xA);                         /* 0x0A is CW1 address */

    /* write EFTW to CW1 register */
    int8 = (unsigned int) (RDW);

    NI_waveform[BPW*(tot_words - 8) + dat_chan] = int1;
    NI_waveform[BPW*(tot_words - 7) + dat_chan] = int2;
    NI_waveform[BPW*(tot_words - 6) + dat_chan] = int3;
    NI_waveform[BPW*(tot_words - 5) + dat_chan] = int4;
    NI_waveform[BPW*(tot_words - 4) + dat_chan] = int5;
    NI_waveform[BPW*(tot_words - 3) + dat_chan] = int6;
    NI_waveform[BPW*(tot_words - 2) + dat_chan] = int7;
    NI_waveform[BPW*(tot_words - 1) + dat_chan] = int8;
}

_declspec (dllexport) void set_freq(double freq, 
                                       int dat_chan, uInt32 NI_waveform[]) {
    /* Function description */

    /* consider using a trick from Hacker's Delight */
    /* perhaps precalculate 'BPW/SMP_CLK' */
    int j;                      /* loop index */
    unsigned int RLatch;
    unsigned int NLatch;
    unsigned int FLatch;
    unsigned int ILatch;
    double RCounter;
    double NCounter;

    NCounter = freq/PFDFREQ;
    RCounter = REFCLKFREQ/PFDFREQ;

    RLatch = (unsigned int) (((unsigned int)RCounter << 2)
        + 0                     /* control bits = 0 = 0b00 */
        + (0 << 16)               /* ABPW = 2.9 ns */
        + (0 << 18)               /* test mode bits 0 */
        + (0 << 20));              /* lock detect precision = 3 cycles */

    NLatch = (unsigned int) (((unsigned int)NCounter << 8)
        + 1                     /* control bits = 1 =0b 01*/
        + (0 << 21));              /* charge pump gain */

    FLatch = (unsigned int) (2                /* control bits = 2 = 0b10 */
        + (0 << 2)                /* counter operation normal */
        + (0 << 3)                /* powerdown 1 = normal operation */
        + (1 << 4)                /* MUXOUT displays Digital Lock Detect */
        + (1 << 7)                /* phase detector polarity = positive */
        + (0 << 8)                /* Charge pump output normal (not tri-state) */
        + (0 << 9)                /* Fastlock disabled */
        + (0 << 11)               /* timeout = 3 PFD cycles */
        + (3 << 15)               /* CP1 current = 2.5 mA */
        + (3 << 18)               /* CP2 current = 2.5 mA */
        + (0 << 21));              /* powerdown 2 = normal operation */

    ILatch = (unsigned int) (3                /* control bits = 3 = 0b11 */
        + (0 << 2)                /* counter operation normal */
        + (0 << 3)                /* powerdown 1 = normal operation */
        + (1 << 4)                /* MUXOUT displays Digital Lock Detect */
        + (1 << 7)                /* phase detector polarity = positive */
        + (0 << 8)                /* Charge pump output normal (not tri-state) */
        + (0 << 9)                /* Fastlock disabled */
        + (0 << 11)               /* timeout = 3 PFD cycles */
        + (3 << 15)               /* CP1 current = 2.5 mA */
        + (3 << 18)               /* CP2 current = 2.5 mA */
        + (0 << 21));              /* powerdown 2 = normal operation */

	/* temporary hack */
    NI_waveform[BPW*(n_offset + 1) + (dat_chan % NUMCHANNELS)] = (unsigned int)ILatch;
    NI_waveform[BPW*(n_offset + 2) + (dat_chan % NUMCHANNELS)] = (unsigned int)FLatch;
    NI_waveform[BPW*(n_offset + 3) + (dat_chan % NUMCHANNELS)] = (unsigned int)RLatch;
    NI_waveform[BPW*(n_offset + 4) + (dat_chan % NUMCHANNELS)] = (unsigned int)NLatch;
	NI_waveform[BPW*(n_offset + 5) + (dat_chan % NUMCHANNELS)] = (unsigned int)ILatch;
    NI_waveform[BPW*(n_offset + 6) + (dat_chan % NUMCHANNELS)] = (unsigned int)FLatch;
    NI_waveform[BPW*(n_offset + 7) + (dat_chan % NUMCHANNELS)] = (unsigned int)RLatch;
    NI_waveform[BPW*(n_offset + 8) + (dat_chan % NUMCHANNELS)] = (unsigned int)NLatch;
	/* end temporary hack */
	
    NI_waveform[BPW*(tot_words - 4) + (dat_chan % NUMCHANNELS)] = (unsigned int)ILatch;
    NI_waveform[BPW*(tot_words - 3) + (dat_chan % NUMCHANNELS)] = (unsigned int)FLatch;
    NI_waveform[BPW*(tot_words - 2) + (dat_chan % NUMCHANNELS)] = (unsigned int)RLatch;
    NI_waveform[BPW*(tot_words - 1) + (dat_chan % NUMCHANNELS)] = (unsigned int)NLatch;

    /*j_last = tot_words - n_offset;*/
	//printf("dat_chan = %d, \t freq = %f\n", dat_chan, freq);
	//printf("dev type = %d\n", devtype[dat_chan]);
}


_declspec (dllexport) void set_freq_time(double freq, double set_time,
                                       int dat_chan, uInt32 NI_waveform[]) {
    /* Function description */

    /* consider using a trick from Hacker's Delight */
    /* perhaps precalculate 'BPW/SMP_CLK' */
    int j;                      /* loop index */
	int set_time_j = digitize_time(set_time);
    unsigned int RLatch;
    unsigned int NLatch;
    unsigned int FLatch;
    unsigned int ILatch;
    double RCounter;
    double NCounter;

    NCounter = freq/PFDFREQ;
    RCounter = REFCLKFREQ/PFDFREQ;

    RLatch = (unsigned int) (((unsigned int)RCounter << 2)
        + 0                     /* control bits = 0 = 0b00 */
        + (0 << 16)               /* ABPW = 2.9 ns */
        + (0 << 18)               /* test mode bits 0 */
        + (0 << 20));              /* lock detect precision = 3 cycles */

    NLatch = (unsigned int) (((unsigned int)NCounter << 8)
        + 1                     /* control bits = 1 =0b 01*/
        + (0 << 21));              /* charge pump gain */

    FLatch = (unsigned int) (2                /* control bits = 2 = 0b10 */
        + (0 << 2)                /* counter operation normal */
        + (0 << 3)                /* powerdown 1 = normal operation */
        + (1 << 4)                /* MUXOUT displays Digital Lock Detect */
        + (1 << 7)                /* phase detector polarity = positive */
        + (0 << 8)                /* Charge pump output normal (not tri-state) */
        + (0 << 9)                /* Fastlock disabled */
        + (0 << 11)               /* timeout = 3 PFD cycles */
        + (3 << 15)               /* CP1 current = 2.5 mA */
        + (3 << 18)               /* CP2 current = 2.5 mA */
        + (0 << 21));              /* powerdown 2 = normal operation */

    ILatch = (unsigned int) (3                /* control bits = 3 = 0b11 */
        + (0 << 2)                /* counter operation normal */
        + (0 << 3)                /* powerdown 1 = normal operation */
        + (1 << 4)                /* MUXOUT displays Digital Lock Detect */
        + (1 << 7)                /* phase detector polarity = positive */
        + (0 << 8)                /* Charge pump output normal (not tri-state) */
        + (0 << 9)                /* Fastlock disabled */
        + (0 << 11)               /* timeout = 3 PFD cycles */
        + (3 << 15)               /* CP1 current = 2.5 mA */
        + (3 << 18)               /* CP2 current = 2.5 mA */
        + (0 << 21));              /* powerdown 2 = normal operation */

    printf("Chan/Bit \t");

	/* temporary hack */
    NI_waveform[BPW*(n_offset + set_time_j + 1) + (dat_chan % NUMCHANNELS)] = (unsigned int)ILatch;
    NI_waveform[BPW*(n_offset + set_time_j + 2) + (dat_chan % NUMCHANNELS)] = (unsigned int)FLatch;
    NI_waveform[BPW*(n_offset + set_time_j + 3) + (dat_chan % NUMCHANNELS)] = (unsigned int)RLatch;
    NI_waveform[BPW*(n_offset + set_time_j + 4) + (dat_chan % NUMCHANNELS)] = (unsigned int)NLatch;
	NI_waveform[BPW*(n_offset + set_time_j + 5) + (dat_chan % NUMCHANNELS)] = (unsigned int)ILatch;
    NI_waveform[BPW*(n_offset + set_time_j + 6) + (dat_chan % NUMCHANNELS)] = (unsigned int)FLatch;
    NI_waveform[BPW*(n_offset + set_time_j + 7) + (dat_chan % NUMCHANNELS)] = (unsigned int)RLatch;
    NI_waveform[BPW*(n_offset + set_time_j + 8) + (dat_chan % NUMCHANNELS)] = (unsigned int)NLatch;
	/* end temporary hack */
	
    /*j_last = tot_words - n_offset;*/
	//printf("dat_chan = %d, \t freq = %f\n", dat_chan, freq);
	//printf("dev type = %d\n", devtype[dat_chan]);
}

_declspec (dllexport) void set_pllRN(unsigned int RCounter, unsigned int NCounter,
                                       int dat_chan, uInt32 NI_waveform[]) {
    /* Function description */

    /* consider using a trick from Hacker's Delight */
    /* perhaps precalculate 'BPW/SMP_CLK' */
    int j;                      /* loop index */
    unsigned int RLatch;
    unsigned int NLatch;
    unsigned int FLatch;
    unsigned int ILatch;
    
    RLatch = (unsigned int) (((unsigned int)RCounter << 2)
        + 0                     /* control bits = 0 = 0b00 */
        + (0 << 16)               /* ABPW = 2.9 ns */
        + (0 << 18)               /* test mode bits 0 */
        + (0 << 20));              /* lock detect precision = 3 cycles */

    NLatch = (unsigned int) (((unsigned int)NCounter << 8)
        + 1                     /* control bits = 1 =0b 01*/
        + (0 << 21));              /* charge pump gain */

    FLatch = (unsigned int) (2                /* control bits = 2 = 0b10 */
        + (0 << 2)                /* counter operation normal */
        + (0 << 3)                /* powerdown 1 = normal operation */
        + (1 << 4)                /* MUXOUT displays Digital Lock Detect */
        + (1 << 7)                /* phase detector polarity = positive */
        + (0 << 8)                /* Charge pump output normal (not tri-state) */
        + (0 << 9)                /* Fastlock disabled */
        + (0 << 11)               /* timeout = 3 PFD cycles */
        + (3 << 15)               /* CP1 current = 2.5 mA */
        + (3 << 18)               /* CP2 current = 2.5 mA */
        + (0 << 21));              /* powerdown 2 = normal operation */

    ILatch = (unsigned int) (3                /* control bits = 3 = 0b11 */
        + (0 << 2)                /* counter operation normal */
        + (0 << 3)                /* powerdown 1 = normal operation */
        + (1 << 4)                /* MUXOUT displays Digital Lock Detect */
        + (1 << 7)                /* phase detector polarity = positive */
        + (0 << 8)                /* Charge pump output normal (not tri-state) */
        + (0 << 9)                /* Fastlock disabled */
        + (0 << 11)               /* timeout = 3 PFD cycles */
        + (3 << 15)               /* CP1 current = 2.5 mA */
        + (3 << 18)               /* CP2 current = 2.5 mA */
        + (0 << 21));              /* powerdown 2 = normal operation */

	/* temporary hack */
    NI_waveform[BPW*(n_offset + 1) + (dat_chan % NUMCHANNELS)] = (unsigned int)ILatch;
    NI_waveform[BPW*(n_offset + 2) + (dat_chan % NUMCHANNELS)] = (unsigned int)FLatch;
    NI_waveform[BPW*(n_offset + 3) + (dat_chan % NUMCHANNELS)] = (unsigned int)RLatch;
    NI_waveform[BPW*(n_offset + 4) + (dat_chan % NUMCHANNELS)] = (unsigned int)NLatch;
	/* end temporary hack */

    NI_waveform[BPW*(tot_words - 4) + (dat_chan % NUMCHANNELS)] = (unsigned int)ILatch;
    NI_waveform[BPW*(tot_words - 3) + (dat_chan % NUMCHANNELS)] = (unsigned int)FLatch;
    NI_waveform[BPW*(tot_words - 2) + (dat_chan % NUMCHANNELS)] = (unsigned int)RLatch;
    NI_waveform[BPW*(tot_words - 1) + (dat_chan % NUMCHANNELS)] = (unsigned int)NLatch;

    /*j_last = tot_words - n_offset;*/
}


_declspec (dllexport) void set_voltage(double voltage, 
                                       int dat_chan, uInt32 NI_waveform[]) {
    /* Function description */
    /* just repeatedly send the digital word associated with some voltage.
     * this function is to be used with the interactive mode. In principle,
     * 'insert_step' could be used with the end time set to exactly match the
     * total length of the experiment (this must be done because, recall, if 
     * a value isn't explicitly set, the 0x00000000 will be sent to the DA).
     * This function though just keeps sending it for all the samples sent, so
     * one doesn't need to worry about float -> int conversions and round off
     * errors that could result in the last word being set being 0x00000000 */ 

    /* consider using a trick from Hacker's Delight */
    /* perhaps precalculate 'BPW/SMP_CLK' */
    int j;                      /* loop index */
	unsigned int (*convert_func)(double);

    convert_func = get_convert_func(dat_chan);

    /*j_last = tot_words - n_offset;*/

    // if (devtype[dat_chan] == 0) {
    //     convert_func = &AD_convert;
    // } else if (devtype[dat_chan] == 1) {
    //     convert_func = &AD_convert_bipolar;
    // } else {
    //     convert_func = &AD_convert;
    // }
	//printf("dat_chan: %d\r\n", dat_chan);
	//printf("dat_chan % NUMCHANNELS: %d\r\n", dat_chan % NUMCHANNELS);
	//printf("devtype: %d\r\n", devtype[dat_chan]);
	//printf("set_voltage sends: %d\r\n", (unsigned int) convert_func(voltage));
	 
	if (devtype[dat_chan] == 2) {
		set_freq(voltage, dat_chan, NI_waveform);
	} else {
		for (j = 0; j < tot_words; j++) {
			NI_waveform[BPW*(n_offset + j) + (dat_chan % NUMCHANNELS)] = convert_func(voltage);
	    }
	}
}

_declspec (dllexport) void insert_step(double step_volts, 
                                       double t_start, double t_stop, 
                                       int dat_chan, uInt32 NI_waveform[]) {
    /* Function description */
    /* consider using a trick from Hacker's Delight */
    /* perhaps precalculate 'BPW/SMP_CLK' */
    int j;                      /* loop index */
    int j_start = digitize_time(t_start);
    int j_stop = digitize_time(t_stop);
	unsigned int (*convert_func)(double);

    convert_func = get_convert_func(dat_chan);

    if (j_stop > j_last) {
        j_last = j_stop;
    }

    for (j = j_start; j < j_stop; j++) {
		NI_waveform[BPW*(n_offset + j) + (dat_chan % NUMCHANNELS)] = convert_func(step_volts);
    }
}

_declspec (dllexport) void insert_exp(double base_volts, double offset_volts,
                                      double t_start, double t_stop, double time_const,
                                      int dat_chan, uInt32 NI_waveform[]) {
    /* Function description */
    /* consider using a trick from Hacker's Delight */
    /* perhaps precalculate 'BPW/SMP_CLK' */
    int j;                      /* loop index */
    int j_start = digitize_time(t_start);
    int j_stop = digitize_time(t_stop);
    int time_const_j = digitize_time(time_const);
	int jduration = j_stop - j_start;
	double ratio;
	unsigned int (*convert_func)(double);

    convert_func = get_convert_func(dat_chan);

    if (j_stop > j_last) {
        j_last = j_stop;
    }

    for (j = j_start; j < j_stop; j++) {
		ratio = (double)(j - j_start)/jduration; 
		NI_waveform[BPW*(n_offset + j) + (dat_chan % NUMCHANNELS)] = 
			convert_func(offset_volts - base_volts * exp((t_stop - t_start)/time_const * ratio));
	}
}

_declspec (dllexport) void insert_exp_and_ramp(double base_volts, double offset_volts,
                                      double t_start, double t_stop, double tramp, double time_const,
                                      double start_volts, double stop_volts,
                                      int dat_chan, uInt32 NI_waveform[]) {
    /* Function description */
    /* consider using a trick from Hacker's Delight */
    /* perhaps precalculate 'BPW/SMP_CLK' */
    int j;                      /* loop index */
    int j_start = digitize_time(t_start);
    int j_stop = digitize_time(t_stop);
    double y = DA_convert(NI_waveform[BPW*(n_offset + j_start) + (dat_chan % NUMCHANNELS)]);
    int time_const_j  = digitize_time(time_const);
    int tramp_j = digitize_time(tramp);
    double slope = (stop_volts - start_volts)/tramp_j;
    double ratio;
    int jduration = j_stop - j_start;
	unsigned int (*convert_func)(double);

    convert_func = get_convert_func(dat_chan);

    if (j_stop > j_last) {
        j_last = j_stop;
    }

    for (j = j_start; j < j_stop; j++) {
        ratio = (double)(j - j_start)/jduration; 

        if (j < (tramp_j + j_start)) {
		    NI_waveform[BPW*(n_offset + j) + (dat_chan % NUMCHANNELS)] = 
                convert_func(y + start_volts + (j - j_start)*slope
                            + offset_volts - base_volts
                            * exp((t_stop - t_start)/time_const * ratio));
        } else {
		    NI_waveform[BPW*(n_offset + j) + (dat_chan % NUMCHANNELS)] = 
                convert_func(y + stop_volts
                            + offset_volts - base_volts
                            * exp((t_stop - t_start)/time_const * ratio)); 
        }
    }
}

_declspec (dllexport) void insert_ramp(double start_volts, double stop_volts,
                                        double t_start, double t_stop, 
                                        int dat_chan, uInt32 NI_waveform[]) {
    /* Function description */
    /* consider using a trick from Hacker's Delight */
    /* perhaps precalculate 'BPW/SMP_CLK' */
    int j;                      /* loop index */
    int j_start = digitize_time(t_start);
    int j_stop = digitize_time(t_stop);
    double slope = (stop_volts - start_volts)/((t_stop - t_start)*SMP_CLK/BPW/1000);
	//FILE *pFile;
	unsigned int (*convert_func)(double);

    convert_func = get_convert_func(dat_chan);
	
    if (j_stop > j_last) {
        j_last = j_stop;
    }

    for (j = j_start; j < j_stop; j++) {
		NI_waveform[BPW*(n_offset + j) + (dat_chan % NUMCHANNELS)] = 
             convert_func(start_volts + slope*(j - j_start));

        /* diagnostic */
        /*printf("cs top %i \n", (NUMCHANNELS*(n_offset + 2*j) + cs_chan)/NUMCHANNELS);
        if (j == j_stop - 1) {
            printf("%i\n", (int)(slope*((j - j_start) << 18)/5));
        } else {
            printf("%i\t", (int)(slope*((j - j_start) << 18)/5));
        }*/
    }
	/* Make sure CS goes high for the last cycle */
	//NI_waveform[NUMCHANNELS*(n_offset + 2*j) + cs_chan] = 0xFFFFFFFF;
	
	/* diagnostic */
	//pFile = fopen ( "ramp_out.txt" , "ab" );
	//fprintf (pFile, "Linear Ramp: %f, %f, %f, %f, %f\n", t_start, t_stop, y_start, y_stop, increment);
 //   for (j = j_start; j < j_stop; j++) {
 //             fprintf (pFile, "%i, %u, %u\n", j, NI_waveform[BPW*(n_offset + j) + dat_chan], j - j_start);
	//}
 //   fclose (pFile);
	//printf("j_start =  %u\n", j_start);
 //   printf("j_stop = %u\n", j_stop);
 //   printf("n_offset = %u\n", n_offset);
}

_declspec (dllexport) void insert_tunnel_sine(double offset_depth, double rel_amp, double freq_Hz, double phase,
                            double t_start, double t_stop,
                            double calib_volt, double slope,
                            int dat_chan, uInt32 NI_waveform[]) {
    /* Sinusoidal modulation of the tunnelling about given lattice depth with relative 
     * amplitude rel_amp and initial phase zero on a selected channel.
     *
     * Arguments:
     * - offset_depth       offset for tunnelling, in recoils
     * - rel_amp            relative amplitude of sinusoidal modulation of tunnelling
     * - freq_Hz            frequency in Hz
     * - tstart             Leading edge time, in milliseconds from the analog output start trigger
     * - tstop              Trailing edge time, in milliseconds from the analog output start trigger
     * - calib_volt         calibrated voltage for offset_depth (e.g. lattice2_low_volt)
     * - slope              slope (in V per decade) of photdiodes
     * - dat_chan           (0-7) Specifies the channel to which the ramp waveform is added
     * - phase              additional phase in units of Pi */

    int j;                      /* loop index */
    int j_start = digitize_time(t_start);
    int j_stop = digitize_time(t_stop);
    double period = 1 / freq_Hz;
    double w = tunnel_from_depth(offset_depth);
    double u;
    double x;
    double y;
	unsigned int (*convert_func)(double);

    convert_func = get_convert_func(dat_chan);
    period = period * SMP_CLK/BPW;

    if (j_stop > j_last) {
        j_last = j_stop;
    }

    if (j_stop > j_start) {
        for (j = j_start; j < j_stop; j++) {
            u = w * (1 + rel_amp * sin(phase * PI + 2 * PI / period * (j - j_start)));
            x = depth_from_tunnel(u);
            y = calib_volt + slope * (log10(x) - log10(offset_depth));
            NI_waveform[BPW*(n_offset + j) + (dat_chan % NUMCHANNELS)] = convert_func(y);
        }
    }
}

_declspec (dllexport) void insert_exp_sane(double v0, double vf, double tau_ms,
                            double t_start, double t_stop,
                            int dat_chan, uInt32 NI_waveform[]) {
    /* exp ramp with reasonable parametrization */

    int j;                      /* loop index */
    int j_start = digitize_time(t_start);
    int j_stop = digitize_time(t_stop);
	double tau = tau_ms/(t_stop - t_start);
	int jduration = j_stop - j_start;
	double ratio;
	unsigned int (*convert_func)(double);

    convert_func = get_convert_func(dat_chan);
    
    if (j_stop > j_last) {
        j_last = j_stop;
    }

    if (j_stop > j_start) {
        for (j = j_start; j < j_stop; j++) {
			ratio = (double)(j - j_start)/jduration;
            NI_waveform[BPW*(n_offset + j) + (dat_chan % NUMCHANNELS)] = convert_func(v0 + (vf-v0)*(exp(ratio/tau) - 1)/(exp(1/tau) - 1));
        }
    }
}

_declspec (dllexport) void insert_tunnel_sine_ramp(double offset_depth, double rel_amp, 
                            double freq_start, double freq_stop,
                            double phase_start, double t_start, double t_stop,
                            double calib_volt, double slope,
                            int dat_chan, uInt32 NI_waveform[]) {
    /* Sinusoidal modulation of the tunnelling about given lattice depth with 
     * linearly increasing frequency and relative amplitude rel_amp. Specify 
     * initial phase in units of Pi.
     *
     * Arguments:
     * - offset_depth:      offset for tunnelling, in recoils
     * - rel_amp:           relative amplitude of sinusoidal modulation of tunnelling
     * - freq_start:        initial frequency in Hz
     * - freq_stop:         final frequency in Hz
     * - t_start:           Leading edge time, in milliseconds from the analog output start trigger
     * - t_stop:            Trailing edge time, in milliseconds from the analog output start trigger
     * - calib_volt         calibrated voltage for offset_depth (e.g. lattice2_low_volt)
     * - slope              slope (in V per decade) of photdiodes
     * - dat_chan           (0-7) Specifies the channel to which the ramp waveform is added
     * - phase              additional phase in units of Pi */

    int j;                      /* loop index */
    int j_start = digitize_time(t_start);
    int j_stop = digitize_time(t_stop);
    double w = tunnel_from_depth(offset_depth);
    double u;
    double x;
    double y;
    double PhiT;
	double omega_start = 2*PI*freq_start*BPW/SMP_CLK;
	double omega_stop = 2*PI*freq_stop*BPW/SMP_CLK;
    double omega_slope = (omega_stop - omega_start)/digitize_time((t_stop - t_start));
	unsigned int (*convert_func)(double);

    convert_func = get_convert_func(dat_chan);

    if (j_stop > j_last) {
        j_last = j_stop;
    }

    if (j_stop > j_start) {
        for (j = j_start; j < j_stop; j++) {
            PhiT = omega_start*(j - j_start) + omega_slope*pow((j - j_start), 2);
            u = w * (1 + rel_amp * sin(PhiT));
            x = depth_from_tunnel(u);
            y = calib_volt + slope * (log10(x) - log10(offset_depth));
		    NI_waveform[BPW*(n_offset + j) + (dat_chan % NUMCHANNELS)] = convert_func(y);
        }
    }
}

_declspec (dllexport) void insert_axial_exp_ramp(double start_2Ddepth, double stop_2Ddepth, 
                            double t_start, double t_stop,
                            double axial_calib_depth, double axial_calib_volt,
                            int dat_chan, uInt32 NI_waveform[]) {
    /* Ramp axial lattice such that axial and 2d tunneling are the same. Assumes 
     * linear ramp in 2D lattice voltage!!! 
     * Calulation saved in Z:\Experiment Software Backup\ExpControl\mathematica
     *
     * Arguments:
     * start_2Ddepth:       initial amplitude of 2d lattice, in 2d recoils
     * tstart:              Leading edge time, in milliseconds from the analog output start trigger
     * tstop:               Trailing edge time, in milliseconds from the analog output start trigger
     * axial_calib_depth:   calibration depth of axial lattice, in axial recoils
     * axial_calib_volt:    calibration voltage axial lattice 
     * dat_chan:            (0-7) Specifies the channel to which the ramp waveform is added
     * time_const:          (1/time constant) for 2d lattice depth exponential ramp (linear in voltage) */

    int j;                      /* loop index */
    int j_start = digitize_time(t_start);
    int j_stop = digitize_time(t_stop);
    double time_const = log10(stop_2Ddepth / start_2Ddepth) / (j_start - j_stop);
	unsigned int (*convert_func)(double);

    convert_func = get_convert_func(dat_chan);
    
    if (j_stop > j_last) {
        j_last = j_stop;
    }

    for (j = j_start; j < j_stop; j++) {
        NI_waveform[BPW*(n_offset + j) + (dat_chan % NUMCHANNELS)] = convert_func(axial_calib_volt 
                + 0.5 * log10(axialdepth_from_2d_depth(start_2Ddepth * pow(10, (time_const * (j - j_start)))) / axial_calib_depth));
    }
}

_declspec (dllexport) void insert_sine(double offset, double amp, double freq,
            double t_start, double t_stop,
            int dat_chan, uInt32 *NI_waveform) {
    /* Adds a sine waveform to a selected channel starting with phase zero.
     * 
     * Arguments:
     * offset:              offset for sine waveform, in volts
     * amp:                 amplitude for sine waveform, in volts
     * freq_Hz:             frequency in Hz
     * t_start:             Leading edge time, in milliseconds from the analog output start trigger
     * tstop:               Trailing edge time, in milliseconds from the analog output start trigger
     * channel:             (0-7) Specifies the channel to which the ramp waveform is added */

    int j;                      /* loop index */
    int j_start = digitize_time(t_start);
    int j_stop = digitize_time(t_stop);
	unsigned int (*convert_func)(double);

    convert_func = get_convert_func(dat_chan);
	freq = 2*PI*freq*BPW/SMP_CLK;

    if (j_stop > j_last) {
        j_last = j_stop;
    }

    for (j = j_start; j < j_stop; j++) {
		NI_waveform[BPW*(n_offset + j ) + (dat_chan % NUMCHANNELS)] = convert_func(amp*sin(freq*(j - j_start)) + offset);
    }
}

_declspec (dllexport) void insert_log_sine3(double lattice_depth, double rel_amp_1, double rel_amp_2, double rel_amp_3, 
            double freq_Hz_1, double freq_Hz_2, double freq_Hz_3,
            double phase_pi, double t_start, double t_stop,
			double voltage_offset, double calib_volt, double calib_depth,
            int dat_chan, uInt32 *NI_waveform) {
	/* Adds sinusoidal modulation with 3 frequency components and phase offset between two components.
	 *
	 * Arguments:
     * lattice_depth:       lattice depth in recoils
     * rel_amp:             relative amplitude in % lattice depth of sinusoidal modulation of tunneling
     * freq_Hz:             frequency in Hz
	 * phase_pi				phase offset between two frequency components
     * t_start:             Leading edge time, in milliseconds from the analog output start trigger
     * tstop:               Trailing edge time, in milliseconds from the analog output start trigger
	 * voltage_offset:		voltage difference between PD value on panel and scope
	 * calib_volt:			voltage from Kapitza-Dirac (in ExpConstants.txt)
	 * calib_depth:			lattice depth corresponding to calib_volt from Kapitza-Dirac (in ExpConstants.txt)
     * dat_chan:             (0-7) Specifies the channel to which the ramp waveform is added */

	int j;                      /* loop index */
    int j_start = digitize_time(t_start);
    int j_stop = digitize_time(t_stop);
	double x;
	double y;
	unsigned int (*convert_func)(double);

    convert_func = get_convert_func(dat_chan);
	freq_Hz_1 = freq_Hz_1 * BPW/SMP_CLK;
	freq_Hz_2 = freq_Hz_2 * BPW/SMP_CLK;
	freq_Hz_3 = freq_Hz_3 * BPW/SMP_CLK;

	if (j_stop > j_last) {
        j_last = j_stop;
    }

    for (j = j_start; j < j_stop; j++) {
		x = lattice_depth * (1 + rel_amp_1 * sin(2 * PI * freq_Hz_1 * (j - j_start)) + rel_amp_2 * sin(2 * PI * freq_Hz_2 * (j - j_start)) + rel_amp_3 * sin(phase_pi * PI + 2 * PI * freq_Hz_3 * (j - j_start)) );
		y = voltage_offset + calib_volt + 0.5 * log10(x / calib_depth);
		NI_waveform[BPW*(n_offset + j ) + (dat_chan % NUMCHANNELS)] = convert_func(y);
    }
}

_declspec (dllexport) void insert_log_sine3_phase(double lattice_depth, double rel_amp_1, double rel_amp_2, double rel_amp_3,
	double freq_Hz_1, double freq_Hz_2, double freq_Hz_3,
	double phase_pi, double t_start, double t_stop, double t_ref,
	double voltage_offset, double calib_volt, double calib_depth,
	int dat_chan, uInt32 *NI_waveform) {
	/* Adds sinusoidal modulation with 3 frequency components and phase offset between two components. Allows for time shift t_ref for all three components.
	*
	* Arguments:
	* lattice_depth:        lattice depth in recoils
	* rel_amp:              relative amplitude in % lattice depth of sinusoidal modulation of tunneling
	* freq_Hz:              frequency in Hz
	* phase_pi				phase offset between two frequency components
	* t_start:              Leading edge time, in milliseconds from the analog output start trigger
	* t_stop:               Trailing edge time, in milliseconds from the analog output start trigger
	* t_ref:               To time-shift the three components by the same amount
	* voltage_offset:		voltage difference between PD value on panel and scope
	* calib_volt:			voltage from Kapitza-Dirac (in ExpConstants.txt)
	* calib_depth:			lattice depth corresponding to calib_volt from Kapitza-Dirac (in ExpConstants.txt)
	* dat_chan:             (0-7) Specifies the channel to which the ramp waveform is added */

	int j;                      /* loop index */
	int j_start = digitize_time(t_start);
	int j_stop = digitize_time(t_stop);
	int j_ref = digitize_time(t_ref);
	double x;
	double y;
	unsigned int(*convert_func)(double);

	convert_func = get_convert_func(dat_chan);
	freq_Hz_1 = freq_Hz_1 * BPW / SMP_CLK;
	freq_Hz_2 = freq_Hz_2 * BPW / SMP_CLK;
	freq_Hz_3 = freq_Hz_3 * BPW / SMP_CLK;

	if (j_stop > j_last) {
		j_last = j_stop;
	}

	for (j = j_start; j < j_stop; j++) {
		x = lattice_depth * (1 + rel_amp_1 * sin(2 * PI * freq_Hz_1 * (j - j_start + j_ref)) + rel_amp_2 * sin(2 * PI * freq_Hz_2 * (j - j_start + j_ref)) + rel_amp_3 * sin(phase_pi * PI + 2 * PI * freq_Hz_3 * (j - j_start + j_ref)));
		y = voltage_offset + calib_volt + 0.5 * log10(x / calib_depth);
		NI_waveform[BPW*(n_offset + j) + (dat_chan % NUMCHANNELS)] = convert_func(y);
	}
}

_declspec (dllexport) void insert_ramp_log_sine3_return(double lattice_depth, double rel_amp_1, double rel_amp_2, double rel_amp_3,
			double freq_Hz_1, double freq_Hz_2, double freq_Hz_3,
			double phase_pi, double t_start, double ramp_dur, double hold_dur,
			double voltage_offset, double calib_volt, double calib_depth,
			int dat_chan, uInt32 *NI_waveform) {
	/* Adds sinusoidal modulation with 3 frequency components and phase offset between two components. Ramps to desired state and reverses back to initial state.
	*
	* Arguments:
	* lattice_depth:		lattice depth in recoils
	* rel_amp:				relative amplitude in % lattice depth of sinusoidal modulation of tunneling
	* freq_Hz:				frequency in Hz
	* phase_pi				phase offset between two frequency components
	* t_start:				Leading edge time, in milliseconds from the analog output start trigger
	* ramp_dur:				ramp time for effective tunneling, from 0 to final value
	* hold_dur:				hold time, to allow for ramping the effective tilt, etc
	* tstop:				Trailing edge time, in milliseconds from the analog output start trigger
	* voltage_offset:		voltage difference between PD value on panel and scope
	* calib_volt:			voltage from Kapitza-Dirac (in ExpConstants.txt)
	* calib_depth:			lattice depth corresponding to calib_volt from Kapitza-Dirac (in ExpConstants.txt)
	* dat_chan:             (0-7) Specifies the channel to which the ramp waveform is added */

	int j;                      /* loop index */
	int j_ramp_up_start = digitize_time(t_start);
	int j_ramp_up_stop = digitize_time(t_start + ramp_dur);
	int j_ramp_down_start = digitize_time(t_start + ramp_dur + hold_dur);
	int j_ramp_down_stop = digitize_time(t_start + ramp_dur + hold_dur + ramp_dur);
	int j_ramp_dur = j_ramp_up_stop - j_ramp_up_start + 1;

	double amp_1;
	double amp_2;
	double amp_3;

	double x;
	double y;
	unsigned int (*convert_func)(double);

    convert_func = get_convert_func(dat_chan);
	freq_Hz_1 = freq_Hz_1 * BPW/SMP_CLK;
	freq_Hz_2 = freq_Hz_2 * BPW/SMP_CLK;
	freq_Hz_3 = freq_Hz_3 * BPW/SMP_CLK;

	if (j_ramp_down_stop > j_last) {
        j_last = j_ramp_down_stop;
    }

	for (j = j_ramp_up_start; j < j_ramp_down_stop; j++) {

		if (j < j_ramp_up_stop) {
			amp_1 = ( (double)(j-j_ramp_up_start) / j_ramp_dur) * rel_amp_1;
			amp_2 = ( (double)(j-j_ramp_up_start) / j_ramp_dur) * rel_amp_2;
			amp_3 = ( (double)(j-j_ramp_up_start) / j_ramp_dur) * rel_amp_3;
			x = lattice_depth * (1 + amp_1 * sin(2 * PI * freq_Hz_1 * (j - j_ramp_up_start)) + amp_2 * sin(2 * PI * freq_Hz_2 * (j - j_ramp_up_start)) + amp_3 * sin(phase_pi * PI + 2 * PI * freq_Hz_3 * (j - j_ramp_up_start)) );
			y = voltage_offset + calib_volt + 0.5 * log10(x / calib_depth);
			NI_waveform[BPW*(n_offset + j ) + (dat_chan % NUMCHANNELS)] = convert_func(y);
		}

		if ((j >= j_ramp_up_stop) && (j < j_ramp_down_start)) {
			amp_1 = rel_amp_1;
			amp_2 = rel_amp_2;
			amp_3 = rel_amp_3;
			x = lattice_depth * (1 + amp_1 * sin(2 * PI * freq_Hz_1 * (j - j_ramp_up_start)) + amp_2 * sin(2 * PI * freq_Hz_2 * (j - j_ramp_up_start)) + amp_3 * sin(phase_pi * PI + 2 * PI * freq_Hz_3 * (j - j_ramp_up_start)) );
			y = voltage_offset + calib_volt + 0.5 * log10(x / calib_depth);
			NI_waveform[BPW*(n_offset + j ) + (dat_chan % NUMCHANNELS)] = convert_func(y);
		}

		if ((j >= j_ramp_down_start) && (j < j_ramp_down_stop)) {
			amp_1 = (1 - (double)(j-j_ramp_down_start)/j_ramp_dur ) * rel_amp_1;
			amp_2 = (1 - (double)(j-j_ramp_down_start)/j_ramp_dur ) * rel_amp_2;
			amp_3 = (1 - (double)(j-j_ramp_down_start)/j_ramp_dur ) * rel_amp_3;
			x = lattice_depth * (1 + amp_1 * sin(2 * PI * freq_Hz_1 * (j - j_ramp_up_start)) + amp_2 * sin(2 * PI * freq_Hz_2 * (j - j_ramp_up_start)) + amp_3 * sin(phase_pi * PI + 2 * PI * freq_Hz_3 * (j - j_ramp_up_start)) );
			y = voltage_offset + calib_volt + 0.5 * log10(x / calib_depth);
			NI_waveform[BPW*(n_offset + j ) + (dat_chan % NUMCHANNELS)] = convert_func(y);
		}
		
	}
	
}

_declspec (dllexport) void insert_ramp_log_sine3_no_return(double lattice_depth, double rel_amp_1, double rel_amp_2, double rel_amp_3,
			double freq_Hz_1, double freq_Hz_2, double freq_Hz_3,
			double phase_pi, double t_start, double ramp_dur, double hold_dur,
			double voltage_offset, double calib_volt, double calib_depth,
			int dat_chan, uInt32 *NI_waveform) {
	/* Adds sinusoidal modulation with 3 frequency components and phase offset between two components. Ramps to desired state and does not reverse to inital state.
	*
	* Arguments:
	* lattice_depth:		lattice depth in recoils
	* rel_amp:				relative amplitude in % lattice depth of sinusoidal modulation of tunneling
	* freq_Hz:				frequency in Hz
	* phase_pi				phase offset between two frequency components
	* t_start:				Leading edge time, in milliseconds from the analog output start trigger
	* ramp_dur:				ramp time for effective tunneling, from 0 to final value
	* hold_dur:				hold time, to allow for ramping the effective tilt, etc
	* voltage_offset:		voltage difference between PD value on panel and scope
	* calib_volt:			voltage from Kapitza-Dirac (in ExpConstants.txt)
	* calib_depth:			lattice depth corresponding to calib_volt from Kapitza-Dirac (in ExpConstants.txt)
	* dat_chan:             (0-7) Specifies the channel to which the ramp waveform is added */

	int j;                      /* loop index */
	int j_ramp_up_start = digitize_time(t_start);
	int j_ramp_up_stop = digitize_time(t_start + ramp_dur);
	int j_stop = digitize_time(t_start + ramp_dur + hold_dur);
	int j_ramp_dur = j_ramp_up_stop - j_ramp_up_start + 1;

	double amp_1;
	double amp_2;
	double amp_3;

	double x;
	double y;
	unsigned int (*convert_func)(double);

    convert_func = get_convert_func(dat_chan);
	freq_Hz_1 = freq_Hz_1 * BPW/SMP_CLK;
	freq_Hz_2 = freq_Hz_2 * BPW/SMP_CLK;
	freq_Hz_3 = freq_Hz_3 * BPW/SMP_CLK;

	if (j_stop > j_last) {
        j_last = j_stop;
    }

	for (j = j_ramp_up_start; j < j_stop; j++) {

		if (j < j_ramp_up_stop) {
			amp_1 = ( (double)(j-j_ramp_up_start) / j_ramp_dur ) * rel_amp_1;
			amp_2 = ( (double)(j-j_ramp_up_start) / j_ramp_dur ) * rel_amp_2;
			amp_3 = ( (double)(j-j_ramp_up_start) / j_ramp_dur ) * rel_amp_3;
			x = lattice_depth * (1 + amp_1 * sin(2 * PI * freq_Hz_1 * (j - j_ramp_up_start)) + amp_2 * sin(2 * PI * freq_Hz_2 * (j - j_ramp_up_start)) + amp_3 * sin(phase_pi * PI + 2 * PI * freq_Hz_3 * (j - j_ramp_up_start)) );
			y = voltage_offset + calib_volt + 0.5 * log10(x / calib_depth);
			NI_waveform[BPW*(n_offset + j ) + (dat_chan % NUMCHANNELS)] = convert_func(y);
		}

		if ((j >= j_ramp_up_stop) && (j < j_stop)) {
			amp_1 = rel_amp_1;
			amp_2 = rel_amp_2;
			amp_3 = rel_amp_3;
			x = lattice_depth * (1 + amp_1 * sin(2 * PI * freq_Hz_1 * (j - j_ramp_up_start)) + amp_2 * sin(2 * PI * freq_Hz_2 * (j - j_ramp_up_start)) + amp_3 * sin(phase_pi * PI + 2 * PI * freq_Hz_3 * (j - j_ramp_up_start)) );
			y = voltage_offset + calib_volt + 0.5 * log10(x / calib_depth);
			NI_waveform[BPW*(n_offset + j ) + (dat_chan % NUMCHANNELS)] = convert_func(y);
		}

	}
	
}

_declspec (dllexport) void insert_ramp_log_sine3_phase_return(double lattice_depth, double rel_amp_1, double rel_amp_2, double rel_amp_3,
	double freq_Hz_1, double freq_Hz_2, double freq_Hz_3,
	double phase_pi_start, double phase_pi_stop, double phase_ramp_dur,
	double t_start, double ramp_dur, double hold_dur,
	double voltage_offset, double calib_volt, double calib_depth,
	int dat_chan, uInt32 *NI_waveform) {
	/* Adds sinusoidal modulation with 3 frequency components and phase offset between two components. Allows for dynamical ramps of the statistical phase. Ramps to desired state and reverses to initial state.
	*
	* Arguments:
	* lattice_depth:		lattice depth in recoils
	* rel_amp:				relative amplitude in % lattice depth of sinusoidal modulation of tunneling
	* freq_Hz:				frequency in Hz
	* phase_pi_start:		starting value of statistical phase. Function is written to always start at 0.
	* phase_pi_stop:		final value of statistical phase
	* phase_ramp_dur:		ramp time for the statistical phase
	* t_start:				Leading edge time, in milliseconds from the analog output start trigger
	* ramp_dur:				ramp time for effective tunneling, from 0 to final value
	* hold_dur:				hold time, to allow for ramping the effective tilt, etc
	* voltage_offset:		voltage difference between PD value on panel and scope
	* calib_volt:			voltage from Kapitza-Dirac (in ExpConstants.txt)
	* calib_depth:			lattice depth corresponding to calib_volt from Kapitza-Dirac (in ExpConstants.txt)
	* dat_chan:             (0-7) Specifies the channel to which the ramp waveform is added */

	int j;                      /* loop index */
	int j_ramp_up_start = digitize_time(t_start);
	int j_ramp_up_stop = digitize_time(t_start + ramp_dur);
	int j_ramp_phase_up_start = digitize_time(t_start + ramp_dur + hold_dur);
	int j_ramp_phase_up_stop = digitize_time(t_start + ramp_dur + hold_dur + phase_ramp_dur);
	int j_ramp_phase_down_stop = digitize_time(t_start + ramp_dur + hold_dur + 2 * phase_ramp_dur);
	int j_ramp_down_start = digitize_time(t_start + ramp_dur + 2*hold_dur + 2*phase_ramp_dur);
	int j_stop = digitize_time(t_start + ramp_dur + 2*hold_dur + 2*phase_ramp_dur + ramp_dur);
	int j_ramp_dur = j_ramp_up_stop - j_ramp_up_start + 1;
	int j_ramp_phase_dur = j_ramp_phase_up_stop - j_ramp_phase_up_start + 1;

	double amp_1;
	double amp_2;
	double amp_3;
	double phase_pi;

	double x;
	double y;
	unsigned int(*convert_func)(double);

	convert_func = get_convert_func(dat_chan);
	freq_Hz_1 = freq_Hz_1 * BPW / SMP_CLK;
	freq_Hz_2 = freq_Hz_2 * BPW / SMP_CLK;
	freq_Hz_3 = freq_Hz_3 * BPW / SMP_CLK;

	if (j_stop > j_last) {
		j_last = j_stop;
	}

	for (j = j_ramp_up_start; j < j_stop; j++) {

		/* ramp J */
		if (j < j_ramp_up_stop) {
			amp_1 = ((double)(j - j_ramp_up_start) / j_ramp_dur) * rel_amp_1;
			amp_2 = ((double)(j - j_ramp_up_start) / j_ramp_dur) * rel_amp_2;
			amp_3 = ((double)(j - j_ramp_up_start) / j_ramp_dur) * rel_amp_3;
			x = lattice_depth * (1 + amp_1 * sin(2 * PI * freq_Hz_1 * (j - j_ramp_up_start)) + amp_2 * sin(2 * PI * freq_Hz_2 * (j - j_ramp_up_start)) + amp_3 * sin(phase_pi_start * PI + 2 * PI * freq_Hz_3 * (j - j_ramp_up_start)));
			y = voltage_offset + calib_volt + 0.5 * log10(x / calib_depth);
			NI_waveform[BPW*(n_offset + j) + (dat_chan % NUMCHANNELS)] = convert_func(y);
		}

		/* ramp tilt */
		if ((j >= j_ramp_up_stop) && (j < j_ramp_phase_up_start)) {
			amp_1 = rel_amp_1;
			amp_2 = rel_amp_2;
			amp_3 = rel_amp_3;
			x = lattice_depth * (1 + amp_1 * sin(2 * PI * freq_Hz_1 * (j - j_ramp_up_start)) + amp_2 * sin(2 * PI * freq_Hz_2 * (j - j_ramp_up_start)) + amp_3 * sin(phase_pi_start * PI + 2 * PI * freq_Hz_3 * (j - j_ramp_up_start)));
			y = voltage_offset + calib_volt + 0.5 * log10(x / calib_depth);
			NI_waveform[BPW*(n_offset + j) + (dat_chan % NUMCHANNELS)] = convert_func(y);
		}

		/* ramp phase */
		if ((j >= j_ramp_phase_up_start) && (j < j_ramp_phase_up_stop)) {
			amp_1 = rel_amp_1;
			amp_2 = rel_amp_2;
			amp_3 = rel_amp_3;
			if (phase_pi_start != phase_pi_stop) {
				phase_pi = ((double)(j - j_ramp_phase_up_start) / j_ramp_phase_dur) * phase_pi_stop;
				x = lattice_depth * (1 + amp_1 * sin(2 * PI * freq_Hz_1 * (j - j_ramp_up_start)) + amp_2 * sin(2 * PI * freq_Hz_2 * (j - j_ramp_up_start)) + amp_3 * sin(phase_pi * PI + 2 * PI * freq_Hz_3 * (j - j_ramp_up_start)));
				y = voltage_offset + calib_volt + 0.5 * log10(x / calib_depth);
				NI_waveform[BPW*(n_offset + j) + (dat_chan % NUMCHANNELS)] = convert_func(y);
			}
			else {
				x = lattice_depth * (1 + amp_1 * sin(2 * PI * freq_Hz_1 * (j - j_ramp_up_start)) + amp_2 * sin(2 * PI * freq_Hz_2 * (j - j_ramp_up_start)) + amp_3 * sin(phase_pi_start * PI + 2 * PI * freq_Hz_3 * (j - j_ramp_up_start)));
				y = voltage_offset + calib_volt + 0.5 * log10(x / calib_depth);
				NI_waveform[BPW*(n_offset + j) + (dat_chan % NUMCHANNELS)] = convert_func(y);
			}
		}
		
		/* ramp phase (return) */
		if ((j >= j_ramp_phase_up_stop) && (j < j_ramp_phase_down_stop)) {
			amp_1 = rel_amp_1;
			amp_2 = rel_amp_2;
			amp_3 = rel_amp_3;
			if (phase_pi_start != phase_pi_stop) {
				phase_pi = (1 - (double)(j - j_ramp_phase_up_stop) / j_ramp_phase_dur) * phase_pi_stop;
				x = lattice_depth * (1 + amp_1 * sin(2 * PI * freq_Hz_1 * (j - j_ramp_up_start)) + amp_2 * sin(2 * PI * freq_Hz_2 * (j - j_ramp_up_start)) + amp_3 * sin(phase_pi * PI + 2 * PI * freq_Hz_3 * (j - j_ramp_up_start)));
				y = voltage_offset + calib_volt + 0.5 * log10(x / calib_depth);
				NI_waveform[BPW*(n_offset + j) + (dat_chan % NUMCHANNELS)] = convert_func(y);
			}
			else {
				x = lattice_depth * (1 + amp_1 * sin(2 * PI * freq_Hz_1 * (j - j_ramp_up_start)) + amp_2 * sin(2 * PI * freq_Hz_2 * (j - j_ramp_up_start)) + amp_3 * sin(phase_pi_start * PI + 2 * PI * freq_Hz_3 * (j - j_ramp_up_start)));
				y = voltage_offset + calib_volt + 0.5 * log10(x / calib_depth);
				NI_waveform[BPW*(n_offset + j) + (dat_chan % NUMCHANNELS)] = convert_func(y);
			}
		}

		/* ramp tilt (return) */
		if ((j >= j_ramp_phase_down_stop) && (j < j_ramp_down_start)) {
			amp_1 = rel_amp_1;
			amp_2 = rel_amp_2;
			amp_3 = rel_amp_3;
			x = lattice_depth * (1 + amp_1 * sin(2 * PI * freq_Hz_1 * (j - j_ramp_up_start)) + amp_2 * sin(2 * PI * freq_Hz_2 * (j - j_ramp_up_start)) + amp_3 * sin(phase_pi_start * PI + 2 * PI * freq_Hz_3 * (j - j_ramp_up_start)));
			y = voltage_offset + calib_volt + 0.5 * log10(x / calib_depth);
			NI_waveform[BPW*(n_offset + j) + (dat_chan % NUMCHANNELS)] = convert_func(y);
		}

		/* ramp J (return) */
		if ((j >= j_ramp_down_start)  && (j < j_stop)) {
			amp_1 = (1 - (double)(j - j_ramp_down_start) / j_ramp_dur) * rel_amp_1;
			amp_2 = (1 - (double)(j - j_ramp_down_start) / j_ramp_dur) * rel_amp_2;
			amp_3 = (1 - (double)(j - j_ramp_down_start) / j_ramp_dur) * rel_amp_3;
			x = lattice_depth * (1 + amp_1 * sin(2 * PI * freq_Hz_1 * (j - j_ramp_up_start)) + amp_2 * sin(2 * PI * freq_Hz_2 * (j - j_ramp_up_start)) + amp_3 * sin(phase_pi_start * PI + 2 * PI * freq_Hz_3 * (j - j_ramp_up_start)));
			y = voltage_offset + calib_volt + 0.5 * log10(x / calib_depth);
			NI_waveform[BPW*(n_offset + j) + (dat_chan % NUMCHANNELS)] = convert_func(y);
		}

	}

}

_declspec (dllexport) void insert_ramp_log_sine3_phase_no_return(double lattice_depth, double rel_amp_1, double rel_amp_2, double rel_amp_3,
	double freq_Hz_1, double freq_Hz_2, double freq_Hz_3,
	double phase_pi_start, double phase_pi_stop, double phase_ramp_dur,
	double t_start, double ramp_dur, double hold_dur,
	double voltage_offset, double calib_volt, double calib_depth,
	int dat_chan, uInt32 *NI_waveform) {
	/* Adds sinusoidal modulation with 3 frequency components and phase offset between two components. Allows for dynamical ramps of the statistical phase. Ramps to desired state and stops.
	*
	* Arguments:
	* lattice_depth:		lattice depth in recoils
	* rel_amp:				relative amplitude in % lattice depth of sinusoidal modulation of tunneling
	* freq_Hz:				frequency in Hz
	* phase_pi_start:		starting value of statistical phase. Function is written to always start at 0.
	* phase_pi_stop:		final value of statistical phase
	* phase_ramp_dur:		ramp time for the statistical phase
	* t_start:				Leading edge time, in milliseconds from the analog output start trigger
	* ramp_dur:				ramp time for effective tunneling, from 0 to final value
	* hold_dur:				hold time, to allow for ramping the effective tilt, etc
	* voltage_offset:		voltage difference between PD value on panel and scope
	* calib_volt:			voltage from Kapitza-Dirac (in ExpConstants.txt)
	* calib_depth:			lattice depth corresponding to calib_volt from Kapitza-Dirac (in ExpConstants.txt)
	* dat_chan:             (0-7) Specifies the channel to which the ramp waveform is added */

	int j;                      /* loop index */
	int j_ramp_up_start = digitize_time(t_start);
	int j_ramp_up_stop = digitize_time(t_start + ramp_dur);
	int j_ramp_phase_start = digitize_time(t_start + ramp_dur + hold_dur);
	int j_stop = digitize_time(t_start + ramp_dur + hold_dur + phase_ramp_dur);
	int j_ramp_dur = j_ramp_up_stop - j_ramp_up_start + 1;

	double amp_1;
	double amp_2;
	double amp_3;
	double phase_pi;

	double x;
	double y;
	unsigned int(*convert_func)(double);

	convert_func = get_convert_func(dat_chan);
	freq_Hz_1 = freq_Hz_1 * BPW / SMP_CLK;
	freq_Hz_2 = freq_Hz_2 * BPW / SMP_CLK;
	freq_Hz_3 = freq_Hz_3 * BPW / SMP_CLK;

	if (j_stop > j_last) {
		j_last = j_stop;
	}

	for (j = j_ramp_up_start; j < j_stop; j++) {

		if (j < j_ramp_up_stop) {
			amp_1 = ((double)(j - j_ramp_up_start) / j_ramp_dur) * rel_amp_1;
			amp_2 = ((double)(j - j_ramp_up_start) / j_ramp_dur) * rel_amp_2;
			amp_3 = ((double)(j - j_ramp_up_start) / j_ramp_dur) * rel_amp_3;
			x = lattice_depth * (1 + amp_1 * sin(2 * PI * freq_Hz_1 * (j - j_ramp_up_start)) + amp_2 * sin(2 * PI * freq_Hz_2 * (j - j_ramp_up_start)) + amp_3 * sin(phase_pi_start * PI + 2 * PI * freq_Hz_3 * (j - j_ramp_up_start)));
			y = voltage_offset + calib_volt + 0.5 * log10(x / calib_depth);
			NI_waveform[BPW*(n_offset + j) + (dat_chan % NUMCHANNELS)] = convert_func(y);
		}

		if ((j >= j_ramp_up_stop) && (j < j_ramp_phase_start)) {
			amp_1 = rel_amp_1;
			amp_2 = rel_amp_2;
			amp_3 = rel_amp_3;
			x = lattice_depth * (1 + amp_1 * sin(2 * PI * freq_Hz_1 * (j - j_ramp_up_start)) + amp_2 * sin(2 * PI * freq_Hz_2 * (j - j_ramp_up_start)) + amp_3 * sin(phase_pi_start * PI + 2 * PI * freq_Hz_3 * (j - j_ramp_up_start)));
			y = voltage_offset + calib_volt + 0.5 * log10(x / calib_depth);
			NI_waveform[BPW*(n_offset + j) + (dat_chan % NUMCHANNELS)] = convert_func(y);
		}

		if ((j >= j_ramp_phase_start) && (j < j_stop)) {
			amp_1 = rel_amp_1;
			amp_2 = rel_amp_2;
			amp_3 = rel_amp_3;
			if (phase_pi_start != phase_pi_stop) {
				phase_pi = ((double)(j - j_ramp_phase_start) / (j_stop - j_ramp_phase_start + 1)) * phase_pi_stop;
				x = lattice_depth * (1 + amp_1 * sin(2 * PI * freq_Hz_1 * (j - j_ramp_up_start)) + amp_2 * sin(2 * PI * freq_Hz_2 * (j - j_ramp_up_start)) + amp_3 * sin(phase_pi * PI + 2 * PI * freq_Hz_3 * (j - j_ramp_up_start)));
				y = voltage_offset + calib_volt + 0.5 * log10(x / calib_depth);
				NI_waveform[BPW*(n_offset + j) + (dat_chan % NUMCHANNELS)] = convert_func(y);
			}
			else {
				x = lattice_depth * (1 + amp_1 * sin(2 * PI * freq_Hz_1 * (j - j_ramp_up_start)) + amp_2 * sin(2 * PI * freq_Hz_2 * (j - j_ramp_up_start)) + amp_3 * sin(phase_pi_start * PI + 2 * PI * freq_Hz_3 * (j - j_ramp_up_start)));
				y = voltage_offset + calib_volt + 0.5 * log10(x / calib_depth);
				NI_waveform[BPW*(n_offset + j) + (dat_chan % NUMCHANNELS)] = convert_func(y);
			}
		}

	}

}

_declspec (dllexport) void insert_ramp_log_sine3(double lattice_depth, double rel_amp_1, double rel_amp_2, double rel_amp_3,
	double freq_Hz_1, double freq_Hz_2, double freq_Hz_3,
	double phase_pi, double tunneling_init, double tunneling_final, double detuning_Hz_init, double detuning_Hz_final,
	double t_start, double t_stop, double t_ref,
	double voltage_offset, double calib_volt, double calib_depth,
	int dat_chan, uInt32 *NI_waveform) {
	/* Adds sinusoidal modulation with 3 frequency components and phase offset between two components. Can ramp effective tunneling, statistical phase, OR effective on-site interaction. 
	*
	*
	* Arguments:
	* lattice_depth:		lattice depth in recoils
	* rel_amp:				relative amplitude in % lattice depth of sinusoidal modulation of tunneling
	* freq_Hz_1:			center frequency E in Hz
	* freq_Hz_2:			sideband E-U_0 in Hz
	* freq_Hz_3:			sideband E+U_0 in Hz
	* phase_pi:				statistical phase in pi
	* tunneling_init:		initial value of effective tunneling, in units of calibrated value 
	* tunneling_final:		final value of effective tunneling
	* detuning_Hz:			interaction U in Hz
	* t_start:				Leading edge time, in milliseconds from the analog output start trigger
	* t_stop:				trailing edge time, in milliseconds from the analog output start trigger
	* t_ref:				To time-shift the three components by the same amount
	* voltage_offset:		voltage difference between PD value on panel and scope
	* calib_volt:			voltage from Kapitza-Dirac (in ExpConstants.txt)
	* calib_depth:			lattice depth corresponding to calib_volt from Kapitza-Dirac (in ExpConstants.txt)
	* dat_chan:             (0-7) Specifies the channel to which the ramp waveform is added */

	int j;                      /* loop index */
	int j_start = digitize_time(t_start);
	int j_stop = digitize_time(t_stop);
	int j_ref = digitize_time(t_ref);
	int j_ramp_dur = j_stop - j_start + 1;

	double amp_1;
	double amp_2;
	double amp_3;
	double freq_2;
	double freq_3;

	double x;
	double y;
	unsigned int(*convert_func)(double);

	convert_func = get_convert_func(dat_chan);
	freq_Hz_1 = freq_Hz_1 * BPW / SMP_CLK;
	freq_Hz_2 = freq_Hz_2 * BPW / SMP_CLK;
	freq_Hz_3 = freq_Hz_3 * BPW / SMP_CLK;
	detuning_Hz_init = detuning_Hz_init * BPW / SMP_CLK;
	detuning_Hz_final = detuning_Hz_final * BPW / SMP_CLK;

	if (j_stop > j_last) {
		j_last = j_stop;
	}

	for (j = j_start; j < j_stop; j++) {
		amp_1 = ((double)(j - j_start) / j_ramp_dur) * (tunneling_final - tunneling_init) * rel_amp_1 + (tunneling_init * rel_amp_1);
		amp_2 = ((double)(j - j_start) / j_ramp_dur) * (tunneling_final - tunneling_init) * rel_amp_2 + (tunneling_init * rel_amp_2);
		amp_3 = ((double)(j - j_start) / j_ramp_dur) * (tunneling_final - tunneling_init) * rel_amp_3 + (tunneling_init * rel_amp_3);
		freq_2 = freq_Hz_2 + 0.5 * ((double)(j - j_start) / j_ramp_dur) * (detuning_Hz_final - detuning_Hz_init) + detuning_Hz_init;
		freq_3 = freq_Hz_3 - 0.5 * ((double)(j - j_start) / j_ramp_dur) * (detuning_Hz_final - detuning_Hz_init) - detuning_Hz_init;
		x = lattice_depth * (1 + amp_1 * sin(2 * PI * freq_Hz_1 * (j - j_start + j_ref)) + amp_2 * sin(2 * PI * freq_2 * (j - j_start + j_ref)) + amp_3 * sin(phase_pi * PI + 2 * PI * freq_3 * (j - j_start + j_ref)));
		y = voltage_offset + calib_volt + 0.5 * log10(x / calib_depth);
		NI_waveform[BPW*(n_offset + j) + (dat_chan % NUMCHANNELS)] = convert_func(y);
	}

}

_declspec (dllexport) void insert_tunneling_ramp(double conversion_coeffs[11],
    double start_tunneling, double stop_tunneling,
    double t_start, double t_stop,
    double voltage_offset, double calib_volt, double calib_depth,
    int dat_chan, uInt32* NI_waveform)
{ 
    /* insert ramp in lattice PD voltage that is linear in tunneling*/
       /* consider using a trick from Hacker's Delight */
       /* perhaps precalculate 'BPW/SMP_CLK' */
    int j;                      /* loop index */
    int j_start = digitize_time(t_start);
    int j_stop = digitize_time(t_stop);
    double slope = (stop_tunneling - start_tunneling) / ((t_stop - t_start) * SMP_CLK / BPW / 1000);
    double x;
    double y;
    double u;

    unsigned int (*convert_func)(double);
    convert_func = get_convert_func(dat_chan);

    if (j_stop > j_last) {
        j_last = j_stop;
    }

    for (j = j_start; j < j_stop; j++) {
        u = start_tunneling + slope * (j - j_start);
        x = depth_from_tunnel_calibrated(conversion_coeffs, u);
        y = voltage_offset + calib_volt + 0.5 * log10(x / calib_depth);
        NI_waveform[BPW * (n_offset + j) + (dat_chan % NUMCHANNELS)] = convert_func(y);
    }
}

_declspec (dllexport) void insert_tunneling_gauge_ramp2(double conversion_coeffs[4],
    double start_tunneling, double stop_tunneling,
    double t_start, double t_stop,
    double calib_volt, double calib_depth,
    int dat_chan, uInt32* NI_waveform)
{
    /* insert ramp in lattice PD voltage that is linear in tunneling*/
       /* consider using a trick from Hacker's Delight */
       /* perhaps precalculate 'BPW/SMP_CLK' */
    int j;                      /* loop index */
    int j_start = digitize_time(t_start);
    int j_stop = digitize_time(t_stop);
    double slope = (stop_tunneling - start_tunneling) / ((t_stop - t_start) * SMP_CLK / BPW / 1000);
    double x;
    double y;
    double u;

    unsigned int (*convert_func)(double);
    convert_func = get_convert_func(dat_chan);

    if (j_stop > j_last) {
        j_last = j_stop;
    }

    for (j = j_start; j < j_stop; j++) {
        u = start_tunneling + slope * (j - j_start);
        x = gauge_depth_from_tunnel_calibrated(conversion_coeffs, u);
        if (x > 0) {
            y = calib_volt + 0.5 * log10(x / calib_depth);
        }
        else {
            y = 0;
        }       
        NI_waveform[BPW * (n_offset + j) + (dat_chan % NUMCHANNELS)] = convert_func(y);
    }
}

_declspec (dllexport) void insert_tunneling_gauge_ramp(double conversion_coeffs[4],
    double start_tunneling, double stop_tunneling,
    double t_start, double t_stop,
    double calib_volt, double calib_depth,
    int dat_chan, uInt32* NI_waveform)
{
    /* insert ramp in gauge PD voltage that is linear in tunneling
     * file to generate coefficients from calibration equation: convert_ramp_files.m */
       
    int j;                      /* loop index */
    int j_start = digitize_time(t_start);
    int j_stop = digitize_time(t_stop);
    double slope = (stop_tunneling - start_tunneling) / ((t_stop - t_start) * SMP_CLK / BPW / 1000);
    double x;
    double y;
    double u;

    unsigned int (*convert_func)(double);
    convert_func = get_convert_func(dat_chan);

    if (j_stop > j_last) {
        j_last = j_stop;
    }

    for (j = j_start; j < j_stop; j++) {
        u = start_tunneling + slope * (j - j_start);
        x = gauge_depth_from_tunnel_calibrated(conversion_coeffs, u);
        if (x <= 0) {
            y = 0;
        }
        else {
            y = calib_volt + 0.5 * log10(x / calib_depth);           
        }      
        NI_waveform[BPW * (n_offset + j) + (dat_chan % NUMCHANNELS)] = convert_func(y);
    }
}

_declspec (dllexport) void insert_sine3(double offset, double amp1, double amp2, double amp3, 
            double freq1, double freq2, double freq3,
            double phase_pi, double t_start, double t_stop,
            int dat_chan, uInt32 *NI_waveform) {
	/* Adds sinusoidal modulation with 3 frequency components and phase offset between two components.
	 *
	 * Arguments:
     * offset:              offset for sine waveform, in volts
     * amp:                 amplitude for sine waveform, in volts
     * freq_Hz:             frequency in Hz
     * t_start:             Leading edge time, in milliseconds from the analog output start trigger
     * tstop:               Trailing edge time, in milliseconds from the analog output start trigger
     * channel:             (0-7) Specifies the channel to which the ramp waveform is added */

	int j;                      /* loop index */
    int j_start = digitize_time(t_start);
    int j_stop = digitize_time(t_stop);
	unsigned int (*convert_func)(double);

    convert_func = get_convert_func(dat_chan);
	freq1 = 2*PI*freq1*BPW/SMP_CLK;
	freq2 = 2*PI*freq2*BPW/SMP_CLK;
	freq3 = 2*PI*freq3*BPW/SMP_CLK;

	if (j_stop > j_last) {
        j_last = j_stop;
    }

    for (j = j_start; j < j_stop; j++) {
		NI_waveform[BPW*(n_offset + j ) + (dat_chan % NUMCHANNELS)] = convert_func(amp1*sin(freq1*(j - j_start)) + amp2*sin(freq2*(j - j_start)) + amp3*sin(freq3*(j - j_start) + PI*phase_pi) + offset);
    }
}

_declspec (dllexport) void insert_sine_phase(double offset, double amp, double freq,
            double phase_pi, double t_start, double t_stop,
            int dat_chan, uInt32 *NI_waveform) {

    /* Adds a sine waveform to a selected channel starting with initial phase pi*phase_pi.
     * 
     * Arguments:
     * offset:              offset for sine waveform, in volts
     * amp:                 amplitude for sine waveform, in volts
     * freq_Hz:             frequency in Hz
     * t_start:             Leading edge time, in milliseconds from the analog output start trigger
     * tstop:               Trailing edge time, in milliseconds from the analog output start trigger
     * channel:             (0-7) Specifies the channel to which the ramp waveform is added */

    int j;                      /* loop index */
    int j_start = digitize_time(t_start);
    int j_stop = digitize_time(t_stop);
	unsigned int (*convert_func)(double);

    convert_func = get_convert_func(dat_chan);
	freq = 2*PI*freq*BPW/SMP_CLK;

    if (j_stop > j_last) {
        j_last = j_stop;
    }

    for (j = j_start; j < j_stop; j++) {
		NI_waveform[BPW*(n_offset + j ) + (dat_chan % NUMCHANNELS)] = convert_func(amp*sin(freq*(j - j_start) + PI*phase_pi) + offset);
    }
}

_declspec (dllexport) void insert_sine_flip(double offset, double amp, double freq,
            double t_start, double t_stop, double fract,
            int dat_chan, uInt32 *NI_waveform) {
    /* Adds a sine waveform to a selected channel starting with phase zero and 
     * phase flip of 180 deg after fraction of modulation time.
     *
     * Arguments:
     * offset:                  offset for sine waveform, in volts
     * amp:                     amplitude for sine waveform, in volts
     * freq:                    frequency in Hz
     * t_start:                 Leading edge time, in milliseconds from the analog output start trigger
     * t_stop:                  Trailing edge time, in milliseconds from the analog output start trigger
     * fract:                   Fraction of the total duration after which Pi phase flip occurs
     * dat_chan:                (0-7) Specifies the channel to which the ramp waveform is added */

    int j;                      /* loop index */
    int j_start = digitize_time(t_start);
    int j_stop = digitize_time(t_stop);
    int j_flip = (int)(j_start + fract * (j_stop - j_start));
	unsigned int (*convert_func)(double);

    convert_func = get_convert_func(dat_chan);
	freq = 2*PI*freq*BPW/SMP_CLK;

    if (j_stop > j_last) {
        j_last = j_stop;
    }

    if (j_stop < j_start) {
        for (j = j_start; j < j_flip; j++) {
		    NI_waveform[BPW*(n_offset + j ) + (dat_chan % NUMCHANNELS)] = convert_func(amp*sin(freq*(j - j_start)) + offset);
        }
        for (j = j_flip; j < j_stop; j++) {
		    NI_waveform[BPW*(n_offset + j ) + (dat_chan % NUMCHANNELS)] = convert_func(amp*sin(freq*(j - j_start) + PI) + offset);
		}
	}
}

_declspec (dllexport) void insert_sine_ramp(double offset, double amp, 
            double freq_start, double freq_stop,
            double t_start, double t_stop,
            int dat_chan, uInt32 *NI_waveform) {
    /* Adds a sine ramp waveform to a selected channel starting with phase 
     * zero, with the frequency linearly increasing in time.
     *
     * Arguments:
     * offset:                  offset for sine waveform, in volts
     * amp:                     amplitude for sine waveform, in volts
     * freq:                    frequency in Hz
     * t_start:                 Leading edge time, in milliseconds from the analog output start trigger
     * t_stop:                  Trailing edge time, in milliseconds from the analog output start trigger
     * dat_chan:                (0-7) Specifies the channel to which the ramp waveform is added */

    int j;                      /* loop index */
    int j_start = digitize_time(t_start);
    int j_stop = digitize_time(t_stop);
    double PhiT;
	double omega_start = 2*PI*freq_start*BPW/SMP_CLK;
	double omega_stop = 2*PI*freq_stop*BPW/SMP_CLK;
    double omega_slope = (omega_stop - omega_start)/digitize_time((t_stop - t_start));
	unsigned int (*convert_func)(double);

    convert_func = get_convert_func(dat_chan);

    if (j_stop > j_last) {
        j_last = j_stop;
    }

    for (j = j_start; j < j_stop; j++) {
        PhiT = omega_start*(j - j_start) + omega_slope*pow((j - j_start), 2);
	    NI_waveform[BPW*(n_offset + j ) + (dat_chan % NUMCHANNELS)] = convert_func(offset + amp * sin(PhiT));
    }
}

_declspec (dllexport) void insert_log_ramp(double start_volts, double stop_volts,
            double t_start, double t_stop,
            int dat_chan, uInt32 *NI_waveform) {
    /* Adds a ramp waveform to a selected channel.
     *
     * Arguments:
     * start_volts:         initial amplitude for the ramp, in volts
     * stop_volts:          final amplitude for the ramp, in volts
     * t_start:             Leading edge time, in milliseconds from the analog output start trigger
     * t_stop:              Trailing edge time, in milliseconds from the analog output start trigger
     * dat_chan:            (0-7) Specifies the channel to which the ramp waveform is added */
    int j;                      /* loop index */
    int j_start = digitize_time(t_start);
    int j_stop = digitize_time(t_stop);
    double ratio;
    int jduration = j_stop - j_start;
	unsigned int (*convert_func)(double);

    convert_func = get_convert_func(dat_chan);

    if (j_stop > j_last) {
        j_last = j_stop;
    }

    for (j = j_start; j < j_stop; j++) {
        ratio = (double)(j - j_start)/jduration; 
		NI_waveform[BPW*(n_offset + j ) + (dat_chan % NUMCHANNELS)] = convert_func((double)start_volts + 0.5 * log10(1 + ratio * (pow(10, 2 * stop_volts - 2 * start_volts) - 1)));
    }
}

_declspec (dllexport) void insert_log_ramp_red_dipole(double red_start_volts, double red_start_freq,
        double lattice_start_depth, double lattice_stop_depth,
        double t_start, double t_stop,
        int dat_chan, uInt32 *NI_waveform) {
    /* Adds a ramp waveform to a selected channel 
     * 
     * Arguments:
     * red_start_volts:         initial amplitude for the ramp, in volts
     * red_start_freq:          initial red dipole frequency in Hz
     * lattice_start_depth:     initial lattice depth in recoils
     * lattice_stop_depth:      final lattice depth in recoils
     * t_start:                 Leading edge time, in milliseconds from the analog output start trigger
     * t_stop:                  Trailing edge time, in milliseconds from the analog output start trigger
     * dat_chan:                (0-7) Specifies the channel to which the ramp waveform is added */

    int j;                      /* loop index */
    int j_start = digitize_time(t_start);
    int j_stop = digitize_time(t_stop);
    double currentDepth;
    double currentVolt;
    double ratio;
    int jduration = j_stop - j_start;
	unsigned int (*convert_func)(double);

    convert_func = get_convert_func(dat_chan);

    if (j_stop > j_last) {
        j_last = j_stop;
    }

    for (j = j_start; j < j_stop; j++) {
        ratio = (double)(j - j_start) / jduration;
        currentDepth = lattice_start_depth + ratio * (lattice_stop_depth - lattice_start_depth);
        currentVolt = red_start_volts + 0.5 * log10(InteractionRatio(currentDepth) + pow(BlueDeconfinement(currentDepth) / red_start_freq, 2));
	    NI_waveform[BPW*(n_offset + j) + (dat_chan % NUMCHANNELS)] = convert_func(currentVolt);
    }
}

_declspec (dllexport) void insert_smooth_ramp(double start_volts, double stop_volts,
        double t_start, double t_stop,
        int dat_chan, uInt32 *NI_waveform) {
    /* Adds a smooth ramp waveform to a selected channel 
     *
     * Arguments:
     * start_volts:         initial amplitude for the ramp, in volts
     * stop_volts:          final amplitude for the ramp, in volts
     * tstart:              Leading edge time, in milliseconds from the analog output start trigger
     * tstop:               Trailing edge time, in milliseconds from the analog output start trigger
     * dat_chan:                (0-7) Specifies the channel to which the ramp waveform is added */

    int j;                      /* loop index */
    int j_start = digitize_time(t_start);
    int j_stop = digitize_time(t_stop);
    int jduration = j_stop - j_start;
    double x, f;
	unsigned int (*convert_func)(double);

    convert_func = get_convert_func(dat_chan);

    if (j_stop > j_last) {
        j_last = j_stop;
    }

    for (j = j_start; j < j_stop; j++) {
        x = (double)(j - j_start) / jduration;
        f = 10 * pow(x, 3) - 15 * pow(x, 4) + 6 * pow(x, 5);
		NI_waveform[BPW*(n_offset + j ) + (dat_chan % NUMCHANNELS)] = convert_func(start_volts + (stop_volts - start_volts) * f);
    }
}

_declspec (dllexport) void insert_afm_ramp(double start_volts, double stop_volts,
        double t_start, double t_stop, double t_ramp, 
        int dat_chan, uInt32 *NI_waveform) {
    /* Adds a smooth ramp waveform to a selected channel 
     *
     * Arguments:
     * start_volts:         initial amplitude for the ramp, in volts
     * stop_volts:          final amplitude for the ramp, in volts
     * tstart:              Leading edge time, in milliseconds from the analog output start trigger
     * tstop:               Trailing edge time, in milliseconds from the analog output start trigger
     * dat_chan:                (0-7) Specifies the channel to which the ramp waveform is added */

    int j;                      /* loop index */
    int j_start = digitize_time(t_start);
    int j_stop = digitize_time(t_stop);
	int j_ramp = digitize_time(t_ramp);
    int jduration = j_ramp - j_start;


    double x, f;
	unsigned int (*convert_func)(double);

    convert_func = get_convert_func(dat_chan);

    if (j_stop > j_last) {
        j_last = j_stop;
    }

    for (j = j_start; j < j_stop; j++) {
       
		x = (double)(j - j_start) / jduration;
        //f = 10 * pow(x, 3) - 15 * pow(x, 4) + 6 * pow(x, 5);
		f = sinh( (2.0*x-1.0)*3.0)/20.0+0.5;
		NI_waveform[BPW*(n_offset + j ) + (dat_chan % NUMCHANNELS)] = convert_func(start_volts + (stop_volts - start_volts) * f);
		
    }
}

_declspec (dllexport) void insert_smooth_box(double start_volts, double stop_volts,
        double t_start, double t_dur, double t_stop,
        int dat_chan, uInt32 *NI_waveform) {
    /* Adds a smooth box waveform to a selected channel 
     *
     * Arguments:
     * start_volts:         initial amplitude for the ramp, in volts
     * stop_volts:          final amplitude for the ramp, in volts
     * tstart:              Leading edge time, in milliseconds from the analog output start trigger
     * tstop:               Trailing edge time, in milliseconds from the analog output start trigger
     * dat_chan:                (0-7) Specifies the channel to which the ramp waveform is added */

    // int j;                      /* loop index */
    // int j_start = digitize_time(t_start);
    // int j_stop = digitize_time(t_stop);
    // int jduration = j_stop - j_start;
    // double x, f;
	// unsigned int (*convert_func)(double);
			
	int j;                      /* loop index */
    int j_start = digitize_time(t_start);
    int j_dur = digitize_time(t_dur);
    int j_stop = digitize_time(t_stop);
    double t_ramp;
    int j_ramp;
    int j_box_start;
    int j_box_end;
    double x, f;
	unsigned int (*convert_func)(double);

    printf("hi smooth box!\n");

	t_ramp = (double) (t_stop - t_start - t_dur)/2;
	j_ramp = digitize_time(t_ramp);
	j_box_start = digitize_time((double) t_start + t_ramp);
	j_box_end = digitize_time((double) t_start + t_ramp + t_dur);

    convert_func = get_convert_func(dat_chan);

    if (j_stop > j_last) {
        j_last = j_stop;
    }

    for (j = j_start; j < j_box_start; j++) {
        x = (double)(j - j_start) / j_ramp;
        f = 10 * pow(x, 3) - 15 * pow(x, 4) + 6 * pow(x, 5);
		NI_waveform[BPW*(n_offset + j) + (dat_chan % NUMCHANNELS)] = convert_func(start_volts + (stop_volts - start_volts) * f);
    }

    for (j = j_box_start; j < j_box_end; j++) {
		NI_waveform[BPW*(n_offset + j) + (dat_chan % NUMCHANNELS)] = convert_func(stop_volts);
    }

    for (j = j_box_end; j < j_stop; j++) {
        x = (double)(j - j_box_end) / j_ramp;
        f = 10 * pow(x, 3) - 15 * pow(x, 4) + 6 * pow(x, 5);
		NI_waveform[BPW*(n_offset + j) + (dat_chan % NUMCHANNELS)] = convert_func(stop_volts + (start_volts - stop_volts) * f);
    }
}

_declspec (dllexport) void insert_s(double background_volts, double final_volts,
            double t_start, double t_stop,
            int dat_chan, uInt32 *NI_waveform) {
    /* Adds an s-shaped smooth waveform to a selected channel 
     *
     * Arguments:
     * start_volts:         initial amplitude for the ramp, in volts, logarithmic photodiode assumed
     * stop_volts:          final asymptotic amplitude for the ramp, in volts, logarithmic photodiode assumed
     * t_start:             Leading edge time, in milliseconds from the analog output start trigger
     * t_stop:              Trailing edge time, in milliseconds from the analog output start trigger
     * dat_chan:                (0-7) Specifies the channel to which the ramp waveform is added */

    int j;                      /* loop index */
    int j_start = digitize_time(t_start);
    int j_stop = digitize_time(t_stop);
    int jduration;
    double f, fmax, g;
	unsigned int (*convert_func)(double);

    convert_func = get_convert_func(dat_chan);
    jduration = j_stop - j_start;

    if (j_stop > j_last) {
        j_last = j_stop;
    }

    fmax = 1.0 - 2.0 / (1 + exp(-pow(2.5 * (1.02 * jduration) / jduration, 2)));
    for (j = j_start; j < j_stop; j++) {
        f = 1.0 - 2.0 / (1 + exp(-pow(2.5 * (j - j_start + 0.02 * jduration) / jduration, 2)));
        g = final_volts + 0.5 * log10(f / fmax);
        g = max(g, background_volts);
        NI_waveform[BPW*(n_offset + j ) + (dat_chan % NUMCHANNELS)] = convert_func(g);
    }
}

_declspec (dllexport) double insert_ramp_red_dipole(double red_start_volts, double red_start_freq,
        double lattice_start_depth, double lattice_stop_depth,
        double t_start, double t_stop,
        int dat_chan, uInt32 *NI_waveform) {
    /* Adds a ramp waveform to a selected channel 
     *
     * Arguments:
     * red_start_volts:         initial amplitude for the ramp, in volts
     * red_start_freq:          initial red dipole frequency in Hz
     * lattice_start_depth:     initial lattice depth in recoils
     * lattice_stop_depth:      final lattice depth in recoils
     * t_start:                 Leading edge time, in milliseconds from the analog output start trigger
     * t_stop:                  Trailing edge time, in milliseconds from the analog output start trigger
     * dat_chan:                (0-7) Specifies the channel to which the ramp waveform is added */

    int j;                      /* loop index */
    int j_start = digitize_time(t_start);
    int j_stop = digitize_time(t_stop);
    double currentDepth;
    double currentVolt;
    double ratio;
    int jduration = j_stop - j_start;
	unsigned int (*convert_func)(double);

    convert_func = get_convert_func(dat_chan);

    if (j_stop > j_last) {
        j_last = j_stop;
    }

    for (j = j_start; j < j_stop; j++) {
        ratio = (double)(j - j_start)/jduration; 
        currentDepth = lattice_start_depth * pow(lattice_stop_depth / lattice_start_depth, ratio);
        currentVolt = red_start_volts + 0.5 * log10(InteractionRatio(currentDepth) + pow(BlueDeconfinement(currentDepth) / red_start_freq, 2));
	    NI_waveform[BPW*(n_offset + j) + (dat_chan % NUMCHANNELS)] = convert_func(currentVolt);
    }
    return currentVolt;
}

_declspec (dllexport) double insert_ramp_red_dipole_bkwd(double red_start_volts, double red_start_freq,
        double lattice_start_depth, double lattice_stop_depth,
        double t_start, double t_stop,
        int dat_chan, uInt32 *NI_waveform) {
    /* Adds a ramp waveform to a selected channel 
     *
     * Arguments:
     * red_start_volts:         initial amplitude for the ramp, in volts
     * red_start_freq:          initial red dipole frequency in Hz
     * lattice_start_depth:     initial lattice depth in recoils
     * lattice_stop_depth:      final lattice depth in recoils
     * t_start:                 Leading edge time, in milliseconds from the analog output start trigger
     * t_stop:                  Trailing edge time, in milliseconds from the analog output start trigger
     * dat_chan:                (0-7) Specifies the channel to which the ramp waveform is added */

    int j;                      /* loop index */
    int j_start = digitize_time(t_start);
    int j_stop = digitize_time(t_stop);
    int jduration = j_stop - j_start;
    double currentDepth = 0;
    double currentVolt = 0;
    double ratio;
	unsigned int (*convert_func)(double);

    convert_func = get_convert_func(dat_chan);

    if (j_stop > j_last) {
        j_last = j_stop;
    }

    for (j = j_start; j < j_stop; j++) {
        ratio = (double)(j_stop - j)/jduration; 
        currentDepth = lattice_start_depth * pow(lattice_stop_depth / lattice_start_depth, ratio);
        currentVolt = red_start_volts + 0.5 * log10(InteractionRatio(currentDepth) + pow(BlueDeconfinement(currentDepth) / red_start_freq, 2));
	    NI_waveform[BPW*(n_offset + j) + (dat_chan % NUMCHANNELS)] = convert_func(currentVolt);
    }
    return currentVolt;
}

_declspec (dllexport) double insert_s_red_dipole(double red_start_volts, double red_start_freq,
        double lattice_start_depth, double lattice_stop_depth,
        double t_start, double t_stop,
        int dat_chan, uInt32 *NI_waveform) {
    /* Adds a ramp waveform to a selected channel 
     * 
     * Arguments:
     * red_start_volts:         initial amplitude for the ramp, in volts
     * red_start_freq:          initial red dipole frequency in Hz
     * lattice_start_depth:     initial lattice depth in recoils
     * lattice_stop_depth:      final lattice depth in recoils
     * t_start:                 Leading edge time, in milliseconds from the analog output start trigger
     * t_stop:                  Trailing edge time, in milliseconds from the analog output start trigger
     * dat_chan:                (0-7) Specifies the channel to which the ramp waveform is added */
    int j;                      /* loop index */
    int j_start = digitize_time(t_start);
    int j_stop = digitize_time(t_stop);
    double f, fmax;
    int jduration = j_stop - j_start;
    double currentDepth = 0;
    double currentVolt = 0;
	unsigned int (*convert_func)(double);

    convert_func = get_convert_func(dat_chan);

    if (j_stop > j_last) {
        j_last = j_stop;
    }

    fmax = 1.0 - 2.0 / (1 + exp(-pow(2.5 * (1.02 * jduration) / jduration, 2)));
    for (j = j_start; j < j_stop; j++) {
        f = 1.0 - 2.0 / (1 + exp(-pow(2.5 * (j - j_start + 0.02 * jduration) / jduration, 2)));
        currentDepth = f / fmax * lattice_stop_depth;
        currentVolt = red_start_volts + 0.5 * log10(InteractionRatio(currentDepth) + pow(BlueDeconfinement(currentDepth) / red_start_freq, 2));
	    NI_waveform[BPW*(n_offset + j) + (dat_chan % NUMCHANNELS)] = convert_func(currentVolt);
    }
    return currentVolt;
}

_declspec (dllexport) void insert_fancy_smooth_ramp(double start_volts, double stop_volts,
        double alpha, double beta,
        double t_start, double t_stop,
        int dat_chan, uInt32 *NI_waveform) {
    /* Adds a smooth ramp waveform to a selected channel 
     * The ramp has zero second derivatives at the endpoints but the first derivative is 
     * alpha at the first endpoint and beta at the second.
     *
     * Arguments:
     * start_volts:         initial amplitude for the ramp, in volts
     * stop_volts:          final amplitude for the ramp, in volts
     * alpha (volts/ms)
     * beta (volts/ms)
     * t_start:                 Leading edge time, in milliseconds from the analog output start trigger
     * t_stop:                  Trailing edge time, in milliseconds from the analog output start trigger
     * dat_chan:                (0-7) Specifies the channel to which the ramp waveform is added */

    int j;                      /* loop index */
    int j_start = digitize_time(t_start);
    int j_stop = digitize_time(t_stop);
    int jduration = j_stop - j_start;
    double x, f;
    double a, b, c;
	unsigned int (*convert_func)(double);

    convert_func = get_convert_func(dat_chan);
    alpha = alpha * 1000.0 / SMP_CLK * BPW / (stop_volts - start_volts);
    beta = beta * 1000.0 / SMP_CLK * BPW / (stop_volts - start_volts);
    a = -3 * (-2 + alpha + beta);
    b = -15 + 8 * alpha + 7 * beta;
    c = -2 * (-5 + 3 * alpha + 2 * beta);
    
    if (j_stop > j_last) {
        j_last = j_stop;
    }

    for (j = j_start; j < j_stop; j++) {
        x = (double)(j - j_start) / jduration;
        f = alpha * x + c * pow(x, 3) + b * pow(x, 4) + a * pow(x, 5);
        NI_waveform[BPW*(n_offset + j ) + (dat_chan % NUMCHANNELS)] = convert_func(start_volts + (stop_volts - start_volts) * f);
    }
}

_declspec (dllexport) void insert_interpolated_ramp_using_file(char *filename,  
		double start_volts, double stop_volts, 
		double t_start, double t_stop,
		int dat_chan, uInt32 *NI_waveform) {
	/* Loads time and amplitude values of a custom ramp from a txt file, 
	   then linearly interpolates between them as needed. */

	#define numRampPts 10000

	int j, segment, prevsegment;        /* loop indices */
	int j_start = digitize_time(t_start);
	int j_stop = digitize_time(t_stop);

	double tval[numRampPts], tvalDummy[numRampPts];
	double ampval[numRampPts], ampvalDummy[numRampPts];
	FILE *fr;
	char *fr_line;
	char *tvalPointer;
	char *ampvalPointer;
	unsigned int(*convert_func)(double);

	double x, y, volt1, volt2;

	fr_line = (char *)malloc(120 * sizeof(char));
	tvalPointer = (char *)malloc(40 * sizeof(char));
	ampvalPointer = (char *)malloc(40 * sizeof(char));

	convert_func = get_convert_func(dat_chan);

	if (j_stop > j_last) {
		j_last = j_stop;
	}

	fr = fopen(filename, "rt");
	j = 0;
	if (fr == NULL) perror("Error opening file.");
	else {
		while (fgets(fr_line, 120, fr) != NULL) {
			/* get a line, up to 120 chars from fr.  done if NULL */
			if ((strncmp(fr_line, "%", 1) & strncmp(fr_line, "\n", 1)) != 0) {
				sscanf(fr_line, "%s %s %*s", &tvalPointer[0], &ampvalPointer[0]);
				tval[j] = strtod(tvalPointer, NULL);
				ampval[j] = strtod(ampvalPointer, NULL);
				j++;
			}
		}
	}

	fclose(fr);  /* close the file prior to exiting the routine */
	free(tvalPointer);
	free(ampvalPointer);
	free(fr_line);

	if (start_volts < stop_volts) {
		for (j = 0; j < numRampPts; j++) {
			tvalDummy[numRampPts - 1 - j] = 1 - tval[j];
			ampvalDummy[numRampPts - 1 - j] = ampval[j];
		}
		for (j = 0; j < numRampPts; j++) {
			tval[j] = tvalDummy[j];
			ampval[j] = ampvalDummy[j];
		}
		volt1 = stop_volts; 
		volt2 = start_volts; 
	}
	else {
		volt1 = start_volts;
		volt2 = stop_volts;
	} 

	prevsegment = 0;
	for (j = j_start; j < j_stop; j++) {

		x = (double)(j - j_start) / (j_stop - j_start);

		for (segment = prevsegment; segment < (sizeof(tval) / sizeof(tval[0]) - 1); segment++) {

			if ((tval[segment] <= x) && (tval[segment + 1] >= x))
			{
				/* Found the correct segment */
				/* Interpolate */
				y = (ampval[segment + 1] - ampval[segment]) / (tval[segment + 1] - tval[segment]) * (x - tval[segment]) + ampval[segment];
				NI_waveform[BPW*(n_offset + j) + (dat_chan % NUMCHANNELS)] = convert_func(volt2 + (volt1 - volt2) * y);
				prevsegment = segment;
				break;
			}

		}

	}

}

_declspec (dllexport) void insert_from_file(char* filename,
        double t_start, double t_stop,
        double max_current, double num_psu,
        int dat_chan, uInt32 *NI_waveform) { 
    /* Runs through a list of voltages defined by an input file. Points are 
     * assumed to be separated by 1/SMP_CLK. */

    int j;                      /* loop index */
    int j_start = digitize_time(t_start);
    int j_stop = digitize_time(t_stop);
    double temp_val;
	FILE *fr;
	unsigned int (*convert_func)(double);
    char *fr_line;
    char *val;
    fr_line = (char *)malloc(120 * sizeof(char));
    val = (char *)malloc(40 * sizeof(char));

    convert_func = get_convert_func(dat_chan);

    if (j_stop > j_last) {
        j_last = j_stop;
    }

	//printf("Parsing '%s' file.\n", filename);
	fr = fopen(filename, "rt");
    j = 0;
	if (fr == NULL) perror ("Error opening file.");
	else {
		while(fgets(fr_line, 120, fr) != NULL) {
			/* get a line, up to 80 chars from fr.  done if NULL */
			if ((strncmp(fr_line, "%", 1) & strncmp(fr_line, "\n", 1)) != 0) {
				sscanf(fr_line, "%s %*s", &val[0]);
				temp_val = strtod(val, NULL);
                temp_val = temp_val / num_psu;
                temp_val = temp_val / max_current * 5;
				if (temp_val < 0) {
					temp_val = 0;
				}
                NI_waveform[BPW*(n_offset + j_start + j) + (dat_chan % NUMCHANNELS)] = convert_func(temp_val);
				j++;
			}
		}
	}
	fclose(fr);  /* close the file prior to exiting the routine */
    free(val);
    free(fr_line);
	//printf("Done with '%s' file.\n", filename);
}

_declspec (dllexport) void insert_from_transport_file(char* filename,
        double t_start, double t_stop,
        double max_current, double num_psu,
        int dat_chan, uInt32 *NI_waveform) { 
    /* Runs through a list of voltages defined by an input file. Points are 
     * assumed to be separated by (34*70)/SMP_CLK ~ 10.5 kHz. */

    int j;                      /* loop index */
    int j_start = digitize_time(t_start);
    int j_stop = digitize_time(t_stop);
    double temp_val;
    int i;                      /* loop index */
    int k;                      /* loop index */
    double increment; 
	FILE *fr;
	char fr_line[120];
	char val[40];
	double* voltage_array;
	int nvoltages = 0;
	unsigned int (*convert_func)(double);
    clock_t begin, end;
    double time_spent;

    convert_func = get_convert_func(dat_chan);
	voltage_array = (double *)malloc(25000*sizeof(double));

    if (j_stop > j_last) {
        j_last = j_stop;
    }

    begin = clock();
	//printf("Parsing '%s' file.\n", filename);
	fr = fopen(filename, "rt");
    j = 0;
	if (fr == NULL) perror ("Error opening file.");
	else {
		while(fgets(fr_line, 120, fr) != NULL) {
			/* get a line, up to 80 chars from fr.  done if NULL */
			if ((strncmp(fr_line, "%", 1) & strncmp(fr_line, "\n", 1)) != 0) {
				sscanf(fr_line, "%s %*s", &val[0]);
				temp_val = strtod(val, NULL);
                temp_val = temp_val / num_psu;
                temp_val = temp_val / max_current * 5;
				if (temp_val < 0) {
					voltage_array[j] = 0;
				} else {
					voltage_array[j] = temp_val;
				}
                j++;
			}
		}
	}
	fclose(fr);  /* close the file prior to exiting the routine */
	//printf("Done with '%s' file.\n", filename);

	voltage_array[j] = voltage_array[j - 1];
	nvoltages = j;

	for (i = 0; i < nvoltages; i++) {
        increment = (voltage_array[i + 1] - voltage_array[i])/70;
        temp_val = voltage_array[i] - increment;
        for (k = 0; k < 70; k++) {
            temp_val = temp_val + increment;
			NI_waveform[BPW*(n_offset + j_start + i*70 + k) + (dat_chan % NUMCHANNELS)] = convert_func(temp_val);
        }
    }

    end = clock();
    time_spent = (double)(end - begin) / CLOCKS_PER_SEC;
    printf("text read time: %f\n", time_spent);
}

_declspec (dllexport) void insert_from_binary_transport_file(char* filename,
		double t_start, double t_stop,
        int dat_chan, uInt32 *NI_waveform) { 
    /* Runs through a list of voltages defined by an input file. Points are 
     * assumed to be separated by (34*linearLength)/SMP_CLK ~ 10.5 kHz. */

    int j;                      /* loop index */
    int j_start = digitize_time(t_start);
    int j_stop = digitize_time(t_stop);
    int temp_val;
	int increment; 
    int i;                      /* loop index */
    int k;                      /* loop index */
    int temp_int;
    int temp_int2;
    size_t read_result;
	FILE *fr;
	unsigned int (*convert_func)(double);
    clock_t begin, end;
    double time_spent;
    int linearLength = 35; /* 70 for 25 MHz, 35 for 12.5 MHz */


    convert_func = get_convert_func(dat_chan);

    if (j_stop > j_last) {
        j_last = j_stop;
    }

    begin = clock();
	//printf("Parsing '%s' file.\n", filename);
	fr = fopen(filename, "rb");
    read_result = fread(&temp_int, sizeof(uInt32), 1, fr);
    read_result = fread(&temp_int2, sizeof(uInt32), 1, fr);
    j = 0;
	if (fr == NULL) perror ("Error opening file.");
	else {
		while (read_result == 1) {
            increment = (int) round(((double)(temp_int2 - temp_int))/(double) linearLength);
            temp_val = temp_int - increment;
            for (k = 0; k < linearLength; k++) {
                temp_val = temp_val + increment;
		    	NI_waveform[BPW*(n_offset + j_start + j*linearLength + k) + (dat_chan % NUMCHANNELS)] = (unsigned int) temp_val;
            }
            temp_int = temp_int2;
            read_result = fread(&temp_int2, sizeof(uInt32), 1, fr);
			j++;
		}
	}
	fclose(fr);  /* close the file prior to exiting the routine */
	//printf("Done with '%s' file.\n", filename);

    end = clock();
    time_spent = (double)(end - begin) / CLOCKS_PER_SEC;
    /*printf("binary read time: %f\n", time_spent);*/
}

void initialize_NI_waveform(int card_number, uInt32 *NI_waveform) {
    int i;                      /* loop index */
	uInt32 *CSblock;
	unsigned int (*convert_func)(double);
    clock_t begin, end;
    double time_spent;

    /* Note, this is a subroutine that is (currently) only inside of 
     * 'initialize_data'. At this point in the typical sequence, the 
     * global variable 'tot_words' is equivalent to 'max_words'. */

    /* Initialize 'CSblock' */
	CSblock = (uInt32 *)malloc(2 * sizeof(*CSblock));
	memset(&CSblock[0], 0, 2 * sizeof(uInt32));
    /* 'CSblock' consists of 2 bytes which represent the 2 clock cycles
     * that occur between each 32-clock-cycle wide section of actual
     * data. During this CSblock, the CS lines need to go high (during
     * the actual data, CS is low). Additionally, we set 'SYNCBAR' and
     * 'RESETBAR' high to keep the AD9522 in normal operation. */
	CSblock[0] = CSblock[0] | (1 << (31 - ADCS));
	CSblock[0] = CSblock[0] | (1 << (31 - RESETBAR));
	CSblock[0] = CSblock[0] | (1 << (31 - CSTop));
	CSblock[0] = CSblock[0] | (1 << (31 - CSBot));
	CSblock[0] = CSblock[0] | (1 << (31 - SYNCBAR));

	CSblock[1] = CSblock[1] | (1 << (31 - ADCS));
	CSblock[1] = CSblock[1] | (1 << (31 - RESETBAR));
	CSblock[1] = CSblock[1] | (1 << (31 - CSTop));
	CSblock[1] = CSblock[1] | (1 << (31 - CSBot));
	CSblock[1] = CSblock[1] | (1 << (31 - SYNCBAR));

    /* the 'REFIN' line should always be cycling between high and low, 
     * so that the PLL remains locked. So, we set it high for the first
     * part of CSblock. */
	CSblock[0] = CSblock[0] | (1 << (31 - REFIN));

    /* for the AD9959 DDS eval board, we need the SYNC_IO line to be high by
     * default to prevent the board from being programmed. In principle, the
     * SYNC_IO line doesn't need to be set high in 'CSblock' because the CS
     * line is set high. However, better safe than sorry. */
    for (i = 0; i < NUMCHANNELS; i++) {
        if ((validNIchannel & (1 << i)) > 0) {
            if (devtype[(card_number - 1)*NUMCHANNELS + i] == 7) {
                CSblock[0] = CSblock[0] | (1 << (31 - i));
                CSblock[1] = CSblock[1] | (1 << (31 - i));
		        printf("dds sync channel: %u \n", i);
		        printf("card number: %u \n", card_number);
            }
        }
    }
	#ifdef DEBUGCS
		printf("CSblock[0] = %u, CSblock[1] = %u.\n", CSblock[0], CSblock[1]);
	#endif

    begin = clock();
    /* raise appropriate lines and insert the clock for the first word */
    memset(&NI_waveform[0], 0, BPW * sizeof(uInt32));
    NI_waveform[REFIN] = 0xAAAAAAAA;
    NI_waveform[RESETBAR] = 0xFFFFFFFF;
    NI_waveform[SYNCBAR] = 0xFFFFFFFF;
    NI_waveform[BPW - 2] = CSblock[0];
    NI_waveform[BPW - 1] = CSblock[1];
    for (i = 0; i < NUMCHANNELS; i++) {
		/* Initialize all the channels to "0 volts." I use quotations because for the PLLs
		 * we simply add something that doesn't effect the already programmed frequency */
        if ((validNIchannel & (1 << i)) > 0) {
			convert_func = get_convert_func((card_number - 1)*NUMCHANNELS + i);
			//printf("chan = %d, \t dev type = %d\n", i, devtype[(card_number - 1)*NUMCHANNELS + i]);
            NI_waveform[i] = convert_func(0);
        }
    }
    for (i = 0; i < tot_words - 1; i++) {
        /* Now copy the first word into every word, i.e. 'i = 0' was set in
         * the previous lines, now copy what's in 'i = 0' for all 'i' up to
         * 'tot_words.' */
        memcpy(&NI_waveform[(i + 1)*BPW], &NI_waveform[i*BPW], BPW*sizeof(uInt32));
    }
	//printf("%d\n", card_number);
	free(CSblock);

    end = clock();
    time_spent = (double)(end - begin) / CLOCKS_PER_SEC;
    printf("initializing NI_waveform: %f seconds\n", time_spent);
}

int parse_dev_types(char *dev, const char* filedir) {
    int i;  /* loop index */
    int card_number;
	//char *device = "ved2";
	char device[5] = {dev[1], dev[2], dev[3], dev[4],'\0'};

	const char *ext = "_device_types.txt";
	char *devtypefile;
	FILE *fr;
    char *fr_line;
    char *chan;
    char *type;

    fr_line = (char *)malloc(120 * sizeof(char));
    devtypefile = (char *)malloc(300 * sizeof(char));
    chan = (char *)malloc(40 * sizeof(char));
    type = (char *)malloc(40 * sizeof(char));

	/* channel device types */
	/* for (i = 0; i < 4; i++) {
		device[i] = dev[i + 1];
	} */

    card_number = atoi(&device[3]);
	/* printf("device = %s \n", device);
     * printf("card_number = %d \n", card_number); */
	strcpy(devtypefile, filedir);
	strcat(devtypefile, device);
    strcat(devtypefile, ext);
	//printf("devtypefile: '%s' file.\n", devtypefile);
	memset(&devtype[(int)NUMCHANNELS*(card_number - 1)], 0, NUMCHANNELS * sizeof(unsigned int));
	fr = fopen(devtypefile, "rt");
	i = 0;
	if (fr == NULL) perror ("Error opening *_device_types.txt");
	else {
		while(fgets(fr_line, 120, fr) != NULL) {
	 		/* get a line, up to 80 chars from fr.  done if NULL */
	 		if ((strncmp(fr_line, "%", 1) & strncmp(fr_line, "\n", 1)) != 0) {
	 			sscanf(fr_line, "%s %s %*s", &chan[0], &type[0]);
	 			devtype[strtol(chan, NULL, 0) + (card_number - 1)*NUMCHANNELS] = strtol(type, NULL, 0);
	 			//printf("chan, device pair found is %s, %s\n", chan, type);
	 			i++;
	 		}
	 	}
	 }
	fclose(fr);  /* close the file prior to exiting the routine */
	//printf("Done with '%s_device_types.txt' file.\n", device);
    
    free(fr_line);
    free(chan);
    free(type);

    return card_number; 
}

void parse_reg_edits(const char* ad_registry, unsigned int **AD_reg_edits) {
    int i;
	int nedits = NEDITS;
    int reg_width = 2;
	FILE *fr;
    char *fr_line;
    char *addr;
    char *val;
    fr_line = (char *)malloc(120 * sizeof(char));
    addr = (char *)malloc(40 * sizeof(char));
    val = (char *)malloc(40 * sizeof(char));

    /* Registry Modifications */
    /* Parse 'reg_edits.txt' file to find the appropriate registry (address,
     * value) data. */
    for (i = 0; i < nedits; i++) {
        memset(AD_reg_edits[i], 0, reg_width * sizeof(unsigned int));
    }
	//printf("Parsing 'reg_edits.txt' file.\n");
	fr = fopen(ad_registry, "rt");
	i = 0;
	if (fr == NULL) perror ("Error opening reg_edits.txt");
	else {
		while(fgets(fr_line, 120, fr) != NULL) {
			/* get a line, up to 80 chars from fr. done if NULL */
			if ((strncmp(fr_line, "%", 1) & strncmp(fr_line, "\n", 1)) != 0) {
				sscanf(fr_line, "%s %s %*s", &addr[0], &val[0]);
				AD_reg_edits[i][0] = strtol(addr, NULL, 0);
				AD_reg_edits[i][1] = strtol(val, NULL, 0);
				i++;
			}
		}
	}
	fclose(fr);  /* close the file prior to exiting the routine */
	//printf("Done with 'reg_edits.txt' file.\n");

    #ifdef DEBUG_REG_EDITS
	for (i = 0; i < 50; i++) {
		printf("(%x, %d)\n", AD_reg_edits[i][0], AD_reg_edits[i][1]);
	}
    #endif
    free(fr_line);
    free(addr);
    free(val);
}

void insert_reg_edits(uInt32 *NI_waveform, unsigned int **AD_reg_edits, int nedits) {
    int i;                      /* loop index */
    int j;
	int j_start = 2*(nedits + 1);
    int j_stop = 2*nedits + NUMVCOCALCYCLES;
	int j_times[2] = {j_start, j_stop - 9};
    //unsigned int disable_clk_edits[5][2];
    
    for (i = 0; i < nedits; i++) {
        /* do not need to check whether address value (which is encoded in
         * 'program_AD_sequence[i][0]') is 0 because the loop only goes
         * through the first 'nedits' number of rows in the
         * 'program_AD_sequence' */
        interleave_zero(&(AD_reg_edits[i][0]));
		interleave_zero(&(AD_reg_edits[i][1]));
        NI_waveform[BPW*(2*i) + ADDAT] = AD_reg_edits[i][0];
        NI_waveform[BPW*(2*i + 1) + ADDAT] = AD_reg_edits[i][1];

        /* Insert clock: full clock required for 16-bit instruction byte. For
         * the 8-bit value byte, only need clocks in the second half of the
         * digital world */
        /* 0x5 = 0101 */
        NI_waveform[BPW*(2*i) + ADCLK] = 0x55555555;
        NI_waveform[BPW*(2*i + 1) + ADCLK] = 0x00005555;

        /* Insert chip select: chip select drops low only for the instruction
         * and value byte, otherwise it is set high. */
		//printf("loop iteration i = %d.\n", i);
    }
	//NI_waveform[BPW*(2*(i - 1) + ) + ADCS] = 0xFFFFFFFF;

	for (j = 0; j < 4; j++) {
		AD_reg_edits[j][0] = 0x191 + j*3;
		AD_reg_edits[j][1] = 0x70;
	}
	AD_reg_edits[4][0] = 0x232;
	AD_reg_edits[4][1] = 0x1;
	
	for (i = 0; i < 2; i++) {
		for (j = 0; j < 5; j++) {
			interleave_zero(&(AD_reg_edits[j][0]));
			interleave_zero(&(AD_reg_edits[j][1]));
			NI_waveform[BPW*(j_times[i] + 2*j) + ADDAT] = AD_reg_edits[j][0];
			NI_waveform[BPW*(j_times[i] + 2*j + 1) + ADDAT] = AD_reg_edits[j][1];
		
			/* Insert clock: full clock required for 16-bit instruction byte. For
				* the 8-bit value byte, only need clocks in the second half of the
				* digital world */
			/* 0x5 = 0101 */
			NI_waveform[BPW*(j_times[i] + 2*j) + ADCLK] = 0x55555555;
			NI_waveform[BPW*(j_times[i] + 2*j + 1) + ADCLK] = 0x00005555;
		}

		/* reset 'AD_reg_edits' with powered on values */
		for (j = 0; j < 4; j++) {
			AD_reg_edits[j][0] = 0x191 + j*3;
			AD_reg_edits[j][1] = 0;
		}
		AD_reg_edits[4][0] = 0x232;
		AD_reg_edits[4][1] = 0x1; 
	}
}

void raise_line(uInt32 NI_waveform[], int chan) {
    int i;                      /* loop index */
	    for (i = 0; i < tot_words; i++) {
        /* 0x8 = 1000 and 0x1 = 0001 */
        //NI_waveform[NUMCHANNELS*i + chan] = 0xFFFFFFFF;
		memset(&NI_waveform[BPW*i + chan], 0xFF, sizeof(uInt32));
    }
}

void insert_clock(uInt32 *NI_waveform) {
    int i;                      /* loop index */
    for (i = 0; i < tot_words; i++) {
        /* 0x5 = 0101 */
        NI_waveform[BPW*i + REFIN] = 0xAAAAAAAA;
		//memset(&NI_waveform[BPW*i + REFIN], 0xAA, sizeof(uInt32));
    }
}

unsigned int AD_convert(double voltage) {
    unsigned int dig_volt;

    dig_volt = (unsigned int)((1 << 18)*fabs(voltage)/5);
    return dig_volt;
}

unsigned int pll_convert(double voltage) {
    unsigned int dig_volt;

	//printf("I'm here in pll_convert\r\n");
	/* this is a bit of a hack, but 'pll_convert' only used in one place */
    dig_volt = (unsigned int) (2                /* control bits = 2 = 0b10 */
        + (0 << 2)                /* counter operation normal */
        + (0 << 3)                /* powerdown 1 = normal operation */
        + (1 << 4)                /* MUXOUT displays Digital Lock Detect */
        + (1 << 7)                /* phase detector polarity = positive */
        + (0 << 8)                /* Charge pump output normal (not tri-state) */
        + (0 << 9)                /* Fastlock disabled */
        + (0 << 11)               /* timeout = 3 PFD cycles */
        + (3 << 15)               /* CP1 current = 2.5 mA */
        + (3 << 18)               /* CP2 current = 2.5 mA */
        + (0 << 21));              /* powerdown 2 = normal operation */
    return dig_volt;
}

unsigned int dds_data_convert(double voltage) {
    unsigned int dig_volt;

    /* read from FR1. first byte is a read instruction. in the last 3 bytes,
     * data from the FR1 register is clocked out */
    dig_volt = (unsigned int) (((1 << 7) << 24)
        + (1 << 24));
    return dig_volt;
}

unsigned int dds_sync_convert(double voltage) {
    unsigned int dig_volt;

    /* make sure the line sync_io line is set high */
    dig_volt = (unsigned int) (0xFFFFFFFF);
    return dig_volt;
}

unsigned int AD_convert_bipolar(double voltage) {
    unsigned int dig_volt;

    dig_volt = (unsigned int)((1 << 18)*(voltage + 5)/10);
	return dig_volt;
}

unsigned int AD_convert_bipolar_doubled(double voltage) {
    unsigned int dig_volt;

    dig_volt = (unsigned int)((1 << 18)*(voltage + 10)/20);
    return dig_volt;
}

unsigned int (*get_convert_func(unsigned int dat_chan))(double)
{
	/* printf("Getting convert_func(0) for dat_chan = %d.\r\n", dat_chan); */
    if (devtype[dat_chan] == 0) {
        return &AD_convert;
    } else if (devtype[dat_chan] == 1) {
        return &AD_convert_bipolar;
    } else if (devtype[dat_chan] == 2) {
        return &pll_convert;
    } else if (devtype[dat_chan] == 3) {
        return &AD_convert_bipolar_doubled;
    } else if (devtype[dat_chan] == 6) {
        return &dds_data_convert;
    } else if (devtype[dat_chan] == 7) {
        return &dds_sync_convert;
    } else {
        return &AD_convert;
    }
}

double DA_convert(unsigned int dig_volt) {
    double voltage;

    voltage = dig_volt/(1 << 18) * 5;
    return voltage;
}

int digitize_time(double real_time) {
    int digital_time;

    digital_time = (int)round(real_time*SMP_CLK/BPW/1000);
    return digital_time;
}

double tunnel_from_depth(double d) {
    /* Numerical expression for tunnelling rate in recoils as a function of lattice 
     * depth from band structure calculation, good between 3 and 40 recoils.
     * Calulation saved in Z:\Experiment Software Backup\ExpControl\mathematica */
    double t = 0.000806452 * (80.85 * exp(-d / 5.822) + 0.63815 * exp(-d / 15.349) + 248.836 * exp(-d / 3.0066));
    return t;
}

double volt_from_tunnel(double t) {
    /* Numerical expression for voltage needed for desired tunnelling rate. 
     * Good between 10 and 30 recoils. 
     * Calulation saved in Z:\Experiment Software Backup\ExpControl\mathematica */
    double v = 0.49064 + 0.3304 * exp(-t / 1.7646) + 0.77322 * exp(-t / 60.384) + 0.27196 * exp(-t / 0.03982);
    return v;
}

double volt_from_tunnel_shallow(double t) {
    /* Fit for shallow lattices!
     * Numerical expression for voltage needed for desired tunnelling rate. Good 
     * from 3 to 10 recoils.
     * Calulation saved in Z:\Experiment Software Backup\ExpControl\mathematica */
    double v = 2 * pow(10, -0.19378 - 0.00584 * t + 7.1696 * pow(10, -5) * pow(t, 2) - 7.2492 * pow(10, -7) * pow(t, 3) + 3.5622 * pow(10, -9) * pow(t, 4) - 7.3566 * pow(10, -12) * pow(t, 5));
    return v;
}

double depth_from_tunnel(double t) {
    /* Fit for lattices < 30 Er.
     * Numerical expression for lattice depth needed for desired tunnelling rate. 
     * Good from 2 to 30 recoils. Tunneling and lattice depth in Er. */
    double d = -0.70255 + 14.1054 * exp(-t / 0.0000553825) + 10.497 * exp(-t / 0.108124) + 10.6318 * exp(-t / 0.00203487) + 9.01271 * exp(-t / 0.0124361) + 12.449 * exp(-t / 0.000336428);
    return d;
}

double depth_from_tunnel_calibrated(double a[11], double t) {
    /* (2024/02/15) updated version of depth_from_tunnel that takes a matrix of coefficients
     * derived by fitting the sum of exponentials to numerical data from the latest lattice depth calibration
     * Numerical expression for lattice depth needed for desired tunnelling rate.
     * Good from 2 to 30 recoils. Tunneling and lattice depth in Er. */
    double d = a[0] + a[1] * exp(-t / a[2]) + a[3] * exp(-t / a[4]) + a[5] * exp(-t / a[6]) + a[7] * exp(-t / a[8]) + a[9] * exp(-t / a[10]);
    return d;
}

double gauge_volt_from_tunnel_calibrated(double a[7], double t) {
    /* (2024/02/15) updated version of depth_from_tunnel that takes a matrix of coefficients
     * derived by fitting the sum of exponentials to numerical data from the latest gauge voltage to depth calibration. */

    double d = a[0] + a[1] * exp(-t * a[2]) + a[3] * exp(-t * a[4]) + a[5] * exp(-t * a[6]);
    return d;
}

double gauge_depth_from_tunnel_calibrated(double a[4], double t) {
    /* (2024/02/15) updated version of depth_from_tunnel that takes a matrix of coefficients
     * derived by fitting the sum of exponentials to numerical data from the latest gauge voltage to depth calibration. */

    double d = a[0] * t + a[1] * pow(t, 2) + a[2] * pow(t, 3) + a[3] * pow(t, 4);
    return d;
}

double axialdepth_from_2d_depth(double v) {
    /* Numerical expression for axial depth needed (in axial recoils) for a given 
     * 2D depth (in 2d recoils) to keep tunneling between planes and within planes 
     * the same.
     * Calulation saved in Z:\Experiment Software Backup\ExpControl\mathematica */
    double d = 8.207 + 1.201 * v; /* may 04 2012 */
    return d;
}

double mod_start_voltage(double offset_depth, double rel_amp, double phase, double calib_volt, double slope) {
    double w = tunnel_from_depth(offset_depth);
    double u = w * (1 + rel_amp * sin(phase * PI));
    double x = depth_from_tunnel(u);
    double v = calib_volt + slope * (log10(x) - log10(offset_depth));
    return v;
}

double mod_end_voltage(double offset_depth, double rel_amp, double phase, double freq_Hz, double duration, double calib_volt, double slope) {
    double w = tunnel_from_depth(offset_depth);
    double u = w * (1 + rel_amp * sin(phase * PI + 2 * PI * freq_Hz * duration / 1000));
    double x = depth_from_tunnel(u);
    double v = calib_volt + slope * (log10(x) - log10(offset_depth));
    return v;
}

double InteractionRatio(double v) {
    /* See pg 26-27 of lab book 7 for documentation on this function.
     * 
     * Arguments:
     * v:           lattice depth in recoils */
    double f = 1 + 0.24256 * pow(v, 1) + 0.0128075 * pow(v, 2) - 0.00108671 * pow(v, 3) + 0.0000304067 * pow(v, 4) - 0.000000297633 * pow(v, 5);
    return f;
}

double BlueDeconfinement(double v) {
    /* See pg 26-27 of lab book 7 for documentation on this function.
     * 
     * Arguments:
     * v:           lattice depth in recoils */

    double g = 6.9397 * pow(v, 0.5) - 1.41963 * pow(v, 1) + 0.034345 * pow(v, 2) - 0.000601338 * pow(v, 3) + 0.00000424209 * pow(v, 4);
    return g;
}

void interleave_zero(unsigned int *x) {
    /* trick from Hacker's Delight */
    *x = ((*x & 0xFF00) << 8) | (*x & 0x00FF);
    *x = ((*x << 4) | *x) & 0x0F0F0F0F;
    *x = ((*x << 2) | *x) & 0x33333333;
    *x = ((*x << 1) | *x) & 0x55555555;
    *x = 3*(*x);
}

/* Straight-line version of transpose32a & b. */

void transpose32c(uInt32 A[32], uInt32 B[32]) {
   unsigned m, t;
   unsigned a0, a1, a2, a3, a4, a5, a6, a7,
            a8, a9, a10, a11, a12, a13, a14, a15,
            a16, a17, a18, a19, a20, a21, a22, a23,
            a24, a25, a26, a27, a28, a29, a30, a31;

   a0  = A[ 0];  a1  = A[ 1];  a2  = A[ 2];  a3  = A[ 3];
   a4  = A[ 4];  a5  = A[ 5];  a6  = A[ 6];  a7  = A[ 7];
   a8  = A[ 8];  a9  = A[ 9];  a10 = A[10];  a11 = A[11];
   a12 = A[12];  a13 = A[13];  a14 = A[14];  a15 = A[15];
   a16 = A[16];  a17 = A[17];  a18 = A[18];  a19 = A[19];
   a20 = A[20];  a21 = A[21];  a22 = A[22];  a23 = A[23];
   a24 = A[24];  a25 = A[25];  a26 = A[26];  a27 = A[27];
   a28 = A[28];  a29 = A[29];  a30 = A[30];  a31 = A[31];

   m = 0x0000FFFF;
   swap(a0,  a16, 16, m)
   swap(a1,  a17, 16, m)
   swap(a2,  a18, 16, m)
   swap(a3,  a19, 16, m)
   swap(a4,  a20, 16, m)
   swap(a5,  a21, 16, m)
   swap(a6,  a22, 16, m)
   swap(a7,  a23, 16, m)
   swap(a8,  a24, 16, m)
   swap(a9,  a25, 16, m)
   swap(a10, a26, 16, m)
   swap(a11, a27, 16, m)
   swap(a12, a28, 16, m)
   swap(a13, a29, 16, m)
   swap(a14, a30, 16, m)
   swap(a15, a31, 16, m)
   m = 0x00FF00FF;
   swap(a0,  a8,   8, m)
   swap(a1,  a9,   8, m)
   swap(a2,  a10,  8, m)
   swap(a3,  a11,  8, m)
   swap(a4,  a12,  8, m)
   swap(a5,  a13,  8, m)
   swap(a6,  a14,  8, m)
   swap(a7,  a15,  8, m)
   swap(a16, a24,  8, m)
   swap(a17, a25,  8, m)
   swap(a18, a26,  8, m)
   swap(a19, a27,  8, m)
   swap(a20, a28,  8, m)
   swap(a21, a29,  8, m)
   swap(a22, a30,  8, m)
   swap(a23, a31,  8, m)
   m = 0x0F0F0F0F;
   swap(a0,  a4,   4, m)
   swap(a1,  a5,   4, m)
   swap(a2,  a6,   4, m)
   swap(a3,  a7,   4, m)
   swap(a8,  a12,  4, m)
   swap(a9,  a13,  4, m)
   swap(a10, a14,  4, m)
   swap(a11, a15,  4, m)
   swap(a16, a20,  4, m)
   swap(a17, a21,  4, m)
   swap(a18, a22,  4, m)
   swap(a19, a23,  4, m)
   swap(a24, a28,  4, m)
   swap(a25, a29,  4, m)
   swap(a26, a30,  4, m)
   swap(a27, a31,  4, m)
   m = 0x33333333;
   swap(a0,  a2,   2, m)
   swap(a1,  a3,   2, m)
   swap(a4,  a6,   2, m)
   swap(a5,  a7,   2, m)
   swap(a8,  a10,  2, m)
   swap(a9,  a11,  2, m)
   swap(a12, a14,  2, m)
   swap(a13, a15,  2, m)
   swap(a16, a18,  2, m)
   swap(a17, a19,  2, m)
   swap(a20, a22,  2, m)
   swap(a21, a23,  2, m)
   swap(a24, a26,  2, m)
   swap(a25, a27,  2, m)
   swap(a28, a30,  2, m)
   swap(a29, a31,  2, m)
   m = 0x55555555;
   swap(a0,  a1,   1, m)
   swap(a2,  a3,   1, m)
   swap(a4,  a5,   1, m)
   swap(a6,  a7,   1, m)
   swap(a8,  a9,   1, m)
   swap(a10, a11,  1, m)
   swap(a12, a13,  1, m)
   swap(a14, a15,  1, m)
   swap(a16, a17,  1, m)
   swap(a18, a19,  1, m)
   swap(a20, a21,  1, m)
   swap(a22, a23,  1, m)
   swap(a24, a25,  1, m)
   swap(a26, a27,  1, m)
   swap(a28, a29,  1, m)
   swap(a30, a31,  1, m)

   B[ 0] = a0;   B[ 1] = a1;   B[ 2] = a2;   B[ 3] = a3;
   B[ 4] = a4;   B[ 5] = a5;   B[ 6] = a6;   B[ 7] = a7;
   B[ 8] = a8;   B[ 9] = a9;   B[10] = a10;  B[11] = a11;
   B[12] = a12;  B[13] = a13;  B[14] = a14;  B[15] = a15;
   B[16] = a16;  B[17] = a17;  B[18] = a18;  B[19] = a19;
   B[20] = a20;  B[21] = a21;  B[22] = a22;  B[23] = a23;
   B[24] = a24;  B[25] = a25;  B[26] = a26;  B[27] = a27;
   B[28] = a28;  B[29] = a29;  B[30] = a30;  B[31] = a31;
}

void print_bit_matrix(uInt32 array[], int sample_number) {
    int i;
    int j;
    /* Advance through array in increasing order. Currently, the array is
     * arranged as (32-samples of chan 0, 32 samples of chan 1, 32-samples of
     * chan 2, etc.) Hence, an increment in 'i' (which is graphically shown by
     * incrementing the row) is an increment in the channel number. */
    for (i = 0; i < NUMCHANNELS; i++) {
        if (i == 0) {
            /* Bit decompose each row in MSB first fashion */
            for (j = 31; j > -1; j--) {
                if (j == 31) {
                    printf("Chan/Bit \t");
                }

                printf("D%02i ", j);

            }
            printf("\n==============================================================================================================================================\n");
        }
        for (j = 31; j > -1; j--) {
            if (j == 31) {
                printf("chan %02u \t", i);
            }

            printf("%d   ", (array[sample_number*NUMCHANNELS + i] & (1 << j)) >= 1);          
        }
		printf("\n");
	}
}

void print_bit_matrixT(uInt32 array[], int sample_number) {
    int i;
    int j;
    /* Advance through array in increasing order. Following the transpose, the
     * array should be arranged as (32-values for time 0, 32-values for time
     * 1,  32-values for time 2,  32-values for time 3, etc.) Hence, an
     * increment in 'i' (which is graphically shown by incrementing the row)
     * is a step through time. Since the DAC is MSB first, hopefully time 0
     * corresponds to MSB.*/
    for (i = 0; i < 32; i++) {
        if (i == 0) {
            /* the bit numbers are calculated LSB -> MSB. The channels are
             * also LSB -> MSB (DIO0 = LSB), then we count up in labelling the
             * channels */
            for (j = 0; j < NUMCHANNELS; j++) {
                if (j == 0) {
                    printf("Bit/Chan \t");
                }

                printf("c%02i ", j);
				}
            printf("\n==============================================================================================================================================\n");
        }

        for (j = 0; j < NUMCHANNELS; j++)  {
            if (j == 0) {
                printf("t%02u \t\t", i);
            }

            printf("%d   ", (array[sample_number*NUMCHANNELS + i] & (1 << j)) >= 1);
        }
		printf("\n");
    }
}

double round(double val) {    
    return floor(val + 0.5);
}
