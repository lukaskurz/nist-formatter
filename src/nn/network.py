import numpy
from scipy.special import expit
import matplotlib.pyplot as plt
import os


class network:
    input_hidden = []
    hidden_output = []
    learningrate = 0.1

    symbols = ["30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "41", "42", "43", "44", "45", "46", "47",
               "48", "49", "4a", "4b", "4c", "4d", "4e", "4f", "50", "51", "52", "53", "54", "55", "56", "57", "58",
               "59", "5a", "61", "62", "63", "64", "65", "66", "67", "68", "69", "6a", "6b", "6c", "6d", "6e", "6f",
               "70", "71", "72", "73", "74", "75", "76", "77", "78", "79", "7a"]

    def __init__(self, learningrate, inputlayer_size, hiddenlayer_size, outputlayer_size=len(symbols)):
        self.input_hidden = numpy.random.normal(0, pow(hiddenlayer_size, -0.5), (inputlayer_size, hiddenlayer_size))
        self.hidden_output = numpy.random.normal(0, pow(outputlayer_size, -0.5), (hiddenlayer_size, outputlayer_size))
        self.learningrate = learningrate

    def train(self, start=[], target=[]):

        # Current prediction
        input_hidden_result = expit(numpy.dot(start, self.input_hidden))
        hidden_output_result = expit(numpy.dot(input_hidden_result, self.hidden_output))

        hidden_output_error = numpy.subtract(target, hidden_output_result)
        input_hidden_error = numpy.dot(hidden_output_error, self.hidden_output.T)

        self.hidden_output += self.learningrate * numpy.dot(
            input_hidden_result[:, None],
            hidden_output_error[None, :] * hidden_output_result[None, :] * (1.0 - hidden_output_result[None, :]))

        self.input_hidden += self.learningrate * numpy.dot(
            start[:, None],
            (input_hidden_error[None, :] * input_hidden_result[None, :] * (1.0 - input_hidden_result[None, :])))

    def predict(self, start=[]):
        """Tries to predict from an image converted to an float array.
        Returns the prediction array"""
        input_hidden_result = expit(numpy.dot(start, self.input_hidden))
        hidden_output_result = expit(numpy.dot(input_hidden_result, self.hidden_output))

        return hidden_output_result

    def mapsymbol(self, symbol):
        """Maps an ascii char to result array"""
        for index in range(len(self.symbols)):
            if bytearray.fromhex(self.symbols[index]).decode() == symbol:
                target = numpy.zeros(len(self.symbols))
                target[index] = 1
                return target
        raise ValueError("Tried to map a char that does not exist in the symbols entries")

    def unmapsymbol(self, result=[]):
        """Maps the result array to an ascii char"""
        biggest = 0
        for index in range(len(result)):
            if result[index] > result[biggest]:
                biggest = index
        return bytearray.fromhex(self.symbols[biggest]).decode()

    def save(self, path = "save.txt"):


fp = open("C:/Github/nist-classificator/nist/by_class/output_0.csv")
content = fp.read()
lines = content.split("\n")
test = network(0.1, 64 * 64, 200)
for i in range(len(lines) - 1):
    line = lines[i]
    splitted = line.split(";")
    correctResult = bytearray.fromhex(splitted[0]).decode()
    input_data = numpy.asfarray(splitted[1].split(",")) / 255 * 0.99
    input_data += 0.01
    input_data -= 1.0
    input_data *= -1
    test.train(input_data, test.mapsymbol(correctResult))

for i in range(100):
    splitted = lines[i].split(";")
    correctResult = bytearray.fromhex(splitted[0]).decode()
    input_data = numpy.asfarray(splitted[1].split(",")) / 255 * 0.99
    input_data += 0.01
    input_data -= 1.0
    input_data *= -1
    res = test.predict(input_data)
    plt.title("Correct:" + correctResult + " Prediction:" + test.unmapsymbol(res))
    plt.plot(res)
    plt.show()