using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GameOfLife
{
    public class LifeModifiedList
    {
        private readonly int[] _deltas; // delta used to computed neighbour location
        private readonly int _length; // width*height

        private readonly int[] _cells;
        private readonly int[] _modifiedIndex; // index of cells modified on previous step
        private int _lastModifiedIndex; // last modified index in modifiedIndex array

        public int Width { get; private set; }
        public int Height { get; private set; }

        public int MaxModified { get; private set; }
        public int MaxNeighbours { get; private set; }
        public int MaxLoopCount { get; private set; }

        public int Generation { get; private set; }

        public LifeModifiedList(int width, int height)
        {
            Width = width;
            Height = height;

            Generation = 0;
            MaxModified = 0;
            MaxNeighbours = 0;

            _length = width*height;
            _cells = new int[_length];
            _modifiedIndex = new int[_length]; // array length <= Width*Height

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

            _lastModifiedIndex = -1; // don't use modified index array
        }

        public void Set(int x, int y)
        {
            int index = GetIndex(x, y);
            _cells[index] = 1;
        }

        public int PopulationCount
        {
            get { return _cells.Count(c => c > 0); }
        }

        public void NextGeneration()
        {
            int loopCount = 0;
            //// DEBUG: display cells
            //for (int y = 0; y < Height; y++)
            //{
            //    for (int x = 0; x < Width; x++)
            //    {
            //        int index = GetIndex(x, y);
            //        System.Diagnostics.Debug.Write(_cells[index] > 0 ? "*" : ".");
            //    }
            //    System.Diagnostics.Debug.WriteLine("");
            //}

            //_lastModifiedIndex = -1; // DEBUG: force full refresh
            Dictionary<int, int> neighboursCount = new Dictionary<int, int>(); // <index,count>

            // count neighbours of modified cells on previous step (or every cells if no previous step)
            if (_lastModifiedIndex == -1)
            {
                // Check every cells
                for (int cellIndex = 0; cellIndex < _length; cellIndex++)
                    neighboursCount.Add(cellIndex, CountNeighbours(cellIndex));
            }
            else
            {
                // Check every modified cell
                for (int index = 0; index < _lastModifiedIndex; index++)
                {
                    loopCount++;
                    int cellIndex = _modifiedIndex[index];

                    //System.Diagnostics.Debug.WriteLine("CELL: {0},{1} -> {2}", cellIndex % Width, cellIndex / Width, _cells[cellIndex]);

                    if (_cells[cellIndex] == 1)
                    {
                        // foreach delta, increase neighbour on index + delta
                        foreach (int delta in _deltas)
                        {
                            loopCount++;

                            int deltaIndex = (_length + cellIndex + delta)%_length;
                            if (neighboursCount.ContainsKey(deltaIndex))
                                neighboursCount[deltaIndex]++;
                            else
                                neighboursCount.Add(deltaIndex, 1);
                            //System.Diagnostics.Debug.WriteLine("NEIGHBOUR INC:{0},{1} -> {2}", deltaIndex%Width, deltaIndex/Width, _cells[deltaIndex]);
                        }
                    }
                }
                MaxNeighbours = Math.Max(neighboursCount.Count, MaxNeighbours);
            }

            //// DEBUG: display neighbours count
            //foreach(KeyValuePair<int,int> kv in neighboursCount)
            //    System.Diagnostics.Debug.WriteLine("NEIGHBOUR COUNT: {0},{1} -> {2}", kv.Key % Width, kv.Key / Width, kv.Value);

            // apply rules using neighbours count
            _lastModifiedIndex = 0;
            foreach (KeyValuePair<int, int> kv in neighboursCount)
            {
                loopCount++;

                int cellIndex = kv.Key;
                int neighbourCount = kv.Value;
                bool modified = false;
                if (_cells[cellIndex] == 0 && neighbourCount == 3) // birth
                {
                    //System.Diagnostics.Debug.WriteLine("BIRTH:{0},{1}", cellIndex % Width, cellIndex / Width);
                    _cells[cellIndex] = 1;
                    modified = true;
                }
                else if (_cells[cellIndex] == 1 && neighbourCount != 2 && neighbourCount != 3) // death
                {
                    //System.Diagnostics.Debug.WriteLine("DEATH:{0},{1}", cellIndex % Width, cellIndex / Width);
                    _cells[cellIndex] = 0;
                    modified = true;
                }
                else if (_cells[cellIndex] == 1) // survived
                {
                    //System.Diagnostics.Debug.WriteLine("SURVIVED:{0},{1}", cellIndex % Width, cellIndex / Width);
                    modified = true;
                }
                if (modified)
                    _modifiedIndex[_lastModifiedIndex++] = cellIndex;
            }

            MaxModified = Math.Max(_lastModifiedIndex, MaxModified);
            MaxLoopCount = Math.Max(loopCount, MaxLoopCount);

            //// DEBUG: display modified
            //for (int i = 0; i < _lastModifiedIndex; i++)
            //{
            //    int index = _modifiedIndex[i];
            //    int x = index % Width;
            //    int y = index / Width;
            //    System.Diagnostics.Debug.WriteLine("MODIFIED: {0},{1}", x, y);
            //}

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
                    cells[x, y] = _cells[index];
                }
            return cells;
        }

        private int CountNeighbours(int index)
        {
            return _deltas.Sum(delta => _cells[(_length + index + delta) % _length]);
        }

        private int GetIndex(int x, int y)
        {
            return x + y*Width;
        }
    }
}
