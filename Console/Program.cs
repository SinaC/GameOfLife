using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameOfLife;

namespace Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            int[] survivals = new[] { 3, 4 };
            int[] births = new[] { 2 };
            Rule rule = new Rule(survivals, births);
            LifeHex life = new LifeHex(1, rule, false);
            life.Set(-1,0,0);
            life.Set(+1, -1, 0);
            life.Set(0, +1, 0);

            for (int i = 0; i < 100; i++)
            {
                Display(life);
                life.NextGeneration();
                System.Console.ReadLine();
                System.Threading.Thread.Sleep(100);
            }
        }

        private static void Main2(string[] args)
        {
            int[] survivals = new[] {2, 3};
            int[] births = new[] {3};
            Rule rule = new Rule(survivals, births);
            Life life = new Life(10, 10, rule, false);
            life.Set(1,0,0);
            life.Set(2,1,0);
            life.Set(0,2,0);
            life.Set(1, 2, 0);
            life.Set(2, 2, 0);

            for (int i = 0; i < 100; i++)
            {
                Display(life);
                life.NextGeneration();
                //System.Console.ReadLine();
                System.Threading.Thread.Sleep(100);
            }
        }

        private static void Display(LifeHex life)
        {
            System.Console.Clear();

            System.Console.WriteLine("Generation: {0}  Population: {1}", life.Generation, life.PopulationCount);
            int[,] cells = life.GetCells();
            int diagonal = 2*life.Radius+1;
            for (int y = 0; y < diagonal; y++)
            {
                StringBuilder sb = new StringBuilder(diagonal);
                for (int x = 0; x < diagonal; x++)
                    //sb.Append(cells[x, y] == -1 ? "." : cells[x,y].ToString());
                    sb.Append(cells[x, y] >= 0 ? "*" : ".");
                System.Console.WriteLine(sb.ToString());
            }
        }

        private static void Display(Life life)
        {
            System.Console.Clear();

            System.Console.WriteLine("Generation: {0}  Population: {1}", life.Generation, life.PopulationCount);
            int[,] cells = life.GetCells();
            for (int y = 0; y < life.Height; y++)
            {
                StringBuilder sb = new StringBuilder(life.Width);
                for (int x = 0; x < life.Width; x++)
                    //sb.Append(cells[x, y] == -1 ? "." : cells[x,y].ToString());
                    sb.Append(cells[x, y]>=0 ? "*" : ".");
                System.Console.WriteLine(sb.ToString());
            }
        }
    }
}
