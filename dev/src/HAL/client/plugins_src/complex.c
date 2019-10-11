#include <stdio.h>
#include <stdlib.h>

typedef struct {
	int x;
	int y;
	int z;
} Point;

typedef struct {
	char 	name[32];
	Point point;
} Cube;

char* run() {
	Cube cube = {
		.name = "cube",
		.point = {
			.x = 0,
			.y = 0,
			.z = 0
		}
	};

	char* data = malloc(256 * sizeof(char));

	sprintf(data, "{\"name\":\"%s\",\n\"point\": {\n\"x\": %d,\n\"y\": %d,\n\"z\": %d,\n}}", cube.name, cube.point.x, cube.point.y, cube.point.z);

	return data;
}