using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife
{
    //http://www.redblobgames.com/grids/hexagons/#map-storage
    //http://www.redblobgames.com/grids/hexagons/#coordinates     axial coordinates
    public class LifeHex
    {
        // store an hexagon in a rectangle wasting upper left or lower right corner
        private readonly Cell[] _board;

        // Size=2
        //     [0,-1]  [1,-1]         0(0,0)[-1,-1] 1(1,0)[0,-1] 2(2,0)[1,-1]
        // [-1,0]  [0,0]  [1,0]   =>  3(0,1)[-1,0]  4(1,1)[0,0]  5(2,1)[1,0]    with 0(0,0)[-1,-1] and 8(2,2)[1,1] wasted
        //     [-1,1]  [0,1]          6(0,2)[-1,1]  7(1,2)[0,1]  8(2,2)[1,1]

        public int Radius { get; private set; }
        public Rule Rule { get; private set; }
        public bool HasBorders { get; private set; }

        public int Generation { get; private set; }

        public LifeHex(int radius, Rule rule, bool hasBorders = true)
        {
            Radius = radius;
            Rule = rule;
            HasBorders = hasBorders;

            Generation = 0;

            int diagonal = 2*Radius+1;
            _board = new Cell[diagonal*diagonal];
            for (int i = 0; i < _board.Length; i++)
                _board[i] = new Cell();
        }

        public void Reset()
        {
            Generation = 0;
            foreach (Cell cell in _board)
                cell.Death();
        }

        public int PopulationCount
        {
            get { return _board.Count(c => !c.IsEmpty); }
        }

        public void Set(int q, int r, int playerId)
        {
            // TODO: check invalid q, r
            Cell cell = Get(q, r);
            if (cell == Cell.NullCell)
                return;
            cell.Generation = 0;
            cell.PlayerId = playerId;
        }

        public void NextGeneration()
        {
            // Compute modifiers
            bool[] modifiers = new bool[_board.Length];
            for (int r = -Radius; r <= Radius; r++)
                for (int q = -Radius; q <= Radius; q++)
                {
                    int index = Index(q, r);
                    modifiers[index] = false;
                    int neighbours = Neighbours(q, r);
                    Cell cell = _board[index];
                    if (cell.IsEmpty) // birth ?
                    {
                        if (Rule.Birth(neighbours))
                            modifiers[index] = true;
                    }
                    else // death ?
                    {
                        if (Rule.Death(neighbours))
                            modifiers[index] = true;
                    }
                }

            // Apply modifiers
            for (int i = 0; i < _board.Length; i++)
            {
                Cell cell = _board[i];
                if (modifiers[i])
                {
                    if (cell.IsEmpty) // birth
                        cell.Born(0); // TODO: played id
                    else // death
                        cell.Death();
                }
                else // survive
                {
                    if (!cell.IsEmpty)
                        cell.Survived();
                }
            }
            Generation++;
        }

        public int[,] GetCells()
        {
            int diagonal = 2*Radius+1;
            int[,] cells = new int[diagonal, diagonal];
            for (int r = -Radius; r <= Radius; r++)
                for (int q = -Radius; q <= Radius; q++)
                {
                    Cell cell = Get(q, r);
                    int x = q + Radius;
                    int y = r + Radius;
                    cells[x, y] = cell.IsEmpty ? -1 : cell.Generation;
                }
            return cells;
        }

        private static readonly int[] NeighbourQ = {0, +1, -1, +1, -1, 0};
        private static readonly int[] NeighbourR = {-1, -1, 0, 0, +1, +1};

        private int Neighbours(int q, int r)
        {
            int neighbours = 0;
            for(int i = 0; i < 6; i++)
            {
                Cell cell = Get(q + NeighbourQ[i], r + NeighbourR[i]);
                neighbours += cell.IsEmpty ? 0 : 1;
            }
            return neighbours;
        }

        private Cell Get(int q, int r)
        {
            if (q < -Radius || q > Radius || r < -Radius || r > Radius)
                return Cell.NullCell;
            // TODO: handle hasBorders and out of board
            int index = Index(q, r);
            return _board[index];
        }

        private int Index(int q, int r)
        {
            return (q + Radius) + (r + Radius)*(2*Radius+1);
        }



        public void Test()
        {
            Console.WriteLine("Radius: {0}", Radius);
            for (int r = -Radius; r <= Radius; r++)
                for (int q = -Radius; q <= Radius; q++)
                {
                    int index = Index(q, r);

                    Console.WriteLine("q:{0}, r:{1} \t ==> {2}", q, r, index);
                }
        }
    }
}
