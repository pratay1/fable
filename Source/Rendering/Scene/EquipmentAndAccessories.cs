partial class Program
{
    static float LerpFloat(float a, float b, float t) => a + (b - a) * Math.Clamp(t, 0f, 1f);

    static Vector2 GetEquippedMuzzleWorld(Vector2 grip, float scale, float rotDeg, int gunIndex)
    {
        float rad = rotDeg * MathF.PI / 180f;
        Vector2 local = GunLocalMuzzlePoint(gunIndex, GunIconGripOffset(gunIndex, scale), scale) - GunIconGripOffset(gunIndex, scale);
        float c = MathF.Cos(rad);
        float s = MathF.Sin(rad);
        return grip + new Vector2(c * local.X - s * local.Y, s * local.X + c * local.Y);
    }

    static void DrawGunIconAtPivot(int index, Vector2 gripWorld, float size, float time, float rotationDeg, float alpha,
        bool worldHeld = false, bool outline = false)
    {
        Rlgl.PushMatrix();
        Rlgl.Translatef(gripWorld.X, gripWorld.Y, 0f);
        Rlgl.Rotatef(rotationDeg, 0f, 0f, 1f);

        Vector2 localCenter = GunIconGripOffset(index, size);
        ref readonly Gun gun = ref Guns[index];
        Color hi = WithAlpha(Lighten(gun.Color, worldHeld ? 0.62f : 0.22f), alpha);

        if (outline)
        {
            Color outlineColor = WithAlpha(ForestShadow, alpha * 0.95f);
            DrawGunIconCore(index, localCenter, size, time, alpha, worldHeld: true, outlineColor: outlineColor);
        }
        else
        {
            DrawGunIconCore(index, localCenter, size, time, alpha, worldHeld);
        }

        if (worldHeld && !outline)
        {
            Vector2 muzzle = GunLocalMuzzlePoint(index, localCenter, size);
            Raylib.DrawLineEx(localCenter + new Vector2(size * 0.05f, 0f), muzzle, 4f, WithAlpha(hi, 0.85f));
            DrawGlow(muzzle, size * 0.2f, hi, 0.28f);
            Raylib.DrawCircleV(muzzle, size * 0.07f, WithAlpha(Color.White, alpha));
        }

        Rlgl.PopMatrix();
    }

    static void DrawGunIcon(int index, Vector2 center, float size, float time, float alpha = 1f)
        => DrawGunIconCore(index, center, size, time, alpha);

    static void DrawGunIconCore(int index, Vector2 center, float size, float time, float alpha = 1f,
        bool worldHeld = false, Color? outlineColor = null)
    {
        ref readonly Gun g = ref Guns[index];
        Color basePrimary = worldHeld ? Lighten(g.Color, 0.42f) : g.Color;
        Color baseShadow = worldHeld ? Darken(g.Color, 0.1f) : Darken(g.Color, 0.35f);
        Color baseHi = worldHeld ? Lighten(g.Color, 0.68f) : Lighten(g.Color, 0.22f);

        Color primary = outlineColor ?? WithAlpha(basePrimary, alpha);
        Color shadow = outlineColor ?? WithAlpha(baseShadow, alpha);
        Color hi = outlineColor ?? WithAlpha(baseHi, alpha);
        float lineMul = worldHeld ? 2.6f : 1f;
        float L(float w) => w * lineMul;
        float s = size;

        switch (index)
        {
            case 0:
                Raylib.DrawLineEx(center + new Vector2(-s * 0.45f, s * 0.28f), center + new Vector2(s * 0.35f, -s * 0.42f), L(2f), shadow);
                Raylib.DrawLineEx(center + new Vector2(s * 0.35f, -s * 0.42f), center + new Vector2(s * 0.48f, -s * 0.52f), L(1.5f), primary);
                Raylib.DrawCircleV(center + new Vector2(s * 0.5f, -s * 0.55f), s * 0.09f * lineMul, hi);
                break;
            case 1:
                DrawGunArchetype(3, center, s, time, primary, shadow, hi, lineMul);
                break;
            case 2:
                Raylib.DrawLineEx(center + new Vector2(-s * 0.22f, s * 0.18f), center + new Vector2(s * 0.22f, -s * 0.18f), L(2.2f), primary);
                Raylib.DrawLineEx(center + new Vector2(s * 0.22f, s * 0.18f), center + new Vector2(-s * 0.22f, -s * 0.18f), L(2.2f), hi);
                Raylib.DrawCircleV(center + new Vector2(-s * 0.28f, -s * 0.24f), s * 0.05f * lineMul, shadow);
                Raylib.DrawCircleV(center + new Vector2(s * 0.28f, s * 0.24f), s * 0.05f * lineMul, shadow);
                break;
            case 3:
                DrawGunArchetype(9, center, s, time, primary, shadow, hi, lineMul);
                break;
            case 4:
                DrawGunArchetype(8, center, s, time, primary, shadow, hi, lineMul);
                break;
            case 5:
                DrawGunArchetype(6, center, s, time, primary, shadow, hi, lineMul);
                break;
            case 6:
                DrawGunArchetype(2, center, s * 1.05f, time, primary, shadow, hi, lineMul);
                Raylib.DrawRectangle((int)(center.X - s * 0.18f), (int)(center.Y + s * 0.08f), (int)(s * 0.36f), (int)(s * 0.06f * lineMul), hi);
                break;
            case 7:
                for (int b = -1; b <= 1; b++)
                    DrawGunArchetype(6, center + new Vector2(b * s * 0.14f, b * s * 0.06f), s * 0.55f, time + b, primary, shadow, hi, lineMul);
                break;
            default:
                DrawGunArchetype(index - 8, center, s, time, primary, shadow, hi, lineMul);
                break;
        }
    }

    enum AccessoryLayer { Back, Mid, Front }

    readonly struct PlayerAccessoryRig
    {
        public readonly Vector2 Center;
        public readonly float Radius;
        public readonly Vector2 Forward;
        public readonly Vector2 Left;

        public PlayerAccessoryRig(Vector2 center, float radius, Vector2 forward)
        {
            Center = center;
            Radius = radius;
            Forward = forward;
            Left = new Vector2(-forward.Y, forward.X);
        }

        public Vector2 At(float forwardMul, float sideMul)
            => Center + Forward * (forwardMul * Radius) + Left * (sideMul * Radius);

        public Vector2 Screen(float xMul, float yMul) => At(yMul, -xMul);

        public float FacingDeg => MathF.Atan2(Forward.Y, Forward.X) * 180f / MathF.PI;
    }

    static readonly Vector2 AccessoryPreviewForward = Vector2.UnitY;

    static PlayerAccessoryRig BuildAccessoryRig(Vector2 center, float radius, Vector2 forward)
    {
        if (forward.LengthSquared() < 0.001f) forward = Vector2.UnitY;
        return new PlayerAccessoryRig(center, radius, Vector2.Normalize(forward));
    }

    static PlayerAccessoryRig BuildAccessoryRig(Vector2 center, float radius)
        => BuildAccessoryRig(center, radius, ResolveWeaponAimDir());

    static AccessoryLayer AccessoryLayerFor(int idx) => idx switch
    {
        9 or 15 or 21 or 25 or 32 => AccessoryLayer.Back,
        3 or 4 or 7 or 11 or 12 or 14 or 19 or 23 or 24 or 28 or 29 or 34 => AccessoryLayer.Mid,
        _ => AccessoryLayer.Front,
    };

    static void WithAccessoryRigRotation(in PlayerAccessoryRig rig, Action draw)
    {
        Rlgl.PushMatrix();
        Rlgl.Translatef(rig.Center.X, rig.Center.Y, 0f);
        Rlgl.Rotatef(rig.FacingDeg - 90f, 0f, 0f, 1f);
        draw();
        Rlgl.PopMatrix();
    }

    static void DrawAccessory(Vector2 p, float r, float time) => DrawAccessory(p, r, time, accessoryIndex);

    static void DrawAccessory(Vector2 p, float r, float time, int idx)
        => DrawAccessory(p, r, time, idx, ResolveWeaponAimDir());

    static void DrawAccessory(Vector2 p, float r, float time, int idx, Vector2 forward)
    {
        if (idx == 0) return;
        var rig = BuildAccessoryRig(p, r, forward);
        DrawAccessoryLayer(rig, time, idx, AccessoryLayer.Back);
        DrawAccessoryLayer(rig, time, idx, AccessoryLayer.Mid);
        DrawAccessoryLayer(rig, time, idx, AccessoryLayer.Front);
    }

    static void DrawTrinketGem(Vector2 c, float radius, Color core, float time, float glowMul = 1f)
    {
        float tw = MathF.Sin(time * 3.2f + c.X * 0.03f + c.Y * 0.05f) * 0.5f + 0.5f;
        DrawGlow(c, radius * 3.6f, core, (0.05f + tw * 0.06f) * glowMul);
        Raylib.DrawCircleV(c, radius * 1.32f, WithAlpha(Darken(core, 0.5f), 0.95f));
        Raylib.DrawCircleV(c, radius, core);
        Raylib.DrawCircleV(c, radius * 0.94f, WithAlpha(Lighten(core, 0.5f), 0.3f));
        Raylib.DrawCircleV(c - new Vector2(radius * 0.32f, radius * 0.34f), radius * 0.36f, WithAlpha(Color.White, 0.55f + tw * 0.35f));
    }

    static void DrawMetalBead(Vector2 c, float radius, Color baseCol)
    {
        Raylib.DrawCircleV(c + new Vector2(radius * 0.16f, radius * 0.22f), radius, WithAlpha(Darken(baseCol, 0.45f), 0.92f));
        Raylib.DrawCircleV(c, radius, baseCol);
        Raylib.DrawCircleV(c - new Vector2(radius * 0.3f, radius * 0.32f), radius * 0.42f, WithAlpha(Lighten(baseCol, 0.55f), 0.85f));
    }

    static void DrawJeweledBand(Vector2 c, float rInner, float rOuter, float startDeg, float endDeg, int seg, Color metal, Color hi)
    {
        Raylib.DrawRing(c, rInner, rOuter, startDeg, endDeg, seg, Darken(metal, 0.3f));
        Raylib.DrawRing(c, rInner, rInner + (rOuter - rInner) * 0.62f, startDeg, endDeg, seg, metal);
        Raylib.DrawRing(c, rOuter - (rOuter - rInner) * 0.34f, rOuter, startDeg, endDeg, seg, WithAlpha(hi, 0.6f));
    }

    static void DrawCursorCrown(in PlayerAccessoryRig rig, float time, float r)
    {
        Color gold = new Color(214, 174, 90, 255);
        Color goldHi = new Color(255, 234, 158, 255);
        Color goldDark = new Color(122, 92, 42, 255);
        Color velvet = new Color(28, 22, 48, 255);
        Color velvetHi = new Color(58, 44, 88, 255);
        Color ruby = new Color(196, 52, 70, 255);
        Color sapphire = new Color(66, 104, 184, 255);
        Color emerald = new Color(58, 152, 118, 255);
        Color cursorGem = new Color(118, 196, 255, 255);
        Color cursorGemHi = new Color(220, 244, 255, 255);
        Color pearl = new Color(232, 226, 214, 255);
        float pulse = MathF.Sin(time * 2.4f) * 0.5f + 0.5f;
        float glint = MathF.Sin(time * 5f) * 0.5f + 0.5f;

        WithAccessoryRigRotation(rig, () =>
        {
            Vector2 crownBase = new Vector2(0f, -r * 0.58f);

            // Velvet cap interior
            Raylib.DrawCircleSector(crownBase, r * 0.72f, 200f, 340f, 28, velvet);
            Raylib.DrawCircleSector(crownBase, r * 0.66f, 204f, 336f, 26, Darken(velvet, 0.15f));
            Raylib.DrawCircleSector(crownBase, r * 0.58f, 208f, 332f, 24, velvetHi);

            // Lower circlet band with pearls
            DrawJeweledBand(crownBase, r * 0.54f, r * 0.68f, 198f, 342f, 32, gold, goldHi);
            for (int p = 0; p < 9; p++)
            {
                float ang = (198f + p * 18f) * MathF.PI / 180f;
                Vector2 pearlPos = crownBase + new Vector2(MathF.Cos(ang), MathF.Sin(ang)) * r * 0.61f;
                DrawMetalBead(pearlPos, r * 0.045f, pearl);
            }

            // Main crown body - arched gallery
            Raylib.DrawRing(crownBase, r * 0.48f, r * 0.56f, 196f, 344f, 30, goldDark);
            Raylib.DrawRing(crownBase, r * 0.50f, r * 0.54f, 198f, 342f, 28, gold);
            for (int arch = 0; arch < 7; arch++)
            {
                float ax = -r * 0.42f + arch * r * 0.14f;
                Raylib.DrawLineEx(new Vector2(ax, -r * 0.52f), new Vector2(ax, -r * 0.78f), 1.2f, WithAlpha(goldHi, 0.35f));
                Raylib.DrawCircleV(new Vector2(ax, -r * 0.8f), 1.4f, WithAlpha(gold, 0.5f));
            }

            // Five royal spires
            ReadOnlySpan<float> spireX = stackalloc float[] { -r * 0.38f, -r * 0.19f, 0f, r * 0.19f, r * 0.38f };
            ReadOnlySpan<float> spireH = stackalloc float[] { 0.34f, 0.42f, 0.52f, 0.42f, 0.34f };
            ReadOnlySpan<Color> spireGems = stackalloc Color[] { sapphire, emerald, cursorGem, ruby, sapphire };
            for (int s = 0; s < 5; s++)
            {
                float sx = spireX[s];
                float top = -r * (0.78f + spireH[s]);
                var body = new Rectangle(sx - r * 0.055f, top, r * 0.11f, r * spireH[s]);
                Raylib.DrawRectangleRounded(body, 0.2f, 4, goldDark);
                Raylib.DrawRectangleRounded(new Rectangle(body.X + 1f, body.Y + 1f, body.Width - 2f, body.Height * 0.55f), 0.2f, 4, gold);
                Raylib.DrawRectangleRounded(new Rectangle(body.X + 1.5f, body.Y + 1.5f, body.Width - 3f, body.Height * 0.22f), 0.25f, 3, WithAlpha(goldHi, 0.75f));

                Vector2 tip = new Vector2(sx, top - r * 0.04f);
                Raylib.DrawTriangle(
                    tip,
                    new Vector2(sx - r * 0.07f, top + r * 0.06f),
                    new Vector2(sx + r * 0.07f, top + r * 0.06f),
                    goldHi);
                Raylib.DrawTriangle(
                    tip + new Vector2(0f, r * 0.02f),
                    new Vector2(sx - r * 0.04f, top + r * 0.05f),
                    new Vector2(sx + r * 0.04f, top + r * 0.05f),
                    gold);

                Vector2 gemPos = new Vector2(sx, top + r * spireH[s] * 0.42f);
                if (s == 2)
                {
                    DrawGlow(gemPos, r * 0.35f, cursorGem, 0.08f + pulse * 0.06f);
                    Raylib.DrawCircleV(gemPos, r * 0.11f, WithAlpha(Darken(cursorGem, 0.4f), 0.95f));
                    Raylib.DrawCircleV(gemPos, r * 0.085f, cursorGem);
                    Raylib.DrawCircleV(gemPos - new Vector2(r * 0.028f, r * 0.03f), r * 0.032f, WithAlpha(cursorGemHi, 0.9f));
                    for (int ray = 0; ray < 4; ray++)
                    {
                        float ra = time * 1.6f + ray * 1.57f;
                        Vector2 rp = gemPos + new Vector2(MathF.Cos(ra), MathF.Sin(ra)) * r * 0.14f;
                        Raylib.DrawLineEx(gemPos, rp, 1f, WithAlpha(cursorGemHi, 0.25f + glint * 0.2f));
                    }
                }
                else
                {
                    DrawTrinketGem(gemPos, r * 0.07f, spireGems[s], time, 0.85f);
                }
            }

            // Filigree scrollwork on band
            for (int f = 0; f < 6; f++)
            {
                float fx = -r * 0.33f + f * r * 0.132f;
                float fy = -r * 0.64f;
                Raylib.DrawLineEx(new Vector2(fx - r * 0.03f, fy), new Vector2(fx, fy - r * 0.04f), 1f, WithAlpha(goldHi, 0.4f));
                Raylib.DrawLineEx(new Vector2(fx, fy - r * 0.04f), new Vector2(fx + r * 0.03f, fy), 1f, WithAlpha(goldHi, 0.4f));
            }

            // Side fleurs
            for (int side = -1; side <= 1; side += 2)
            {
                float fx = side * r * 0.46f;
                float fy = -r * 0.66f;
                Raylib.DrawCircleV(new Vector2(fx, fy), r * 0.04f, gold);
                Raylib.DrawLineEx(new Vector2(fx, fy), new Vector2(fx + side * r * 0.08f, fy - r * 0.06f), 1.2f, goldHi);
                Raylib.DrawLineEx(new Vector2(fx, fy), new Vector2(fx + side * r * 0.08f, fy + r * 0.04f), 1.2f, goldHi);
                DrawTrinketGem(new Vector2(fx, fy - r * 0.02f), r * 0.035f, emerald, time, 0.5f);
            }

            // Halo glow behind crown
            DrawGlow(new Vector2(0f, -r * 0.72f), r * 1.1f, gold, 0.05f + pulse * 0.04f);
            DrawGlow(new Vector2(0f, -r * 0.95f), r * 0.55f, cursorGem, 0.04f + glint * 0.03f);
        });
    }

    static void DrawAccessoryLayer(in PlayerAccessoryRig rig, float time, int idx, AccessoryLayer layer)
    {
        if (idx == 0 || AccessoryLayerFor(idx) != layer) return;

        float r = rig.Radius;
        Color iron = new Color(126, 124, 122, 255);
        Color ironHi = new Color(196, 196, 192, 255);
        Color ironDark = new Color(56, 54, 54, 255);
        Color steel = new Color(150, 160, 176, 255);
        Color steelHi = new Color(224, 232, 244, 255);
        Color steelDark = new Color(64, 72, 86, 255);
        Color gold = new Color(214, 174, 90, 255);
        Color goldHi = new Color(255, 234, 158, 255);
        Color goldDark = new Color(122, 92, 42, 255);
        Color cloth = new Color(58, 56, 66, 255);
        Color clothHi = new Color(108, 102, 120, 255);
        Color crimson = new Color(154, 46, 58, 255);
        Color crimsonHi = new Color(220, 96, 100, 255);
        Color plum = new Color(110, 74, 132, 255);
        Color plumHi = new Color(176, 126, 200, 255);
        Color slate = new Color(92, 104, 124, 255);
        Color copper = new Color(176, 112, 74, 255);
        Color copperHi = new Color(232, 168, 116, 255);
        Color ruby = new Color(196, 52, 70, 255);
        Color sapphire = new Color(66, 104, 184, 255);
        Color emerald = new Color(58, 152, 118, 255);
        Color amethyst = new Color(150, 96, 196, 255);
        Color amber = new Color(224, 150, 70, 255);
        Color bone = new Color(208, 198, 180, 255);
        Color boneHi = new Color(240, 234, 220, 255);
        Color flame = new Color(255, 176, 96, 255);
        float pulse = MathF.Sin(time * 2.2f + idx) * 0.5f + 0.5f;
        float glint = MathF.Sin(time * 4f + idx * 1.3f) * 0.5f + 0.5f;
        float rot = rig.FacingDeg - 90f;

        switch (idx)
        {
            case 1: // Iron Circlet - golden jeweled circlet
            {
                Vector2 head = rig.At(-0.55f, 0f);
                DrawJeweledBand(head, r * 0.6f, r * 0.74f, rot + 196f, rot + 344f, 24, gold, goldHi);
                DrawMetalBead(rig.At(-0.78f, -0.42f), r * 0.06f, goldHi);
                DrawMetalBead(rig.At(-0.78f, 0.42f), r * 0.06f, goldHi);
                DrawTrinketGem(rig.At(-0.92f, 0f), r * 0.13f, ruby, time);
                break;
            }
            case 2: // Hood - draped cloth with shadowed opening
                WithAccessoryRigRotation(rig, () =>
                {
                    Vector2 hc = new Vector2(0f, -r * 0.2f);
                    Raylib.DrawCircleSector(hc, r * 1.06f, 192f, 348f, 26, Darken(cloth, 0.35f));
                    Raylib.DrawCircleSector(hc, r * 0.99f, 196f, 344f, 24, cloth);
                    Raylib.DrawCircleSector(hc, r * 0.82f, 202f, 338f, 22, Lighten(cloth, 0.12f));
                    Raylib.DrawCircleSector(hc, r * 0.64f, 206f, 334f, 20, new Color(16, 14, 20, 255));
                    Raylib.DrawRing(hc, r * 0.99f, r * 1.06f, 192f, 348f, 26, WithAlpha(clothHi, 0.5f));
                });
                break;
            case 3: // Pauldrons - layered steel shoulder guards
                WithAccessoryRigRotation(rig, () =>
                {
                    for (int s = -1; s <= 1; s += 2)
                    {
                        float bx = s < 0 ? -r * 0.92f : r * 0.37f;
                        var pl = new Rectangle(bx, -r * 0.42f, r * 0.55f, r * 0.5f);
                        Raylib.DrawRectangleRounded(pl, 0.3f, 5, Darken(steel, 0.3f));
                        Raylib.DrawRectangleRounded(new Rectangle(pl.X + 1.5f, pl.Y + 1.5f, pl.Width - 3f, pl.Height * 0.55f), 0.3f, 5, steel);
                        Raylib.DrawRectangleRounded(new Rectangle(pl.X + 2f, pl.Y + 2f, pl.Width - 4f, pl.Height * 0.22f), 0.4f, 4, WithAlpha(steelHi, 0.7f));
                        Raylib.DrawRectangleRoundedLines(pl, 0.3f, 5, WithAlpha(gold, 0.5f));
                        Raylib.DrawCircleV(new Vector2(pl.X + pl.Width * 0.5f, pl.Y + pl.Height * 0.7f), 2f, goldHi);
                    }
                });
                break;
            case 4: // Cloak Pin - golden clasp with gem
            {
                Vector2 clasp = rig.Screen(0.58f, -0.08f);
                Vector2 anchor = rig.Screen(-0.15f, -0.35f);
                Raylib.DrawLineEx(anchor, clasp, 3f, goldDark);
                Raylib.DrawLineEx(anchor, clasp, 1.5f, goldHi);
                Raylib.DrawCircleV(clasp, 5f, gold);
                DrawTrinketGem(clasp, 3.2f, ruby, time);
                break;
            }
            case 5: // Torch Bearer - lit torch with living flame
                WithAccessoryRigRotation(rig, () =>
                {
                    Vector2 local = new Vector2(-r * 0.78f, -r * 0.12f);
                    Raylib.DrawRectangleRounded(new Rectangle(local.X - 3f, local.Y, 6f, r * 0.4f), 0.3f, 3, new Color(74, 52, 36, 255));
                    Raylib.DrawLineEx(local + new Vector2(0, 2f), local + new Vector2(0, r * 0.36f), 1.5f, WithAlpha(copperHi, 0.5f));
                    Vector2 fb = local + new Vector2(0, -3f);
                    float fl = 0.8f + pulse * 0.4f;
                    DrawGlow(fb, r * 0.7f, flame, 0.12f + pulse * 0.06f);
                    Raylib.DrawCircleV(fb, 5f * fl, WithAlpha(new Color(180, 70, 40, 255), 0.9f));
                    Raylib.DrawCircleV(fb + new Vector2(0, -2f), 3.4f * fl, flame);
                    Raylib.DrawCircleV(fb + new Vector2(0, -3.5f), 1.8f * fl, WithAlpha(boneHi, 0.9f));
                });
                break;
            case 6: // Chain Coif - interlocking mail
                WithAccessoryRigRotation(rig, () =>
                {
                    for (int row = 0; row < 4; row++)
                    {
                        float ry = -r * (0.75f - row * 0.12f);
                        for (int col = -3; col <= 3; col++)
                        {
                            Vector2 lp = new Vector2(col * r * 0.14f + (row % 2) * r * 0.07f, ry);
                            Raylib.DrawCircleV(lp, 2.6f, WithAlpha(steelDark, 0.85f));
                            Raylib.DrawCircleLinesV(lp, 2.6f, WithAlpha(steelHi, 0.7f));
                            Raylib.DrawCircleV(lp - new Vector2(0.8f, 0.8f), 0.9f, WithAlpha(steelHi, 0.6f));
                        }
                    }
                });
                break;
            case 7: // Belt Pouch - leather satchel with gold buckle
                WithAccessoryRigRotation(rig, () =>
                {
                    var belt = new Rectangle(-r * 0.5f, r * 0.16f, r * 1.0f, r * 0.14f);
                    Raylib.DrawRectangleRounded(belt, 0.4f, 3, new Color(58, 42, 34, 255));
                    var pouch = new Rectangle(-r * 0.22f, r * 0.22f, r * 0.44f, r * 0.4f);
                    Raylib.DrawRectangleRounded(pouch, 0.35f, 4, new Color(96, 66, 46, 255));
                    Raylib.DrawRectangleRounded(new Rectangle(pouch.X + 1.5f, pouch.Y + 1.5f, pouch.Width - 3f, pouch.Height * 0.4f), 0.35f, 4, new Color(126, 90, 62, 255));
                    Raylib.DrawRectangleRoundedLines(pouch, 0.35f, 4, WithAlpha(new Color(40, 28, 22, 255), 0.8f));
                    Raylib.DrawRectangleRounded(new Rectangle(-r * 0.07f, r * 0.2f, r * 0.14f, r * 0.1f), 0.3f, 2, gold);
                    Raylib.DrawCircleV(new Vector2(0f, r * 0.25f), 1.6f, goldHi);
                });
                break;
            case 8: // Signet Ring - gold band with engraved gem
            {
                Vector2 finger = rig.Screen(-0.58f, 0.12f);
                Raylib.DrawCircleV(finger, 6f, goldDark);
                Raylib.DrawCircleV(finger, 5f, gold);
                Raylib.DrawCircleLinesV(finger, 6.5f, WithAlpha(goldHi, 0.8f));
                DrawTrinketGem(finger, 2.8f, sapphire, time, 0.8f);
                break;
            }
            case 9: // Warden Cape - flowing cape with trim
                WithAccessoryRigRotation(rig, () =>
                {
                    float sway = MathF.Sin(time * 1.6f) * r * 0.05f;
                    Vector2 top = new Vector2(0f, -r * 0.22f);
                    Raylib.DrawTriangle(top, new Vector2(-r * 0.98f + sway, -r * 0.95f), new Vector2(r * 0.98f + sway, -r * 0.95f), Darken(cloth, 0.28f));
                    Raylib.DrawTriangle(top, new Vector2(-r * 0.66f + sway, -r * 0.95f), new Vector2(r * 0.66f + sway, -r * 0.95f), cloth);
                    Raylib.DrawTriangle(top, new Vector2(-r * 0.28f + sway, -r * 0.95f), new Vector2(r * 0.16f + sway, -r * 0.95f), Lighten(cloth, 0.14f));
                    Raylib.DrawLineEx(new Vector2(-r * 0.9f + sway, -r * 0.88f), new Vector2(r * 0.9f + sway, -r * 0.88f), 2.5f, WithAlpha(plumHi, 0.55f));
                });
                break;
            case 10: // Skull Brooch - bone skull on gold mount
            {
                Vector2 brooch = rig.Screen(0.62f, -0.05f);
                DrawGlow(brooch, 14f, gold, 0.05f);
                Raylib.DrawCircleV(brooch, 7.5f, goldDark);
                Raylib.DrawCircleV(brooch, 6.5f, bone);
                Raylib.DrawCircleV(brooch - new Vector2(2f, 2f), 2.5f, boneHi);
                Raylib.DrawCircleV(brooch + rig.Left * 2.6f - rig.Forward * 0.5f, 1.7f, new Color(20, 16, 18, 255));
                Raylib.DrawCircleV(brooch - rig.Left * 2.6f - rig.Forward * 0.5f, 1.7f, new Color(20, 16, 18, 255));
                Raylib.DrawLineEx(brooch + rig.Forward * 5f - rig.Left * 3f, brooch + rig.Forward * 5f + rig.Left * 3f, 1f, ironDark);
                break;
            }
            case 11: // Battle Scarf - windblown scarf
                WithAccessoryRigRotation(rig, () =>
                {
                    float w = MathF.Sin(time * 2.4f) * r * 0.12f;
                    Raylib.DrawLineEx(new Vector2(-r * 0.85f, -r * 0.05f), new Vector2(r * 0.85f, r * 0.35f), 6f, Darken(crimson, 0.25f));
                    Raylib.DrawLineEx(new Vector2(-r * 0.8f, -r * 0.02f), new Vector2(r * 0.8f, r * 0.32f), 3.5f, crimson);
                    Raylib.DrawLineEx(new Vector2(-r * 0.78f, 0f), new Vector2(r * 0.7f, r * 0.28f), 1.2f, WithAlpha(crimsonHi, 0.7f));
                    Raylib.DrawLineEx(new Vector2(r * 0.6f, r * 0.28f), new Vector2(r * 0.9f + w, r * 0.6f), 4f, crimson);
                    Raylib.DrawLineEx(new Vector2(r * 0.9f + w, r * 0.6f), new Vector2(r * 0.7f + w, r * 0.85f), 3f, Darken(crimson, 0.2f));
                });
                break;
            case 12: // Spiked Collar - studded steel collar
            {
                Vector2 cc = rig.At(0.05f, 0f);
                Raylib.DrawRing(cc, r * 0.72f, r * 0.88f, 0f, 360f, 28, Darken(steel, 0.3f));
                Raylib.DrawRing(cc, r * 0.72f, r * 0.8f, 0f, 360f, 28, steel);
                for (int spike = 0; spike < 10; spike++)
                {
                    float ang = spike * MathF.PI * 2f / 10f;
                    Vector2 dir = new Vector2(MathF.Cos(ang), MathF.Sin(ang));
                    Vector2 basePt = cc + dir * r * 0.8f;
                    Vector2 tip = cc + dir * r * 1.04f;
                    Raylib.DrawLineEx(basePt, tip, 2.4f, steel);
                    Raylib.DrawLineEx(basePt, tip, 1f, steelHi);
                    Raylib.DrawCircleV(tip, 1.3f, steelHi);
                }
                break;
            }
            case 13: // Lantern Hook - hanging lantern
                WithAccessoryRigRotation(rig, () =>
                {
                    Vector2 hook = new Vector2(-r * 0.65f, -r * 0.2f);
                    Raylib.DrawLineEx(hook + new Vector2(0, -r * 0.4f), hook, 2f, iron);
                    var box = new Rectangle(hook.X - 5f, hook.Y, 10f, r * 0.3f);
                    Raylib.DrawRectangleRounded(box, 0.2f, 3, steelDark);
                    Raylib.DrawRectangleRoundedLines(box, 0.2f, 3, WithAlpha(steelHi, 0.6f));
                    Vector2 lflame = hook + new Vector2(0, r * 0.13f);
                    DrawGlow(lflame, r * 0.5f, amber, 0.1f + pulse * 0.06f);
                    Raylib.DrawCircleV(lflame, 2.6f + pulse, WithAlpha(amber, 0.85f));
                    Raylib.DrawCircleV(lflame, 1.2f, boneHi);
                });
                break;
            case 14: // Prayer Beads - polished rosary
                for (int bead = 0; bead < 9; bead++)
                {
                    float t = bead / 8f;
                    Vector2 bp = rig.Screen(-0.55f + t * 1.1f, 0.22f + MathF.Sin(t * MathF.PI) * 0.2f);
                    DrawMetalBead(bp, bead % 3 == 0 ? 2.8f : 2f, bead % 3 == 0 ? amethyst : bone);
                }
                break;
            case 15: // Royal Mantle - regal cape with ermine collar
                WithAccessoryRigRotation(rig, () =>
                {
                    float sway = MathF.Sin(time * 1.3f) * r * 0.05f;
                    Vector2 top = new Vector2(0f, -r * 0.35f);
                    Raylib.DrawTriangle(top, new Vector2(-r * 1.08f + sway, -r * 0.95f), new Vector2(r * 1.08f + sway, -r * 0.95f), Darken(plum, 0.3f));
                    Raylib.DrawTriangle(top, new Vector2(-r * 0.7f + sway, -r * 0.95f), new Vector2(r * 0.7f + sway, -r * 0.95f), plum);
                    Raylib.DrawTriangle(top, new Vector2(-r * 0.32f + sway, -r * 0.95f), new Vector2(r * 0.2f + sway, -r * 0.95f), Lighten(plum, 0.18f));
                    Raylib.DrawLineEx(new Vector2(-r * 0.6f, -r * 0.05f), new Vector2(r * 0.6f, -r * 0.05f), 4f, boneHi);
                    for (int sp = -2; sp <= 2; sp++)
                        Raylib.DrawCircleV(new Vector2(sp * r * 0.22f, -r * 0.05f), 1.3f, ironDark);
                    DrawTrinketGem(new Vector2(0f, -r * 0.18f), r * 0.1f, amethyst, time);
                });
                break;
            case 16: // Black Hood - deep shadowed cowl
                WithAccessoryRigRotation(rig, () =>
                {
                    Vector2 hc = new Vector2(0f, -r * 0.15f);
                    Raylib.DrawCircleSector(hc, r * 1.08f, 188f, 352f, 26, new Color(14, 13, 16, 255));
                    Raylib.DrawCircleSector(hc, r * 0.9f, 196f, 344f, 22, new Color(28, 26, 32, 255));
                    Raylib.DrawCircleSector(hc, r * 0.66f, 204f, 336f, 20, new Color(6, 6, 8, 255));
                    Raylib.DrawRing(hc, r * 1.0f, r * 1.08f, 188f, 352f, 26, WithAlpha(clothHi, 0.22f));
                });
                break;
            case 17: // Tower Crest - heraldic crest
                WithAccessoryRigRotation(rig, () =>
                {
                    var keep = new Rectangle(-r * 0.34f, -r * 0.96f, r * 0.68f, r * 0.5f);
                    Raylib.DrawRectangleRounded(keep, 0.12f, 3, Darken(steel, 0.25f));
                    Raylib.DrawRectangleRounded(new Rectangle(keep.X + 1.5f, keep.Y + 1.5f, keep.Width - 3f, keep.Height * 0.4f), 0.12f, 3, steel);
                    for (int b = 0; b < 3; b++)
                        Raylib.DrawRectangle((int)(keep.X + b * keep.Width / 2.6f), (int)(keep.Y - r * 0.08f), (int)(keep.Width / 5f), (int)(r * 0.1f), steel);
                    Raylib.DrawRectangle((int)(-r * 0.1f), (int)(-r * 0.6f), (int)(r * 0.2f), (int)(r * 0.16f), new Color(16, 14, 18, 255));
                    DrawTrinketGem(new Vector2(0f, -r * 0.52f), r * 0.07f, sapphire, time, 0.7f);
                });
                break;
            case 18: // Ash Veil - drifting translucent veil
                WithAccessoryRigRotation(rig, () =>
                {
                    Raylib.DrawCircleSector(new Vector2(0f, -r * 0.1f), r * 0.9f, 180f, 360f, 20, WithAlpha(new Color(120, 116, 128, 255), 0.5f));
                    Raylib.DrawCircleSector(new Vector2(0f, -r * 0.1f), r * 0.78f, 184f, 356f, 18, WithAlpha(new Color(150, 146, 158, 255), 0.3f));
                    for (int wisp = 0; wisp < 5; wisp++)
                    {
                        float wx = -r * 0.5f + wisp * r * 0.25f;
                        float wob = MathF.Sin(time * 1.5f + wisp) * r * 0.05f;
                        Raylib.DrawLineEx(new Vector2(wx, -r * 0.45f), new Vector2(wx + wob, r * 0.45f), 1.2f, WithAlpha(boneHi, 0.22f));
                    }
                });
                break;
            case 19: // Blood Sash - crimson sash with medallion
                WithAccessoryRigRotation(rig, () =>
                {
                    Raylib.DrawLineEx(new Vector2(-r * 0.78f, r * 0.0f), new Vector2(r * 0.78f, r * 0.4f), 7f, Darken(crimson, 0.25f));
                    Raylib.DrawLineEx(new Vector2(-r * 0.72f, r * 0.05f), new Vector2(r * 0.72f, r * 0.42f), 4f, crimson);
                    Raylib.DrawLineEx(new Vector2(-r * 0.7f, r * 0.07f), new Vector2(r * 0.6f, r * 0.36f), 1.2f, WithAlpha(crimsonHi, 0.6f));
                    Vector2 med = new Vector2(-r * 0.5f, r * 0.12f);
                    Raylib.DrawCircleV(med, 4.5f, gold);
                    Raylib.DrawCircleLinesV(med, 4.5f, goldHi);
                    DrawTrinketGem(med, 2.2f, ruby, time, 0.7f);
                });
                break;
            case 20: // Iron Halo - radiant ringed halo
            {
                Vector2 halo = rig.At(-0.75f, 0f);
                DrawGlow(halo, r * 1.1f, goldHi, 0.05f + glint * 0.03f);
                Raylib.DrawRing(halo, r * 0.52f, r * 0.64f, 0f, 360f, 32, goldDark);
                Raylib.DrawRing(halo, r * 0.54f, r * 0.6f, 0f, 360f, 32, gold);
                for (int spoke = 0; spoke < 8; spoke++)
                {
                    float ang = spoke * MathF.PI / 4f + time * 0.4f;
                    Vector2 off = new Vector2(MathF.Cos(ang) * r * 0.5f, MathF.Sin(ang) * r * 0.24f);
                    Raylib.DrawLineEx(halo, halo + off, 1.2f, WithAlpha(goldHi, 0.6f));
                }
                break;
            }
            case 21: // Keep Banner - waving heraldic banner
                WithAccessoryRigRotation(rig, () =>
                {
                    float wave = MathF.Sin(time * 2.5f) * r * 0.1f;
                    float wave2 = MathF.Sin(time * 2.5f + 1f) * r * 0.06f;
                    Raylib.DrawLineEx(new Vector2(-r * 0.6f, -r * 0.95f), new Vector2(-r * 0.6f, -r * 0.15f), 2.5f, new Color(70, 52, 36, 255));
                    Raylib.DrawCircleV(new Vector2(-r * 0.6f, -r * 0.95f), 3f, gold);
                    Raylib.DrawTriangle(new Vector2(-r * 0.6f, -r * 0.8f), new Vector2(-r * 0.6f, -r * 0.25f), new Vector2(-r * 0.02f + wave, -r * 0.55f), Darken(crimson, 0.2f));
                    Raylib.DrawTriangle(new Vector2(-r * 0.6f, -r * 0.8f), new Vector2(-r * 0.6f, -r * 0.55f), new Vector2(-r * 0.05f + wave2, -r * 0.72f), crimson);
                    Raylib.DrawCircleV(new Vector2(-r * 0.32f + wave * 0.5f, -r * 0.62f), r * 0.1f, gold);
                });
                break;
            case 22: // Dungeon Keys - ornate key ring
            {
                Vector2 keyRing = rig.Screen(-0.45f, 0.15f);
                Raylib.DrawCircleLinesV(keyRing, 6f, gold);
                Raylib.DrawCircleLinesV(keyRing, 6.8f, WithAlpha(goldHi, 0.6f));
                for (int k = 0; k < 2; k++)
                {
                    Vector2 tip = rig.Screen(-0.7f - k * 0.12f, 0.42f + k * 0.05f);
                    Raylib.DrawLineEx(keyRing, tip, 2.2f, k == 0 ? gold : copper);
                    Raylib.DrawLineEx(keyRing, tip, 1f, goldHi);
                    Raylib.DrawCircleV(tip, 2.4f, k == 0 ? gold : copper);
                    Raylib.DrawCircleV(tip, 1f, new Color(20, 18, 16, 255));
                }
                break;
            }
            case 23: // Oath Band - gilded brow band
            {
                Vector2 brow = rig.At(-0.45f, 0f);
                DrawJeweledBand(brow, r * 0.46f, r * 0.58f, rot + 208f, rot + 332f, 18, gold, goldHi);
                DrawTrinketGem(rig.At(-0.78f, 0f), r * 0.1f, emerald, time);
                break;
            }
            case 24: // Siege Gloves - plated gauntlets
                WithAccessoryRigRotation(rig, () =>
                {
                    for (int s = -1; s <= 1; s += 2)
                    {
                        float bx = s < 0 ? -r * 0.82f : r * 0.44f;
                        var gl = new Rectangle(bx, -r * 0.15f, r * 0.38f, r * 0.44f);
                        Raylib.DrawRectangleRounded(gl, 0.3f, 4, Darken(steel, 0.3f));
                        Raylib.DrawRectangleRounded(new Rectangle(gl.X + 1.5f, gl.Y + 1.5f, gl.Width - 3f, gl.Height * 0.4f), 0.3f, 4, steel);
                        for (int k = 0; k < 3; k++)
                            Raylib.DrawLineEx(new Vector2(gl.X + 2f, gl.Y + gl.Height * (0.45f + k * 0.18f)), new Vector2(gl.X + gl.Width - 2f, gl.Y + gl.Height * (0.45f + k * 0.18f)), 1f, WithAlpha(steelDark, 0.7f));
                        Raylib.DrawCircleV(new Vector2(gl.X + gl.Width * 0.5f, gl.Y + 2.5f), 1.4f, goldHi);
                    }
                });
                break;
            case 25: // Grave Shawl - tattered funeral shawl
                WithAccessoryRigRotation(rig, () =>
                {
                    Vector2 top = new Vector2(0f, -r * 0.25f);
                    Raylib.DrawTriangle(top, new Vector2(-r * 0.88f, -r * 0.85f), new Vector2(r * 0.88f, -r * 0.85f), new Color(38, 36, 44, 255));
                    Raylib.DrawTriangle(top, new Vector2(-r * 0.55f, -r * 0.85f), new Vector2(r * 0.55f, -r * 0.85f), new Color(54, 52, 62, 255));
                    for (int frill = 0; frill < 6; frill++)
                    {
                        float fx = -r * 0.7f + frill * r * 0.28f;
                        Raylib.DrawLineEx(new Vector2(fx, -r * 0.82f), new Vector2(fx + r * 0.04f, -r * 0.96f), 2f, new Color(38, 36, 44, 255));
                    }
                    for (int stitch = 0; stitch < 5; stitch++)
                        Raylib.DrawCircleV(new Vector2(-r * 0.35f + stitch * r * 0.18f, -r * 0.35f), 1.5f, WithAlpha(boneHi, 0.5f));
                });
                break;
            case 26: // Rampart Pin - shield brooch
            {
                Vector2 pin = rig.Screen(0.62f, 0.05f);
                Raylib.DrawLineEx(rig.Screen(0.2f, -0.25f), pin, 2f, goldDark);
                DrawGlow(pin, 12f, steel, 0.04f);
                Raylib.DrawCircleV(pin, 5f, Darken(steel, 0.3f));
                Raylib.DrawCircleV(pin, 4f, steel);
                Raylib.DrawCircleV(pin - new Vector2(1.4f, 1.4f), 1.6f, steelHi);
                Raylib.DrawLineEx(pin - rig.Forward * 3f, pin + rig.Forward * 3f, 1f, WithAlpha(gold, 0.7f));
                break;
            }
            case 27: // Lord's Mask - gilded visage
            {
                Vector2 face = rig.At(0.15f, 0f);
                Vector2 faceLeft = face - rig.Left * r * 0.18f;
                Vector2 faceRight = face + rig.Left * r * 0.18f;
                Vector2 sheen = face - rig.Forward * r * 0.12f;
                Raylib.DrawEllipse((int)face.X, (int)face.Y, (int)(r * 0.56f), (int)(r * 0.44f), Darken(gold, 0.35f));
                Raylib.DrawEllipse((int)sheen.X, (int)sheen.Y, (int)(r * 0.46f), (int)(r * 0.3f), gold);
                Raylib.DrawEllipse((int)faceLeft.X, (int)faceLeft.Y, 4, 6, new Color(10, 8, 12, 255));
                Raylib.DrawEllipse((int)faceRight.X, (int)faceRight.Y, 4, 6, new Color(10, 8, 12, 255));
                Raylib.DrawLineEx(faceLeft + rig.Forward * r * 0.18f, faceRight + rig.Forward * r * 0.18f, 1.5f, goldHi);
                Raylib.DrawCircleV(face + rig.Forward * r * 0.05f, 1.4f, goldHi);
                break;
            }
            case 28: // Bell Cord - golden bell on cord
                WithAccessoryRigRotation(rig, () =>
                {
                    Raylib.DrawLineEx(new Vector2(0f, -r * 0.85f), new Vector2(0f, r * 0.12f), 2f, new Color(72, 58, 46, 255));
                    Raylib.DrawCircleV(new Vector2(0f, -r * 0.88f), 3f, goldHi);
                    Vector2 bell = new Vector2(0f, r * 0.2f);
                    DrawGlow(bell, r * 0.4f, gold, 0.04f + glint * 0.03f);
                    Raylib.DrawCircleV(bell, 5f, goldDark);
                    Raylib.DrawCircleV(bell - new Vector2(0, 1f), 4f, gold);
                    Raylib.DrawCircleV(bell - new Vector2(1.4f, 2f), 1.5f, goldHi);
                    Raylib.DrawCircleV(bell + new Vector2(0, r * 0.07f), 1.6f, goldDark);
                });
                break;
            case 29: // Stone Rosary - carved bead loop
                for (int bead = 0; bead < 9; bead++)
                {
                    float ang = bead * 0.5f - 1.1f;
                    Vector2 bp = rig.At(0.15f + MathF.Sin(ang) * 0.35f, MathF.Cos(ang) * 0.55f);
                    if (bead == 4) DrawTrinketGem(bp, 3f, sapphire, time, 0.7f);
                    else DrawMetalBead(bp, bead % 2 == 0 ? 3f : 2.2f, bead % 2 == 0 ? slate : bone);
                }
                break;
            case 30: // War Paint - fierce face markings
            {
                Vector2 face = rig.At(0.12f, 0f);
                Vector2 fl = face - rig.Left * r * 0.35f;
                Vector2 fr = face + rig.Left * r * 0.35f;
                Color war = new Color(178, 40, 44, 255);
                Raylib.DrawLineEx(fl + rig.Forward * r * 0.25f, face + rig.Forward * r * 0.05f, 4f, Darken(war, 0.25f));
                Raylib.DrawLineEx(fl + rig.Forward * r * 0.25f, face + rig.Forward * r * 0.05f, 2f, crimsonHi);
                Raylib.DrawLineEx(fr + rig.Forward * r * 0.25f, face + rig.Forward * r * 0.05f, 4f, Darken(war, 0.25f));
                Raylib.DrawLineEx(fr + rig.Forward * r * 0.25f, face + rig.Forward * r * 0.05f, 2f, crimsonHi);
                Raylib.DrawLineEx(face - rig.Left * r * 0.22f + rig.Forward * r * 0.06f, face + rig.Left * r * 0.22f + rig.Forward * r * 0.06f, 2.5f, war);
                break;
            }
            case 31: // Crown Fragments - shattered gold crown
                WithAccessoryRigRotation(rig, () =>
                {
                    DrawGlow(new Vector2(0f, -r * 0.7f), r * 0.8f, gold, 0.04f);
                    for (int frag = 0; frag < 3; frag++)
                    {
                        float fx = -r * 0.35f + frag * r * 0.35f;
                        Raylib.DrawTriangle(new Vector2(fx, -r * 0.9f), new Vector2(fx - r * 0.12f, -r * 0.52f), new Vector2(fx + r * 0.12f, -r * 0.52f), gold);
                        Raylib.DrawTriangle(new Vector2(fx, -r * 0.82f), new Vector2(fx - r * 0.05f, -r * 0.56f), new Vector2(fx + r * 0.05f, -r * 0.56f), goldHi);
                        DrawTrinketGem(new Vector2(fx, -r * 0.6f), r * 0.05f, frag == 1 ? ruby : sapphire, time, 0.6f);
                    }
                });
                break;
            case 32: // Bastion Wings - steel angel wings
                WithAccessoryRigRotation(rig, () =>
                {
                    float flap = MathF.Sin(time * 2f) * r * 0.06f;
                    for (int s = -1; s <= 1; s += 2)
                    {
                        float dir = s;
                        Raylib.DrawTriangle(
                            new Vector2(dir * r * 0.2f, -r * 0.15f),
                            new Vector2(dir * r * 1.1f, -r * 0.1f - flap),
                            new Vector2(dir * r * 0.28f, -r * 0.55f),
                            Darken(steel, 0.25f));
                        Raylib.DrawTriangle(
                            new Vector2(dir * r * 0.22f, -r * 0.05f),
                            new Vector2(dir * r * 0.85f, -r * 0.12f - flap),
                            new Vector2(dir * r * 0.28f, -r * 0.48f),
                            steel);
                        for (int f = 0; f < 3; f++)
                            Raylib.DrawLineEx(new Vector2(dir * r * 0.3f, -r * (0.1f + f * 0.1f)), new Vector2(dir * r * (0.7f - f * 0.12f), -r * (0.1f + f * 0.12f) - flap), 1f, WithAlpha(steelHi, 0.5f));
                    }
                });
                break;
            case 33: // Crypt Lantern - spectral lantern
                WithAccessoryRigRotation(rig, () =>
                {
                    Vector2 lantern = new Vector2(-r * 0.55f, -r * 0.25f);
                    Raylib.DrawLineEx(lantern + new Vector2(0, -r * 0.2f), lantern + new Vector2(0, -r * 0.42f), 1.5f, iron);
                    var frame = new Rectangle(lantern.X - 6f, lantern.Y - r * 0.2f, 12f, r * 0.4f);
                    Raylib.DrawRectangleRounded(frame, 0.18f, 3, new Color(30, 36, 32, 255));
                    DrawGlow(lantern, r * 0.7f, new Color(90, 220, 150, 255), 0.1f + pulse * 0.07f);
                    Raylib.DrawCircleV(lantern, 4f, WithAlpha(new Color(120, 240, 170, 255), 0.7f + pulse * 0.3f));
                    Raylib.DrawCircleV(lantern, 1.6f, WithAlpha(boneHi, 0.85f));
                    Raylib.DrawRectangleRoundedLines(frame, 0.18f, 3, WithAlpha(new Color(140, 200, 160, 255), 0.5f));
                });
                break;
            case 34: // Throne Chain - gold chain with pendant
                WithAccessoryRigRotation(rig, () =>
                {
                    for (int link = 0; link < 6; link++)
                    {
                        float lx = -r * 0.5f + link * r * 0.2f;
                        float ly = -r * 0.5f + MathF.Abs(link - 2.5f) * r * 0.12f;
                        Raylib.DrawCircleV(new Vector2(lx, ly), 3f, goldDark);
                        Raylib.DrawCircleV(new Vector2(lx, ly), 2.2f, gold);
                        Raylib.DrawCircleV(new Vector2(lx - 0.7f, ly - 0.7f), 0.9f, goldHi);
                    }
                    Vector2 pend = new Vector2(0f, r * 0.05f);
                    Raylib.DrawLineEx(new Vector2(0f, -r * 0.2f), pend, 1.5f, gold);
                    Raylib.DrawCircleV(pend, 5f, gold);
                    DrawTrinketGem(pend, 3f, amethyst, time);
                });
                break;
            case 35: // Last Oath - sealed vow scroll
                WithAccessoryRigRotation(rig, () =>
                {
                    var scroll = new Rectangle(-r * 0.13f, -r * 0.75f, r * 0.26f, r * 0.95f);
                    Raylib.DrawRectangleRounded(scroll, 0.12f, 3, bone);
                    Raylib.DrawRectangleRounded(new Rectangle(scroll.X, scroll.Y, scroll.Width * 0.4f, scroll.Height), 0.12f, 3, boneHi);
                    Raylib.DrawCircleV(new Vector2(0f, -r * 0.78f), 3.2f, new Color(150, 140, 120, 255));
                    Raylib.DrawCircleV(new Vector2(0f, r * 0.2f), 3.2f, new Color(150, 140, 120, 255));
                    for (int ln = 0; ln < 4; ln++)
                        Raylib.DrawLineEx(new Vector2(-r * 0.07f, -r * 0.5f + ln * r * 0.16f), new Vector2(r * 0.07f, -r * 0.5f + ln * r * 0.16f), 1f, WithAlpha(ironDark, 0.6f));
                    Vector2 seal = new Vector2(0f, -r * 0.15f);
                    DrawGlow(seal, r * 0.3f, crimson, 0.05f);
                    Raylib.DrawCircleV(seal, 3.5f, crimson);
                    Raylib.DrawCircleV(seal, 1.6f, crimsonHi);
                });
                break;
            case 36: // Cursor Crown - secret regal diadem
                DrawCursorCrown(rig, time, r);
                break;
            case 37: // Storm Glass - crystalline foresight orb
                WithAccessoryRigRotation(rig, () =>
                {
                    Vector2 orb = new Vector2(r * 0.42f, -r * 0.08f);
                    Color stormCore = new Color(72, 148, 220, 255);
                    Color stormHi = new Color(168, 220, 255, 255);
                    Color stormDeep = new Color(28, 52, 96, 255);
                    DrawGlow(orb, r * 0.95f, stormCore, 0.08f + pulse * 0.06f);
                    Raylib.DrawCircleV(orb, r * 0.28f, WithAlpha(stormDeep, 0.92f));
                    Raylib.DrawCircleV(orb, r * 0.24f, WithAlpha(stormCore, 0.55f));
                    Raylib.DrawCircleV(orb, r * 0.2f, WithAlpha(stormHi, 0.35f + glint * 0.2f));
                    for (int arc = 0; arc < 3; arc++)
                    {
                        float a0 = time * 1.8f + arc * 2.1f;
                        Vector2 p0 = orb + new Vector2(MathF.Cos(a0), MathF.Sin(a0)) * r * 0.08f;
                        Vector2 p1 = orb + new Vector2(MathF.Cos(a0 + 1.4f), MathF.Sin(a0 + 1.4f)) * r * 0.16f;
                        Raylib.DrawLineEx(p0, p1, 1.2f, WithAlpha(stormHi, 0.45f + glint * 0.35f));
                    }
                    Raylib.DrawCircleV(orb + new Vector2(-r * 0.06f, -r * 0.07f), r * 0.05f, WithAlpha(Color.White, 0.55f + glint * 0.35f));
                    Raylib.DrawLineEx(orb + new Vector2(0f, r * 0.18f), orb + new Vector2(0f, r * 0.34f), 1.4f, WithAlpha(steel, 0.75f));
                    Raylib.DrawCircleV(orb + new Vector2(0f, r * 0.36f), 2.2f, steelHi);
                });
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(idx), idx, "Unexpected accessory index.");
        }
    }

}
