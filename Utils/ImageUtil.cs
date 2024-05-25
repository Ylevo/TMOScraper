using ImageMagick;
using Serilog;
using TMOScrapper.Properties;

namespace TMOScrapper.Utils
{
    public static class ImageUtil
    {
        public static async Task ConvertImages(string folder, CancellationToken token)
        {
            try
            {
                Log.Information("Starting images conversion.");
                var imagesPaths = Directory.GetFiles(folder);

                foreach (string originalImage in imagesPaths)
                {
                    token.ThrowIfCancellationRequested();
                    using MagickImageCollection imgCollection = new(originalImage);

                    if (imgCollection.Count > 1)
                    {
                        await imgCollection.WriteAsync(Path.ChangeExtension(originalImage, "gif"), MagickFormat.Gif, token);
                    }
                    else
                    {
                        using MagickImage newImg = (MagickImage)imgCollection[0];

                        switch (Settings.Default.ConvertFormat)
                        {
                            case "JPEG":
                                UseJPEGSettings(newImg);
                                break;
                            case "PNG":
                                UsePNGSettings(newImg);
                                break;
                            case "PNG 4 bpp":
                                UsePNG4bppSettings(newImg);
                                break;
                        }

                        string format = Settings.Default.ConvertFormat == "JPEG" ? "jpeg" : "png";
                        Log.Information($"Converting {Path.GetFileName(originalImage)} to {format}.");
                        await imgCollection.WriteAsync(Path.ChangeExtension(originalImage, format), token);
                        File.Delete(originalImage);
                    }
                }

                Log.Information("Finished images conversion.");
            }
            catch(Exception ex) when (ex is not OperationCanceledException)
            {
                Log.Error($"Unexpected error while converting images.");
                throw;
            }
        }

        public static async Task SplitImages(string folder, CancellationToken token)
        {
            try 
            {
                Log.Information("Starting images cropping.");
                var files = Directory.GetFiles(folder);

                foreach (string file in files)
                {
                    token.ThrowIfCancellationRequested();

                    using MagickImage originalImg = new(file);
                    int originalImgHeight = originalImg.Height;

                    if (originalImgHeight >= 10000)
                    {
                        int numberOfSlices = (int)Math.Ceiling((decimal)originalImgHeight / 5000);
                        int sizeOfSlice = (int)Math.Ceiling((decimal)originalImgHeight / numberOfSlices);

                        Log.Information($"Splitting {Path.GetFileName(file)} into {numberOfSlices} parts.");

                        for (int i = 0; i < numberOfSlices; i++)
                        {
                            using MagickImage newSlice = new(originalImg);
                            MagickGeometry size = new(0, sizeOfSlice * i, newSlice.Width, sizeOfSlice);
                            newSlice.Crop(size);
                            newSlice.RePage();
                            await newSlice.WriteAsync(AppendToFileName(file, "-" + $"{i + 1}".PadLeft(3, '0')), token);
                        }

                        File.Delete(file);
                    }
                }

                Log.Information("Finished images cropping.");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Log.Error($"Unexpected error while cropping images.");
                throw;
            }
        }

        private static void UseJPEGSettings(MagickImage img)
        {
            img.Format = MagickFormat.Jpeg;
            img.Quality = 90;
            img.ColorSpace = ColorSpace.sRGB;
            img.Settings.Interlace = Interlace.Jpeg;
            img.Settings.SetDefine(MagickFormat.Jpeg, "sampling-factor", "4:2:0");
            img.Settings.SetDefine(MagickFormat.Jpeg, "dct-method", "float");
            img.Strip();
        }

        private static void UsePNGSettings(MagickImage img)
        {
            img.Format = MagickFormat.Png;
            img.Quality = 100;
            img.ColorSpace = ColorSpace.sRGB;
            img.Strip();
        }

        private static void UsePNG4bppSettings(MagickImage img)
        {
            img.Format = MagickFormat.Png;
            img.Quality = 95;
            img.SetBitDepth(4, Channels.Gray);
            img.ColorSpace = ColorSpace.Gray;
            img.Posterize(16, DitherMethod.No, Channels.Gray);
            img.Normalize();
            img.Strip();
        }

        private static string AppendToFileName(string source, string appendValue)
        {
            return $"{Path.Combine(Path.GetDirectoryName(source), Path.GetFileNameWithoutExtension(source))}{appendValue}{Path.GetExtension(source)}";
        }
    }
}
