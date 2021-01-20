using GoogleMLKitDemo.Helpers;
using GoogleMLKitDemo.OCR;
using Plugin.Media;
using System;
using System.IO;
using System.Windows.Input;
using Tesseract;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace GoogleMLKitDemo.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel()
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

        private string _mlExtractedBlocks;
        public string MLExtractedBlocks
        {
            get => _mlExtractedBlocks;
            set
            {
                SetProperty(ref _mlExtractedBlocks, value);
            }
        }

        private string _mlExtractedLines;
        public string MLExtractedLines
        {
            get => _mlExtractedLines;
            set
            {
                SetProperty(ref _mlExtractedLines, value);
            }
        }

        private async void OnCaptureClicked(object obj)
        {
            var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium,
                Directory = "Sample",
                Name = "ATM_Receipt.jpg",
                RotateImage = Device.RuntimePlatform != Device.iOS,
                // MaxWidthHeight = maxWidthHeight,
                DefaultCamera = Plugin.Media.Abstractions.CameraDevice.Rear,

                //CompressionQuality = quality,
                AllowCropping = true,
                ModalPresentationStyle = Plugin.Media.Abstractions.MediaPickerModalPresentationStyle.FullScreen,

                // OverlayViewProvider = func
            });
            if (file == null)
                return;
            Stream stream = file.GetStream();
            byte[] imageData;
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                imageData = ms.ToArray();
            }
            LoadingHelper.Show("Please wait");
            try
            {
                var tesseractApi = DependencyService.Get<ITesseractApi>();
                if (!tesseractApi.Initialized)
                {
                    var initResult = await tesseractApi.Init("eng");
                }
                ExtractedText = string.Empty;
                var result = await tesseractApi.SetImage(imageData);
                if (result)
                {
                    ExtractedText = tesseractApi.Text;
                }
            }
            catch (Exception ex)
            {
            }

            // google ml kit text recognition commented
            var ocrExtractor = DependencyService.Get<IOcrExtractor>();
            var mlResult = ocrExtractor.ProcessImage(imageData);
            MLExtractedLines = mlResult.lines;
            MLExtractedBlocks = mlResult.blocks;
            LoadingHelper.Hide();
        }
    }
}