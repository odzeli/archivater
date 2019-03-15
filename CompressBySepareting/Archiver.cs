using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace CompressBySepareting
{
    class Archiver
    {
        private static int _bufferSize = 1000000;

        public void StartCompress(string sourceFile, string targetCompressedFile)
        {
            var threadPool = new PoolOfThread();

            var fs = new FileInfo(sourceFile);
            if (fs.Length == 0)
            {
                Console.WriteLine("You try to archive empty file.");
                return;
            }
            var partsCount = (int)(fs.Length / _bufferSize);
            partsCount = fs.Length % _bufferSize != 0 ? ++partsCount : partsCount;

            Console.WriteLine("Compressing started");
            var compressor = new Compressor(_bufferSize, targetCompressedFile, partsCount);
            for (var i = 0; i < partsCount;)
            {
                long offset = i * (long)_bufferSize;
                var blockCompress = new BlockCompressor(sourceFile, _bufferSize, offset, targetCompressedFile);
                Thread myThread = new Thread(blockCompress.Compressing) { Name = i.ToString() };
                threadPool.Add(myThread);
                i++;
            }
            threadPool.Wait();
            compressor.PutAllCompressedFilesTogether();
            Console.WriteLine($"\nParts count: {partsCount}");
        }

        public void StartDecompress(string sourceCompresedFile, string targetDecompressedFile)
        {
            try
            {
                var threadPool = new PoolOfThread();
                var compressedBufferHistory = Decompressor.CountCompressedBufferHistory(sourceCompresedFile);
                Console.WriteLine("Decompressing started...");
                for (var i = 0; i < compressedBufferHistory.Count;)
                {
                    var blockDecompress = new BlockDecompressor(sourceCompresedFile, compressedBufferHistory, targetDecompressedFile, i);
                    Thread myThread = new Thread(blockDecompress.Decompressing) { Name = i.ToString() };
                    threadPool.Add(myThread);
                    i++;
                }
                threadPool.Wait();
                Console.WriteLine($"\nParts count: {compressedBufferHistory.Count}");
            }
            catch (NullReferenceException e)
            {
                throw e;
            }
        }

        public static string FormNewFileName(string fullPath, long offset)
        {
            return $"{fullPath}-FROM-{offset}-TO-{offset + _bufferSize}.gz";
        }
    }
}
