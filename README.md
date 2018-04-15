# nist-formatter
A formatting tool for the nist database19.

## Architecture
The NistFormatter is implemented once in .Net Framework and once in .Net Core. For deployment docker is used.

## NistFormatter
The raw data from the dataset is in a .png format which needs to be converted and normalized to be used as input for machine learning algorithms.
This is done using the NistFormatter, a simple tool that formats the image files to .csv data. It can also crop the picture to a defined size and automatically changes the rgb values to an integer value ranging between 0 and 255. The data gets split up into multiple files, due to many programs and editors having problems with very large files (multiple gigabytes).
The First version of the NistFormatter was done in using the .Net Framework. Due to incompatibility with Linux, the same logic was implemented in .Net Core. To supplement the System.Drawing, which was to this point not yet implemented in .Net Core, CoreCompat.System.Drawing was used, which is basically the same as System.Drawing, only that it is working in .Net Core.

## Setup
In order to easily deploy the formatter on any machine, you can build a docker image using the Dockerfile. The formatted files are stored in the container, from where they have to be retrieved.