﻿using System.Collections.Generic;
using KinaUnaXamarin.Models.KinaUna;
using MvvmHelpers;
using Plugin.Multilingual;
using Xamarin.Forms;

namespace KinaUnaXamarin.ViewModels.Details
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
        private bool _editMode;
        private bool _canUserEditItems;
        private List<string> _accessLevelList;
        private bool _showComments;
        private int _videoHours;
        private int _videoMinutes;
        private int _videoSeconds;
        private List<string> _locationAutoSuggestList;
        private List<string> _tagsAutoSuggestList;
        private LayoutOptions _videoVerticalOptions;
        private bool _isSaving;

        public VideoDetailViewModel()
        {
            VideoItems = new ObservableRangeCollection<VideoViewModel>();
            _accessLevelList = new List<string>();
            var ci = CrossMultilingual.Current.CurrentCultureInfo.TwoLetterISOLanguageName;
            if (ci == "da")
            {
                _accessLevelList.Add("Administratorer");
                _accessLevelList.Add("Familie");
                _accessLevelList.Add("Omsorgspersoner/Speciel adgang");
                _accessLevelList.Add("Venner");
                _accessLevelList.Add("Registrerede brugere");
                _accessLevelList.Add("Offentlig/alle");
            }
            else
            {
                if (ci == "de")
                {
                    _accessLevelList.Add("Administratoren");
                    _accessLevelList.Add("Familie");
                    _accessLevelList.Add("Betreuer/Spezial");
                    _accessLevelList.Add("Freunde");
                    _accessLevelList.Add("Registrierte Benutzer");
                    _accessLevelList.Add("Allen zugänglich");
                }
                else
                {
                    _accessLevelList.Add("Hidden/Private");
                    _accessLevelList.Add("Family");
                    _accessLevelList.Add("Caretakers/Special Access");
                    _accessLevelList.Add("Friends");
                    _accessLevelList.Add("Registered Users");
                    _accessLevelList.Add("Public/Anyone");
                }
            }
        }

        public bool IsSaving
        {
            get => _isSaving;
            set => SetProperty(ref _isSaving, value);
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

        public List<string> AccessLevelList
        {
            get => _accessLevelList;
            set => SetProperty(ref _accessLevelList, value);
        }

        public List<string> LocationAutoSuggestList
        {
            get => _locationAutoSuggestList;
            set => SetProperty(ref _locationAutoSuggestList, value);
        }

        public List<string> TagsAutoSuggestList
        {
            get => _tagsAutoSuggestList;
            set => SetProperty(ref _tagsAutoSuggestList, value);
        }

        public bool CanUserEditItems
        {
            get => _canUserEditItems;
            set => SetProperty(ref _canUserEditItems, value);
        }

        public bool EditMode
        {
            get => _editMode;
            set => SetProperty(ref _editMode, value);
        }

        public bool ShowComments
        {
            get => _showComments;
            set => SetProperty(ref _showComments, value);
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

        public int VideoHours
        {
            get => _videoHours;
            set => SetProperty(ref _videoHours, value);
        }

        public int VideoMinutes
        {
            get => _videoMinutes;
            set => SetProperty(ref _videoMinutes, value);
        }

        public int VideoSeconds
        {
            get => _videoSeconds;
            set => SetProperty(ref _videoSeconds, value);
        }

        public LayoutOptions VideoVerticalOptions
        {
            get => _videoVerticalOptions;
            set => SetProperty(ref _videoVerticalOptions, value);
        }
    }
}
