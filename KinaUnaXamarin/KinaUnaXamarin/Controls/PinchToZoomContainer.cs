using System;
using KinaUnaXamarin.Extensions;
using Xamarin.Forms;

namespace KinaUnaXamarin.Controls
{
    // Original source: https://forums.xamarin.com/discussion/74168/full-screen-image-viewer-with-pinch-to-zoom-pan-to-move-tap-to-show-captions-for-xamarin-forms/p2

    public class PinchToZoomContainer : ContentView
    {
        private double _startScale, _currentScale;
        private double _startX, _startY;
        private double _xOffset, _yOffset;
        private bool _isPinching;
        private double _lastScale;
        private DateTime _lastPinch;

        public double MinScale { get; set; } = 1;
        public double MaxScale { get; set; } = 8;

        public PinchToZoomContainer()
        {
            var tap = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
            tap.Tapped += OnTapped;
            GestureRecognizers.Add(tap);

            var pinchGesture = new PinchGestureRecognizer();
            pinchGesture.PinchUpdated += OnPinchUpdated;
            GestureRecognizers.Add(pinchGesture);

            var pan = new PanGestureRecognizer();
            pan.PanUpdated += OnPanUpdated;
            GestureRecognizers.Add(pan);
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            RestoreScaleValues();
            Content.AnchorX = 0.5;
            Content.AnchorY = 0.5;
            base.OnSizeAllocated(width, height);
        }

        private void RestoreScaleValues()
        {
            Content.ScaleTo(MinScale, 250, Easing.CubicInOut);
            Content.TranslateTo(0, 0, 250, Easing.CubicInOut);

            _currentScale = MinScale;
            _xOffset = Content.TranslationX = 0;
            _yOffset = Content.TranslationY = 0;
        }

        private void OnTapped(object sender, EventArgs e)
        {
            if (Content.Scale > MinScale)
            {
                RestoreScaleValues();
            }
            else
            {
                //todo: Add tap position somehow
                StartScaling();
                ExecuteScaling(2, .5, .5);
                EndGesture();
            }
            if (Math.Abs(_currentScale - 1) < 0.025)
            {
                IsZoomed = false;
            }
            else
            {
                IsZoomed = true;
            }
        }

        private void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
        {
            if (!IsScaleEnabled)
            {
                return;
            }

            _isPinching = true;
            switch (e.Status)
            {
                case GestureStatus.Started:
                    _lastScale = e.Scale;
                    StartScaling();
                    break;

                case GestureStatus.Running:
                    if (e.Scale < 0 || Math.Abs(_lastScale - e.Scale) > (_lastScale * 1.3) - _lastScale)
                    {
                        return;
                    }
                    _lastScale = e.Scale;
                    ExecuteScaling(e.Scale, e.ScaleOrigin.X, e.ScaleOrigin.Y);
                    break;

                case GestureStatus.Completed:
                    EndGesture();
                    _isPinching = false;
                    _lastPinch = DateTime.UtcNow;
                    break;
            }
        }

        private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            if (!IsTranslateEnabled || _isPinching)
            {
                return;
            }

            if (_lastPinch + TimeSpan.FromMilliseconds(300) > DateTime.UtcNow)
            {
                return;
            }

            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    _startX = e.TotalX;
                    _startY = e.TotalY;

                    Content.AnchorX = 0;
                    Content.AnchorY = 0;

                    break;

                case GestureStatus.Running:
                    if (!_isPinching)
                    {
                        var maxTranslationX = Content.Scale * Content.Width - Content.Width;
                        Content.TranslationX = Math.Min(0, Math.Max(-maxTranslationX, _xOffset + e.TotalX - _startX));

                        var maxTranslationY = Content.Scale * Content.Height - Content.Height;
                        Content.TranslationY = Math.Min(0, Math.Max(-maxTranslationY, _yOffset + e.TotalY - _startY));

                    }

                    break;

                case GestureStatus.Completed:
                    EndGesture();
                    break;
            }
        }

        private void StartScaling()
        {
            _startScale = Content.Scale;

            Content.AnchorX = 0;
            Content.AnchorY = 0;
        }

        private void ExecuteScaling(double scale, double x, double y)
        {
            _currentScale += (scale - 1) * _startScale;
            _currentScale = Math.Max(MinScale, _currentScale);
            _currentScale = Math.Min(MaxScale, _currentScale);

            var deltaX = (Content.X + _xOffset) / Width;
            var deltaWidth = Width / (Content.Width * _startScale);
            var originX = (x - deltaX) * deltaWidth;

            var deltaY = (Content.Y + _yOffset) / Height;
            var deltaHeight = Height / (Content.Height * _startScale);
            var originY = (y - deltaY) * deltaHeight;

            var targetX = _xOffset - (originX * Content.Width) * (_currentScale - _startScale);
            var targetY = _yOffset - (originY * Content.Height) * (_currentScale - _startScale);

            Content.TranslationX = targetX.Clamp(-Content.Width * (_currentScale - 1), 0);
            Content.TranslationY = targetY.Clamp(-Content.Height * (_currentScale - 1), 0);

            Content.Scale = _currentScale;
        }

        private void EndGesture()
        {
            _xOffset = Content.TranslationX;
            _yOffset = Content.TranslationY;
            if (Math.Abs(_currentScale - 1) < 0.025)
            {
                IsZoomed = false;
            }
            else
            {
                IsZoomed = true;
            }
        }

        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            Scale = MinScale;
            TranslationX = TranslationY = 0;
            AnchorX = AnchorY = 0;
            return base.OnMeasure(widthConstraint, heightConstraint);
        }

        #region IsScaleEnabled property

        /// <summary>
        /// Identifies the <see cref="IsScaleEnabledProperty" /> property.
        /// </summary>
        public static readonly BindableProperty IsScaleEnabledProperty =
            BindableProperty.Create(nameof(IsScaleEnabled), typeof(bool), typeof(PinchToZoomContainer), default(bool)); // BindableProperty.Create<MultiTouchBehavior, bool>(w => w.IsScaleEnabled, default(bool));


        /// <summary>
        /// Identifies the <see cref="IsScaleEnabled" /> dependency / bindable property.
        /// </summary>
        public bool IsScaleEnabled
        {
            get { return (bool)GetValue(IsScaleEnabledProperty); }
            set { SetValue(IsScaleEnabledProperty, value); }
        }

        #endregion

        #region IsTranslateEnabled property

        /// <summary>
        /// Identifies the <see cref="IsTranslateEnabledProperty" /> property.
        /// </summary>
        public static readonly BindableProperty IsTranslateEnabledProperty =
            BindableProperty.Create(nameof(IsTranslateEnabled), typeof(bool), typeof(PinchToZoomContainer), default(bool)); // BindableProperty.Create<MultiTouchBehavior, bool>(w => w.IsTranslateEnabled, default(bool));

        /// <summary>
        /// Identifies the <see cref="IsTranslateEnabled" /> dependency / bindable property.
        /// </summary>
        public bool IsTranslateEnabled
        {
            get { return (bool)GetValue(IsTranslateEnabledProperty); }
            set { SetValue(IsTranslateEnabledProperty, value); }
        }

        #endregion

        #region IsZoomed property

        /// <summary>
        /// Identifies the <see cref="IsZoomedProperty" /> property.
        /// </summary>
        public static readonly BindableProperty IsZoomedProperty =
            BindableProperty.Create(nameof(IsZoomed), typeof(bool), typeof(PinchToZoomContainer), default(bool), propertyChanged: OnIsZoomedPropertyChanged);


        // See https://forums.xamarin.com/discussion/96459/xamarin-forms-parent-bindingcontext-in-datatemplate-in-the-xaml
        private static void OnIsZoomedPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue != oldValue && newValue != null)
            {
                if (bindable is PinchToZoomContainer pinchToZoomContainer)
                {
                    if (newValue is bool newBool)
                    {
                        pinchToZoomContainer.IsZoomed = newBool;
                        if (newBool == true)
                        {
                            pinchToZoomContainer.IsTranslateEnabled = true;
                        }
                        else
                        {
                            pinchToZoomContainer.IsTranslateEnabled = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Identifies the <see cref="IsZoomed" /> dependency / bindable property.
        /// </summary>
        public bool IsZoomed
        {
            get { return (bool)GetValue(IsZoomedProperty); }
            set
            {
                SetValue(IsZoomedProperty, value);
                if (value == false)
                {
                    RestoreScaleValues();
                }
            }
        }

        #endregion
    }
}
