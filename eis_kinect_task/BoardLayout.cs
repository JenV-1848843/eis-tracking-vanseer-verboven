using System;
using System.Drawing;

namespace eis_kinect_task
{
    public class BoardLayout
    {
        private const int GridSize = 3;

        private readonly RectangleF m_area;

        public BoardLayout(RectangleF area)
        {
            m_area = area;
        }

        public RectangleF Area
        {
            get
            {
                return m_area;
            }
        }

        public System.Windows.Point[] Corners()
        {
            return new System.Windows.Point[]
            {
                new System.Windows.Point(m_area.Left, m_area.Top),
                new System.Windows.Point(m_area.Right, m_area.Top),
                new System.Windows.Point(m_area.Right, m_area.Bottom),
                new System.Windows.Point(m_area.Left, m_area.Bottom)
            };
        }

        public int CellAt(System.Windows.Point point)
        {
            double x = point.X - m_area.Left;
            double y = point.Y - m_area.Top;
            int cell = -1;

            if (x >= 0 && x < m_area.Width && y >= 0 && y < m_area.Height)
            {
                int column = Math.Min(GridSize - 1, (int)(x / (m_area.Width / GridSize)));
                int row = Math.Min(GridSize - 1, (int)(y / (m_area.Height / GridSize)));
                cell = row * GridSize + column;
            }

            return cell;
        }

        public RectangleF CellBounds(int index)
        {
            float cellWidth = m_area.Width / GridSize;
            float cellHeight = m_area.Height / GridSize;
            float left = m_area.Left + (index % GridSize) * cellWidth;
            float top = m_area.Top + (index / GridSize) * cellHeight;
            return new RectangleF(left, top, cellWidth, cellHeight);
        }

        public PointF CellCenter(int index)
        {
            RectangleF bounds = CellBounds(index);
            return new PointF(bounds.Left + bounds.Width / 2, bounds.Top + bounds.Height / 2);
        }
    }
}
