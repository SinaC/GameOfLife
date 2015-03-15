using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife
{
    class LifeLookup8X8 // data are stored in a 64 bits
    {
        // real data are in the big 8x8 square, surrounded by neighbours in other cell
        // small 2x2 square represents data used in lookup table
        // 63  50 51 54 55 58 59 62 63  50
        //     -----------------------
        // 13 |00 01|04 05|08 09|12 13| 00
        // 15 |02 03|06 07|10 11|14 15| 02
        //     ----- ----- ----- -----
        // 29 |16 17|20 21|24 25|28 29| 16
        // 31 |18 19|22 23|26 27|30 31| 18
        //     ----- ----- ----- -----
        // 45 |32 33|36 37|40 41|44 45| 32
        // 47 |34 35|38 39|42 43|46 47| 34
        //     ----- ----- ---- ------
        // 61 |48 49|52 53|56 57|60 61| 48
        // 63 |50 51|54 55|58 59|62 63| 50
        //     -----------------------
        // 63  00 01 04 05 08 09 12 13  00
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
        // 00 01 02 03 | 63 50 51 54 | 13 | 04 | 15 | 06 | 29 16 17 20
        // <   cur   >   ^  <  y-1 >   x-1  cur  x-1  cur x-1 < cur  >
        //               |__ x-1,y-1
        //
        // 04 05 06 07 | 51 54 55 58 | 01 | 08 | 03 | 10 | 17 20 21 24
        // <   cur   >   <   y-1   >   cur  cur  cur  cur  <   cur   >
        //
        // 16 17 18 19 | 15 02 03 06 | 29 | 20 | 31 | 22 | 45 32 33 36
        // <   cur   >  x-1 <  cur >  x-1   cur  x-1  cur x-1 <  cur >
        //
        // 20 21 22 23 | 03 06 07 10 | 17 | 24 | 19 | 26 | 33 36 37 40
        // <                          cur                            >
        //
        // each sub-2x2 is converted to 4x4 (by adding neighbours), 'transformed' with lookup table, then sub-2x2 from lookup is injected in result
    }

    // better idea
    // 00 01 04 05 16 17 20 21
    // 02 03 06 07 18 19 22 23
    // ... etc
    // this order allows faster 4x4 tests
}
