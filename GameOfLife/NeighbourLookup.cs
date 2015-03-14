namespace GameOfLife
{
    // Lookup table 
    // Precomputing rules on each 2x2 surrounded by a border of 1 -> 4x4
    // bit 5, 6, 9, 10: 2x2
    // bit 0, 1, 2, 3: top border
    // bit 0, 4, 8, 12: left border
    // bit 3, 7, 11, 15: right border
    // bit 12, 13, 14, 15: bottom border
    //      00  01  02  03
    //          ------
    //      04 |05  06| 07
    //
    //      08 |09  10| 11
    //          ------
    //      12  13  14  15
    public class NeighbourLookup
    {
        private readonly Rule _rule;
        private readonly int[] _neighbourLookup;

        public NeighbourLookup(Rule rule)
        {
            _rule = rule;
            _neighbourLookup = new int[65536];
            BuildLookup();
        }

        public int this[int value/*4x4*/]
        {
            get { return _neighbourLookup[value]; }
        }

        public long this[long value/*4x4*/]
        {
            get { return _neighbourLookup[value]; }
        }

        private void BuildLookup()
        {
            for (int i = 0; i < 65536; i++)
                _neighbourLookup[i] = ApplyRulesOnBlock(i);
        }

        // apply rules on inner 2x2 using borders to compute neighbours
        private int ApplyRulesOnBlock(int value/*4x4*/)
        {
            // compute neighbours
            // neighbours of 5 are 0, 1, 2, 4, 6, 8, 9, 10
            // neighbours of 6 are 1, 2, 3, 5, 7, 9, 10, 11
            // neighbours of 9 are 4, 5, 6, 8, 10, 12, 13, 14
            // neighbours of 10 are 5, 6, 7, 9, 11, 13, 14, 15
            int neighbour5 = CountNeighbours(value, 0, 1, 2, 4, 6, 8, 9, 10);
            int neighbour6 = CountNeighbours(value, 1, 2, 3, 5, 7, 9, 10, 11);
            int neighbour9 = CountNeighbours(value, 4, 5, 6, 8, 10, 12, 13, 14);
            int neighbour10 = CountNeighbours(value, 5, 6, 7, 9, 11, 13, 14, 15);

            // apply rules on 2x2
            int topLeft5 = ApplyRulesOnCell((value >> 5) & 1, neighbour5);
            int topRight6 = ApplyRulesOnCell((value >> 6) & 1, neighbour6);
            int bottomLeft9 = ApplyRulesOnCell((value >> 9) & 1, neighbour9);
            int bottomRight10 = ApplyRulesOnCell((value >> 10) & 1, neighbour10);

            // build result: clear bit 5, 6, 9, 10 and set new bit 5, 6, 9, 10
            int result = (value & ~((1 << 5) | (1 << 6) | (1 << 9) | (1 << 10)))
                         | (topLeft5 << 5) | (topRight6 << 6) | (bottomLeft9 << 9) | (bottomRight10 << 10);

            return result;
        }

        private int ApplyRulesOnCell(int bit/*1x1*/, int neighbourCount)
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
        private int CountNeighbours(int value, int n0, int n1, int n2, int n3, int n4, int n5, int n6, int n7)
        {
            return ((value >> n0) & 1) + ((value >> n1) & 1) + ((value >> n2) & 1) + ((value >> n3) & 1) + ((value >> n4) & 1) + ((value >> n5) & 1) + ((value >> n6) & 1) + ((value >> n7) & 1);
        }
    }
}
