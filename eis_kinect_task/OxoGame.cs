using Microsoft.Kinect;

namespace eis_kinect_task
{
    public class OxoGame
    {
        private const int HoldFrames = 30;

        private readonly OxoBoard m_board = new OxoBoard();
        private readonly GestureDetector m_gestures = new GestureDetector(HoldFrames);
        private System.Windows.Point m_userPoint;
        private bool m_hasUser = false;
        private int m_currentCell = -1;

        public OxoBoard Board
        {
            get
            {
                return m_board;
            }
        }

        public GestureDetector Gestures
        {
            get
            {
                return m_gestures;
            }
        }

        public bool HasUser
        {
            get
            {
                return m_hasUser;
            }
        }

        public System.Windows.Point UserPoint
        {
            get
            {
                return m_userPoint;
            }
        }

        public int CurrentCell
        {
            get
            {
                return m_currentCell;
            }
        }

        public void Update(Skeleton skeleton, FloorCalibration calibration, BoardLayout layout)
        {
            m_hasUser = skeleton != null;
            m_currentCell = -1;

            if (m_hasUser)
            {
                m_userPoint = calibration.ToScreen(skeleton.Position);
                m_currentCell = layout.CellAt(m_userPoint);
            }

            Symbol completed = m_gestures.Update(skeleton);

            if (completed != Symbol.Empty)
            {
                m_board.TryPlace(m_currentCell, completed);
            }
        }

        public void Reset()
        {
            m_board.Reset();
            m_gestures.Reset();
        }
    }
}
