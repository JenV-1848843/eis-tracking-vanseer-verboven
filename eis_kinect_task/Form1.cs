using Microsoft.Kinect;
using Microsoft.Samples.Kinect.ControlsBasics;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace eis_kinect_task
{
    public partial class Form1 : Form
    {
        private const int Margin40 = 40;
        private const int HoldFrames = 30;
        private const int SymbolX = 1;
        private const int SymbolO = 2;

        private KinectSensor m_sensor = null;
        private PartialCalibrationClass m_calibration = null;
        private Skeleton[] m_skeletons = new Skeleton[0];
        private SkeletonPoint m_position;
        private bool m_hasPosition = false;
        private int m_pointCount = 0;

        private System.Windows.Point m_screenPoint;
        private int[] m_board = new int[9];
        private int m_oxoLine = -1;
        private bool m_boardFull = false;
        private int m_currentCell = -1;

        private int m_heldGesture = 0;
        private int m_heldFrames = 0;
        private bool m_latched = false;

        public Form1()
        {
            InitializeComponent();
            ClientSize = new Size(800, 600);
            DoubleBuffered = true;
            KeyPreview = true;
            KeyDown += Form1_KeyDown;
            Paint += Form1_Paint;
            FormClosing += Form1_FormClosing;

            foreach (KinectSensor candidate in KinectSensor.KinectSensors)
            {
                if (m_sensor == null && candidate.Status == KinectStatus.Connected)
                {
                    m_sensor = candidate;
                }
            }

            if (m_sensor != null)
            {
                m_sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                m_sensor.SkeletonStream.Enable();
                m_sensor.SkeletonFrameReady += Sensor_SkeletonFrameReady;
                m_sensor.Start();
                m_calibration = new PartialCalibrationClass(m_sensor);
            }
        }

        private System.Windows.Point[] Corners()
        {
            return new System.Windows.Point[]
            {
                new System.Windows.Point(Margin40, Margin40),
                new System.Windows.Point(ClientSize.Width - Margin40, Margin40),
                new System.Windows.Point(ClientSize.Width - Margin40, ClientSize.Height - Margin40),
                new System.Windows.Point(Margin40, ClientSize.Height - Margin40)
            };
        }

        private void Sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    if (m_skeletons.Length != frame.SkeletonArrayLength)
                    {
                        m_skeletons = new Skeleton[frame.SkeletonArrayLength];
                    }

                    frame.CopySkeletonDataTo(m_skeletons);
                    m_hasPosition = false;
                    int gesture = 0;

                    foreach (Skeleton skeleton in m_skeletons)
                    {
                        if (!m_hasPosition && skeleton != null && skeleton.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            m_position = skeleton.Position;
                            m_hasPosition = true;
                            gesture = DetectGesture(skeleton);
                        }
                    }

                    if (m_pointCount == 4)
                    {
                        UpdateGame(gesture);
                    }

                    Invalidate();
                }
            }
        }

        private int DetectGesture(Skeleton skeleton)
        {
            SkeletonPoint handLeft = skeleton.Joints[JointType.HandLeft].Position;
            SkeletonPoint handRight = skeleton.Joints[JointType.HandRight].Position;
            SkeletonPoint head = skeleton.Joints[JointType.Head].Position;

            bool rightUp = handRight.Y > head.Y;
            bool leftUp = handLeft.Y > head.Y;
            int gesture = 0;

            if (rightUp && !leftUp)
            {
                gesture = SymbolX;
            }
            else if (leftUp && !rightUp)
            {
                gesture = SymbolO;
            }

            return gesture;
        }

        private void UpdateGame(int gesture)
        {
            m_currentCell = -1;

            if (m_hasPosition)
            {
                m_screenPoint = m_calibration.kinectToProjectionPoint(m_position);
                m_currentCell = CellFromPoint(m_screenPoint);
            }

            if (gesture == 0)
            {
                m_heldGesture = 0;
                m_heldFrames = 0;
                m_latched = false;
            }
            else if (gesture != m_heldGesture)
            {
                m_heldGesture = gesture;
                m_heldFrames = 1;
            }
            else
            {
                m_heldFrames++;
            }

            bool canPlace = !m_latched && m_heldFrames >= HoldFrames && m_oxoLine < 0 && m_currentCell >= 0;

            if (canPlace && m_board[m_currentCell] == 0)
            {
                m_board[m_currentCell] = gesture;
                m_latched = true;
                CheckBoard();
            }
        }

        private int CellFromPoint(System.Windows.Point point)
        {
            double width = ClientSize.Width - 2 * Margin40;
            double height = ClientSize.Height - 2 * Margin40;
            double x = point.X - Margin40;
            double y = point.Y - Margin40;
            int cell = -1;

            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                int column = Math.Min(2, (int)(x / (width / 3)));
                int row = Math.Min(2, (int)(y / (height / 3)));
                cell = row * 3 + column;
            }

            return cell;
        }

        private int[,] Lines()
        {
            return new int[,]
            {
                { 0, 1, 2 }, { 3, 4, 5 }, { 6, 7, 8 },
                { 0, 3, 6 }, { 1, 4, 7 }, { 2, 5, 8 },
                { 0, 4, 8 }, { 2, 4, 6 }
            };
        }

        private void CheckBoard()
        {
            int[,] lines = Lines();
            m_oxoLine = -1;

            for (int i = 0; i < 8; i++)
            {
                bool isOxo = m_board[lines[i, 0]] == SymbolO && m_board[lines[i, 1]] == SymbolX && m_board[lines[i, 2]] == SymbolO;

                if (m_oxoLine < 0 && isOxo)
                {
                    m_oxoLine = i;
                }
            }

            m_boardFull = true;

            for (int i = 0; i < 9; i++)
            {
                if (m_board[i] == 0)
                {
                    m_boardFull = false;
                }
            }
        }

        private void ResetGame()
        {
            m_board = new int[9];
            m_oxoLine = -1;
            m_boardFull = false;
            m_heldGesture = 0;
            m_heldFrames = 0;
            m_latched = false;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space && m_calibration != null && m_hasPosition && m_pointCount < 4)
            {
                m_calibration.addCalibrationPoint(Corners()[m_pointCount], m_position);
                m_pointCount++;

                if (m_pointCount == 4)
                {
                    m_calibration.calibrate();
                }

                Invalidate();
            }
            else if (e.KeyCode == Keys.R && m_pointCount == 4)
            {
                ResetGame();
                Invalidate();
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if (m_pointCount < 4)
            {
                PaintCalibration(e.Graphics);
            }
            else
            {
                PaintGame(e.Graphics);
            }
        }

        private void PaintCalibration(Graphics graphics)
        {
            System.Windows.Point[] corners = Corners();

            for (int i = 0; i < corners.Length; i++)
            {
                Brush brush = i < m_pointCount ? Brushes.Green : Brushes.Gray;
                graphics.FillEllipse(brush, (float)corners[i].X - 10, (float)corners[i].Y - 10, 20, 20);
            }

            string tracked = m_hasPosition ? "ja" : "nee";
            string text = "Ga op hoek " + (m_pointCount + 1) + " staan en druk op spatie. Persoon gevolgd: " + tracked;
            graphics.DrawString(text, Font, Brushes.Black, 80, 12);
        }

        private void PaintGame(Graphics graphics)
        {
            float width = ClientSize.Width - 2 * Margin40;
            float height = ClientSize.Height - 2 * Margin40;
            float cellWidth = width / 3;
            float cellHeight = height / 3;

            if (m_currentCell >= 0)
            {
                float cellX = Margin40 + (m_currentCell % 3) * cellWidth;
                float cellY = Margin40 + (m_currentCell / 3) * cellHeight;
                graphics.FillRectangle(Brushes.LightYellow, cellX, cellY, cellWidth, cellHeight);
            }

            using (Pen gridPen = new Pen(Color.Black, 4))
            {
                graphics.DrawRectangle(gridPen, Margin40, Margin40, width, height);

                for (int i = 1; i < 3; i++)
                {
                    graphics.DrawLine(gridPen, Margin40 + i * cellWidth, Margin40, Margin40 + i * cellWidth, Margin40 + height);
                    graphics.DrawLine(gridPen, Margin40, Margin40 + i * cellHeight, Margin40 + width, Margin40 + i * cellHeight);
                }
            }

            using (Pen symbolPen = new Pen(Color.DarkRed, 8))
            {
                for (int i = 0; i < 9; i++)
                {
                    float left = Margin40 + (i % 3) * cellWidth + 30;
                    float top = Margin40 + (i / 3) * cellHeight + 30;
                    float right = left + cellWidth - 60;
                    float bottom = top + cellHeight - 60;

                    if (m_board[i] == SymbolX)
                    {
                        graphics.DrawLine(symbolPen, left, top, right, bottom);
                        graphics.DrawLine(symbolPen, left, bottom, right, top);
                    }
                    else if (m_board[i] == SymbolO)
                    {
                        graphics.DrawEllipse(symbolPen, left, top, right - left, bottom - top);
                    }
                }
            }

            if (m_hasPosition)
            {
                graphics.FillEllipse(Brushes.Blue, (float)m_screenPoint.X - 15, (float)m_screenPoint.Y - 15, 30, 30);
            }

            string status;

            if (m_oxoLine >= 0)
            {
                status = "OXO! Druk op R voor een nieuw spel.";
            }
            else if (m_boardFull)
            {
                status = "Bord vol, geen OXO. Druk op R voor een nieuw spel.";
            }
            else
            {
                status = "Rechterarm omhoog = X, linkerarm omhoog = O.";
            }

            if (m_heldGesture != 0 && !m_latched)
            {
                string symbol = m_heldGesture == SymbolX ? "X" : "O";
                int percent = Math.Min(100, m_heldFrames * 100 / HoldFrames);
                status = status + "  |  gebaar " + symbol + ": " + percent + "%";
            }

            graphics.DrawString(status, Font, Brushes.Black, Margin40, 12);

            if (m_oxoLine >= 0)
            {
                int[,] lines = Lines();
                int firstCell = lines[m_oxoLine, 0];
                int lastCell = lines[m_oxoLine, 2];
                float startX = Margin40 + (firstCell % 3 + 0.5f) * cellWidth;
                float startY = Margin40 + (firstCell / 3 + 0.5f) * cellHeight;
                float endX = Margin40 + (lastCell % 3 + 0.5f) * cellWidth;
                float endY = Margin40 + (lastCell / 3 + 0.5f) * cellHeight;

                using (Pen linePen = new Pen(Color.Green, 12))
                {
                    graphics.DrawLine(linePen, startX, startY, endX, endY);
                }

                using (Font bigFont = new Font(Font.FontFamily, 72, FontStyle.Bold))
                {
                    SizeF textSize = graphics.MeasureString("OXO!", bigFont);
                    float textX = (ClientSize.Width - textSize.Width) / 2;
                    float textY = (ClientSize.Height - textSize.Height) / 2;
                    graphics.DrawString("OXO!", bigFont, Brushes.Green, textX, textY);
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_sensor != null)
            {
                m_sensor.Stop();
            }
        }
    }
}