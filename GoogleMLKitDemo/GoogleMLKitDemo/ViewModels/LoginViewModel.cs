using GoogleMLKitDemo.OCR;
using GoogleMLKitDemo.Views;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace GoogleMLKitDemo.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        public Command LoginCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new Command(OnLoginClicked);
        }

        private async void OnLoginClicked(object obj)
        {
            // Prefixing with `//` switches to a different navigation stack instead of pushing to the active one
            //await Shell.Current.GoToAsync($"//{nameof(AboutPage)}");


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
            //file.GetStreamWithImageRotatedForExternalStorage();
            if (file == null)
                return;

            Stream stream;
            if (Device.RuntimePlatform == Device.iOS)
            {
                stream = file.GetStream();//GetStreamWithImageRotatedForExternalStorage();
            }
            else
            {
                stream = file.GetStream();
            }
            byte[] imageData;
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                imageData = ms.ToArray();
            }


            var ocrExtractor = DependencyService.Get<IOcrExtractor>();
            ocrExtractor.ProcessImage(imageData);


        }

    }
}
