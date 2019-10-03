package main

import (
    "encoding/json"
		"C"
		"time"
)

func main() {
}

//export run
func run() *C.char {
	data := map[string]time.Time{"date": time.Now().UTC()}
	res,_ := json.Marshal(data)

	return C.CString(string(res))
}


