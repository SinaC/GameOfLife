using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameOfLife
{
    internal class SparseMatrix<T>
    {
        // Hold rows of column dictionary
        private readonly Dictionary<int, Dictionary<int, T>> _rows;

        public SparseMatrix()
        {
            _rows = new Dictionary<int, Dictionary<int, T>>();
        }

        public void Clear()
        {
            foreach (KeyValuePair<int, Dictionary<int, T>> columnData in _rows)
                columnData.Value.Clear();
            _rows.Clear();
        }

        public T this[int row, int col]
        {
            get
            {
                return GetAt(row, col);
            }
            set
            {
                SetAt(row, col, value);
            }
        }

        public T GetAt(int row, int col)
        {
            Dictionary<int, T> cols;
            if (_rows.TryGetValue(row, out cols))
            {
                T value = default(T);
                if (cols.TryGetValue(col, out value))
                    return value;
            }
            return default(T);
        }

        public void SetAt(int row, int col, T value)
        {
            if (EqualityComparer<T>.Default.Equals(value, default(T)))
                // Remove any existing object if value is default(T)
                RemoveAt(row, col);
            else
            {
                // Set value
                Dictionary<int, T> cols;
                if (!_rows.TryGetValue(row, out cols))
                {
                    cols = new Dictionary<int, T>();
                    _rows.Add(row, cols);
                }
                cols[col] = value;
            }
        }

        public void RemoveAt(int row, int col)
        {
            Dictionary<int, T> cols;
            if (_rows.TryGetValue(row, out cols))
            {
                // Remove column from this row
                cols.Remove(col);
                // Remove entire row if empty
                if (cols.Count == 0)
                    _rows.Remove(row);
            }
        }

        public IEnumerable<int> GetRowIndexes()
        {
            return _rows.Keys;
        }

        public IEnumerable<int> GetColumnIndexes()
        {
            return _rows.SelectMany(rowData => rowData.Value.Keys).Distinct();
        }

        public int GetRowDataCount(int row)
        {
            Dictionary<int, T> cols;
            if (_rows.TryGetValue(row, out cols))
                return cols.Count;
            return 0;
        }

        public IEnumerable<T> GetRowData(int row)
        {
            Dictionary<int, T> cols;
            if (_rows.TryGetValue(row, out cols))
                foreach (KeyValuePair<int, T> pair in cols)
                    yield return pair.Value;
        }

        public int GetColumnDataCount(int col)
        {
            return _rows.Count(cols => cols.Value.ContainsKey(col));
        }

        public IEnumerable<T> GetColumnData(int col)
        {
            foreach (KeyValuePair<int, Dictionary<int, T>> rowData in _rows)
            {
                T result;
                if (rowData.Value.TryGetValue(col, out result))
                    yield return result;
            }
        }

        public IEnumerable<T> GetData()
        {
            return _rows.SelectMany(row => row.Value.Values);
        }

        public override string ToString()
        {
            List<int> rowIndexes = GetRowIndexes().ToList();
            int rowMin = rowIndexes.Min();
            int rowMax = rowIndexes.Max();

            List<int> columnIndexes = GetColumnIndexes().ToList();
            int columnMin = columnIndexes.Min();
            int columnMax = columnIndexes.Max();

            StringBuilder matrix = new StringBuilder();
            for(int y = columnMin; y <= columnMax; y++)
            {
                StringBuilder line = new StringBuilder();
                for (int x = rowMin; x <= rowMax; x++)
                {
                    T value = this[x, y];
                    if (EqualityComparer<T>.Default.Equals(value, default(T)))
                        line.Append(" ");
                    else
                        line.Append(value);
                }
                matrix.AppendLine(line.ToString());
            }
            return matrix.ToString();
        }
    }
}
