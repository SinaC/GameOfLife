using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using GameOfLife;

namespace Console
{
    internal class Program
    {
        private const int SquareX = 120;
        private const int SquareY = 80;

        private static void Main(string[] args)
        {
            System.Console.SetWindowSize(SquareX + 1, SquareY + 3);
            System.Console.SetBufferSize(SquareX + 1, SquareY + 3);

            //TestSquare();

            int[] survivals = new[] {2, 3};
            int[] births = new[] {3};
            Rule rule = new Rule(survivals, births);

            Boundary boundary = new ToroidalBoundary(-5000, -5000, 5000, 5000);

            //LifeLookup4X4 test4X4 = new LifeLookup4X4(8,8,rule);
            //const long value = ((long) 1 << 9) | ((long) 1 << 18) | ((long) 1 << 27) | ((long) 1 << 36); // a diagonal from top left to bottom right
            //long newValue = test4X4.ComputeNewValue(0, value);
            //test4X4.Set(0,0);
            //test4X4.Set(1, 1);
            //test4X4.Set(2, 2);
            //test4X4.Set(3, 3);

            LifeLookup4X4 life = new LifeLookup4X4(64, 64, rule, new ToroidalBoundary(0, 0, 15, 15));
            //LifeLookup2X2 life = new LifeLookup2X2(60, 60, rule);
            //LifeLookup1X1 life = new LifeLookup1X1(60, 60, rule);
            //LifeSparse life = new LifeSparse(rule, boundary);
            //LifeSparse2 life = new LifeSparse2(rule, boundary);
            //LifeDoubleBuffered life = new LifeDoubleBuffered(60, 60);
            //LifeNaive life = new LifeNaive(60,60,rule, false);
            TestILife(life);
        }

        private static void TestILife(ILife life)
        {
            // oscillator xxx
            //life.Set(1, 2);
            //life.Set(2, 2);
            //life.Set(3, 2);

            // glider
            //life.Set(3, 3);
            //life.Set(4, 4);
            //life.Set(5, 4);
            //life.Set(3, 5);
            //life.Set(4, 5);

            // R-pentomino
            //life.Set(20, 20);
            //life.Set(21, 20);
            //life.Set(19, 21);
            //life.Set(20, 21);
            //life.Set(20, 22);

            // Diehard
            //life.Set(20 + 3, 20 - 1);
            //life.Set(20 - 3, 20);
            //life.Set(20 - 2, 20);
            //life.Set(20 - 2, 20 + 1);
            //life.Set(20 + 2, 20 + 1);
            //life.Set(20 + 3, 20 + 1);
            //life.Set(20 + 4, 20 + 1);

            //AddRLE(life, @"d:\github\gameoflife\rle\natural-lwss.rle");
            //AddRLE(life, @"d:\github\gameoflife\rle\iwona.rle");
            AddRLE(life, @"d:\github\gameoflife\rle\gosper glider gun.rle", 5, 10);
            //AddRLE(life, @"d:\github\gameoflife\rle\pulsars-big-s.rle", 40, 30);
            //AddRLE(life, @"d:\github\gameoflife\rle\glider.rle");
            //AddRLE(life, @"d:\github\gameoflife\rle\lightweightspaceship.rle", 10, 10);
            //AddRLE(life, @"d:\github\gameoflife\rle\stripey.rle");
            //AddRLE(life, @"d:\github\gameoflife\rle\puffer-train.rle");
            //AddRLE(life, @"d:\github\gameoflife\rle\make-lightbulb.rle"); // BUG !!! 2nd and 3rd gliders on column 16->19 should have 2 rows between them instead of one
            //AddRLE(life, @"d:\github\gameoflife\rle\orthogonal c-7.rle", 10, 10);
            //AddRLE(life, @"d:\github\gameoflife\rle\b52.rle", 10, 10);
            //AddRLE(life, @"d:\github\gameoflife\rle\beacon maker.rle", 10, 10);

            AddRLE(life, @"d:\github\gameoflife\rle\eater1.rle", 56, 47); // to use with glider gun at 5,10

            bool pause = true;
            while (true)
            {
                //DisplayILife(life, '█', ' ');
                DisplayILife(life, 'O', ' ');
                //DisplayILifeView(life, 0, 0, 80, 60);
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

        private static void DisplayILife(ILife life, char cell = '*', char noCell = '.')
        {
            System.Console.SetCursorPosition(0, 0);
            System.Console.WriteLine("Generation: {0,5}  Population: {1,5}", life.Generation, life.Population);

            Tuple<int, int, int, int> minmax = life.GetMinMaxIndexes();
            bool[,] view = new bool[minmax.Item3 - minmax.Item1 + 1, minmax.Item4 - minmax.Item2 + 1];
            life.GetView(minmax.Item1, minmax.Item2, minmax.Item3, minmax.Item4, view);
            for (int y = 0; y <= minmax.Item4 - minmax.Item2; y++)
            {
                StringBuilder sb = new StringBuilder(11);
                for (int x = 0; x <= minmax.Item3 - minmax.Item1; x++)
                    sb.Append(view[x, y] ? cell : noCell);
                System.Console.WriteLine(sb.ToString());
            }
        }

        private static void DisplayILifeView(ILife life, int minX, int minY, int maxX, int maxY)
        {
            Tuple<int, int, int, int> minmax = life.GetMinMaxIndexes();

            //System.Console.Clear();
            System.Console.SetCursorPosition(0, 0);
            System.Console.WriteLine("Generation: {0,5}  Population: {1,5}  X:{2,3}->{3,3} Y:{4,3}->{5,3}", life.Generation, life.Population, minmax.Item1, minmax.Item3, minmax.Item2, minmax.Item4);

            bool[,] view = new bool[minmax.Item3 - minmax.Item1 + 1, minmax.Item4 - minmax.Item2 + 1];
            life.GetView(minmax.Item1, minmax.Item2, minmax.Item3, minmax.Item4, view);
            for (int y = 0; y <= maxY - minY; y++)
            {
                StringBuilder sb = new StringBuilder(11);
                for (int x = 0; x <= maxX - minX; x++)
                {
                    //sb.Append(cells[x, y] == -1 ? "." : cells[x,y].ToString());
                    //sb.Append(cells[x, y] >= 0 ? "*" : ".");
                    string display = "*";
                    if (!view[x, y])
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

        private static void TestModifiedList()
        {
            LifeModifiedList life = new LifeModifiedList(60, 60); // BUGGED with gosper glider run
            //// Glider
            //life.Set(2, 1);
            //life.Set(3, 2);
            //life.Set(1, 3);
            //life.Set(2, 3);
            //life.Set(3, 3);
            IEnumerable<Tuple<int, int>> cells = ReadRLE(@"d:\github\gameoflife\rle\gosper glider gun.rle");
            foreach (Tuple<int, int> cell in cells)
                life.Set(cell.Item1, cell.Item2);

            bool pause = true;
            while (true)
            {
                DisplayModifiedList(life);
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

        private static void DisplayModifiedList(LifeModifiedList life)
        {
            //System.Console.Clear();
            System.Console.SetCursorPosition(0, 0);

            System.Console.WriteLine("Generation: {0,5}  Population: {1,5}  MaxModified: {2,3} MaxNeighbour: {3,3}  MaxLoop: {4,3}", life.Generation, life.PopulationCount, life.MaxModified, life.MaxNeighbours, life.MaxLoopCount);
            int[,] cells = life.GetCells();
            for (int y = 0; y < life.Height; y++)
            {
                StringBuilder sb = new StringBuilder(life.Width);
                for (int x = 0; x < life.Width; x++)
                    sb.Append(cells[x, y] > 0 ? "*" : " ");
                System.Console.WriteLine(sb.ToString());
            }
        }

        private static void TestHex()
        {
            int[] survivals = new[] {3, 4};
            int[] births = new[] {2};
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

            System.Console.WriteLine("Generation: {0,5}  Population: {1,5}", life.Generation, life.PopulationCount);
            int[,] cells = life.GetCells();
            int diagonal = 2*life.Radius + 1;
            for (int y = 0; y < diagonal; y++) // TODO: display only valid cells (not wasted ones)
            {
                StringBuilder sb = new StringBuilder(diagonal);
                for (int x = 0; x < diagonal; x++)
                    sb.Append(cells[x, y] >= 0 ? "*" : ".");
                System.Console.WriteLine(sb.ToString());
            }
        }

        private static void AddRLE(ILife life, string filename, int x = 0, int y = 0)
        {
            IEnumerable<Tuple<int, int>> cells = ReadRLE(filename);
            foreach (Tuple<int, int> cell in cells)
                life.Set(cell.Item1 + x, cell.Item2 + y);
        }

        private static IEnumerable<Tuple<int, int>> ReadRLE(string filename)
        {
            //http://www.conwaylife.com/wiki/Run_Length_Encoded
            using (TextReader tr = new StreamReader(filename))
            {
                string line;
                // Skip comment
                while (true)
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
                string content = tr.ReadToEnd().Replace(Environment.NewLine, String.Empty).Replace("\n",String.Empty).Replace("\r", String.Empty);
                // create cells map
                char[] ob = new[] {'o', 'b'};
                // parse cells
                string[] rows = content.Split('$', '!').Where(row => !String.IsNullOrEmpty(row)).ToArray();
                List<Tuple<int, int>> cells = new List<Tuple<int, int>>();
                int rowIndex = 0;
                foreach (string row in rows)
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
                                cells.Add(new Tuple<int, int>(columnIndex + i, rowIndex));
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
