import json
import getpass
from datetime import datetime

connected_user = getpass.getuser()
str_date = datetime.now().strftime("%Y-%m-%dT%H:%M:%S")

print(json.dumps({"connected_user": connected_user, "date":str_date}), end='')
import json
import platform

version = platform.platform()

print(json.dumps({'kernel_version': version}), end='')
