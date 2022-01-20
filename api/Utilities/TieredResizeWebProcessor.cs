using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Processors;
using System.Globalization;

namespace Sorigin.Utilities;

public class TieredResizeWebProcessor : IImageWebProcessor
{
    public const string Size = "size";
    private static readonly IEnumerable<string> _resizeCommands = new[] { Size };

    public IEnumerable<string> Commands => _resizeCommands;

    public FormattedImage Process(FormattedImage image, ILogger logger, IDictionary<string, string> commands, CommandParser parser, CultureInfo culture)
    {
        string size = commands.GetValueOrDefault(Size);

        if (size is null)
            return image;

        size = size.ToLower();

        if (size == "small")
            SetSize(image, 128);
        else if (size == "medium")
            SetSize(image, 512);
        else if (size == "large")
            SetSize(image, 1024);

        return image;
    }

    private static void SetSize(FormattedImage image, int size)
    {
        if (image.Image.Width > size || image.Image.Height > size)
        {
            image.Image.Mutate(i => i.Resize(new Size(size)));
        }
    }
}