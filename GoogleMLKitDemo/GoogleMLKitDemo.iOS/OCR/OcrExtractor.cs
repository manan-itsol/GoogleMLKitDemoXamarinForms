using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.MLKit.Vision;
using Foundation;
using GoogleMLKitDemo.iOS.OCR;
using GoogleMLKitDemo.OCR;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(OcrExtractor))]
namespace GoogleMLKitDemo.iOS.OCR
{
    public class OcrExtractor :IOcrExtractor
    {
        #region service methods
        public async Task<string> ProcessImageAsync(byte[] imageData)
        {
            using (var vision = VisionApi.Create())
            {
                using (VisionTextRecognizer textRecognizer = vision.GetOnDeviceTextRecognizer())
                {
                    var nsData = NSData.FromArray(imageData);
                    var uiImage = UIImage.LoadFromData(nsData);
                    VisionImage visionImage = new VisionImage(uiImage);
                    var textResult = await textRecognizer.ProcessImageAsync(visionImage);
                    var textBlocks = textResult.Blocks;
                    var text = ProcessText(textBlocks);
                    return text;
                }
            }
        }
        #endregion service methods

        #region helpers methods
        private string ProcessText(VisionTextBlock[] textBlocks)
        {
            List<TextExtractionModel> textExtractions = new List<TextExtractionModel>();
            for (int index = 0; index < textBlocks.Length; index++)
            {
                VisionTextBlock tBlock = textBlocks[index];
                foreach (var line in tBlock.Lines)
                {
                    var x = (int)line.Frame.X;
                    var y = (int)line.Frame.Y;
                    if (textExtractions.Count == 0)
                    {
                        textExtractions.Add(new TextExtractionModel
                        {
                            LinesList = new List<LineWithXY>
                            {
                                new LineWithXY(x, y, line.Text)
                            }
                        });
                    }
                    else
                    {
                        var nearest = GetNearest(textExtractions, y);
                        if (y >= nearest.CenterY - 15 && y <= nearest.CenterY + 15)
                        {
                            textExtractions.FirstOrDefault(a => a.CenterY == nearest.CenterY).LinesList.Add(new LineWithXY(x, y, line.Text));
                        }
                        else
                        {
                            if (textExtractions.Any(a => a.CenterY == y))
                            {
                                textExtractions.FirstOrDefault(a => a.CenterY == y).LinesList.Add(new LineWithXY(x, y, line.Text));
                            }
                            else
                            {
                                textExtractions.Add(new TextExtractionModel
                                {
                                    LinesList = new List<LineWithXY>
                                    {
                                        new LineWithXY(x,y,line.Text)
                                    }
                                });
                            }
                        }
                    }
                }
            }
            textExtractions = MergeNearestRows(textExtractions);
            string finalLines = string.Empty;
            foreach (var item in textExtractions.OrderBy(x => x.CenterY))
            {
                finalLines = $"{finalLines}{string.Join(' ', item.LinesList.OrderBy(x => x.X).Select(x => x.Text).ToList())}\n";
            }
            return finalLines;
        }

        private TextExtractionModel GetNearest(List<TextExtractionModel> textExtractions, int currentKey)
        {
            var sorted = textExtractions.OrderBy(x => x.CenterY).ToList();
            TextExtractionModel last = null;
            foreach (var item in sorted)
            {
                var less = currentKey < item.CenterY;
                if (less)
                {
                    last = item;
                }
                else
                {
                    if (last == null)
                        return item;
                    var lessDiff = currentKey - last.CenterY;
                    var greaterDiff = item.CenterY - currentKey;
                    if (lessDiff < greaterDiff)
                        return last;
                    else
                        return item;
                }
            }
            return last;
        }

        private List<TextExtractionModel> MergeNearestRows(List<TextExtractionModel> textExtractions)
        {
            textExtractions = textExtractions.OrderBy(x => x.CenterY).ToList();
            List<TextExtractionModel> toRemove = new List<TextExtractionModel>();
            TextExtractionModel last = null;
            foreach (var current in textExtractions)
            {
                if (last == null)
                {
                    last = current;
                    continue;
                }
                var diff = current.CenterY - last.CenterY;
                if (diff <= 20)
                {
                    current.LinesList.AddRange(last.LinesList);
                    toRemove.Add(last);
                }
                last = current;
            }
            foreach (var item in toRemove)
            {
                textExtractions.Remove(item);
            }
            return textExtractions;
        }
        #endregion helpers methods
    }

    public class TextExtractionModel
    {
        public TextExtractionModel()
        {
            LinesList = new List<LineWithXY>();
        }

        public float CenterY
        {
            get
                {
                float avg = 0;
                if (LinesList != null && LinesList.Count > 0)
                {
                    avg = LinesList.Sum(x => x.Y) / LinesList.Count;
                }
                return avg;
            }
        }
        public List<LineWithXY> LinesList { get; set; }
    }

    public class LineWithXY
    {
        public LineWithXY(int x, int y, string text)
        {
            X = x;
            Y = y;
            Text = text;
        }
        public int X { get; set; }
        public int Y { get; set; }
        public string Text { get; set; }
    }
}
