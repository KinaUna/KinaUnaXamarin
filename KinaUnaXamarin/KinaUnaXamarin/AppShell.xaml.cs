using System;
using System.Collections.Generic;
using System.Reflection;
using System.Resources;
using KinaUnaXamarin.Helpers;
using KinaUnaXamarin.Views;
using Plugin.Multilingual;
using Xamarin.Forms;

namespace KinaUnaXamarin
{
    public partial class AppShell : Xamarin.Forms.Shell
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
            _routes.Add("sleep", typeof(SleepPage));
            _routes.Add("friends", typeof(FriendsPage));
            _routes.Add("contacts", typeof(FriendsPage));
            _routes.Add("sleepstats", typeof(SleepStatsPage));
            _routes.Add("language", typeof(LanguagePage));
            _routes.Add("mychildren", typeof(MyChildrenPage));
            _routes.Add("useraccess", typeof(UserAccessPage));
            foreach (var item in _routes)
            {
                Routing.RegisterRoute(item.Key, item.Value);
            }
        }

        void UpdateLanguage()
        {
            var ci = CrossMultilingual.Current.CurrentCultureInfo;
            LanguageTabItem.Title = resmgr.Value.GetString("Language", ci);
            AccountTabItem.Title = resmgr.Value.GetString("Account", ci);
            AboutTabItem.Title = resmgr.Value.GetString("About", ci);
            SettingsFlyoutItem.Title = resmgr.Value.GetString("Settings", ci);
            PhotosFlyoutItem.Title = PhotosMenuItem.Title = resmgr.Value.GetString("Photos", ci);
            VideosFlyoutItem.Title = VideosMenuItem.Title = resmgr.Value.GetString("Videos", ci);
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
        }
    }
}
