using Abbyy.CloudSdk.V2.Client;
using Abbyy.CloudSdk.V2.Client.Models;
using Abbyy.CloudSdk.V2.Client.Models.Enums;
using Abbyy.CloudSdk.V2.Client.Models.RequestParams;
using GoogleMLKitDemo.Helpers;
using GoogleMLKitDemo.OCR;
using Plugin.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace GoogleMLKitDemo.ViewModels
{
    public class TextRecognitionViewModel : BaseViewModel
    {
        //private const string ApplicationId = @"b0a71d6d-2dde-441e-93d6-254d454af97f"; //A Mannan
        //private const string Password = @"F3Y93kmd5BfVT9e/LnwBtFPa"; //A Mannan

        private const string ApplicationId = @"c203a13c-bce9-4c49-8dc2-5875ca6d648b"; //A Mannan
        private const string Password = @"JN3ctQLEOq+SQi6d2aktEu9W"; //A Mannan

        

        public TextRecognitionViewModel()
        {
            Title = "About";
            OpenWebCommand = new Command(async () => await Browser.OpenAsync("https://aka.ms/xamarin-quickstart"));
            CaptureCommand = new Command(OnCaptureClicked);
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
            var type = obj as string;
            var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Full,
                Directory = "Sample",
                Name = "ATM_Receipt.jpg",
                RotateImage = Device.RuntimePlatform != Device.iOS,
                // MaxWidthHeight = maxWidthHeight,
                DefaultCamera = Plugin.Media.Abstractions.CameraDevice.Rear,

                //CompressionQuality = quality,
                AllowCropping = true,
                ModalPresentationStyle = Plugin.Media.Abstractions.MediaPickerModalPresentationStyle.FullScreen,
            });
            if (file == null)
                return;
            byte[] imageData;
            Stream stream = file.GetStream();
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                imageData = ms.ToArray();
            }

            LoadingHelper.Show("Please wait");
            if(type == "Abbyy")
            {
                await ExtractTextAbbyy(imageData);
            }
            else
            {
                ExtractTextMlKit(imageData);
            }
            LoadingHelper.Hide();
        }

        private void ExtractTextMlKit(byte[] image)
        {
            #region ml-kit text recognition
            // google ml kit text recognition commented
            var ocrExtractor = DependencyService.Get<IOcrExtractor>();
            var mlResult = ocrExtractor.ProcessImage(image);
            ExtractedText = mlResult;
            #endregion ml-kit text recognition
        }

        private async Task ExtractTextAbbyy(byte[] image)
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
        }
    }
}