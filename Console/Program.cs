using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameOfLife;

namespace Console
{
    internal class Program
    {
        private const int SquareX = 120;
        private const int SquareY = 80;

        private static void Main(string[] args)
        {
            System.Console.SetWindowSize(SquareX + 1, SquareY + 2);
            System.Console.SetBufferSize(SquareX+1, SquareY + 2);
            //bool[,] cells = ReadRLE(@"d:\github\gameoflife\rle\glider.rle");

            //TestSquare();
            //TestSparse();
            TestRectangularOptimized();
        }

        private static void TestRectangularOptimized()
        {
            LifeRectangularOptimized life = new LifeRectangularOptimized(SquareX, SquareY);
            // Glider
            life.Set(2,1);
            life.Set(3,2);
            life.Set(1,3);
            life.Set(2, 3);
            life.Set(3, 3);

            bool pause = true;
            while (true)
            {
                DisplayRectangularOptimized(life);
                life.NextGeneration();
                if (System.Console.KeyAvailable || pause)
                {
                    ConsoleKeyInfo keyInfo = System.Console.ReadKey();
                    if (keyInfo.Key == ConsoleKey.X)
                        break;
                    else if (keyInfo.Key == ConsoleKey.Spacebar)
                        pause = !pause;
                }
                System.Threading.Thread.Sleep(10);
            }
        }

        private static void DisplayRectangularOptimized(LifeRectangularOptimized life)
        {
            //System.Console.Clear();
            System.Console.SetCursorPosition(0, 0);

            System.Console.WriteLine("Generation: {0:#####}  Population: {1:#####}", life.Generation, life.PopulationCount);
            int[,] cells = life.GetCells();
            for (int y = 0; y < life.Height; y++)
            {
                StringBuilder sb = new StringBuilder(life.Width);
                for (int x = 0; x < life.Width; x++)
                    sb.Append(cells[x, y] > 0 ? "*" : " ");
                System.Console.WriteLine(sb.ToString());
            }
        }

        private static void TestSparse()
        {
            int[] survivals = new[] { 2, 3 };
            int[] births = new[] { 3 };
            Rule rule = new Rule(survivals, births);
            LifeSparse sparse = new LifeSparse(rule);
            //sparse.Set(1, 0, 0);
            //sparse.Set(2, 1, 0);
            //sparse.Set(0, 2, 0);
            //sparse.Set(1, 2, 0);
            //sparse.Set(2, 2, 0);

            //List<Tuple<int,int>> cells = ReadRLE(@"d:\github\gameoflife\rle\natural-lwss.rle");
            //List<Tuple<int, int>> cells = ReadRLE(@"d:\github\gameoflife\rle\iwona.rle");
            List<Tuple<int, int>> cells = ReadRLE(@"d:\github\gameoflife\rle\gosper glider gun.rle");
            //List<Tuple<int, int>> cells = ReadRLE(@"d:\github\gameoflife\rle\pulsars-big-s.rle");
            //List<Tuple<int, int>> cells = ReadRLE(@"d:\github\gameoflife\rle\glider.rle");
            //List<Tuple<int, int>> cells = ReadRLE(@"d:\github\gameoflife\rle\lightweightspaceship.rle");
            //List<Tuple<int, int>> cells = ReadRLE(@"d:\github\gameoflife\rle\stripey.rle");
            //List<Tuple<int, int>> cells = ReadRLE(@"d:\github\gameoflife\rle\puffer-train.rle");
            //List<Tuple<int, int>> cells = ReadRLE(@"d:\github\gameoflife\rle\make-lightbulb.rle"); // BUG !!! 2nd and 3rd gliders on column 16->19 should have 2 rows between them instead of one
            
            
            foreach (Tuple<int, int> cell in cells)
                sparse.Set(cell.Item1, cell.Item2, 0);

            bool pause = true;
            while (true)
            {
                DisplaySparse(sparse);
                sparse.NextGeneration();
                if (System.Console.KeyAvailable || pause)
                {
                    ConsoleKeyInfo keyInfo = System.Console.ReadKey();
                    if (keyInfo.Key == ConsoleKey.X)
                        break;
                    else if (keyInfo.Key == ConsoleKey.Spacebar)
                        pause = !pause;
                }
                System.Threading.Thread.Sleep(10);
            }
        }

        private static void DisplaySparse(LifeSparse sparse)
        {
            Tuple<int, int, int, int> minmax = sparse.GetMinMaxIndexes();

            //System.Console.Clear();
            System.Console.SetCursorPosition(0, 0);
            System.Console.WriteLine("Generation: {0:#####}  Population: {1:#####}  X:{2:###}->{3:###} Y:{4:###}->{5:###}", sparse.Generation, sparse.PopulationCount, minmax.Item1, minmax.Item3, minmax.Item2, minmax.Item4);

            int minx = 0;
            int maxx = 100;
            int miny = 0;
            int maxy = 79;
            int[,] cells = sparse.GetView(minx, miny, maxx, maxy);
            for (int y = 0; y <= maxy-miny; y++)
            {
                StringBuilder sb = new StringBuilder(11);
                for (int x = 0; x <= maxx - minx; x++)
                {
                    //sb.Append(cells[x, y] == -1 ? "." : cells[x,y].ToString());
                    //sb.Append(cells[x, y] >= 0 ? "*" : ".");
                    string display = "*";
                    if (cells[x,y] < 0)
                    {
                        if (x == 0)
                            display = (y%10).ToString(CultureInfo.InvariantCulture);
                        else if (y == 0)
                            display = (x%10).ToString(CultureInfo.InvariantCulture);
                        else
                            display = " ";
                    }
                    sb.Append(display);
                }
                System.Console.WriteLine(sb.ToString());
            }
        }

        private static void TestSquare()
        {
            int[] survivals = new[] {2, 3};
            int[] births = new[] {3};
            Rule rule = new Rule(survivals, births);
            Life life = new Life(SquareX, SquareY, rule, true);
            // Glider
            //life.Set(1,0,0);
            //life.Set(2,1,0);
            //life.Set(0,2,0);
            //life.Set(1, 2, 0);
            //life.Set(2, 2, 0);
            // Lightweight spaceship
            //life.Set(6,4,0);
            //life.Set(7, 4, 0);
            //life.Set(4, 5, 0);
            //life.Set(5, 5, 0);
            //life.Set(7, 5, 0);
            //life.Set(8, 5, 0);
            //life.Set(4, 6, 0);
            //life.Set(5, 6, 0);
            //life.Set(6, 6, 0);
            //life.Set(7, 6, 0);
            //life.Set(5, 7, 0);
            //life.Set(6, 7, 0);
            // R-pentomino
            //life.Set(life.Width / 2, life.Height / 2, 0);
            //life.Set(life.Width / 2 + 1, life.Height / 2, 0);
            //life.Set(life.Width / 2 - 1, life.Height / 2 + 1, 0);
            //life.Set(life.Width / 2, life.Height / 2 + 1, 0);
            //life.Set(life.Width / 2, life.Height / 2 + 2, 0);
            // Diehard
            //life.Set(life.Width/2+3, life.Height/2-1, 0);
            //life.Set(life.Width / 2 -3, life.Height / 2, 0);
            //life.Set(life.Width / 2 - 2, life.Height / 2, 0);
            //life.Set(life.Width / 2 - 2, life.Height / 2+1, 0);
            //life.Set(life.Width / 2 +2, life.Height / 2 + 1, 0);
            //life.Set(life.Width / 2 + 3, life.Height / 2 + 1, 0);
            //life.Set(life.Width / 2 + 4, life.Height / 2 + 1, 0);

            List<Tuple<int, int>> cells = ReadRLE(@"d:\github\gameoflife\rle\gosper glider gun.rle");
            //bool[,] cells = ReadRLE(@"d:\github\gameoflife\rle\natural-lwss.rle");
            //for (int y = 0; y < cells.GetLength(1); y++)
            //    for (int x = 0; x < cells.GetLength(0); x++)
            //        if (cells[x, y])
            //            life.Set(50 + x, 30 + y, 0);
            foreach(Tuple<int,int> cell in cells)
                life.Set(cell.Item1, cell.Item2, 0);

            while (true)
            {
                DisplaySquare(life);
                life.NextGeneration();
                if (System.Console.KeyAvailable)
                {
                    System.Console.ReadKey();
                    break;
                }
                System.Threading.Thread.Sleep(10);
            }
        }

        private static void DisplaySquare(Life life)
        {
            //System.Console.Clear();
            System.Console.SetCursorPosition(0,0);

            System.Console.WriteLine("Generation: {0:#####}  Population: {1:#####}", life.Generation, life.PopulationCount);
            int[,] cells = life.GetCells();
            for (int y = 0; y < life.Height; y++)
            {
                StringBuilder sb = new StringBuilder(life.Width);
                for (int x = 0; x < life.Width; x++)
                    //sb.Append(cells[x, y] == -1 ? "." : cells[x,y].ToString());
                    sb.Append(cells[x, y]>=0 ? "*" : " ");
                System.Console.WriteLine(sb.ToString());
            }
        }

        private static void TestHex()
        {
            int[] survivals = new[] { 3, 4 };
            int[] births = new[] { 2 };
            Rule rule = new Rule(survivals, births);
            LifeHex life = new LifeHex(4, rule, false);
            // 2-step oscillator
            //life.Set(-1,0,0);
            //life.Set(+1, -1, 0);
            //life.Set(0, +1, 0);
            // 4-step oscillator
            life.Set(-1, +1, 0);
            life.Set(-1, 0, 0);
            life.Set(0, -1, 0);
            life.Set(+1, -1, 0);

            for (int i = 0; i < 100; i++)
            {
                DisplayHex(life);
                life.NextGeneration();
                //System.Console.ReadLine();
                System.Threading.Thread.Sleep(100);
            }
        }

        private static void DisplayHex(LifeHex life)
        {
            System.Console.Clear();

            System.Console.WriteLine("Generation: {0:#####}  Population: {1:#####}", life.Generation, life.PopulationCount);
            int[,] cells = life.GetCells();
            int diagonal = 2 * life.Radius + 1;
            for (int y = 0; y < diagonal; y++) // TODO: display only valid cells (not wasted ones)
            {
                StringBuilder sb = new StringBuilder(diagonal);
                for (int x = 0; x < diagonal; x++)
                    sb.Append(cells[x, y] >= 0 ? "*" : ".");
                System.Console.WriteLine(sb.ToString());
            }
        }

        private static List<Tuple<int,int>> ReadRLE(string filename)
        {
            //http://www.conwaylife.com/wiki/Run_Length_Encoded
            using(TextReader tr = new StreamReader(filename))
            {
                string line;
                // Skip comment
                while(true)
                {
                    line = tr.ReadLine();
                    if (String.IsNullOrEmpty(line) || !line.StartsWith("#"))
                        break;
                }
                if (String.IsNullOrEmpty(line))
                    return null;
                // x = 3, y = 3, rule = B3/S23
                string[] headers = line.Split(',').Select(h => h.Trim()).ToArray();
                if (headers.Length < 2)
                    return null;
                int x, y;
                if (!int.TryParse(headers[0].Substring(4), out x))
                    return null;
                if (!int.TryParse(headers[1].Substring(4), out y))
                    return null;
                // x, y not used
                // read cells
                string content = tr.ReadToEnd().Replace(Environment.NewLine,String.Empty);
                // create cells map
                char[] ob = new[] { 'o', 'b' };
                // parse cells
                string[] rows = content.Split('$', '!').Where(row => !String.IsNullOrEmpty(row)).ToArray();
                List<Tuple<int, int>> cells = new List<Tuple<int,int>>();
                int rowIndex = 0;
                foreach(string row in rows)
                {
                    int parseIndex = 0;
                    int columnIndex = 0;

                    while (true)
                    {
                        int obIndex = row.IndexOfAny(ob, parseIndex);
                        int occurence;
                        if (obIndex == parseIndex) // occurence 1
                            occurence = 1;
                        else if (obIndex == -1)
                        {
                            int rowSkip;
                            if (int.TryParse(row.Substring(parseIndex), out rowSkip))
                                rowIndex += rowSkip;
                            else
                                rowIndex++;
                            break;
                        }
                        else
                        {
                            if (!int.TryParse(row.Substring(parseIndex, obIndex - parseIndex), out occurence))
                                return null;
                        }

                        //for (int i = 0; i < occurence; i++ )
                        //    System.Diagnostics.Debug.Write(row[obIndex] == 'o' ? "0" : " ");

                        if (row[obIndex] == 'o')
                        {
                            // alive
                            for (int i = 0; i < occurence; i++)
                                cells.Add(new Tuple<int, int>(columnIndex+i, rowIndex));
                        }
                        parseIndex = obIndex + 1;
                        columnIndex += occurence;
                        if (parseIndex >= row.Length)
                        {
                            rowIndex++;
                            break;
                        }
                    }
                    //System.Diagnostics.Debug.WriteLine("");
                }

                return cells;
            }
        }
    }
}
