using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Islidesshow;
using Image = System.Windows.Controls.Image;

namespace WpfApp1;

public class OpacityEffect : ISlideshowEffect
{
    public string Name => "Opacity Effect";

    public async Task PlaySlideshow(Image imageIn, Image imageOut)
    {
        var duration = TimeSpan.FromSeconds(5);

        imageOut.Opacity = 0.0;

        var animationOut = new DoubleAnimation(0.0, 1.0, duration);

        imageOut.BeginAnimation(UIElement.OpacityProperty, animationOut);

        await Task.Delay(duration);

        imageOut.Opacity = 1.0;
    }
}