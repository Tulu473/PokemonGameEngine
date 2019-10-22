﻿using Avalonia.Media;
using System;

namespace Kermalis.MapEditor.UI
{
    internal sealed class Selection
    {
        private int _x;
        public int X
        {
            get => _x;
            set
            {
                if (_x != value)
                {
                    _x = value;
                    Changed?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        private int _y;
        public int Y
        {
            get => _y;
            set
            {
                if (_y != value)
                {
                    _y = value;
                    Changed?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        private int _width;
        public int Width
        {
            get => _width;
            set
            {
                if (_width != value)
                {
                    _width = value;
                    Changed?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        private int _height;
        public int Height
        {
            get => _height;
            set
            {
                if (_height != value)
                {
                    _height = value;
                    Changed?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler Changed;

        public static SolidColorBrush SelectionBrush, SelectingBrush;
        public static Pen SelectionPen, SelectingPen;

        public Selection()
        {
            Start(0, 0, 1, 1);
        }

        public static void CreateBrushes()
        {
            SelectionBrush = new SolidColorBrush(0x64FF0000);
            SelectingBrush = new SolidColorBrush(0x64FFFF00);
            SelectionPen = new Pen(0x80FF0000);
            SelectingPen = new Pen(0x80FFFF00);
        }

        public void Start(int x, int y, int w, int h)
        {
            X = x;
            Y = y;
            Width = w;
            Height = h;
        }

        public void Move(int x, int y)
        {
            int w = x - _x + 1;
            if (w < 1)
            {
                int diff = 1 - w;
                X -= 1 * diff;
                w += 2 * diff;
            }
            Width = w;
            int h = y - _y + 1;
            if (h < 1)
            {
                int diff = 1 - h;
                Y -= 1 * diff;
                h += 2 * diff;
            }
            Height = h;
        }
    }
}