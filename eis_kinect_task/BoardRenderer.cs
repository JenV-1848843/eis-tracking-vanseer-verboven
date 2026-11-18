using System;
using System.Drawing;

namespace eis_kinect_task
{
    public class BoardRenderer
    {
        private const float SymbolPadding = 30;

        public void DrawNoSensor(Graphics graphics, Font font)
        {
            graphics.DrawString("Geen Kinect gevonden. Controleer voeding en USB.", font, Brushes.Black, 40, 12);
        }

        // Tekent hoekpunten voor calibratie: grijs, geel of groene bolletjes.
        public void DrawCalibration(Graphics graphics, FloorCalibration calibration, bool userTracked, Font font)
        {
            System.Windows.Point[] targets = calibration.Targets;

            for (int i = 0; i < targets.Length; i++)
            {
                Brush brush = Brushes.Gray;

                if (i < calibration.PointCount)
                {
                    brush = Brushes.Green;
                }
                else if (i == calibration.PointCount)
                {
                    brush = Brushes.Gold;
                }

                graphics.FillEllipse(brush, (float)targets[i].X - 10, (float)targets[i].Y - 10, 20, 20);
            }

            string tracked = userTracked ? "ja" : "nee";
            string text = "Ga op hoek " + (calibration.PointCount + 1) + " staan en druk op spatie. Persoon gevolgd: " + tracked;
            graphics.DrawString(text, font, Brushes.Black, 80, 12);
        }

        public void DrawGame(Graphics graphics, OxoGame game, BoardLayout layout, Font font)
        {
            DrawCurrentCell(graphics, game, layout);
            DrawGrid(graphics, layout);
            DrawSymbols(graphics, game.Board, layout);
            DrawUser(graphics, game);
            DrawStatus(graphics, game, layout, font);
            DrawOxo(graphics, game.Board, layout);
        }

        private void DrawCurrentCell(Graphics graphics, OxoGame game, BoardLayout layout)
        {
            if (game.CurrentCell >= 0)
            {
                RectangleF bounds = layout.CellBounds(game.CurrentCell);
                graphics.FillRectangle(Brushes.LightYellow, bounds);
            }
        }

        private void DrawGrid(Graphics graphics, BoardLayout layout)
        {
            RectangleF area = layout.Area;
            float cellWidth = area.Width / 3;
            float cellHeight = area.Height / 3;

            using (Pen gridPen = new Pen(Color.Black, 4))
            {
                graphics.DrawRectangle(gridPen, area.Left, area.Top, area.Width, area.Height);

                for (int i = 1; i < 3; i++)
                {
                    graphics.DrawLine(gridPen, area.Left + i * cellWidth, area.Top, area.Left + i * cellWidth, area.Bottom);
                    graphics.DrawLine(gridPen, area.Left, area.Top + i * cellHeight, area.Right, area.Top + i * cellHeight);
                }
            }
        }

        private void DrawSymbols(Graphics graphics, OxoBoard board, BoardLayout layout)
        {
            using (Pen symbolPen = new Pen(Color.DarkRed, 8))
            {
                for (int i = 0; i < OxoBoard.CellCount; i++)
                {
                    RectangleF bounds = layout.CellBounds(i);
                    RectangleF inner = RectangleF.Inflate(bounds, -SymbolPadding, -SymbolPadding);

                    if (board.GetCell(i) == Symbol.X)
                    {
                        graphics.DrawLine(symbolPen, inner.Left, inner.Top, inner.Right, inner.Bottom);
                        graphics.DrawLine(symbolPen, inner.Left, inner.Bottom, inner.Right, inner.Top);
                    }
                    else if (board.GetCell(i) == Symbol.O)
                    {
                        graphics.DrawEllipse(symbolPen, inner);
                    }
                }
            }
        }

        private void DrawUser(Graphics graphics, OxoGame game)
        {
            if (game.HasUser)
            {
                float x = (float)game.UserPoint.X;
                float y = (float)game.UserPoint.Y;
                graphics.FillEllipse(Brushes.Blue, x - 15, y - 15, 30, 30);
            }
        }

        private void DrawStatus(Graphics graphics, OxoGame game, BoardLayout layout, Font font)
        {
            string status;

            if (game.Board.HasOxo)
            {
                status = "OXO! Druk op R voor een nieuw spel.";
            }
            else if (game.Board.IsFull)
            {
                status = "Bord vol, geen OXO. Druk op R voor een nieuw spel.";
            }
            else
            {
                status = "Rechterarm omhoog = X, linkerarm omhoog = O.";
            }

            if (game.Gestures.Progress > 0.0)
            {
                int percent = (int)(game.Gestures.Progress * 100);
                status = status + "  |  gebaar " + game.Gestures.HeldSymbol + ": " + percent + "%";
            }

            graphics.DrawString(status, font, Brushes.Black, layout.Area.Left, 12);
        }

        private void DrawOxo(Graphics graphics, OxoBoard board, BoardLayout layout)
        {
            int firstCell;
            int lastCell;

            if (board.TryGetOxoLine(out firstCell, out lastCell))
            {
                PointF start = layout.CellCenter(firstCell);
                PointF end = layout.CellCenter(lastCell);

                using (Pen linePen = new Pen(Color.Green, 12))
                {
                    graphics.DrawLine(linePen, start, end);
                }

                using (Font bigFont = new Font(FontFamily.GenericSansSerif, 72, FontStyle.Bold))
                {
                    RectangleF area = layout.Area;
                    SizeF textSize = graphics.MeasureString("OXO!", bigFont);
                    float textX = area.Left + (area.Width - textSize.Width) / 2;
                    float textY = area.Top + (area.Height - textSize.Height) / 2;
                    graphics.DrawString("OXO!", bigFont, Brushes.Green, textX, textY);
                }
            }
        }
    }
}
