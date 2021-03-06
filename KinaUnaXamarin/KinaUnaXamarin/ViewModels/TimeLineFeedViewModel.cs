﻿using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using MvvmHelpers;
using Xamarin.Forms;

namespace KinaUnaXamarin.ViewModels
{
    class TimelineFeedViewModel : BaseViewModel
    {
        private bool _isLoggedIn = true;
        private Progeny _progeny;
        private int _userAccessLevel;
        private bool _showOptions;
        private int _selectedYear;
        private int _selectedMonth;
        private int _selectedDay;
        private DateTime _maximumDate;
        private bool _canUserAddItems;
        private bool _canShowMore;
        private bool _online = true;

        public ObservableCollection<Progeny> ProgenyCollection { get; }

        public TimelineFeedViewModel()
        {
            LoginCommand = new Command(Login);
            ViewChild = Constants.DefaultChildId;
            ProgenyCollection = new ObservableCollection<Progeny>();
            TimeLineItems = new ObservableRangeCollection<TimeLineItem>();
            _canShowMore = true;
        }

        public int ViewChild { get; set; }

        public UserInfo UserInfo { get; set; }
        
        public string AccessToken { get; set; }

        public string UserEmail { get; set; }

        public bool Online
        {
            get => _online;
            set => SetProperty(ref _online, value);
        }
        
        public ICommand LoginCommand
        {
            get;
            private set;
        }

        public async void Login()
        {
            IsLoggedIn = await UserService.LoginIdsAsync();
        }

        public bool CanUserAddItems
        {
            get => _canUserAddItems;
            set => SetProperty(ref _canUserAddItems, value);
        }

        public DateTime MaxDate
        {
            get => _maximumDate;
            set => SetProperty(ref _maximumDate, value);
        }

        public int SelectedYear
        {
            get => _selectedYear;
            set => SetProperty(ref _selectedYear, value);
        }

        public int SelectedMonth
        {
            get => _selectedMonth;
            set => SetProperty(ref _selectedMonth, value);
        }
        public int SelectedDay
        {
            get => _selectedDay;
            set => SetProperty(ref _selectedDay, value);
        }
        public bool ShowOptions
        {
            get => _showOptions;
            set => SetProperty(ref _showOptions, value);
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

        public ObservableRangeCollection<TimeLineItem> TimeLineItems { get; set; }

        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => SetProperty(ref _isLoggedIn, value);
        }

        public bool CanShowMore
        {
            get => _canShowMore;
            set => SetProperty(ref _canShowMore, value);
        }
    }
}
