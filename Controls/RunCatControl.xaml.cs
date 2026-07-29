using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace AgyUsageShower.Controls
{
    public partial class RunCatControl : System.Windows.Controls.UserControl
    {
        private readonly DispatcherTimer _animTimer;
        private int _currentFrame = 0;
        private static readonly Geometry[] CatFrames;

        static RunCatControl()
        {
            CatFrames = new Geometry[5]
            {
                Geometry.Parse("M 2,12 C 5,6 10,7 15,5 C 17,4 18,6 19,9 L 19,13 L 17,14 L 14,11 L 10,12 L 7,15 Z M 16,5 A 1.5,1.5 0 1 0 16,2 A 1.5,1.5 0 1 0 16,5 Z"),
                Geometry.Parse("M 3,13 C 6,5 11,6 16,4 C 18,3 19,5 20,8 L 20,12 L 18,14 L 15,10 L 11,11 L 8,14 Z M 17,4 A 1.5,1.5 0 1 0 17,1 A 1.5,1.5 0 1 0 17,4 Z"),
                Geometry.Parse("M 4,14 C 7,7 12,7 16,6 C 17,5 18,7 18,10 L 18,14 L 16,14 L 13,11 L 10,12 L 7,14 Z M 15,6 A 1.5,1.5 0 1 0 15,3 A 1.5,1.5 0 1 0 15,6 Z"),
                Geometry.Parse("M 3,11 C 6,5 11,6 15,4 C 17,3 18,5 19,8 L 19,12 L 16,13 L 13,10 L 9,11 L 6,14 Z M 16,4 A 1.5,1.5 0 1 0 16,1 A 1.5,1.5 0 1 0 16,4 Z"),
                Geometry.Parse("M 2,10 C 5,4 10,5 15,3 C 17,2 18,4 19,7 L 19,11 L 17,12 L 14,9 L 10,10 L 7,13 Z M 16,3 A 1.5,1.5 0 1 0 16,0 A 1.5,1.5 0 1 0 16,3 Z")
            };
        }

        public static readonly DependencyProperty RemainingQuotaPercentProperty =
            DependencyProperty.Register(
                nameof(RemainingQuotaPercent),
                typeof(double),
                typeof(RunCatControl),
                new PropertyMetadata(100.0, OnRemainingQuotaChanged));

        public double RemainingQuotaPercent
        {
            get => (double)GetValue(RemainingQuotaPercentProperty);
            set => SetValue(RemainingQuotaPercentProperty, value);
        }

        public RunCatControl()
        {
            InitializeComponent();
            _animTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(150)
            };
            _animTimer.Tick += AnimTimer_Tick;
            _animTimer.Start();
            UpdateFrame();
        }

        private static void OnRemainingQuotaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RunCatControl control)
            {
                control.UpdateSpeed();
            }
        }

        private void UpdateSpeed()
        {
            double consumption = Math.Max(0.0, Math.Min(100.0, 100.0 - RemainingQuotaPercent));
            int intervalMs = (int)Math.Max(35.0, 240.0 - (consumption * 2.05));
            _animTimer.Interval = TimeSpan.FromMilliseconds(intervalMs);
        }

        private void AnimTimer_Tick(object? sender, EventArgs e)
        {
            _currentFrame = (_currentFrame + 1) % CatFrames.Length;
            UpdateFrame();
        }

        private void UpdateFrame()
        {
            CatPath.Data = CatFrames[_currentFrame];
        }
    }
}
