using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife
{
    public class LifeLookup4X4
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

        // stored in a 64 bits, real data are stored at position
        // 09 10 11 12
        // 17 18 19 20
        // 25 26 27 28
        // 33 34 35 36

        // when computing new value, we initialize a 64 bits with original value (4x4) and set neighbours
        // in bits 
        //  00 01 02 03 04 05 
        //  08             13 
        //  16             21 
        //  24             29 
        //  32             37 
        //  40 41 42 43 44 45
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
        // this allows faster 2x2 lookup because neighbourhood will already be stored and we don't have to access other cell anymore

        // compute each 2x2 block
        // loop on value 09, 11, 25, 27
        //   build a 16 bit value to use on lookup, real data are stored in 5, 6, 9, 10 and neighbours in 0, 1, 2, 3, 4, 7, 8, 11, 12, 13, 14, 15
        //      00 01 02 03
        //      04 05 06 07
        //      08 09 10 11
        //      12 13 14 15
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

        private int _sizeX;
        private int _sizeY;
        private int _length;
        private long[] _current;
        private NeighbourLookup _lookup;

        private readonly static int[] TopLeftOfSub2X2 = {9, 11, 25, 27};

        public int Width { get; private set; }
        public int Height { get; private set; }
        public Rule Rule { get; private set; }

        public LifeLookup4X4(int width, int height, Rule rule)
        {
            Width = width;
            Height = height;
            Rule = rule;

            _lookup = new NeighbourLookup(rule);

            _sizeX = width/4; // TODO: must be a multiple of 4
            _sizeY = height/4;
            _length = _sizeX*_sizeY;
            _current = new long[_length];
        }

        // TODO: optimize  
        //      ((x >> y) & 1) << z   into  (x & mask) << (y-z)  with y > z
        //      ((x >> y) & 1) << z   into  (x & mask) >> (z-y)  with y < z
        public long ComputeNewValue(int i, long value)
        {
            long topLeft = _current[(_length + i - _sizeX - 1)%_length];
            long top = _current[(_length + i - _sizeX) % _length];
            long topRight = _current[(_length + i - _sizeX + 1) % _length];
            long left = _current[(_length + i - 1) % _length];
            long right = _current[(_length + i + 1) % _length];
            long bottomLeft = _current[(_length + i + _sizeX - 1) % _length];
            long bottom = _current[(_length + i + _sizeX) % _length];
            long bottomRight = _current[(_length + i + _sizeX + 1) % _length];

            //initialize a 64 bits with original value (4x4) and set neighbours
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
            lookup6X6 |= ((right >> 17) & 1) << 20;
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

            // compute each 2x2 block
            long result = 0;
            foreach(int subIndex in TopLeftOfSub2X2)
            {
                //   build a 16 bit value to use on lookup, real data are stored in 5, 6, 9, 10 and neighbours in 0, 1, 2, 3, 4, 7, 8, 11, 12, 13, 14, 15
                //      00 01 02 03
                //      04 05 06 07
                //      08 09 10 11
                //      12 13 14 15
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
                long lookup2X2 = // stored in a 64 bits to avoid cast on short on each step
                    // inner 2x2
                    ((lookup6X6 >> subIndex) & 1) << 5 |
                    ((lookup6X6 >> subIndex + 1) & 1) << 6 |
                    ((lookup6X6 >> subIndex + 8) & 1) << 9 |
                    ((lookup6X6 >> subIndex + 9) & 1) << 10;
                // neighbours
                lookup2X2 |= ((lookup6X6 >> (subIndex - 9)) & 0xF) << 0; // top
                lookup2X2 |= ((lookup6X6 >> (subIndex - 1)) & 1) << 4; // left top
                lookup2X2 |= ((lookup6X6 >> (subIndex + 7)) & 1) << 8; // left bottom
                lookup2X2 |= ((lookup6X6 >> (subIndex + 2)) & 1) << 7; // right top
                lookup2X2 |= ((lookup6X6 >> (subIndex + 10)) & 1) << 11; // right bottom
                lookup2X2 |= ((lookup6X6 >> (subIndex + 15)) & 0xF) << 12; // bottom   

                int result2X2 = _lookup[(int) lookup2X2];

                // TODO: inject result2x2 into result  (only use bit 5, 6, 9, 10 from result2x2)
            }

            return result;
        }
    }
}
