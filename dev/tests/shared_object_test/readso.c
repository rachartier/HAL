#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <assert.h>

#if __linux__
# include <dlfcn.h>
# define EXPORT
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

	static char* _cpy_mem_string(char* data, size_t len) {
		char* string = NULL;
		
		if (data != NULL) {
			string = malloc(len * sizeof(*string));

			if (string != NULL) {
				memcpy(string, data, len * sizeof(*string));
				return string;
			}
		}

		return NULL;
	}



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
			char* dll_result = NULL;

#if __linux__
			extrun = dlsym(lib, "run");
			dll_result = extrun();

			char* result = _cpy_mem_string(dll_result, strlen(dll_result) + 1);
			
			dlclose(lib);

			return result;
#else
			extrun = (dll_function)GetProcAddress(lib, "run");

			if (extrun) {
				dll_result = extrun();
				FreeLibrary(lib);

				return  _cpy_mem_string(dll_result, strlen(dll_result) + 1);;
			}
#endif
		}

		return NULL;
	}

#ifdef __cplusplus
}
#endif

