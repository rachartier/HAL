#include <stdio.h>
#include <stdlib.h>
#include <time.h>

char *run() {
	char* 			date = calloc(64, sizeof(char));
	time_t 	    rawtime = time(NULL);
	struct tm*  tm = localtime(&rawtime);

	sprintf(date, "%.4d-%.2d-%.2dT%.2d:%.2d:%.2d",
			1900+tm->tm_year,
			tm->tm_mon,
			tm->tm_mday,
			tm->tm_hour,
			tm->tm_min,
			tm->tm_sec
			);

	return date;
}
