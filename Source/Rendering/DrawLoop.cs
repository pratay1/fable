partial class Program
{
    // ---------------------------------------------------------------- Draw

    static void Draw()
    {
        frameTime = (float)Raylib.GetTime();
        Raylib.BeginDrawing();
        Raylib.ClearBackground(ForestShadow);

        DrawForestBackground();

        switch (state)
        {
            case GameState.MainMenu:
                DrawMainMenu();
                break;
            case GameState.DifficultySelect:
                DrawDifficultySelect();
                break;
            case GameState.Playing:
                DrawPlayScene();
                break;
            case GameState.Paused:
                DrawPlayScene();
                DrawPauseMenu();
                break;
            case GameState.GameOver:
                DrawPlayScene();
                DrawGameOverScreen();
                break;
            case GameState.Customize:
                DrawCustomize();
                break;
            case GameState.Settings:
                if (settingsReturnState == GameState.Paused)
                {
                    DrawPlayScene();
                }
                DrawSettings();
                break;
            default:
                throw new UnreachableException();
        }

        DrawVignette();
        DrawWindowCloseButton();
        if (showFps) DrawFpsOverlay();
        Raylib.EndDrawing();
    }

    static void DrawFpsOverlay()
    {
        int fps = Raylib.GetFPS();
        string text = fps + " FPS";
        int fontSize = 16;
        int w = Raylib.MeasureText(text, fontSize);
        int x = WindowWidth - w - 16;
        int y = WindowHeight - fontSize - 12;
        Color tint = fps >= 60 ? new Color(120, 220, 140, 255)
            : fps >= 30 ? new Color(228, 196, 96, 255)
            : new Color(224, 96, 84, 255);
        Raylib.DrawText(text, x + 1, y + 1, fontSize, WithAlpha(Color.Black, 0.55f));
        Raylib.DrawText(text, x, y, fontSize, tint);
    }

    static Rectangle WindowCloseButtonRect()
        => new(WindowWidth - WindowCloseBtnPad - WindowCloseBtnSize, WindowCloseBtnPad, WindowCloseBtnSize, WindowCloseBtnSize);

    static bool IsWindowDragZone(Vector2 mouse, Rectangle closeBtn)
    {
        if (mouse.Y > WindowDragBarHeight) return false;
        return !Raylib.CheckCollisionPointRec(mouse, closeBtn);
    }

    static void UpdateWindowChrome()
    {
        Vector2 mouse = Raylib.GetMousePosition();
        Rectangle closeBtn = WindowCloseButtonRect();
        windowCloseHovered = Raylib.CheckCollisionPointRec(mouse, closeBtn);

        if (Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            if (windowCloseHovered)
            {
                Raylib.CloseWindow();
                return;
            }

            if (IsWindowDragZone(mouse, closeBtn))
            {
                windowDragging = true;
            }
        }

        if (!Raylib.IsMouseButtonDown(MouseButton.Left))
        {
            windowDragging = false;
        }

        if (windowDragging)
        {
            Vector2 delta = Raylib.GetMouseDelta();
            Vector2 pos = Raylib.GetWindowPosition();
            Raylib.SetWindowPosition((int)(pos.X + delta.X), (int)(pos.Y + delta.Y));
        }
    }

    static void DrawWindowCloseButton()
    {
        Rectangle btn = WindowCloseButtonRect();
        float cx = btn.X + btn.Width * 0.5f;
        float cy = btn.Y + btn.Height * 0.5f;

        if (windowCloseHovered)
        {
            Raylib.DrawCircleV(new Vector2(cx, cy), btn.Width * 0.52f, new Color(42, 40, 46, 230));
            Raylib.DrawCircleLines((int)cx, (int)cy, btn.Width * 0.52f, WithAlpha(UiBorder, 0.45f));
        }

        float half = 7f;
        Color xCol = windowCloseHovered ? new Color(196, 192, 184, 255) : new Color(68, 66, 62, 255);
        float thick = windowCloseHovered ? 2.3f : 1.9f;
        Raylib.DrawLineEx(new Vector2(cx - half, cy - half), new Vector2(cx + half, cy + half), thick, xCol);
        Raylib.DrawLineEx(new Vector2(cx + half, cy - half), new Vector2(cx - half, cy + half), thick, xCol);
    }

    static void DrawForestBackground()
    {
        Raylib.DrawRectangle(0, 0, WindowWidth, WindowHeight, new Color(1, 1, 1, 255));
        Raylib.DrawRectangleGradientV(0, 0, WindowWidth, WindowHeight, new Color(2, 2, 2, 255), new Color(6, 5, 5, 255));

        Raylib.DrawRectangleGradientV(0, WindowHeight / 2, WindowWidth, WindowHeight / 2,
            WithAlpha(new Color(10, 9, 8, 255), 0f),
            WithAlpha(new Color(10, 9, 8, 255), 0.04f));

        float time = (float)Raylib.GetTime();

        if (state == GameState.MainMenu)
        {
            if (menuCastleEnabled) DrawMenuCastleFraming(time);
            if (backgroundMotes) DrawMotes(0.32f);
        }
        else
        {
            DrawEdgeCastle(time, 0.55f);
            if (backgroundMotes) DrawMotes();
        }
    }

    // New detailed framing growth for the main menu, perfectly centered.
    static void DrawMenuForestFraming(float time)
    {
        Color leafMid = new Color(42, 64, 44, 255);
        Color barkDark = new Color(48, 36, 26, 255);

        // Ancient framing tree bases in bottom corners (detailed)
        DrawDetailedGroveFraming(new Vector2(30f, WindowHeight - 10f), 1.25f, leafMid, barkDark);
        DrawDetailedGroveFraming(new Vector2(WindowWidth - 30f, WindowHeight - 10f), 1.15f, leafMid, barkDark);

        // Centered creeping vine pattern
        float spin = time * 0.4f;
        int max = 8;
        for (int i = 0; i < max; i++)
        {
            float frac = i / (float)max;
            float rfrac = frac + spin % (1f / max);
            if (rfrac > 1f) rfrac -= 1f;
            float p = EaseOutCubic(rfrac);
            float swing = MathF.Sin(time * 0.8f + i) * 8f * (1f - rfrac);
            Vector2 vineP = new Vector2(WindowWidth / 2f + swing, WindowHeight * 0.9f - WindowHeight * 0.6f * p);
            Color vineCol = WithAlpha(LerpColor(Darken(Emerald, 0.6f), Lighten(MossLight, 0.3f), p), (1f - p) * 0.8f);
            Raylib.DrawCircleV(vineP, 2.5f * (1f - rfrac) + 1f, vineCol);
        }
    }

    static void DrawDetailedGroveFraming(Vector2 root, float scale, Color leafCol, Color trunkCol)
    {
        float h = 100f * scale;
        Vector2 mid = new Vector2(root.X, root.Y - h * 0.35f);
        Vector2 top = new Vector2(root.X, root.Y - h * 0.75f);
        float trunkW = 6f * scale;

        // Trunk base
        Raylib.DrawLineEx(root, top, trunkW, WithAlpha(trunkCol, 0.9f));
        for (int s = -1; s <= 1; s += 2)
        {
            Vector2 offP = new Vector2(root.X + s * trunkW, root.Y - h * 0.15f);
            Raylib.DrawLineEx(root, offP, 3f * scale, WithAlpha(Darken(trunkCol, 0.4f), 0.7f));
        }

        // Detailed vine/leaf canopy
        float rad = 18f * scale;
        Vector2 canopy = new Vector2(root.X, root.Y - h * 0.6f);
        Raylib.DrawCircleV(new Vector2(canopy.X - rad * 0.7f, canopy.Y + rad * 0.1f), rad, WithAlpha(leafCol, 0.75f));
        Raylib.DrawCircleV(new Vector2(canopy.X + rad * 0.8f, canopy.Y - rad * 0.1f), rad * 0.9f, WithAlpha(leafCol, 0.8f));
        Raylib.DrawCircleV(canopy, rad * 1.1f, WithAlpha(Lighten(leafCol, 0.2f), 0.7f));
    }

}
