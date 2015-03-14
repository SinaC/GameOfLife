using System;
using System.Text;

namespace GameOfLife
{
    public class LifeLookup4X4 : ILife
    {
        // 00  01 02 03 04  05 06 07
        //     -----------
        // 08 |09 10 11 12| 13 14 15
        // 16 |17 18 19 20| 21 22 23
        // 24 |25 26 27 28| 29 30 31
        // 32 |33 34 35 36| 37 38 39
        //     -----------
        // 40  41 42 43 44  45 46 47
        // 48  49 50 51 52  53 54 55
        // 56  57 58 59 60  61 62 63

        // cell are stored in a 64 bits, real data are stored at position
        // 09 10 11 12
        // 17 18 19 20
        // 25 26 27 28
        // 33 34 35 36
        // cell are store in a 64 bits instead of 16 bits to avoid cast while building 6x6 containing neighbours

        // when computing new value, we initialize a 64 bits with original value (4x4) and set neighbours (6x6)
        // in position 
        //  00 01 02 03 04 05 
        //  08    REAL     13 
        //  16    DATA     21 
        //  24    GOES     29 
        //  32    HERE     37 
        //  40 41 42 43 44 45

        //09 10 11 12  09 10 11 12  09 10 11 12
        //17 18 19 20  17 18 19 20  17 18 19 20
        //25 26 27 28  25 26 27 28  25 26 27 28
        //33 34 35 36  33 34 35 36  33 34 35 36
        //             -----------
        //09 10 11 12 |09 10 11 12| 09 10 11 12
        //17 18 19 20 |17 18 19 20| 17 18 19 20
        //25 26 27 28 |25 26 27 28| 25 26 27 28
        //33 34 35 36 |33 34 35 36| 33 34 35 36
        //             -----------
        //09 10 11 12  09 10 11 12  09 10 11 12
        //17 18 19 20  17 18 19 20  17 18 19 20
        //25 26 27 28  25 26 27 28  25 26 27 28
        //33 34 35 36  33 34 35 36  33 34 35 36

        // top
        //  bit 36 of x-1,y-1 -> bit 0
        //  bit 33 of x  ,y-1 -> bit 1
        //  bit 34 of x  ,y-1 -> bit 2
        //  bit 35 of x  ,y-1 -> bit 3
        //  bit 36 of x  ,y-1 -> bit 4
        //  bit 33 of x+1,y-1 -> bit 5
        // left
        //  bit 12 of x-1,y   -> bit 8
        //  bit 20 of x-1,y   -> bit 16
        //  bit 28 of x-1,y   -> bit 24
        //  bit 36 of x-1,y   -> bit 32
        // right
        //  bit  9 of x+1,y   -> bit 13
        //  bit 17 of x+1,y   -> bit 21
        //  bit 25 of x+1,y   -> bit 29
        //  bit 33 of x+1,y   -> bit 37
        // bottom
        //  bit 12 of x-1,y+1 -> bit 40
        //  bit  9 of x  ,y+1 -> bit 41
        //  bit 10 of x  ,y+1 -> bit 42
        //  bit 11 of x  ,y+1 -> bit 43
        //  bit 12 of x  ,y+1 -> bit 44
        //  bit  9 of x+1,y+1 -> bit 45
        // this allows faster 2x2 lookup because neighbourhood will already be stored and we don't have to access other cells anymore

        // compute each sub-2x2 block
        // loop on value 09, 11, 25, 27 (top left bit of each sub-2x2)
        //   build a 16 bit value to use on lookup, real data are stored in 5, 6, 9, 10 and neighbours in 0, 1, 2, 3, 4, 7, 8, 11, 12, 13, 14, 15
        //      00  01 02  03
        //          -----
        //      04 |05 06| 07
        //      08 |09 10| 11
        //          -----
        //      12  13 14  15
        //   inner 2x2: 
        //          bit value -> stored in bit 5
        //          bit value+1 -> stored in bit 6
        //          bit value+8 -> stored in bit 9
        //          bit value+9 -> stored in bit 10
        //   neighbours:
        //          top (4 bits): from bit value-9 to bit value-6 -> stored in bit 0 to 3
        //          left top: bit value-1 -> stored in bit 4
        //          left bottom: bit value+7 -> stored in bit 8
        //          right top: bit value+2 -> stored in bit 7
        //          right bottom: bit value+10 -> stored in bit 11
        //          bottom (4 bits): from bit value+15 to bit value+18 -> stored in bit 12 to 15

        private readonly int _sizeX;
        private readonly int _sizeY;
        private readonly int _length;
        private long[] _current;
        private long[] _next;
        private readonly NeighbourLookup _lookup;

        private readonly static int[] TopLeftOfSub2X2 = {9, 11, 25, 27};

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Generation { get; private set; }
        public Rule Rule { get; private set; }
        public Boundary Boundary { get; private set; }

        public int Population
        {
            get
            {
                long count = 0;
                for (int i = 0; i < _length; i++)
                {
                    //09 10 11 12
                    //17 18 19 20
                    //25 26 27 28
                    //33 34 35 36
                    long value = _current[i]; // count bit in 2x2 block
                    count += ((value >> 9) & 1) + ((value >> 10) & 1) + ((value >> 11) & 1) + ((value >> 12) & 1)
                        + ((value >> 17) & 1) + ((value >> 18) & 1) + ((value >> 19) & 1) + ((value >> 20) & 1)
                        + ((value >> 25) & 1) + ((value >> 26) & 1) + ((value >> 27) & 1) + ((value >> 28) & 1)
                        + ((value >> 33) & 1) + ((value >> 34) & 1) + ((value >> 35) & 1) + ((value >> 36) & 1);
                }
                return  (int)count;
            }
        }

        public LifeLookup4X4(int width, int height, Rule rule, Boundary boundary)
        {
            Width = width;
            Height = height;
            Rule = rule;
            Boundary = boundary;

            _lookup = new NeighbourLookup(rule);

            _sizeX = width/4; // TODO: must be a multiple of 4
            _sizeY = height/4;
            _length = _sizeX*_sizeY;
            _current = new long[_length];
            _next = new long[_length];

            Generation = 0;
        }

        public void Reset()
        {
            for (int i = 0; i < _length; i++)
            {
                _current[i] = 0;
                _next[i] = 0;
            }
            Generation = 0;
        }

        public void Set(int x, int y)
        {
            int cellX = x%4;
            int cellY = y%4;
            //09 10 11 12
            //17 18 19 20
            //25 26 27 28
            //33 34 35 36
            int shift = cellY*8 + 9 + cellX;
            int gridX = x/4;
            int gridY = y/4;
            int index = gridX + gridY*_sizeX;
            _current[index] |= ((long)1 << shift);
        }

        public void NextGeneration()
        {
            //System.Diagnostics.Debug.WriteLine("Generation: {0}", Generation);
            for (int i = 0; i < _length; i++)
                _next[i] = ComputeNewValue(i, _current[i]);
            //
            long[] temp = _next;
            _next = _current;
            _current = temp;
            //
            Generation++;
        }

        public void GetView(int minX, int minY, int maxX, int maxY, bool[,] view)
        {
            // doesn't use parameters
            for (int y = 0; y < _sizeY; y++)
                for (int x = 0; x < _sizeX; x++)
                {
                    int index = x + y*_sizeX;
                    //09 10 11 12
                    //17 18 19 20
                    //25 26 27 28
                    //33 34 35 36
                    for(int yi = 0; yi < 4; yi++)
                        for (int xi = 0; xi < 4; xi++)
                        {
                            int shift = 8*yi + 9 + xi;
                            view[4*x + xi, 4*y + yi] = ((_current[index] >> shift) & 1) == 1;
                        }
                }
        }

        public Tuple<int, int, int, int> GetMinMaxIndexes()
        {
            return new Tuple<int, int, int, int>(0, 0, Width - 1, Height - 1);
        }
        
        // TODO: optimize  
        //      ((x >> y) & 1) << z   into  (x & mask) << (y-z)  with y > z
        //      ((x >> y) & 1) << z   into  (x & mask) >> (z-y)  with y < z
        public long ComputeNewValue(int i, long value)
        {
            // Fake toroidal coordinates
            //long topLeft = _current[(_length + i - _sizeX - 1)%_length];
            //long top = _current[(_length + i - _sizeX) % _length];
            //long topRight = _current[(_length + i - _sizeX + 1) % _length];
            //long left = _current[(_length + i - 1) % _length];
            //long right = _current[(_length + i + 1) % _length];
            //long bottomLeft = _current[(_length + i + _sizeX - 1) % _length];
            //long bottom = _current[(_length + i + _sizeX) % _length];
            //long bottomRight = _current[(_length + i + _sizeX + 1) % _length];

            int x = i%_sizeX;
            int y = i/_sizeX;
            int topIndex, bottomIndex, leftIndex, rightIndex;
            Boundary.AddStepX(x, -1, out leftIndex);
            Boundary.AddStepX(x, 1, out rightIndex);
            Boundary.AddStepY(y, -1, out topIndex);
            Boundary.AddStepY(y, 1, out bottomIndex);

            long topLeft = _current[GetIndex(leftIndex, topIndex)];
            long top = _current[GetIndex(x, topIndex)];
            long topRight = _current[GetIndex(rightIndex, topIndex)];
            long left = _current[GetIndex(leftIndex, y)];
            long right = _current[GetIndex(rightIndex, y)];
            long bottomLeft = _current[GetIndex(leftIndex, bottomIndex)];
            long bottom = _current[GetIndex(x, bottomIndex)];
            long bottomRight = _current[GetIndex(rightIndex, bottomIndex)];

            //System.Diagnostics.Debug.WriteLine("current:");
            //System.Diagnostics.Debug.WriteLine("{0},{1}", i%_sizeX, i/_sizeX);
            //Dump4X4(value);

            // real data are stored in bits
            //09 10 11 12
            //17 18 19 20
            //25 26 27 28
            //33 34 35 36

            //initialize a 64 bits with original value (4x4) and set neighbours
            //  00 01 02 03 04 05 
            //  08             13 
            //  16             21 
            //  24             29 
            //  32             37 
            //  40 41 42 43 44 45
            // this allows faster 2x2 lookup because neighbourhood will already be stored and we don't have to access other cell anymore
            long lookup6X6 = value; // copy value no need to filter bits, they will be cleared by neighbours
            // build 6x6 block with real value and neighbours
            // top
            //  bit 36 of x-1,y-1 -> bit 0
            //  bit 33 of x  ,y-1 -> bit 1
            //  bit 34 of x  ,y-1 -> bit 2
            //  bit 35 of x  ,y-1 -> bit 3
            //  bit 36 of x  ,y-1 -> bit 4
            //  bit 33 of x+1,y-1 -> bit 5
            lookup6X6 |= ((topLeft >> 36) & 1) << 0;
            lookup6X6 |= ((top >> 33) & 0xF) << 1; // keep 4 bits from 33 to 36 and put them in 1 to 4
            lookup6X6 |= ((topRight >> 33) & 1) << 5;
            // left
            //  bit 12 of x-1,y   -> bit 8
            //  bit 20 of x-1,y   -> bit 16
            //  bit 28 of x-1,y   -> bit 24
            //  bit 36 of x-1,y   -> bit 32
            lookup6X6 |= ((left >> 12) & 1) << 8;
            lookup6X6 |= ((left >> 20) & 1) << 16;
            lookup6X6 |= ((left >> 28) & 1) << 24;
            lookup6X6 |= ((left >> 36) & 1) << 32;
            // right
            //  bit  9 of x+1,y   -> bit 13
            //  bit 17 of x+1,y   -> bit 21
            //  bit 25 of x+1,y   -> bit 29
            //  bit 33 of x+1,y   -> bit 37
            lookup6X6 |= ((right >> 9) & 1) << 13;
            lookup6X6 |= ((right >> 17) & 1) << 21;
            lookup6X6 |= ((right >> 25) & 1) << 29;
            lookup6X6 |= ((right >> 33) & 1) << 37;
            // bottom
            //  bit 12 of x-1,y+1 -> bit 40
            //  bit  9 of x  ,y+1 -> bit 41
            //  bit 10 of x  ,y+1 -> bit 42
            //  bit 11 of x  ,y+1 -> bit 43
            //  bit 12 of x  ,y+1 -> bit 44
            //  bit  9 of x+1,y+1 -> bit 45
            lookup6X6 |= ((bottomLeft >> 12) & 1) << 40;
            lookup6X6 |= ((bottom >> 9) & 0xF) << 41; // keep 4 bits from 9 to 12 and put them in 41 to 44
            lookup6X6 |= ((bottomRight >> 9) & 1) << 45;

            // if empty cell and empty neighbourhood -> no birth, death, survival -> no need to compute anything
            if (lookup6X6 == 0)
                return value;

            //System.Diagnostics.Debug.WriteLine("neighbours");
            //Dump6X6(lookup6X6);

            // compute each sub-2x2 block (bit index of each top left cell in block in stored in an array)
            // 09, 10,    11, 12,            bit index,   bit index+1
            // 17, 18     19, 20             bit index+8, bit index+9
            //
            // 25, 26,    27, 28,
            // 33, 34     35, 36
            // neighbours (sample with block 9, 10, 17, 18)
            //  00 01 02 03 04 05   top row: bit index-9 -> bit index-6
            //     -----
            //  08|09 10|11 12 13   left top: bit index-1     right top: bit index+2
            //  16|17 18|19 20 21   left bottom: bit index+7  right bottom: bit index+10
            //     -----
            //  24 25 26 27 28 29   bottom row: bit index+5 -> bit index+18
            //  32 33 34 35 36 37 
            //  40 41 42 43 44 45
            long result = 0;
            foreach(int bitIndex in TopLeftOfSub2X2)
            {
                //   build a 16 bit value to use on lookup table, real data are stored in 5, 6, 9, 10 and neighbours in 0, 1, 2, 3, 4, 7, 8, 11, 12, 13, 14, 15
                //      00 01 02 03
                //      04 05 06 07
                //      08 09 10 11
                //      12 13 14 15
                //   inner 2x2: 
                //          bit index -> stored in bit 5
                //          bit index+1 -> stored in bit 6
                //          bit index+8 -> stored in bit 9
                //          bit index+9 -> stored in bit 10
                //   neighbours:
                //          top (4 bits): from bit index-9 to bit index-6 -> stored in bit 0 to 3
                //          left top: bit index-1 -> stored in bit 4
                //          left bottom: bit index+7 -> stored in bit 8
                //          right top: bit index+2 -> stored in bit 7
                //          right bottom: bit index+10 -> stored in bit 11
                //          bottom (4 bits): from bit index+15 to bit index+18 -> stored in bit 12 to 15
                long lookup4X4 = // stored in a 64 bits to avoid cast on short on each step
                    // inner 2x2
                    ((lookup6X6 >> bitIndex) & 1) << 5 |
                    ((lookup6X6 >> bitIndex + 1) & 1) << 6 |
                    ((lookup6X6 >> bitIndex + 8) & 1) << 9 |
                    ((lookup6X6 >> bitIndex + 9) & 1) << 10;
                // neighbours
                lookup4X4 |= ((lookup6X6 >> (bitIndex - 9)) & 0xF) << 0; // top
                lookup4X4 |= ((lookup6X6 >> (bitIndex - 1)) & 1) << 4; // left top
                lookup4X4 |= ((lookup6X6 >> (bitIndex + 7)) & 1) << 8; // left bottom
                lookup4X4 |= ((lookup6X6 >> (bitIndex + 2)) & 1) << 7; // right top
                lookup4X4 |= ((lookup6X6 >> (bitIndex + 10)) & 1) << 11; // right bottom
                lookup4X4 |= ((lookup6X6 >> (bitIndex + 15)) & 0xF) << 12; // bottom

                //System.Diagnostics.Debug.WriteLine("Sub2x2 + neighbours {0}", bitIndex);
                //Dump4X4((int)lookup4X4);

                long result2X2 = _lookup[lookup4X4]; // result 2x2 must be a long to avoid bit loss while shifting in injection into result

                //System.Diagnostics.Debug.WriteLine("Result 2x2: row1:{0} row2:{1}", (result2X2 >> 5) & 0x3, (result2X2 >> 9) & 0x3);
                //Dump4X4((int)result2X2);

                // inject result2x2 into result, only use bit 5, 6, 9, 10
                // bit 5, 6 -> bit index, bit index+1
                // bit 9, 10 -> bit index+8, bit index+9
                result |= ((result2X2 >> 5) & 0x3) << bitIndex; // keep 2 bits: 5, 6 -> bit index, bit index+1
                result |= ((result2X2 >> 9) & 0x3) << (bitIndex+8); // keep 2 bits: 9, 10 -> bit index+8, bit index+9

                //System.Diagnostics.Debug.WriteLine("temp result");
                //Dump4X4(result);
            }

            //System.Diagnostics.Debug.WriteLine("new:");
            //Dump4X4(result);

            return result;
        }

        private int GetIndex(int x, int y)
        {
            return x + y*_sizeX;
        }

        private static void Dump2X2(int value)
        {
            System.Diagnostics.Debug.WriteLine(value);
            //05 06
            //09 10
            int topLeft = (value >> 5) & 1;
            int topRight = (value >> 6) & 1;
            System.Diagnostics.Debug.WriteLine("{0}{1}", topLeft, topRight);

            int bottomLeft = (value >> 9) & 1;
            int bottomRight = (value >> 10) & 1;
            System.Diagnostics.Debug.WriteLine("{0}{1}", bottomLeft, bottomRight);
        }

        private static void Dump4X4(int value)
        {
            System.Diagnostics.Debug.WriteLine(value);
            //00 01 02 03
            //04 05 06 07
            //08 09 10 11
            //12 13 14 15
            for (int y = 0; y < 4; y++)
            {
                StringBuilder sb = new StringBuilder();
                for (int x = 0; x < 4; x++)
                {
                    int shift = y * 4 + x;
                    long v = (value >> shift) & 1;
                    sb.Append(v);
                }
                System.Diagnostics.Debug.WriteLine(sb.ToString());
            }
        }

        private static void Dump4X4(long value)
        {
            System.Diagnostics.Debug.WriteLine(value);
            //09 10 11 12
            //17 18 19 20
            //25 26 27 28
            //33 34 35 36
            //long v09 = (value >> 9) & 1;
            //long v10 = (value >> 10) & 1;
            //long v11 = (value >> 11) & 1;
            //long v12 = (value >> 12) & 1;
            //System.Diagnostics.Debug.WriteLine("{0}{1}{2}{3}", v09, v10, v11, v12);
            //long v17 = (value >> 17) & 1;
            //long v18 = (value >> 18) & 1;
            //long v19 = (value >> 19) & 1;
            //long v20 = (value >> 20) & 1;
            //System.Diagnostics.Debug.WriteLine("{0}{1}{2}{3}", v17, v18, v19, v20);
            //long v25 = (value >> 25) & 1;
            //long v26 = (value >> 26) & 1;
            //long v27 = (value >> 27) & 1;
            //long v28 = (value >> 28) & 1;
            //System.Diagnostics.Debug.WriteLine("{0}{1}{2}{3}", v25, v26, v27, v28);
            //long v33 = (value >> 33) & 1;
            //long v34 = (value >> 34) & 1;
            //long v35 = (value >> 35) & 1;
            //long v36 = (value >> 36) & 1;
            //System.Diagnostics.Debug.WriteLine("{0}{1}{2}{3}", v33, v34, v35, v36);
            for (int y = 0; y < 4; y++)
            {
                StringBuilder sb = new StringBuilder();
                for (int x = 0; x < 4; x++)
                {
                    int shift = y * 8 + 9 + x;
                    long v = (value >> shift) & 1;
                    sb.Append(v);
                }
                System.Diagnostics.Debug.WriteLine(sb.ToString());
            }
        }

        private static void Dump6X6(long value)
        {
            System.Diagnostics.Debug.WriteLine(value);
            //  00 01 02 03 04 05 
            //  08             13 
            //  16             21 
            //  24             29 
            //  32             37 
            //  40 41 42 43 44 45
            for (int y = 0; y < 6; y++)
            {
                StringBuilder sb = new StringBuilder();
                for (int x = 0; x < 6; x++)
                {
                    int shift = y*8 + x;
                    long v = (value >> shift) & 1;
                    sb.Append(v);
                }
                System.Diagnostics.Debug.WriteLine(sb.ToString());
            }
        }
    }
}
