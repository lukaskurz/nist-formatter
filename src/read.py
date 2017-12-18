import os
import myshutil
import sys

abspath = os.path.abspath(__file__)
dname = os.path.dirname(abspath)
os.chdir(dname)
os.chdir("./../nist/by_class")

alphabet = os.listdir()
finished = True
count = 0
complete = 731668
while True:
    finished = True
    for letter in alphabet:
        images = os.listdir("{}/train_{}".format(letter, letter))
        if(len(images) > count):
            print(images[count])
            finished = False
    if(finished):
        break
    count += 1
