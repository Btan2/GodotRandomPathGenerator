using System.Drawing;

public class ImageMapLoader
{
    public (bool[,], int) ReadImageMap(string dir)
    {
        Bitmap img = new Bitmap(dir);

        // Image width and height must be uniform and not rectangular
        int gridSize = img.Width;
        bool[,] ends = new bool[gridSize-1, gridSize-1];
        
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                Color pixel = img.GetPixel(i,j);
                if (pixel == Color.Black)
                {
                    // End type
                    ends[i,j] = true;
                }
                else if (pixel == Color.Green)
                {
                    // Start type
                    ends[i,j] = true;
                }
                else
                {
                    // Path type
                    ends[i,j] = false;
                }
            }
        }

        return (ends, gridSize);
    }
}
