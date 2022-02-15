using Abbyy.CloudSdk.V2.Client;
using Abbyy.CloudSdk.V2.Client.Models;
using Abbyy.CloudSdk.V2.Client.Models.Enums;
using Abbyy.CloudSdk.V2.Client.Models.RequestParams;
using GoogleMLKitDemo.Helpers;
using GoogleMLKitDemo.OCR;
using MobileCapture.Core.Common;
using MobileCapture.Forms.Common;
using Newtonsoft.Json;
using Plugin.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using ImageSource = Xamarin.Forms.ImageSource;

namespace GoogleMLKitDemo.ViewModels
{
    public class TextRecognitionViewModel : BaseViewModel
    {
        //private const string ApplicationId = @"b0a71d6d-2dde-441e-93d6-254d454af97f"; //A Mannan
        //private const string Password = @"F3Y93kmd5BfVT9e/LnwBtFPa"; //A Mannan

        private const string ApplicationId = @"73ed1f01-6d44-4cf6-9433-3cae6a6d6534"; //A Mannan
        private const string Password = @"WH7+7I21+WxHuuszp+VbNB7i"; //A Mannan
        private string LicenseName => "MobileCapture.License";

        private readonly IEngine coreApiEngine;

        public TextRecognitionViewModel()
        {
            Title = "About";
            OpenWebCommand = new Command(async () => await Browser.OpenAsync("https://aka.ms/xamarin-quickstart"));
            CaptureCommand = new Command(OnCaptureClicked);
            coreApiEngine = DependencyService.Get<IEngineFactory>().GetEngine(LicenseName);
        }

        public ICommand OpenWebCommand { get; }

        public Command CaptureCommand { get; }

        private string _extractedText;
        public string ExtractedText
        {
            get => _extractedText;
            set
            {
                SetProperty(ref _extractedText, value);
            }
        }

        private string _amcExtractedText;
        public string AmcExtractedText
        {
            get => _amcExtractedText;
            set
            {
                SetProperty(ref _amcExtractedText, value);
            }
        }

        private ImageSource _imageSource;
        public ImageSource ImageSource
        {
            get => _imageSource;
            set
            {
                _imageSource = value;
                OnPropertyChanged(nameof(ImageSource));
            }
        }

        private Xamarin.Forms.ImageSource _capturedImage;
        public Xamarin.Forms.ImageSource CapturedImage
        {
            get => _capturedImage;
            set
            {
                SetProperty(ref _capturedImage, value);
            }
        }

        private async void OnCaptureClicked(object obj)
        {
            IsBusy = true;
            byte[] imageData = null;
            try
            {
                var captureSettings = new ImageCaptureSettings
                {
                    RequiredPageCount = 1,
                    PageCompressionLevel = ImageCaptureSettings.CompressionLevel.Low
                };
                var captureResult = await DependencyService.Get<ICapture>().StartImageCaptureAsync(
                    null,
                    null,
                    captureSettings);
                if (captureResult != null)
                {
                    var imagePath = captureResult.Pages[0].ImagePath;
                    ImageSource = ImageSource.FromFile(imagePath);
                    imageData = await FileHelper.GetBytesAsync(imagePath);
                    PerformRecognition();
                }
            }
            catch (TaskCanceledException)
            {
                await App.Current.MainPage.DisplayAlert(null, "Task was canceled manually by user", "OK");
            }
            catch (Exception e)
            {
                await App.Current.MainPage.DisplayAlert("Something went wrong", e.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }


            var type = obj as string;
            //var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            //{
            //    PhotoSize = Plugin.Media.Abstractions.PhotoSize.Full,
            //    Directory = "Sample",
            //    Name = "ATM_Receipt.jpg",
            //    RotateImage = Device.RuntimePlatform != Device.iOS,
            //    // MaxWidthHeight = maxWidthHeight,
            //    DefaultCamera = Plugin.Media.Abstractions.CameraDevice.Rear,

            //    //CompressionQuality = quality,
            //    AllowCropping = true,
            //    ModalPresentationStyle = Plugin.Media.Abstractions.MediaPickerModalPresentationStyle.FullScreen,
            //});
            //if (file == null)
            //    return;
            //Stream stream = file.GetStream();
            //using (MemoryStream ms = new MemoryStream())
            //{
            //    stream.CopyTo(ms);
            //    imageData = ms.ToArray();
            //}

            LoadingHelper.Show("Please wait");
            if (type == "Abbyy")
            {
                await ExtractTextAbbyy(imageData);
            }
            else
            {
                await ExtractTextMlKit(imageData);
            }
            LoadingHelper.Hide();
        }

        private async void PerformRecognition()
        {
            if (ImageSource == null)
            {
                return;
            }
            var recognitionType = (RecognitionType)Enum.Parse(typeof(RecognitionType), "Text");
            try
            {
                await RecognizeText(ImageSource, new HashSet<RecognitionLanguage> { RecognitionLanguage.English });
            }
            catch (Exception e)
            {
                await App.Current.MainPage.DisplayAlert("Something went wrong", e.Message, "OK");
            }
        }

        private async Task RecognizeText(ImageSource source, ISet<RecognitionLanguage> languages)
        {
            var textCaptureCoreAPI = coreApiEngine.CreateTextCaptureCoreAPI();
            textCaptureCoreAPI.Settings.Languages = languages;
            var result = await textCaptureCoreAPI.RecognizeAsync(
                    source,
                    ProgressHandler,
                    orientation =>
                    {
                        // handle orientation
                    });

            var json = JsonConvert.SerializeObject(result);

            //var lines = result
            //    .SelectMany(block => block.Lines.Select(line => line.Text))
            //    .Distinct();
            //ResultRepresentation = string.Join("\n\n", lines.ToArray());

            AmcExtractedText = ProcessTextBlocks(result);
            await Clipboard.SetTextAsync(AmcExtractedText);
        }


        private async Task ExtractTextMlKit(byte[] image)
        {
            #region ml-kit text recognition
            // google ml kit text recognition commented
            var ocrExtractor = DependencyService.Get<IOcrExtractor>();
            var mlResult = await ocrExtractor.ProcessImageAsync(image);
            ExtractedText = mlResult;
            await Clipboard.SetTextAsync(ExtractedText);
            #endregion ml-kit text recognition
        }

        private async Task ExtractTextAbbyy(byte[] image)
        {
            try
            {
                var authInfo = new AuthInfo
                {
                    Host = "https://cloud-westus.ocrsdk.com/v2",
                    ApplicationId = ApplicationId,
                    Password = Password,
                };
                var client = new OcrClient(authInfo);
                var imageParams = new ImageProcessingParams
                {
                    ExportFormats = new[] { ExportFormat.Txt },
                    Language = "English",
                    Profile = ProcessingProfile.TextExtraction,
                    TextTypes = new[] { Abbyy.CloudSdk.V2.Client.Models.Enums.TextType.OcrB, Abbyy.CloudSdk.V2.Client.Models.Enums.TextType.OcrA, Abbyy.CloudSdk.V2.Client.Models.Enums.TextType.Normal },
                    ImageSource = Abbyy.CloudSdk.V2.Client.Models.Enums.ImageSource.Auto,
                    //ExportParagraphAsOneLine = true
                };
                var taskInfo = await client.ProcessImageAsync(imageParams, new MemoryStream(image), "generatedImagepixmap.jpg", waitTaskFinished: true);
                foreach (var resultUrl in taskInfo.ResultUrls)
                {
                    HttpClient httpClient = new HttpClient();
                    HttpResponseMessage response = await httpClient.GetAsync(resultUrl);
                    string strContent = await response.Content.ReadAsStringAsync();
                    ExtractedText = strContent;
                }
                await Clipboard.SetTextAsync(ExtractedText);
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Something went wrong", ex.Message, "OK");
            }
        }

        private float _recognitionProgress;
        public float RecognitionProgress
        {
            get => _recognitionProgress;
            set
            {
                _recognitionProgress = value;
                OnPropertyChanged(nameof(RecognitionProgress));
            }
        }

        private string _warningMessage;
        public string WarningMessage
        {
            get => _warningMessage;
            set
            {
                _warningMessage = value;
                OnPropertyChanged(nameof(WarningMessage));
            }
        }

        bool ProgressHandler(int progress, RecognitionWarning warning)
        {
            RecognitionProgress = progress / 100f;
            switch (warning)
            {
                case RecognitionWarning.NoWarning:
                    WarningMessage = null;
                    break;
                case RecognitionWarning.ProbablyLowQualityImage:
                    WarningMessage = "The image probably has low quality.";
                    break;
                case RecognitionWarning.ProbablyWrongLanguage:
                    WarningMessage = "The chosen recognition language is probably wrong.";
                    break;
                case RecognitionWarning.RecognitionIsSlow:
                    WarningMessage = "The image is being recognized too slowly, perhaps something is going wrong.";
                    break;
                case RecognitionWarning.TextTooSmall:
                    WarningMessage = "Text is too small.";
                    break;
                case RecognitionWarning.WrongLanguage:
                    WarningMessage = "The chosen recognition language is wrong.";
                    break;
                default: break;
            }
            return true;
        }


        #region text extraction mobile capture processing

        private string ProcessTextBlocks(List<TextBlock> textBlocks)
        {
            var lines = textBlocks.SelectMany(x => x.Lines).ToList();
            List<TextExtractionModel> textExtractions = new List<TextExtractionModel>();
            foreach (var line in lines)
            {
                var x = line.BoundingRect.Center.X;
                var y = line.BoundingRect.Center.Y;
                if (textExtractions.Count == 0)
                {
                    textExtractions.Add(new TextExtractionModel
                    {
                        LinesList = new List<LineWithXY>
                            {
                                new LineWithXY(x,y,line.Text)
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

            textExtractions = MergeNearestRows(textExtractions);
            string finalLines = string.Empty;
            foreach (var item in textExtractions.OrderBy(x => x.CenterY))
            {
                finalLines = $"{finalLines}{string.Join(" ", item.LinesList.OrderBy(x => x.X).Select(x => x.Text).ToList())}\n";
            }
            return finalLines;
        }

        private TextExtractionModel GetNearest(List<TextExtractionModel> textExtractions, double currentKey)
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
        #endregion text extraction mobile capture processing
    }

    public enum RecognitionType
    {
        Text,
        BCR
    }

    public class TextExtractionModel
    {
        public TextExtractionModel()
        {
            LinesList = new List<LineWithXY>();
        }

        public double CenterY
        {
            get
            {
                double avg = 0;
                if (LinesList != null && LinesList.Count > 0)
                {
                    avg = LinesList.Sum(a => a.Y) / LinesList.Count;
                }
                return avg;
            }
        }
        public List<LineWithXY> LinesList { get; set; }
    }

    public class LineWithXY
    {
        public LineWithXY(double x, double y, string text)
        {
            X = x;
            Y = y;
            Text = text;
        }
        public double X { get; set; }
        public double Y { get; set; }
        public string Text { get; set; }
    }
}