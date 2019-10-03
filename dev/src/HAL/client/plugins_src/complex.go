package main

import (
    "encoding/json"
		"C"
)

type Point struct {
	X int
	Y int
	Z int
}

type Cube struct {
	Name string
	Point Point
}

//export run
func run() *C.char {
	cube := &Cube {
		Name: "cube",
		Point: Point {
			X: 0,
			Y: 0,
			Z: 0,
		},
	}

	jsonData,_ := json.Marshal(cube);

	return C.CString(string(jsonData))
}

func main() {
}
