import shutil
import json

total, used, free = shutil.disk_usage("/")

print(json.dumps({"total": (total // (2**30)), "used": (used // (2**30)), "free": (free // (2**30))}), end="")