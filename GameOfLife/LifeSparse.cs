using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GameOfLife
{
    internal class CellSparse
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Generation { get; private set; }

        public CellSparse(int x, int y)
        {
            X = x;
            Y = y;

            Generation = 0;
        }

        public void Survived()
        {
            Generation++;
        }

        public override string ToString()
        {
            return "*";
        }
    }

    public class LifeSparse : ILife
    {
        private readonly SparseMatrix<CellSparse> _matrix;

        public Rule Rule { get; private set; }
        public Boundary Boundary { get; private set; }
        public int Generation { get; private set; }

        public int Population
        {
            get { return _matrix.GetData().Count(); }
        }

        public LifeSparse(Rule rule, Boundary boundary)
        {
            if (rule == null)
                throw new ArgumentNullException("rule");
            if (boundary == null)
                throw new ArgumentNullException("boundary");

            Rule = rule;
            Generation = 0;
            Boundary = boundary;

            _matrix = new SparseMatrix<CellSparse>();
        }

        public void Reset()
        {
            Generation = 0;
            _matrix.Clear();
        }

        public void Set(int x, int y)
        {
            if (!Boundary.IsXValid(x) || !Boundary.IsYValid(x))
                return;
            if (_matrix[x, y] != null)
                return;
            CellSparse cell = new CellSparse(x, y);
            _matrix.SetAt(x, y, cell);
        }

        private class NeighbourInfo
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Count { get; set; }

            public override string ToString()
            {
                return Count.ToString(CultureInfo.InvariantCulture);
            }
        }

        public void NextGeneration()
        {
            //System.Diagnostics.Debug.WriteLine("NEXT");
            //System.Diagnostics.Debug.WriteLine(_matrix);

            // Compute neighbours
            //  foreach alive cell, add +1 to every cell in neighbourhood
            // TODO: optimize this, should not check every neighbour of every cell
            SparseMatrix<NeighbourInfo> neighboursInfo = new SparseMatrix<NeighbourInfo>();
            foreach (CellSparse cell in _matrix.GetData())
            {
                for (int stepY = -1; stepY <= +1; stepY++)
                    for (int stepX = -1; stepX <= +1; stepX++)
                    {
                        // Check boundaries
                        int x, y;
                        bool isXValid = Boundary.AddStepX(cell.X, stepX, out x);
                        bool isYValid = Boundary.AddStepY(cell.Y, stepY, out y);
                        if (isXValid && isYValid)
                        {
                            NeighbourInfo neighbourInfo = neighboursInfo[x, y];
                            bool ourself = stepX == 0 && stepY == 0; // when we're checking ourself, our neighbour value is 0
                            if (neighbourInfo == null)
                            {
                                neighboursInfo[x, y] = new NeighbourInfo
                                {
                                    X = x,
                                    Y = y,
                                    Count = ourself ? 0 : 1 // when we're checking ourself, our neighbour value is 0
                                };
                            }
                            else
                                neighbourInfo.Count += ourself ? 0 : 1; // when we're checking ourself, our neighbour value is 0
                        }
                        //System.Diagnostics.Debug.WriteLine("[{0},{1}] -> [{2},{3}] : {4}", cell.X, cell.Y, x, y, neighboursInfo[x, y].Count);
                    }
            }

            //System.Diagnostics.Debug.WriteLine(neighboursInfo);

            // Apply rules
            foreach (NeighbourInfo neighbour in neighboursInfo.GetData())
            {
                CellSparse cell = _matrix[neighbour.X, neighbour.Y];
                if (cell == null) // birth ?
                {
                    if (Rule.Birth(neighbour.Count))
                    {
                        CellSparse baby = new CellSparse(neighbour.X, neighbour.Y);
                        _matrix.SetAt(neighbour.X, neighbour.Y, baby);
                    }
                }
                else // survival or death ?
                {
                    if (Rule.Death(neighbour.Count))
                        _matrix.RemoveAt(neighbour.X, neighbour.Y);
                    else
                        cell.Survived();
                }
            }

            //
            Generation++;
        }

        public int Oldest
        {
            get { return _matrix.GetData().Max(c => c.Generation); }
        }

        public Tuple<int, int, int, int> GetMinMaxIndexes() // minx, miny, maxx, maxy
        {
            List<int> rowIndexes = _matrix.GetRowIndexes().ToList();
            int rowMin = rowIndexes.Min();
            int rowMax = rowIndexes.Max();

            List<int> columnIndexes = _matrix.GetColumnIndexes().ToList();
            int columnMin = columnIndexes.Min();
            int columnMax = columnIndexes.Max();

            return new Tuple<int, int, int, int>(rowMin, columnMin, rowMax, columnMax);
        }

        public bool[,] GetView(int minX, int minY, int maxX, int maxY)
        {
            bool[,] cells = new bool[maxX - minX + 1, maxY - minY + 1];
            for (int y = minY; y <= maxY; y++)
                for (int x = minX; x <= maxX; x++)
                {
                    CellSparse value = _matrix[x, y];
                    cells[x - minX, y - minY] = value != null;
                }
            return cells;
        }
    }
}
