import json
import platform

version = platform.platform()

print(json.dumps({'kernel_version': version}))
