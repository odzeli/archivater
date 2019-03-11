using System;

namespace CompressBySepareting
{
    class Program
    {
        static void Main(string[] args)
        {
            //Please, use follow pattern to compress or decompress file:
            //GZipTest.exe compress [Source file path] [Destination file path]
            //If you use app from VS with debug then you should to uncomment args below
            //args = new string[3];
            //args[0] = @"decompress";
            //args[1] = @"D:\text2.gz";
            //args[2] = @"D:\text2.txt";

            try
            {
                Validator.CheckInputFileNames(args);

                var file = new Archiver();
                switch (args[0].ToLower())
                {
                    case "compress":
                        file.StartCompress(args[1], args[2]);
                        break;
                    case "decompress":
                        file.StartDecompress(args[1], args[2]);
                        break;
                }
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadKey();
            }
        }
    }
}
