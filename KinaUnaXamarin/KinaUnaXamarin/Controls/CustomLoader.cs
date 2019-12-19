using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace KinaUnaXamarin.Controls
{
    // Original source: https://github.com/lubiepomaranczki/CustomLoader/blob/master/CustomLoader/CustomLoader.cs
    public class CustomLoader : Image
    {
        #region Fields

        private CancellationTokenSource _cancellationToken;

        #endregion

        #region Binadables

        public static BindableProperty IsRunningProperty = BindableProperty.Create(
            propertyName: nameof(IsRunning),
            returnType: typeof(bool),
            declaringType: typeof(CustomLoader),
            defaultValue: false);

        public bool IsRunning
        {
            get => (bool)GetValue(IsRunningProperty);
            set => SetValue(IsRunningProperty, value);
        }

        public static BindableProperty RotationLenghtProperty = BindableProperty.Create(
            propertyName: nameof(RotationLength),
            returnType: typeof(int),
            declaringType: typeof(CustomLoader),
            defaultValue: 2500);

        public int RotationLength
        {
            get => (int)GetValue(RotationLenghtProperty);
            set => SetValue(RotationLenghtProperty, value);
        }

        public static BindableProperty EasingProperty = BindableProperty.Create(
            propertyName: nameof(Easing),
            returnType: typeof(Easing),
            declaringType: typeof(CustomLoader),
            defaultValue: Easing.CubicInOut);

        public Easing Easing
        {
            get => (Easing)GetValue(EasingProperty);
            set => SetValue(EasingProperty, value);
        }

        #endregion

        #region Constructor(s)

        public CustomLoader()
        {
            Opacity = 0;
        }

        #endregion

        #region Overrides

        protected override async void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == IsRunningProperty.PropertyName)
            {
                if (IsRunning)
                {
                    await this.FadeTo(1);
                    _cancellationToken = new CancellationTokenSource();
                    await RotateElement(this, _cancellationToken.Token);
                }
                else
                {
                    _cancellationToken?.Cancel();
                    await this.FadeTo(0);
                }
            }
        }

        #endregion

        #region Methods

        private async Task RotateElement(VisualElement element, CancellationToken cancellation)
        {
            while (!cancellation.IsCancellationRequested)
            {
                await element.RotateTo(360, (uint)RotationLength, this.Easing);
                await element.RotateTo(0, 0);
            }
        }

        #endregion
    }
}
