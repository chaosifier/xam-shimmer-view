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
        public bool _animationRunning;
        public float WaveSize { get; set; } = .15f;
        public float Speed { get; set; } = 1f;
        public SKColor WaveColor { get; set; } = SKColors.Gray;
        public SKColor RestColor { get; set; } = SKColors.LightGray;

        private float _wavePosition = 0f;

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


        private SKCanvasView _canvas;
        private AbsoluteLayout _absoluteLayout;
        private ContentView _gridContainer;

        public ShimmerLayout()
        {
            _gridContainer = new ContentView() { HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };
            _absoluteLayout = new AbsoluteLayout() { HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };
            Content = _absoluteLayout;

            _absoluteLayout.Children.Add(_gridContainer, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All);

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

            double angle = 45;
            float startPosX = 0f;
            float startPosY = 0f;
            float endPosX = info.Width;
            float endPosY = (float)Math.Tan(angle) * endPosX;

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
                                    new SKColor[] { RestColor, WaveColor, RestColor },
                                    new float[] { leftPosition, _wavePosition, rightPosition },
                                    SKShaderTileMode.Clamp);


                var mainPath = new SKPath();
                mainPath.AddRect(rect);

                var childPath = new SKPath();
                foreach (var childView in ShimmerGrid.Children)
                {
                    if (childView is ShimmerBoxView theView)
                    {
                        AddViewPath(ref childPath, theView);
                    }
                    else
                    {

                    }
                }

                canvas.ClipPath(childPath);

                // canvas.ClipPath(keyholePath);

                // Set transform to center and enlarge clip path to window height
                //SKRect bounds;
                //keyholePath.GetTightBounds(out bounds);

                canvas.DrawPath(mainPath, paint);
            }
        }

        private void AddViewPath(ref SKPath pathToAddTo, ShimmerBoxView theView)
        {
            pathToAddTo.AddRoundRect(theView.Bounds.ToSKRect(), (float)theView.CornerRadius.TopLeft, (float)theView.CornerRadius.TopRight, SKPathDirection.CounterClockwise);
        }

        public void StartAnimation()
        {
            _animationRunning = true;
            Device.StartTimer(TimeSpan.FromMilliseconds(20), () =>
            {
                if (_wavePosition > 1.2f)
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

        SKPath keyholePath = SKPath.ParseSvgPathData(
            "M 300 130 L 250 350 L 450 350 L 400 130 A 70 70 0 1 0 300 130 Z");

    }
}