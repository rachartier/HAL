#include <stdio.h>
#include <stdlib.h>
#include <dlfcn.h>

#ifdef UNIX 
# define EXPORT extern
#elif (defined (_WINDOWS))
# define EXPORT extern __declspec( dllexport )
#endif

EXPORT int read_shareobject() {
	return 32;
	/*	int (*extrun)(void);

			if(input_file == NULL)
			return NULL;

			void *lib = dlopen(input_file, RTLD_LAZY);

			if(lib != NULL) {
			extrun = dlsym(lib, "run");
			return extrun();
			}

			return NULL;*/
}
