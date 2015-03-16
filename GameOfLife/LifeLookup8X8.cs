using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife
{
    public class LifeLookup8X8 // data are stored in a 64 bits
    {
        // real data are in the big 8x8 square, surrounded by neighbours in other cell
        // small 2x2 square represents data used in lookup table
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
        // 21  00 01 04 05 08 09 12 13  00
        //  <---TOP---> 4 bits
        //  LT  2x2  RT 1+2+1 bits
        //  LB  2x2  RB 1+2+1 bits
        //  <--BOTTOM--> 4 bits
        // lookup is built using following order (16 bits)
        //  2x2 on 4 bits | top neighbours on 4 bits | LT on 1 bit | RT on 1 bit | LB on 1 bit | RB on 1 bit | bottom neighbours on 4 bits
        // lookup results is stored in the same order
        // this allow faster injection/extraction from/to original
        // 16 sub-2x2 blocks with neighbours, some samples:
        //
        // 00 01 02 03 | 63 42 43 46 | 21 | 04 | 23 | 06 | 29 08 09 12
        // <   cur   >   ^  <  y-1 >   x-1  cur  x-1  cur x-1 < cur  >
        //               |__ x-1,y-1
        //
        // 04 05 06 07 | 43 46 47 58 | 01 | 16 | 03 | 18 | 09 12 13 24
        // <   cur   >   <   y-1   >   cur  cur  cur  cur  <   cur   >
        //
        // 08 09 10 11 | 23 02 03 06 | 29 | 12 | 31 | 14 | 53 32 33 36
        // <   cur   >  x-1 <  cur >  x-1   cur  x-1  cur x-1 <  cur >
        //
        // 12 13 14 15 | 03 06 07 18 | 09 | 24 | 11 | 26 | 33 36 37 48
        // <                          cur                            >
        //
        // each sub-2x2 is converted to 4x4 (by adding neighbours), 'transformed' with lookup table, then sub-2x2 from lookup is injected in result

        private readonly int _sizeX;
        private readonly int _sizeY;
        private readonly int _length;
        private ulong[] _current;
        private ulong[] _next;
        private readonly NeighbourLookupUnnaturalOrder _lookup;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Generation { get; private set; }
        public Rule Rule { get; private set; }
        public Boundary Boundary { get; private set; }

        public int Population
        {
            get
            {
                ulong count = 0;
                for (int i = 0; i < _length; i++)
                {
                    ulong value = _current[i];
                    // count bit (naive method)
                    // could use http://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetNaive 
                    // or http://stackoverflow.com/questions/2709430/count-number-of-bits-in-a-64-bit-ulong-big-integer
                    for (int bit = 0; bit < 64; bit++)
                        count += (value >> bit) & 1;
                }
                return  (int)count;
            }
        }

        public LifeLookup8X8(int width, int height, Rule rule, Boundary boundary)
        {
            if (width % 8 != 0)
                throw new ArgumentException("Width must be a multiple of 8", "width");
            if (height % 8 != 0)
                throw new ArgumentException("Height must be a multiple of 8", "height");

            Width = width;
            Height = height;
            Rule = rule;
            Boundary = boundary;

            _lookup = new NeighbourLookupUnnaturalOrder(rule);

            _sizeX = width/8;
            _sizeY = height/8;
            _length = _sizeX*_sizeY;
            _current = new ulong[_length];
            _next = new ulong[_length];

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

        public ulong ComputeNewValue(int i, ulong value)
        {
            return 0; // TODO
        }
    }
}
