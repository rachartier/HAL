#include <ifaddrs.h>
#include <stdio.h>
#include <stdlib.h>

char* run(void) {
	struct ifaddrs* id;
	int val;
	char* ret = malloc(1024);

	val = getifaddrs(&id);

	sprintf(ret, "{ \"name\": \"%s\", \"addr\": %d, \"data\": %d}", id->ifa_name, id->ifa_addr, id->ifa_data);

	return ret;
}
