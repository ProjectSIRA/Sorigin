using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Processors;
using System.Collections.Generic;
using System.Globalization;

namespace Sorigin.Processors
{
    public class SimpleResizeWebProcessor : IImageWebProcessor
    {
        public const string Size = "size";

        private static readonly IEnumerable<string> ResizeCommands = new[] { Size };

        public IEnumerable<string> Commands => ResizeCommands;

        public FormattedImage Process(FormattedImage image, ILogger logger, IDictionary<string, string> commands, CommandParser parser, CultureInfo culture)
        {
            uint size = parser.ParseValue<uint>(commands.GetValueOrDefault(Size), culture);

            if (size >= image.Image.Width || size >= image.Image.Height)
            {
                return image;
            }

            if (size == 16 || size == 32 || size == 64 || size == 128 || size == 256 || size == 512 || size == 1024 || size == 2048 || size == 4096)
            {
                image.Image.Mutate(x =>
                {
                    x.Resize(new Size((int)size));
                });
                return image;
            }
            return image;
        }
    }
}