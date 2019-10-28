
using System;
using System.Threading.Tasks;
using WindowsAzure.Messaging;
using Android.App;
using Android.Util;
using Firebase.Iid;
using Firebase.Messaging;
using Newtonsoft.Json;
using Xamarin.Essentials;

namespace KinaUnaXamarin.Droid
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
    public class FirebaseRegistrationService : FirebaseMessagingService // FirebaseInstanceIdService
    {
        //public override async void OnTokenRefresh()
        //{
        //    string token = FirebaseInstanceId.Instance.Token;

        //    // NOTE: logging the token is not recommended in production but during
        //    // development it is useful to test messages directly from Firebase
        //    Log.Info(AzureNotificationsConstants.DebugTag, $"Token received: {token}");

        //    await SecureStorage.SetAsync("FirebaseToken", token);
        //    await SendRegistrationToServer(token);
        //}

        public override void OnNewToken(string token)
        {
            SecureStorage.SetAsync("FirebaseToken", token);
            SendRegistrationToServer(token);
        }

        private void SendRegistrationToServer(string token)
        {
            try
            {
                NotificationHub hub = new NotificationHub(AzureNotificationsConstants.NotificationHubName, AzureNotificationsConstants.ListenConnectionString, this);

                // register device with Azure Notification Hub using the token from FCM
                Registration reg = hub.Register(token, AzureNotificationsConstants.SubscriptionTags);

                // subscribe to the SubscriptionTags list with a simple template.
                string pnsHandle = reg.PNSHandle;
                SecureStorage.SetAsync("PnsHandle", pnsHandle);
                var cats = string.Join(", ", reg.Tags);
                var temp = hub.RegisterTemplate(pnsHandle, "defaultTemplate", AzureNotificationsConstants.FCMTemplateBody, AzureNotificationsConstants.SubscriptionTags);
            }
            catch (Exception e)
            {
                Log.Error(AzureNotificationsConstants.DebugTag, $"Error registering device: {e.Message}");
            }
        }
    }
}