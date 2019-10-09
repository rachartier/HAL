#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#ifdef __linux__
# define EXPORT 
#else
# define EXPORT __depdecl(dllexport)
#endif

#define BLOCK_SIZE 1024

char* EXPORT launch_command(const char *command) {
	if(!command) return NULL;

	FILE* fp = NULL;
	fp = popen(command, "r");

	if(!fp) return NULL;

	size_t 	block_read = 0U;
	size_t 	data_size = 0U;

	char*		data_result = malloc((BLOCK_SIZE + 1) * sizeof(*data_result));

	if(!data_result) return NULL;

	while(!feof(fp)) {
		size_t block_start = data_size;
		size_t byte_read = fread(data_result + block_start, sizeof(char), BLOCK_SIZE, fp);

		data_size += byte_read;

		if(data_size >= BLOCK_SIZE * (block_read + 1)) {
			++block_read;

			char* resized_data = realloc(data_result, ((block_read + 1) * BLOCK_SIZE) * sizeof(*data_result));

			if(!resized_data) {
				free(data_result);
				return NULL;
			}

			data_result = resized_data;
		}
	}

	data_result[data_size] = '\0';

	return data_result;
}
