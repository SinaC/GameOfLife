using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SparseMatrix
{
    internal class SparseMatrixElement<T>
    {
        public int Row { get; set; }
        public int Column { get; set; }

        public T Value { get; set; }

        public SparseMatrixElement<T> PreviousInRow { get; set; }
        public SparseMatrixElement<T> NextInRow { get; set; }

        public SparseMatrixElement<T> PreviousInColumn { get; set; }
        public SparseMatrixElement<T> NextInColumn { get; set; }

        public void ClearLinks()
        {
            PreviousInRow = null;
            NextInRow = null;

            PreviousInColumn = null;
            NextInColumn = null;
        }
    }

    public class SparseMatrix<T>
    {
        private readonly Dictionary<int, Dictionary<int, SparseMatrixElement<T>>> _columns;

        public SparseMatrix()
        {
            _columns = new Dictionary<int, Dictionary<int, SparseMatrixElement<T>>>();
        }

        public void Clear()
        {
            foreach (KeyValuePair<int, Dictionary<int, SparseMatrixElement<T>>> columnData in _columns)
            {
                foreach(SparseMatrixElement<T> cell in columnData.Value.Values)
                    cell.ClearLinks();
                columnData.Value.Clear();
            }
            _columns.Clear();
        }

        public T this[int row, int col]
        {
            get => GetAt(row, col);
            set => SetAt(row, col, value);
        }

        public T GetAt(int row, int col)
        {
            SparseMatrixElement<T> cell = InternalGetAt(row, col);
            if (cell == null)
                return default(T);
            return cell.Value;
        }

        public void SetAt(int row, int col, T value)
        {
            // Set value
            Dictionary<int, SparseMatrixElement<T>> cols;
            if (!_columns.TryGetValue(col, out cols))
            {
                cols = new Dictionary<int, SparseMatrixElement<T>>();
                _columns.Add(col, cols);
            }

            SparseMatrixElement<T> previousInRow = InternalGetAt(row, col - 1);
            SparseMatrixElement<T> nextInRow = InternalGetAt(row, col + 1);
            SparseMatrixElement<T> previousInColumn = InternalGetAt(row - 1, col);
            SparseMatrixElement<T> nextInColumn = InternalGetAt(row + 1, col);
            cols[row] = new SparseMatrixElement<T>
            {
                Row = row,
                Column = col,

                Value = value,

                PreviousInRow = previousInRow,
                NextInRow = nextInRow,

                PreviousInColumn = previousInColumn,
                NextInColumn = nextInColumn
            };
        }

        public void RemoveAt(int row, int col)
        {
            Dictionary<int, SparseMatrixElement<T>> cols;
            if (_columns.TryGetValue(col, out cols))
            {
                // Clear links
                SparseMatrixElement<T> value;
                if (cols.TryGetValue(row, out value))
                    value.ClearLinks();
                // Remove column from this row
                cols.Remove(row);
                // Remove entire row if empty
                if (cols.Count == 0)
                    _columns.Remove(col);
            }
        }

        public IEnumerable<int> GetRowIndexes()
        {
            return _columns.SelectMany(rowData => rowData.Value.Keys).Distinct();
        }

        public IEnumerable<int> GetColumnIndexes()
        {
            return _columns.Keys;
        }

        public override string ToString()
        {
            List<int> rowIndexes = GetRowIndexes().ToList();
            int rowMin = rowIndexes.Min();
            int rowMax = rowIndexes.Max();

            List<int> columnIndexes = GetColumnIndexes().ToList();
            int columnMin = columnIndexes.Min();
            int columnMax = columnIndexes.Max();

            int cellCount = _columns.SelectMany(col => col.Value.Values).Count();

            StringBuilder matrix = new StringBuilder();
            matrix.AppendLine($"Row {rowMin}->{rowMax}  Column {columnMin}->{columnMax}");
            matrix.AppendLine($"#Cell:{cellCount}");
            matrix.Append("|".PadLeft(4, ' '));
            for (int col = columnMin; col <= columnMax; col++)
                matrix.Append($"{col,3}|");
            matrix.AppendLine();
            matrix.AppendLine("".PadLeft((columnMax - columnMin+2) * 4, '-'));
            for (int row = rowMin; row <= rowMax; row++)
            {
                StringBuilder line = new StringBuilder();
                line.Append($"{row,3}|");
                for (int col = columnMin; col <= columnMax; col++)
                {
                    SparseMatrixElement<T> value = InternalGetAt(row, col);
                    if (value == null)
                        line.Append("|".PadLeft(4));
                    else
                        line.Append(value.Value.ToString().PadLeft(3, ' ') + "|");
                }
                matrix.AppendLine(line.ToString());
                matrix.AppendLine("".PadLeft((columnMax - columnMin + 2) * 4, '-'));
            }
            return matrix.ToString();
        }

        private SparseMatrixElement<T> InternalGetAt(int row, int col)
        {
            Dictionary<int, SparseMatrixElement<T>> cols;
            if (_columns.TryGetValue(col, out cols))
            {
                SparseMatrixElement<T> value;
                if (cols.TryGetValue(row, out value))
                    return value;
            }
            return null;
        }
    }
}
