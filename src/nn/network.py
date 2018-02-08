import numpy
import scipy.special
import matplotlib.pyplot as plt
import os


class network:
    weights = []

    symbols = ["30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "41", "42", "43", "44", "45", "46", "47", "48", "49", "4a", "4b", "4c", "4d", "4e", "4f", "50", "51", "52", "53", "54",
               "55", "56", "57", "58", "59", "5a", "61", "62", "63", "64", "65", "66", "67", "68", "69", "6a", "6b", "6c", "6d", "6e", "6f", "70", "71", "72", "73", "74", "75", "76", "77", "78", "79", "7a"]

    def __init__(self, layers=[32 * 32, 100, 100, 62]):
        for i in range(len(layers) - 1):
            self.weights.append(scipy.special.expit(numpy.random.normal(
                0, pow(layers[i + 1], -0.5), (layers[i], layers[i + 1]))))

    def train(self, start=[], target=[]):
        output = self.predict(start)
        error = numpy.subtract(target, output)
        return error

    def predict(self, start=[]):
        output = start
        for weight in self.weights:
            output = numpy.dot(output, weight)
            output = scipy.special.expit(output)
        return output

    def mapsymbol(self, symbol):
        for i in self.symbols:
            if bytearray.fromhex(i).decode() == symbol:
                return i
        return -1

    def unmapsymbol(self, result=[]):
        biggest = 0
        for i in range(len(result)):
            if result[i] > result[biggest]:
                biggest = i
        return bytearray.fromhex(self.symbols[biggest]).decode()


fp = open("C:/Github/nist-classificator/nist/by_class/output_0.csv")
content = fp.read()
lines = content.split("\n")
# for line in lines:
#     elements = line.split(";")
#     pixels = numpy.asarray(elements[1].split(","))
#     pixel2d = numpy.reshape(pixels, (32, 32))
#     plt.imshow(pixel2d, cmap='Greys', interpolation='None')

elements = lines[0].split(";")
pixels = numpy.asarray(elements[1].split(","))
pixel2d = numpy.reshape(pixels, (32, 32))
pixel2d = numpy.divide(pixel2d, 255.0)
plt.imshow(pixel2d, cmap='Greys', interpolation='None')
plt.show()


# test = network()
# res = test.predict(numpy.random.random_sample((32 * 32)))
# plt.plot(res)
# plt.show()
