using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;

namespace CompressBySepareting
{
    class Validator
    {
        public static void CheckInputFileNames(string[] args)
        {
            if (args.Length == 0 || args.Length > 3)
            {
                throw new Exception("Please, follow for the next pattern: GZipTest.exe compress/decompress [source file name] [target file name]");
            }

            if (string.IsNullOrEmpty(args[0]) || string.IsNullOrEmpty(args[1]) || string.IsNullOrEmpty(args[1]) || string.IsNullOrEmpty(args[2]))
            {
                throw new Exception("Some argument not filled. Please, follow for the next pattern: GZipTest.exe compress/decompress [source file name] [target file name]");
            }

            FileInfo sourceFile = new FileInfo(args[1]);
            FileInfo targetFile = new FileInfo(args[2]);

            if (!sourceFile.Exists)
            {
                throw new Exception($"Application can't find source file '{sourceFile.FullName}' for process it.");
            }

            if (targetFile.Exists)
            {
                Console.WriteLine($"Warning: this file '{targetFile.FullName}' exist already. It will be overwritten.");
                Console.WriteLine("Press 'Y' to continue and 'N' to cancel.");
                var answer = Console.ReadLine();
                while (answer == null || (answer.ToLower() != "y" && answer.ToLower() != "n"))
                {
                    Console.WriteLine("Press 'Y' to continue and 'N' to cancel.");
                    answer = Console.ReadLine();
                }

                if (answer.ToLower() == "y")
                {
                    Console.WriteLine("We continue process");
                }
                else
                {
                    throw new Exception("Press any button to exit");
                }
            }
            if (args[0] == "compress")
            {
                if (sourceFile.Extension == ".gz")
                {
                    throw new Exception("File was archivated before");
                }

                if (targetFile.Extension != ".gz")
                {
                    throw new Exception("Target file's type is wrong. Please, replace file and try again.");
                }
            }
            if (args[0] == "decompress")
            {
                if (sourceFile.Extension != ".gz")
                {
                    throw new Exception("Source file's type is wrong. Please, replace file and try again.");
                }

                if (targetFile.Extension == ".gz")
                {
                    throw new Exception("Target file's type is wrong. Please, replace file and try again.");
                }
            }
        }
    }
}
