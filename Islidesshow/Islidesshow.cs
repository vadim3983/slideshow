using System.Threading.Tasks;
using System.Windows.Controls;
using Image = System.Windows.Controls.Image;

namespace Islidesshow;

public interface ISlideshowEffect
{
    string Name { get; }
    Task PlaySlideshow(Image imageIn, Image imageOut);
}