using System;
using System.Linq;

namespace GameOfLife
{
    public class LifeLookupTest : ILife
    {
        private readonly NeighbourLookup _lookup;
        private readonly int[] _deltas; // delta used to computed neighbour location
        private readonly int _length; // width*height

        // no data compression, one cell in one array entry
        private int[] _current; // 1: alive  0: dead
        private int[] _next; // used to compute next generation

        public int Width { get; private set; }
        public int Height { get; private set; }
        public Rule Rule { get; private set; }
        public int Generation { get; private set; }

        public int Population
        {
            get { return _current.Count(c => c > 0); }
        }

        public LifeLookupTest(int width, int height, Rule rule)
        {
            Width = width;
            Height = height;
            Rule = rule;

            _lookup = new NeighbourLookup(rule);

            _length = width*height;
            _current = new int[_length];
            _next = new int[_length];

            _deltas = new int[8];
            _deltas[0] = -width - 1;
            _deltas[1] = -width;
            _deltas[2] = -width + 1;

            _deltas[3] = -1;
            // don't count ourself
            _deltas[4] = +1;

            _deltas[5] = +width - 1;
            _deltas[6] = +width;
            _deltas[7] = +width + 1;

            Reset();
        }

        public void Reset()
        {
            Generation = 0;
            for (int i = 0; i < _length; i++)
            {
                _current[i] = 0;
                _next[i] = 0;
            }
        }

        public void Set(int x, int y)
        {
            int index = GetIndex(x, y);
            _current[index] ^= 1;
        }

        public void NextGeneration()
        {
            // from current to next
            for (int y = 0; y < Width; y += 2)
            {
                for (int x = 0; x < Height; x += 2)
                {
                    int newValue = GetBlock(x, y);
                    // only use bits 5, 6, 9, 10
                    int bit5 = (newValue >> 5) & 1;
                    int bit6 = (newValue >> 6) & 1;
                    int bit9 = (newValue >> 9) & 1;
                    int bit10 = (newValue >> 10) & 1;
                    // 05 06
                    // 09 10
                    _next[GetIndex(x, y)] = bit5;
                    _next[GetIndex(x + 1, y)] = bit6;
                    _next[GetIndex(x, y + 1)] = bit9;
                    _next[GetIndex(x + 1, y + 1)] = bit10;
                }
            }

            // switch next and current
            int[] tmp = _next;
            _next = _current;
            _current = tmp;

            //
            Generation++;
        }

        public bool[,] GetView(int minX, int minY, int maxX, int maxY)
        {
            int width = maxX - minX + 1;
            int height = maxY - minY + 1;
            bool[,] cells = new bool[width, height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    int index = GetIndex(minX + x, minY + y);
                    cells[x, y] = _current[index] == 1;
                }
            return cells;
        }

        public Tuple<int, int, int, int> GetMinMaxIndexes()
        {
            return new Tuple<int, int, int, int>(0, 0, Width - 1, Height - 1);
        }

        private int GetBlock(int x, int y) // get 4x4 block with inner 2x2 top left cell in x,y
        {
            // cell on x, y is 5
            // 00 01 02 03
            // 04 05 06 07
            // 08 09 10 11
            // 12 13 14 15
            int cell0 = _current[GetIndex(x - 1, y - 1)];
            int cell1 = _current[GetIndex(x, y - 1)];
            int cell2 = _current[GetIndex(x + 1, y - 1)];
            int cell3 = _current[GetIndex(x + 2, y - 1)];

            int cell4 = _current[GetIndex(x - 1, y)];
            int cell5 = _current[GetIndex(x, y)];
            int cell6 = _current[GetIndex(x + 1, y)];
            int cell7 = _current[GetIndex(x + 2, y)];

            int cell8 = _current[GetIndex(x - 1, y + 1)];
            int cell9 = _current[GetIndex(x, y + 1)];
            int cell10 = _current[GetIndex(x + 1, y + 1)];
            int cell11 = _current[GetIndex(x + 2, y + 1)];

            int cell12 = _current[GetIndex(x - 1, y + 2)];
            int cell13 = _current[GetIndex(x, y + 2)];
            int cell14 = _current[GetIndex(x + 1, y + 2)];
            int cell15 = _current[GetIndex(x + 2, y + 2)];

            int value =
                (cell0) | (cell1 << 1) | (cell2 << 2) | (cell3 << 3) |
                (cell4 << 4) | (cell5 << 5) | (cell6 << 6) | (cell7 << 7) |
                (cell8 << 8) | (cell9 << 9) | (cell10 << 10) | (cell11 << 11) |
                (cell12 << 12) | (cell13 << 13) | (cell14 << 14) | (cell15 << 15);

            int ret = _lookup[value];
            return ret;
        }

        private int GetIndex(int x, int y)
        {
            return (_length + (x + y*Width))%_length; // fake toroidal coordinates
        }
    }
}
