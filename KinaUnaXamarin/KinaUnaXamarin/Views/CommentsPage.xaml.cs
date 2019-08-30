using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using KinaUnaXamarin.Helpers;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using KinaUnaXamarin.ViewModels;
using Plugin.Multilingual;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KinaUnaXamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CommentsPage : ContentPage
    {
        private readonly int _commentThread;
        private readonly CommentsPageViewModel _commentsPageViewModel;
        const string ResourceId = "KinaUnaXamarin.Resources.Translations";
        static readonly Lazy<ResourceManager> resmgr = new Lazy<ResourceManager>(() => new ResourceManager(ResourceId, typeof(TranslateExtension).GetTypeInfo().Assembly));

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
            await GetComments();
        }

        private async void AddCommentButton_OnClicked(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(AddCommentEditor.Text))
            {
                AddCommentButton.IsEnabled = false;
                await ProgenyService.AddComment(_commentThread, AddCommentEditor.Text);
                AddCommentEditor.Text = "";
                await GetComments();
                AddCommentButton.IsEnabled = true;
            }
        }

        private async Task GetComments()
        {
            _commentsPageViewModel.CommentsCollection.Clear();
            List<Comment> commentsList = await ProgenyService.GetComments(_commentThread);
            if (commentsList.Any())
            {
                foreach (var comment in commentsList)
                {
                    _commentsPageViewModel.CommentsCollection.Add(comment);
                }
            }
        }

        private async void DeleteCommentButton_OnClicked(object sender, EventArgs e)
        {
            Button deleteButton = (Button) sender;
            string commentIdString = deleteButton.CommandParameter.ToString();
            int.TryParse(commentIdString, out int commentId);
            Comment comment = _commentsPageViewModel.CommentsCollection.SingleOrDefault(c => c.CommentId == commentId);
            if (comment != null)
            {
                var ci = CrossMultilingual.Current.CurrentCultureInfo;
                string deleteTitle = resmgr.Value.GetString("DeleteTitle", ci);
                string areYouSure = resmgr.Value.GetString("ConfirmCommentDelete", ci);
                string yes = resmgr.Value.GetString("Yes", ci);
                string no = resmgr.Value.GetString("No", ci);
                var confirmDelete = await DisplayAlert(deleteTitle, areYouSure, yes, no);
                if (confirmDelete)
                {
                    await ProgenyService.DeleteComment(comment);
                    await GetComments();
                }
                
            }
        }
    }
}