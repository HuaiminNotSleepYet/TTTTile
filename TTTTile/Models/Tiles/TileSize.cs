using System;

namespace TTTTile.Models.Tiles
{
    public class TileSize
    {
        public static TileSize Small { get; } = new TileSize(1, 1, Windows.UI.StartScreen.TileSize.Square71x71);
        public static TileSize Medium { get; } = new TileSize(2, 2, Windows.UI.StartScreen.TileSize.Square150x150);
        public static TileSize Large { get; } = new TileSize(4, 4, Windows.UI.StartScreen.TileSize.Square310x310);
        public static TileSize Wide { get; } = new TileSize(4, 2, Windows.UI.StartScreen.TileSize.Wide310x150);

        public static int TileMargin => 4;
        public static int TileUnitSize => 48;

        public Windows.UI.StartScreen.TileSize WindowsTileSize { get; }
        public int Width { get; }
        public int Height { get; }
        public int PixelWidth { get; }
        public int PixelHeight { get; }


        private TileSize(int width, int height, Windows.UI.StartScreen.TileSize windowsTileSize)
        {
            if (width < 1 || height < 1)
                throw new ArgumentException();
            WindowsTileSize = windowsTileSize;
            Width = width;
            Height = height;
            PixelWidth = Width * TileUnitSize + (Width - 1) * TileMargin;
            PixelHeight = Height * TileUnitSize + (Height - 1) * TileMargin;
        }
    }
}
