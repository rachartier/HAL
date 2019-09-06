#include <stdio.h>
#include <stdlib.h>
#include <dlfcn.h>

extern char *run_entrypoint_sharedobject(char *input_file) {
	char *(*extrun)(void);

	if(input_file == NULL)
		return NULL;

	void *lib = dlopen(input_file, RTLD_LAZY);

	if(lib != NULL) {
		extrun = dlsym(lib, "run");
		return extrun();
	}

	return NULL;
}
