using System;
using System.Text;

namespace GameOfLife
{
    public class LifeLookup2X2 : ILife
    {
        private readonly NeighbourLookupNaturalOrder _lookup;
        private readonly int _xSize; // grid size (half size)
        private readonly int _ySize;
        private uint[] _current; // 2x2 block stored in bit 5, 6, 9, 10
        private uint[] _next;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Generation { get; private set; }
        public Rule Rule { get; private set; }

        public int Population
        {
            get
            {
                uint count = 0;
                for (int i = 0; i < _xSize*_ySize; i++)
                {
                    uint value = _current[i]; // count bit in 2x2 block
                    count += ((value >> 5) & 1) + ((value >> 6) & 1) + ((value >> 9) & 1) + ((value >> 10) & 1);
                }
                return (int)count;
            }
        }

        public LifeLookup2X2(int width, int height, Rule rule)
        {
            Width = width;
            Height = height;
            Rule = rule;

            _lookup = new NeighbourLookupNaturalOrder(rule);

            _xSize = width/2;
            _ySize = height/2;

            _current = new uint[_xSize*_ySize];
            _next = new uint[_xSize * _ySize];
        }

        public void Reset()
        {
            Generation = 0;
            for (int i = 0; i < _xSize * _ySize; i++)
            {
                _current[i] = 0;
                _next[i] = 0;
            }
        }

        public void Set(int x, int y)
        {
            // 0   1   2   3
            //     -----
            // 4  |5   6|  7
            //    |     |
            // 8  |9  10|  11
            //     -----
            // 12  13  14  15
            int cellX = x%2;
            int cellY = y%2;
            int cellShift;
            if (cellX == 0)
                cellShift = cellY == 0 ? 5/*top left*/ : 9/*bottom left*/;
            else
                cellShift = cellY == 0 ? 6/*top right*/ : 10/*bottom right*/;

            int blockX = x / 2;
            int blockY = y / 2;
            int index = blockX + blockY * _xSize;
            // Set value
            _current[index] |= ((uint)1 << cellShift);
            /*
            // Set neighbours
            // neighbourhood
            // 5: x-1,y-1,15
            //    x,y-1,13
            //    x,y-1,14
            //    x-1,y,7
            //    x-1,y,11
            // 6: x,y-1,13
            //    x,y-1,14
            //    x+1,y-1:12
            //    x+1,y,4
            //    x+1,y,8
            // 9: x-1,y,7
            //    x-1,y,11
            //    x-1,y+1:3
            //    x,y+1:1
            //    x,y+1:2
            //10: x,y+1:1
            //    x,y+1:2
            //    x+1,y+1:0
            //    x+1,y:4
            //    x+1,y:8
            int topLeftBlock = GetIndex(x - 1, y - 1);
            int topBlock = GetIndex(x, y - 1);
            int topRightBlock = GetIndex(x+1, y - 1);
            int leftBlock = GetIndex(x - 1, y);
            int rightBlock = GetIndex(x + 1, y);
            int bottomLeftBlock = GetIndex(x - 1, y + 1);
            int bottomBlock = GetIndex(x, y + 1);
            int bottomRightBlock = GetIndex(x + 1, y + 1);
            if (cellShift == 5) // top left cell
            {
                _current[topLeftBlock] |= (1 << 15);
                _current[topBlock] |= (1 << 13);
                _current[topBlock] |= (1 << 14);
                _current[leftBlock] |= (1 << 7);
                _current[leftBlock] |= (1 << 11);
            }
            else if (cellShift == 6) // top right cell
            {
                _current[topBlock] |= (1 << 13);
                _current[topBlock] |= (1 << 14);
                _current[topRightBlock] |= (1 << 12);
                _current[rightBlock] |= (1 << 4);
                _current[rightBlock] |= (1 << 8);
            }
            else if (cellShift == 9) // bottom left
            {
                _current[leftBlock] |= (1 << 7);
                _current[leftBlock] |= (1 << 11);
                _current[bottomLeftBlock] |= (1 << 3);
                _current[bottomBlock] |= (1 << 1);
                _current[bottomBlock] |= (1 << 2);
            }
            else if (cellShift == 10) // bottom right
            {
                _current[bottomBlock] |= (1 << 1);
                _current[bottomBlock] |= (1 << 2);
                _current[bottomRightBlock] |= (1 << 0);
                _current[rightBlock] |= (1 << 4);
                _current[rightBlock] |= (1 << 8);
            }
             */
        }

        public void NextGeneration()
        {
            int length = _xSize*_ySize;
            for(int i = 0; i < _xSize*_ySize; i++)
            {
                uint block2X2 = _current[i];

                // inner 2x2 is stored in position 5, 6, 9, 10

                // build 4x4 from inner 2x2 of value and neighbours
                uint topLeft = _current[(length + i -_xSize - 1)%length];
                uint top = _current[(length + i - _xSize) % length];
                uint topRight = _current[(length + i - _xSize+1) % length];
                uint left = _current[(length + i - 1) % length];
                uint right = _current[(length + i +1) % length];
                uint bottomLeft = _current[(length + i +_xSize - 1) % length];
                uint bottom = _current[(length + i +_xSize) % length];
                uint bottomRight = _current[(length + i +_xSize + 1) % length];

                uint value = (block2X2 & ((1 << 5) | (1 << 6) | (1 << 9) | (1 << 10)))

                            | (((topLeft >> 10) & 1) << 0)
                            | (((top >> 9) & 1) << 1)
                            | (((top >> 10) & 1) << 2)
                            | (((topRight >> 9) & 1) << 3)

                            | (((left >> 6) & 1) << 4)
                            // 5, 6 from inner block
                            | (((right >> 5) & 1) << 7)
                            | (((left >> 10) & 1) << 8)
                            // 9, 10 from inner block
                            | (((right >> 9) & 1) << 11)

                            | (((bottomLeft >> 6) & 1) << 12)
                            | (((bottom >> 5) & 1) << 13)
                            | (((bottom >> 6) & 1) << 14)
                            | (((bottomRight >> 5) & 1) << 15);

                _next[i] = _lookup[value];
            }
            //
            uint[] temp = _next;
            _next = _current;
            _current = temp;
            //
            Generation++;
        }

        public void GetView(int minX, int minY, int maxX, int maxY, bool[,] view)
        {
            // doesn't use parameters
            for (int y = 0; y < _ySize; y++)
                for (int x = 0; x < _xSize; x++)
                {
                    int index = x + y * _xSize;
                    view[2 * x, 2 * y] = ((_current[index] >> 5) & 1) == 1;
                    view[2 * x + 1, 2 * y] = ((_current[index] >> 6) & 1) == 1;
                    view[2 * x, 2 * y + 1] = ((_current[index] >> 9) & 1) == 1;
                    view[2 * x + 1, 2 * y + 1] = ((_current[index] >> 10) & 1) == 1;
                }
        }

        public Tuple<int, int, int, int> GetMinMaxIndexes()
        {
            return new Tuple<int, int, int, int>(0, 0, Width-1, Height-1);
        }

        public void Dump()
        {
            // Dump map
            string separatorMap = String.Empty.PadLeft(_xSize * 2 + 3, '-');
            StringBuilder sbMap = new StringBuilder();
            for(int y = 0; y < _ySize; y++)
            {
                StringBuilder top = new StringBuilder();
                StringBuilder bottom = new StringBuilder();
                for(int x = 0; x < _xSize; x++)
                {
                    int index = x + y*_xSize;
                    uint value = _current[index];
                    uint topLeft = (value >> 5) & 1;
                    uint topRight = (value >> 6) & 1;
                    top.Append(topLeft);
                    top.Append(topRight);

                    uint bottomLeft = (value >> 9) & 1;
                    uint bottomRight = (value >> 10) & 1;
                    bottom.Append(bottomLeft);
                    bottom.Append(bottomRight);
                    //
                    top.Append('|');
                    bottom.Append('|');
                }
                sbMap.AppendLine(top.ToString());
                sbMap.AppendLine(bottom.ToString());
                sbMap.AppendLine(separatorMap);
            }
            System.Diagnostics.Debug.WriteLine(sbMap.ToString());
            // Dump map+neighbourhood
            string separatorNeighbours = String.Empty.PadLeft(_xSize*4 + 3, '-');
            StringBuilder sbMapNeighbours = new StringBuilder();
            for (int y = 0; y < _ySize; y++)
            {
                StringBuilder line0123 = new StringBuilder();
                StringBuilder line4567 = new StringBuilder();
                StringBuilder line891011 = new StringBuilder();
                StringBuilder line12131415 = new StringBuilder();
                for (int x = 0; x < _xSize; x++)
                {
                    int index = x + y * _xSize;
                    uint value = _current[index];

                    char bit0 = ((value >> 0) & 1) == 1 ? '.' : ' ';
                    char bit1 = ((value >> 1) & 1) == 1 ? '.' : ' ';
                    char bit2 = ((value >> 2) & 1) == 1 ? '.' : ' ';
                    char bit3 = ((value >> 3) & 1) == 1 ? '.' : ' ';
                    line0123.Append(bit0);
                    line0123.Append(bit1);
                    line0123.Append(bit2);
                    line0123.Append(bit3);

                    char bit4 = ((value >> 4) & 1) == 1 ? '.' : ' ';
                    char bit5 = ((value >> 5) & 1) == 1 ? '1' : '0';
                    char bit6 = ((value >> 6) & 1) == 1 ? '1' : '0';
                    char bit7 = ((value >> 7) & 1) == 1 ? '.' : ' ';
                    line4567.Append(bit4);
                    line4567.Append(bit5);
                    line4567.Append(bit6);
                    line4567.Append(bit7);

                    char bit8 = ((value >> 8) & 1) == 1 ? '.' : ' ';
                    char bit9 = ((value >> 9) & 1) == 1 ? '1' : '0';
                    char bit10 = ((value >> 10) & 1) == 1 ? '1' : '0';
                    char bit11 = ((value >> 11) & 1) == 1 ? '.' : ' ';
                    line891011.Append(bit8);
                    line891011.Append(bit9);
                    line891011.Append(bit10);
                    line891011.Append(bit11);

                    char bit12 = ((value >> 12) & 1) == 1 ? '.' : ' ';
                    char bit13 = ((value >> 13) & 1) == 1 ? '.' : ' ';
                    char bit14 = ((value >> 14) & 1) == 1 ? '.' : ' ';
                    char bit15 = ((value >> 15) & 1) == 1 ? '.' : ' ';
                    line12131415.Append(bit12);
                    line12131415.Append(bit13);
                    line12131415.Append(bit14);
                    line12131415.Append(bit15);
                    
                    //
                    line0123.Append('|');
                    line4567.Append('|');
                    line891011.Append('|');
                    line12131415.Append('|');
                }
                sbMapNeighbours.AppendLine(line0123.ToString());
                sbMapNeighbours.AppendLine(line4567.ToString());
                sbMapNeighbours.AppendLine(line891011.ToString());
                sbMapNeighbours.AppendLine(line12131415.ToString());
                sbMapNeighbours.AppendLine(separatorNeighbours);
            }
            System.Diagnostics.Debug.WriteLine(sbMapNeighbours.ToString());
        }
    }
}
