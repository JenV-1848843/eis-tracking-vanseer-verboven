namespace eis_kinect_task
{
    public class OxoBoard
    {
        public const int CellCount = 9;

        private static readonly int[,] s_lines = new int[,]
        {
            { 0, 1, 2 }, { 3, 4, 5 }, { 6, 7, 8 },
            { 0, 3, 6 }, { 1, 4, 7 }, { 2, 5, 8 },
            { 0, 4, 8 }, { 2, 4, 6 }
        };

        private Symbol[] m_cells = new Symbol[CellCount];
        private int m_oxoLine = -1;

        public bool HasOxo
        {
            get
            {
                return m_oxoLine >= 0;
            }
        }

        public bool IsFull
        {
            get
            {
                bool full = true;

                for (int i = 0; i < CellCount; i++)
                {
                    if (m_cells[i] == Symbol.Empty)
                    {
                        full = false;
                    }
                }

                return full;
            }
        }

        public bool IsFinished
        {
            get
            {
                return HasOxo || IsFull;
            }
        }

        public Symbol GetCell(int index)
        {
            return m_cells[index];
        }

        public bool TryPlace(int index, Symbol symbol)
        {
            bool placed = false;
            bool validIndex = index >= 0 && index < CellCount;

            if (!IsFinished && validIndex && symbol != Symbol.Empty && m_cells[index] == Symbol.Empty)
            {
                m_cells[index] = symbol;
                placed = true;
                UpdateOxoLine();
            }

            return placed;
        }

        public bool TryGetOxoLine(out int firstCell, out int lastCell)
        {
            firstCell = -1;
            lastCell = -1;

            if (HasOxo)
            {
                firstCell = s_lines[m_oxoLine, 0];
                lastCell = s_lines[m_oxoLine, 2];
            }

            return HasOxo;
        }

        public void Reset()
        {
            m_cells = new Symbol[CellCount];
            m_oxoLine = -1;
        }

        private void UpdateOxoLine()
        {
            for (int i = 0; i < s_lines.GetLength(0); i++)
            {
                bool isOxo = m_cells[s_lines[i, 0]] == Symbol.O
                    && m_cells[s_lines[i, 1]] == Symbol.X
                    && m_cells[s_lines[i, 2]] == Symbol.O;

                if (m_oxoLine < 0 && isOxo)
                {
                    m_oxoLine = i;
                }
            }
        }
    }
}
