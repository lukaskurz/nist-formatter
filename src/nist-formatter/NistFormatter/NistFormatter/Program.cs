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
				foreach (var file in Directory.GetFiles(trainDirectory))
				{
					Console.WriteLine(file);

					Bitmap bmp = new Bitmap(file);
					Bitmap resized = new Bitmap(ResizeImage(bmp,new Size(32,32)));
				}
			}
			Console.ReadKey();
		}
		public static Image ResizeImage(Image image, Size size, bool preserveAspectRatio = true)
		{
			int newWidth;
			int newHeight;
			if (preserveAspectRatio)
			{
				int originalWidth = image.Width;
				int originalHeight = image.Height;
				float percentWidth = (float)size.Width / (float)originalWidth;
				float percentHeight = (float)size.Height / (float)originalHeight;
				float percent = percentHeight < percentWidth ? percentHeight : percentWidth;
				newWidth = (int)(originalWidth * percent);
				newHeight = (int)(originalHeight * percent);
			}
			else
			{
				newWidth = size.Width;
				newHeight = size.Height;
			}
			Image newImage = new Bitmap(newWidth, newHeight);
			using (Graphics graphicsHandle = Graphics.FromImage(newImage))
			{
				graphicsHandle.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphicsHandle.DrawImage(image, 0, 0, newWidth, newHeight);
			}
			return newImage;
		}
	}
}