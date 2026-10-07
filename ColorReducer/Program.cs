using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ColorReducer
{
    class Program
    {
        static void Main(string[] args)
        {
            string inputPath = "../coins.jpeg";
            string outputPath = "../coins_flattened.png";

            using (Bitmap bitmap = new Bitmap(inputPath))
            {
                Color c1 = Color.FromArgb(229, 229, 229); // #E5E5E5
                Color c2 = Color.FromArgb(83, 83, 83);    // #535353

                Color[,] pixels = new Color[bitmap.Width, bitmap.Height];
                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        Color pixel = bitmap.GetPixel(x, y);
                        int d1 = DistanceSq(pixel, c1);
                        int d2 = DistanceSq(pixel, c2);
                        pixels[x, y] = d1 < d2 ? c1 : c2;
                    }
                }

                Color[,] newPixels = new Color[bitmap.Width, bitmap.Height];
                int[] dx = { -1, 1, 0, 0, -1, -1, 1, 1 };
                int[] dy = { 0, 0, -1, 1, -1, 1, -1, 1 };

                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        Color current = pixels[x, y];
                        newPixels[x, y] = current;
                        
                        if (current == c1) // c1 is background, check if adjacent to c2 (grey)
                        {
                            bool adjacentToGrey = false;
                            for (int i = 0; i < 8; i++)
                            {
                                int nx = x + dx[i];
                                int ny = y + dy[i];
                                if (nx >= 0 && nx < bitmap.Width && ny >= 0 && ny < bitmap.Height)
                                {
                                    if (pixels[nx, ny] == c2)
                                    {
                                        adjacentToGrey = true;
                                        break;
                                    }
                                }
                            }
                            if (adjacentToGrey)
                            {
                                newPixels[x, y] = c2;
                            }
                        }
                    }
                }

                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        bitmap.SetPixel(x, y, newPixels[x, y]);
                    }
                }

                bitmap.Save(outputPath, ImageFormat.Png);
                Console.WriteLine("Processing complete. Output saved to " + outputPath);
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
