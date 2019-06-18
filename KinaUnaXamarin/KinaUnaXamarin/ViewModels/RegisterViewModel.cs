using System.Windows.Input;
using KinaUnaXamarin.Services;
using Xamarin.Forms;

namespace KinaUnaXamarin.ViewModels
{
    class RegisterViewModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }

        public string Message { get; set; }

        public ICommand RegisterCommand
        {
            get
            {
                return new Command(async () =>
                {
                    var isSuccess = await UserService.RegisterAsync(Email, Password, ConfirmPassword);
                    if (isSuccess)
                    {
                        Message = "Registered successfully";
                    }
                    else
                    {
                        Message = "Registration error. Try again later.";
                    }
                });
            }
        }
    }
}
