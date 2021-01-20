using Android.App;
using Android.Content;
using Android.Gms.Tasks;
using Android.Gms.Vision;
using Android.Gms.Vision.Texts;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Firebase.ML.Vision;
using Firebase.ML.Vision.Common;
using Firebase.ML.Vision.Document;
using GoogleMLKitDemo.Droid.OCR;
using GoogleMLKitDemo.OCR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.Platform.Android;

[assembly: Xamarin.Forms.Dependency(typeof(OcrExtractor))]
namespace GoogleMLKitDemo.Droid.OCR
{
    public class OcrExtractor : IOcrExtractor
    {
        public (string blocks,string lines) ProcessImage(byte[] imageData)
        {
            try
            {
                Bitmap bitmap = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);
                List<TextBlock> result = new List<TextBlock>();
                var textRecognizer = new TextRecognizer.Builder(Application.Context).Build();
                Frame imageFrame = new Frame.Builder().SetBitmap(bitmap).Build();
                SparseArray textBlocks = textRecognizer.Detect(imageFrame);

                var text = ProcessText(textBlocks);
                //for (int i = 0; i < textBlocks.Size(); i++)
                //{
                //    var test = textBlocks.Get(textBlocks.KeyAt(i));
                //    TextBlock textBlock = (TextBlock)textBlocks.Get(textBlocks.KeyAt(i));
                //    result.Add(textBlock);
                //}
                textRecognizer.Release();
                return text;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private (string blocks, string lines) ProcessText(SparseArray textBlocks)
        {
            String blocks = "";
            String lines = "";
            for (int index = 0; index < textBlocks.Size(); index++)
            {
                TextBlock tBlock = (TextBlock)textBlocks.ValueAt(index);
                var boundingBox = tBlock.BoundingBox;
                blocks = blocks + tBlock.Value + $"({boundingBox.Bottom},{boundingBox.Left},{boundingBox.Top},{boundingBox.Right})\n";
                foreach (var line in tBlock.Components)
                {
                    lines = lines + line.Value + "\n";
                }
            }
            return (blocks,lines);

            //if (textBlocks.Size() == 0)
            //{
            //    // Log.d(TAG, "getTextFromBitmap: Scan Failed: Found nothing to scan");
            //    //return new String[] { "Scan Failed: Found nothing to scan" };
            //}
            //else
            //{
            //    String[] textOnScreen = lines.Split("\n");
            //    int lineCount = textOnScreen.Length;
            //    if (lineCount > 3)
            //    {
            //        String question = "";
            //        for (int i = 0; i < lineCount - 3; i++)
            //        {
            //            question += textOnScreen[i];
            //        }
            //        var sa = new String[] { question, textOnScreen[lineCount - 3], textOnScreen[lineCount - 2], textOnScreen[lineCount - 1] };

            //    }
            //    var ss = new String[] { "Scan Failed: Could not read options" };

            //}
        }
    }
}