using System;
using TTTTile.Tiles;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace TTTTile.Controls
{
    public class TilePanel : Panel
    {
        public static readonly DependencyProperty XProperty =
            DependencyProperty.RegisterAttached("X", typeof(int), typeof(TilePanel), new PropertyMetadata(0, OnTilePositionChanged));

        public static readonly DependencyProperty YProperty =
            DependencyProperty.RegisterAttached("Y", typeof(int), typeof(TilePanel), new PropertyMetadata(0, OnTilePositionChanged));

        public static readonly DependencyProperty SizeProperty =
            DependencyProperty.RegisterAttached("Size", typeof(TileSize), typeof(TilePanel), new PropertyMetadata(TileSize.Medium, OnTilePositionChanged));

        private static void OnTilePositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                if (VisualTreeHelper.GetParent(element) is TilePanel panel)
                {
                    panel.InvalidateMeasure();
                    panel.InvalidateArrange();
                }
            }
        }

        public static int GetX(DependencyObject obj) { return (int)obj.GetValue(XProperty); }
        public static void SetX(DependencyObject obj, int value) { obj.SetValue(XProperty, value); }

        public static int GetY(DependencyObject obj) { return (int)obj.GetValue(YProperty); }
        public static void SetY(DependencyObject obj, int value) { obj.SetValue(YProperty, value); }

        public static TileSize GetSize(DependencyObject obj) { return (TileSize)obj.GetValue(SizeProperty); }
        public static void SetSize(DependencyObject obj, TileSize value) { obj.SetValue(SizeProperty, value); }

        public TilePanel() { }

        protected override Size MeasureOverride(Size availableSize)
        {
            int x = 0;
            int y = 0;
            foreach (UIElement element in Children)
            {
                TileSizeInfo sizeInfo = TileSizeInfo.GetInfo(GetSize(element));
                x = Math.Max(x, GetX(element) + sizeInfo.Width);
                y = Math.Max(y, GetY(element) + sizeInfo.Height);
                element.Measure(new Size(sizeInfo.PixelWidth, sizeInfo.PixelHeight));
            }

            return new Size(x * (TileSizeInfo.SmallTilePixelSize + TileSizeInfo.TileMargin) + TileSizeInfo.TileMargin,
                            y * (TileSizeInfo.SmallTilePixelSize + TileSizeInfo.TileMargin) + TileSizeInfo.TileMargin);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement element in Children)
            {
                int x = GetX(element);
                int y = GetY(element);
                TileSizeInfo sizeInfo = TileSizeInfo.GetInfo(GetSize(element));
                element.Arrange(new Rect(
                    x * (TileSizeInfo.SmallTilePixelSize + TileSizeInfo.TileMargin) + TileSizeInfo.TileMargin,
                    y * (TileSizeInfo.SmallTilePixelSize + TileSizeInfo.TileMargin) + TileSizeInfo.TileMargin,
                    sizeInfo.PixelWidth,
                    sizeInfo.PixelHeight));
            }

            return finalSize;
        }
    }
}
