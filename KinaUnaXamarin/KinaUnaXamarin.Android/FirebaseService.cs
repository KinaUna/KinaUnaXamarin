using System;
using System.Threading.Tasks;
using WindowsAzure.Messaging;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Util;
using Firebase.Messaging;
using Xamarin.Essentials;

namespace KinaUnaXamarin.Droid
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class FirebaseService : FirebaseMessagingService
    {
        public override async void OnNewToken(string token)
        {
            await SecureStorage.SetAsync("FirebaseToken", token);
            await SendRegistrationToServer(token);
        }

        private async Task SendRegistrationToServer(string token)
        {
            try
            {
                NotificationHub hub = new NotificationHub(AzureNotificationsConstants.NotificationHubName, AzureNotificationsConstants.ListenConnectionString, this);

                // register device with Azure Notification Hub using the token from FCM
                Registration reg = hub.Register(token, AzureNotificationsConstants.SubscriptionTags);

                // subscribe to the SubscriptionTags list with a simple template.
                string pnsHandle = reg.PNSHandle;
                string registrationId = reg.RegistrationId;
                await SecureStorage.SetAsync("PnsHandle", pnsHandle);
                await SecureStorage.SetAsync("RegistrationId", registrationId);
            }
            catch (Exception e)
            {
                Log.Error(AzureNotificationsConstants.DebugTag, $"Error registering device: {e.Message}");
            }
        }

        public override void OnMessageReceived(RemoteMessage message)
        {
            base.OnMessageReceived(message);
            string messageBody = string.Empty;
            string title = string.Empty;
            string tItem = string.Empty;
            if (message.GetNotification() != null)
            {
                messageBody = message.GetNotification().Body;
                title = message.GetNotification().Title;
            }
            // NOTE: test messages sent via the Azure portal will be received here
            else
            {
                if (message.Data.ContainsKey("title"))
                {
                    title = message.Data["title"];
                }

                if (message.Data.ContainsKey("message"))
                {
                    messageBody = message.Data["message"];
                }

                if (message.Data.ContainsKey("notData"))
                {
                    tItem = message.Data["notData"];
                    
                }
                // messageBody = message.Data.Values.First();
            }

            // convert the incoming message to a local notification
            SendLocalNotification(title, messageBody, tItem);

            // send the incoming message directly to the MainPage
            // SendMessageToMainPage(messageBody);
        }

        void SendLocalNotification(string title, string body, string timeLineItem)
        {
            var intent = new Intent(this, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.ClearTop);
            intent.PutExtra("message", body);
            intent.PutExtra("notData", timeLineItem);
            var pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.OneShot);

            if (string.IsNullOrEmpty(title))
            {
                title = "New Notification from KinaUna";
            }
            var notificationBuilder = new NotificationCompat.Builder(this)
                .SetContentTitle(title)
                .SetSmallIcon(Resource.Drawable.ic_stat_kinaunalogo_300x300)
                .SetColor(Android.Graphics.Color.ParseColor("#6828A9"))
                .SetContentText(body)
                .SetAutoCancel(true)
                .SetShowWhen(false)
                .SetContentIntent(pendingIntent);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                notificationBuilder.SetChannelId(AzureNotificationsConstants.NotificationChannelName);
            }

            var notificationManager = NotificationManager.FromContext(this);
            notificationManager.Notify(0, notificationBuilder.Build());
        }

        void SendMessageToMainPage(string title, string body, string timeLineItem)
        {
            int.TryParse(timeLineItem, out int timeLineId);
            (App.Current.MainPage as AppShell)?.AddMessage(title, body, timeLineId);
        }
    }
}