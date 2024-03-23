using System;
using System.Collections.Generic;
using System.Linq;
using TTTTile.Models;
using TTTTile.Models.Tiles;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using SecondaryTile = Windows.UI.StartScreen.SecondaryTile;
using WinTileSize = Windows.UI.StartScreen.TileSize;

namespace TTTTile.Controls
{
    public sealed partial class ImageTileView : UserControl
    {
        private readonly static int _tileGridSize = TileSize.TileUnitSize + TileSize.TileMargin;
        private readonly static int _panelWidth = 8;

        public static readonly DependencyProperty ImageScaleProperty =
            DependencyProperty.Register("ImageScale", typeof(double), typeof(ImageTileView), new PropertyMetadata(1.0));

        public static readonly DependencyProperty ImageXProperty =
            DependencyProperty.Register("ImageX", typeof(double), typeof(ImageTileView), new PropertyMetadata(0.0));

        public static readonly DependencyProperty ImageYProperty =
            DependencyProperty.Register("ImageY", typeof(double), typeof(ImageTileView), new PropertyMetadata(0.0));

        public double ImageScale
        {
            get { return (double)GetValue(ImageScaleProperty); }
            set
            { 
                SetValue(ImageScaleProperty, value); 
                foreach(ImageTile tile in _tiles.Keys)
                {
                    tile.ImageScaleTransform.ScaleX = value;
                    tile.ImageScaleTransform.ScaleY = value;
                }
            }
        }

        public double ImageX
        {
            get { return (double)GetValue(ImageXProperty); }
            set
            {
                SetValue(ImageXProperty, value);
                foreach (ImageTile tile in _tiles.Keys)
                    tile.ImageTranslateTransform.X = -Canvas.GetLeft(tile) + value;
            }
        }

        public double ImageY
        {
            get { return (double)GetValue(ImageYProperty); }
            set
            {
                SetValue(ImageYProperty, value);
                foreach (ImageTile tile in _tiles.Keys)
                    tile.ImageTranslateTransform.Y = -Canvas.GetTop(tile) + value;
            }
        }

        private readonly Dictionary<ImageTile, (int x, int y)> _tiles = new Dictionary<ImageTile, (int x, int y)>();

        public Guid Id { get; set; } = Guid.NewGuid();

        public IEnumerable<(TileSize size, int x, int y)> Tiles => _tiles.Select(kv => (kv.Key.Size, kv.Value.x, kv.Value.y));

        public ImageTileView()
        {
            double width = _panelWidth * _tileGridSize + TileSize.TileMargin;
            MinWidth = width;
            Width = width;
            MaxWidth = width;
            InitializeComponent();
        }

        private SoftwareBitmap _softwareBitmap = null;
        private readonly SoftwareBitmapSource _softwareBitmapSource = new SoftwareBitmapSource();

        public void SetImage(SoftwareBitmap softwareBitmap)
        {
            _softwareBitmap = softwareBitmap;
            _ = _softwareBitmapSource.SetBitmapAsync(SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied));
            foreach (ImageTile tile in _tiles.Keys)
                tile.ImageSource = _softwareBitmapSource;
        }

        public async void RequirePinAsync(double dpiScaling)
        {
            double scaling = dpiScaling;

            StorageFolder folder = (await ApplicationData.Current.LocalFolder.TryGetItemAsync("Tiles")) as StorageFolder;
            if (folder == null)
                folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Tiles", CreationCollisionOption.ReplaceExisting);

            int i = 0;
            foreach (ImageTile tile in _tiles.Keys)
            {
                string tileId = $"{Id}_{i:d2}";
                string tileFilename = $"{tileId}.png";

                StorageFile tileFile = await folder.CreateFileAsync(tileFilename, CreationCollisionOption.ReplaceExisting);

                using (IRandomAccessStream stream = await tileFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);

                    encoder.SetSoftwareBitmap(_softwareBitmap);
                    encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Fant;
                    encoder.BitmapTransform.Bounds = new BitmapBounds()
                    {
                        X = (uint)(-tile.ImageTranslateTransform.X * scaling),
                        Y = (uint)(-tile.ImageTranslateTransform.Y * scaling),
                        Width = (uint)(tile.Size.PixelWidth * scaling),
                        Height = (uint)(tile.Size.PixelHeight * scaling),
                    };
                    encoder.BitmapTransform.ScaledWidth = (uint)(_softwareBitmap.PixelWidth * ImageScale * scaling);
                    encoder.BitmapTransform.ScaledHeight = (uint)(_softwareBitmap.PixelHeight * ImageScale * scaling);

                    await encoder.FlushAsync().AsTask().ContinueWith(async _ =>
                    {
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                        {
                            WinTileSize ts = tile.Size.WindowsTileSize;
                            switch (ts)
                            {
                                case WinTileSize.Default:
                                case WinTileSize.Square150x150:
                                case WinTileSize.Wide310x150:
                                    break;

                                case WinTileSize.Square71x71:
                                    ts = WinTileSize.Square150x150;
                                    break;

                                default:
                                    ts = WinTileSize.Default;
                                    break;
                            };

                            SecondaryTile t = new SecondaryTile(
                                tileId: tileId,
                                displayName: "wdaf",
                                arguments: "wdnmd",
                                square150x150Logo: new Uri($"ms-appdata:///local/Tiles/{tileFilename}"),
                                desiredSize: ts);
                            t.VisualElements.Wide310x150Logo = t.VisualElements.Square150x150Logo;

                            await t.RequestCreateAsync();
                        });
                    });
                }
                i++;
            }
        }

        public (int x, int y) GetTilePosition(ImageTile tile)
        {
            return _tiles.TryGetValue(tile, out (int x, int y) pos) ? pos : (-1, -1); 
        }

        private void SetTilePosition(ImageTile tile, int x, int y)
        {
            _tiles[tile] = (x, y);

            double newLeft = x * _tileGridSize + TileSize.TileMargin;
            double newTop = y * _tileGridSize + TileSize.TileMargin;

            Canvas.SetLeft(tile, newLeft);
            Canvas.SetTop(tile, newTop);

            tile.ImageTranslateTransform.X = -newLeft + ImageX;
            tile.ImageTranslateTransform.Y = -newTop + ImageY;

            Height = _tiles.Max(kv => (kv.Key.Size.Height + kv.Value.y) * _tileGridSize + 4);
        }

        private ImageTile TileAt(int x, int y)
        {
            foreach (var kv in _tiles)
            {
                ImageTile tile = kv.Key;
                (int x, int y) pos = kv.Value;
                if (x >= pos.x && x < pos.x + tile.Size.Width
                 && y >= pos.y && y < pos.y + tile.Size.Height)
                    return tile;
            }
            return null;
        }

        public void AddTile(TileSize size)
        {
            AddTile(size, 0, _tiles.Count == 0 ? 0 : _tiles.Max(x => x.Value.y + x.Key.Size.Height));
        }

        public void AddTile(TileSize size, int x, int y)
        {
            ImageTile tile = new ImageTile(size);
            tile.ImageSource = _softwareBitmapSource;
            tile.ImageScaleTransform.ScaleX = ImageScale;
            tile.ImageScaleTransform.ScaleY = ImageScale;

            tile.PointerPressed += OnTilePointerPressed;
            tile.PointerReleased += OnTilePointerReleased;
            tile.PointerWheelChanged += OnTilePointerWheelChanged;

            tile.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
            tile.ManipulationDelta += OnTileManipulationDelta;
            tile.ManipulationCompleted += OnTileManipulationCompleted;

            _cancas.Children.Add(tile);
            MoveTileTo(tile, x, y, tile);
        }

        public void ClearTile()
        {
            _cancas.Children.Clear();
            _tiles.Clear();
            ImageX = 0;
            ImageY = 0;
            ImageScale = 1.0;
            SetImage(null);
        }

        private void MoveTileTo(ImageTile tile, int x, int y, ImageTile ignore)
        {
            for (int i = x; i - x < tile.Size.Width; i++)
            {
                for (int j = y; j - y < tile.Size.Height; j++)
                {
                    ImageTile t = TileAt(i, j);
                    if (t == null || t == tile || t == ignore)
                        continue;
                    MoveTileTo(t, GetTilePosition(t).x, y + tile.Size.Height, ignore);
                }
            }
            SetTilePosition(tile, x, y);
        }

        private bool RemoveTile(ImageTile tile)
        {
            if (!_tiles.ContainsKey(tile))
                return false;
            _tiles.Remove(tile);
            _cancas.Children.Remove(tile);
            return true;
        }

        private bool _moveTile = false;
        private bool _moveImage = false;

        private void OnTilePointerPressed(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint pointerPoint = e.GetCurrentPoint(null);
            if (pointerPoint.Properties.IsLeftButtonPressed)
                _moveTile = true;
            else if (pointerPoint.Properties.IsRightButtonPressed)
                _moveImage = true;
            else if (pointerPoint.Properties.IsMiddleButtonPressed && sender is ImageTile tile)
                RemoveTile(tile);
            e.Handled = true;
        }

        private void OnTilePointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _moveTile = false;
            _moveImage = false;
            e.Handled = true;
        }

        private void OnTilePointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (sender is ImageTile)
                ImageScale *= 1 + (0.0005 * e.GetCurrentPoint(null).Properties.MouseWheelDelta);
            e.Handled = true;
        }

        private void OnTileManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (!(sender is ImageTile tile))
            {
                e.Handled = true;
                return;
            }

            double dx = e.Delta.Translation.X;
            double dy = e.Delta.Translation.Y;
            if (_moveTile)
            {
                Canvas.SetLeft(tile, Canvas.GetLeft(tile) + dx);
                Canvas.SetTop(tile, Canvas.GetTop(tile) + dy);
                tile.ImageTranslateTransform.X -= dx;
                tile.ImageTranslateTransform.Y -= dy;
            }
            else if (_moveImage)
            {
                ImageX += dx;
                ImageY += dy;
            }

            e.Handled = true;
        }

        private void OnTileManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (_moveTile && sender is ImageTile tile)
            {
                int x = Math.Clamp((int)((Canvas.GetLeft(tile) + (_tileGridSize / 2)) / _tileGridSize), 0, _panelWidth - 1);
                if (x + tile.Size.Width > _panelWidth)
                    x -= x + tile.Size.Width - _panelWidth;
                int y = Math.Clamp((int)((Canvas.GetTop(tile) + (_tileGridSize / 2)) / _tileGridSize), 0, int.MaxValue);
                MoveTileTo(tile, x, y, tile);
                _moveTile = false;
            }
            else if (_moveImage)
            {
                _moveImage = false;
            }

            e.Handled = true;
        }
    }
}
