import json
import os
import pwd
from datetime import datetime

def get_username():
    return os.popen('users').read()

connected_users = get_username().split()
str_date = datetime.now().strftime("%Y-%m-%dT%H:%M:%S")

print(json.dumps({"connected_users": connected_users, "date":str_date}))
