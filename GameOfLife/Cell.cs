namespace GameOfLife
{
    internal class Cell
    {
        private const int NoGeneration = -1;
        private const int NoPlayerId = -1;

        public static readonly Cell NullCell = new Cell
        {
            Generation = NoGeneration,
            PlayerId = NoPlayerId
        };

        public int Generation { get; set; }
        public int PlayerId { get; set; }

        public Cell()
        {
            Generation = NoGeneration;
            PlayerId = NoPlayerId;
        }

        public bool IsEmpty
        {
            get { return Generation == NoGeneration && PlayerId == NoPlayerId; }
        }

        public void Death()
        {
            Generation = NoGeneration;
            PlayerId = NoPlayerId;
        }

        public void Born(int playerId)
        {
            Generation = 0;
            PlayerId = playerId;
        }

        public void Survived()
        {
            Generation++;
        }

        public override string ToString()
        {
            return IsEmpty ? "." : "*";
        }
    }
}
