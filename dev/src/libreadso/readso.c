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


/*
	 libreadso had to be created in order to read shared object file and "classic" dll

	 c# has a tag (DllImport) to load a so/dll, but you need to specifically add one for each dll/so
	 if we want to load them on the fly, then we'll want to have this utility librairie.

	 it serves two purposes:
	 - execute the "run" procedure of a .so/.dll and get the resulting string
	 - allocate the memory needed by c#'s marshal of the data returned by the run procedure 

	 if no memory is allocated, then marshal will get the adress of the first result, then add the size of the second data, the size of the third data and so on.

*/

#ifdef DEBUG
# define DPRINT(msg, ...) printf("line %d: " msg "\n", __LINE__, __VA_ARGS__ )
#else 
# define DPRINT(msg, ...) 
#endif

#define NUMBER_ENTRYPOINT_FUNC_NAME 3

// the code need to be executed as C
#ifdef __cplusplus
extern "C" {
#endif

	// a pointer to function to call the dll entry point
	typedef char* (*DllFunction)(void);

	static const char* g_entrypoint_name[NUMBER_ENTRYPOINT_FUNC_NAME] = {
		"run",
		"Run"
	};

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

	static DllFunction _get_entrypoint(void *lib) {
		DllFunction entrypoint = NULL;

		for(int i = 0; i < NUMBER_ENTRYPOINT_FUNC_NAME; ++i) {
#if __linux__
			entrypoint = dlsym(lib, g_entrypoint_name[i]);
#elif defined _WIN32 || defined _WIN64
			entrypoint = (DllFunction)GetProcAddress(lib, g_entrypoint_name[i]);
#endif

			if(entrypoint != NULL) {
				DPRINT("entrypoint found: %s", g_entrypoint_name[i]);
				return entrypoint;
			}
		}


		return NULL;
	}

	// the entrypoint wich will be called when needed by the client
	char* EXPORT run_entrypoint_sharedobject(char* input_file) {

		if (input_file == NULL) {
			return NULL;
		}

		void* lib = NULL;

		// open the dll/so
#if __linux__
		lib = dlopen(input_file, RTLD_LAZY);
#else 
		lib = LoadLibrary(input_file);
#endif

		if (lib != NULL) {
			DllFunction extrun = NULL;
			char* dll_result = NULL;

			extrun = _get_entrypoint(lib);

			if(extrun) {
				dll_result = extrun();
#if __linux__
				char* result = _cpy_mem_string(dll_result, strlen(dll_result) + 1);

				dlclose(lib);

				return result;

#elif defined _WIN32 || defined _WIN64
				FreeLibrary(lib);

				return  _cpy_mem_string(dll_result, strlen(dll_result) + 1);;
#endif
			}
		}

		return NULL;
	}

#ifdef __cplusplus
}
#endif


