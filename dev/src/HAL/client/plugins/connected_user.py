import json
import os
import pwd
from datetime import datetime

def get_username():
    return pwd.getpwuid( os.getuid() )[ 0 ]

connected_user = get_username()
str_date = datetime.now().strftime("%Y-%m-%dT%H:%M:%S")

print(json.dumps({"connected_user": connected_user, "date":str_date}))
