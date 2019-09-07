#include <stdio.h>
#include <stdlib.h>
#include <time.h>
#include <string.h>

#if __linux
# define CALL
#else
# define CALL __stdcall
#endif

#define O_VICTORY 0
#define X_VICTORY 1
#define NO_VICTORY 2

#define MAX_JSON_SIZE 1024

const char player_char[] = { 'O', 'X' };

static char json[1024] = { '\0' };
static int victory = -1;

static char board[3][3] = {
	"---",
	"---",
	"---"
};

static int is_cell_empty(int x, int y) {
	return board[y][x] == '-';
}

static int is_board_full() {
	for (int y = 0; y < 3; ++y) {
		for (int x = 0; x < 3; ++x) {
			if (board[y][x] == '-')
				return 0;
		}
	}

	return 1;
}

static int check_row(int y, char player) {
	for (int x = 0; x < 3; ++x) {
		if (board[y][x] != player)
			return 0;
	}

	return 1;
}

static int check_col(int x, char player) {
	for (int y = 0; y < 3; ++y) {
		if (board[y][x] != player)
			return 0;
	}

	return 1;
}

static int check_dia(char player) {
	int winner = 1;
	
	for (int i = 0; i < 3; ++i) {
		if (board[i][i] != player)
			return 0;
	}

	if (winner)
		return 1;

	for (int i = 0; i < 3; ++i) {
		if(board[i][2 - i] != player)
			return 0;
	}

	return 1;
}

static void make_turn(int player_index) {
	char pc = player_char[player_index];
	
	int x = rand() % 3;
	int y = rand() % 3;

	while (!is_cell_empty(x, y)) {
		x = rand() % 3;
		y = rand() % 3;
	}
	board[y][x] = pc;

	if (check_row(y, pc)
	||  check_col(x, pc)
	||  check_dia(pc)) {
		victory = player_index;
	}
}

void json_write(char* data) {
	static int index = 0;

	int data_size = strlen(data);

	if (data_size >= MAX_JSON_SIZE - 1) {
		json[index] = '}';
		json[index + 1] = '\0';
	}

	strncpy(json + index, data, data_size);

	index += data_size;
}

void json_write_value(char* data) {
	int data_size = strlen(data);
	char normalized_json[data_size + 3];

	sprintf(normalized_json, "\"%s\",", data);
	json_write(normalized_json);
}

void json_delete_last_comma() {
	char* ptr = strrchr(json, ',');

	if (ptr != NULL) {
		json[ptr - json] = ' ';
	}
}

char* run() {
	int turn = 0;
	int winner;
	
	srand(time(NULL));

	json_write("{ \"history\": [");


	while (victory == -1) {
		make_turn(turn % 2);

		char one_line_board[32];
	
		sprintf(one_line_board, "\%s%s%s", board[0], board[1], board[2]);
		json_write_value(one_line_board);

		if (is_board_full()) {
			victory = NO_VICTORY;
		}

		++turn;
	}
	json_delete_last_comma();

	json_write("], \"victory\":");

	if (victory == O_VICTORY) {
		json_write_value("O");
	}
	else if (victory == X_VICTORY) {
		json_write_value("X");
	}
	else {
		json_write_value("draw");
	}

	json_delete_last_comma();
	json_write("}");

	return json;
}