import numpy
import cudamat as cm
from scipy.special import expit
import matplotlib.pyplot as plt
import os
import os.path
from glob import glob
from threading import Thread
from time import sleep
import time
from stopwatch import Timer
from cudamat import CUDAMatrix as cudamatrix


class Network:
    input_hidden = []
    hidden_output = []
    learningrate = 0.1

    targets = None

    symbols = ["30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "41", "42", "43", "44", "45", "46", "47",
               "48", "49", "4a", "4b", "4c", "4d", "4e", "4f", "50", "51", "52", "53", "54", "55", "56", "57", "58",
               "59", "5a", "61", "62", "63", "64", "65", "66", "67", "68", "69", "6a", "6b", "6c", "6d", "6e", "6f",
               "70", "71", "72", "73", "74", "75", "76", "77", "78", "79", "7a"]

    def __init__(self, learningrate, inputlayer_size, hiddenlayer_size, outputlayer_size=len(symbols)):
        self.input_hidden = numpy.random.normal(0, pow(hiddenlayer_size, -0.5), (inputlayer_size, hiddenlayer_size))
        self.hidden_output = numpy.random.normal(0, pow(outputlayer_size, -0.5), (hiddenlayer_size, outputlayer_size))
        self.learningrate = learningrate
        self.init_map()

    def init_map(self):
        self.targets = {}
        for index in range(len(self.symbols)):
            target = numpy.ndarray((1, len(self.symbols)))
            target.fill(0.001 * 0.999)
            target[0][index] = 1.001 * 0.999
            self.targets.update({self.symbols[index]: target})

    def train_gpu(self, start=numpy.empty(()), target=numpy.empty(())):
        """Tries to predict the correct letter, compares with the target and changes weights using the error and GD"""
        gpu_start = cudamatrix(start)
        gpu_target = cudamatrix(target)
        gpu_input_hidden = cudamatrix(self.input_hidden)
        gpu_hidden_output = cudamatrix(self.hidden_output)

        gpu_input_hidden_result = cm.exp(cm.dot(gpu_start, gpu_input_hidden))
        gpu_hidden_output_result = cm.exp(cm.dot(gpu_input_hidden_result, gpu_hidden_output))

        gpu_hidden_output_error = gpu_target.subtract(gpu_hidden_output_result)
        gpu_input_hidden_error = cm.dot(gpu_hidden_output_error, gpu_hidden_output.transpose())

        gpu_hidden_output.add(cm.dot(gpu_input_hidden_result.transpose(), gpu_hidden_output_error.mult(
            gpu_hidden_output_result.subtract(cm.pow(gpu_hidden_output_result, 2)))).mult(self.learningrate))
        gpu_input_hidden.add(cm.dot(gpu_start.transpose(), gpu_input_hidden_error.mult(
            gpu_input_hidden_result.subtract(cm.pow(gpu_input_hidden_result, 2)))).mult(self.learningrate))

        self.hidden_output = gpu_hidden_output.asarray()
        self.input_hidden = gpu_input_hidden.asarray()

    def train(self, start=[], target=[]):
        """Tries to predict the correct letter, compares with the target and changes weights using the error and GD"""
        input_hidden_result = expit(numpy.dot(start, self.input_hidden))
        hidden_output_result = expit(numpy.dot(input_hidden_result, self.hidden_output))

        hidden_output_error = numpy.subtract(target, hidden_output_result)
        input_hidden_error = numpy.dot(hidden_output_error, self.hidden_output.T)

        self.hidden_output += self.learningrate * numpy.dot(
            input_hidden_result.transpose(),
            hidden_output_error * hidden_output_result * (1.0 - hidden_output_result))

        self.input_hidden += self.learningrate * numpy.dot(
            start.transpose(),
            (input_hidden_error * input_hidden_result * (1.0 - input_hidden_result)))

    def predict(self, start):
        """Tries to predict from an image converted to an float array.
        Returns the prediction array"""
        input_hidden_result = expit(numpy.dot(start, self.input_hidden))
        hidden_output_result = expit(numpy.dot(input_hidden_result, self.hidden_output))

        return hidden_output_result

    def mapsymbol(self, symbol):
        """Maps an hexadecimal ascii char to a result array"""
        return self.targets[symbol]

    def unmapsymbol(self, result):
        """Maps the result array to an ascii char"""
        biggest = 0
        for index in range(len(result)):
            if result[index] > result[biggest]:
                biggest = index
        return bytearray.fromhex(self.symbols[biggest]).decode()

    def save(self, path="save/save.txt"):
        if os.path.isfile(path):
            os.remove(path)

        fp = open(path, mode="w")
        fp.write("name;size,size;weight;weight;weight;weight\n")

        fp.write("input_hidden;" + str(len(self.input_hidden[0:])) + "," + str(len(self.input_hidden[0][0:])))
        weights = numpy.ndarray.flatten(self.input_hidden)
        for index in range(len(weights)):
            fp.write(";" + str(weights[index]))
        fp.write("\n")

        fp.write("hidden_output;" + str(len(self.hidden_output[0:])) + "," + str(len(self.hidden_output[0][0:])))
        weights = numpy.ndarray.flatten(self.hidden_output)
        for index in range(len(weights)):
            fp.write(";" + str(weights[index]))
        fp.flush()
        fp.close()

    def import_weights(self, path="save/save.txt"):
        if not os.path.isfile(path):
            return

        fp = open(path, "r")
        lines = fp.read().split("\n")

        elements_ih = lines[1].split(";")
        weights = numpy.asfarray(elements_ih[2:])
        dimensions = elements_ih[1].split(",")
        weights = weights.reshape((int(dimensions[0]), int(dimensions[1])))
        self.input_hidden = weights

        elements_ho = lines[2].split(";")
        weights = numpy.asfarray(elements_ho[2:])
        dimensions = elements_ho[1].split(",")
        weights = weights.reshape((int(dimensions[0]), int(dimensions[1])))
        self.hidden_output = weights
        fp.flush()
        fp.close()


files = glob("./../../nist/by_class/*.csv")
max_count = 60000.0
current_count = 0.0
running = True
cm.init()
hypothesis = Network(0.1, 64 * 64, 100)


# hypothesis.import_weights()


def write_progress():
    while running:
        print("%.2f" % (current_count / max_count * 100) + "% ~ line " + str(int(current_count)))
        sleep(1)


output_thread = Thread(target=write_progress)
output_thread.start()

stopwatch = Timer()
stopwatch.start()

for file in files:
    fp = open(file)
    file_content = fp.read()
    lines = file_content.split("\n")
    for line in lines:
        current_count += 1
        if current_count >= max_count:  # break if limit is reached
            break

        parts = line.split(";")
        target_data = hypothesis.mapsymbol(parts[0])
        input_data = (((numpy.asfarray(parts[1].split(","))
                        / 255)  # normalize to range of ]0;1[
                       + 0.01)  # so no 0 inputs exist
                      * 0.99).reshape((1, -1))  # no inputs bigger than 1 and correct dim

        hypothesis.train_gpu(input_data, target_data)

    fp.flush()
    fp.close()
    hypothesis.save()
    if current_count >= max_count:
        break

print(stopwatch.stop("Time:"))
running = False
output_thread.join()

correct = 0.0
for i in range(1000):
    splitted = lines[i].split(";")
    correctResult = bytearray.fromhex(splitted[0]).decode()
    input_data = numpy.asfarray(splitted[1].split(",")) / 255 * 0.99
    input_data += 0.01
    input_data -= 1.0
    input_data *= -1
    res = hypothesis.predict(input_data)

    if correctResult == hypothesis.unmapsymbol(res):
        correct += 1.0
print("Performance p: " + str(float(correct / 1000)))
cm.shutdown()
