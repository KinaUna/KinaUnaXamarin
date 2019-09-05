using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using MvvmHelpers;

namespace KinaUnaXamarin.ViewModels
{
    class VideoDetailViewModel : BaseViewModel
    {
        private int _currentVideoId;
        private int _currentIndex;
        private int _userAccessLevel;
        private bool _loggedOut;
        private Progeny _progeny;
        private bool _isLoggedIn;
        private bool _isNotZoomed = true;
        private bool _isZoomed;
        private double _imageHeight;
        private double _imageWidth;
        private VideoViewModel _currentVideosViewModel;
        private string _picYears;
        private string _picMonths;
        private string[] _picWeeks;
        private string _picDays;
        private string _picHours;
        private string _picMinutes;
        private bool _picTimeValid;

        public VideoDetailViewModel()
        {
            VideoItems = new ObservableRangeCollection<VideoViewModel>();
        }

        public ObservableRangeCollection<VideoViewModel> VideoItems { get; set; }

        public VideoViewModel CurrentVideoViewModel
        {
            get => _currentVideosViewModel;
            set => SetProperty(ref _currentVideosViewModel, value);
        }
        public int CurrentVideoId
        {
            get => _currentVideoId;
            set => SetProperty(ref _currentVideoId, value);
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

        public bool IsZoomed
        {
            get => _isZoomed;
            set
            {
                SetProperty(ref _isZoomed, value);
                IsNotZoomed = !_isZoomed;
            }
        }

        public bool IsNotZoomed
        {
            get => _isNotZoomed;
            set => SetProperty(ref _isNotZoomed, value);
        }

        public double ImageHeight
        {
            get => _imageHeight;
            set => SetProperty(ref _imageHeight, value);
        }

        public double ImageWidth
        {
            get => _imageWidth;
            set => SetProperty(ref _imageWidth, value);
        }

        public string PicYears
        {
            get => _picYears;
            set => SetProperty(ref _picYears, value);
        }

        public string PicMonths
        {
            get => _picMonths;
            set => SetProperty(ref _picMonths, value);
        }

        public string[] PicWeeks
        {
            get => _picWeeks;
            set => SetProperty(ref _picWeeks, value);
        }

        public string PicDays
        {
            get => _picDays;
            set => SetProperty(ref _picDays, value);
        }

        public string PicHours
        {
            get => _picHours;
            set => SetProperty(ref _picHours, value);
        }

        public string PicMinutes
        {
            get => _picMinutes;
            set => SetProperty(ref _picMinutes, value);
        }

        public bool PicTimeValid
        {
            get => _picTimeValid;
            set => SetProperty(ref _picTimeValid, value);
        }
    }
}
