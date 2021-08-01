using System;
using BigTed;
using GoogleMLKitDemo.iOS.Services;
using GoogleMLKitDemo.Services;

[assembly: Xamarin.Forms.Dependency(typeof(LoadingService))]
namespace GoogleMLKitDemo.iOS.Services
{
    public class LoadingService: ILoadingService
    {
        public LoadingService()
        {
            BTProgressHUD.ForceiOS6LookAndFeel = true;
        }
        public void Hide()
        {
            BTProgressHUD.Dismiss();
        }

        public void Show(string message = "Loading")
        {
            BTProgressHUD.Show(maskType: ProgressHUD.MaskType.Clear);
        }

        public void ShowSuccess(string message = "Loading")
        {
            BTProgressHUD.ShowSuccessWithStatus("");
        }


        public void ShowToast(string message = "")
        {
            throw new NotImplementedException();
        }
    }
}
