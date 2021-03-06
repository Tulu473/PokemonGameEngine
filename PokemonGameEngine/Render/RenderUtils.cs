﻿using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Kermalis.PokemonGameEngine.Util;
using System;

namespace Kermalis.PokemonGameEngine.Render
{
    internal sealed class RenderUtils
    {
        public static WriteableBitmap ToWriteableBitmap(Bitmap bmp)
        {
            var wb =  new WriteableBitmap(bmp.PixelSize, bmp.Dpi, PixelFormat.Bgra8888);
            using (IRenderTarget rtb = Utils.RenderInterface.CreateRenderTarget(new[] { new WriteableBitmapSurface(wb) }))
            using (IDrawingContextImpl ctx = rtb.CreateDrawingContext(null))
            {
                var rect = new Rect(bmp.Size);
                ctx.DrawBitmap(bmp.PlatformImpl, 1, rect, rect);
            }
            bmp.Dispose();
            return wb;
        }
        public static unsafe uint[] ToBitmap(WriteableBitmap wb, out int width, out int height)
        {
            using (ILockedFramebuffer l = wb.Lock())
            {
                uint* bmpAddress = (uint*)l.Address.ToPointer();
                PixelSize ps = wb.PixelSize;
                height = ps.Height;
                width = ps.Width;
                uint[] arr = new uint[height * width];
                for (int py = 0; py < height; py++)
                {
                    for (int px = 0; px < width; px++)
                    {
                        int i = px + (py * width);
                        arr[i] = *(bmpAddress + i);
                    }
                }
                return arr;
            }
        }

        public static unsafe Sprite[] LoadSpriteSheet(string resource, int spriteWidth, int spriteHeight)
        {
            uint[][] bitmaps = LoadBitmapSheet(resource, spriteWidth, spriteHeight);
            var arr = new Sprite[bitmaps.Length];
            for (int i = 0; i < bitmaps.Length; i++)
            {
                arr[i] = new Sprite(bitmaps[i], spriteWidth, spriteHeight);
            }
            return arr;
        }
        public static unsafe uint[][] LoadBitmapSheet(string resource, int spriteWidth, int spriteHeight)
        {
            using (WriteableBitmap wb = ToWriteableBitmap(new Bitmap(Utils.GetResourceStream(resource))))
            using (ILockedFramebuffer l = wb.Lock())
            {
                uint* bmpAddress = (uint*)l.Address.ToPointer();
                PixelSize ps = wb.PixelSize;
                int sheetWidth = ps.Width;
                int sheetHeight = ps.Height;
                int numSpritesX = sheetWidth / spriteWidth;
                int numSpritesY = sheetHeight / spriteHeight;
                uint[][] sprites = new uint[numSpritesX * numSpritesY][];
                int sprite = 0;
                for (int sy = 0; sy < numSpritesY; sy++)
                {
                    for (int sx = 0; sx < numSpritesX; sx++)
                    {
                        sprites[sprite++] = GetBitmapUnchecked(bmpAddress, sheetWidth, sx * spriteWidth, sy * spriteHeight, spriteWidth, spriteHeight);
                    }
                }
                return sprites;
            }
        }

        public static unsafe uint[] GetBitmapUnchecked(uint* bmpAddress, int bmpWidth, int x, int y, int width, int height)
        {
            uint[] arr = new uint[width * height];
            for (int py = 0; py < height; py++)
            {
                for (int px = 0; px < width; px++)
                {
                    arr[px + (py * width)] = *(bmpAddress + (x + px) + ((y + py) * bmpWidth));
                }
            }
            return arr;
        }

        public static unsafe void FillColor(uint* bmpAddress, int bmpWidth, int bmpHeight, uint color)
        {
            FillColor(bmpAddress, bmpWidth, bmpHeight, 0, 0, bmpWidth, bmpHeight, color);
        }
        public static unsafe void FillColor(uint* bmpAddress, int bmpWidth, int bmpHeight, int x, int y, int width, int height, uint color)
        {
            for (int py = y; py < y + height; py++)
            {
                if (py >= 0 && py < bmpHeight)
                {
                    for (int px = x; px < x + width; px++)
                    {
                        if (px >= 0 && px < bmpWidth)
                        {
                            DrawUnchecked(bmpAddress + px + (py * bmpWidth), color);
                        }
                    }
                }
            }
        }
        public static unsafe void Modulate(uint* bmpAddress, int bmpWidth, int bmpHeight, float aMod, float rMod, float gMod, float bMod)
        {
            for (int y = 0; y < bmpHeight; y++)
            {
                for (int x = 0; x < bmpWidth; x++)
                {
                    ModulateUnchecked(bmpAddress + x + (y * bmpWidth), aMod, rMod, gMod, bMod);
                }
            }
        }

        public static unsafe void DrawBitmap(uint* bmpAddress, int bmpWidth, int bmpHeight, int x, int y, uint[] otherBmp, int otherBmpWidth, int otherBmpHeight, bool xFlip = false, bool yFlip = false)
        {
            fixed (uint* otherBmpAddress = otherBmp)
            {
                DrawBitmap(bmpAddress, bmpWidth, bmpHeight, x, y, otherBmpAddress, otherBmpWidth, otherBmpHeight, xFlip: xFlip, yFlip: yFlip);
            }
        }
        public static unsafe void DrawBitmap(uint* bmpAddress, int bmpWidth, int bmpHeight, int x, int y, uint* otherBmpAddress, int otherBmpWidth, int otherBmpHeight, bool xFlip = false, bool yFlip = false)
        {
            for (int cy = 0; cy < otherBmpHeight; cy++)
            {
                int py = yFlip ? (y + (otherBmpHeight - 1 - cy)) : (y + cy);
                if (py >= 0 && py < bmpHeight)
                {
                    for (int cx = 0; cx < otherBmpWidth; cx++)
                    {
                        int px = xFlip ? (x + (otherBmpWidth - 1 - cx)) : (x + cx);
                        if (px >= 0 && px < bmpWidth)
                        {
                            DrawUnchecked(bmpAddress + px + (py * bmpWidth), *(otherBmpAddress + cx + (cy * otherBmpWidth)));
                        }
                    }
                }
            }
        }

        public static unsafe void DrawBitmap(uint* bmpAddress, int bmpWidth, int bmpHeight, int x, int y, int width, int height, uint[] otherBmp, int otherBmpWidth, int otherBmpHeight, bool xFlip = false, bool yFlip = false)
        {
            fixed (uint* otherBmpAddress = otherBmp)
            {
                DrawBitmap(bmpAddress, bmpWidth, bmpHeight, x, y, width, height, otherBmpAddress, otherBmpWidth, otherBmpHeight, xFlip: xFlip, yFlip: yFlip);
            }
        }
        public static unsafe void DrawBitmap(uint* bmpAddress, int bmpWidth, int bmpHeight, int x, int y, int width, int height, uint* otherBmpAddress, int otherBmpWidth, int otherBmpHeight, bool xFlip = false, bool yFlip = false)
        {
            // Slight optimization
            if (width == otherBmpWidth && height == otherBmpHeight)
            {
                DrawBitmap(bmpAddress, bmpWidth, bmpHeight, x, y, otherBmpAddress, otherBmpWidth, otherBmpHeight, xFlip: xFlip, yFlip: yFlip);
                return;
            }
            float hScale = (float)height / otherBmpHeight;
            float wScale = (float)width / otherBmpWidth;
            for (int cy = 0; cy < height; cy++)
            {
                int py = yFlip ? (y + (height - 1 - cy)) : (y + cy);
                if (py >= 0 && py < bmpHeight)
                {
                    int ty = (int)(cy / hScale);
                    for (int cx = 0; cx < width; cx++)
                    {
                        int px = xFlip ? (x + (width - 1 - cx)) : (x + cx);
                        if (px >= 0 && px < bmpWidth)
                        {
                            int tx = (int)(cx / wScale);
                            DrawUnchecked(bmpAddress + px + (py * bmpWidth), *(otherBmpAddress + tx + (ty * otherBmpWidth)));
                        }
                    }
                }
            }
        }

        public static unsafe void DrawHorizontalLine(uint* bmpAddress, int bmpWidth, int bmpHeight, int x, int y, int width, uint color)
        {
            if (y < 0 || y >= bmpHeight)
            {
                return;
            }
            int target = x + width;
            for (int px = x; px < target; px++)
            {
                if (px >= 0 && px < bmpWidth)
                {
                    DrawUnchecked(bmpAddress + px + (y * bmpWidth), color);
                }
            }
        }
        public static unsafe void DrawVerticalLine(uint* bmpAddress, int bmpWidth, int bmpHeight, int x, int y, int height, uint color)
        {
            if (x < 0 || x >= bmpWidth)
            {
                return;
            }
            int target = y + height;
            for (int py = y; py < target; py++)
            {
                if (py >= 0 && py < bmpHeight)
                {
                    DrawUnchecked(bmpAddress + x + (py * bmpWidth), color);
                }
            }
        }
        // Bresenham's line algorithm
        public static unsafe void DrawLineLow(uint* bmpAddress, int bmpWidth, int bmpHeight, int x1, int y1, int x2, int y2, uint color)
        {
            int dx = x2 - x1;
            int dy = y2 - y1;
            int yi = 1;
            if (dy < 0)
            {
                yi = -1;
                dy = -dy;
            }
            int d = 2 * dy - dx;
            int py = y1;
            for (int px = x1; px <= x2; px++)
            {
                if (px >= 0 && px < bmpWidth && py >= 0 && py < bmpHeight)
                {
                    DrawUnchecked(bmpAddress + px + (py * bmpWidth), color);
                }
                if (d > 0)
                {
                    py += yi;
                    d -= 2 * dx;
                }
                d += 2 * dy;
            }
        }
        public static unsafe void DrawLineHigh(uint* bmpAddress, int bmpWidth, int bmpHeight, int x1, int y1, int x2, int y2, uint color)
        {
            int dx = x2 - x1;
            int dy = y2 - y1;
            int xi = 1;
            if (dx < 0)
            {
                xi = -1;
                dx = -dx;
            }
            int d = 2 * dx - dy;
            int px = x1;
            for (int py = y1; py <= y2; py++)
            {
                if (px >= 0 && px < bmpWidth && py >= 0 && py < bmpHeight)
                {
                    DrawUnchecked(bmpAddress + px + (py * bmpWidth), color);
                }
                if (d > 0)
                {
                    px += xi;
                    d -= 2 * dy;
                }
                d += 2 * dx;
            }
        }
        public static unsafe void DrawLine(uint* bmpAddress, int bmpWidth, int bmpHeight, int x1, int y1, int x2, int y2, uint color)
        {
            if (x1 == x2)
            {
                int y;
                int height;
                if (y1 < y2)
                {
                    y = y1;
                    height = y2 - y1;
                }
                else
                {
                    y = y2;
                    height = y1 - y2;
                }
                DrawVerticalLine(bmpAddress, bmpWidth, bmpHeight, x1, y, height, color);
            }
            else if (y1 == y2)
            {
                int x;
                int width;
                if (x1 < x2)
                {
                    x = x1;
                    width = x2 - x1;
                }
                else
                {
                    x = x2;
                    width = x1 - x2;
                }
                DrawVerticalLine(bmpAddress, bmpWidth, bmpHeight, x, y1, width, color);
            }
            else if (Math.Abs(y2 - y1) < Math.Abs(x2 - x1))
            {
                if (x1 > x2)
                {
                    DrawLineLow(bmpAddress, bmpWidth, bmpHeight, x2, y2, x1, y1, color);
                }
                else
                {
                    DrawLineLow(bmpAddress, bmpWidth, bmpHeight, x1, y1, x2, y2, color);
                }
            }
            else
            {
                if (y1 > y2)
                {
                    DrawLineHigh(bmpAddress, bmpWidth, bmpHeight, x2, y2, x1, y1, color);
                }
                else
                {
                    DrawLineHigh(bmpAddress, bmpWidth, bmpHeight, x1, y1, x2, y2, color);
                }
            }
        }

        public static unsafe void ModulateUnchecked(uint* pixelAddress, float aMod, float rMod, float gMod, float bMod)
        {
            uint current = *pixelAddress;
            uint a = current >> 24;
            uint r = (current >> 16) & 0xFF;
            uint g = (current >> 8) & 0xFF;
            uint b = current & 0xFF;
            a = (byte)(a * aMod);
            r = (byte)(r * rMod);
            g = (byte)(g * gMod);
            b = (byte)(b * bMod);
            *pixelAddress = (a << 24) | (r << 16) | (g << 8) | b;
        }
        public static unsafe void DrawUnchecked(uint* pixelAddress, uint color)
        {
            if (color == 0x00000000)
            {
                return;
            }
            uint aA = color >> 24;
            if (aA == 0xFF)
            {
                *pixelAddress = color;
            }
            else
            {
                uint rA = (color >> 16) & 0xFF;
                uint gA = (color >> 8) & 0xFF;
                uint bA = color & 0xFF;
                uint current = *pixelAddress;
                uint aB = current >> 24;
                uint rB = (current >> 16) & 0xFF;
                uint gB = (current >> 8) & 0xFF;
                uint bB = current & 0xFF;
                uint a = aA + (aB * (0xFF - aA) / 0xFF);
                uint r = (rA * aA / 0xFF) + (rB * aB * (0xFF - aA) / (0xFF * 0xFF));
                uint g = (gA * aA / 0xFF) + (gB * aB * (0xFF - aA) / (0xFF * 0xFF));
                uint b = (bA * aA / 0xFF) + (bB * aB * (0xFF - aA) / (0xFF * 0xFF));
                *pixelAddress = (a << 24) | (r << 16) | (g << 8) | b;
            }
        }
    }
}
