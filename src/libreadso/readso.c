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

// the code need to be executed as C
#ifdef __cplusplus
extern "C" {
#endif

	// a pointer to function to call the dll entry point
	typedef char* (*dll_function)(void);

	// copy a string into a new one
	static char* _cpy_mem_string(char* data, size_t len) {
		char* string = NULL;

		if (data != NULL) {
			size_t data_size = (len * sizeof(*string));

			string = malloc(data_size + 1);

			if (string != NULL) {
				memcpy(string, data, data_size);
				string[data_size + 1] = '\0';

				return string;
			}
		}

		return NULL;
	}

	// the entrypoint wich will be called when needed by the client
	char* EXPORT run_entrypoint_sharedobject(char* input_file) {
		dll_function extrun;

		if (input_file == NULL)
			return NULL;

		void* lib = NULL;

		// open the dll/so
#if __linux__
		lib = dlopen(input_file, RTLD_LAZY);
#else 
		lib = LoadLibrary(input_file);
#endif

		if (lib != NULL) {
			char* dll_result = NULL;

#if __linux__
			// it need to extract the run function to call the plugin code
			extrun = dlsym(lib, "run");
			dll_result = extrun();

			dlclose(lib);
#elif defined _WIN32 || defined _WIN64
			// same as linux, it need to extract the run function
			extrun = (dll_function)GetProcAddress(lib, "run");

			if (extrun) {
				dll_result = extrun();
				FreeLibrary(lib);
			}
			else {
				return NULL;
			}
#endif
			// a new string need to be created in order to marshal's c# can do operations on it
			return _cpy_mem_string(dll_result, strlen(dll_result) + 1);;
		}

		return NULL;
	}

#ifdef __cplusplus
}
#endif


