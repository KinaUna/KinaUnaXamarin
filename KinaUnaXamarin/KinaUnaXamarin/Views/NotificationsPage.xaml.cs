using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using MvvmHelpers;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KinaUnaXamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NotificationsPage : ContentPage
    {
        private readonly ObservableRangeCollection<MobileNotification> _notificationsList;
        

        public NotificationsPage()
        {
            _notificationsList = new ObservableRangeCollection<MobileNotification>();
            InitializeComponent();
            NotificationsListCollectionView.ItemsSource = _notificationsList;

        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (Shell.Current.Navigation.ModalStack.Any())
            {
                TitleLabel.IsVisible = true;
                CloseStackLayout.IsVisible = true;
            }
            else
            {
                TitleLabel.IsVisible = false;
                CloseStackLayout.IsVisible = false;
            }
            await Reload();
        }

        private async Task Reload()
        {
            List<MobileNotification> notifications = await UserService.GetNotificationsList(10, 0, "EN");
            _notificationsList.ReplaceRange(notifications);
        }

        private async void NotificationsListCollectionView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NotificationsListCollectionView.SelectedItem is MobileNotification notification)
            {
                if (!notification.Read)
                {
                    notification.Read = true;
                    await UserService.UpdateNotification(notification);
                }

                int itemId;
                bool parsed = int.TryParse(notification.ItemId, out itemId);
                if (notification.ItemType == (int) KinaUnaTypes.TimeLineType.Sleep)
                {
                    if (parsed)
                    {
                        Sleep sleep = await ProgenyService.GetSleep(itemId, await UserService.GetAuthAccessToken(),
                            await UserService.GetUserTimezone());
                        SleepDetailPage sleepDetailPage = new SleepDetailPage(sleep);
                        await Shell.Current.Navigation.PushModalAsync(sleepDetailPage);
                    }
                }

                if (notification.ItemType == (int)KinaUnaTypes.TimeLineType.Photo)
                {
                    if (parsed)
                    {
                        PhotoDetailPage photoDetailPage = new PhotoDetailPage(itemId);
                        await Shell.Current.Navigation.PushModalAsync(photoDetailPage);
                    }
                }

                if (notification.ItemType == (int)KinaUnaTypes.TimeLineType.Video)
                {
                    if (parsed)
                    {
                        VideoDetailPage videoDetailPage = new VideoDetailPage(itemId);
                        await Shell.Current.Navigation.PushModalAsync(videoDetailPage);
                    }
                }

                if (notification.ItemType == (int)KinaUnaTypes.TimeLineType.Note)
                {
                    if (parsed)
                    {
                        Note noteItem = await ProgenyService.GetNote(itemId, await UserService.GetAuthAccessToken(), await UserService.GetUserTimezone());
                        NoteDetailPage noteDetailPage = new NoteDetailPage(noteItem);
                        await Shell.Current.Navigation.PushModalAsync(noteDetailPage);
                    }
                }

                if (notification.ItemType == (int)KinaUnaTypes.TimeLineType.Measurement)
                {
                    if (parsed)
                    {
                        Measurement measurementItem = await ProgenyService.GetMeasurement(itemId, await UserService.GetAuthAccessToken());
                        MeasurementDetailPage measurementDetailPage = new MeasurementDetailPage(measurementItem);
                        await Shell.Current.Navigation.PushModalAsync(measurementDetailPage);
                    }
                }

                if (notification.ItemType == (int)KinaUnaTypes.TimeLineType.Calendar)
                {
                    if (parsed)
                    {
                        CalendarItem calendarItem = await ProgenyService.GetCalendarItem(itemId, await UserService.GetAuthAccessToken(), await UserService.GetUserTimezone());
                        EventDetailPage eventDetailPage = new EventDetailPage(calendarItem);
                        await Shell.Current.Navigation.PushModalAsync(eventDetailPage);
                    }
                }

                if (notification.ItemType == (int)KinaUnaTypes.TimeLineType.Contact)
                {
                    if (parsed)
                    {
                        Contact contact = await ProgenyService.GetContact(itemId, await UserService.GetAuthAccessToken());
                        ContactDetailPage contactDetailPage = new ContactDetailPage(contact);
                        await Shell.Current.Navigation.PushModalAsync(contactDetailPage);
                    }
                }

                if (notification.ItemType == (int)KinaUnaTypes.TimeLineType.Friend)
                {
                    if (parsed)
                    {
                        Friend friend = await ProgenyService.GetFriend(itemId, await UserService.GetAuthAccessToken());
                        FriendDetailPage friendDetailPage = new FriendDetailPage(friend);
                        await Shell.Current.Navigation.PushModalAsync(friendDetailPage);
                    }
                }

                if (notification.ItemType == (int)KinaUnaTypes.TimeLineType.Location)
                {
                    if (parsed)
                    {
                        Location location = await ProgenyService.GetLocation(itemId, await UserService.GetAuthAccessToken(), await UserService.GetUserTimezone());
                        LocationDetailPage locationDetailPage = new LocationDetailPage(location);
                        await Shell.Current.Navigation.PushModalAsync(locationDetailPage);
                    }
                }

                if (notification.ItemType == (int)KinaUnaTypes.TimeLineType.Skill)
                {
                    if (parsed)
                    {
                        Skill skill = await ProgenyService.GetSkill(itemId, await UserService.GetAuthAccessToken(), await UserService.GetUserTimezone());
                        SkillDetailPage skillDetailPage = new SkillDetailPage(skill);
                        await Shell.Current.Navigation.PushModalAsync(skillDetailPage);
                    }
                }

                if (notification.ItemType == (int)KinaUnaTypes.TimeLineType.Vaccination)
                {
                    if (parsed)
                    {
                        Vaccination vaccination = await ProgenyService.GetVaccination(itemId, await UserService.GetAuthAccessToken(), await UserService.GetUserTimezone());
                        VaccinationDetailPage vaccinationDetailPage = new VaccinationDetailPage(vaccination);
                        await Shell.Current.Navigation.PushModalAsync(vaccinationDetailPage);
                    }
                }

                if (notification.ItemType == (int)KinaUnaTypes.TimeLineType.Vocabulary)
                {
                    if (parsed)
                    {
                        VocabularyItem vocabularyItem = await ProgenyService.GetVocabularyItem(itemId, await UserService.GetAuthAccessToken(), await UserService.GetUserTimezone());
                        VocabularyDetailPage vocabularyDetailPage = new VocabularyDetailPage(vocabularyItem);
                        await Shell.Current.Navigation.PushModalAsync(vocabularyDetailPage);
                    }
                }

                if (notification.ItemType == (int)KinaUnaTypes.TimeLineType.UserAccess)
                {
                    if (parsed)
                    {
                        await Shell.Current.GoToAsync("useraccess");
                    }
                }
            }

            NotificationsListCollectionView.SelectedItem = null;
            await Reload();
        }

        private async void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
        {
            await Shell.Current.Navigation.PopModalAsync();
        }
    }
}