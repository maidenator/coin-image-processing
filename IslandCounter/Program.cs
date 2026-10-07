using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace IslandCounter
{
    class Program
    {
        static void Main(string[] args)
        {
            string inputPath = "../coins_flattened.png";

            Console.WriteLine("Loading image...");
            using (Bitmap bitmap = new Bitmap(inputPath))
            {
                int width = bitmap.Width;
                int height = bitmap.Height;
                bool[,] visited = new bool[width, height];
                
                Color[,] pixels = new Color[width, height];
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        pixels[x, y] = bitmap.GetPixel(x, y);
                    }
                }

                Color bgBase = Color.FromArgb(229, 229, 229);
                Color fgBase = Color.FromArgb(83, 83, 83);
                
                Color topleft = pixels[0, 0];
                bool isTopLeftBg = DistanceSq(topleft, bgBase) < DistanceSq(topleft, fgBase);
                Color actualBgBase = isTopLeftBg ? bgBase : fgBase;
                Color actualFgBase = isTopLeftBg ? fgBase : bgBase;

                Console.WriteLine($"Background base color: C({actualBgBase.R}, {actualBgBase.G}, {actualBgBase.B})");

                List<List<Point>> islands = new List<List<Point>>();

                int[] dx = { -1, 1, 0, 0, -1, -1, 1, 1 };
                int[] dy = { 0, 0, -1, 1, -1, 1, -1, 1 };

                bool[,] isGrey = new bool[width, height];
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        Color pColor = pixels[x, y];
                        int distToBg = DistanceSq(pColor, actualBgBase);
                        int distToFg = DistanceSq(pColor, actualFgBase);
                        isGrey[x, y] = distToFg < distToBg;
                    }
                }

                Console.WriteLine("Finding islands using DFS...");

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (visited[x, y]) continue;

                        if (isGrey[x, y])
                        {
                            List<Point> currentIsland = new List<Point>();
                            Stack<Point> stack = new Stack<Point>();
                            stack.Push(new Point(x, y));

                            while (stack.Count > 0)
                            {
                                Point curr = stack.Pop();
                                int cx = curr.X;
                                int cy = curr.Y;

                                if (visited[cx, cy]) continue;

                                visited[cx, cy] = true;
                                currentIsland.Add(new Point(cx, cy));

                                for (int i = 0; i < 8; i++)
                                {
                                    int nx = cx + dx[i];
                                    int ny = cy + dy[i];

                                    if (nx >= 0 && nx < width && ny >= 0 && ny < height && !visited[nx, ny])
                                    {
                                        if (isGrey[nx, ny])
                                        {
                                            stack.Push(new Point(nx, ny));
                                        }
                                    }
                                }
                            }
                            
                            islands.Add(currentIsland);
                        }
                    }
                }

                Console.WriteLine($"\nFound {islands.Count} total coins.");

                var coinAreas = islands.Where(isl => isl.Count > 1000).Select(isl => isl.Count).ToList();

                if (coinAreas.Count > 0)
                {
                    coinAreas.Sort();
                    
                    int numGroups = 5;
                    var gaps = new List<(int size, int index)>();
                    for (int i = 1; i < coinAreas.Count; i++)
                    {
                        gaps.Add((coinAreas[i] - coinAreas[i - 1], i));
                    }

                    var splitIndices = gaps.OrderByDescending(g => g.size)
                                           .Take(numGroups - 1)
                                           .Select(g => g.index)
                                           .OrderBy(i => i)
                                           .ToList();

                    List<List<int>> groups = new List<List<int>>();
                    List<int> currentGroup = new List<int>();
                    
                    int currentSplit = 0;
                    for (int i = 0; i < coinAreas.Count; i++)
                    {
                        if (currentSplit < splitIndices.Count && i == splitIndices[currentSplit])
                        {
                            groups.Add(currentGroup);
                            currentGroup = new List<int>();
                            currentSplit++;
                        }
                        currentGroup.Add(coinAreas[i]);
                    }
                    groups.Add(currentGroup);

                    Console.WriteLine($"Grouped into {groups.Count} coin denominations:");
                    
                    string[] coinNames = { "5 Cent", "10 Cent", "25 Cent", "1 Peso", "5 Peso" };
                    for (int i = 0; i < groups.Count; i++)
                    {
                        double avgArea = groups[i].Average();
                        string name = i < coinNames.Length ? coinNames[i] : $"Group {i + 1}";
                        Console.WriteLine($"{name} (~{avgArea:F0} pixels): {groups[i].Count} coins");
                    }
                }
                else
                {
                    Console.WriteLine("No valid coins found to group.");
                }
            }
        }

        static int DistanceSq(Color a, Color b)
        {
            int dr = a.R - b.R;
            int dg = a.G - b.G;
            int db = a.B - b.B;
            return dr * dr + dg * dg + db * db;
        }
    }
}
