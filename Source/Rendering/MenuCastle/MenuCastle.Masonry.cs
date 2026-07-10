partial class Program
{
    // -------------------------------------------------------------------------
    // Ultra masonry pass
    // -------------------------------------------------------------------------

    static void DrawMenuMasonryUltra(Rectangle region, MenuCastlePalette palette, int rows, int cols)
    {
        float rowH = region.Height / rows;
        float colW = region.Width / cols;
        for (int row = 0; row < rows; row++)
        {
            float y = region.Y + row * rowH;
            float offset = (row % 2 == 0 ? 0f : 0.5f) * colW;
            for (int col = -1; col < cols + 1; col++)
            {
                float x = region.X + col * colW + offset;
                float inset = 1f + Hash(row * 97 + col * 53) * 1.8f;
                var brick = new Rectangle(x + inset, y + inset, colW - inset * 2f, rowH - inset * 2f);
                if (brick.X + brick.Width < region.X || brick.X > region.X + region.Width) continue;
                int seed = (int)(x * 3.1f) ^ (int)(y * 7.3f) ^ row * 131 ^ col * 97;
                float n = Hash(seed);
                float n2 = Hash(seed + 17);
                Color brickColor = LerpColor(palette.StoneMid, palette.Stone, n * 0.45f + 0.2f);
                brickColor = LerpColor(brickColor, palette.StoneLight, n2 * 0.22f);
                if (n > 0.82f) brickColor = Darken(brickColor, 0.14f);
                if (n < 0.18f) brickColor = Lighten(brickColor, 0.06f);
                float faceAlpha = 0.58f + n2 * 0.28f;
                Raylib.DrawRectangleRounded(brick, 0.06f + n * 0.05f, 4, WithAlpha(brickColor, faceAlpha));
                // Top-left catch light
                Raylib.DrawLine((int)brick.X, (int)brick.Y, (int)(brick.X + brick.Width * 0.72f), (int)brick.Y,
                    WithAlpha(palette.StoneHi, 0.12f + n2 * 0.14f));
                Raylib.DrawLine((int)brick.X, (int)brick.Y, (int)brick.X, (int)(brick.Y + brick.Height * 0.55f),
                    WithAlpha(palette.StoneHi, 0.08f + n * 0.1f));
                // Bottom/right ambient occlusion on each block
                Raylib.DrawLine((int)brick.X, (int)(brick.Y + brick.Height - 1f), (int)(brick.X + brick.Width), (int)(brick.Y + brick.Height - 1f),
                    WithAlpha(palette.StoneDeep, 0.18f + n * 0.12f));
                Raylib.DrawLine((int)(brick.X + brick.Width - 1f), (int)brick.Y, (int)(brick.X + brick.Width - 1f), (int)(brick.Y + brick.Height),
                    WithAlpha(palette.StoneDeep, 0.12f + n2 * 0.1f));
                // Chips and breaks
                if (Hash(seed + 3) > 0.78f)
                {
                    float chipX = brick.X + Hash(seed + 5) * brick.Width * 0.6f;
                    float chipY = brick.Y + Hash(seed + 7) * brick.Height * 0.5f;
                    Raylib.DrawCircleV(new Vector2(chipX, chipY), 1f + Hash(seed + 9), WithAlpha(palette.StoneDeep, 0.35f));
                }
                // Lichen speckles
                for (int lic = 0; lic < 3; lic++)
                {
                    if (Hash(seed + 20 + lic) < 0.72f) continue;
                    float lx = brick.X + Hash(seed + 30 + lic) * brick.Width;
                    float ly = brick.Y + Hash(seed + 40 + lic) * brick.Height;
                    Raylib.DrawCircleV(new Vector2(lx, ly), 0.8f + Hash(seed + 50 + lic),
                        WithAlpha(palette.Lichen, 0.12f + Hash(seed + 60 + lic) * 0.18f));
                }
                // Moisture stain (lower bricks)
                float rowFrac = row / (float)Math.Max(1, rows - 1);
                if (rowFrac > 0.55f && Hash(seed + 71) > 0.55f)
                {
                    var stain = new Rectangle(brick.X + 1f, brick.Y + brick.Height * 0.45f,
                        brick.Width - 2f, brick.Height * 0.5f);
                    DrawGradientWash(stain, WithAlpha(Darken(palette.Stone, 0.2f), 0.2f),
                        WithAlpha(palette.StoneMid, 0f), new Vector2(0.5f, 1f), 1.4f);
                }
            }
        }
        // Mortar grid
        for (int row = 0; row <= rows; row++)
        {
            float y = region.Y + row * rowH;
            Raylib.DrawLine((int)region.X, (int)y, (int)(region.X + region.Width), (int)y, WithAlpha(palette.Mortar, 0.48f));
        }
        for (int col = 0; col <= cols; col++)
        {
            float x = region.X + col * colW;
            Raylib.DrawLine((int)x, (int)region.Y, (int)x, (int)(region.Y + region.Height), WithAlpha(palette.Mortar, 0.32f));
        }
        // Moss in random joints
        for (int j = 0; j < rows * cols / 4; j++)
        {
            float jx = region.X + Hash(j * 19 + 2) * region.Width;
            float jy = region.Y + Hash(j * 23 + 5) * region.Height;
            Raylib.DrawCircleV(new Vector2(jx, jy), 1.5f + Hash(j * 29), WithAlpha(palette.Moss, 0.2f + Hash(j * 31) * 0.25f));
        }
    }

}
