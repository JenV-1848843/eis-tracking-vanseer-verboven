using Microsoft.Kinect;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace eis_kinect_task
{
    public class GameForm : Form
    {
        private const float BoardMargin = 40;

        private readonly KinectTracker m_tracker = new KinectTracker();
        private readonly OxoGame m_game = new OxoGame();
        private readonly BoardRenderer m_renderer = new BoardRenderer();
        private BoardLayout m_layout = null;
        private FloorCalibration m_calibration = null;
        private Skeleton m_user = null;

        public GameForm()
        {
            Text = "OXO Kinect";
            ClientSize = new Size(800, 600);
            DoubleBuffered = true;
            KeyPreview = true;

            m_tracker.SkeletonUpdated += Tracker_SkeletonUpdated;
            m_tracker.Start();
            ResetCalibration();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Space && m_calibration != null && !m_calibration.IsCalibrated && m_user != null)
            {
                m_calibration.AddPoint(m_user.Position);
            }
            else if (e.KeyCode == Keys.R)
            {
                m_game.Reset();
            }
            else if (e.KeyCode == Keys.C)
            {
                ResetCalibration();
            }

            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            bool calibrationStarted = m_calibration != null && m_calibration.PointCount > 0;

            if (!calibrationStarted)
            {
                ResetCalibration();
            }

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (m_calibration == null)
            {
                m_renderer.DrawNoSensor(e.Graphics, Font);
            }
            else if (!m_calibration.IsCalibrated)
            {
                m_renderer.DrawCalibration(e.Graphics, m_calibration, m_user != null, Font);
            }
            else
            {
                m_renderer.DrawGame(e.Graphics, m_game, m_layout, Font);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            m_tracker.Dispose();
            base.OnFormClosing(e);
        }

        private void Tracker_SkeletonUpdated(Skeleton skeleton)
        {
            m_user = skeleton;

            if (m_calibration != null && m_calibration.IsCalibrated)
            {
                m_game.Update(skeleton, m_calibration, m_layout);
            }

            Invalidate();
        }

        private void ResetCalibration()
        {
            RectangleF area = new RectangleF(
                BoardMargin,
                BoardMargin,
                ClientSize.Width - 2 * BoardMargin,
                ClientSize.Height - 2 * BoardMargin);

            m_layout = new BoardLayout(area);
            m_calibration = null;

            if (m_tracker.IsConnected)
            {
                m_calibration = new FloorCalibration(m_tracker.Sensor, m_layout.Corners());
            }

            m_game.Reset();
        }
    }
}
