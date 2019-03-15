using System;
using System.IO;
using System.Collections.Generic;

namespace CompressBySepareting
{
    public class Compressor
    {
        private readonly int _partsCount;
        private readonly int _compressBufferSize;
        private readonly string _targetCompressedFile;
        private readonly object _locker = new object();

        public Compressor(int compressBufferSize, string targetCompressedFile, int partsCount)
        {
            _compressBufferSize = compressBufferSize;
            _targetCompressedFile = targetCompressedFile;
            _partsCount = partsCount;
        }

        public void PutAllCompressedFilesTogether()
        {
            var previousBuffer = new Dictionary<int, int>();
            for (int i = 0; i < _partsCount; i++)
            {
                Console.Write(".");
                long offset = i * (long)_compressBufferSize;
                var file = Archiver.FormNewFileName(_targetCompressedFile, offset);
                using (FileStream nextFile = new FileStream(file, FileMode.Open))
                {
                    var bufferMessage = new byte[nextFile.Length];
                    nextFile.Read(bufferMessage, 0, bufferMessage.Length);
                    previousBuffer.Add(i, bufferMessage.Length);
                    using (FileStream targetStream = new FileStream(_targetCompressedFile, FileMode.OpenOrCreate))
                    {
                        var localOffset = CountAllPreviouslyOffset(previousBuffer, i);
                        targetStream.Seek(localOffset, SeekOrigin.Begin);
                        targetStream.Write(bufferMessage, 0, bufferMessage.Length);
                    }
                }
                File.Delete(file);
            }
        }

        private static long CountAllPreviouslyOffset(Dictionary<int, int> previousBuffer, int i)
        {
            try
            {

                long offset = 0;
                if (i > 0)
                {
                    for (var j = 0; j < i; j++)
                    {
                        offset = offset + previousBuffer[j];
                    }
                    return offset;
                }
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadKey();
                throw;
            }
        }

        public void DeleteTemporaryFiles()
        {
            lock (_locker)
            {
                for (int i = 0; i < _partsCount; i++)
                {
                    Console.Write(".");
                    long offset = i * (long)_compressBufferSize;
                    var file = Archiver.FormNewFileName(_targetCompressedFile, offset);
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                }
            }
        }

    }
}
