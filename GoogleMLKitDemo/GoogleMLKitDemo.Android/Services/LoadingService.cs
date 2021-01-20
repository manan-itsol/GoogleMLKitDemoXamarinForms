using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using GoogleMLKitDemo.Droid.Services;
using GoogleMLKitDemo.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(LoadingService))]
namespace GoogleMLKitDemo.Droid.Services
{
    public class LoadingService : ILoadingService
    {
        public LoadingService()
        {
        }
        public void Show(string message = "")
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                //Show a simple status message with an indeterminate spinner
                AndHUD.Shared.Show(Forms.Context, message, maskType: MaskType.Black);
            });
        }

        //public void ShowToast(string message = "")
        //{
        //    Device.BeginInvokeOnMainThread(() =>
        //    {
        //        //Show a simple status message with an indeterminate spinner
        //        AndHUD.Shared.ShowToast(Forms.Context, message, MaskType.Clear, TimeSpan.FromSeconds(2));
        //    });
        //}
        public void ShowSuccess(string message = "Done")
        {
            AndHUD.Shared.ShowSuccessWithStatus(Forms.Context, message);
        }
        public void Hide()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                AndHUD.Shared.Dismiss(Forms.Context);
            });
        }
    }
}