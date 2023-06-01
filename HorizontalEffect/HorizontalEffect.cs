using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Threading.Tasks;
using System.Windows.Controls;
using Islidesshow;
using Image = System.Windows.Controls.Image;

namespace WpfApp1;


public class HorizontalEffect  : ISlideshowEffect
{
    public string Name => "Horizontal Effect";

    public Task PlaySlideshow(Image imageIn, Image imageOut)
    {
        var duration = TimeSpan.FromSeconds(5);

        var transformIn = new TranslateTransform();
        imageIn.RenderTransform = transformIn;

        var transformOut = new TranslateTransform();
        imageOut.RenderTransform = transformOut;

        double windowWidth = 1024;

        var animationIn = new DoubleAnimation(0, -windowWidth, duration);
        var animationOut = new DoubleAnimation(windowWidth, 0, duration);

        var tcs = new TaskCompletionSource<bool>();

        EventHandler handler = null;
        handler = (s, e) =>
        {
            animationIn.Completed -= handler;
            animationOut.Completed -= handler;
            tcs.SetResult(true);
        };

        animationIn.Completed += handler;
        animationOut.Completed += handler;

        transformIn.BeginAnimation(TranslateTransform.XProperty, animationIn);
        transformOut.BeginAnimation(TranslateTransform.XProperty, animationOut);

        return tcs.Task;
    }
}