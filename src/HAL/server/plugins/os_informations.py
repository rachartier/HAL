import platform
import json

osInfo = {
	"machine":platform.machine(),
	"version":platform.version(),
	"platform":platform.platform(),
	"system":platform.system(),
	"processor":platform.processor()
}

print(json.dumps(osInfo));
