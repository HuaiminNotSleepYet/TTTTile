using TTTTile.Tiles;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace TTTTile.Controls
{
    public sealed partial class ImageTileDraggingPreviewer : UserControl
    {
        private bool _showPreviewer = false;
        public bool ShowPreviewer
        {
            get { return _showPreviewer; }
            set 
            {
                _imageTile.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                _showPreviewer = value; 
            }
        }

        public double PreviewerX
        {
            get { return Canvas.GetLeft(_imageTile); }
            set { Canvas.SetLeft(_imageTile, value); }
        }

        public double PreviewerY
        {
            get { return Canvas.GetTop(_imageTile); }
            set { Canvas.SetTop(_imageTile, value); }
        }

        private TileSize _previewerSize = TileSize.Medium;
        public TileSize PreviewerSize
        {
            get { return _previewerSize; }
            set
            {
                TileSizeInfo sizeInfo = TileSizeInfo.GetInfo(value);
                _imageTile.Width  = sizeInfo.PixelWidth;
                _imageTile.Height = sizeInfo.PixelHeight;
                _previewerSize = value;
            }
        }

        public ImageSource ImageSource
        {
            get { return _imageTile.ImageSource;  }
            set { _imageTile.ImageSource = value; }
        }

        public ScaleTransform ImageScaleTransform => _imageTile.ImageScaleTransform;

        public TranslateTransform ImageTranslateTransform => _imageTile.ImageTranslateTransform;

        public ImageTileDraggingPreviewer()
        {
            InitializeComponent();
        }
    }
}
