import os
import myshutil

print(os.getcwd())
os.chdir("./../nist/by_class")
print(os.getcwd())
print(os.listdir("./30"))

alphabet = os.listdir()
count = 0
finished = True
for letter in alphabet:
    directory = "./"+letter
    print(directory)
    os.chdir(directory)
    if os.path.exists("train"):
        myshutil.rmtree("train")
    #if os.path.exists("test"):
    #    myshutil.rmtree("test")
    fromPath = "./train_"+letter
    toPath = "./train"
    print("from {} to {}".format(fromPath,toPath))
    
    myshutil.copytree(fromPath, toPath)

    
    os.chdir("..")
    

