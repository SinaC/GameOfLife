namespace GameOfLife
{
    public abstract class Boundary
    {
        // perform x + stepX and return computed value (returns false if impossible move)
        public abstract bool AddStepX(int x, int stepX, out int newX);
        public abstract bool AddStepY(int y, int stepY, out int newY);
        public abstract bool IsXValid(int x);
        public abstract bool IsYValid(int y);
    }

    public class NoBoundary : Boundary
    {
        public override bool AddStepX(int x, int stepX, out int newX)
        {
            newX = x + stepX;
            return true;
        }

        public override bool AddStepY(int y, int stepY, out int newY)
        {
            newY = y + stepY;
            return true;
        }

        public override bool IsXValid(int x)
        {
            return true;
        }

        public override bool IsYValid(int y)
        {
            return true;
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

        public override bool AddStepX(int x, int stepX, out int newX)
        {
            // TODO: no conditionnal use modulo
            newX = x + stepX;
            if (newX < LowX)
                newX = newX + (HighX - LowX + 1);
            else if (newX > HighX)
                newX = newX - (HighX - LowX + 1);
            return true;
        }

        public override bool AddStepY(int y, int stepY, out int newY)
        {
            // TODO: no conditionnal use modulo
            newY = y + stepY;
            if (newY < LowY)
                newY = newY + (HighY - LowY + 1);
            else if (newY > HighY)
                newY = newY - (HighY - LowY + 1);
            return true;
        }

        public override bool IsXValid(int x)
        {
            return true;
        }

        public override bool IsYValid(int y)
        {
            return true;
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

        public override bool AddStepX(int x, int stepX, out int newX)
        {
            newX = x + stepX;
            return newX >= LowX && newX <= HighX;
        }

        public override bool AddStepY(int y, int stepY, out int newY)
        {
            newY = y + stepY;
            return newY >= LowX && newY <= HighX;
        }

        public override bool IsXValid(int x)
        {
            return x >= LowX && x <= HighX;
        }

        public override bool IsYValid(int y)
        {
            return y >= LowX && y <= HighX;
        }
    }
}
