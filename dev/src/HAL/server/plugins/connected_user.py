import json
import os
import sys 

if not sys.platform.startswith('win32'):
	import pwd

from datetime import datetime

def get_username():
	if sys.platform.startswith('win32'):
		return [os.getenv('username')]
	return os.popen('users').read()

str_date = datetime.now().strftime("%Y-%m-%dT%H:%M:%S")
if sys.platform.startswith('win32'):
	connected_users = get_username()
else:
	connected_users = get_username().split()

print(json.dumps({"connected_users": connected_users, "date":str_date}))
