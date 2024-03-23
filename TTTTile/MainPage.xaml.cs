using System;
using System.Threading.Tasks;
using TTTTile.Models.Tiles;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace TTTTile
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            _sliderDpiScaling.Value = Windows.Graphics.Display.DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel * 100;

            _buttonSetBackground.Click += (s, e) => SetBackgroundImageAsync();
            _buttonSetTileImage.Click  += (s, e) => SetTileImageAsync();

            _buttonAddSmallTile.Click  += (s, e) => _imageTileView.AddTile(TileSize.Small);
            _buttonAddMediumTile.Click += (s, e) => _imageTileView.AddTile(TileSize.Medium);
            _buttonAddWideTile.Click   += (s, e) => _imageTileView.AddTile(TileSize.Wide);

            _buttonPin.Click += (s, e) => _imageTileView.RequirePinAsync((double)_sliderDpiScaling.Value / 100);

            _buttonHelp.Click += (s, e)    => _ = _dialogHelp.ShowAsync();
            _buttonSetting.Click += (s, e) => _ = _dialogSetting.ShowAsync();
        }

        private async void SetTileImageAsync()
        {
            StorageFile file = await SelectImageAsync();
            if (file == null)
                return;
            BitmapDecoder bitmapDecoder = await BitmapDecoder.CreateAsync(await file.OpenAsync(FileAccessMode.Read));
            SoftwareBitmap softwareBitmap = await bitmapDecoder.GetSoftwareBitmapAsync();
            _imageTileView.Id = Guid.NewGuid();
            _imageTileView.SetImage(softwareBitmap);
        }

        private async void SetBackgroundImageAsync()
        {
            StorageFile file = await SelectImageAsync();
            if (file == null)
                return;
            BitmapDecoder bitmapDecoder = await BitmapDecoder.CreateAsync(await file.OpenAsync(FileAccessMode.Read));
            SoftwareBitmapSource bitmapSource = new SoftwareBitmapSource();
            await bitmapSource.SetBitmapAsync(SoftwareBitmap.Convert(await bitmapDecoder.GetSoftwareBitmapAsync(), BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied));
            _backgroundImage.Source = bitmapSource;
        }

        private async Task<StorageFile> SelectImageAsync()
        {
            FileOpenPicker picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            return await picker.PickSingleFileAsync();
        }
    }
}
