import os

def getDirectories(name):
    return [i for i in next(os.walk(name))[1]]



for directory in getDirectories("./../nist/by_class"):
    for file in getDirectories("./../nist/by_class/{}".format(directory)):
        if file.startswith("train"):
            print(file)

