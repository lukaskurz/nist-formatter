using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace NistFormatter
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Hello");
			Console.WriteLine("This is a formatter specially designed for the NIST Database 19");


			string nistPath = "";
			do
			{
				Console.WriteLine("Please enter the path of the NIST Database 19 Directory:");
				string path = Console.ReadLine();
				if (Directory.Exists(path))
				{
					nistPath = path;
					break;
				}
				Console.WriteLine("Path entered was invalid. Please Try again.");
			} while (true);


			Console.WriteLine("Searching for deprecated directories...");
			var oldDirectories = CheckForOldDirectories(nistPath);


			do
			{
				if (oldDirectories.Length == 0)
				{
					break;
				}
				Console.WriteLine($"{oldDirectories.Length} old directories found. Do you want to delete them (y/n)?");
				string response = Console.ReadLine().ToLower();
				if (response.StartsWith("y"))
				{
					foreach (var oldDirectory in oldDirectories)
					{
						Console.WriteLine($"Deleting {oldDirectory}");
						Directory.Delete(oldDirectory, true);
					}
					break;
				}
				else if (response.StartsWith("n"))
				{
					break;
				}
				Console.WriteLine("Invalid answer. Try again");
			} while (true);


			Console.WriteLine("Gathering paths of files...");
			var files = GetAllFiles(nistPath);
			Console.WriteLine($"Found {files.Count} test example files.");


			Console.WriteLine("Randomizing file list...");
			files.Shuffle();
			Console.WriteLine("Finished randomizing.");


			Console.WriteLine("Due to problems with big files, the data is split into multiple files.");
			int limit = -1;
			bool isNumber = false;
			while (!isNumber)
			{
				Console.WriteLine("How many test examples should be grouped together into one file?");
				isNumber = Int32.TryParse(Console.ReadLine(), out limit);
				if (!isNumber)
				{
					Console.WriteLine("Invalid number. Try again.");
				}
			}

			int imageSize = -1;
			isNumber = false;
			while (!isNumber)
			{
				Console.WriteLine("What should be the size of the quadratic Images (in pixels)?");
				isNumber = Int32.TryParse(Console.ReadLine(), out imageSize);
				if (!isNumber)
				{
					Console.WriteLine("Invalid number. Try again.");
				}
			}

			Console.WriteLine("Normalizing files...");
			NormalizeFiles(files, nistPath, imageSize, limit);
			Console.WriteLine("Finished normalizing files.");
			Console.ReadKey();

		}

		private static string[] CheckForOldDirectories(string nistPath)
		{
			var rootDirectories = Directory.GetDirectories(nistPath);
			List<string> oldDirectories = new List<string>();
			foreach (var rootDirectory in rootDirectories)
			{
				var directories = Directory.GetDirectories(rootDirectory).ToList();
				oldDirectories.AddRange(directories.Where(d => d.Split('\\').Last().StartsWith("train")));
			}

			return oldDirectories.ToArray();
		}

		private static List<TestExample> GetAllFiles(string nistPath)
		{
			var rootDirectories = Directory.GetDirectories(nistPath);
			List<TestExample> files = new List<TestExample>();
			foreach (var rootDirectory in rootDirectories)
			{
				string currentResult = Convert.ToChar(Convert.ToUInt32(rootDirectory.Split('\\').Last(),16)).ToString();
				var subdirectories = Directory.GetDirectories(rootDirectory);
				foreach (var subdirectory in subdirectories)
				{
					files.AddRange(Directory.GetFiles(subdirectory).Select(f => new TestExample(f,currentResult)));
				}
			}

			return files;
		}

		private static void NormalizeFiles(List<TestExample> files, string nistPath, int imageSize, int limit)
		{
			Queue<Thread> threads = new Queue<Thread>();
			var numberOfFiles = files.Count / limit;
			for (int i = 0; i < numberOfFiles; i++)
			{
				string filepath = $"{nistPath}\\datafile_{i}.csv";
				var currentFiles = files.GetRange(limit * i, limit);
				var t = new Thread(() => WriteTestExamples(currentFiles, imageSize, filepath));
				threads.Enqueue(t);
			}
			if (files.Count % limit > 0)
			{
				string filepath = $"{nistPath}\\datafile_{numberOfFiles}.csv";
				var currentFiles = files.GetRange(limit * numberOfFiles, files.Count % limit);
				var t = new Thread(() => WriteTestExamples(currentFiles, imageSize, filepath));
				threads.Enqueue(t);
			}

			while (threads.Count != 0)
			{
				Console.WriteLine($"Starting new threads...");
				var timer = Stopwatch.StartNew();
				var active = new List<Thread>();
				for (int i = 0; i < 4; i++)
				{
					if(threads.Count != 0)
					{
						var temp = threads.Dequeue();
						temp.Start();
						active.Add(temp);
					}
				}
				foreach (var at in active)
				{
					at.Join();
				}
				timer.Stop();
				Console.WriteLine($"Finished threads in {timer.ElapsedMilliseconds/1000.0}s");
			}
		}

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

		static string WriteTestExample(TestExample file, int imageSize)
		{
			Bitmap bmp = new Bitmap(file.Filename);
			bmp = bmp.Resize(new Size(imageSize, imageSize));
			return $"{file.CorrectResult};{bmp.Normalize()}";
		}
	}
}