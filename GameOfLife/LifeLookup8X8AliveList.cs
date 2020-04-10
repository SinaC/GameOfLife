using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace GameOfLife
{
    // TODO: 
    //  instead of storing a simple 64 bit in sparse matrix, store a super cell containing 4 or 16 cell of 64 bits
    //  instead of copying NewData into CurrentData, use Data1 and Data2 and algorithm switch between 1 and 2
    //  no need to use a SparseMatrix to store newly created cells
    //  store neighbor left/top/right/bottom index in cell (computed when creating cell)

    // 63  42 43 46 47 58 59 62 63  42
    //     -----------------------
    // 21 |00 01|04 05|16 17|20 21| 00
    // 23 |02 03|06 07|18 19|22 23| 02
    //     ----- ----- ----- -----
    // 29 |08 09|12 13|24 25|28 29| 08
    // 31 |10 11|14 15|26 27|30 31| 10
    //     ----- ----- ----- -----
    // 53 |32 33|36 37|48 49|52 53| 32
    // 55 |34 35|38 39|50 51|54 55| 34
    //     ----- ----- ---- ------
    // 61 |40 41|44 45|56 57|60 61| 40
    // 63 |42 43|46 47|58 59|62 63| 42
    //     -----------------------
    // 21  00 01 04 05 16 17 20 21  00
    public class LifeLookup8X8AliveList : ILife, IAliveList
    {
        internal class CellSparse8X8
        {
            public static readonly CellSparse8X8 EmptyCell = new CellSparse8X8(0, 0);

            public int X { get; }
            public int Y { get; }

            public ulong CurrentData { get; set; }
            public ulong NewData { get; set; } // temporary, once every alive cell has been handled, this value is copied in CurrentData

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
                        int shift = Shift[x + y*8];
                        ulong v = (CurrentData >> shift) & 1;
                        line.Append(v);
                    }
                    sb.AppendLine(line.ToString());
                }
                return sb.ToString();
            }
        }

        //00 01|04 05|16 17|20 21
        //02                   23
        //--                   --
        //08                   29
        //10                   31
        //--                   --
        //32                   53
        //34                   55
        //--                   --
        //40                   61
        //42 43|46 47|58 59|62 63

        private const ulong BottomRightMask = ((ulong)1 << 63);
        private const ulong BottomMask = ((ulong)1 << 42) | ((ulong)1 << 43) | ((ulong)1 << 46) | ((ulong)1 << 47) | ((ulong)1 << 58) | ((ulong)1 << 59) | ((ulong)1 << 62) | ((ulong)1 << 63);
        private const ulong BottomLeftMask = ((ulong) 1 << 42);
        private const ulong RightMask = ((ulong) 1 << 21) | ((ulong) 1 << 23) | ((ulong) 1 << 29) | ((ulong) 1 << 31) | ((ulong) 1 << 53) | ((ulong) 1 << 55) | ((ulong) 1 << 61) | ((ulong) 1 << 63);
        private const ulong LeftMask = ((ulong) 1 << 0) | ((ulong) 1 << 2) | ((ulong) 1 << 8) | ((ulong) 1 << 10) | ((ulong) 1 << 32) | ((ulong) 1 << 34) | ((ulong) 1 << 40) | ((ulong) 1 << 42);
        private const ulong TopRightMask = ((ulong) 1 << 21);
        private const ulong TopMask = ((ulong) 1 << 0) | ((ulong) 1 << 1) | ((ulong) 1 << 4) | ((ulong) 1 << 5) | ((ulong) 1 << 16) | ((ulong) 1 << 17) | ((ulong) 1 << 20) | ((ulong) 1 << 21);
        private const ulong TopLeftMask = ((ulong) 1 << 0);

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

        private readonly SparseMatrix<CellSparse8X8> _alive; // store cell with at least one bit set and neighbor of alive cell

        private readonly NeighbourLookupUnnaturalOrder _lookup;

        public int AliveCount => _alive.GetData().Count();

        public int Generation { get; private set; }
        public Rule Rule { get; }
        public Boundary Boundary { get; }

        public int Population
        {
            get
            {
                ulong count = 0;
                foreach (CellSparse8X8 cell in _alive.GetData().Where(c => c.CurrentData != 0))
                {
                    // count bit (naive method)
                    // could use http://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetNaive 
                    // or http://stackoverflow.com/questions/2709430/count-number-of-bits-in-a-64-bit-ulong-big-integer
                    for (int bit = 0; bit < 64; bit++)
                        count += (cell.CurrentData >> bit) & 1;
                }
                return (int) count;
            }
        }

        public LifeLookup8X8AliveList(Rule rule, Boundary boundary)
        {
            Rule = rule;
            Boundary = boundary;

            _lookup = new NeighbourLookupUnnaturalOrder(rule);
            _alive = new SparseMatrix<CellSparse8X8>();

            Generation = 0;
        }

        public void Reset()
        {
            _alive.Clear();
            Generation = 0;
        }

        public void Set(int x, int y)
        {
            // 1 -> 1
            // 7 -> 7
            // 15 -> 7
            // -1 -> 7
            // -8 -> 0
            // -9 -> 7
            // -16 -> 0
            // Fill (create if needed) cell
            int cellX = (8 + (x%8))%8; //
            int cellY = (8 + (y%8))%8;
            int shift = Shift[cellX + cellY*8];
            int gridX = x/8;
            int gridY = y/8;
            CellSparse8X8 cell = _alive[gridX, gridY] ?? SpawnCell(gridX, gridY, _alive); // get/create cell
            cell.CurrentData |= ((ulong) 1 << shift);

            Debug.WriteLine("SET:{0},{1}", x, y);

            // Create neighbors if needed
            int topIndex, bottomIndex, leftIndex, rightIndex;
            Boundary.AddStepX(gridX, -1, out leftIndex);
            Boundary.AddStepX(gridX, 1, out rightIndex);
            Boundary.AddStepY(gridY, -1, out topIndex);
            Boundary.AddStepY(gridY, 1, out bottomIndex);
            SpawnNeighbours(gridX, gridY, cell.CurrentData, topIndex, bottomIndex, leftIndex, rightIndex, _alive);

            //Dump8X8(cell.Data);
            //System.Diagnostics.Debug.WriteLine(_alive);
        }

        public void NextGeneration()
        {
            Debug.WriteLine("NextGeneration Cells: {0} Generation: {1}", _alive.GetData().Count(), Generation);

            SparseMatrix<CellSparse8X8> newCells = new SparseMatrix<CellSparse8X8>(); // TODO: is SparseMatrix needed ?
            foreach (CellSparse8X8 cell in _alive.GetData())
            {
                //Debug.WriteLine("Handling cell {0},{1}", cell.X, cell.Y);
                //Dump8X8(cell.CurrentData);

                if (cell.X == 5 && cell.Y == 0 && Generation >= 23)
                {
                }

                int x = cell.X;
                int y = cell.Y;
                int topIndex, bottomIndex, leftIndex, rightIndex;
                bool leftValid = Boundary.AddStepX(x, -1, out leftIndex);
                bool rightValid = Boundary.AddStepX(x, 1, out rightIndex);
                bool topValid = Boundary.AddStepY(y, -1, out topIndex);
                bool bottomValid = Boundary.AddStepY(y, 1, out bottomIndex);

                ulong topLeft = (leftValid && topValid ? (_alive[leftIndex, topIndex] ?? CellSparse8X8.EmptyCell) : CellSparse8X8.EmptyCell).CurrentData;
                ulong top = (topValid ? (_alive[x, topIndex] ?? CellSparse8X8.EmptyCell) : CellSparse8X8.EmptyCell).CurrentData;
                ulong topRight = (rightValid && topValid ? (_alive[rightIndex, topIndex] ?? CellSparse8X8.EmptyCell) : CellSparse8X8.EmptyCell).CurrentData;
                ulong left = (leftValid ? (_alive[leftIndex, y] ?? CellSparse8X8.EmptyCell) : CellSparse8X8.EmptyCell).CurrentData;
                ulong right = (rightValid ? (_alive[rightIndex, y] ?? CellSparse8X8.EmptyCell) : CellSparse8X8.EmptyCell).CurrentData;
                ulong bottomLeft = (leftValid && bottomValid ? (_alive[leftIndex, bottomIndex] ?? CellSparse8X8.EmptyCell) : CellSparse8X8.EmptyCell).CurrentData;
                ulong bottom = (bottomValid ? (_alive[x, bottomIndex] ?? CellSparse8X8.EmptyCell) : CellSparse8X8.EmptyCell).CurrentData;
                ulong bottomRight = (bottomValid && rightValid ? (_alive[rightIndex, bottomIndex] ?? CellSparse8X8.EmptyCell) : CellSparse8X8.EmptyCell).CurrentData;

                ulong newValue = ComputeNewValue(cell.CurrentData, topLeft, top, topRight, left, right, bottomLeft, bottom, bottomRight);

                //Dump8X8(newValue);

                if (newValue != 0) // no death -> add neighbors in new cell list
                    SpawnNeighbours(x, y, newValue, topIndex, bottomIndex, leftIndex, rightIndex, newCells);

                cell.NewData = newValue; // Save new value, it will be copied in current data once every cells have been handled
            }
            // Copy new value to current value + list dead cells
            List<CellSparse8X8> toRemove = new List<CellSparse8X8>();
            foreach (CellSparse8X8 cell in _alive.GetData())
            {
                cell.CurrentData = cell.NewData;
                // if newValue == 0 and nothing in neighborhood, remove cell
                if (cell.CurrentData == 0 && !HasNeighbours(cell))
                    toRemove.Add(cell);
            }

            // Remove dead cells
            foreach (CellSparse8X8 cell in toRemove)
            {
                Debug.WriteLine("INCINERATE {0},{1}", cell.X, cell.Y);
                _alive.RemoveAt(cell.X, cell.Y);
            }

            // Add cell from new list to alive list
            foreach (CellSparse8X8 newCell in newCells.GetData())
                if (_alive[newCell.X, newCell.Y] == null)
                    _alive.SetAt(newCell.X, newCell.Y, newCell);
            //
            Generation++;
        }

        public void GetView(int minX, int minY, int maxX, int maxY, bool[,] view)
        {
            // doesn't use parameters
            //foreach (CellSparse8X8 cell in _alive.GetData().Where(c => c.CurrentData != 0))
            //{
            //    for (int yi = 0; yi < 8; yi++)
            //        for (int xi = 0; xi < 8; xi++)
            //        {
            //            int shift = Shift[xi + yi * 8];

            //            int x = cell.X*8 + xi - minX;
            //            int y = cell.Y*8 + yi - minY;
            //            view[x,y] = ((cell.CurrentData >> shift) & 1) != 0;
            //        }
            //}
            foreach (CellSparse8X8 cell in _alive.GetData().Where(c => c.CurrentData != 0))
            {
                for (int yi = 0; yi < 8; yi++)
                    for (int xi = 0; xi < 8; xi++)
                    {
                        int shift = Shift[xi + yi*8];

                        int x = cell.X*8 + xi;
                        int y = cell.Y*8 + yi;
                        if (x >= minX && x <= maxX && y >= minY && y <= maxY)
                            view[x - minX, y - minY] = ((cell.CurrentData >> shift) & 1) != 0;
                    }
            }
        }

        public Tuple<int, int, int, int> GetMinMaxIndexes()
        {
            if (_alive.GetData().All(c => c.CurrentData == 0))
                return null;

            CellSparse8X8 minXCell = _alive.GetData().Where(c => c.CurrentData != 0).OrderBy(c => c.X).First();
            CellSparse8X8 minYCell = _alive.GetData().Where(c => c.CurrentData != 0).OrderBy(c => c.Y).First();
            CellSparse8X8 maxXCell = _alive.GetData().Where(c => c.CurrentData != 0).OrderByDescending(c => c.X).First();
            CellSparse8X8 maxYCell = _alive.GetData().Where(c => c.CurrentData != 0).OrderByDescending(c => c.Y).First();

            // stored X is grid x (real x / 8) and not real X, in a cell x goes from 0 to 7 -> *8 + 7. Same goes for y
            return new Tuple<int, int, int, int>(minXCell.X*8, minYCell.Y*8, maxXCell.X*8 + 7, maxYCell.Y*8 + 7);
        }

        private bool HasNeighbours(CellSparse8X8 cell)
        {
            int x = cell.X;
            int y = cell.Y;
            int topIndex, bottomIndex, leftIndex, rightIndex;
            bool leftValid = Boundary.AddStepX(x, -1, out leftIndex);
            bool rightValid = Boundary.AddStepX(x, 1, out rightIndex);
            bool topValid = Boundary.AddStepY(y, -1, out topIndex);
            bool bottomValid = Boundary.AddStepY(y, 1, out bottomIndex);

            ulong topLeft = (leftValid && topValid ? (_alive[leftIndex, topIndex] ?? CellSparse8X8.EmptyCell) : CellSparse8X8.EmptyCell).CurrentData;
            ulong top = (topValid ? (_alive[x, topIndex] ?? CellSparse8X8.EmptyCell) : CellSparse8X8.EmptyCell).CurrentData;
            ulong topRight = (rightValid && topValid ? (_alive[rightIndex, topIndex] ?? CellSparse8X8.EmptyCell) : CellSparse8X8.EmptyCell).CurrentData;
            ulong left = (leftValid ? (_alive[leftIndex, y] ?? CellSparse8X8.EmptyCell) : CellSparse8X8.EmptyCell).CurrentData;
            ulong right = (rightValid ? (_alive[rightIndex, y] ?? CellSparse8X8.EmptyCell) : CellSparse8X8.EmptyCell).CurrentData;
            ulong bottomLeft = (leftValid && bottomValid ? (_alive[leftIndex, bottomIndex] ?? CellSparse8X8.EmptyCell) : CellSparse8X8.EmptyCell).CurrentData;
            ulong bottom = (bottomValid ? (_alive[x, bottomIndex] ?? CellSparse8X8.EmptyCell) : CellSparse8X8.EmptyCell).CurrentData;
            ulong bottomRight = (bottomValid && rightValid ? (_alive[rightIndex, bottomIndex] ?? CellSparse8X8.EmptyCell) : CellSparse8X8.EmptyCell).CurrentData;

            // every neighbor is empty
            if (topLeft == 0 && top == 0 && topRight == 0 && left == 0 && right == 0 && bottomLeft == 0 && bottom == 0 && bottomRight == 0)
                return false;
            return true;
            //// check neighbor borders   THIS IS WRONG with cell on position 63 + spacefiller on generation 25 at 47,7
            //if ((topLeft & BottomRightMask) != 0 || (top & BottomMask) != 0 || (topRight & BottomLeftMask) != 0 || (left & RightMask) != 0 || (right & LeftMask) != 0 || (bottomLeft & TopRightMask) != 0 || (bottom & TopMask) != 0 || (bottomRight & TopLeftMask) != 0)
            //    return true;
            //return false;
        }

        private ulong ComputeNewValue(ulong value, ulong topLeft, ulong top, ulong topRight, ulong left, ulong right, ulong bottomLeft, ulong bottom, ulong bottomRight)
        {
            // 63  42 43 46 47 58 59 62 63  42
            //     -----------------------
            // 21 |00 01|04 05|16 17|20 21| 00
            // 23 |02 03|06 07|18 19|22 23| 02
            //     ----- ----- ----- -----
            // 29 |08 09|12 13|24 25|28 29| 08
            // 31 |10 11|14 15|26 27|30 31| 10
            //     ----- ----- ----- -----
            // 53 |32 33|36 37|48 49|52 53| 32
            // 55 |34 35|38 39|50 51|54 55| 34
            //     ----- ----- ---- ------
            // 61 |40 41|44 45|56 57|60 61| 40
            // 63 |42 43|46 47|58 59|62 63| 42
            //     -----------------------
            // 21  00 01 04 05 16 17 20 21  00

            // lookup is built using following order (16 bits)
            //  2x2 on 4 bits | top neighbors on 4 bits | LT on 1 bit | RT on 1 bit | LB on 1 bit | RB on 1 bit | bottom neighbors on 4 bits

            // TODO: test each sub-4x4 block, if block is 0 and no neighbors -> no need to compute value

            // Sub-4x4 block order
            // 00 10 20 30
            // 01 11 21 31
            // 02 12 22 32
            // 03 13 23 33

            // Sub-4x4 block 00 -> 15
            ulong block00 = (value & 0xF) // 2x2  00, 01, 02, 03
                            | ((topLeft >> 63) & 1) << 4 | ((top >> 42) & 1) << 5 | ((top >> 43) & 1) << 6 | ((top >> 46) & 1) << 7 // top
                            | ((left >> 21) & 1) << 8 // LT
                            | ((value >> 4) & 1) << 9 // RT
                            | ((left >> 23) & 1) << 10 // LB
                            | ((value >> 6) & 1) << 11 // RB
                            | ((left >> 29) & 1) << 12 | ((value >> 8) & 1) << 13 | ((value >> 9) & 1) << 14 | ((value >> 12) & 1) << 15; // bottom
            ulong block10 = ((value >> 4) & 0xF) // 2x2  04, 05, 06, 07
                            | ((top >> 43) & 1) << 4 | ((top >> 46) & 1) << 5 | ((top >> 47) & 1) << 6 | ((top >> 58) & 1) << 7 // top
                            | ((value >> 1) & 1) << 8 // LT
                            | ((value >> 16) & 1) << 9 // RT
                            | ((value >> 3) & 1) << 10 // LB
                            | ((value >> 18) & 1) << 11 // RB
                            | ((value >> 9) & 1) << 12 | ((value >> 12) & 1) << 13 | ((value >> 13) & 1) << 14 | ((value >> 24) & 1) << 15; // bottom
            ulong block01 = ((value >> 8) & 0xF) // 2x2  08, 09, 10, 11
                            | ((left >> 23) & 1) << 4 | ((value >> 2) & 1) << 5 | ((value >> 3) & 1) << 6 | ((value >> 6) & 1) << 7 // top
                            | ((left >> 29) & 1) << 8 // LT
                            | ((value >> 12) & 1) << 9 // RT
                            | ((left >> 31) & 1) << 10 // LB
                            | ((value >> 14) & 1) << 11 // RB
                            | ((left >> 53) & 1) << 12 | ((value >> 32) & 1) << 13 | ((value >> 33) & 1) << 14 | ((value >> 36) & 1) << 15; // bottom
            ulong block11 = ((value >> 12) & 0xF) // 2x2  12, 13, 14, 15
                            | ((value >> 3) & 1) << 4 | ((value >> 6) & 1) << 5 | ((value >> 7) & 1) << 6 | ((value >> 18) & 1) << 7 // top
                            | ((value >> 9) & 1) << 8 // LT
                            | ((value >> 24) & 1) << 9 // RT
                            | ((value >> 11) & 1) << 10 // LB
                            | ((value >> 26) & 1) << 11 // RB
                            | ((value >> 33) & 1) << 12 | ((value >> 36) & 1) << 13 | ((value >> 37) & 1) << 14 | ((value >> 48) & 1) << 15; // bottom
            // Sub-4x4 block 16 -> 31
            ulong block20 = ((value >> 16) & 0xF) // 2x2  16, 17, 18, 19
                            | ((top >> 47) & 1) << 4 | ((top >> 58) & 1) << 5 | ((top >> 59) & 1) << 6 | ((top >> 62) & 1) << 7 // top
                            | ((value >> 5) & 1) << 8 // LT
                            | ((value >> 20) & 1) << 9 // RT
                            | ((value >> 7) & 1) << 10 // LB
                            | ((value >> 22) & 1) << 11 // RB
                            | ((value >> 13) & 1) << 12 | ((value >> 24) & 1) << 13 | ((value >> 25) & 1) << 14 | ((value >> 28) & 1) << 15; // bottom
            ulong block30 = ((value >> 20) & 0xF) // 2x2  20, 21, 22, 23
                            | ((top >> 59) & 1) << 4 | ((top >> 62) & 1) << 5 | ((top >> 63) & 1) << 6 | ((topRight >> 42) & 1) << 7 // top
                            | ((value >> 17) & 1) << 8 // LT
                            | ((right >> 0) & 1) << 9 // RT
                            | ((value >> 19) & 1) << 10 // LB
                            | ((right >> 2) & 1) << 11 // RB
                            | ((value >> 25) & 1) << 12 | ((value >> 28) & 1) << 13 | ((value >> 29) & 1) << 14 | ((right >> 8) & 1) << 15; // bottom
            ulong block21 = ((value >> 24) & 0xF) // 2x2  24, 25, 26, 27
                            | ((value >> 7) & 1) << 4 | ((value >> 18) & 1) << 5 | ((value >> 19) & 1) << 6 | ((value >> 22) & 1) << 7 // top
                            | ((value >> 13) & 1) << 8 // LT
                            | ((value >> 28) & 1) << 9 // RT
                            | ((value >> 15) & 1) << 10 // LB
                            | ((value >> 30) & 1) << 11 // RB
                            | ((value >> 37) & 1) << 12 | ((value >> 48) & 1) << 13 | ((value >> 49) & 1) << 14 | ((value >> 52) & 1) << 15; // bottom
            ulong block31 = ((value >> 28) & 0xF) // 2x2  28, 29, 30, 31
                            | ((value >> 19) & 1) << 4 | ((value >> 22) & 1) << 5 | ((value >> 23) & 1) << 6 | ((right >> 02) & 1) << 7 // top
                            | ((value >> 25) & 1) << 8 // LT
                            | ((right >> 8) & 1) << 9 // RT
                            | ((value >> 27) & 1) << 10 // LB
                            | ((right >> 10) & 1) << 11 // RB
                            | ((value >> 49) & 1) << 12 | ((value >> 52) & 1) << 13 | ((value >> 53) & 1) << 14 | ((right >> 32) & 1) << 15; // bottom
            // Sub-4x4 block 32 -> 47
            ulong block02 = ((value >> 32) & 0xF) // 2x2  32, 33, 34, 35
                            | ((left >> 31) & 1) << 4 | ((value >> 10) & 1) << 5 | ((value >> 11) & 1) << 6 | ((value >> 14) & 1) << 7 // top
                            | ((left >> 53) & 1) << 8 // LT
                            | ((value >> 36) & 1) << 9 // RT
                            | ((left >> 55) & 1) << 10 // LB
                            | ((value >> 38) & 1) << 11 // RB
                            | ((left >> 61) & 1) << 12 | ((value >> 40) & 1) << 13 | ((value >> 41) & 1) << 14 | ((value >> 44) & 1) << 15; // bottom
            ulong block12 = ((value >> 36) & 0xF) // 2x2  36, 37, 38, 39
                            | ((value >> 11) & 1) << 4 | ((value >> 14) & 1) << 5 | ((value >> 15) & 1) << 6 | ((value >> 26) & 1) << 7 // top
                            | ((value >> 33) & 1) << 8 // LT
                            | ((value >> 48) & 1) << 9 // RT
                            | ((value >> 35) & 1) << 10 // LB
                            | ((value >> 50) & 1) << 11 // RB
                            | ((value >> 41) & 1) << 12 | ((value >> 44) & 1) << 13 | ((value >> 45) & 1) << 14 | ((value >> 56) & 1) << 15; // bottom
            ulong block03 = ((value >> 40) & 0xF) // 2x2  40, 41, 42, 43
                            | ((left >> 55) & 1) << 4 | ((value >> 34) & 1) << 5 | ((value >> 35) & 1) << 6 | ((value >> 38) & 1) << 7 // top
                            | ((left >> 61) & 1) << 8 // LT
                            | ((value >> 44) & 1) << 9 // RT
                            | ((left >> 63) & 1) << 10 // LB
                            | ((value >> 46) & 1) << 11 // RB
                            | ((bottomLeft >> 21) & 1) << 12 | ((bottom >> 0) & 1) << 13 | ((bottom >> 1) & 1) << 14 | ((bottom >> 4) & 1) << 15; // bottom
            ulong block13 = ((value >> 44) & 0xF) // 2x2  44, 45, 46, 47
                            | ((value >> 35) & 1) << 4 | ((value >> 38) & 1) << 5 | ((value >> 39) & 1) << 6 | ((value >> 50) & 1) << 7 // top
                            | ((value >> 41) & 1) << 8 // LT
                            | ((value >> 56) & 1) << 9 // RT
                            | ((value >> 43) & 1) << 10 // LB
                            | ((value >> 58) & 1) << 11 // RB
                            | ((bottom >> 1) & 1) << 12 | ((bottom >> 4) & 1) << 13 | ((bottom >> 5) & 1) << 14 | ((bottom >> 16) & 1) << 15; // bottom
            // Sub-4x4 block 48 -> 63
            ulong block22 = ((value >> 48) & 0xF) // 2x2  48, 49, 50, 51
                            | ((value >> 15) & 1) << 4 | ((value >> 26) & 1) << 5 | ((value >> 27) & 1) << 6 | ((value >> 30) & 1) << 7 // top
                            | ((value >> 37) & 1) << 8 // LT
                            | ((value >> 52) & 1) << 9 // RT
                            | ((value >> 39) & 1) << 10 // LB
                            | ((value >> 54) & 1) << 11 // RB
                            | ((value >> 45) & 1) << 12 | ((value >> 56) & 1) << 13 | ((value >> 57) & 1) << 14 | ((value >> 60) & 1) << 15; // bottom
            ulong block32 = ((value >> 52) & 0xF) // 2x2  52, 53, 54, 55
                            | ((value >> 27) & 1) << 4 | ((value >> 30) & 1) << 5 | ((value >> 31) & 1) << 6 | ((right >> 10) & 1) << 7 // top
                            | ((value >> 49) & 1) << 8 // LT
                            | ((right >> 32) & 1) << 9 // RT
                            | ((value >> 51) & 1) << 10 // LB
                            | ((right >> 34) & 1) << 11 // RB
                            | ((value >> 57) & 1) << 12 | ((value >> 60) & 1) << 13 | ((value >> 61) & 1) << 14 | ((right >> 40) & 1) << 15; // bottom
            ulong block23 = ((value >> 56) & 0xF) // 2x2  56, 57, 58, 59
                            | ((value >> 39) & 1) << 4 | ((value >> 50) & 1) << 5 | ((value >> 51) & 1) << 6 | ((value >> 54) & 1) << 7 // top
                            | ((value >> 45) & 1) << 8 // LT
                            | ((value >> 60) & 1) << 9 // RT
                            | ((value >> 47) & 1) << 10 // LB
                            | ((value >> 62) & 1) << 11 // RB
                            | ((bottom >> 5) & 1) << 12 | ((bottom >> 16) & 1) << 13 | ((bottom >> 17) & 1) << 14 | ((bottom >> 20) & 1) << 15; // bottom
            ulong block33 = ((value >> 60) & 0xF) // 2x2  60, 61, 62, 63
                            | ((value >> 51) & 1) << 4 | ((value >> 54) & 1) << 5 | ((value >> 55) & 1) << 6 | ((right >> 34) & 1) << 7 // top
                            | ((value >> 57) & 1) << 8 // LT
                            | ((right >> 40) & 1) << 9 // RT
                            | ((value >> 59) & 1) << 10 // LB
                            | ((right >> 42) & 1) << 11 // RB
                            | ((bottom >> 17) & 1) << 12 | ((bottom >> 20) & 1) << 13 | ((bottom >> 21) & 1) << 14 | ((bottomRight >> 0) & 1) << 15; // bottom

            // Apply rules on each Sub-4x4
            ulong newBlock00 = _lookup[block00];
            ulong newBlock10 = _lookup[block10];
            ulong newBlock01 = _lookup[block01];
            ulong newBlock11 = _lookup[block11];

            ulong newBlock20 = _lookup[block20];
            ulong newBlock30 = _lookup[block30];
            ulong newBlock21 = _lookup[block21];
            ulong newBlock31 = _lookup[block31];

            ulong newBlock02 = _lookup[block02];
            ulong newBlock12 = _lookup[block12];
            ulong newBlock03 = _lookup[block03];
            ulong newBlock13 = _lookup[block13];

            ulong newBlock22 = _lookup[block22];
            ulong newBlock32 = _lookup[block32];
            ulong newBlock23 = _lookup[block23];
            ulong newBlock33 = _lookup[block33];

            // Rearrange Sub-2x2 of each Sub-4x4 (keep 4 first bits of each new block value and shift)
            ulong newValue = ((newBlock00 & 0xF) << 0) | ((newBlock10 & 0xF) << 4) | ((newBlock01 & 0xF) << 8) | ((newBlock11 & 0xF) << 12) |
                             ((newBlock20 & 0xF) << 16) | ((newBlock30 & 0xF) << 20) | ((newBlock21 & 0xF) << 24) | ((newBlock31 & 0xF) << 28) |
                             ((newBlock02 & 0xF) << 32) | ((newBlock12 & 0xF) << 36) | ((newBlock03 & 0xF) << 40) | ((newBlock13 & 0xF) << 44) |
                             ((newBlock22 & 0xF) << 48) | ((newBlock32 & 0xF) << 52) | ((newBlock23 & 0xF) << 56) | ((newBlock33 & 0xF) << 60);

            //Dump8X8(value, String.Format("cell: {0}", i));
            //System.Diagnostics.Debug.WriteLine("row 0");
            ////
            //Dump4X4(block00, "old 00");
            //Dump4X4(newBlock00, "new 00");
            //Dump4X4(block10, "old 10");
            //Dump4X4(newBlock10, "new 10");
            //Dump4X4(block20, "old 20");
            //Dump4X4(newBlock20, "new 20");
            //Dump4X4(block30, "old 30");
            //Dump4X4(newBlock30, "new 30");
            //System.Diagnostics.Debug.WriteLine("row 1");
            //Dump4X4(block01, "old 01");
            //Dump4X4(newBlock01, "new 01");
            //Dump4X4(block11, "old 11");
            //Dump4X4(newBlock11, "new 11");
            //Dump4X4(block21, "old 21");
            //Dump4X4(newBlock21, "new 21");
            //Dump4X4(block31, "old 31");
            //Dump4X4(newBlock31, "new 31");
            //System.Diagnostics.Debug.WriteLine("row 2");
            //Dump4X4(block02, "old 02");
            //Dump4X4(newBlock02, "new 02");
            //Dump4X4(block12, "old 12");
            //Dump4X4(newBlock12, "new 12");
            //Dump4X4(block22, "old 22");
            //Dump4X4(newBlock22, "new 22");
            //Dump4X4(block32, "old 32");
            //Dump4X4(newBlock32, "new 32");
            //System.Diagnostics.Debug.WriteLine("row 3");
            //Dump4X4(block03, "old 03");
            //Dump4X4(newBlock03, "new 03");
            //Dump4X4(block13, "old 13");
            //Dump4X4(newBlock13, "new 13");
            //Dump4X4(block23, "old 23");
            //Dump4X4(newBlock23, "new 23");
            //Dump4X4(block33, "old 33");
            //Dump4X4(newBlock33, "new 33");
            ////
            //Dump8X8(newValue, "newValue");

            return newValue;
        }

        private CellSparse8X8 SpawnCell(int x, int y, SparseMatrix<CellSparse8X8> cells)
        {
            Debug.WriteLine("SPAWN:{0} {1}", x, y);

            CellSparse8X8 cell = new CellSparse8X8(x, y);
            cells.SetAt(x, y, cell);
            return cell;
        }

        // Create neighbors if needed (cell has a bit set on border and neighbor cell doesn't exist yet)
        private void SpawnNeighbours(int x, int y, ulong value, int topIndex, int bottomIndex, int leftIndex, int rightIndex, SparseMatrix<CellSparse8X8> cells)
        {
            //Dump8X8(value, String.Format("AddNeighbors {0},{1}", x, y));

            // check cell borders
            ulong topLeftCorner = value & TopLeftMask;
            ulong topBorder = value & TopMask;
            ulong topRightCorner = value & TopRightMask;
            ulong leftBorder = value & LeftMask;
            ulong rightBorder = value & RightMask;
            ulong bottomLeftCorner = value & BottomLeftMask;
            ulong bottomBorder = value & BottomMask;
            ulong bottomRightCorner = value & BottomRightMask;

            if (topBorder == 0 && leftBorder == 0 && rightBorder == 0 && bottomBorder == 0)
                return; // nothing to do

            // create neighbor cell if cell on borders
            if (topLeftCorner != 0 && _alive[leftIndex, topIndex] == null)
                SpawnCell(leftIndex, topIndex, cells);
            if (topBorder != 0 && _alive[x, topIndex] == null)
                SpawnCell(x, topIndex, cells);
            if (topRightCorner != 0 && _alive[rightIndex, topIndex] == null)
                SpawnCell(rightIndex, topIndex, cells);
            if (leftBorder != 0 && _alive[leftIndex, y] == null)
                SpawnCell(leftIndex, y, cells);
            if (rightBorder != 0 && _alive[rightIndex, y] == null)
                SpawnCell(rightIndex, y, cells);
            if (bottomLeftCorner != 0 && _alive[leftIndex, bottomIndex] == null)
                SpawnCell(leftIndex, bottomIndex, cells);
            if (bottomBorder != 0 && _alive[x, bottomIndex] == null)
                SpawnCell(x, bottomIndex, cells);
            if (bottomRightCorner != 0 && _alive[rightIndex, bottomIndex] == null)
                SpawnCell(rightIndex, bottomIndex, cells);
        }

        private static void Dump8X8(ulong value, string msg = null)
        {
            Debug.WriteLine(value + " " + (msg ?? String.Empty));
            for (int y = 0; y < 8; y++)
            {
                StringBuilder sb = new StringBuilder();
                for (int x = 0; x < 8; x++)
                {
                    int shift = Shift[x + y*8];
                    ulong v = (value >> shift) & 1;
                    sb.Append(v);
                }
                Debug.WriteLine(sb.ToString());
            }
        }
    }
}
