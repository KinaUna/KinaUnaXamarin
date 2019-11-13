using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Browser;
using KinaUnaXamarin.Extensions;
using KinaUnaXamarin.Models;
using KinaUnaXamarin.Models.KinaUna;
using Newtonsoft.Json;
using TimeZoneConverter;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace KinaUnaXamarin.Services
{
    public static class UserService
    {
        public static async Task<UserInfo> GetUserInfo(string userEmail)
        {
            bool online = ProgenyService.Online();
            if (online)
            {
                var client = new HttpClient();
                string accessToken = await SecureStorage.GetAsync(Constants.AuthAccessTokenKey);
                client.SetBearerToken(accessToken);
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

                var result = await client.GetAsync("api/userinfo/byemail/" + userEmail).ConfigureAwait(false);

                if (result.IsSuccessStatusCode)
                {
                    var userinfoString = await result.Content.ReadAsStringAsync();
                    UserInfo userinfo = JsonConvert.DeserializeObject<UserInfo>(userinfoString);
                    // await SecureStorage.SetAsync("UserInfo" + userEmail, JsonConvert.SerializeObject(userinfo));
                    await App.Database.SaveUserInfoAsync(userinfo);
                    return userinfo;
                }
                else
                {
                    // Todo: Handle errors
                    // string userinfoString = await SecureStorage.GetAsync("UserInfo" + userEmail);
                    UserInfo userinfo = await App.Database.GetUserInfoAsync(userEmail);
                    if (userinfo != null)
                    {
                        return userinfo;
                    }

                    return OfflineDefaultData.DefaultUserInfo;
                }
            }
            else
            {
                UserInfo userinfo = await App.Database.GetUserInfoAsync(userEmail);
                if (userinfo != null)
                {
                    return userinfo;
                }

                return OfflineDefaultData.DefaultUserInfo;
            }
        }

        public static async Task<string> GetUserPicture(string pictureId)
        {
            bool online = ProgenyService.Online();
            if (online)
            {
                var client = new HttpClient();
                string accessToken = await SecureStorage.GetAsync(Constants.AuthAccessTokenKey);
                client.SetBearerToken(accessToken);
                client.BaseAddress = new Uri(Constants.MediaApiUrl);

                var result = await client.GetAsync("api/pictures/getprofilepicture/" + pictureId).ConfigureAwait(false);

                if (result.IsSuccessStatusCode)
                {
                    string resultString = await result.Content.ReadAsStringAsync();
                    string pictureResultString = Regex.Replace(resultString, @"([|""|])", "");
                    // await SecureStorage.SetAsync("ProfilePicture" + pictureId, pictureResultString);
                    await App.Database.SaveUserPictureAsync(pictureId, pictureResultString);
                    return pictureResultString;
                }
                else
                {
                    // Todo: Handle errors
                    return Constants.ProfilePicture;
                }
            }
            else
            {
                // string pictureResultString = await SecureStorage.GetAsync("ProfilePicture" + pictureId);
                string pictureResultString = await App.Database.GetUserPictureAsync(pictureId);
                if (!string.IsNullOrEmpty(pictureResultString))
                {
                    return pictureResultString;
                }

                return Constants.ProfilePicture;
            }
        }

        public static async Task<bool> RegisterAsync(string email, string password, string confirmPassword, string timezone = "", string language = "en")
        {
            var client = new HttpClient();
            var model = new RegisterUser();
            model.Email = email;
            model.Password = password;
            model.ConfirmPassword = confirmPassword;
            model.Language = language;
            model.TimeZone = timezone;

            var json = JsonConvert.SerializeObject(model);
            HttpContent content = new StringContent(json);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            // Todo: Save url in configuration. 
            // Todo: Create Api for registering in IDP Project.
            var response = await client.PostAsync(Constants.RegisterUserUrl, content);

            return response.IsSuccessStatusCode;
        }

        public static async Task<bool> LoginIdsAsync()
        {
            var browser = DependencyService.Get<IBrowser>();

            var options = new OidcClientOptions
            {
                Authority = Constants.AuthServerUrl,
                ClientId = "kinaunaxamarin",
                RedirectUri = "kinaunaxamarinclients://callback",
                PostLogoutRedirectUri = "kinaunaxamarinclients://callback",
                Scope = "openid profile email firstname middlename lastname timezone viewchild kinaunaprogenyapi kinaunamediaapi",
                Browser = browser,
                ResponseMode = OidcClientOptions.AuthorizeResponseMode.Redirect
            };

            var oidcClient = new OidcClient(options);
            var result = await oidcClient.LoginAsync(new LoginRequest());

            if (result.IsError)
            {
                Debug.WriteLine(result.Error);
                return false;
            }
            else
            {
                ApplicationUser user = IdentityParser.Parse(result.User);
                try
                {
                    TimeZoneInfo.FindSystemTimeZoneById(user.TimeZone);
                }
                catch (Exception)
                {
                    user.TimeZone = TZConvert.WindowsToIana(user.TimeZone);
                }
                try
                {
                    await SecureStorage.SetAsync(Constants.AuthAccessTokenKey, result.AccessToken);
                    await SecureStorage.SetAsync(Constants.AuthAccessTokenExpiresKey, result.AccessTokenExpiration.Ticks.ToString());
                    await SecureStorage.SetAsync(Constants.AuthIdTokenKey, result.IdentityToken);
                    
                    await SecureStorage.SetAsync(Constants.UserNameKey, user.UserName);
                    await SecureStorage.SetAsync(Constants.UserEmailKey, user.Email);
                    await SecureStorage.SetAsync(Constants.UserFirstNameKey, user.FirstName);
                    await SecureStorage.SetAsync(Constants.UserMiddleNameKey, user.MiddleName);
                    await SecureStorage.SetAsync(Constants.UserLastNameKey, user.LastName);
                    await SecureStorage.SetAsync(Constants.UserIdKey, user.Id);
                    await SecureStorage.SetAsync(Constants.UserViewChildKey, user.ViewChild.ToString());
                    await ProgenyService.GetProgeny(user.ViewChild);
                    await UserService.GetUserInfo(user.Email);
                    await SecureStorage.SetAsync(Constants.UserTimezoneKey, user.TimeZone);

                    await RegisterDevice(user.Email);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            return true;
        }

        private static async Task RegisterDevice(string user)
        {
            if (Device.RuntimePlatform == Device.Android)
            {
                string pnsHandle = "";
                string registrationId = "";
                try
                {
                    pnsHandle = await SecureStorage.GetAsync("PnsHandle");
                    registrationId = await SecureStorage.GetAsync("RegistrationId");
                }
                catch (Exception)
                {
                    return;
                }

                if (string.IsNullOrEmpty(pnsHandle) || string.IsNullOrEmpty(registrationId))
                {
                    return;
                }

                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                DeviceRegistration deviceRegistration = new DeviceRegistration();
                deviceRegistration.Platform = "fcm";
                deviceRegistration.Handle = pnsHandle;
                List<string> tags = new List<string>();
                foreach (string str in AzureNotificationsConstants.SubscriptionTags)
                {
                    tags.Add(str);
                }

                string userEmail = await GetUserEmail();
                tags.Add("userEmail:" + userEmail.ToUpper());
                foreach (Progeny progeny in await ProgenyService.GetProgenyList(await GetUserEmail()))
                {
                    tags.Add("progenyId:" + progeny.Id);
                }
                deviceRegistration.Tags = tags.ToArray();
                await client.PutAsync("api/register/" + registrationId, new StringContent(JsonConvert.SerializeObject(deviceRegistration), System.Text.Encoding.UTF8, "application/json")).ConfigureAwait(false);
            }

            if (Device.RuntimePlatform == Device.UWP)
            {
                string registrationId = "";
                string pnsHandle = "";
                try
                {
                    registrationId = await SecureStorage.GetAsync("RegistrationId");
                    pnsHandle = await SecureStorage.GetAsync("PnsHandle");
                }
                catch (Exception)
                {
                    if (string.IsNullOrEmpty(pnsHandle))
                    {
                        return;
                    }
                }

                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);

                if (string.IsNullOrEmpty(registrationId))
                {
                    var response = await client.PostAsync("api/register/", new StringContent("", System.Text.Encoding.UTF8, "application/json")).ConfigureAwait(false);
                    if (response.IsSuccessStatusCode)
                    {
                        registrationId = await response.Content.ReadAsStringAsync();
                        registrationId = registrationId.Substring(1, registrationId.Length - 2);
                        await SecureStorage.SetAsync("RegistrationId", registrationId);
                    }
                }

                DeviceRegistration deviceRegistration = new DeviceRegistration();
                deviceRegistration.Platform = "wns";
                deviceRegistration.Handle = pnsHandle;
                List<string> tags = new List<string>();
                foreach (string str in AzureNotificationsConstants.SubscriptionTags)
                {
                    tags.Add(str);
                }

                string userEmail = await GetUserEmail();
                tags.Add("userEmail:" + userEmail.ToUpper());
                foreach (Progeny progeny in await ProgenyService.GetProgenyList(await GetUserEmail()))
                {
                    tags.Add("progenyId:" + progeny.Id);
                }
                deviceRegistration.Tags = tags.ToArray();
                await client.PutAsync("api/register/" + registrationId, new StringContent(JsonConvert.SerializeObject(deviceRegistration), System.Text.Encoding.UTF8, "application/json")).ConfigureAwait(false);
            }
        }

        private static async Task DeRegisterDevice()
        {
            if (Device.RuntimePlatform == Device.Android)
            {
                string registerId = "";
                try
                {
                    registerId = await SecureStorage.GetAsync("RegistrationId");
                }
                catch (Exception)
                {
                    return;
                }
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                var result = await client.DeleteAsync("api/register/" + registerId).ConfigureAwait(false);
                if (result.IsSuccessStatusCode)
                {
                    SecureStorage.Remove("NotificationRegistrationId");
                }
            }
        }

        public static async Task<string> GetAuthAccessToken()
        {
            try
            {
                string accessToken = await SecureStorage.GetAsync(Constants.AuthAccessTokenKey);
                if (String.IsNullOrEmpty(accessToken))
                {
                    accessToken = "";
                }

                return accessToken;
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }

        public static async Task<string> GetAuthAccessTokenExpires()
        {
            try
            {
                return await SecureStorage.GetAsync(Constants.AuthAccessTokenExpiresKey);
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }

        public static bool IsAccessTokenCurrent(string accessTokenExpires)
        {
            bool accessTokenCurrentParsed = long.TryParse(accessTokenExpires, out long accessTokenTime);
            
            if (accessTokenCurrentParsed)
            {
                if (accessTokenTime > DateTime.UtcNow.Ticks)
                {
                    return true;
                }
            }

            return false;
        }

        public static async Task<string> GetUsername()
        {
            try
            {
                return await SecureStorage.GetAsync(Constants.UserNameKey);
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }

        public static async Task<string> GetUserEmail()
        {
            try
            {
                string userEmail = await SecureStorage.GetAsync(Constants.UserEmailKey);
                if (String.IsNullOrEmpty(userEmail))
                {
                    userEmail = Constants.DummyEmail;
                }
                return userEmail;
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }

        public static async Task<string> GetFullname()
        {
            try
            {
                string fullname;
                fullname = await SecureStorage.GetAsync(Constants.UserFirstNameKey);
                fullname = fullname + " " + await SecureStorage.GetAsync(Constants.UserMiddleNameKey);
                fullname = fullname.Trim() + " " + await SecureStorage.GetAsync(Constants.UserLastNameKey);
                fullname = fullname.Trim();
                return fullname;
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }

        public static async Task<string> GetUserId()
        {
            try
            {
                return await SecureStorage.GetAsync(Constants.UserIdKey);
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }

        public static async Task<string> GetUserTimezone()
        {
            try
            {
                return await SecureStorage.GetAsync(Constants.UserTimezoneKey);
            }
            catch (Exception ex)
            {
                return Constants.DefaultTimeZone;
            }
        }

        public static async Task<string> GetViewChild()
        {
            try
            {
                return await SecureStorage.GetAsync(Constants.UserViewChildKey);
            }
            catch (Exception ex)
            {
                return Constants.DefaultChildId.ToString();
            }
        }

        public static async Task<string> GetLanguage()
        {
            try
            {
                return await SecureStorage.GetAsync("UserLanguage");
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static async Task SetLanguage(string language)
        {
            try
            {
                await SecureStorage.SetAsync("UserLanguage", language);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public static async Task<bool> LogoutIdsAsync()
        {
            await DeRegisterDevice();
            App.Database.ResetAll();
            try
            {
                string pnsHandle = await SecureStorage.GetAsync("PnsHandle");
                SecureStorage.RemoveAll();
                await SecureStorage.SetAsync("PnsHandle", pnsHandle);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            
            string idToken = "";
            try
            {
                idToken = await SecureStorage.GetAsync(Constants.AuthIdTokenKey);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            var browser = DependencyService.Get<IBrowser>();
            var options = new OidcClientOptions
            {
                Authority = Constants.AuthServerUrl,
                ClientId = "kinaunaxamarin",
                RedirectUri = "kinaunaxamarinclients://callback",
                PostLogoutRedirectUri = "kinaunaxamarinclients://callback",
                Scope = "openid profile email firstname middlename lastname timezone viewchild kinaunaprogenyapi kinaunamediaapi",
                Browser = browser,
                ResponseMode = OidcClientOptions.AuthorizeResponseMode.Redirect
            };

            var oidcClient = new OidcClient(options);
            LogoutRequest logoutRequest = new LogoutRequest();
            logoutRequest.IdTokenHint = idToken;
            await oidcClient.LogoutAsync(logoutRequest);
            
            return true;
        }

        public static async Task<string> UploadProfilePicture(string fileName)
        {
            if (ProgenyService.Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.MediaApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);

                var fileBytes = File.ReadAllBytes(fileName);
                MemoryStream stream = new MemoryStream(fileBytes);
                HttpContent fileStreamContent = new StreamContent(stream);

                fileStreamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "file", FileName = "file" };
                fileStreamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                MultipartFormDataContent content = new MultipartFormDataContent();
                content.Add(fileStreamContent);
                var result = await client.PostAsync("api/pictures/uploadprofilepicture/", content).ConfigureAwait(false);

                if (result.IsSuccessStatusCode)
                {
                    string resultString = await result.Content.ReadAsStringAsync();
                    string pictureResultString = Regex.Replace(resultString, @"([|""|])", "");
                    return pictureResultString;
                }
            }

            return "";
        }

        public static async Task<UserInfo> UpdateUserInfo(UserInfo updatedUserInfo)
        {
            if (ProgenyService.Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                var result = await client.PutAsync("api/userinfo/" + updatedUserInfo.UserId, new StringContent(JsonConvert.SerializeObject(updatedUserInfo), System.Text.Encoding.UTF8, "application/json")).ConfigureAwait(false);
                if (result.IsSuccessStatusCode)
                {
                    string resultString = await result.Content.ReadAsStringAsync();
                    UserInfo resultUserInfo = JsonConvert.DeserializeObject<UserInfo>(resultString);

                    await SecureStorage.SetAsync(Constants.UserNameKey, resultUserInfo.UserName);
                    await SecureStorage.SetAsync(Constants.UserFirstNameKey, resultUserInfo.FirstName);
                    await SecureStorage.SetAsync(Constants.UserMiddleNameKey, resultUserInfo.MiddleName);
                    await SecureStorage.SetAsync(Constants.UserLastNameKey, resultUserInfo.LastName);
                    try
                    {
                        TimeZoneInfo.FindSystemTimeZoneById(resultUserInfo.Timezone);
                    }
                    catch (Exception)
                    {
                        resultUserInfo.Timezone = TZConvert.WindowsToIana(resultUserInfo.Timezone);
                    }
                    await SecureStorage.SetAsync(Constants.UserTimezoneKey, resultUserInfo.Timezone);
                    
                    return resultUserInfo;
                }
            }

            return new UserInfo();
        }

        public static async Task<List<MobileNotification>> GetNotificationsList(int count, int start, string language)
        {
            bool online = ProgenyService.Online();
            if (online)
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

                string accessToken = await UserService.GetAuthAccessToken();

                if (String.IsNullOrEmpty(accessToken))
                {

                    return new List<MobileNotification>();
                    
                }
                else
                {
                    client.SetBearerToken(accessToken);

                    var result = await client.GetAsync("api/notifications/latest/" + count + "/" + start + "/" + language).ConfigureAwait(false);

                    string userTimeZone = await GetUserTimezone();
                    if (result.IsSuccessStatusCode)
                    {
                        var notificationsString = await result.Content.ReadAsStringAsync();
                        List<MobileNotification> notificationsList = JsonConvert.DeserializeObject<List<MobileNotification>>(notificationsString);
                        foreach (MobileNotification notif in notificationsList)
                        {
                            notif.Time = TimeZoneInfo.ConvertTimeFromUtc(notif.Time, TimeZoneInfo.FindSystemTimeZoneById(userTimeZone));
                        }
                        await SecureStorage.SetAsync("NotificationsList" + count + "start" + start + "lang" + language, JsonConvert.SerializeObject(notificationsList));
                        return notificationsList;
                    }
                    else
                    {
                        return new List<MobileNotification>();
                    }
                }
            }
            else
            {
                string notificationsString = await SecureStorage.GetAsync("NotificationsList" + count + "start" + start + "lang" + language);
                if (string.IsNullOrEmpty(notificationsString))
                {
                    return new List<MobileNotification>();
                }
                List<MobileNotification> notificationsList = JsonConvert.DeserializeObject<List<MobileNotification>>(notificationsString);
                return notificationsList;
            }
        }

        public static async Task<MobileNotification> UpdateNotification(MobileNotification notification)
        {
            if (ProgenyService.Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                var result = await client.PutAsync("api/notifications/" + notification.NotificationId, new StringContent(JsonConvert.SerializeObject(notification), System.Text.Encoding.UTF8, "application/json")).ConfigureAwait(false);
                if (result.IsSuccessStatusCode)
                {
                    string resultString = await result.Content.ReadAsStringAsync();
                    MobileNotification resultItem = JsonConvert.DeserializeObject<MobileNotification>(resultString);
                    return resultItem;
                }
            }

            return notification;
        }

        public static async Task<MobileNotification> DeleteNotification(MobileNotification notification)
        {
            if (ProgenyService.Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                var result = await client.DeleteAsync("api/notifications/" + notification.NotificationId).ConfigureAwait(false);
                if (result.IsSuccessStatusCode)
                {
                    MobileNotification deleteNotification = new MobileNotification();
                    deleteNotification.NotificationId = 0;
                    return deleteNotification;
                }
            }

            return notification;
        }
    }
}
