using PCLStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMLKitDemo.Helpers
{
    public class FileHelper
    {
        public async static Task<byte[]> GetBytesAsync(string filePath)
        {
            // get hold of the file system  
            IFolder folder = FileSystem.Current.LocalStorage;

            //open file if exists  
            IFile file = await folder.GetFileAsync(filePath);
            //load stream to buffer  
            using (System.IO.Stream stream = await file.OpenAsync(FileAccess.ReadAndWrite))
            {
                long length = stream.Length;
                byte[] streamBuffer = new byte[length];
                stream.Read(streamBuffer, 0, (int)length);
                return streamBuffer.ToArray();
            }

        }
    }
}
