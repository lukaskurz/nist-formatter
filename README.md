# nist-classificator
A classificator using a neural network and the NIST database 19, which includes the all the datasets from NIST regarding handwritten characters.

## Architecture
The raw data from the dataset is in a .png format which needs to be converted and normalized to be used as input for the neural network.
This is done using the NistFormatter, a simple CLI that formats the image files to .csv data. It can also crop the picture to a defined size and automatically changes the rgb values to an integer value ranging between 0 and 255. The data gets split up into multiple files, due to many programs and editors having problems with very large files (multiple gigabytes).
The First version of the NistFormatter was done in using the .Net Framework. Due to incompatibility with Linux, the same logic was implemented in .Net Core. To supplement the System.Drawing, which was to this point not yet implemented in .Net Core, CoreCompat.System.Drawing was used, which is basically the same as System.Drawing, only that it is working in .Net Core.

The csv files get parsed by main neural network code. The current architecture of the neural network is very simple, having only 1 input, hidden and output layer. The neural network code was implemented in python using numpy, scipy and matplotlib. I did some testing using cudamat in order to test performance differences. Not only was cudamat rather difficult to install and configure, it also resulted in worse training times, which led me to believe that I probably installed it wrong. It could also be due to the rather large overhead when piping data from the gpu and back, which could be due to wrong implementation on my side.
