using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TTTTile.Tiles;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

namespace TTTTile.Controls
{
    public sealed partial class ImageTileView : UserControl
    {
        private readonly static int _tileGridCellSize = (TileSizeInfo.SmallTilePixelSize + TileSizeInfo.TileMargin);
        private readonly static int _tileGridColumnCount = 8;

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
                DraggingPreviewer.ImageScaleTransform.ScaleX = value;
                DraggingPreviewer.ImageScaleTransform.ScaleY = value;
                foreach (ImageTile tile in _tiles)
                {
                    tile.ImageScaleTransform.ScaleX = value;
                    tile.ImageScaleTransform.ScaleY = value;
                }
                SetValue(ImageScaleProperty, value);
            }
        }

        public double ImageX
        {
            get { return (double)GetValue(ImageXProperty); }
            set
            {
                foreach (ImageTile tile in _tiles)
                    tile.ImageTranslateTransform.X = -TilePanel.GetX(tile) * _tileGridCellSize + value;
                SetValue(ImageXProperty, value);
            }
        }

        public double ImageY
        {
            get { return (double)GetValue(ImageYProperty); }
            set
            {
                foreach (ImageTile tile in _tiles)
                    tile.ImageTranslateTransform.Y = -TilePanel.GetY(tile) * _tileGridCellSize + value;
                SetValue(ImageYProperty, value);
            }
        }

        private SoftwareBitmap _image;
        private SoftwareBitmapSource _imageSource = new SoftwareBitmapSource();

        public SoftwareBitmap Image
        {
            get { return _image; }
            set
            {
                if (value == null)
                    _ = _imageSource.SetBitmapAsync(null);
                else
                    _ = _imageSource.SetBitmapAsync(SoftwareBitmap.Convert(value, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied));

                foreach (ImageTile tile in _tiles)
                    tile.ImageSource = _imageSource;

                _image = value;

                AdjustImageToFill();
            }
        }

        private ImageTileDraggingPreviewer _dragingPreviewer = null;

        public ImageTileDraggingPreviewer DraggingPreviewer
        {
            get { return _dragingPreviewer ?? _defaultPreviewer; }
            set 
            {
                if (value == null)
                {
                    _defaultPreviewer.Visibility = Visibility.Visible;
                }
                else
                {
                    value.ImageSource = _imageSource;
                    _defaultPreviewer.Visibility = Visibility.Collapsed;
                }
                _dragingPreviewer = value; 
            }
        }

        private readonly List<ImageTile> _tiles = new List<ImageTile>();

        public ImageTileView()
        {
            InitializeComponent();

            _tilePanel.Width = _tileGridColumnCount * _tileGridCellSize + TileSizeInfo.TileMargin;
            DraggingPreviewer.ImageSource = _imageSource;
        }

        public async Task RequirePinAsync(string displayName)
        {
            if (Image == null || _tiles.Count == 0)
                return;

            foreach (ImageTile tile in _tiles)
            {
                await ImageTileManager.PinAsync(Image,
                    -tile.ImageTranslateTransform.X,
                    -tile.ImageTranslateTransform.Y,
                    ImageScale,
                    TilePanel.GetSize(tile),
                    displayName);
            }
        }

        public void AddTile(TileSize size)
        {
            AddTile(size, 0, _tiles.Count == 0
                ? 0
                : _tiles.Max(x => TilePanel.GetY(x) + TileSizeInfo.GetInfo(TilePanel.GetSize(x)).Height));
        }

        public void AddTile(TileSize size, int x, int y)
        {
            ImageTile tile = new ImageTile();
            tile.ImageSource = _imageSource;
            tile.ImageScaleTransform.ScaleX = ImageScale;
            tile.ImageScaleTransform.ScaleY = ImageScale;

            tile.PointerPressed += OnTilePointerPressed;
            tile.PointerReleased += OnTilePointerReleased;
            tile.PointerWheelChanged += OnTilePointerWheelChanged;

            tile.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
            tile.ManipulationStarted += OnTileManipulationStarted;
            tile.ManipulationDelta += OnTileManipulationDelta;
            tile.ManipulationCompleted += OnTileManipulationCompleted;

            TilePanel.SetSize(tile, size);
            _tiles.Add(tile);
            MoveTileTo(tile, x, y);
            _tilePanel.Children.Add(tile);
        }

        public void ClearTiles()
        {
            _tiles.Clear();
            _tilePanel.Children.Clear();
        }

        private void SetTilePosition(ImageTile tile, int x, int y)
        {
            TilePanel.SetX(tile, x);
            TilePanel.SetY(tile, y);
            tile.ImageTranslateTransform.X = -x * _tileGridCellSize + ImageX;
            tile.ImageTranslateTransform.Y = -y * _tileGridCellSize + ImageY;
        }

        private ImageTile TileAt(int x, int y)
        {
            foreach (ImageTile tile in _tiles)
            {
                int tilePositionX = TilePanel.GetX(tile);
                int tilePositionY = TilePanel.GetY(tile);
                TileSizeInfo sizeInfo = TileSizeInfo.GetInfo(TilePanel.GetSize(tile));
                if (x >= tilePositionX && x < tilePositionX + sizeInfo.Width
                 && y >= tilePositionY && y < tilePositionY + sizeInfo.Height)
                    return tile;
            }
            return null;
        }

        private void MoveTileTo(ImageTile tile, int x, int y)
        {
            _tiles.Remove(tile);

            TileSizeInfo sizeInfo = TileSizeInfo.GetInfo(TilePanel.GetSize(tile));
            for (int i = x; i - x < sizeInfo.Width; i++)
            {
                for (int j = y; j - y < sizeInfo.Height; j++)
                {
                    ImageTile t = TileAt(i, j);
                    if (t != null)
                    {
                        MoveTileTo(t, TilePanel.GetX(t), y + sizeInfo.Height);
                    }
                }
            }

            _tiles.Add(tile);
            SetTilePosition(tile, x, y);
        }

        private bool RemoveTile(ImageTile tile)
        {
            if (_tiles.Remove(tile))
            {
                _tilePanel.Children.Remove(tile);
                return true;
            }
            return false;
        }

        private (int x, int y, int width, int height) GetTilePixelBounds()
        {
            if (Image == null || _tiles.Count == 0)
                return (0, 0, 0, 0);

            int left   = _tiles.Min(x => TilePanel.GetX(x));
            int top    = _tiles.Min(x => TilePanel.GetY(x));
            int right  = _tiles.Max(x => TilePanel.GetX(x) + TileSizeInfo.GetInfo(TilePanel.GetSize(x)).Width);
            int bottom = _tiles.Max(x => TilePanel.GetY(x) + TileSizeInfo.GetInfo(TilePanel.GetSize(x)).Height);
            int width  = right - left;
            int height = bottom - top;

            return (left   * _tileGridCellSize,
                    top    * _tileGridCellSize,
                    width  * _tileGridCellSize - TileSizeInfo.TileMargin,
                    height * _tileGridCellSize - TileSizeInfo.TileMargin);
        }

        private void AdjustImageToFill()
        {
            if (Image == null || _tiles.Count == 0)
                return;

            var (_, _, width, height) = GetTilePixelBounds();

            double imgScale = Math.Max((double)width / Image.PixelWidth, (double)height / Image.PixelHeight);
            double imgX = -(Image.PixelWidth  * imgScale - width ) / 2;
            double imgY = -(Image.PixelHeight * imgScale - height) / 2;

            ImageScale = imgScale;
            ImageX = imgX;
            ImageY = imgY;
        }

        private bool _moveTile = false;
        private bool _moveImage = false;

        private void OnTilePointerPressed(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint pointerPoint = e.GetCurrentPoint(null);
            if (_tiles.Count > 0 && pointerPoint.Properties.IsLeftButtonPressed)
                _moveTile = true;
            else if (Image != null && pointerPoint.Properties.IsRightButtonPressed)
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
            if (Image != null && sender is ImageTile)
            {
                double newScale = ImageScale * (1 + (0.0005 * e.GetCurrentPoint(null).Properties.MouseWheelDelta));
                var (x, y, width, height) = GetTilePixelBounds();
                if (ImageX + Image.PixelWidth  * newScale >= x + width
                 && ImageY + Image.PixelHeight * newScale >= y + height)
                {
                    ImageScale = newScale;
                }
            }
            e.Handled = true;
        }

        private void OnTileManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            if (sender is ImageTile tile && _moveTile)
            {
                Point position = tile.TransformToVisual(DraggingPreviewer).TransformPoint(new Point(0, 0));
                DraggingPreviewer.PreviewerSize = TilePanel.GetSize(tile);
                DraggingPreviewer.PreviewerX = position.X;
                DraggingPreviewer.PreviewerY = position.Y;
                DraggingPreviewer.ImageTranslateTransform.X = -TilePanel.GetX(tile) * _tileGridCellSize + ImageX;
                DraggingPreviewer.ImageTranslateTransform.Y = -TilePanel.GetY(tile) * _tileGridCellSize + ImageY;

                DraggingPreviewer.ShowPreviewer = true;
                tile.Opacity = 0.0;
            }
        }

        private void OnTileManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (sender is ImageTile)
            {
                double dx = e.Delta.Translation.X;
                double dy = e.Delta.Translation.Y;
                if (_moveTile)
                {
                    DraggingPreviewer.PreviewerX += dx;
                    DraggingPreviewer.PreviewerY += dy;
                    DraggingPreviewer.ImageTranslateTransform.X -= dx;
                    DraggingPreviewer.ImageTranslateTransform.Y -= dy;
                }
                else if (_moveImage)
                {
                    var (x, y, width, height) = GetTilePixelBounds();
                    double newX = ImageX + dx;
                    double newY = ImageY + dy;
                    if (newX <= x && newX + Image.PixelWidth  * ImageScale >= width)
                        ImageX = newX;
                    if (newY <= y && newY + Image.PixelHeight * ImageScale >= height)
                        ImageY = newY;
                }
            }
            e.Handled = true;
        }

        private void OnTileManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (_moveTile && sender is ImageTile tile)
            {
                {
                    Point position = DraggingPreviewer.TransformToVisual(_tilePanel)
                                                      .TransformPoint(new Point(DraggingPreviewer.PreviewerX, DraggingPreviewer.PreviewerY));

                    TileSizeInfo sizeInfo = TileSizeInfo.GetInfo(TilePanel.GetSize(tile));

                    int x = Math.Clamp((int)((position.X + (_tileGridCellSize / 2)) / _tileGridCellSize), 0, _tileGridColumnCount - 1);
                    if (x + sizeInfo.Width > _tileGridColumnCount)
                        x -= x + sizeInfo.Width - _tileGridColumnCount;
                    int y = Math.Clamp((int)((position.Y + (_tileGridCellSize / 2)) / _tileGridCellSize), 0, int.MaxValue);

                    MoveTileTo(tile, x, y);
                }

                if (Image != null)
                {
                    var (x, y, width, height) = GetTilePixelBounds();
                    if (ImageX + Image.PixelWidth  * ImageScale <= x + width
                     || ImageY + Image.PixelHeight * ImageScale <= y + height)
                    {
                        ImageScale = Math.Max((x - ImageX + width)  / Image.PixelWidth,
                                              (y - ImageY + height) / Image.PixelHeight);
                    }
                }

                tile.Opacity = 1.0;
                DraggingPreviewer.ShowPreviewer = false;
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
