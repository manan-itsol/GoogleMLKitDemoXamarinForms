using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMLKitDemo.OCR
{
    public interface IOcrExtractor
    {
        (string blocks, string lines) ProcessImage(byte[] imageData);
    }
}
