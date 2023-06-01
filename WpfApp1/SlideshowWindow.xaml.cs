using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Islidesshow;

namespace WpfApp1;

public partial class SlideshowWindow : Window
{
    private static readonly Action EmptyDelegate = delegate { };
    private CancellationTokenSource _cts;
    private bool _isPaused;

    public SlideshowWindow()
    {
        InitializeComponent();


        MouseLeftButtonDown += (sender, e) =>
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        };
    }

    public async Task PlaySlideshow(ISlideshowEffect effect, List<string> imagePaths)
    {
        _cts?.Dispose();
        _isPaused = false;
        _cts = new CancellationTokenSource();
        slideshowImage.Source = new BitmapImage(new Uri(imagePaths[0]));

        var i = 0;
        while (_cts != null && !_cts.IsCancellationRequested)
        {
            var imagePath = imagePaths[i];
            ImageSource imageIn = new BitmapImage(new Uri(imagePath));

            var nextImagePath = imagePaths[(i + 1) % imagePaths.Count];
            ImageSource imageOut = new BitmapImage(new Uri(nextImagePath));

            i = (i + 1) % imagePaths.Count;

            await Dispatcher.Invoke(async () =>
            {
                slideshowImage.Source = imageIn;

                var nextImageControl = new Image
                {
                    Source = imageOut,
                    Stretch = Stretch.Fill,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top
                };

                nextImageControl.MouseRightButtonDown += Image_MouseRightButtonDown;

                var grid = slideshowImage.Parent as Grid;
                Debug.Assert(grid != null, nameof(grid) + " != null");
                grid.Children.Add(nextImageControl);
                Panel.SetZIndex(slideshowImage, 0);
                Panel.SetZIndex(nextImageControl, 1);

                await effect.PlaySlideshow(slideshowImage, nextImageControl);

                grid.Children.Remove(slideshowImage);
                slideshowImage.MouseRightButtonDown -= Image_MouseRightButtonDown;

                slideshowImage = nextImageControl;
            });

            while (_isPaused && _cts != null && !_cts.IsCancellationRequested) await Task.Delay(1000);
        }
    }

    private void Image_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        var cm = FindResource("MyContextMenu") as ContextMenu;
        Debug.Assert(cm != null, nameof(cm) + " != null");
        cm.PlacementTarget = sender as Image;
        cm.IsOpen = true;
    }


    private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
    {
        _isPaused = !_isPaused;
        (sender as MenuItem)!.Header = _isPaused ? "Play" : "Pause";
    }

    private void StopButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_cts is { IsCancellationRequested: false })
            {
                _cts.Cancel();
                _cts.Dispose();
            }

            _cts = null;
            Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}