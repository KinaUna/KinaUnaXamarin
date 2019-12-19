using System;
using System.Collections.Generic;
using System.Reflection;
using System.Resources;
using KinaUnaXamarin.Helpers;
using KinaUnaXamarin.Views;
using KinaUnaXamarin.Views.Details;
using KinaUnaXamarin.Views.MyFamily;
using KinaUnaXamarin.Views.Settings;
using Plugin.Multilingual;
using Xamarin.Forms;

namespace KinaUnaXamarin
{
    public partial class AppShell
    {
        private readonly Dictionary<string, Type> _routes = new Dictionary<string, Type>();
        const string ResourceId = "KinaUnaXamarin.Resources.Translations";
        static readonly Lazy<ResourceManager> resmgr = new Lazy<ResourceManager>(() => new ResourceManager(ResourceId, typeof(TranslateExtension).GetTypeInfo().Assembly));
        public Dictionary<string, Type> Routes => _routes;

        public AppShell()
        {
            InitializeComponent();
            RegisterRoutes();
            MessagingCenter.Subscribe<LanguagePage>(this, "LanguageChanged", (sender) =>
            {
                UpdateLanguage();
            });
        }
        

        void RegisterRoutes()
        {
            _routes.Add("home", typeof(HomePage));
            _routes.Add("yearago", typeof(YearAgoPage));
            _routes.Add("timeline", typeof(TimelinePage));
            _routes.Add("account", typeof(AccountPage));
            _routes.Add("register", typeof(RegisterPage));
            _routes.Add("about", typeof(AboutPage));
            _routes.Add("photos", typeof(PhotosPage));
            _routes.Add("videos", typeof(VideosPage));
            _routes.Add("notes", typeof(VideosPage));
            _routes.Add("sleep", typeof(SleepPage));
            _routes.Add("measurements", typeof(MeasurementsPage));
            _routes.Add("friends", typeof(FriendsPage));
            _routes.Add("contacts", typeof(ContactsPage));
            _routes.Add("sleepstats", typeof(SleepStatsPage));
            _routes.Add("measurementsstats", typeof(MeasurementsStatsPage));
            _routes.Add("skills", typeof(SkillsPage));
            _routes.Add("language", typeof(LanguagePage));
            _routes.Add("mychildren", typeof(MyChildrenPage));
            _routes.Add("useraccess", typeof(UserAccessPage));
            _routes.Add("locations", typeof(LocationsPage));
            _routes.Add("photolocations", typeof(PhotoLocationsPage));
            _routes.Add("notifications", typeof(NotificationsPageNav));
            _routes.Add("photodetailpage", typeof(PhotoDetailPage));
            foreach (var item in _routes)
            {
                Routing.RegisterRoute(item.Key, item.Value);
            }
        }

        void UpdateLanguage()
        {
            if (Device.RuntimePlatform == Device.UWP)
            {
                Device.BeginInvokeOnMainThread(SetLanguageStrings);
            }
            else
            {
                SetLanguageStrings();
            }
            
        }

        void SetLanguageStrings()
        {
            var ci = CrossMultilingual.Current.CurrentCultureInfo;
            LanguageTabItem.Title = resmgr.Value.GetString("Language", ci);
            AccountTabItem.Title = resmgr.Value.GetString("Account", ci);
            AboutTabItem.Title = resmgr.Value.GetString("About", ci);
            SettingsFlyoutItem.Title = resmgr.Value.GetString("Settings", ci);
            PhotosFlyoutItem.Title = PhotosMenuItem.Title = resmgr.Value.GetString("Photos", ci);
            VideosFlyoutItem.Title = VideosMenuItem.Title = resmgr.Value.GetString("Videos", ci);
            NotesFlyoutItem.Title = NotesMenuItem.Title = resmgr.Value.GetString("Notes", ci);
            TimelineFlyoutItem.Title = TimelineMenuItem.Title = resmgr.Value.GetString("Timeline", ci);
            YearAgoMenuItem.Title = resmgr.Value.GetString("OnThisDay", ci);
            MyFamilyFlyoutItem.Title = resmgr.Value.GetString("MyFamily", ci);
            MyChildrenTabItem.Title = resmgr.Value.GetString("MyChildren", ci);
            UserAccessTabItem.Title = resmgr.Value.GetString("UserAccess", ci);
            SleepFlyoutItem.Title = resmgr.Value.GetString("Sleep", ci);
            SleepStatsTabItem.Title = resmgr.Value.GetString("Statistics", ci);
            SleepTabItem.Title = resmgr.Value.GetString("Sleep", ci);
            FriendsFlyoutItem.Title = FriendsMenuItem.Title = resmgr.Value.GetString("Friends", ci);
            ContactsFlyoutItem.Title = ContactsMenuItem.Title = resmgr.Value.GetString("Contacts", ci);
            MeasurementsFlyoutItem.Title = resmgr.Value.GetString("Measurements", ci);
            MeasurementsStatsTabItem.Title = resmgr.Value.GetString("Statistics", ci);
            MeasurementsTabItem.Title = resmgr.Value.GetString("Measurements", ci);
            SkillsFlyoutItem.Title = SkillsMenuItem.Title = resmgr.Value.GetString("Skills", ci);
            VocabularyFlyoutItem.Title = resmgr.Value.GetString("Vocabulary", ci);
            VocabularyStatsTabItem.Title = resmgr.Value.GetString("Statistics", ci);
            VocabularyTabItem.Title = resmgr.Value.GetString("Vocabulary", ci);
            VaccinationsFlyoutItem.Title = VaccinationsMenuItem.Title = resmgr.Value.GetString("Vaccinations", ci);
            LocationsFlyoutItem.Title = LocationsMenuItem.Title = resmgr.Value.GetString("Locations", ci);
            PhotoLocationsTabItem.Title = resmgr.Value.GetString("PhotoLocations", ci);
            NotificationsFlyoutItem.Title = NotificationsMenuItem.Title = resmgr.Value.GetString("Notifications", ci);
        }

        public async void AddMessage(string title, string message, int timeLineItem)
        {
            // Todo: Show notification.
            NotificationsPage notificationsPage = new NotificationsPage();
            await Shell.Current.Navigation.PushModalAsync(notificationsPage);
        }
    }
}
