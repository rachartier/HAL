#include <stdio.h>
#include <stdlib.h>

#if __linux
# define CALL 
#else 
# define CALL __stdcall 
#endif

char *run() {
	return "je suis le run d'une DLL";
}
