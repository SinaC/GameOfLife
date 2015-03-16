namespace GameOfLife
{
    // Lookup table 
    // Precomputing rules on each 2x2 surrounded by a border of 1 -> 4x4
    //  2x2 on 4 bits | top neighbours on 4 bits | LT on 1 bit | RT on 1 bit | LB on 1 bit | RB on 1 bit | bottom neighbours on 4 bits
    //
    //  <---TOP---> 4 bits
    //  LT  2x2  RT 1+2+1 bits
    //  LB  2x2  RB 1+2+1 bits
    //  <--BOTTOM--> 4 bits
    //
    //      04  05  06  07
    //          ------
    //      08 |00  01| 09
    //      10 |02  03| 11
    //          ------
    //      12  13  14  15
    public class NeighbourLookupUnnaturalOrder
    {
        private readonly Rule _rule;
        private readonly uint[] _neighbourLookup;

        public NeighbourLookupUnnaturalOrder(Rule rule)
        {
            _rule = rule;
            _neighbourLookup = new uint[65536];
            BuildLookup();
        }

        public uint this[uint value /*4x4*/]
        {
            get { return _neighbourLookup[value]; }
        }

        public ulong this[ulong value /*4x4*/]
        {
            get { return _neighbourLookup[value]; }
        }

        private void BuildLookup()
        {
            for (uint i = 0; i < 65536; i++)
                _neighbourLookup[i] = ApplyRulesOnBlock(i);
        }

        // apply rules on inner 2x2 using borders to compute neighbours
        private uint ApplyRulesOnBlock(uint value /*4x4*/)
        {
            // compute neighbours
            // neighbours of 0 are 4, 5, 6, 8, 1, 10, 2, 3
            // neighbours of 1 are 5, 6, 7, 0, 9, 2, 3, 11
            // neighbours of 2 are 8, 0, 1, 10, 3, 12, 13, 14
            // neighbours of 3 are 0, 1, 9, 2, 11, 13, 14, 15
            uint neighbour0 = CountNeighbours(value, 4, 5, 6, 8, 1, 10, 2, 3);
            uint neighbour1 = CountNeighbours(value, 5, 6, 7, 0, 9, 2, 3, 11);
            uint neighbour2 = CountNeighbours(value, 8, 0, 1, 10, 3, 12, 13, 14);
            uint neighbour3 = CountNeighbours(value, 0, 1, 9, 2, 11, 13, 14, 15);

            // apply rules on 2x2
            uint topLeft0 = ApplyRulesOnCell((value >> 0) & 1, neighbour0);
            uint topRight1 = ApplyRulesOnCell((value >> 1) & 1, neighbour1);
            uint bottomLeft2 = ApplyRulesOnCell((value >> 2) & 1, neighbour2);
            uint bottomRight3 = ApplyRulesOnCell((value >> 3) & 1, neighbour3);

            // build result: clear bits 0, 1, 2, 3 and set new value
            uint result = (value & 0xF)
                          | (topLeft0 << 0) | (topRight1 << 1) | (bottomLeft2 << 2) | (bottomRight3 << 3);

            return result;
        }

        private uint ApplyRulesOnCell(uint bit /*1x1*/, uint neighbourCount)
        {
            if (bit == 0)
            {
                if (_rule.Birth(neighbourCount))
                    return 1;
            }
            else
            {
                if (_rule.Survive(neighbourCount))
                    return 1;
            }
            return 0;
        }

        // n0 -> n7 stands for bit index of neighbour
        private uint CountNeighbours(uint value, int n0, int n1, int n2, int n3, int n4, int n5, int n6, int n7)
        {
            return ((value >> n0) & 1) + ((value >> n1) & 1) + ((value >> n2) & 1) + ((value >> n3) & 1) + ((value >> n4) & 1) + ((value >> n5) & 1) + ((value >> n6) & 1) + ((value >> n7) & 1);
        }
    }
}
