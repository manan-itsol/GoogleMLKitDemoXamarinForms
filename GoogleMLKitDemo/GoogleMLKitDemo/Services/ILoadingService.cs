using System;
using System.Collections.Generic;
using System.Text;

namespace GoogleMLKitDemo.Services
{
    public interface ILoadingService
    {
        void Show(string message = "Loading");
        void ShowSuccess(string message = "");
        void Hide();
    }
}
