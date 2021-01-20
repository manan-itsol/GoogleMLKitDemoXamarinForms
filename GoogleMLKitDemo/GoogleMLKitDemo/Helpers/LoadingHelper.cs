using GoogleMLKitDemo.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace GoogleMLKitDemo.Helpers
{
    public class LoadingHelper
    {
        public static void Show(string message)
        {
            var indicator = DependencyService.Get<ILoadingService>();
            indicator.Show(message);
        }

        public static void Hide()
        {
            var indicator = DependencyService.Get<ILoadingService>();
            indicator.Hide();
        }

        public static void ShowSuccess(string message)
        {
            var indicator = DependencyService.Get<ILoadingService>();
            indicator.ShowSuccess(message);
        }
    }
}
