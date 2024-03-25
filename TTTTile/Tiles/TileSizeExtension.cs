using System;

namespace TTTTile.Tiles
{
    public static class TileSizeExtension
    {
        public static Windows.UI.StartScreen.TileSize AsWindowsTileSize(this TileSize tileSize)
        {
            switch (tileSize)
            {
                case TileSize.Small:  return Windows.UI.StartScreen.TileSize.Square71x71;
                case TileSize.Medium: return Windows.UI.StartScreen.TileSize.Square150x150;
                case TileSize.Wide:   return Windows.UI.StartScreen.TileSize.Wide310x150;
                case TileSize.Large:  return Windows.UI.StartScreen.TileSize.Square310x310;
                default: throw new ArgumentException($"Unknown tile size: {tileSize}.");
            }
        }
    }
}
