using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;
using dotMorten.Xamarin.Forms;
using KinaUnaXamarin.Helpers;
using KinaUnaXamarin.Models;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using KinaUnaXamarin.ViewModels;
using Plugin.Media;
using Plugin.Multilingual;
using TimeZoneConverter;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KinaUnaXamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ContactDetailPage : ContentPage
    {
        private readonly ContactDetailViewModel _viewModel = new ContactDetailViewModel();
        private UserInfo _userInfo;
        private string _accessToken;
        private int _viewChild = Constants.DefaultChildId;
        private bool _online = true;
        private string _filePath;

        const string ResourceId = "KinaUnaXamarin.Resources.Translations";
        static readonly Lazy<ResourceManager> resmgr = new Lazy<ResourceManager>(() => new ResourceManager(ResourceId, typeof(TranslateExtension).GetTypeInfo().Assembly));

        public ContactDetailPage(Contact contactItem)
        {
            _viewModel = new ContactDetailViewModel();
            InitializeComponent();
            _viewModel.CurrentContactId = contactItem.ContactId;
            _viewModel.AccessLevel = contactItem.AccessLevel;
            _viewModel.FirstName = contactItem.FirstName;
            _viewModel.MiddleName = contactItem.MiddleName;
            _viewModel.LastName = contactItem.LastName;
            _viewModel.DisplayName = contactItem.DisplayName;
            if (contactItem.Address != null)
            {
                _viewModel.AddressLine1 = contactItem.Address.AddressLine1;
                _viewModel.AddressLine2 = contactItem.Address.AddressLine2;
                _viewModel.City = contactItem.Address.City;
                _viewModel.State = contactItem.Address.City;
                _viewModel.PostalCode = contactItem.Address.PostalCode;
                _viewModel.Country = contactItem.Address.Country;
            }
            _viewModel.Email1 = contactItem.Email1;
            _viewModel.Email2 = contactItem.Email2;
            _viewModel.PhoneNumber = contactItem.PhoneNumber;
            _viewModel.MobileNumber = contactItem.MobileNumber;
            _viewModel.Website = contactItem.Website;
            _viewModel.Context = contactItem.Context;
            _viewModel.Notes = contactItem.Notes;
            _viewModel.Tags = contactItem.Tags;
            ContactImage.Source = contactItem.PictureLink;
            _viewModel.Active = contactItem.Active;
            BindingContext = _viewModel;
            
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
            var networkAccess = Connectivity.NetworkAccess;
            bool internetAccess = networkAccess == NetworkAccess.Internet;
            if (internetAccess)
            {
                OfflineStackLayout.IsVisible = false;
            }
            else
            {
                OfflineStackLayout.IsVisible = true;
            }

            await CheckAccount();
            await Reload();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
            
        }

        private async void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            var networkAccess = e.NetworkAccess;
            bool internetAccess = networkAccess == NetworkAccess.Internet;
            if (internetAccess != _online)
            {
                _online = internetAccess;
                await Reload();
            }
        }

        private async Task CheckAccount()
        {
            string userEmail = await UserService.GetUserEmail();
            _accessToken = await UserService.GetAuthAccessToken();
            bool accessTokenCurrent = false;
            if (_accessToken != "")
            {
                string accessTokenExpires = await UserService.GetAuthAccessTokenExpires();
                accessTokenCurrent = UserService.IsAccessTokenCurrent(accessTokenExpires);

                if (!accessTokenCurrent)
                {
                    bool loginSuccess = await UserService.LoginIdsAsync();
                    if (loginSuccess)
                    {
                        _accessToken = await UserService.GetAuthAccessToken();
                        accessTokenCurrent = true;
                    }

                    await Reload();
                }
            }

            if (String.IsNullOrEmpty(_accessToken) || !accessTokenCurrent)
            {

                _viewModel.IsLoggedIn = false;
                _viewModel.LoggedOut = true;
                _accessToken = "";
                _userInfo = OfflineDefaultData.DefaultUserInfo;

            }
            else
            {
                _viewModel.IsLoggedIn = true;
                _viewModel.LoggedOut = false;
                _userInfo = await UserService.GetUserInfo(userEmail);
            }

            string userviewchild = await SecureStorage.GetAsync(Constants.UserViewChildKey);
            bool viewchildParsed = int.TryParse(userviewchild, out _viewChild);
            if (!viewchildParsed)
            {
                _viewChild = _userInfo.ViewChild;
            }
            if (_viewChild == 0)
            {
                if (_userInfo.ViewChild != 0)
                {
                    _viewChild = _userInfo.ViewChild;
                }
                else
                {
                    _viewChild = Constants.DefaultChildId;
                }
            }

            if (String.IsNullOrEmpty(_userInfo.Timezone))
            {
                _userInfo.Timezone = Constants.DefaultTimeZone;
            }
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(_userInfo.Timezone);
            }
            catch (Exception)
            {
                _userInfo.Timezone = TZConvert.WindowsToIana(_userInfo.Timezone);
            }

            Progeny progeny = await ProgenyService.GetProgeny(_viewChild);
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(progeny.TimeZone);
            }
            catch (Exception)
            {
                progeny.TimeZone = TZConvert.WindowsToIana(progeny.TimeZone);
            }
            _viewModel.Progeny = progeny;

            _viewModel.UserAccessLevel = await ProgenyService.GetAccessLevel(_viewChild);
            if (_viewModel.UserAccessLevel == 0)
            {
                _viewModel.CanUserEditItems = true;
            }
        }

        private async Task Reload()
        {
            _viewModel.IsBusy = true;
            await CheckAccount();
            _viewModel.CurrentContact =
                await ProgenyService.GetContact(_viewModel.CurrentContactId, _accessToken, _userInfo.Timezone);

            _viewModel.AccessLevel = _viewModel.CurrentContact.AccessLevel;
            _viewModel.CurrentContact.Progeny = await ProgenyService.GetProgeny(_viewModel.CurrentContact.ProgenyId);
            
            _viewModel.UserAccessLevel = await ProgenyService.GetAccessLevel(_viewModel.CurrentContact.ProgenyId);
            if (_viewModel.UserAccessLevel == 0)
            {
                _viewModel.CanUserEditItems = true;
            }
            else
            {
                _viewModel.CanUserEditItems = false;
            }

            _viewModel.CurrentContactId = _viewModel.CurrentContact.ContactId;
            _viewModel.AccessLevel = _viewModel.CurrentContact.AccessLevel;
            _viewModel.FirstName = _viewModel.CurrentContact.FirstName;
            _viewModel.MiddleName = _viewModel.CurrentContact.MiddleName;
            _viewModel.LastName = _viewModel.CurrentContact.LastName;
            _viewModel.DisplayName = _viewModel.CurrentContact.DisplayName;
            _viewModel.AddressIdNumber = 0;
            if (_viewModel.CurrentContact.Address != null)
            {
                _viewModel.AddressIdNumber = _viewModel.CurrentContact.AddressIdNumber.Value;
                _viewModel.AddressLine1 = _viewModel.CurrentContact.Address.AddressLine1;
                _viewModel.AddressLine2 = _viewModel.CurrentContact.Address.AddressLine2;
                _viewModel.City = _viewModel.CurrentContact.Address.City;
                _viewModel.State = _viewModel.CurrentContact.Address.State;
                _viewModel.PostalCode = _viewModel.CurrentContact.Address.PostalCode;
                _viewModel.Country = _viewModel.CurrentContact.Address.Country;
            }
            _viewModel.Email1 = _viewModel.CurrentContact.Email1;
            _viewModel.Email2 = _viewModel.CurrentContact.Email2;
            _viewModel.PhoneNumber = _viewModel.CurrentContact.PhoneNumber;
            _viewModel.MobileNumber = _viewModel.CurrentContact.MobileNumber;
            _viewModel.Website = _viewModel.CurrentContact.Website;
            _viewModel.Context = _viewModel.CurrentContact.Context;
            _viewModel.Notes = _viewModel.CurrentContact.Notes;
            _viewModel.Tags = _viewModel.CurrentContact.Tags;
            _viewModel.Active = _viewModel.CurrentContact.Active;
            ContactImage.Source = _viewModel.CurrentContact.PictureLink;

            var networkInfo = Connectivity.NetworkAccess;

            if (networkInfo == NetworkAccess.Internet)
            {
                // Connection to internet is available
                _online = true;
                OfflineStackLayout.IsVisible = false;
            }
            else
            {
                _online = false;
                OfflineStackLayout.IsVisible = true;
            }

            _viewModel.IsBusy = false;

        }
        
        private async void EditButton_OnClicked(object sender, EventArgs e)
        {
            if (_viewModel.EditMode)
            {
                _viewModel.EditMode = false;
                _viewModel.IsBusy = true;

                _viewModel.CurrentContact.FirstName = _viewModel.FirstName;
                _viewModel.CurrentContact.MiddleName = _viewModel.MiddleName;
                _viewModel.CurrentContact.LastName = _viewModel.LastName;
                _viewModel.CurrentContact.DisplayName = _viewModel.DisplayName;
                
                _viewModel.CurrentContact.AddressIdNumber = _viewModel.AddressIdNumber;
                Address address = new Address();
                address.AddressLine1 = _viewModel.AddressLine1;
                address.AddressLine2 = _viewModel.AddressLine2;
                address.City = _viewModel.City;
                address.State = _viewModel.State;
                address.PostalCode = _viewModel.PostalCode;
                address.Country = _viewModel.Country;
                address.AddressId = _viewModel.AddressIdNumber;
                _viewModel.CurrentContact.Address = address;

                _viewModel.CurrentContact.Email1 = _viewModel.Email1;
                _viewModel.CurrentContact.Email2 = _viewModel.Email2;
                _viewModel.CurrentContact.PhoneNumber = _viewModel.PhoneNumber;
                _viewModel.CurrentContact.MobileNumber = _viewModel.MobileNumber;
                _viewModel.CurrentContact.Website = _viewModel.Website;
                _viewModel.CurrentContact.Notes = _viewModel.Notes;
                
                _viewModel.CurrentContact.Context = ContextEntry?.Text ?? ""; ;
                _viewModel.CurrentContact.Tags = TagsEntry?.Text ?? "";
                _viewModel.CurrentContact.AccessLevel = _viewModel.AccessLevel;
                _viewModel.CurrentContact.Active = _viewModel.Active;
                
                if (string.IsNullOrEmpty(_filePath) || !File.Exists(_filePath))
                {
                    _viewModel.CurrentContact.PictureLink = "[KeepExistingLink]";
                }
                else
                {
                    // Upload photo file, get a reference to the image.
                    string pictureLink = await ProgenyService.UploadContactPicture(_filePath);
                    if (pictureLink == "")
                    {
                        _viewModel.IsBusy = false;
                        // Todo: Show error
                        _viewModel.CurrentContact.PictureLink = "[KeepExistingLink]";
                        return;
                    }
                    _viewModel.CurrentContact.PictureLink = pictureLink;
                }
                // Save changes.
                Contact resultContact = await ProgenyService.UpdateContact(_viewModel.CurrentContact);
                _viewModel.IsBusy = false;
                EditButton.Text = IconFont.CalendarEdit;
                if (resultContact != null)  // Todo: Error message if update fails.
                {
                    MessageLabel.Text = "Contact Updated"; // Todo: Translate
                    MessageLabel.BackgroundColor = Color.DarkGreen;
                    MessageLabel.IsVisible = true;
                    await Reload();
                }
            }
            else
            {
                EditButton.Text = IconFont.ContentSave;

                _viewModel.EditMode = true;
                _viewModel.TagsAutoSuggestList = await ProgenyService.GetTagsAutoSuggestList(_viewModel.CurrentContact.ProgenyId, 0);
                _viewModel.ContextAutoSuggestList = await ProgenyService.GetContextAutoSuggestList(_viewModel.CurrentContact.ProgenyId, 0);
            }
        }

        private async void SelectImageButton_OnClicked(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();
            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                return;
            }

            var file = await CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
            {
                RotateImage = false,
                SaveMetaData = true
            });

            if (file == null)
            {
                return;
            }

            _filePath = file.Path;
            ContactImage.Source = ImageSource.FromStream(() =>
            {
                var stream = file.GetStream();
                file.Dispose();
                return stream;
            });
        }

        private async void CancelButton_OnClicked(object sender, EventArgs e)
        {
            EditButton.Text = IconFont.AccountEdit;
            _viewModel.EditMode = false;
            await Reload();
        }
        
        private async void DeleteButton_OnClickedButton_OnClicked(object sender, EventArgs e)
        {
            var ci = CrossMultilingual.Current.CurrentCultureInfo;
            string confirmTitle = resmgr.Value.GetString("DeleteContact", ci);
            string confirmMessage = resmgr.Value.GetString("DeleteContactMessage", ci) + " ? ";
            string yes = resmgr.Value.GetString("Yes", ci);
            string no = resmgr.Value.GetString("No", ci); ;
            bool confirmDelete = await DisplayAlert(confirmTitle, confirmMessage, yes, no);
            if (confirmDelete)
            {
                _viewModel.IsBusy = true;
                _viewModel.EditMode = false;
                Contact deletedContact = await ProgenyService.DeleteContact(_viewModel.CurrentContact);
                if (deletedContact.ContactId == 0)
                {
                    _viewModel.EditMode = false;
                    // Todo: Show success message

                }
                else
                {
                    _viewModel.EditMode = true;
                    // Todo: Show failed message
                }
                _viewModel.IsBusy = false;
            }
        }

        private void TagsEditor_OnTextChanged(object sender, AutoSuggestBoxTextChangedEventArgs e)
        {
            // Only get results when it was a user typing, 
            // otherwise assume the value got filled in by TextMemberPath 
            // or the handler for SuggestionChosen.
            if (e.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                AutoSuggestBox autoSuggestBox = sender as AutoSuggestBox;
                if (autoSuggestBox != null && autoSuggestBox.Text.Length > 0)
                {
                    string lastTag = autoSuggestBox.Text.Split(',').LastOrDefault();
                    if (!string.IsNullOrEmpty(lastTag) && lastTag.Length > 0)
                    {
                        List<string> filteredTags = new List<string>();
                        foreach (string tagString in _viewModel.TagsAutoSuggestList)
                        {
                            if (tagString.Trim().ToUpper().Contains(lastTag.Trim().ToUpper()))
                            {
                                filteredTags.Add(tagString);
                            }
                        }
                        //Set the ItemsSource to be your filtered dataset
                        autoSuggestBox.ItemsSource = filteredTags;
                    }
                    else
                    {
                        autoSuggestBox.ItemsSource = null;
                    }

                }
                else
                {
                    if (autoSuggestBox != null)
                    {
                        autoSuggestBox.ItemsSource = null;
                    }
                }
            }
        }

        private void TagsEditor_OnQuerySubmitted(object sender, AutoSuggestBoxQuerySubmittedEventArgs e)
        {
            if (e.ChosenSuggestion != null)
            {
                AutoSuggestBox autoSuggestBox = sender as AutoSuggestBox;
                if (autoSuggestBox != null)
                {
                    // User selected an item from the suggestion list, take an action on it here.
                    List<string> existingTags = TagsEntry.Text.Split(',').ToList();
                    existingTags.Remove(existingTags.Last());
                    string newText = "";
                    if (existingTags.Any())
                    {
                        foreach (string tagString in existingTags)
                        {
                            newText = newText + tagString + ", ";
                        }
                    }
                    newText = newText + e.ChosenSuggestion.ToString() + ", ";
                    autoSuggestBox.Text = newText;

                    autoSuggestBox.ItemsSource = null;
                }
            }
            else
            {
                // User hit Enter from the search box. Use e.QueryText to determine what to do.
            }
        }

        private void TagsEditor_OnSuggestionChosen(object sender, AutoSuggestBoxSuggestionChosenEventArgs e)
        {
            AutoSuggestBox autoSuggestBox = sender as AutoSuggestBox;
            // Set sender.Text. You can use e.SelectedItem to build your text string.
            if (autoSuggestBox != null)
            {
                //List<string> existingTags = TagsEditor.Text.Split(',').ToList();
                //existingTags.Remove(existingTags.Last());
                //autoSuggestBox.Text = "";
                //foreach (string tagString in existingTags)
                //{
                //    autoSuggestBox.Text = autoSuggestBox.Text + ", " + tagString;
                //}
                //autoSuggestBox.Text = autoSuggestBox.Text + e.SelectedItem.ToString();
            }
        }

        private void ContextEntry_OnTextChanged(object sender, AutoSuggestBoxTextChangedEventArgs e)
        {
            // Only get results when it was a user typing, 
            // otherwise assume the value got filled in by TextMemberPath 
            // or the handler for SuggestionChosen.
            if (e.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                AutoSuggestBox autoSuggestBox = sender as AutoSuggestBox;
                if (autoSuggestBox != null && autoSuggestBox.Text.Length > 0)
                {
                    List<string> filteredContexts = new List<string>();
                    foreach (string contextString in _viewModel.ContextAutoSuggestList)
                    {
                        if (contextString.ToUpper().Contains(autoSuggestBox.Text.Trim().ToUpper()))
                        {
                            filteredContexts.Add(contextString);
                        }
                    }
                    //Set the ItemsSource to be your filtered dataset
                    autoSuggestBox.ItemsSource = filteredContexts;
                }
                else
                {
                    if (autoSuggestBox != null)
                    {
                        autoSuggestBox.ItemsSource = null;
                    }
                }
            }
        }

        private void ContextEntry_OnQuerySubmitted(object sender, AutoSuggestBoxQuerySubmittedEventArgs e)
        {
            if (e.ChosenSuggestion != null)
            {
                AutoSuggestBox autoSuggestBox = sender as AutoSuggestBox;
                if (autoSuggestBox != null)
                {
                    // User selected an item from the suggestion list, take an action on it here.
                    autoSuggestBox.Text = e.ChosenSuggestion.ToString();
                    autoSuggestBox.ItemsSource = null;
                }
            }
            else
            {
                // User hit Enter from the search box. Use e.QueryText to determine what to do.
            }
        }

        private void ContextEntry_OnSuggestionChosen(object sender, AutoSuggestBoxSuggestionChosenEventArgs e)
        {
            AutoSuggestBox autoSuggestBox = sender as AutoSuggestBox;
            // Set sender.Text. You can use e.SelectedItem to build your text string.
            if (autoSuggestBox != null)
            {
                autoSuggestBox.Text = e.SelectedItem.ToString();
            }
        }

        private void CallPhoneNumberButton_OnClicked(object sender, EventArgs e)
        {
            try
            {
                PhoneDialer.Open(_viewModel.CurrentContact.PhoneNumber);
            }
            catch (ArgumentNullException)
            {
                // Number was null or white space
            }
            catch (FeatureNotSupportedException)
            {
                // Phone Dialer is not supported on this device.
            }
            catch (Exception)
            {
                // Other error has occurred.
            }
        }

        private void CallMobileNumberButton_OnClicked(object sender, EventArgs e)
        {
            try
            {
                PhoneDialer.Open(_viewModel.CurrentContact.MobileNumber);
            }
            catch (ArgumentNullException)
            {
                // Number was null or white space
            }
            catch (FeatureNotSupportedException)
            {
                // Phone Dialer is not supported on this device.
            }
            catch (Exception)
            {
                // Other error has occurred.
            }
        }

        private async void SmsMobileNumberButton_OnClicked(object sender, EventArgs e)
        {
            try
            {
                var message = new SmsMessage("", new[] { _viewModel.CurrentContact.MobileNumber });
                await Sms.ComposeAsync(message);
            }
            catch (FeatureNotSupportedException ex)
            {
                // Sms is not supported on this device.
            }
            catch (Exception)
            {
                // Other error has occurred.
            }
        }

        private async void Email1Button_OnClicked(object sender, EventArgs e)
        {
            List<string> recipients = new List<string>();
            recipients.Add(_viewModel.CurrentContact.Email1);
            var message = new EmailMessage
            {
                Subject = "",
                Body = "",
                To = recipients,
                //Cc = ccRecipients,
                //Bcc = bccRecipients
            };
            await Email.ComposeAsync(message);
        }

        private async void Email2Button_OnClicked(object sender, EventArgs e)
        {
            List<string> recipients = new List<string>();
            recipients.Add(_viewModel.CurrentContact.Email2);
            var message = new EmailMessage
            {
                Subject = "",
                Body = "",
                To = recipients,
                //Cc = ccRecipients,
                //Bcc = bccRecipients
            };
            await Email.ComposeAsync(message);
        }

        private async void WebsiteButton_OnClicked(object sender, EventArgs e)
        {
            string uriString = _viewModel.CurrentContact.Website;
            if (!uriString.StartsWith("http"))
            {
                uriString = "https://" + uriString;
            }
            Uri websiteUri = new Uri(uriString);
            await Browser.OpenAsync(websiteUri, BrowserLaunchMode.SystemPreferred);
        }
    }
}