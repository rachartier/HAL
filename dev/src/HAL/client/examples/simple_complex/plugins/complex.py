import json

class Point(object):
    def __init__(self, x, y, z):
        self.x = x
        self.y = y
        self.z = z

class Cube(object):
    def __init__(self):
        self.point = Point(0, 0, 0)
        self.name = "cube"

def obj_to_dict(obj):
   return obj.__dict__

cube = Cube()

print(json.dumps(cube.__dict__, default=obj_to_dict))
