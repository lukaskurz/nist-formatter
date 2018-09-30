using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace NistFormatter
{
	static class Util
	{
        /// <summary>
        /// Resizes an image
        /// </summary>
        /// <param name="bmp">the image to be resized</param>
        /// <param name="size">New size of the image</param>
        /// <param name="preserveAspectRatio"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Applies grayscale to an image, flattens it to an array and writes it to a string as comma seperated values.
        /// </summary>
        /// <param name="bmp">The image that will be normalized</param>
        /// <returns></returns>
		public static string Normalize(this Bitmap bmp)
		{
			string result = "";
			for (int y = 0; y < bmp.Height; y++)
			{
				for (int x = 0; x < bmp.Width; x++)
				{
					var pixel = bmp.GetPixel(y, x);
					var grayscale = 0.2126 * pixel.R + 0.7152 * pixel.G + 0.0722 * pixel.B;
					result += grayscale.ToString();
					if (x + 1 < bmp.Width) result += ",";
				}
				if (y + 1 < bmp.Height) result += ",";
			}

			return result;
		}

        /// <summary>
        /// Randomly shuffles the a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
		public static void Shuffle<T>(this IList<T> list)
		{
			Random rng = new Random();
			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k = rng.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}
	}
}
