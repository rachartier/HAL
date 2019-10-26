import json
import datetime

utc_datetime = datetime.datetime.utcnow()
date = utc_datetime.strftime("%Y-%m-%dT%H:%M:%S")

print(json.dumps({
    "date": date 
}))
