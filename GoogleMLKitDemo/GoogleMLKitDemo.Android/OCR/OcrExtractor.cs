using Android.App;
using Android.Content;
using Android.Gms.Vision;
using Android.Gms.Vision.Texts;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
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

        public void Main()
        {
            var textRecognizer = new TextRecognizer.Builder(Application.Context).Build();
            //textRecognizer.Detect(new Frame())


            //Frame imageFrame = new Frame.Builder().SetBitmap().build();
            //SparseArray<TextBlock> textBlocks = textRecognizer.Detect(imageFrame);
            //for (int i = 0; i < textBlocks.size(); i++)
            //{
            //    TextBlock textBlock = textBlocks.Get(textBlocks.KeyAt(i));
            //    result.add(textBlock);
            //}
            //textRecognizer.release();
            //return result;
        }

        public void ProcessImage(byte[] imageData)
        {
            try
            {
                Bitmap bitmap = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);
                List<TextBlock> result = new List<TextBlock>();
                var textRecognizer = new TextRecognizer.Builder(Application.Context).Build();
                Frame imageFrame = new Frame.Builder().SetBitmap(bitmap).Build();
                SparseArray textBlocks = textRecognizer.Detect(imageFrame);
                ProcessText(textBlocks);
                for (int i = 0; i < textBlocks.Size(); i++)
                {
                    var test = textBlocks.Get(textBlocks.KeyAt(i));
                    TextBlock textBlock = (TextBlock)textBlocks.Get(textBlocks.KeyAt(i));
                    result.Add(textBlock);
                }
                textRecognizer.Release();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void ProcessText(SparseArray textBlocks)
        {
            String blocks = "";
            String lines = "";
            for (int index = 0; index < textBlocks.Size(); index++)
            {
                TextBlock tBlock = (TextBlock)textBlocks.ValueAt(index);
                blocks = blocks + tBlock.Value + "\n";
                foreach (var line in tBlock.Components)
                {
                    lines = lines + line.Value + "\n";
                }
            }

            if (textBlocks.Size() == 0)
            {
                // Log.d(TAG, "getTextFromBitmap: Scan Failed: Found nothing to scan");
                //return new String[] { "Scan Failed: Found nothing to scan" };
            }
            else
            {
                String[] textOnScreen = lines.Split("\n");
                int lineCount = textOnScreen.Length;
                if (lineCount > 3)
                {
                    String question = "";
                    for (int i = 0; i < lineCount - 3; i++)
                    {
                        question += textOnScreen[i];
                    }
                    var sa = new String[] { question, textOnScreen[lineCount - 3], textOnScreen[lineCount - 2], textOnScreen[lineCount - 1] };

                }
                var ss = new String[] { "Scan Failed: Could not read options" };

            }
        }
    }
}