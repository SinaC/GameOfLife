using System;

namespace GameOfLife
{
    public interface ILife
    {
        int Generation { get; }
        int Population { get; }
        Rule Rule { get; }

        void Reset();
        void Set(int x, int y);

        void NextGeneration();

        void GetView(int minX, int minY, int maxX, int maxY, bool[,] view);
        Tuple<int, int, int, int> GetMinMaxIndexes(); // minX, minY, maxX, maxY
    }
}
