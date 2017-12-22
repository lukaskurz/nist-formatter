using System;
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
			var directories = Directory.GetDirectories(@".\..\..\..\..\..\..\nist\by_class");
			foreach (var directory in directories)
			{
				var trainDirectory = Directory.GetDirectories(directory).Where(text => text.Split('\\').Last().StartsWith("train_")).First();
				var letter = ((char)Int16.Parse(trainDirectory.Split('_').Last(), NumberStyles.AllowHexSpecifier)).ToString();
				var filestream = File.AppendText($@"{trainDirectory}\{letter}");
				filestream.AutoFlush = true;
				foreach (var file in Directory.GetFiles(trainDirectory).Where(name => name.EndsWith("png")))
				{
					Console.WriteLine(file);

					Bitmap bmp = new Bitmap(file);
					Bitmap resized = bmp.Resize(new Size(32, 32));

					var normalized = resized.Normalize();
					filestream.WriteLine($"{normalized};{letter};");
				}
			}
			Console.ReadKey();
		}
		
	}
}