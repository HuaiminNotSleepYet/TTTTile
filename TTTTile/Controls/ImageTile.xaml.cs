using TTTTile.Tiles;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace TTTTile.Controls
{
    public sealed partial class ImageTile : UserControl
    {
        public TileSize Size { get; }

        public ImageSource ImageSource
        {
            get => _imageBrush.ImageSource;
            set => _imageBrush.ImageSource = value;
        }

        public ScaleTransform ImageScaleTransform => _imageBrushScaleTransform;

        public TranslateTransform ImageTranslateTransform => _imageBrushTranslateTransform;

        public ImageTile(TileSize size)
        {
            Size = size;

            TileSizeInfo sizeInfo = TileSizeInfo.GetInfo(size);
            Width = sizeInfo.PixelWidth;
            Height = sizeInfo.PixelHeight;

            InitializeComponent();
        }
    }
}
