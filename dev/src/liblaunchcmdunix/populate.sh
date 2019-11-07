
x86_64-linux-gnu-gcc -shared -fPIC -Wall -Wextra launchcmdunix.c -o ../HAL/client/lib/liblaunchcmdunix.dll && echo "client populated (dll)."

x86_64-linux-gnu-gcc -shared -fPIC -Wall -Wextra launchcmdunix.c -o ../HAL/plugins_checker/lib/liblaunchcmdunix.dll && echo "plugins_checked populated (dll)."

gcc -shared -fPIC -Wall -Wextra launchcmdunix.c -o ../HAL/plugins_checker/lib/liblaunchcmdunix.so && echo "plugins_checker populated (so)."

gcc -shared -fPIC -Wall -Wextra launchcmdunix.c -o ../HAL/client/lib/liblaunchcmdunix.so && echo "plugins_checker populated (so)."
