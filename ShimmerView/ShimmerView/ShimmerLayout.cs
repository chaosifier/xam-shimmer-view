using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Xamarin.Forms;

namespace ShimmerView
{
    public class ShimmerLayout : ContentView
    {
        #region WaveColor
        public Color WaveColor
        {
            get { return (Color)GetValue(WaveColorProperty); }
            set { SetValue(WaveColorProperty, value); }
        }
        private static void WaveColorChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ShimmerLayout shimmerLayout)
            {
              //  shimmerLayout.WaveColor = (Color)newValue;
            }
        }
        public static readonly BindableProperty WaveColorProperty = BindableProperty.Create(nameof(WaveColor), typeof(Color), typeof(ShimmerLayout), Color.Gray, BindingMode.Default, null, WaveColorChanged);
        #endregion

        #region BackgroundColor
        public new Color BackgroundColor
        {
            get { return (Color)GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }
        private static void BackgroundColorChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ShimmerLayout shimmerLayout)
            {
               // shimmerLayout.BackgroundColor = (Color)newValue;
            }
        }
        public static new readonly BindableProperty BackgroundColorProperty = BindableProperty.Create(nameof(BackgroundColor), typeof(Color), typeof(ShimmerLayout), Color.LightGray, BindingMode.Default, null, BackgroundColorChanged);
        #endregion

        #region WaveSize
        public float WaveSize
        {
            get { return (float)GetValue(WaveSizeProperty); }
            set { SetValue(WaveSizeProperty, value); }
        }
        private static void WaveSizeChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ShimmerLayout shimmerLayout)
            {
               // shimmerLayout.WaveSize = (float)newValue;
            }
        }
        public static readonly BindableProperty WaveSizeProperty = BindableProperty.Create(nameof(WaveSize), typeof(float), typeof(ShimmerLayout), .15f, BindingMode.Default, null, WaveSizeChanged);
        #endregion

        #region Speed
        public float Speed
        {
            get { return (float)GetValue(SpeedProperty); }
            set { SetValue(SpeedProperty, value); }
        }
        private static void SpeedChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ShimmerLayout shimmerLayout)
            {
                //shimmerLayout.Speed = (float)newValue;
            }
        }
        public static readonly BindableProperty SpeedProperty = BindableProperty.Create(nameof(Speed), typeof(float), typeof(ShimmerLayout), 1f, BindingMode.Default, null, SpeedChanged);
        #endregion

        #region WaveAngle
        public int WaveAngle
        {
            get { return (int)GetValue(WaveAngleProperty); }
            set { SetValue(WaveAngleProperty, value); }
        }
        private static void WaveAngleChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ShimmerLayout shimmerLayout)
            {
               // shimmerLayout.WaveAngle = (int)newValue;
            }
        }
        public static readonly BindableProperty WaveAngleProperty = BindableProperty.Create(nameof(WaveAngle), typeof(int), typeof(ShimmerLayout), 45, BindingMode.Default, null, WaveAngleChanged);
        #endregion

        private Grid _shimmerGrid;
        public Grid ShimmerGrid
        {
            get { return _shimmerGrid; }
            set
            {
                _shimmerGrid = value;
                _gridContainer.Content = value;
            }
        }

        public bool _animationRunning;
        private float _wavePosition = 0f;
        private SKCanvasView _canvas;
        private AbsoluteLayout _absoluteLayout;
        private ContentView _gridContainer;

        public ShimmerLayout()
        {
            _gridContainer = new ContentView() { HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };
            _absoluteLayout = new AbsoluteLayout() { HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };
            Content = _absoluteLayout;

            _absoluteLayout.Children.Add(_gridContainer, new Rectangle(0, 0, 1,1), AbsoluteLayoutFlags.All);

            _canvas = new SKCanvasView()
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand
            };
            _absoluteLayout.Children.Add(_canvas, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All);

            _canvas.PaintSurface += _canvas_PaintSurface;

            StartAnimation();
        }

        private void _canvas_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            float startPosX = 0f;
            float startPosY = 0f;
            float endPosX = info.Width;
            float endPosY = (float)Math.Tan(WaveAngle) * endPosX;

            canvas.Clear();

            var scale = e.Info.Width / _canvas.Width;
            canvas.Scale((float)scale);

            using (SKPaint paint = new SKPaint())

            {
                SKRect rect = new SKRect(0, 0, info.Width, info.Height);

                float leftPosition = _wavePosition - WaveSize;
                float rightPosition = _wavePosition + WaveSize;

                // Create linear gradient from upper-left to lower-right
                paint.Shader = SKShader.CreateLinearGradient(
                                    new SKPoint(startPosX, startPosY),
                                    new SKPoint(endPosX, endPosY),
                                    new SKColor[] { BackgroundColor.ToSKColor(), WaveColor.ToSKColor(), BackgroundColor.ToSKColor() },
                                    new float[] { leftPosition, _wavePosition, rightPosition },
                                    SKShaderTileMode.Clamp);


                var mainPath = new SKPath();
                mainPath.AddRect(rect);

                var childPath = new SKPath();
                foreach (var childView in ShimmerGrid.Children)
                {
                    if (childView is ShimmerCell theView)
                    {
                        AddViewPath(ref childPath, theView);
                    }
                }

                canvas.ClipPath(childPath);

                canvas.DrawPath(mainPath, paint);
            }
        }

        private void AddViewPath(ref SKPath pathToAddTo, ShimmerCell theView)
        {
            pathToAddTo.AddRoundRect(theView.Bounds.ToSKRect(), (float)theView.CornerRadius.TopLeft, (float)theView.CornerRadius.TopRight, SKPathDirection.CounterClockwise);
        }

        public void StartAnimation()
        {
            _animationRunning = true;
            Device.StartTimer(TimeSpan.FromMilliseconds(20), () =>
            {
                if (_wavePosition > 1f)
                {
                    _wavePosition = -0.2f;
                }
                else
                {
                    _wavePosition += .01f * Speed;
                }
                _canvas.InvalidateSurface();
                return _animationRunning;
            });
        }

        public void StopAnimation()
        {
            _animationRunning = false;
        }
    }
}