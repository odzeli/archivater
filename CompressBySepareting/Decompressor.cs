using System;
using System.Collections.Generic;
using System.IO;

namespace CompressBySepareting
{
    public class Decompressor
    {
        public static Dictionary<int, BlockDetails> CountCompressedBufferHistory(string sourceCompressedFile)
        {
            try
            {
                int count = 0;
                Dictionary<int, BlockDetails> compressedBufferHistory = new Dictionary<int, BlockDetails>();
                using (FileStream streamToDecompress = new FileStream(sourceCompressedFile, FileMode.Open))
                {
                    while (streamToDecompress.Position < streamToDecompress.Length - 1)
                    {
                        var bufferForMetaData = new byte[8];
                        streamToDecompress.Read(bufferForMetaData, 0, bufferForMetaData.Length);
                        var bufferInfo = new BlockDetails { IndexOffsetOfCompressedFileBlock = BitConverter.ToInt32(bufferForMetaData, 4) };
                        var compressedBlock = new byte[bufferInfo.IndexOffsetOfCompressedFileBlock];
                        bufferForMetaData.CopyTo(compressedBlock, 0);
                        streamToDecompress.Read(compressedBlock, bufferForMetaData.Length, compressedBlock.Length - 8);
                        bufferInfo.IndexOffsetForDecompressFileBlock = BitConverter.ToInt32(compressedBlock, bufferInfo.IndexOffsetOfCompressedFileBlock - 4);
                        //it means that size of archive bigger then inner file's size and remaining bytes are empty
                        if (bufferInfo.IndexOffsetForDecompressFileBlock != 0)
                        {
                            compressedBufferHistory.Add(count, bufferInfo);
                        }
                        count++;
                    }
                }
                return compressedBufferHistory;
            }
            catch (UnauthorizedAccessException)
            {
                throw new UnauthorizedAccessException($"App hasn't acces to file {sourceCompressedFile}");
            }
        }

    }
}
