using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife
{
    class LifeLookup8X8 // same as 2x2 in a 64 bits
    {
        // one cell contains a 64 bits representing a 8x8 square
        // 00 01|02 03|04 05|06 07
        // 08 09|10 11|12 13|14 15
        // ----- ----- ----- -----
        // 16 17|18 19|20 21|22 23
        // 24 25|26 27|28 29|30 31
        // ----- ----- ----- -----
        // 32 33|34 35|36 37|38 39
        // 40 41|42 43|44 45|46 47
        // ----- ----- ----- -----
        // 48 49|50 51|52 53|54 55
        // 56 57|58 59|60 61|62 63

        // lookup uses 2x2 + neighbours

        // sample for block 18 19
        //                  26 27
        // 09 10 11 12
        // 17 18 19 20  will be used on lookup to get 18, 19
        // 25 26 27 28                                26, 27  new values
        // 33 34 35 36

        // sample for block 00 01  on x,y
        //                  08 09
        // 
        // x-1,y-1  63|56 57 58  x,y-1
        //          -- --------
        //          07|00 01 02
        // x-1,y    15|08 09 10  x,y
        //          23|16 17 18
    }
}
