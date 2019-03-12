using System;
using System.IO;
using System.Collections.Generic;

namespace CompressBySepareting
{
    public class Compressor 
    {
        private readonly int _compressBufferSize;
        private readonly string _targetCompressedFile;

        public Compressor( int compressBufferSize, string targetCompressedFile) 
        {
            _compressBufferSize = compressBufferSize;
            _targetCompressedFile = targetCompressedFile;
        }

        public void PutAllCompressedFilesTogether(int partsCount)
        {
            try
            {
                var previousBuffer = new Dictionary<int, int>();
                for (int i = 0; i < partsCount; i++)
                {
                    Console.Write(".");
                    long offset = i * _compressBufferSize;
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
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadKey();
            }
        }

        private static long CountAllPreviouslyOffset(Dictionary<int, int> previousBuffer, int i)
        {
            try
            {

                long offset = 0;
                if (i > 0)
                {
                    for (int j = 0; j < i; j++)
                    {
                        offset = offset + previousBuffer[j];
                    }
                    return offset;
                }
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadKey();
                throw;
            }
        }
    }
}
