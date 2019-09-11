using System;
using System.Collections.Generic;
using KinaUnaXamarin.Models.KinaUna;
using Plugin.Multilingual;
using Xamarin.Forms;

namespace KinaUnaXamarin.Helpers
{
    public class FriendTypeConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return GetFriendTypeString(value);
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return GetFriendTypeInt(value);
        }

        string GetFriendTypeString(object parameter)
        {
            if (parameter is int intParameter)
            {
                List<string> friendTypeList = new List<string>();
                var ci = CrossMultilingual.Current.CurrentCultureInfo.TwoLetterISOLanguageName;
                if (ci == "da")
                {
                    friendTypeList.Add("Personlige venner");
                    friendTypeList.Add("Legetøj/Dyr");
                    friendTypeList.Add("Forældre");
                    friendTypeList.Add("Familie");
                    friendTypeList.Add("Omsorgspersoner");
                }
                else
                {
                    if (ci == "de")
                    {
                        friendTypeList.Add("Persönliche Freunde");
                        friendTypeList.Add("Spielzeuge/Tiere");
                        friendTypeList.Add("Eltern");
                        friendTypeList.Add("Familie");
                        friendTypeList.Add("Betreuer");
                    }
                    else
                    {
                        friendTypeList.Add("Personal Friends");
                        friendTypeList.Add("Toy/Animal");
                        friendTypeList.Add("Parents");
                        friendTypeList.Add("Family");
                        friendTypeList.Add("Caretakers");
                    }
                }

                if (intParameter < friendTypeList.Count)
                {
                    return friendTypeList[intParameter];
                }
            }
            return "";
        }

        int GetFriendTypeInt(object parameter)
        {
            if (parameter is string stringParameter)
            {
                List<string> friendTypeList = new List<string>();
                var ci = CrossMultilingual.Current.CurrentCultureInfo.TwoLetterISOLanguageName;
                if (ci == "da")
                {
                    friendTypeList.Add("Personlige venner");
                    friendTypeList.Add("Legetøj/Dyr");
                    friendTypeList.Add("Forældre");
                    friendTypeList.Add("Familie");
                    friendTypeList.Add("Omsorgspersoner");
                }
                else
                {
                    if (ci == "de")
                    {
                        friendTypeList.Add("Persönliche Freunde");
                        friendTypeList.Add("Spielzeuge/Tiere");
                        friendTypeList.Add("Eltern");
                        friendTypeList.Add("Familie");
                        friendTypeList.Add("Betreuer");
                    }
                    else
                    {
                        friendTypeList.Add("Personal Friends");
                        friendTypeList.Add("Toy/Animal");
                        friendTypeList.Add("Parents");
                        friendTypeList.Add("Family");
                        friendTypeList.Add("Caretakers");
                    }
                }

                if (friendTypeList.Contains(stringParameter))
                {
                    return friendTypeList.IndexOf(stringParameter);
                }
            }
            return 0;
        }
    }
}
