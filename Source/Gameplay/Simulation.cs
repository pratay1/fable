partial class Program
{
    // ---------------------------------------------------------------- Sim helpers

    static void UpdateParticles(float dt)
    {
        if (particles.Count > ParticleCap)
        {
            int remove = particles.Count - ParticleCap;
            particles.RemoveRange(0, remove);
        }

        float time = frameTime > 0f ? frameTime : (float)Raylib.GetTime();
        int write = 0;
        for (int i = 0; i < particles.Count; i++)
        {
            Particle p = particles[i];
            p.Velocity *= MathF.Exp(-p.Drag * dt);
            p.Velocity.Y += MathF.Sin(time * 3f + p.Position.X * 0.05f) * 6f * dt;
            p.Position += p.Velocity * dt;
            p.Rotation += p.Spin * dt;
            p.Alpha -= p.Fade * dt;

            if (p.Alpha > 0f)
            {
                particles[write++] = p;
            }
        }

        if (write < particles.Count)
        {
            particles.RemoveRange(write, particles.Count - write);
        }
    }

    static void UpdateTrails(float dt)
    {
        int write = 0;
        for (int i = 0; i < trails.Count; i++)
        {
            DashTrail t = trails[i];
            t.Alpha -= dt * 3.2f;
            if (t.Alpha > 0f)
            {
                trails[write++] = t;
            }
        }

        if (write < trails.Count)
        {
            trails.RemoveRange(write, trails.Count - write);
        }
    }

    static void UpdateFloaters(float dt)
    {
        int write = 0;
        for (int i = 0; i < floaters.Count; i++)
        {
            FloatingText f = floaters[i];
            f.Position += f.Velocity * dt;
            f.Velocity *= MathF.Exp(-2.5f * dt);
            f.Life -= dt;
            if (f.Life > 0f)
            {
                floaters[write++] = f;
            }
        }

        if (write < floaters.Count)
        {
            floaters.RemoveRange(write, floaters.Count - write);
        }
    }

    static void AddTrauma(float amount)
        => trauma = Math.Clamp(trauma + amount * (reduceMotion ? 0.4f : 1f), 0f, 1f);

    enum ImpactTier { Light, Medium, Major }

    static bool ProcessHitStop(float dt)
    {
        if (!hitStopEnabled)
        {
            hitstop = 0f;
            return false;
        }
        if (hitstop <= 0f) return false;

        hitstop = Math.Max(0f, hitstop - dt);
        impactFlash *= MathF.Exp(-34f * dt);
        trauma = Math.Max(0f, trauma - TraumaDecay * dt);
        UpdateParticles(dt);
        UpdateTrails(dt);
        UpdateFloaters(dt);
        return true;
    }

    static void TriggerImpact(ImpactTier tier, Color flashColor)
    {
        impactFlashColor = flashColor;
        float hitScale = reduceMotion ? 0.35f : 1f;
        switch (tier)
        {
            case ImpactTier.Light:
                hitstop = Math.Max(hitstop, 0.028f * hitScale);
                impactFlash = Math.Max(impactFlash, 0.55f);
                impactFlashSharp = false;
                AddFlash(flashColor, 0.13f);
                break;
            case ImpactTier.Medium:
                hitstop = Math.Max(hitstop, 0.085f * hitScale);
                impactFlash = 1f;
                impactFlashSharp = true;
                AddFlash(flashColor, 0.28f);
                if (!reduceMotion) zoomPunch = Math.Max(zoomPunch, 0.065f);
                SpawnGfxLightPulse(playerPos, flashColor, 180f, 1.15f, 0.38f);
                break;
            case ImpactTier.Major:
                hitstop = Math.Max(hitstop, 0.16f * hitScale);
                impactFlash = 1f;
                impactFlashSharp = true;
                impactFlashColor = Color.White;
                AddFlash(flashColor, 0.55f);
                if (!reduceMotion) zoomPunch = Math.Max(zoomPunch, 0.12f);
                SpawnGfxLightPulse(playerPos, flashColor, 360f, 1.85f, 0.68f);
                break;
            default:
                throw new UnreachableException();
        }
    }

    static void RegisterWeaponImpact(Vector2 pos, Color color, float damage, GunFireStyle style)
    {
        if (style == GunFireStyle.Mortar)
        {
            TriggerImpact(ImpactTier.Medium, color);
            AddTrauma(0.28f);
            zoomPunch = Math.Max(zoomPunch, 0.075f);
            return;
        }

        if (style == GunFireStyle.Sniper || style == GunFireStyle.Lance)
        {
            TriggerImpact(ImpactTier.Medium, color);
            AddTrauma(0.12f);
            zoomPunch = Math.Max(zoomPunch, 0.035f);
            return;
        }

        if (damage >= 20f)
        {
            TriggerImpact(ImpactTier.Light, color);
            zoomPunch = Math.Max(zoomPunch, 0.028f);
            AddTrauma(0.06f);
        }
        else if (damage >= 12f)
        {
            AddTrauma(0.035f);
            zoomPunch = Math.Max(zoomPunch, 0.016f);
        }
    }

    static void AddFlash(Color c, float amount)
    {
        if (amount >= flash) flashColor = c;
        flash = Math.Max(flash, amount);
    }

    static bool TryGetTileUnder(Vector2 position, out int tileX, out int tileY)
    {
        tileX = (int)(position.X / TileSize);
        tileY = (int)(position.Y / TileSize);
        return tileX >= 0 && tileX < GridSize && tileY >= 0 && tileY < GridSize;
    }

    static bool IsWithinPlayerSpawnExclusion(int tileX, int tileY)
    {
        if (!TryGetTileUnder(playerPos, out int playerTileX, out int playerTileY)) return false;
        return Math.Abs(tileX - playerTileX) <= 1 && Math.Abs(tileY - playerTileY) <= 1;
    }

    static bool IsWithinPlayerSpawnExclusion(Vector2 position)
    {
        return TryGetTileUnder(position, out int tileX, out int tileY) && IsWithinPlayerSpawnExclusion(tileX, tileY);
    }

    static Vector2 RandomEdgePosition(float radius)
    {
        const int maxAttempts = 48;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector2 pos = PickRandomEdgePosition(radius);
            if (!IsWithinPlayerSpawnExclusion(pos)) return pos;
        }

        return RandomSpawnPositionAwayFromPlayer(radius);
    }

    static Vector2 PickRandomEdgePosition(float radius)
    {
        int edge = Rng.Next(4);
        return edge switch
        {
            0 => new Vector2(Rng.Next((int)radius, WindowWidth - (int)radius), radius),
            1 => new Vector2(Rng.Next((int)radius, WindowWidth - (int)radius), WindowHeight - radius),
            2 => new Vector2(radius, Rng.Next((int)radius, WindowHeight - (int)radius)),
            3 => new Vector2(WindowWidth - radius, Rng.Next((int)radius, WindowHeight - (int)radius)),
            _ => throw new UnreachableException(),
        };
    }

    static Vector2 RandomSpawnPositionAwayFromPlayer(float radius)
    {
        int bestX = -1;
        int bestY = -1;
        int bestScore = int.MinValue;
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if (!IsTileViable(x, y) || IsWithinPlayerSpawnExclusion(x, y)) continue;
                int edgeDist = Math.Min(Math.Min(x, GridSize - 1 - x), Math.Min(y, GridSize - 1 - y));
                int score = edgeDist * 10 + Rng.Next(8);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestX = x;
                    bestY = y;
                }
            }
        }

        Vector2 fallback = bestX >= 0 ? TileCenter(bestX, bestY) : new Vector2(WindowWidth / 2f, radius);
        return ClampToArena(fallback, radius);
    }

    static float GetEnemyRadius(EnemyType type) => GetDef(type).Radius;

    static string GetEnemyName(EnemyType type) => GetDef(type).Name;

    static string GetEnemyDesc(EnemyType type) => GetDef(type).Desc;

    static Color GetEnemyColor(EnemyType type) => GetDef(type).Color;

    static int GetEnemySides(EnemyType type) => GetDef(type).Sides;

    static Color ComboColor(int c)
    {
        if (c < 3) return Color.White;
        if (c < 5) return Gold;
        if (c < 8) return new Color(196, 108, 48, 255);
        return Danger;
    }

    static Color WithAlpha(Color c, float a)
    {
        int alpha = (int)Math.Clamp(a * 255f, 0f, 255f);
        return new Color((int)c.R, (int)c.G, (int)c.B, alpha);
    }

    static Color LerpColor(Color a, Color b, float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        return new Color(
            (int)(a.R + (b.R - a.R) * t),
            (int)(a.G + (b.G - a.G) * t),
            (int)(a.B + (b.B - a.B) * t),
            (int)(a.A + (b.A - a.A) * t));
    }

    static Color Darken(Color c, float a) => LerpColor(c, ForestShadow, a);
    static Color Lighten(Color c, float a) => LerpColor(c, Color.White, a);

    static float Hash(int n)
    {
        n = (n << 13) ^ n;
        int m = (n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff;
        return 1f - m / 1073741824f;
    }

    static Color TileFill(float durability)
    {
        float t = Math.Clamp(durability / MaxDurability, 0f, 1f);
        return GetTileHealthVisual(t) switch
        {
            TileHealthVisual.Pristine => LerpColor(MossLight, Lighten(Emerald, 0.08f), Math.Clamp((t - 0.92f) / 0.08f, 0f, 1f)),
            TileHealthVisual.Sturdy => LerpColor(MossLight, Emerald, (t - 0.7f) / 0.22f),
            TileHealthVisual.Worn => LerpColor(Amber, MossLight, (t - 0.5f) / 0.2f),
            TileHealthVisual.Cracked => LerpColor(new Color(62, 58, 54, 255), Amber, (t - 0.35f) / 0.15f),
            TileHealthVisual.Fractured => LerpColor(new Color(48, 42, 40, 255), new Color(72, 58, 52, 255), (t - 0.2f) / 0.15f),
            TileHealthVisual.Critical => LerpColor(new Color(36, 30, 28, 255), new Color(58, 44, 40, 255), t / 0.2f),
            _ => Danger,
        };
    }

    static TileHealthVisual GetTileHealthVisual(float ratio)
    {
        if (ratio >= 0.92f) return TileHealthVisual.Pristine;
        if (ratio >= 0.7f) return TileHealthVisual.Sturdy;
        if (ratio >= 0.5f) return TileHealthVisual.Worn;
        if (ratio >= 0.35f) return TileHealthVisual.Cracked;
        if (ratio >= 0.2f) return TileHealthVisual.Fractured;
        return TileHealthVisual.Critical;
    }

    static Color TileHealthAccent(TileHealthVisual band) => band switch
    {
        TileHealthVisual.Pristine => Lighten(MossLight, 0.35f),
        TileHealthVisual.Sturdy => MossLight,
        TileHealthVisual.Worn => Amber,
        TileHealthVisual.Cracked => new Color(88, 72, 58, 255),
        TileHealthVisual.Fractured => new Color(96, 62, 52, 255),
        TileHealthVisual.Critical => new Color(118, 52, 44, 255),
        _ => Danger,
    };

    static void DrawTileStonePattern(Rectangle bodyRect, int x, int y, Color fill, TileHealthVisual band)
    {
        if (band is not (TileHealthVisual.Pristine or TileHealthVisual.Sturdy or TileHealthVisual.Worn)) return;

        float inset = 3f;
        float cellW = (bodyRect.Width - inset * 2f) / 2f - 0.5f;
        float cellH = (bodyRect.Height - inset * 2f) / 2f - 0.5f;
        int seed = x * 48271 + y * 29347;

        for (int gy = 0; gy < 2; gy++)
        {
            for (int gx = 0; gx < 2; gx++)
            {
                float jitter = Hash(seed + gx + gy * 2) * 0.04f;
                bool light = (gx + gy + x + y) % 2 == 0;
                Color cell = light ? Lighten(fill, 0.05f + jitter) : Darken(fill, 0.04f + jitter);
                if (band == TileHealthVisual.Worn && Hash(seed + gx * 3 + gy) > 0.55f)
                {
                    cell = Darken(cell, 0.08f);
                }

                var cellRect = new Rectangle(
                    bodyRect.X + inset + gx * (cellW + 1f),
                    bodyRect.Y + inset + gy * (cellH + 1f),
                    cellW, cellH);
                Raylib.DrawRectangleRounded(cellRect, 0.08f, 4, WithAlpha(cell, band == TileHealthVisual.Pristine ? 0.42f : 0.3f));
            }
        }

        float mortarA = band == TileHealthVisual.Pristine ? 0.18f : 0.12f;
        float midX = bodyRect.X + bodyRect.Width / 2f;
        float midY = bodyRect.Y + bodyRect.Height / 2f;
        Raylib.DrawLineEx(new Vector2(midX, bodyRect.Y + inset), new Vector2(midX, bodyRect.Y + bodyRect.Height - inset), 0.8f,
            WithAlpha(Darken(fill, 0.2f), mortarA));
        Raylib.DrawLineEx(new Vector2(bodyRect.X + inset, midY), new Vector2(bodyRect.X + bodyRect.Width - inset, midY), 0.8f,
            WithAlpha(Darken(fill, 0.2f), mortarA));
    }

    static void DrawTileMossFlecks(Rectangle bodyRect, int x, int y)
    {
        int seed = x * 92821 + y * 68917;
        for (int i = 0; i < 4; i++)
        {
            float fx = bodyRect.X + 5f + Hash(seed + i * 5) * (bodyRect.Width - 10f);
            float fy = bodyRect.Y + 5f + Hash(seed + i * 5 + 2) * (bodyRect.Height - 10f);
            float r = 1f + Hash(seed + i * 5 + 4) * 1.4f;
            Raylib.DrawCircleV(new Vector2(fx, fy), r, WithAlpha(Lighten(MossLight, 0.25f), 0.38f));
            Raylib.DrawCircleV(new Vector2(fx + 0.6f, fy - 0.4f), r * 0.45f, WithAlpha(LeafGold, 0.22f));
        }
    }

    static void DrawTileCornerChips(Rectangle bodyRect, int x, int y, Color fill, float severity)
    {
        int seed = x * 73856093 ^ y * 19349663;
        Color chip = WithAlpha(Darken(fill, 0.35f), 0.45f + severity * 0.35f);
        float s = 3f + severity * 2.5f;

        if (Hash(seed) > 0.25f)
        {
            Raylib.DrawTriangle(
                new Vector2(bodyRect.X + 2f, bodyRect.Y + 2f),
                new Vector2(bodyRect.X + 2f + s, bodyRect.Y + 2f),
                new Vector2(bodyRect.X + 2f, bodyRect.Y + 2f + s),
                chip);
        }
        if (Hash(seed + 11) > 0.35f)
        {
            float br = bodyRect.X + bodyRect.Width;
            float bb = bodyRect.Y + bodyRect.Height;
            Raylib.DrawTriangle(
                new Vector2(br - 2f, bb - 2f),
                new Vector2(br - 2f - s, bb - 2f),
                new Vector2(br - 2f, bb - 2f - s),
                chip);
        }
        if (severity > 0.45f && Hash(seed + 23) > 0.4f)
        {
            float br = bodyRect.X + bodyRect.Width;
            Raylib.DrawTriangle(
                new Vector2(br - 2f, bodyRect.Y + 2f),
                new Vector2(br - 2f - s, bodyRect.Y + 2f),
                new Vector2(br - 2f, bodyRect.Y + 2f + s),
                chip);
        }
    }

    static void DrawTileHealthMeter(Rectangle bodyRect, float ratio, TileHealthVisual band)
    {
        if (band is TileHealthVisual.Pristine or TileHealthVisual.Sturdy) return;

        float barH = 3f;
        float barW = bodyRect.Width - 8f;
        float bx = bodyRect.X + 4f;
        float by = bodyRect.Y + bodyRect.Height - barH - 3f;
        var track = new Rectangle(bx, by, barW, barH);

        Raylib.DrawRectangleRounded(track, 0.5f, 3, WithAlpha(ForestShadow, 0.62f));
        Color barCol = TileHealthAccent(band);
        float fillW = MathF.Max(0f, barW * ratio);
        if (fillW > 0.5f)
        {
            var fill = new Rectangle(bx, by, fillW, barH);
            Raylib.DrawRectangleRounded(fill, 0.5f, 3, WithAlpha(Darken(barCol, 0.15f), 0.88f));
            Raylib.DrawRectangleGradientV((int)fill.X, (int)fill.Y, (int)fill.Width, (int)fill.Height,
                WithAlpha(Lighten(barCol, 0.22f), 0.55f), WithAlpha(barCol, 0.78f));
        }
        Raylib.DrawRectangleRoundedLines(track, 0.5f, 3, WithAlpha(Darken(barCol, 0.2f), 0.5f));
    }

    static void DrawTileHealthSurface(
        int x, int y, float cx, float cy, Rectangle bodyRect, Color fill,
        float ratio, TileHealthVisual band, Tile tile, float time)
    {
        switch (band)
        {
            case TileHealthVisual.Pristine:
                DrawTileStonePattern(bodyRect, x, y, fill, band);
                DrawTileMossFlecks(bodyRect, x, y);
                Raylib.DrawRectangleRoundedLines(bodyRect, 0.12f, 6, WithAlpha(Lighten(MossLight, 0.2f), 0.22f));
                break;

            case TileHealthVisual.Sturdy:
                DrawTileStonePattern(bodyRect, x, y, fill, band);
                if ((x + y) % 5 == 0)
                {
                    DrawTileMossFlecks(bodyRect, x, y);
                }
                break;

            case TileHealthVisual.Worn:
                DrawTileStonePattern(bodyRect, x, y, fill, band);
                DrawTileCornerChips(bodyRect, x, y, fill, 0.25f);
                DrawCracks(cx, cy, x, y, 0.22f, 2);
                DrawTileHealthMeter(bodyRect, ratio, band);
                break;

            case TileHealthVisual.Cracked:
                DrawTileCornerChips(bodyRect, x, y, fill, 0.5f);
                DrawCracks(cx, cy, x, y, 0.48f, 4);
                Raylib.DrawRectangleRounded(bodyRect, 0.12f, 6, WithAlpha(Amber, 0.08f));
                DrawTileHealthMeter(bodyRect, ratio, band);
                break;

            case TileHealthVisual.Fractured:
                DrawTileCornerChips(bodyRect, x, y, fill, 0.72f);
                DrawCracks(cx, cy, x, y, 0.72f, 6);
                DrawFractureWeb(cx, cy, x, y, 0.55f);
                {
                    float dust = MathF.Sin(time * 9f + x + y) * 0.5f + 0.5f;
                    Raylib.DrawCircleV(new Vector2(cx, cy - 2f), 1.2f + dust, WithAlpha(Amber, 0.25f + dust * 0.2f));
                }
                DrawTileHealthMeter(bodyRect, ratio, band);
                if (tile.State == 1)
                {
                    float pulse = MathF.Sin(time * 7f + x + y) * 0.5f + 0.5f;
                    Raylib.DrawRectangleRoundedLines(bodyRect, 0.12f, 6, WithAlpha(TileHealthAccent(band), pulse * 0.55f));
                }
                break;

            case TileHealthVisual.Critical:
                DrawTileCornerChips(bodyRect, x, y, fill, 1f);
                DrawCracks(cx, cy, x, y, 1f, 7);
                DrawFractureWeb(cx, cy, x, y, 0.85f);
                {
                    float pulse = MathF.Sin(time * 11f + x * 1.3f + y) * 0.5f + 0.5f;
                    float wobble = MathF.Sin(time * 14f + x * 2.1f + y * 1.7f) * 0.35f;
                    Raylib.DrawRectangleRounded(
                        new Rectangle(bodyRect.X + wobble, bodyRect.Y, bodyRect.Width, bodyRect.Height),
                        0.12f, 6, WithAlpha(TileHealthAccent(band), 0.12f + pulse * 0.18f));
                    Raylib.DrawRectangleRoundedLines(bodyRect, 0.12f, 6, WithAlpha(TileHealthAccent(band), 0.35f + pulse * 0.45f));
                    for (int d = 0; d < 3; d++)
                    {
                        float da = Hash(x * 991 + y * 677 + d) * MathF.PI * 2f;
                        var dustPos = new Vector2(cx + MathF.Cos(da + time * 2f) * 5f, cy + MathF.Sin(da + time * 2f) * 5f);
                        Raylib.DrawCircleV(dustPos, 1f + pulse * 0.8f, WithAlpha(Amber, 0.2f + pulse * 0.25f));
                    }
                }
                DrawTileHealthMeter(bodyRect, ratio, band);
                break;

            default:
                throw new UnreachableException();
        }
    }

    static void DrawFractureWeb(float cx, float cy, int x, int y, float severity)
    {
        int seed = x * 83492791 ^ y * 41524173;
        Color web = WithAlpha(ForestShadow, severity * 0.65f);
        for (int k = 0; k < 4; k++)
        {
            float a = Hash(seed + k * 13) * MathF.PI * 2f;
            float len = TileSize * (0.18f + Hash(seed + k * 17) * 0.22f);
            var hub = new Vector2(cx + MathF.Cos(a) * 2f, cy + MathF.Sin(a) * 2f);
            var end = new Vector2(cx + MathF.Cos(a + Hash(seed + k) * 0.8f) * len, cy + MathF.Sin(a + Hash(seed + k) * 0.8f) * len);
            Raylib.DrawLineEx(hub, end, 1.2f, web);
            var branch = new Vector2(
                (hub.X + end.X) * 0.5f + MathF.Cos(a + 1.2f) * 4f,
                (hub.Y + end.Y) * 0.5f + MathF.Sin(a + 1.2f) * 4f);
            Raylib.DrawLineEx(hub, branch, 1f, WithAlpha(web, severity * 0.8f));
        }
    }

    static string KeyName(KeyboardKey k)
    {
        int v = (int)k;
        if (v >= 65 && v <= 90) return ((char)v).ToString();
        if (v >= 48 && v <= 57) return ((char)v).ToString();
        return k switch
        {
            KeyboardKey.Space => "SPACE",
            KeyboardKey.LeftShift => "LSHIFT",
            KeyboardKey.LeftControl => "LCTRL",
            KeyboardKey.Enter => "ENTER",
            KeyboardKey.Null => "NONE",
            _ => v.ToString(),
        };
    }

}
