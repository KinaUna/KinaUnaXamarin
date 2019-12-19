using System.Collections.ObjectModel;
using KinaUnaXamarin.Models.KinaUna;
using MvvmHelpers;

namespace KinaUnaXamarin.ViewModels
{
    class CommentsPageViewModel: BaseViewModel
    {
        public ObservableCollection<Comment> CommentsCollection { get; set; }

        public CommentsPageViewModel()
        {
            CommentsCollection = new ObservableCollection<Comment>();
        }
    }
}
