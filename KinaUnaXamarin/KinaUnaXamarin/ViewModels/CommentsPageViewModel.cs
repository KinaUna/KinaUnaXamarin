using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using KinaUnaXamarin.Models.KinaUna;
using MvvmHelpers;

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
