import json
import getpass
from datetime import datetime

connected_user = getpass.getuser()
str_date = datetime.now().strftime("%Y-%m-%dT%H:%M:%S")

print(json.dumps({"connected_user": connected_user, "date":str_date}), end='')

import platform

version = platform.platform()

print(json.dumps({'kernel_version': version}), end='')
<CHECKSUM>bd88cf3fd121c1e74eeb1f7484c49664f79a56b0faf47df439072e6dbb295929</CHECKSUM></3><EOF>