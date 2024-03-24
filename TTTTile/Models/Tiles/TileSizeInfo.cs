﻿using System;
using Windows.UI.StartScreen;

namespace TTTTile.Models.Tiles
{
    public class TileSizeInfo
    {
        public static int SmallTilePixelSize { get; } = 48;
        public static int TileMargin { get; } = 4;

        public static TileSize SmallTileSize  { get; } = TileSize.Square71x71;
        public static TileSize MediumTileSize { get; } = TileSize.Square150x150;
        public static TileSize WideTileSize   { get; } = TileSize.Wide310x150;
        public static TileSize LargeTileSize  { get; } = TileSize.Square310x310;

        public int Width { get; }
        public int Height { get; }
        public int PixelWidth { get; }
        public int PixelHeight { get; }

        private TileSizeInfo(int width, int height)
        {
            if (width < 1 || height < 1)
                throw new ArgumentOutOfRangeException("Tile size can't less than 1.");
            Width = width;
            Height = height;
            PixelWidth = width * SmallTilePixelSize + (width - 1) * TileMargin;
            PixelHeight = height * SmallTilePixelSize + (height - 1) * TileMargin;
        }

        private static readonly TileSizeInfo _smallSizeInfo  = new TileSizeInfo(1, 1);
        private static readonly TileSizeInfo _mediumSizeInfo = new TileSizeInfo(2, 2);
        private static readonly TileSizeInfo _wideSizeInfo   = new TileSizeInfo(4, 2);
        private static readonly TileSizeInfo _largeSizeInfo  = new TileSizeInfo(4, 4);

        public static TileSizeInfo GetInfo(TileSize size)
        {
            switch (size)
            {
                case TileSize.Square71x71:   return _smallSizeInfo;
                case TileSize.Square150x150: return _mediumSizeInfo;
                case TileSize.Wide310x150:   return _wideSizeInfo;
                case TileSize.Square310x310: return _largeSizeInfo;
                default: throw new ArgumentException($"Unsupported tile size: {size}.");
            }
        }
    }
}
