using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using MvvmHelpers;

namespace KinaUnaXamarin.ViewModels
{
    class PhotoDetailViewModel : BaseViewModel
    {
        private int _currentPictureId;
        private int _currentIndex;
        private int _userAccessLevel;
        private bool _loggedOut;
        private Progeny _progeny;
        private bool _isLoggedIn;
        
        public PhotoDetailViewModel()
        {
            PhotoItems = new ObservableRangeCollection<PictureViewModel>();
        }

        public ObservableRangeCollection<PictureViewModel> PhotoItems { get; set; }

        public int CurrentPictureId
        {
            get => _currentPictureId;
            set => SetProperty(ref _currentPictureId, value);
        }

        public int CurrentIndex
        {
            get => _currentIndex;
            set => SetProperty(ref _currentIndex, value);
        }

        public bool LoggedOut
        {
            get => _loggedOut;
            set => SetProperty(ref _loggedOut, value);
        }
        
        
        public Progeny Progeny
        {
            get => _progeny;
            set => SetProperty(ref _progeny, value);
        }

        public int UserAccessLevel
        {
            get => _userAccessLevel;
            set => SetProperty(ref _userAccessLevel, value);
        }
        
        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => SetProperty(ref _isLoggedIn, value);
        }
    }
}
