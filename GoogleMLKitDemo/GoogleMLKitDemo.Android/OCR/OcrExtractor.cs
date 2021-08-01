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
        public string ProcessImageAsync(byte[] imageData)
        {
            try
            {
                Bitmap bitmap = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);
                var textRecognizer = new TextRecognizer.Builder(Application.Context).Build();
                Frame imageFrame = new Frame.Builder().SetBitmap(bitmap).Build();
                SparseArray textBlocks = textRecognizer.Detect(imageFrame);

                var textResult = ProcessText(textBlocks);
                textRecognizer.Release();
                return textResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string ProcessText(SparseArray textBlocks)
        {
            Dictionary<int, List<(int x, int y, string text)>> ss = new Dictionary<int, List<(int x, int y, string text)>>();
            for (int index = 0; index < textBlocks.Size(); index++)
            {
                TextBlock tBlock = (TextBlock)textBlocks.ValueAt(index);
                foreach (var line in tBlock.Components)
                {
                    var x = line.BoundingBox.CenterX();
                    var y = line.BoundingBox.CenterY();
                    if (ss.Count == 0)
                    {
                        ss.Add(y, new List<(int x, int y, string text)> { (x, y, line.Value) });
                    }
                    else
                    {
                        var last = ss.LastOrDefault();
                        if (y >= last.Key - 15 && y <= last.Key + 15)
                        {
                            ss.Where(x => x.Key == last.Key).FirstOrDefault().Value.Add((x, y, line.Value));
                        }
                        else
                        {
                            if (ss.Any(x => x.Key == y))
                            {
                                ss.Where(x => x.Key == y).FirstOrDefault().Value.Add((x, y, line.Value));
                            }
                            else
                            {
                                ss.Add(y, new List<(int x, int y, string text)> { (x, y, line.Value) });
                            }
                        }
                    }
                }
            }
            string finalLines = string.Empty;
            int count = 1;
            foreach (var item in ss.OrderBy(x => x.Key))
            {
                finalLines = $"{finalLines}Row{count}: {string.Join(' ', item.Value.OrderBy(x => x.x).Select(x => x.text).ToList())}\n";
                count++;
            }
            return finalLines;
        }
    }
}