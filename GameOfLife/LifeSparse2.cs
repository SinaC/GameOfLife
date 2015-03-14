using System;
using System.Collections.Generic;
using System.Linq;

namespace GameOfLife
{
    public class LifeSparse2 : ILife
    {
        private SparseMatrix<CellSparse> _matrix;

        public Rule Rule { get; private set; }
        public Boundary Boundary { get; private set; }
        public int Generation { get; private set; }

        public LifeSparse2(Rule rule, Boundary boundary)
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

        public int Population
        {
            get { return _matrix.GetData().Count(); }
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

        public void NextGeneration()
        {
            SparseMatrix<CellSparse> newMatrix = new SparseMatrix<CellSparse>();

            // Count the number of set neighbours to each cell.
            // Count also the number of cell besides each empty cell. 
            // The empty cell gets set if it matches birth rule
            // (at least one neighbour set, so it's enough to check only clear cell)
            foreach (CellSparse cell in _matrix.GetData())
            {
                int neighbourCount = 0;

                for (int stepY = -1; stepY <= +1; stepY++)
                    for (int stepX = -1; stepX <= +1; stepX++)
                        if (stepX != 0 || stepY != 0)
                        {
                            int x, y;
                            bool isXValid = Boundary.AddStepX(cell.X, stepX, out x);
                            bool isYValid = Boundary.AddStepY(cell.Y, stepY, out y);
                            if (isXValid && isYValid)
                            {
                                CellSparse neighbour = _matrix[x, y];
                                if (neighbour != null) // neighbour exists
                                    neighbourCount++;
                                else // neighbour doesn't exist, count cell around
                                {
                                    int nearNeighbourCount = 0;
                                    for (int nearStepY = -1; nearStepY <= +1; nearStepY++)
                                        for (int nearStepX = -1; nearStepX <= +1; nearStepX++)
                                            if (stepX != 0 || stepY != 0)
                                            {
                                                int nearX, nearY;
                                                bool isNearXValid = Boundary.AddStepX(x, nearStepX, out nearX);
                                                bool isNearYValid = Boundary.AddStepY(y, nearStepY, out nearY);
                                                if (isNearXValid && isNearYValid)
                                                {
                                                    CellSparse nearNeighbour = _matrix[nearX, nearY];
                                                    if (nearNeighbour != null)
                                                        nearNeighbourCount++;
                                                }
                                            }
                                    if (Rule.Birth(nearNeighbourCount)) // birth
                                        newMatrix.SetAt(x, y, new CellSparse(x, y));
                                }
                            }
                        }
                if (Rule.Survive(neighbourCount)) // survival
                    newMatrix.SetAt(cell.X, cell.Y, cell);
            }

            _matrix = newMatrix; // swap matrix

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

        public void GetView(int minX, int minY, int maxX, int maxY, bool[,] view)
        {
            for (int y = minY; y <= maxY; y++)
                for (int x = minX; x <= maxX; x++)
                {
                    CellSparse value = _matrix[x, y];
                    view[x - minX, y - minY] = value != null;
                }
        }
    }
}
