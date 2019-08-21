using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using KinaUnaXamarin.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KinaUnaXamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CommentsPage : ContentPage
    {
        private int _commentThread;
        private CommentsPageViewModel _commentsPageViewModel;

        public CommentsPage(int commentThread)
        {
            _commentThread = commentThread;
            InitializeComponent();
            _commentsPageViewModel = new CommentsPageViewModel(_commentThread);
            BindingContext = _commentsPageViewModel;
            CommentsCollectionView.ItemsSource = _commentsPageViewModel.CommentsCollection;
            
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            List<Comment> commentsList = await ProgenyService.GetComments(_commentThread);
            foreach (var comment in commentsList)
            {
                _commentsPageViewModel.CommentsCollection.Add(comment);
            }
        }
    }
}