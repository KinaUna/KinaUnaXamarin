using System;
using WindowsAzure.Messaging;
using Android.App;
using Android.Util;
using Firebase.Messaging;
using Xamarin.Essentials;

namespace KinaUnaXamarin.Droid
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
    public class FirebaseRegistrationService : FirebaseMessagingService // FirebaseInstanceIdService
    {
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
            }
            catch (Exception e)
            {
                Log.Error(AzureNotificationsConstants.DebugTag, $"Error registering device: {e.Message}");
            }
        }
    }
}