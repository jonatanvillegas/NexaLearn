using NexaLearn.Models;
using NexaLearn.Exceptions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;

namespace NexaLearn.Services;

public class ImageSharp : IImageSharp
{
    private const long MaxBytes = 10 * 1024 * 1024;
    private const long MaxPixels = 20_000_000;

    public async Task<CursoImagen> ProcesarAsync(IFormFile imagen, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(imagen);
        cancellationToken.ThrowIfCancellationRequested();

        if (imagen.Length <= 0 || imagen.Length > MaxBytes)
        {
            throw new ImagenInvalidaException("La imagen debe tener contenido y no superar los 10 MB.");
        }

        var extension = Path.GetExtension(imagen.FileName).ToLowerInvariant();
        //Formatos aceptados para el guardado 
        var expectedFormat = extension switch
        {
            ".jpg" or ".jpeg" => "JPEG",
            ".png" => "PNG",
            _ => throw new ImagenInvalidaException("Solo se permiten imágenes JPG, JPEG o PNG.")
        };
        var expectedMime = expectedFormat == "JPEG" ? "image/jpeg" : "image/png";
        if (!string.Equals(imagen.ContentType, expectedMime, StringComparison.OrdinalIgnoreCase))
        {
            throw new ImagenInvalidaException("El tipo MIME no coincide con la extensión de la imagen.");
        }

        try
        {
            await using var input = imagen.OpenReadStream();
            var options = new DecoderOptions { MaxFrames = 1 };
            var info = await Image.IdentifyAsync(options, input, cancellationToken);
            if (!string.Equals(info.Metadata.DecodedImageFormat?.Name, expectedFormat, StringComparison.OrdinalIgnoreCase))
            {
                throw new ImagenInvalidaException("El formato real del archivo no coincide con su extensión.");
            }

            if ((long)info.Width * info.Height > MaxPixels)
            {
                throw new ImagenInvalidaException("La imagen no debe superar los 20 megapíxeles.");
            }

            input.Position = 0;
            using var image = await Image.LoadAsync(options, input, cancellationToken);
            image.Mutate(context => context.AutoOrient());
            if (image.Width > 1600 || image.Height > 1600)
            {
                image.Mutate(context => context.Resize(new ResizeOptions
                {
                    Size = new Size(1600, 1600),
                    Mode = ResizeMode.Max
                }));
            }

            image.Mutate(context => context.BackgroundColor(Color.White));
            await using var output = new MemoryStream();
            await image.SaveAsync(output, new JpegEncoder { Quality = 75, SkipMetadata = true }, cancellationToken);

            return new CursoImagen
            {
                NombreArchivo = $"{Guid.NewGuid():N}.jpg",
                MimeType = "image/jpeg",
                Contenido = output.ToArray()
            };
        }
        catch (UnknownImageFormatException exception)
        {
            throw new ImagenInvalidaException("El archivo no contiene una imagen JPG o PNG válida.", exception);
        }
        catch (InvalidImageContentException exception)
        {
            throw new ImagenInvalidaException("La imagen está dañada o su contenido no es válido.", exception);
        }
    }
}
