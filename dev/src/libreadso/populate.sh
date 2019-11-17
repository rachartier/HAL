
x86_64-linux-gnu-gcc -shared -fPIC -Wall -Wextra readso.c -o ../HAL/client/lib/libreadso.dll && echo "client populated (dll)."

x86_64-linux-gnu-gcc -shared -fPIC -Wall -Wextra readso.c -o ../HAL/plugins_checker/lib/libreadso.dll && echo "plugins_checked populated (dll)."

gcc -shared -fPIC -Wall -Wextra readso.c -o ../HAL/plugins_checker/lib/libreadso.so && echo "plugins_checker populated (so)."

gcc -shared -fPIC -Wall -Wextra readso.c -o ../HAL/client/lib/libreadso.so && echo "plugins_checker populated (so)."
