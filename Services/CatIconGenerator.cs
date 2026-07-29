using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace AgyUsageShower.Services
{
    public static class CatIconGenerator
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        private static readonly Icon[] IconCache = new Icon[5];

        public static Icon GetCatFrameIcon(int frameIndex, double remainingQuotaPercent)
        {
            frameIndex = Math.Abs(frameIndex % 5);

            using Bitmap bmp = new Bitmap(32, 32);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                // Choose color based on quota status
                Color catColor = remainingQuotaPercent >= 70 ? Color.FromArgb(166, 227, 161) : // Green (#A6E3A1)
                                remainingQuotaPercent >= 30 ? Color.FromArgb(249, 226, 175) : // Yellow (#F9E2AF)
                                                              Color.FromArgb(243, 139, 168);  // Red (#F38BA8)

                using SolidBrush brush = new SolidBrush(catColor);

                // Draw cat body & legs according to animation frame pose
                switch (frameIndex)
                {
                    case 0: // Extended leap
                        g.FillEllipse(brush, 8, 12, 14, 8); // Body
                        g.FillEllipse(brush, 18, 9, 7, 7);  // Head
                        g.FillPolygon(brush, new Point[] { new Point(20, 9), new Point(22, 5), new Point(23, 10) }); // Ear
                        g.FillRectangle(brush, 19, 17, 6, 3); // Front paws extended
                        g.FillRectangle(brush, 4, 16, 6, 3);  // Rear paws extended back
                        g.FillRectangle(brush, 2, 10, 5, 3);  // Tail up
                        break;

                    case 1: // Front paws contact
                        g.FillEllipse(brush, 8, 11, 14, 8);
                        g.FillEllipse(brush, 18, 10, 7, 7);
                        g.FillPolygon(brush, new Point[] { new Point(20, 10), new Point(22, 6), new Point(23, 11) });
                        g.FillRectangle(brush, 18, 18, 4, 4);
                        g.FillRectangle(brush, 6, 17, 5, 3);
                        g.FillRectangle(brush, 2, 8, 5, 3);
                        break;

                    case 2: // Mid-stride gathered
                        g.FillEllipse(brush, 8, 10, 13, 9);
                        g.FillEllipse(brush, 17, 9, 7, 7);
                        g.FillPolygon(brush, new Point[] { new Point(19, 9), new Point(21, 5), new Point(22, 10) });
                        g.FillRectangle(brush, 14, 17, 4, 4);
                        g.FillRectangle(brush, 9, 17, 4, 4);
                        g.FillRectangle(brush, 3, 12, 5, 3);
                        break;

                    case 3: // Rear push off
                        g.FillEllipse(brush, 8, 11, 14, 8);
                        g.FillEllipse(brush, 18, 9, 7, 7);
                        g.FillPolygon(brush, new Point[] { new Point(20, 9), new Point(22, 5), new Point(23, 10) });
                        g.FillRectangle(brush, 11, 17, 5, 4);
                        g.FillRectangle(brush, 3, 16, 6, 3);
                        g.FillRectangle(brush, 2, 13, 5, 3);
                        break;

                    case 4: // Airborne float
                        g.FillEllipse(brush, 8, 9, 14, 8);
                        g.FillEllipse(brush, 18, 7, 7, 7);
                        g.FillPolygon(brush, new Point[] { new Point(20, 7), new Point(22, 3), new Point(23, 8) });
                        g.FillRectangle(brush, 18, 15, 6, 3);
                        g.FillRectangle(brush, 3, 14, 6, 3);
                        g.FillRectangle(brush, 2, 7, 5, 3);
                        break;
                }

                // Draw tiny status indicator bar at the bottom of tray icon
                using SolidBrush barBg = new SolidBrush(Color.FromArgb(49, 50, 68));
                g.FillRectangle(barBg, 2, 28, 28, 3);

                int fillWidth = (int)Math.Max(2, (remainingQuotaPercent / 100.0) * 28);
                g.FillRectangle(brush, 2, 28, fillWidth, 3);
            }

            IntPtr hIcon = bmp.GetHicon();
            Icon icon = (Icon)Icon.FromHandle(hIcon).Clone();
            DestroyIcon(hIcon);
            return icon;
        }
    }
}
