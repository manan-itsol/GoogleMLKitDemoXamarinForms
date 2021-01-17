using GoogleMLKitDemo.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace GoogleMLKitDemo.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}