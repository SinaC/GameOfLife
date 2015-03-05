namespace GameOfLife
{
    internal class CellSparse
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Generation { get; private set; }
        public int PlayerId { get; private set; }

        public CellSparse(int x, int y, int playerId)
        {
            X = x;
            Y = y;
            PlayerId = playerId;

            Generation = 0;
        }

        public void Survived()
        {
            Generation++;
        }

        public override string ToString()
        {
            return "*";
        }
    }
}
