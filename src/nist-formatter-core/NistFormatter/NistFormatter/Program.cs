using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;

namespace NistFormatter
{
    class Program
    {
		public static IConfiguration Configuration { get; set; }
		static void Main(string[] args)
        {
			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json");

			Configuration = builder.Build();

			Console.WriteLine("Hello");
			Console.WriteLine("This is a formatter specially designed for the NIST Database 19");


            // Check if the nist directory exists
			string nistPath = Configuration["nistpath"];
			if (!Directory.Exists(nistPath))
			{
				throw new DirectoryNotFoundException("Nist directory not found");
			}

            // Get a list of all the file paths to the images
			Console.WriteLine("Gathering paths of files...");
			var files = GetAllFiles(nistPath);
			Console.WriteLine($"Found {files.Count} test example files.");

            // Shuffling the data to randomize the order
			Console.WriteLine("Randomizing file list...");
			files.Shuffle();
			Console.WriteLine("Finished randomizing.");


			int limit = Convert.ToInt32(Configuration["examplesPerFile"]);  // Number of examples per file
			int imageSize = Convert.ToInt32(Configuration["pictureSize"]);  // Size of the image in pixels. All the images are quadratic so only one number needed.

            // Applying grayscale to images, resizing the and flattening them to number arrays
			Console.WriteLine("Normalizing files...");
			var timer = Stopwatch.StartNew();   // For benchmarking purposes

            // Defines if only a subset of the dataset should be used.
            // If the subsetSize is -1 or null, the complete dataset is used
            var subsetSize = -1;
            if(Configuration["subsetSize"] != null)
            {
                subsetSize = Convert.ToInt32(Configuration["subsetSize"]);
                if (subsetSize < 0)
                {
                    NormalizeFiles(files, nistPath, imageSize, limit);
                }
                else
                {
                    NormalizeFiles(files.GetRange(0, Math.Min(subsetSize, files.Count)), nistPath, imageSize, limit);
                }
            }
            else
            {
                NormalizeFiles(files, nistPath, imageSize, limit);
            }

            timer.Stop();
			Console.WriteLine("Finished normalizing files.");
			Console.WriteLine($"Took {timer.Elapsed.Hours:00}:{timer.Elapsed.Minutes:00}:{timer.Elapsed.Seconds:00}:{timer.Elapsed.Milliseconds:00}");
		}

        /// <summary>
        /// Checking for directories generated with an old version of the formatter. Only needed while developing this tool
        /// </summary>
        /// <param name="nistPath"></param>
        /// <returns></returns>
		private static string[] CheckForOldDirectories(string nistPath)
		{
			var rootDirectories = Directory.GetDirectories(nistPath);
			List<string> oldDirectories = new List<string>();
			foreach (var rootDirectory in rootDirectories)
			{
				var directories = Directory.GetDirectories(rootDirectory).ToList();
				oldDirectories.AddRange(directories.Where(d => d.Split(Path.DirectorySeparatorChar).Last().StartsWith("train")));
			}

			return oldDirectories.ToArray();
		}

        /// <summary>
        /// Gets a list of all images with associated labels in the nist folder
        /// </summary>
        /// <param name="nistPath">Path of the nist folder</param>
        /// <returns></returns>
		private static List<TestExample> GetAllFiles(string nistPath)
		{
			var rootDirectories = Directory.GetDirectories(nistPath);
			List<TestExample> files = new List<TestExample>();
			foreach (var rootDirectory in rootDirectories)
			{
				string currentResult = Convert.ToChar(Convert.ToUInt32(rootDirectory.Split(Path.DirectorySeparatorChar).Last(), 16)).ToString();
				var subdirectories = Directory.GetDirectories(rootDirectory);
				foreach (var subdirectory in subdirectories)
				{
					files.AddRange(Directory.GetFiles(subdirectory).Select(f => new TestExample(f, currentResult)));
				}
			}

			return files;
		}

        /// <summary>
        /// Applies grayscale to images, resizes them, flattens them to arrays and writes them into .csv files
        /// </summary>
        /// <param name="files">List of all the images with associated labels that will be normalized</param>
        /// <param name="nistPath">Filepath of the nist dataset folder</param>
        /// <param name="imageSize">Height/Width in pixels to which the images should be resized</param>
        /// <param name="limit">Number of examples per file</param>
		private static void NormalizeFiles(List<TestExample> files, string nistPath, int imageSize, int limit)
		{
			int numberOfParallelThreads = Convert.ToInt32(Configuration["numberOfParallelThreads"]);    // Number of threads running in parallel. Results in better performance if tuned to cpu core count
			Queue<Thread> threads = new Queue<Thread>();
			var numberOfFiles = files.Count / limit;    // Number of .csv files that will be generated. 
			for (int i = 0; i < numberOfFiles; i++)
			{
				string filepath = $"{nistPath}\\datafile_{i}.csv";
				var currentFiles = files.GetRange(limit * i, limit);
				var t = new Thread(() => WriteTestExamples(currentFiles, imageSize, filepath)); // Start new thread to work on this file
				threads.Enqueue(t);
			}
			if (files.Count % limit > 0)    // In the division above, a set of example might be left out at end. These are addressed in this file
			{
				string filepath = $"{nistPath}\\datafile_{numberOfFiles}.csv";
				var currentFiles = files.GetRange(limit * numberOfFiles, files.Count % limit);
				var t = new Thread(() => WriteTestExamples(currentFiles, imageSize, filepath));
				threads.Enqueue(t);
			}

            // While there are still jobs/threads in the queue, keep starting them
			while (threads.Count != 0)
			{
				Console.WriteLine($"Starting new threads...");
				var timer = Stopwatch.StartNew();
				var active = new List<Thread>();
				for (int i = 0; i < numberOfParallelThreads; i++)
				{
					if (threads.Count != 0)
					{
						var temp = threads.Dequeue();
						temp.Start();
						active.Add(temp);
					}
				}

                // Wait for all the active threads to finish.
                // Prevents using more threads in parallel than defined, which could result in worse performance
				foreach (var at in active)
				{
					at.Join();
				}
				timer.Stop();
				Console.WriteLine($"Finished threads in {timer.ElapsedMilliseconds / 1000.0}s");
			}
		}

        /// <summary>
        /// Write a list of test example to .csv file
        /// </summary>
        /// <param name="files">List of test examples with labels</param>
        /// <param name="imageSize">Height/Width in pixels to which the images should be resized</param>
        /// <param name="filename">name of the .csv file</param>
		static void WriteTestExamples(List<TestExample> files, int imageSize, string filename)
		{
			var fs = File.Create(filename);
			StreamWriter sw = new StreamWriter(fs);
			sw.WriteLine(files.Count);
			List<string> lines = new List<string>();
			for (int lineIndex = 0; lineIndex < files.Count; lineIndex++)
			{
				var file = files[lineIndex];
				lines.Add(WriteTestExample(file, imageSize));
			}

			foreach (var line in lines)
			{
				sw.WriteLine(line);
			}

			sw.Flush();
			sw.Close();
			fs.Close();
		}

        /// <summary>
        /// Converts a single example with label to a string
        /// </summary>
        /// <param name="file">Test example with label</param>
        /// <param name="imageSize">Height/Width in pixels to which the image should be resized</param>
        /// <returns></returns>
		static string WriteTestExample(TestExample file, int imageSize)
		{
			Bitmap bmp = new Bitmap(file.Filename);
			bmp = bmp.Resize(new Size(imageSize, imageSize));
			return $"{file.CorrectResult};{bmp.Normalize()}";
		}
	}
}
