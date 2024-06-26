﻿using System;
using System.Threading.Tasks;
using TTTTile.Tiles;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace TTTTile
{
    public sealed partial class MainPage : Page
    {
        private string _currentTileImageFilename = string.Empty;

        public MainPage()
        {
            InitializeComponent();

            SizeChanged += (s, e) =>
            {
                Thickness oldMargin = _scrollViewer.Margin;
                double leftMargin = _buttonTilePanel.ActualWidth + 8;
                double dd = _scrollViewer.ActualWidth;
                if (e.NewSize.Width < (212 + 420 + 8))
                {
                    if (e.NewSize.Width < (420 + 8))
                        leftMargin = 4;
                    else
                        leftMargin = e.NewSize.Width - 420;
                }
                _scrollViewer.Margin = new Thickness(leftMargin, oldMargin.Top, oldMargin.Right, oldMargin.Bottom);
            };

            _sliderDpiScaling.Value = ImageTileManager.DpiScaling * 100;
            _sliderDpiScaling.ValueChanged += (s, e) => ImageTileManager.DpiScaling = e.NewValue / 100;

            _buttonSetBackground.Click += (s, e) => SetBackgroundImageAsync();
            _buttonSetTileImage.Click  += (s, e) => SetTileImageAsync();

            _buttonAddSmallTile.Click  += (s, e) => _imageTileView.AddTile(TileSize.Small);
            _buttonAddMediumTile.Click += (s, e) => _imageTileView.AddTile(TileSize.Medium);
            _buttonAddWideTile.Click   += (s, e) => _imageTileView.AddTile(TileSize.Wide);

            _buttonClear.Click += (s, e) => _imageTileView.ClearTiles();
            _buttonPin.Click   += (s, e) => _ = _imageTileView.RequirePinAsync(_currentTileImageFilename);

            _buttonHelp.Click += (s, e)    => _ = _dialogHelp.ShowAsync();
            _buttonSetting.Click += (s, e) => _ = _dialogSetting.ShowAsync();

            _imageTileView.AddTile(TileSize.Wide, 0, 0);
            _imageTileView.AddTile(TileSize.Wide, 4, 0);
            _imageTileView.AddTile(TileSize.Wide, 0, 2);
            _imageTileView.AddTile(TileSize.Wide, 4, 2);
            _imageTileView.AddTile(TileSize.Wide, 0, 4);
            _imageTileView.AddTile(TileSize.Medium, 4, 4);
            _imageTileView.AddTile(TileSize.Medium, 6, 4);
        }

        private async void SetTileImageAsync()
        {
            StorageFile file = await SelectImageAsync();
            if (file == null)
                return;
            _currentTileImageFilename = file.Name;
            BitmapDecoder bitmapDecoder = await BitmapDecoder.CreateAsync(await file.OpenAsync(FileAccessMode.Read));
            _imageTileView.Image = await bitmapDecoder.GetSoftwareBitmapAsync();
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
