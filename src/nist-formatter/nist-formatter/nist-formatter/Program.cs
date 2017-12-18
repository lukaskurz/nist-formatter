using System;
using System.IO;
using System.Linq;

namespace NistFormatter
{
    class Program
    {
        static void Main(string[] args)
        {
			var directories = Directory.GetDirectories(@".\..\..\..\..\nist\by_class");
			foreach (var directory in directories) { 
				var files = Directory.GetDirectories(directory).Where(text => text.Split('\\').Last().StartsWith("train_"));
				foreach (var file in files)
				{
					Console.WriteLine(file); 
				}
			}
			Console.ReadKey();
        }
    }
}
