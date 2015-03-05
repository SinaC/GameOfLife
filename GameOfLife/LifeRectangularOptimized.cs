using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife
{
    public class LifeRectangularOptimized
    {
        private readonly int[] _deltas; // delta used to computed neighbour location
        private readonly int _length; // width*height

        private int[] _current;// 1: alive  0: dead
        private int[] _next; // used to compute next generation

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Generation { get; private set; }

        public LifeRectangularOptimized(int width, int height)
        {
            Width = width;
            Height = height;

            Generation = 0;

            _length = width*height;
            _current = new int[_length];
            _next = new int[_length];

            //_deltas = new int[9];
            //_deltas[0] = -height - 1;
            //_deltas[1] = -height;
            //_deltas[2] = -height + 1;

            //_deltas[3] = -1;
            //_deltas[4] = 0;
            //_deltas[5] = +1;

            //_deltas[6] = +height - 1;
            //_deltas[7] = +height;
            //_deltas[8] = +height + 1;

            _deltas = new int[8];
            _deltas[0] = -width - 1;
            _deltas[1] = -width;
            _deltas[2] = -width + 1;

            _deltas[3] = -1;
            _deltas[4] = +1;

            _deltas[5] = +width - 1;
            _deltas[6] = +width;
            _deltas[7] = +width + 1;
        }

        public void Set(int x, int y)
        {
            int index = GetIndex(x, y);
            _current[index] = 1;
        }

        public int PopulationCount
        {
            get { return _current.Count(c => c > 0); }
        }

        // TODO: 
        //  store a list of modified cell in previous step
        //  build a list of neighbour from previously stored list
        //  apply rule on neighour list to update board

        public void NextGeneration()
        {
            //for (int y = 0; y < Height; y++)
            //{
            //    for (int x = 0; x < Width; x++)
            //    {
            //        int index = GetIndex(x, y);
            //        int neighbours = CountNeighbours(index);
            //        System.Diagnostics.Debug.Write(String.Format("[{0}|{1}]", _current[index], neighbours));
            //    }
            //    System.Diagnostics.Debug.WriteLine("");
            //}

            // from current to next
            for (int i = 0; i < _length; i++)
            {
                // TODO: handle borders
                int neighbours = CountNeighbours(i);
                if (_current[i] == 0 && neighbours == 3)
                    _next[i] = 1;
                else if (_current[i] == 1 && (neighbours == 2 || neighbours == 3))
                    _next[i] = 1;
                else
                    _next[i] = 0;
            }

            // switch next and current
            int[] tmp = _next;
            _next = _current;
            _current = tmp;

            //
            Generation++;
        }

        public int[,] GetCells()
        {
            int[,] cells = new int[Width, Height];
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    int index = GetIndex(x, y);
                    cells[x, y] = _current[index];
                }
            return cells;
        }

        private int CountNeighbours(int index)
        {
            return _deltas.Sum(delta => _current[(_length + index + delta) % _length]);
        }

        private int GetIndex(int x, int y)
        {
            return x + y*Width;
        }
    }
}
