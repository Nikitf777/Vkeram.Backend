using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using Vkeram.Backend.Models;

namespace Vkeram.Backend.Services;

public interface IImagePreviewService
{
    ProductImagePreview GeneratePreview(ProductImage image);
}

public class ImagePreviewService : IImagePreviewService
{
    public ProductImagePreview GeneratePreview(ProductImage image)
    {
        using var msIn = new MemoryStream(image.ImageData);
        using var img = Image.Load(msIn);

        var size = Math.Min(img.Width, img.Height);
        img.Mutate(x =>
        {
            var crop = new Rectangle((img.Width - size) / 2, (img.Height - size) / 2, size, size);
            x.Crop(crop);
            x.Resize(512, 512);
        });

        using var msOut = new MemoryStream();
        img.Save(msOut, new JpegEncoder { Quality = 80 });

        return new ProductImagePreview
        {
            ProductId = image.ProductId,
            ImageId = image.Id,
            ImageData = msOut.ToArray(),
            ContentType = "image/jpeg"
        };
    }
}
