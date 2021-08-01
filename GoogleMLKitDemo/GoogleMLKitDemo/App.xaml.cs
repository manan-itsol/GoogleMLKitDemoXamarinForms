using GoogleMLKitDemo.Services;
using GoogleMLKitDemo.Views;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace GoogleMLKitDemo
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            MainPage = new TextRecognitionPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
