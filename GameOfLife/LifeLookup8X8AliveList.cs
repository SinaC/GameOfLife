using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameOfLife
{
    public class LifeLookup8X8AliveList : ILife
    {
        internal class CellSparse8X8
        {
            public int X { get; private set; }
            public int Y { get; private set; }

            public ulong Data { get; set; }

            public CellSparse8X8(int x, int y)
            {
                X = x;
                Y = y;
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                for (int y = 0; y < 8; y++)
                {
                    StringBuilder line = new StringBuilder();
                    for (int x = 0; x < 8; x++)
                    {
                        int shift = Shift[x + y * 8];
                        ulong v = (Data >> shift) & 1;
                        line.Append(v);
                    }
                    sb.Append(line);
                }
                return sb.ToString();
            }
        }

        private static readonly int[] Shift = // shift to apply to get bit on position x,y on 8x8 data
            {
                00, 01, 04, 05, 16, 17, 20, 21,
                02, 03, 06, 07, 18, 19, 22, 23,
                08, 09, 12, 13, 24, 25, 28, 29,
                10, 11, 14, 15, 26, 27, 30, 31,
                32, 33, 36, 37, 48, 49, 52, 53,
                34, 35, 38, 39, 50, 51, 54, 55,
                40, 41, 44, 45, 56, 57, 60, 61,
                42, 43, 46, 47, 58, 59, 62, 63
            };

        private readonly SparseMatrix<CellSparse8X8> _alive; // store cell with at least one bit set and neighbour of alive cell

        private readonly NeighbourLookupUnnaturalOrder _lookup;

        public int Generation { get; private set; }
        public int Population { get; private set; }
        public Rule Rule { get; private set; }
        public Boundary Boundary { get; private set; }

        public LifeLookup8X8AliveList(Rule rule, Boundary boundary)
        {
            Rule = rule;
            Boundary = boundary;

            _lookup = new NeighbourLookupUnnaturalOrder(rule);
            _alive = new SparseMatrix<CellSparse8X8>();

            Population = 0;
        }

        public void Reset()
        {
            _alive.Clear();
            Generation = 0;
        }

        public void Set(int x, int y)
        {
            // Fill (create if needed) cell
            int cellX = x%8;
            int cellY = y%8;
            int shift = Shift[cellX + cellY*8];
            int gridX = x/8;
            int gridY = y/8;
            CellSparse8X8 cell = _alive[gridX, gridY] ?? SpawnCell(gridX, gridY); // get/create cell
            cell.Data |= ((ulong) 1 << shift);

            // Create neighbours if needed
            AddNeighbours(x, y);

            //Dump8X8(cell.Data);
            System.Diagnostics.Debug.WriteLine(_alive);
        }

        public void NextGeneration()
        {
            // TODO
        }

        public void GetView(int minX, int minY, int maxX, int maxY, bool[,] view)
        {
            // TODO: from minX/8 + minX%8 to ...
            for (int y = minY; y <= maxY; y++)
                for (int x = minX; x <= maxX; x++)
                {
                    CellSparse8X8 value = _alive[x, y];
                    view[x - minX, y - minY] = value.Data != 0;
                }
        }

        public Tuple<int, int, int, int> GetMinMaxIndexes()
        {
            List<CellSparse8X8> cells = _alive.GetData().ToList();

            int rowMin = cells.Select(c => c.X).Min();
            int rowMax = cells.Select(c => c.X).Max();
            int columnMin = cells.Select(c => c.Y).Min();
            int columnMax = cells.Select(c => c.Y).Max();

            return new Tuple<int, int, int, int>(rowMin, columnMin, rowMax, columnMax);
        }

        private static void Dump8X8(ulong value, string msg = null)
        {
            System.Diagnostics.Debug.WriteLine(value + " " + (msg ?? String.Empty));
            for (int y = 0; y < 8; y++)
            {
                StringBuilder sb = new StringBuilder();
                for (int x = 0; x < 8; x++)
                {
                    int shift = Shift[x + y * 8];
                    ulong v = (value >> shift) & 1;
                    sb.Append(v);
                }
                System.Diagnostics.Debug.WriteLine(sb.ToString());
            }
        }

        private CellSparse8X8 SpawnCell(int x, int y)
        {
            CellSparse8X8 cell = new CellSparse8X8(x, y);
            _alive.SetAt(x, y, cell);
            return cell;
        }

        private void AddNeighbours(int x, int y)
        {
            int topIndex, bottomIndex, leftIndex, rightIndex;
            Boundary.AddStepX(x, -1, out leftIndex);
            Boundary.AddStepX(x, 1, out rightIndex);
            Boundary.AddStepY(y, -1, out topIndex);
            Boundary.AddStepY(y, 1, out bottomIndex);

            CellSparse8X8 topLeft = _alive[leftIndex, topIndex] ?? SpawnCell(leftIndex, topIndex);
            CellSparse8X8 top = _alive[x, topIndex] ?? SpawnCell(x, topIndex);
            CellSparse8X8 topRight = _alive[rightIndex, topIndex] ?? SpawnCell(rightIndex, topIndex);
            CellSparse8X8 left = _alive[leftIndex, y] ?? SpawnCell(leftIndex, y);
            CellSparse8X8 right = _alive[rightIndex, y] ?? SpawnCell(rightIndex, y);
            CellSparse8X8 bottomLeft = _alive[leftIndex, bottomIndex] ?? SpawnCell(leftIndex, bottomIndex);
            CellSparse8X8 bottom = _alive[x, bottomIndex] ?? SpawnCell(x, bottomIndex);
            CellSparse8X8 bottomRight = _alive[rightIndex, bottomIndex] ?? SpawnCell(rightIndex, bottomIndex);
        }
    }
}
