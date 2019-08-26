using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using MvvmHelpers;
using Xamarin.Forms;

namespace KinaUnaXamarin.ViewModels
{
    class CommentsPageViewModel: BaseViewModel
    {
        private int _commentThread;
        public ObservableCollection<Comment> CommentsCollection { get; set; }

        public CommentsPageViewModel(int commentThread)
        {
            _commentThread = commentThread;
            CommentsCollection = new ObservableCollection<Comment>();
        }
    }
}
