using System;
using System.IO;
using System.IO.Compression;

namespace CompressBySepareting
{
    public class BlockCompressor
    {

        private readonly long _offset;
        private readonly object _locker;
        private readonly string _sourceFile;
        private readonly int _compressBufferSize;
        private readonly string _targetCompressedFile;
        private readonly int _part;

        public BlockCompressor(string sourceFile, int compressBufferSize, object locker, long offset,
            string targetCompressedFile, int part)
        {
            _part = part;
            _locker = locker;
            _offset = offset;
            _sourceFile = sourceFile;
            _compressBufferSize = compressBufferSize;
            _targetCompressedFile = targetCompressedFile;
        }

        public void Compressing()
        {
            Console.Write(".");
            try
            {
                using (var sourceStream = new FileStream(_sourceFile, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
                {
                    var newFile = Archiver.FormNewFileName(_targetCompressedFile, _offset);
                    using (var targetStream = new FileStream(newFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write))
                    {
                        sourceStream.Seek(_offset, SeekOrigin.Begin);
                        var remainingLength = sourceStream.Length - _offset;
                        var bufferMessage = remainingLength < _compressBufferSize ? new byte[remainingLength] : new byte[_compressBufferSize];
                        sourceStream.Read(bufferMessage, 0, bufferMessage.Length);
                        using (var memoryStream = new MemoryStream())
                        {
                            using (var compressionStream = new GZipStream(memoryStream, CompressionMode.Compress))
                            {
                                compressionStream.Write(bufferMessage, 0, bufferMessage.Length);
                            }

                            var compressedBlock = memoryStream.ToArray();
                            AddMetaDataToBlock(ref compressedBlock);
                            targetStream.Write(compressedBlock, 0, compressedBlock.Length);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadKey();
            }
        }

        private static void AddMetaDataToBlock(ref byte[] compressedBlock)
        {
            var blocksLong = BitConverter.GetBytes(compressedBlock.Length);
            Array.Resize(ref blocksLong, 4);
            var i = 4;
            foreach (var item in blocksLong)
            {
                compressedBlock[i] = item;
                i++;
            }
        }
    }
}
