using System;
using System.Linq;

namespace GameOfLife
{
    internal class Cell
    {
        private const int NoGeneration = -1;
        private const int NoPlayerId = -1;

        public static readonly Cell NullCell = new Cell
        {
            Generation = NoGeneration,
            PlayerId = NoPlayerId
        };

        public int Generation { get; set; }
        public int PlayerId { get; set; }

        public Cell()
        {
            Generation = NoGeneration;
            PlayerId = NoPlayerId;
        }

        public bool IsEmpty
        {
            get { return Generation == NoGeneration && PlayerId == NoPlayerId; }
        }

        public void Death()
        {
            Generation = NoGeneration;
            PlayerId = NoPlayerId;
        }

        public void Born(int playerId)
        {
            Generation = 0;
            PlayerId = playerId;
        }

        public void Survived()
        {
            Generation++;
        }

        public override string ToString()
        {
            return IsEmpty ? "." : "*";
        }
    }

    public class LifeNaive : ILife
    {
        private readonly Cell[] _board;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public Rule Rule { get; private set; }
        public bool HasBorders { get; private set; }

        public int Generation { get; private set; }

        public LifeNaive(int width, int height, Rule rule, bool hasBorders=true)
        {
            if (rule == null)
                throw new ArgumentNullException("rule");

            Width = width;
            Height = height;
            Rule = rule;
            HasBorders = hasBorders;

            Generation = 0;

            _board = new Cell[width*height];
            for (int i = 0; i < _board.Length; i++ )
                _board[i] = new Cell();
        }

        public void Reset()
        {
            Generation = 0;
            foreach (Cell cell in _board)
                cell.Death();
        }

        public int Population
        {
            get { return _board.Count(c => !c.IsEmpty); }
        }

        public void Set(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return;
            Cell cell = Get(x, y);
            if (cell == Cell.NullCell)
                return;
            cell.Generation = 0;
            cell.PlayerId = 0;
        }

        public void NextGeneration()
        {
            // Compute modifiers
            bool[] modifiers = new bool[_board.Length];
            for(int y = 0; y < Height; y++)
                for(int x = 0; x < Width; x++)
                {
                    int index = Index(x, y);
                    modifiers[index] = false;
                    int neighbours = Neighbours(x, y);
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
            //
            Generation++;
        }

        public int[,] GetCells()
        {
            int[,] cells = new int[Width, Height];
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    Cell cell = Get(x, y);
                    cells[x, y] = cell.IsEmpty ? -1 : cell.Generation;
                }
            return cells;
        }

        public void GetView(int minX, int minY, int maxX, int maxY, bool[,] view)
        {
            int width = maxX - minX + 1;
            int height = maxY - minY + 1;
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    Cell cell = Get(minX+x, minY+y);
                    view[x, y] = cell != null && !cell.IsEmpty;
                }
        }

        public Tuple<int, int, int, int> GetMinMaxIndexes()
        {
            return new Tuple<int, int, int, int>(0, 0, Width - 1, Height - 1);
        }

        private int Neighbours(int x, int y)
        {
            int neighbours = 0;
            for (int stepY = -1; stepY <= +1; stepY++)
                for (int stepX = -1; stepX <= +1; stepX++)
                    if (stepX != 0 || stepY != 0)
                        neighbours += Get(x + stepX, y + stepY).IsEmpty ? 0 : 1;
            return neighbours;
        }

        private Cell Get(int x, int y)
        {
            if (HasBorders)
            {
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                    return Cell.NullCell;
            }
            else
            {
                if (x < 0)
                    x += Width;
                else if (x >= Width)
                    x -= Width;
                if (y < 0)
                    y += Height;
                else if (y >= Height)
                    y -= Height;
            }
            int index = Index(x, y);
            return _board[index];
        }

        private int Index(int x, int y)
        {
            return x + y*Width;
        }
    }
}
