using System;
using System.Linq;

namespace GameOfLife
{
    public class LifeLookup1X1 : ILife
    {
        private readonly NeighbourLookupNaturalOrder _lookup;
        private readonly int _length; // width*height

        // no data compression, one cell in one array entry
        private uint[] _current; // 1: alive  0: dead
        private uint[] _next; // used to compute next generation

        public int Width { get; private set; }
        public int Height { get; private set; }
        public Rule Rule { get; private set; }
        public int Generation { get; private set; }

        public int Population
        {
            get { return _current.Count(c => c > 0); }
        }

        public LifeLookup1X1(int width, int height, Rule rule)
        {
            Width = width;
            Height = height;
            Rule = rule;

            _lookup = new NeighbourLookupNaturalOrder(rule);

            _length = width*height;
            _current = new uint[_length];
            _next = new uint[_length];

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
                    uint newValue = GetBlock(x, y);
                    // only use bits 5, 6, 9, 10
                    uint bit5 = (newValue >> 5) & 1;
                    uint bit6 = (newValue >> 6) & 1;
                    uint bit9 = (newValue >> 9) & 1;
                    uint bit10 = (newValue >> 10) & 1;
                    // 05 06
                    // 09 10
                    _next[GetIndex(x, y)] = bit5;
                    _next[GetIndex(x + 1, y)] = bit6;
                    _next[GetIndex(x, y + 1)] = bit9;
                    _next[GetIndex(x + 1, y + 1)] = bit10;
                }
            }

            // switch next and current
            uint[] tmp = _next;
            _next = _current;
            _current = tmp;

            //
            Generation++;
        }

        public void GetView(int minX, int minY, int maxX, int maxY, bool[,] view)
        {
            int width = maxX - minX + 1;
            int height = maxY - minY + 1;
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    int index = GetIndex(minX + x, minY + y);
                    view[x, y] = _current[index] == 1;
                }
        }

        public Tuple<int, int, int, int> GetMinMaxIndexes()
        {
            return new Tuple<int, int, int, int>(0, 0, Width - 1, Height - 1);
        }

        private uint GetBlock(int x, int y) // get 4x4 block with inner 2x2 top left cell in x,y
        {
            // cell on x, y is 5
            // 00 01 02 03
            // 04 05 06 07
            // 08 09 10 11
            // 12 13 14 15
            uint cell0 = _current[GetIndex(x - 1, y - 1)];
            uint cell1 = _current[GetIndex(x, y - 1)];
            uint cell2 = _current[GetIndex(x + 1, y - 1)];
            uint cell3 = _current[GetIndex(x + 2, y - 1)];
            
            uint cell4 = _current[GetIndex(x - 1, y)];
            uint cell5 = _current[GetIndex(x, y)];
            uint cell6 = _current[GetIndex(x + 1, y)];
            uint cell7 = _current[GetIndex(x + 2, y)];
            
            uint cell8 = _current[GetIndex(x - 1, y + 1)];
            uint cell9 = _current[GetIndex(x, y + 1)];
            uint cell10 = _current[GetIndex(x + 1, y + 1)];
            uint cell11 = _current[GetIndex(x + 2, y + 1)];
            
            uint cell12 = _current[GetIndex(x - 1, y + 2)];
            uint cell13 = _current[GetIndex(x, y + 2)];
            uint cell14 = _current[GetIndex(x + 1, y + 2)];
            uint cell15 = _current[GetIndex(x + 2, y + 2)];

            uint value =
                (cell0) | (cell1 << 1) | (cell2 << 2) | (cell3 << 3) |
                (cell4 << 4) | (cell5 << 5) | (cell6 << 6) | (cell7 << 7) |
                (cell8 << 8) | (cell9 << 9) | (cell10 << 10) | (cell11 << 11) |
                (cell12 << 12) | (cell13 << 13) | (cell14 << 14) | (cell15 << 15);

            uint ret = _lookup[value];
            return ret;
        }

        private int GetIndex(int x, int y)
        {
            return (_length + (x + y*Width))%_length; // fake toroidal coordinates
        }
    }
}
