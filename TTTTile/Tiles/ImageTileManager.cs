using System;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.StartScreen;

namespace TTTTile.Tiles
{
    public static class ImageTileManager
    {
        public static double DpiScaling { get; set; } = 1.0;

        public static async void CleanAsync()
        {
            if (await ApplicationData.Current.LocalFolder.TryGetItemAsync("Tiles") is StorageFolder folder)
            {
                foreach (StorageFile file in await folder.GetFilesAsync())
                {
                    if (!SecondaryTile.Exists(file.DisplayName))
                        await file.DeleteAsync();
                }
            }
        }

        public static async void PinAsync(SoftwareBitmap image, double x, double y, double scale, TileSize size, string displayName = "")
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFolder tilesFolder = (await localFolder.TryGetItemAsync("Tiles")) as StorageFolder;
            if (tilesFolder == null)
                tilesFolder = await localFolder.CreateFolderAsync("Tiles", CreationCollisionOption.ReplaceExisting);

            Guid guid;
            string tileId;
            string tileFilename;
            do
            {
                guid = Guid.NewGuid();
                tileId = guid.ToString();
                tileFilename = $"{guid}.png";
            } while (await tilesFolder.TryGetItemAsync(tileFilename) != null);

            StorageFile tileFile = await tilesFolder.CreateFileAsync(tileFilename, CreationCollisionOption.ReplaceExisting);

            TileSizeInfo sizeInfo = TileSizeInfo.GetInfo(size);
            
            using (IRandomAccessStream stream = await tileFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);

                encoder.SetSoftwareBitmap(image);
                encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Fant;
                encoder.BitmapTransform.Bounds = new BitmapBounds()
                {
                    X = (uint)(x * DpiScaling),
                    Y = (uint)(y * DpiScaling),
                    Width = (uint)(sizeInfo.PixelWidth * DpiScaling),
                    Height = (uint)(sizeInfo.PixelHeight * DpiScaling),
                };
                encoder.BitmapTransform.ScaledWidth = (uint)(image.PixelWidth * scale * DpiScaling);
                encoder.BitmapTransform.ScaledHeight = (uint)(image.PixelHeight * scale * DpiScaling);

                await encoder.FlushAsync().AsTask().ContinueWith(async _ =>
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(
                        Windows.UI.Core.CoreDispatcherPriority.Normal,
                        async () =>
                        {
                            var t = new SecondaryTile(
                                tileId: tileId,
                                displayName: displayName,
                                arguments: "wdnmd",
                                square150x150Logo: new Uri($"ms-appdata:///local/Tiles/{tileFilename}"),
                                desiredSize: (size == TileSize.Small ? TileSize.Medium : size).AsWindowsTileSize());

                            t.VisualElements.Wide310x150Logo = t.VisualElements.Square150x150Logo;

                            await t.RequestCreateAsync();
                        });
                });
            }
        }

    }
}
