using System;
using System.Collections.Generic;
using KinaUnaXamarin.Views;
using Xamarin.Forms;

namespace KinaUnaXamarin
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        private readonly Dictionary<string, Type> _routes = new Dictionary<string, Type>();
        public Dictionary<string, Type> Routes => _routes;

        public AppShell()
        {
            InitializeComponent();
            RegisterRoutes();
        }

        void RegisterRoutes()
        {
            _routes.Add("home", typeof(HomePage));
            _routes.Add("timeline", typeof(TimelinePage));
            _routes.Add("account", typeof(AccountPage));
            _routes.Add("register", typeof(RegisterPage));
            _routes.Add("about", typeof(AboutPage));
            foreach (var item in _routes)
            {
                Routing.RegisterRoute(item.Key, item.Value);
            }
        }
    }
}
