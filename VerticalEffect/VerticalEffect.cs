using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Islidesshow;
using Image = System.Windows.Controls.Image;

namespace WpfApp1;

public class VerticalEffect : ISlideshowEffect
{
    public string Name => "Vertical Effect";

    public Task PlaySlideshow(Image imageIn, Image imageOut)
    {
        var duration = TimeSpan.FromSeconds(5);

        var transformIn = new TranslateTransform();
        imageIn.RenderTransform = transformIn;

        var transformOut = new TranslateTransform();
        imageOut.RenderTransform = transformOut;

        double windowHeight = 768;

        var animationIn = new DoubleAnimation(0, -windowHeight, duration);
        var animationOut = new DoubleAnimation(windowHeight, 0, duration);

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

        transformIn.BeginAnimation(TranslateTransform.YProperty, animationIn);
        transformOut.BeginAnimation(TranslateTransform.YProperty, animationOut);

        return tcs.Task;
    }
}