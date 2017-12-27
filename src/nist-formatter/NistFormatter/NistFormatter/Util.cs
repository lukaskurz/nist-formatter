using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NistFormatter
{
	static class Util
	{
		public static Bitmap Resize(this Bitmap bmp, Size size, bool preserveAspectRatio = true)
		{
			int newWidth;
			int newHeight;
			if (preserveAspectRatio)
			{
				int originalWidth = bmp.Width;
				int originalHeight = bmp.Height;
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
			Bitmap newBmp = new Bitmap(newWidth, newHeight);
			using (Graphics graphicsHandle = Graphics.FromImage(newBmp))
			{
				graphicsHandle.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphicsHandle.DrawImage(bmp, 0, 0, newWidth, newHeight);
			}
			return newBmp;
		}

		public static string Normalize(this Bitmap bmp)
		{
			string result = "";
			for (int x = 0; x < bmp.Width; x++)
			{
				for (int y = 0; y < bmp.Height; y++)
				{
					var pixel = bmp.GetPixel(x, y);
					var grayscale = 0.2126 * pixel.R + 0.7152 * pixel.G + 0.0722 * pixel.B;
					result += grayscale.ToString();
					if (y + 1 < bmp.Height) result += ",";
				}
				if (x + 1 < bmp.Width) result += ",";
			}

			return result;
		}
		private static Random rng = new Random();
		public static void Shuffle<T>(this IList<T> list)
		{
			int n = list.Count;
			Console.ForegroundColor = ConsoleColor.Yellow;
			while (n > 1)
			{
				n--;
				int k = rng.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
				Console.WriteLine((100.0-((float)n/(float)list.Count*100.0)).ToString());
			}
		}
	}
}
