#define ADCS 16			/* 31 - 15 = 16 */
#define ADCLK 14		/* 31 - 17 = 14 */
#define ADDAT 12		/* 31 - 19 = 12 */
#define RESETBAR 10		/* 31 - 21 = 10 */
#define CSTop 30		/* 31- 1 = 30 */
#define CSBot 0			/* 31 - 31 = 0 */
#define REFIN 20		/* 31 - 11 = 20 */	
#define SYNCBAR 18		/* 31 - 13 = 18 */
/* channel redefinitions: from top to bottom, left to right */
/* these aren't actually used ever in the DLL, but are here for reference purposes... */
#define DIO18 13		/* 31 - 18 = 13 */
#define DIO16 15		/* 31 - 16 = 15 */
#define DIO22 9		    /* 31 - 22 = 9 */
#define DIO20 11		/* 31 - 20 = 11 */
#define DIO26 5		    /* 31 - 26 = 5 */
#define DIO24 7		    /* 31 - 24 = 7 */
#define DIO30 1			/* 31 - 30 = 1 */
#define DIO28 3			/* 31 - 28 = 3 */
#define DIO27 4			/* 31 - 27 = 4 */
#define DIO29 2			/* 31 - 29 = 2 */
#define DIO23 8			/* 31 - 23 = 8 */
#define DIO25 6			/* 31 - 25 = 6 */
#define DIO07 24		/* 31 - 7 = 24 */
#define DIO09 22		/* 31 - 9 = 22 */
#define DIO03 28		/* 31 - 3 = 28 */
#define DIO05 26		/* 31 - 5 = 26 */
#define DIO02 29		/* 31 - 2 = 29 */
#define DIO00 31		/* 31 - 0 = 31 */
#define DIO06 25		/* 31 - 6 = 25 */
#define DIO04 27		/* 31 - 4 = 27 */
#define DIO10 21		/* 31 - 10 = 21 */
#define DIO08 23		/* 31 - 8 = 23 */
#define DIO14 17		/* 31 - 14 = 17 */
#define DIO12 19		/* 31 - 12 = 19 */
/* #define SMP_CLK 25000000.0 */
#define SMP_CLK 12500000.0
/* 32 bits per word, but then 2 extra bits for CSblock */
#define BPW 34
#define NUMCHANNELS 32
/* longer calibration is needed because I was moronic in making the PLL loop filter */
#define NUMVCOCALCYCLES 3000		/* 4450/32 * 2 */
#define PFDFREQ 0.1		/* 100 kHz */
#define REFCLKFREQ 10		/* 10 MHz */
#define SYSCLK 400
#define PI 3.141592653589793 
#define EE 2.71828182846
#define NEDITS 50
#define DEVTYPEFILEDIR "C:\\Users\\Rb Lab\\Documents\\ExpControl\\dynacode\\variables\\"
#define REGEDITSFILE "C:\\Users\\Rb Lab\\Documents\\NI_653x_routines\\reg_edits.txt" 
#define REGEDITSFILE "C:\\Users\\Rb Lab\\Documents\\NI_653x_routines\\reg_edits_slow.txt"
