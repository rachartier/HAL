#include <stdio.h>
#include <stdlib.h>

#if __linux__
# include <dlfcn.h>
# define EXPORT extern
#else
# include <windows.h>
# include <winbase.h>
# include <windef.h>
# define EXPORT __declspec(dllexport)
#endif

#ifdef __cplusplus
extern "C" {
#endif

	typedef char* (*dll_function)(void);

	char* EXPORT run_entrypoint_sharedobject(char* input_file) {
		dll_function extrun;

		if (input_file == NULL)
			return NULL;

		void* lib = NULL;

#if __linux__
		lib = dlopen(input_file, RTLD_LAZY);
#else 
		lib = LoadLibrary(input_file);
#endif

		if (lib != NULL) {

#if __linux__
			extrun = dlsym(lib, "run");
			return extrun();
#else
			extrun = (dll_function)GetProcAddress(lib, "run");

			if (extrun) {
				char* dll_result = extrun();

				// we need to allocate memory otherwise c#'s marshal can't process it. 
				char* ret = malloc(strlen(dll_result) + 1);
				strcpy(ret, dll_result);

				FreeLibrary(lib);

				return ret;
			}
#endif
		}

		return NULL;
	}

#ifdef __cplusplus
}
#endif

