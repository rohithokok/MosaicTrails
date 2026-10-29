using UnityEngine;

/// <summary>Creates original, deterministic postcard illustrations for the level set.</summary>
public static class PuzzleArtworkGenerator
{
    private const int Size = 512;

    public static Texture2D Create(PuzzleArtworkTheme theme)
    {
        Color[] pixels = new Color[Size * Size];
        Color top;
        Color bottom;

        switch (theme)
        {
            case PuzzleArtworkTheme.CoralCove:
                top = new Color(0.05f, 0.22f, 0.40f);
                bottom = new Color(0.11f, 0.62f, 0.66f);
                break;
            case PuzzleArtworkTheme.CloudMarket:
                top = new Color(0.38f, 0.43f, 0.68f);
                bottom = new Color(0.98f, 0.66f, 0.47f);
                break;
            default:
                top = new Color(0.09f, 0.16f, 0.28f);
                bottom = new Color(0.25f, 0.48f, 0.43f);
                break;
        }

        for (int y = 0; y < Size; y++)
        {
            Color row = Color.Lerp(bottom, top, y / (float)(Size - 1));
            for (int x = 0; x < Size; x++)
                pixels[y * Size + x] = row;
        }

        switch (theme)
        {
            case PuzzleArtworkTheme.CoralCove:
                DrawCoralCove(pixels);
                break;
            case PuzzleArtworkTheme.CloudMarket:
                DrawCloudMarket(pixels);
                break;
            default:
                DrawLanternGarden(pixels);
                break;
        }

        Texture2D texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
        {
            name = theme + " Original Puzzle Illustration",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        texture.SetPixels(pixels);
        texture.Apply(false, false);
        return texture;
    }

    private static void DrawLanternGarden(Color[] p)
    {
        Color cream = new Color(1f, 0.81f, 0.52f);
        Color frame = new Color(0.65f, 0.58f, 0.42f);
        Color dark = new Color(0.11f, 0.22f, 0.25f);
        FillCircle(p, 392, 402, 47, cream);
        FillCircle(p, 409, 417, 43, new Color(0.10f, 0.18f, 0.29f));
        for (int i = 0; i < 15; i++)
        {
            int x = 28 + (i * 73) % 455;
            int y = 310 + (i * 47) % 175;
            FillCircle(p, x, y, 2 + i % 3, new Color(1f, 0.85f, 0.62f));
        }

        // Glasshouse silhouette and warm window panes.
        FillRect(p, 60, 52, 452, 278, new Color(0.16f, 0.32f, 0.32f));
        FillTriangle(p, 40, 264, 256, 420, 472, 264, new Color(0.20f, 0.36f, 0.35f));
        Line(p, 40, 264, 256, 420, 7, frame);
        Line(p, 256, 420, 472, 264, 7, frame);
        Line(p, 40, 264, 472, 264, 7, frame);
        Line(p, 68, 264, 68, 59, 7, frame);
        Line(p, 444, 264, 444, 59, 7, frame);
        Line(p, 256, 412, 256, 59, 5, frame);
        Line(p, 151, 338, 151, 59, 5, frame);
        Line(p, 361, 338, 361, 59, 5, frame);
        Line(p, 72, 170, 440, 170, 5, frame);
        FillRect(p, 82, 178, 146, 254, new Color(0.26f, 0.44f, 0.40f));
        FillRect(p, 284, 178, 352, 254, new Color(0.28f, 0.44f, 0.41f));
        FillRect(p, 177, 178, 236, 254, new Color(0.32f, 0.46f, 0.39f));

        // Tall plants, broad leaves, terracotta pots, and floating fireflies.
        Color leaf = new Color(0.32f, 0.64f, 0.43f);
        Color leafLight = new Color(0.58f, 0.76f, 0.43f);
        Plant(p, 131, 74, leaf, leafLight, 1.0f);
        Plant(p, 382, 75, new Color(0.23f, 0.56f, 0.49f), leafLight, 0.82f);
        Plant(p, 255, 74, new Color(0.30f, 0.56f, 0.34f), new Color(0.81f, 0.69f, 0.34f), 0.66f);
        FillRect(p, 95, 56, 166, 87, new Color(0.72f, 0.34f, 0.25f));
        FillRect(p, 352, 56, 416, 87, new Color(0.81f, 0.43f, 0.27f));
        FillRect(p, 226, 54, 285, 78, new Color(0.71f, 0.37f, 0.27f));
        for (int i = 0; i < 7; i++)
        {
            int x = 90 + i * 54;
            int y = 294 + (i % 2) * 42;
            FillCircle(p, x, y, 7, cream);
            FillCircle(p, x, y, 3, new Color(0.95f, 0.55f, 0.33f));
        }
        FillRect(p, 0, 0, Size, 34, dark);
    }

    private static void DrawCoralCove(Color[] p)
    {
        Color sand = new Color(0.89f, 0.69f, 0.46f);
        Color sea = new Color(0.08f, 0.37f, 0.54f);
        FillCircle(p, 118, 400, 46, new Color(0.91f, 0.82f, 0.62f));
        FillRect(p, 0, 0, Size, 78, sand);
        FillTriangle(p, 0, 96, 175, 63, 290, 105, new Color(0.95f, 0.78f, 0.52f));
        FillTriangle(p, 206, 104, 365, 62, 512, 99, new Color(0.95f, 0.78f, 0.52f));

        // Layered distant rock shelves.
        FillTriangle(p, 0, 75, 0, 265, 180, 156, new Color(0.08f, 0.31f, 0.43f));
        FillTriangle(p, 512, 74, 512, 272, 340, 167, new Color(0.07f, 0.30f, 0.43f));
        Line(p, 24, 185, 117, 126, 7, new Color(0.18f, 0.53f, 0.58f));
        Line(p, 483, 198, 391, 137, 7, new Color(0.18f, 0.53f, 0.58f));

        // Coral branches and rounded anemones.
        Branch(p, 79, 79, 67, 245, new Color(0.94f, 0.42f, 0.39f));
        Branch(p, 428, 78, 66, 237, new Color(0.96f, 0.62f, 0.38f));
        Branch(p, 252, 79, 44, 178, new Color(0.80f, 0.48f, 0.70f));
        for (int i = 0; i < 10; i++)
        {
            int x = 30 + (i * 89) % 455;
            int y = 150 + (i * 61) % 276;
            FillCircle(p, x, y, 4 + i % 5, new Color(0.66f, 0.88f, 0.84f));
            FillCircle(p, x - 2, y + 2, 2, new Color(0.85f, 0.96f, 0.87f));
        }

        // A small school of original, two-tone fish.
        Fish(p, 236, 327, 1.0f, new Color(1f, 0.73f, 0.40f));
        Fish(p, 327, 265, 0.72f, new Color(0.93f, 0.53f, 0.43f));
        Fish(p, 164, 239, 0.60f, new Color(0.75f, 0.78f, 0.49f));
        FillRect(p, 0, 0, Size, 26, sea);
    }

    private static void DrawCloudMarket(Color[] p)
    {
        Color cloud = new Color(1f, 0.84f, 0.67f);
        Color roof = new Color(0.28f, 0.31f, 0.50f);
        FillCircle(p, 391, 391, 47, new Color(1f, 0.82f, 0.51f));
        Cloud(p, 97, 376, cloud);
        Cloud(p, 264, 445, new Color(1f, 0.90f, 0.77f));
        Cloud(p, 450, 321, new Color(1f, 0.88f, 0.72f));
        FillTriangle(p, 0, 64, 160, 190, 312, 78, new Color(0.47f, 0.55f, 0.69f));
        FillTriangle(p, 177, 66, 365, 198, 512, 72, new Color(0.39f, 0.49f, 0.65f));
        FillTriangle(p, 0, 30, 256, 137, 512, 24, new Color(0.27f, 0.42f, 0.55f));

        // Floating market stalls on a broad stone terrace.
        FillRect(p, 48, 64, 465, 116, new Color(0.83f, 0.59f, 0.43f));
        FillRect(p, 68, 114, 451, 139, new Color(0.94f, 0.74f, 0.52f));
        Stall(p, 94, 140, 120, roof, new Color(0.93f, 0.49f, 0.40f));
        Stall(p, 226, 151, 128, new Color(0.24f, 0.47f, 0.53f), new Color(0.91f, 0.75f, 0.43f));
        Stall(p, 362, 141, 117, new Color(0.43f, 0.38f, 0.58f), new Color(0.56f, 0.73f, 0.58f));
        for (int i = 0; i < 5; i++)
        {
            int x = 104 + i * 73;
            FillCircle(p, x, 31 + (i % 2) * 18, 4 + i % 3, new Color(1f, 0.91f, 0.69f));
        }

        // Three playful hot-air balloons above the rooftops.
        Balloon(p, 109, 308, 36, new Color(0.93f, 0.47f, 0.41f), new Color(1f, 0.81f, 0.56f));
        Balloon(p, 260, 339, 45, new Color(0.37f, 0.65f, 0.61f), new Color(0.91f, 0.75f, 0.46f));
        Balloon(p, 406, 270, 31, new Color(0.67f, 0.50f, 0.68f), new Color(0.98f, 0.74f, 0.48f));
    }

    private static void Plant(Color[] p, int x, int potY, Color leaf, Color light, float scale)
    {
        int stemTop = potY + Mathf.RoundToInt(175 * scale);
        Line(p, x, potY + 24, x, stemTop, 5, new Color(0.25f, 0.45f, 0.33f));
        for (int i = 0; i < 5; i++)
        {
            int y = potY + 48 + Mathf.RoundToInt(i * 25 * scale);
            int direction = i % 2 == 0 ? -1 : 1;
            FillEllipse(p, x + direction * Mathf.RoundToInt(21 * scale), y, Mathf.Max(4, Mathf.RoundToInt(23 * scale)), Mathf.Max(4, Mathf.RoundToInt(11 * scale)), i % 2 == 0 ? leaf : light);
        }
        FillCircle(p, x, stemTop + 8, Mathf.Max(5, Mathf.RoundToInt(11 * scale)), new Color(0.93f, 0.63f, 0.47f));
    }

    private static void Branch(Color[] p, int x, int y, int height, int sway, Color color)
    {
        Line(p, x, y, x + sway / 3, y + height, 12, color);
        for (int i = 1; i <= 4; i++)
        {
            int branchY = y + height * i / 5;
            int branchX = x + sway * i / 12;
            int side = i % 2 == 0 ? 1 : -1;
            Line(p, branchX, branchY, branchX + side * 42, branchY + 28, 9, color);
            FillCircle(p, branchX + side * 42, branchY + 28, 7, Color.Lerp(color, Color.white, 0.2f));
        }
        for (int i = 0; i < 4; i++)
            FillCircle(p, x + sway / 3 + (i % 2 == 0 ? -8 : 8), y + height + i * 8, 7, color);
    }

    private static void Fish(Color[] p, int x, int y, float scale, Color color)
    {
        int rx = Mathf.RoundToInt(36 * scale);
        int ry = Mathf.RoundToInt(20 * scale);
        FillTriangle(p, x - rx, y, x - rx - Mathf.RoundToInt(24 * scale), y + Mathf.RoundToInt(16 * scale), x - rx - Mathf.RoundToInt(24 * scale), y - Mathf.RoundToInt(16 * scale), color);
        FillEllipse(p, x, y, rx, ry, color);
        FillEllipse(p, x + 7 * scale, y + 4 * scale, rx * 0.55f, ry * 0.65f, Color.Lerp(color, Color.white, 0.35f));
        FillCircle(p, x + Mathf.RoundToInt(20 * scale), y + 5, Mathf.Max(2, Mathf.RoundToInt(3 * scale)), new Color(0.08f, 0.21f, 0.30f));
    }

    private static void Cloud(Color[] p, int x, int y, Color color)
    {
        FillEllipse(p, x, y, 54, 22, color);
        FillCircle(p, x - 25, y + 11, 25, color);
        FillCircle(p, x + 5, y + 20, 32, color);
        FillCircle(p, x + 34, y + 10, 23, color);
    }

    private static void Stall(Color[] p, int x, int y, int width, Color roofColor, Color awning)
    {
        FillRect(p, x + 8, y - 59, x + width - 8, y, new Color(0.89f, 0.69f, 0.48f));
        FillTriangle(p, x, y, x + width / 2, y + 39, x + width, y, roofColor);
        FillRect(p, x + 5, y - 14, x + width - 5, y - 4, awning);
        FillRect(p, x + 20, y - 57, x + 41, y - 23, new Color(0.31f, 0.40f, 0.44f));
        FillRect(p, x + width - 43, y - 57, x + width - 21, y - 23, new Color(0.31f, 0.40f, 0.44f));
    }

    private static void Balloon(Color[] p, int x, int y, int radius, Color main, Color stripe)
    {
        FillEllipse(p, x, y, radius, Mathf.RoundToInt(radius * 1.25f), main);
        FillEllipse(p, x, y, Mathf.Max(3, radius / 4), Mathf.RoundToInt(radius * 1.1f), stripe);
        int bottom = y - Mathf.RoundToInt(radius * 1.2f);
        Line(p, x - 10, bottom, x - 6, bottom - 24, 2, new Color(0.30f, 0.31f, 0.40f));
        Line(p, x + 10, bottom, x + 6, bottom - 24, 2, new Color(0.30f, 0.31f, 0.40f));
        FillRect(p, x - 9, bottom - 31, x + 9, bottom - 23, new Color(0.69f, 0.42f, 0.32f));
    }

    private static void FillRect(Color[] p, int x0, int y0, int x1, int y1, Color color)
    {
        for (int y = Mathf.Max(0, y0); y < Mathf.Min(Size, y1); y++)
            for (int x = Mathf.Max(0, x0); x < Mathf.Min(Size, x1); x++)
                p[y * Size + x] = color;
    }

    private static void FillCircle(Color[] p, int cx, int cy, int radius, Color color)
    {
        int r2 = radius * radius;
        for (int y = Mathf.Max(0, cy - radius); y <= Mathf.Min(Size - 1, cy + radius); y++)
            for (int x = Mathf.Max(0, cx - radius); x <= Mathf.Min(Size - 1, cx + radius); x++)
            {
                int dx = x - cx;
                int dy = y - cy;
                if (dx * dx + dy * dy <= r2)
                    p[y * Size + x] = color;
            }
    }

    private static void FillEllipse(Color[] p, float cx, float cy, float rx, float ry, Color color)
    {
        int minX = Mathf.Max(0, Mathf.FloorToInt(cx - rx));
        int maxX = Mathf.Min(Size - 1, Mathf.CeilToInt(cx + rx));
        int minY = Mathf.Max(0, Mathf.FloorToInt(cy - ry));
        int maxY = Mathf.Min(Size - 1, Mathf.CeilToInt(cy + ry));
        for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
            {
                float dx = (x - cx) / rx;
                float dy = (y - cy) / ry;
                if (dx * dx + dy * dy <= 1f)
                    p[y * Size + x] = color;
            }
    }

    private static void FillTriangle(Color[] p, int ax, int ay, int bx, int by, int cx, int cy, Color color)
    {
        int minX = Mathf.Max(0, Mathf.Min(ax, Mathf.Min(bx, cx)));
        int maxX = Mathf.Min(Size - 1, Mathf.Max(ax, Mathf.Max(bx, cx)));
        int minY = Mathf.Max(0, Mathf.Min(ay, Mathf.Min(by, cy)));
        int maxY = Mathf.Min(Size - 1, Mathf.Max(ay, Mathf.Max(by, cy)));
        float area = Edge(ax, ay, bx, by, cx, cy);
        for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
            {
                float e1 = Edge(ax, ay, bx, by, x, y);
                float e2 = Edge(bx, by, cx, cy, x, y);
                float e3 = Edge(cx, cy, ax, ay, x, y);
                if ((area >= 0f && e1 >= 0f && e2 >= 0f && e3 >= 0f) ||
                    (area < 0f && e1 <= 0f && e2 <= 0f && e3 <= 0f))
                    p[y * Size + x] = color;
            }
    }

    private static float Edge(float ax, float ay, float bx, float by, float px, float py)
    {
        return (px - ax) * (by - ay) - (py - ay) * (bx - ax);
    }

    private static void Line(Color[] p, int x0, int y0, int x1, int y1, int width, Color color)
    {
        float dx = x1 - x0;
        float dy = y1 - y0;
        float len = dx * dx + dy * dy;
        int radius = Mathf.CeilToInt(width * 0.5f);
        for (int y = Mathf.Max(0, Mathf.Min(y0, y1) - radius); y <= Mathf.Min(Size - 1, Mathf.Max(y0, y1) + radius); y++)
            for (int x = Mathf.Max(0, Mathf.Min(x0, x1) - radius); x <= Mathf.Min(Size - 1, Mathf.Max(x0, x1) + radius); x++)
            {
                float t = len > 0f ? Mathf.Clamp01(((x - x0) * dx + (y - y0) * dy) / len) : 0f;
                float ex = x - (x0 + t * dx);
                float ey = y - (y0 + t * dy);
                if (ex * ex + ey * ey <= width * width * 0.25f)
                    p[y * Size + x] = color;
            }
    }
}
