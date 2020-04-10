using System;
using System.Text;

namespace SparseMatrix
{
    // Non-circular
    internal class SparseVectorElement<T>
    {
        public int Index { get; set; }

        public SparseVectorElement<T> Previous { get; set; }
        public SparseVectorElement<T> Next { get; set; }

        public T Value { get; set; }
    }

    public class SparseVector<T>
    {
        private SparseVectorElement<T> _header;

        public T this[int index]
        {
            get
            {
                // List empty
                if (_header == null)
                    return default(T);
                // Lucky case: header
                if (_header.Index == index)
                    return _header.Value;
                // Check if moving backward or forward
                Func<SparseVectorElement<T>, SparseVectorElement<T>> moveFunc;
                Func<int, int, bool> compareFunc;
                // Will move backward
                if (index < _header.Index)
                {
                    moveFunc = p => p.Previous;
                    compareFunc = (i, j) => i < j;
                }
                // Will move forward
                else
                {
                    moveFunc = p => p.Next;
                    compareFunc = (i, j) => i > j;
                }
                // Move
                SparseVectorElement<T> current = moveFunc(_header);
                while (current != null && compareFunc(index, current.Index))
                    current = moveFunc(current);
                if (current == null)
                    return default(T);
                if (current.Index == index)
                    return current.Value;
                return default(T);
            }
            set
            {
                // First element
                if (_header == null)
                {
                    _header = new SparseVectorElement<T>
                    {
                        Index = index,
                        Value = value,
                        Previous = null,
                        Next = null
                    };
                }
                else
                {
                    // Modify header
                    if (_header.Index == index)
                        _header.Value = value;
                    else
                    {
                        // Check if moving backward or forward
                        Func<SparseVectorElement<T>, SparseVectorElement<T>> moveFunc;
                        Func<int, int, bool> compareFunc;
                        Action<SparseVectorElement<T>, SparseVectorElement<T>, int, T> insertFunc;
                        // Will move backward
                        if (index < _header.Index)
                        {
                            moveFunc = p => p.Previous;
                            compareFunc = (i, j) => i < j;
                            insertFunc = InsertBackward;
                        }
                        // Will move forward
                        else
                        {
                            moveFunc = p => p.Next;
                            compareFunc = (i, j) => i > j;
                            insertFunc = InsertForward;
                        }
                        // Move
                        SparseVectorElement<T> previous = _header;
                        SparseVectorElement<T> current = moveFunc(_header);
                        while (current != null && compareFunc(index, current.Index))
                        {
                            previous = current;
                            current = moveFunc(current);
                        }
                        // Update/Insert
                        if (current != null && current.Index == index)
                            current.Value = value;
                        else
                            insertFunc(previous, current, index, value);
                    }
                }
            }
        }

        private static void InsertForward(SparseVectorElement<T> previous, SparseVectorElement<T> current, int index, T value)
        {
            SparseVectorElement<T> newElement = new SparseVectorElement<T>
            {
                Index = index,
                Value = value,
                Previous = previous,
                Next = current
            };
            previous.Next = newElement;
            if (current != null)
                current.Previous = newElement;
        }

        private static void InsertBackward(SparseVectorElement<T> previous, SparseVectorElement<T> current, int index, T value)
        {
            SparseVectorElement<T> newElement = new SparseVectorElement<T>
            {
                Index = index,
                Value = value,
                Previous = current,
                Next = previous
            };
            previous.Previous = newElement;
            if (current != null)
                current.Next = newElement;
        }

        public override string ToString()
        {
            if (_header == null)
                return "empty";
            // Full backward
            SparseVectorElement<T> ptr = _header;
            while (ptr.Previous != null)
                ptr = ptr.Previous;
            // Full forward and display
            StringBuilder sb = new StringBuilder();
            while (ptr != null)
            {
                sb.Append($"[{ptr.Index}->{ptr.Value}],");
                ptr = ptr.Next;
            }
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }
    }
}
