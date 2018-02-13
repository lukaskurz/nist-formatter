using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;

namespace NistFormatter
{
	class Program
	{
		static void Main(string[] args)
		{
			AggregateAllFiles(5000);

			Console.ReadKey();
		}

		static void DeleteSummaries()
		{
			var directories = Directory.GetDirectories(@".\..\..\..\..\..\..\nist\by_class");
			foreach (var directory in directories)
			{
				Console.WriteLine(directory);
				var letter = directory.Split('\\').Last();
				var name = $@"{directory}\train_{letter}";
				var files = Directory.GetFiles(name);
				var delete = files.Where(filename => filename.EndsWith(".csv"));
				foreach (var file in delete)
				{
					File.Delete(file);
				}
			}
			Console.ReadKey();
		}

		static void AggregateAllFiles(int max = int.MaxValue)
		{
			var directories = Directory.GetDirectories(@".\..\..\..\..\..\..\nist\by_class");
			List<string> allFiles = new List<string>();
			Console.ForegroundColor = ConsoleColor.DarkYellow;
			foreach (var directory in directories)
			{
				var letter = directory.Split('\\').Last();
				var files = Directory.GetFiles($@"{directory}\train_{letter}");
				allFiles.AddRange(files);
				Console.WriteLine(directory);
			}
			allFiles.Shuffle();
			Console.ForegroundColor = ConsoleColor.Green;

			FileStream output = File.Open($@".\..\..\..\..\..\..\nist\by_class\output.csv", FileMode.OpenOrCreate);
			StreamWriter sout = new StreamWriter(output);
			sout.AutoFlush = true;

			max = Math.Min(allFiles.Count, max);

			for (int i = 0; i < max; i++)
			{
				if (i % 50000 == 0)
				{
					output = File.Open($@".\..\..\..\..\..\..\nist\by_class\output_{i}.csv", FileMode.OpenOrCreate);
					sout = new StreamWriter(output);
					sout.AutoFlush = true;
				}

				var name = allFiles[i];
				Bitmap bmp = new Bitmap(name);
				Bitmap resized = bmp.Resize(new Size(64, 64));

				var normalized = resized.Normalize();

				sout.WriteLine($"{name.Split('\\').Last().Split('_')[1]};{normalized};");
				Console.WriteLine(((float)i / (float)max * 100).ToString());
			}
			Console.ReadKey();
		}

		static void DeleteOldDirectories()
		{
			var directories = Directory.GetDirectories(@".\..\..\..\..\..\..\nist\by_class");
			foreach (var directory in directories)
			{
				Console.WriteLine(directory);
				var toDelete = Directory.GetDirectories(directory).Where(name => name.EndsWith("test") || name.EndsWith("train"));
				foreach (var oldDirectory in toDelete)
				{
					Console.WriteLine(oldDirectory);
					Directory.Delete(oldDirectory, true);
				}
			}
			Console.ReadKey();
		}

		static void AggregateLetterwise()
		{
			var directories = Directory.GetDirectories(@".\..\..\..\..\..\..\nist\by_class");
			foreach (var directory in directories)
			{
				var trainDirectory = Directory.GetDirectories(directory).Where(text => text.Split('\\').Last().StartsWith("train_")).First();
				var letter = ((char)Int16.Parse(trainDirectory.Split('_').Last(), NumberStyles.AllowHexSpecifier)).ToString();
				var filestream = File.AppendText($@"{trainDirectory}\{letter}.csv");
				filestream.AutoFlush = true;
				foreach (var file in Directory.GetFiles(trainDirectory).Where(name => name.EndsWith("png")))
				{
					Console.WriteLine(file);

					Bitmap bmp = new Bitmap(file);
					Bitmap resized = bmp.Resize(new Size(32, 32));

					var normalized = resized.Normalize();
					filestream.WriteLine($"{letter};{normalized};");
				}
			}
			Console.ReadKey();
		}

	}
}