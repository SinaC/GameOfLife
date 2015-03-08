namespace GameOfLife
{
    public abstract class Boundary
    {
        // perform x + stepX and return computed value (-1 if not valid)
        public abstract int AddStepX(int x, int stepX);
        public abstract int AddStepY(int y, int stepY);
    }

    public class NoBoundary : Boundary
    {
        public override int AddStepX(int x, int stepX)
        {
            return x + stepX;
        }

        public override int AddStepY(int y, int stepY)
        {
            return y + stepY;
        }
    }

    public class ToroidalBoundary : Boundary
    {
        public int LowX { get; private set; }
        public int LowY { get; private set; }
        public int HighX { get; private set; }
        public int HighY { get; private set; }

        public ToroidalBoundary(int lowX, int lowY, int highX, int highY)
        {
            LowX = lowX;
            LowY = lowY;
            HighX = highX;
            HighY = highY;
        }

        public override int AddStepX(int x, int stepX)
        {
            x += stepX;
            if (x < LowX)
                x += LowX;
            else if (x > HighX)
                x -= HighX;
            return x;
        }

        public override int AddStepY(int y, int stepY)
        {
            y += stepY;
            if (y < LowY)
                y += LowY;
            else if (y > HighY)
                y -= HighY;
            return y;
        }
    }

    public class FixedBoundary : Boundary
    {
        public int LowX { get; private set; }
        public int LowY { get; private set; }
        public int HighX { get; private set; }
        public int HighY { get; private set; }

        public FixedBoundary(int lowX, int lowY, int highX, int highY)
        {
            LowX = lowX;
            LowY = lowY;
            HighX = highX;
            HighY = highY;
        }

        public override int AddStepX(int x, int stepX)
        {
            x += stepX;
            if (x < LowX || x > HighX)
                return -1; // out of bounds
            return x;
        }

        public override int AddStepY(int y, int stepY)
        {
            y += stepY;
            if (y < LowY || y > HighY)
                return -1; // out of bounds
            return y;
        }
    }
}
