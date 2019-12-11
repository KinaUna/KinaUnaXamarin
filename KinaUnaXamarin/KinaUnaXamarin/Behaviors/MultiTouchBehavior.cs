// Original inspiration:
// https://github.com/davidezordan/multi-touch
// and
// https://stackoverflow.com/questions/40181090/xamarin-forms-pinch-and-pan-together

using System;
using System.Threading.Tasks;
using FFImageLoading.Forms;
using KinaUnaXamarin.Extensions;
using Xamarin.Forms;

namespace KinaUnaXamarin.Behaviors
{
    /// <summary>
    /// Implements Multi-Touch manipulations
    /// Uses code from original Xamarin Forms samples: https://github.com/xamarin/xamarin-forms-samples/tree/master/WorkingWithGestures/PinchGesture
    /// </summary>
    public class MultiTouchBehavior : Behavior<View>
    {
        #region Fields

        private double _currentScale = 1, _startScale = 1, _xOffset, _yOffset, _lastScale, _startX, _startY;
        private bool _isPinching;
        private PinchGestureRecognizer _pinchGestureRecognizer;
        private PanGestureRecognizer _panGestureRecognizer;
        private TapGestureRecognizer _tapGestureRecognizer;
        private ContentView _parent;

        private View _associatedObject;
        private DateTime _lastPinch;
        private double _imageHeight;
        private double _imageWidth;
        

        #endregion

        /// <summary>
        /// Occurs when BindingContext is changed: used to initialise the Gesture Recognizers.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The event parameters.</param>
        private void AssociatedObjectBindingContextChanged(object sender, EventArgs e)
        {
            _parent = _associatedObject.Parent as ContentView;
            
            // _parent?.GestureRecognizers.Remove(_panGestureRecognizer);
            // _parent?.GestureRecognizers.Add(_panGestureRecognizer);
            _parent?.GestureRecognizers.Remove(_pinchGestureRecognizer);
            _parent?.GestureRecognizers.Add(_pinchGestureRecognizer);
            _parent?.GestureRecognizers.Remove(_tapGestureRecognizer);
            _parent?.GestureRecognizers.Add(_tapGestureRecognizer);
        }

        /// <summary>
        /// Cleanup the events.
        /// </summary>
        private void CleanupEvents()
        {
            _pinchGestureRecognizer.PinchUpdated -= OnPinchUpdated;
            _panGestureRecognizer.PanUpdated -= OnPanUpdated;
            _tapGestureRecognizer.Tapped -= OnTapped;
            _associatedObject.BindingContextChanged -= AssociatedObjectBindingContextChanged;
        }

        /// <summary>
        /// Initialise the events.
        /// </summary>
        private void InitializeEvents()
        {
            CleanupEvents();
            _pinchGestureRecognizer.PinchUpdated += OnPinchUpdated;
            _panGestureRecognizer.PanUpdated += OnPanUpdated;
            _tapGestureRecognizer.Tapped += OnTapped;
            _associatedObject.BindingContextChanged += AssociatedObjectBindingContextChanged;
        }

        /// <summary>
        /// Initialise the Gesture Recognizers.
        /// </summary>
        private void InitialiseRecognizers()
        {
            _pinchGestureRecognizer = new PinchGestureRecognizer();
            _panGestureRecognizer = new PanGestureRecognizer();
            _tapGestureRecognizer = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
        }

        /// <summary>
        /// Occurs when Behavior is attached to the View: initialises fields, properties and events.
        /// </summary>
        protected override void OnAttachedTo(View associatedObject)
        {
            InitialiseRecognizers();
            _associatedObject = associatedObject;
            InitializeEvents();
            base.OnAttachedTo(associatedObject);
        }

        /// <summary>
        /// Occurs when Behavior is detached from the View: cleanup fields, properties and events.
        /// </summary>
        protected override void OnDetachingFrom(View associatedObject)
        {
            CleanupEvents();

            _parent = null;
            _pinchGestureRecognizer = null;
            _panGestureRecognizer = null;
            _tapGestureRecognizer = null;
            _associatedObject = null;

            base.OnDetachingFrom(associatedObject);
        }

        private void OnTapped(object sender, EventArgs e)
        {
            

            if (_parent.Content.Scale > 1)
            {
                ResetZoom();
                System.Threading.Thread.Sleep(200);
                IsZoomed = false;
                IsTranslateEnabled = false;
            }
            else
            {
                // Todo: Ease in/out.
                StartScaling();
                ExecuteScaling(2, .5, .5);
                EndGesture();
                IsZoomed = true;
                IsTranslateEnabled = true;
                IsScaleEnabled = true;
            }
            
        }
        
        /// <summary>
        /// Implements Pan/Translate.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The event parameters.</param>
        private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            if (_parent == null || _isPinching)
            {
                return;
            }

            if (!IsTranslateEnabled)
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

                    _parent.Content.AnchorX = 0;
                    _parent.Content.AnchorY = 0;

                    break;

                case GestureStatus.Running:
                    // Todo: Limit movement more precisely, as the container and image most likely don't have the same proportions.
                    var maxTranslationX = _parent.Content.Scale * _parent.Content.Width - _parent.Content.Width;
                    _parent.Content.TranslationX = Math.Min(0, Math.Max(-maxTranslationX, _xOffset + e.TotalX - _startX));
                    var maxTranslationY = _parent.Content.Scale * _parent.Content.Height - _parent.Content.Height;
                    _parent.Content.TranslationY = Math.Min(0, Math.Max(-maxTranslationY, _yOffset + e.TotalY - _startY));
                    break;

                case GestureStatus.Completed:
                    if (!_isPinching)
                    {
                        EndGesture();
                    }
                    break;
            }
        }
        
        /// <summary>
        /// Implements Pinch/Zoom.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The event parameters.</param>
        private void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
        {
            if (_parent == null)
            {
                return;
            }

            if (!IsScaleEnabled)
            {
                return;
            }
            
            switch (e.Status)
            {
                case GestureStatus.Started:
                    _isPinching = true;
                    _startScale = _parent.Content.Scale;
                    _parent.Content.AnchorX = 0;
                    _parent.Content.AnchorY = 0;
                    _lastScale = e.Scale;
                    IsZoomed = true;
                    break;

                case GestureStatus.Running:
                    _isPinching = true;
                    if (e.Scale < 0 || Math.Abs(_lastScale - e.Scale) > (_lastScale * 1.3) - _lastScale)
                    {
                        return;
                    }
                    _lastScale = e.Scale;
                    
                    ExecuteScaling(e.Scale, e.ScaleOrigin.X, e.ScaleOrigin.Y);
                    
                    break;

                case GestureStatus.Completed:
                    EndGesture();
                    if (Math.Abs(_currentScale - 1) < 0.025)
                    {
                        IsZoomed = false;
                    }
                    else
                    {
                        IsZoomed = true;
                    }
                    _lastPinch = DateTime.UtcNow;
                    _isPinching = false;
                    break;
            }
        }

        private void StartScaling()
        {
            _startScale = _parent.Content.Scale;
            if (Device.RuntimePlatform == Device.UWP)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    _parent.Content.AnchorX = 0;
                    _parent.Content.AnchorY = 0;
                });
            }
            else
            {
                _parent.Content.AnchorX = 0;
                _parent.Content.AnchorY = 0;
            }
            
        }

        private void ExecuteScaling(double scale, double x, double y)
        {
            _currentScale += (scale - 1) * _startScale;
            _currentScale = Math.Max(1, _currentScale);

            var renderedX = _parent.Content.X + _xOffset;
            var deltaX = renderedX / _parent.Width;
            var deltaWidth = _parent.Width / (_parent.Content.Width * _startScale);
            var originX = (x - deltaX) * deltaWidth;

            var renderedY = _parent.Content.Y + _yOffset;
            var deltaY = renderedY / _parent.Height;
            var deltaHeight = _parent.Height / (_parent.Content.Height * _startScale);
            var originY = (y - deltaY) * deltaHeight;

            var targetX = _xOffset - (originX * _parent.Content.Width) * (_currentScale - _startScale);
            var targetY = _yOffset - (originY * _parent.Content.Height) * (_currentScale - _startScale);

            _parent.Content.TranslationX = targetX.Clamp(-_parent.Content.Width * (_currentScale - 1), 0);
            _parent.Content.TranslationY = targetY.Clamp(-_parent.Content.Height * (_currentScale - 1), 0);

            _parent.Content.Scale = _currentScale;
        }

        private void EndGesture()
        {
            _xOffset = _parent.Content.TranslationX;
            _yOffset = _parent.Content.TranslationY;
        }

        private void ResetZoom()
        {
            // Todo: Ease in/out.
            double tempScale = _currentScale;
            StartScaling();
            ExecuteScaling(1, 0, 0);
            EndGesture();
            _currentScale = 1;
            _xOffset = _parent.Content.TranslationX = 0;
            _yOffset = _parent.Content.TranslationY = 0;
        }

        /// <summary>
        /// Initialize the behavior when OnAppearing is executed.
        /// </summary>
        public void OnAppearing()
        {
            AssociatedObjectBindingContextChanged(_associatedObject, null);
            if (_parent.Content is CachedImage cachedImage)
            {
                _imageHeight = cachedImage.Height;
                _imageWidth = cachedImage.Width;
                //ResetZoom();
            }
        }

        public async Task OnDisAppearing()
        {
            await Task.Factory.StartNew(() => ResetZoom());
            // ResetZoom();
        }

        #region IsScaleEnabled property

        /// <summary>
        /// Identifies the <see cref="IsScaleEnabledProperty" /> property.
        /// </summary>
        public static readonly BindableProperty IsScaleEnabledProperty =
            BindableProperty.Create(nameof(IsScaleEnabled), typeof(bool), typeof(MultiTouchBehavior), default(bool)); // BindableProperty.Create<MultiTouchBehavior, bool>(w => w.IsScaleEnabled, default(bool));


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
            BindableProperty.Create(nameof(IsTranslateEnabled), typeof(bool), typeof(MultiTouchBehavior), default(bool)); // BindableProperty.Create<MultiTouchBehavior, bool>(w => w.IsTranslateEnabled, default(bool));

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
            BindableProperty.Create(nameof(IsZoomed), typeof(bool), typeof(MultiTouchBehavior), default(bool), propertyChanged: OnIsZoomedPropertyChanged);

        
        // See https://forums.xamarin.com/discussion/96459/xamarin-forms-parent-bindingcontext-in-datatemplate-in-the-xaml
        private static void OnIsZoomedPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue != oldValue && newValue != null)
            {
                if (bindable is MultiTouchBehavior multiTouchBehavior)
                {
                    if (newValue is bool newBool)
                    {
                        multiTouchBehavior.IsZoomed = newBool;
                        if (newBool == true)
                        {
                            multiTouchBehavior.IsTranslateEnabled = true;
                        }
                        else
                        {
                            multiTouchBehavior.IsTranslateEnabled = false;
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
                    _parent?.GestureRecognizers.Remove(_panGestureRecognizer);
                    ResetZoom();
                }
                else
                {
                    _parent?.GestureRecognizers.Remove(_panGestureRecognizer);
                    _parent?.GestureRecognizers.Add(_panGestureRecognizer);
                }
            }
        }

        #endregion

        #region ContainerHeight property

        /// <summary>
        /// Identifies the <see cref="ContainerHeightProperty" /> property.
        /// </summary>
        public static readonly BindableProperty ContainerHeightProperty =
            BindableProperty.Create(nameof(ContainerHeight), typeof(double), typeof(MultiTouchBehavior), default(double), propertyChanged: OnContainerHeightPropertyChanged);


        // See https://forums.xamarin.com/discussion/96459/xamarin-forms-parent-bindingcontext-in-datatemplate-in-the-xaml
        private static void OnContainerHeightPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue != oldValue && newValue != null)
            {
                if (bindable is MultiTouchBehavior multiTouchBehavior)
                {
                    if (newValue is double newdouble)
                    {
                        multiTouchBehavior.ContainerHeight = newdouble;
                    }
                }
            }
        }

        /// <summary>
        /// Identifies the <see cref="ContainerHeight" /> dependency / bindable property.
        /// </summary>
        public double ContainerHeight
        {
            get { return (double)GetValue(ContainerHeightProperty); }
            set
            {
                SetValue(ContainerHeightProperty, value);
            }
        }

        #endregion

        #region ContainerWidth property

        /// <summary>
        /// Identifies the <see cref="ContainerWidthProperty" /> property.
        /// </summary>
        public static readonly BindableProperty ContainerWidthProperty =
            BindableProperty.Create(nameof(ContainerWidth), typeof(double), typeof(MultiTouchBehavior), default(double), propertyChanged: OnContainerWidthPropertyChanged);


        // See https://forums.xamarin.com/discussion/96459/xamarin-forms-parent-bindingcontext-in-datatemplate-in-the-xaml
        private static void OnContainerWidthPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue != oldValue && newValue != null)
            {
                if (bindable is MultiTouchBehavior multiTouchBehavior)
                {
                    if (newValue is double newdouble)
                    {
                        multiTouchBehavior.ContainerWidth = newdouble;
                    }
                }
            }
        }

        /// <summary>
        /// Identifies the <see cref="ContainerWidth" /> dependency / bindable property.
        /// </summary>
        public double ContainerWidth
        {
            get { return (double)GetValue(ContainerWidthProperty); }
            set
            {
                SetValue(ContainerWidthProperty, value);
            }
        }

        #endregion
    }
}
