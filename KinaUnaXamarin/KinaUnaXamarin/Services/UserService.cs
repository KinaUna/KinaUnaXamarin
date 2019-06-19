using System;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
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
                    await SecureStorage.SetAsync("UserInfo" + userEmail, JsonConvert.SerializeObject(userinfo));
                    return userinfo;
                }
                else
                {
                    // Todo: Handle errors
                    string userinfoString = await SecureStorage.GetAsync("UserInfo" + userEmail);
                    if (!string.IsNullOrEmpty(userinfoString))
                    {
                        UserInfo userinfo = JsonConvert.DeserializeObject<UserInfo>(userinfoString);
                        return userinfo;
                    }

                    return OfflineDefaultData.DefaultUserInfo;
                }
            }
            else
            {
                string userinfoString = await SecureStorage.GetAsync("UserInfo" + userEmail);
                if (!string.IsNullOrEmpty(userinfoString))
                {
                    UserInfo userinfo = JsonConvert.DeserializeObject<UserInfo>(userinfoString);
                    return userinfo;
                }

                return OfflineDefaultData.DefaultUserInfo;
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
                try
                {
                    await SecureStorage.SetAsync(Constants.AuthAccessTokenKey, result.AccessToken);
                    await SecureStorage.SetAsync(Constants.AuthAccessTokenExpiresKey, result.AccessTokenExpiration.Ticks.ToString());
                    await SecureStorage.SetAsync(Constants.AuthIdTokenKey, result.IdentityToken);
                    ApplicationUser user = IdentityParser.Parse(result.User);
                    await SecureStorage.SetAsync(Constants.UserNameKey, user.UserName);
                    await SecureStorage.SetAsync(Constants.UserEmailKey, user.Email);
                    await SecureStorage.SetAsync(Constants.UserFirstNameKey, user.FirstName);
                    await SecureStorage.SetAsync(Constants.UserMiddleNameKey, user.MiddleName);
                    await SecureStorage.SetAsync(Constants.UserLastNameKey, user.LastName);
                    await SecureStorage.SetAsync(Constants.UserIdKey, user.Id);
                    await SecureStorage.SetAsync(Constants.UserViewChildKey, user.ViewChild.ToString());
                    await ProgenyService.GetProgeny(user.ViewChild);
                    await UserService.GetUserInfo(user.Email);
                    try
                    {
                        TimeZoneInfo.FindSystemTimeZoneById(user.TimeZone);
                    }
                    catch (Exception)
                    {
                        user.TimeZone = TZConvert.WindowsToIana(user.TimeZone);
                    }
                    await SecureStorage.SetAsync(Constants.UserTimezoneKey, user.TimeZone);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            return true;
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
                return "Error: " + ex.Message;
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
                return "Error: " + ex.Message;
            }
        }


        public static async Task<bool> LogoutIdsAsync()
        {
            var browser = DependencyService.Get<IBrowser>();
            
            string idToken = "";
            try
            {
                idToken = await SecureStorage.GetAsync(Constants.AuthIdTokenKey);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

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

            try
            {
                SecureStorage.RemoveAll();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return true;
        }
    }
}
