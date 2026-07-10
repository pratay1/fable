partial class Program
{
    // ---------------------------------------------------------------- Floor events (quicktime grove hazards)

    static readonly string[] FloorEventSubtitles =
    {
        "",
        "The stone bleeds - flee the marked earth!",
        "A sanctified band appears - reach the moss-lit edge.",
        "Black and white - the dark squares fall!",
        "The outer ring crumbles inward!",
        "Leap between the last stone islands!",
        "Pits open one by one - keep moving!",
        "Moss creeps across half the arena!",
        "Runes ignite - bolts fall from above!",
        "The heart collapses - hug the walls!",
        "Blight lightning - then total ruin!",
        "A tidal surge erupts from the depths - flee the flood!",
        "The floor splits along a rift - leap the drowning gap!",
        "The tide recedes - only the outer bastion holds!",
        "Columns of brine hammer down in relentless waves!",
        "Echoing ripples shatter stone in widening rings!",
        "An undertow drags the center into the abyss!",
        "The crest rises - higher ground or drowning!",
        "A wall of water advances - outrun the deluge!",
        "Drop anchor on the last dry stones or sink!",
        "Foaming chaos devours every unmarked tile!",
        "Burning columns sweep the arena - dodge the rain!",
        "A fiery corridor opens - sprint through the gate!",
        "Pulse waves radiate from a molten heart!",
        "A blazing cross sears the floor - flee the arms!",
        "Twin ember bridges - leap the chasm between!",
        "Stand still and the stone ignites beneath you!",
        "A serpent of flame coils across the tiles!",
        "Only the central hive remains untouched!",
        "A rising tide of embers devours the floor!",
        "The cage shrinks - cling to the dying center!",
        "The earth quakes - tiles fall before the storm!",
        "Four ember seeds bloom into infernal gardens!",
        "One altar tile survives - kneel or perish!",
        "Ancient wax seals the inner vault - only the outer ring endures!",
        "A banshee wail sweeps the catacombs - outrun the sound!",
        "Ghost-torches blaze along the cross - walk the lit path!",
        "Iron chains rattle column by column - the floor unbinds!",
        "Cold crypt-mist chokes the stones - then the grave claims all!",
        "Your tomb is already marked - flee before the lid falls!",
        "Funeral shrouds spread along the crossed diagonals!",
        "For one heartbeat the safe stones glow - then oblivion!",
        "Bone-rattle diagonals march across the burial hall!",
        "Echoes collapse the rings in alternating waves of ruin!",
        "Fresh graves open one by one - do not linger above the dead!",
        "A lantern spiral winds through darkness - follow the light!",
        "The veil rotates - stand in the lit quadrant or perish!",
        "A whirlpool gnaws the edges - cling to the dry heart!",
        "A lone beacon flickers - reach it before the flood!",
        "Tidal bolts strike in sequence - dodge the salvo!",
        "The regent's gauntlet awakens - prove your right to rule!",
        "Gilded squares of the old dynasty plummet into shadow!",
        "Jagged crown-shards erupt beneath every careless foot!",
        "The vacant throne beckons - seize it before the court crumbles!",
        "A royal edict sanctifies one border - obey or be cast out!",
        "Usurper's rot creeps across half the coronation hall!",
        "Judgment bolts hammer the condemned from the heights!",
        "The coronation ring collapses inward upon the unworthy!",
        "Leap between the last bastions of a shattered crown!",
        "Blight tempests ravage the gilded stones - then annihilation!",
        "Only the coronation dais survives the sacred ritual!",
        "Cross-shaped betrayal splits the hall asunder!",
        "Diagonal scars of reckoning tear the floor apart!",
        "The safe rim marches - keep pace with the rotating band!",
        "Iron portcullis columns slam down in ordered ruin!",
        "Your heraldry alone holds - the rest of the stone rots!",
    };

    static int TileEdgeDistance(int x, int y)
        => Math.Min(Math.Min(x, GridSize - 1 - x), Math.Min(y, GridSize - 1 - y));

    static float GridCenterDistanceSq(int x, int y)
    {
        float cx = GridSize / 2f - 0.5f;
        float cy = GridSize / 2f - 0.5f;
        float dx = x - cx;
        float dy = y - cy;
        return dx * dx + dy * dy;
    }

    static float CryptTorchCorridorDistance(int x, int y)
    {
        int mx = GridSize / 2;
        int my = GridSize / 2;
        return Math.Min(Math.Abs(x - mx), Math.Abs(y - my));
    }

    static float CryptShroudDiagonalDistance(int x, int y)
        => Math.Min(Math.Abs(x - y), Math.Abs(x + y - (GridSize - 1)));

    static bool IsCryptStaggeredCollapse(FloorEventType ev) => ev is
        FloorEventType.CryptSeal or FloorEventType.CryptWail or FloorEventType.CryptTorch
        or FloorEventType.CryptChains or FloorEventType.CryptGlimpse or FloorEventType.CryptRattle
        or FloorEventType.CryptEcho or FloorEventType.CryptShroud or FloorEventType.CryptLantern
        or FloorEventType.CryptTomb;

    static bool IsCrownStaggeredCollapse(FloorEventType ev) => ev is
        FloorEventType.CrownTrial or FloorEventType.CrownFall or FloorEventType.CrownShard
        or FloorEventType.CrownRing or FloorEventType.CrownIsles or FloorEventType.CrownCoronation
        or FloorEventType.CrownUsurpation or FloorEventType.CrownReckoning or FloorEventType.CrownThrone;

    static void MarkOutsideCenter(int radius)
    {
        int cx = GridSize / 2;
        int cy = GridSize / 2;
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if (tiles[x, y].State == 2) continue;
                bool inside = Math.Abs(x - cx) <= radius && Math.Abs(y - cy) <= radius;
                tiles[x, y].EventMarked = !inside;
            }
        }
    }

    static void ConfigureCrownThrone()
    {
        int r = 2;
        int side = (r * 2 + 1) * TileSize;
        int ox = (GridSize / 2 - r) * TileSize;
        int oy = (GridSize / 2 - r) * TileSize;
        eventSafeRect = new Rectangle(ox, oy, side, side);
        MarkOutsideCenter(r);
    }

    static void MarkCrownGildedCheckerboard()
    {
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if (tiles[x, y].State != 2 && ((x + y) / 2) % 2 != 0)
                {
                    tiles[x, y].EventMarked = true;
                }
            }
        }
    }

    static void MarkCrownCross()
    {
        int cx = GridSize / 2;
        int cy = GridSize / 2;
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if (tiles[x, y].State != 2 && (x == cx || y == cy))
                {
                    tiles[x, y].EventMarked = true;
                }
            }
        }
    }

    static void MarkCrownDiagonalBands()
    {
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if (tiles[x, y].State != 2 && (x + y) % 3 == 0)
                {
                    tiles[x, y].EventMarked = true;
                }
            }
        }
    }

    static bool PlayerInCrownCenter(int radiusTiles)
    {
        if (!TryGetTileUnder(playerPos, out int tx, out int ty)) return false;
        int cx = GridSize / 2;
        int cy = GridSize / 2;
        return Math.Abs(tx - cx) <= radiusTiles && Math.Abs(ty - cy) <= radiusTiles;
    }

    static Vector2 DirectionToArenaCenter()
    {
        Vector2 center = new Vector2(WindowWidth / 2f, WindowHeight / 2f);
        if (Vector2.DistanceSquared(playerPos, center) < 36f) return Vector2.Zero;
        return SafeNormalize(center - playerPos);
    }

    static void MarkAllActiveTiles()
    {
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if (tiles[x, y].State != 2) tiles[x, y].EventMarked = true;
            }
        }
    }

    static void MarkInverseRing(int safeThickness)
    {
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if (tiles[x, y].State == 2) continue;
                bool edge = x < safeThickness || y < safeThickness
                    || x >= GridSize - safeThickness || y >= GridSize - safeThickness;
                tiles[x, y].EventMarked = !edge;
            }
        }
    }

    static void MarkCryptCrossCorridors()
    {
        int mx = GridSize / 2;
        int my = GridSize / 2;
        MarkAllThenUnmark((x, y) => Math.Abs(x - mx) <= 1 || Math.Abs(y - my) <= 1);
    }

    static void MarkCryptChains()
    {
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if (tiles[x, y].State == 2) continue;
                tiles[x, y].EventMarked = x % 3 != 0;
            }
        }
    }

    static void MarkCryptShroud()
    {
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if (tiles[x, y].State == 2) continue;
                tiles[x, y].EventMarked = x == y || x + y == GridSize - 1;
            }
        }
    }

    static void MarkCryptTomb()
    {
        if (!TryGetTileUnder(playerPos, out int tx, out int ty))
        {
            tx = GridSize / 2;
            ty = GridSize / 2;
        }

        eventEpicenter = TileCenter(tx, ty);
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                int x = tx + dx;
                int y = ty + dy;
                if (x < 0 || x >= GridSize || y < 0 || y >= GridSize) continue;
                if (tiles[x, y].State != 2) tiles[x, y].EventMarked = true;
            }
        }
    }

    static void MarkCryptLanternSpiral()
    {
        MarkAllActiveTiles();
        int x = GridSize / 2;
        int y = GridSize / 2;
        int dir = 0;
        int leg = 1;
        int legCount = 0;
        int unmarked = 0;
        int target = GridSize * 2 + 4;
        ReadOnlySpan<int> dx = stackalloc int[] { 1, 0, -1, 0 };
        ReadOnlySpan<int> dy = stackalloc int[] { 0, 1, 0, -1 };

        while (unmarked < target)
        {
            if (x >= 0 && x < GridSize && y >= 0 && y < GridSize && tiles[x, y].State != 2)
            {
                tiles[x, y].EventMarked = false;
                unmarked++;
            }

            x += dx[dir];
            y += dy[dir];
            legCount++;
            if (legCount >= leg)
            {
                legCount = 0;
                dir = (dir + 1) % 4;
                if (dir % 2 == 0) leg++;
            }
        }
    }

    static void ConfigureCryptVeil()
    {
        float qw = WindowWidth / 2f;
        float qh = WindowHeight / 2f;
        eventSafeRect = eventSide switch
        {
            0 => new Rectangle(0f, 0f, qw, qh),
            1 => new Rectangle(qw, 0f, qw, qh),
            2 => new Rectangle(qw, qh, qw, qh),
            3 => new Rectangle(0f, qh, qw, qh),
            _ => throw new UnreachableException(),
        };
    }

    static Color EventAccentColor(FloorEventType ev) => ev switch
    {
        FloorEventType.CrimsonCrumble => new Color(196, 52, 44, 255),
        FloorEventType.SafeZoneRush => new Color(108, 132, 102, 255),
        FloorEventType.Checkerfall => new Color(88, 82, 96, 255),
        FloorEventType.RingCollapse => new Color(168, 92, 48, 255),
        FloorEventType.StoneIslands => new Color(148, 138, 118, 255),
        FloorEventType.ScatterPits => new Color(108, 72, 52, 255),
        FloorEventType.MossRot => new Color(88, 108, 62, 255),
        FloorEventType.MarkedStrike => new Color(220, 88, 48, 255),
        FloorEventType.CenterSnare => Danger,
        FloorEventType.BlightStorm => new Color(72, 148, 88, 255),
        FloorEventType.EmberRain => new Color(255, 118, 42, 255),
        FloorEventType.EmberGate => new Color(228, 78, 28, 255),
        FloorEventType.EmberPulse => new Color(255, 148, 52, 255),
        FloorEventType.EmberCross => new Color(212, 62, 38, 255),
        FloorEventType.EmberBridge => new Color(198, 108, 48, 255),
        FloorEventType.EmberFury => new Color(255, 88, 32, 255),
        FloorEventType.EmberSnake => new Color(240, 92, 38, 255),
        FloorEventType.EmberHive => new Color(255, 168, 58, 255),
        FloorEventType.EmberTide => new Color(220, 72, 22, 255),
        FloorEventType.EmberCage => new Color(188, 48, 28, 255),
        FloorEventType.EmberQuake => new Color(255, 132, 48, 255),
        FloorEventType.EmberBloom => new Color(248, 108, 42, 255),
        FloorEventType.EmberAltar => new Color(255, 198, 72, 255),
        FloorEventType.TideSurge => new Color(48, 108, 168, 255),
        FloorEventType.TideRift => new Color(62, 92, 148, 255),
        FloorEventType.TideRecede => new Color(72, 128, 182, 255),
        FloorEventType.TideColumn => new Color(88, 148, 198, 255),
        FloorEventType.TideEcho => new Color(56, 118, 162, 255),
        FloorEventType.TideUndertow => new Color(38, 82, 138, 255),
        FloorEventType.TideCrest => new Color(96, 162, 208, 255),
        FloorEventType.TideWall => new Color(52, 102, 158, 255),
        FloorEventType.TideAnchor => new Color(108, 148, 188, 255),
        FloorEventType.TideFoam => new Color(168, 198, 228, 255),
        FloorEventType.CryptSeal => new Color(128, 112, 148, 255),
        FloorEventType.CryptWail => new Color(156, 132, 172, 255),
        FloorEventType.CryptTorch => new Color(212, 178, 88, 255),
        FloorEventType.CryptChains => new Color(92, 92, 112, 255),
        FloorEventType.CryptMist => new Color(108, 116, 148, 255),
        FloorEventType.CryptTomb => new Color(72, 68, 92, 255),
        FloorEventType.CryptShroud => new Color(136, 96, 152, 255),
        FloorEventType.CryptGlimpse => new Color(176, 168, 196, 255),
        FloorEventType.CryptRattle => new Color(108, 92, 72, 255),
        FloorEventType.CryptEcho => new Color(140, 120, 176, 255),
        FloorEventType.CryptGrave => new Color(88, 80, 100, 255),
        FloorEventType.CryptLantern => new Color(220, 192, 96, 255),
        FloorEventType.CryptVeil => new Color(112, 100, 132, 255),
        FloorEventType.TideWhirlpool => new Color(28, 78, 128, 255),
        FloorEventType.TideBeacon => new Color(98, 178, 212, 255),
        FloorEventType.TideStrike => new Color(168, 208, 232, 255),
        FloorEventType.CrownTrial => new Color(212, 175, 55, 255),
        FloorEventType.CrownFall => new Color(148, 42, 88, 255),
        FloorEventType.CrownShard => new Color(188, 162, 98, 255),
        FloorEventType.CrownThrone => new Color(220, 198, 72, 255),
        FloorEventType.CrownEdict => new Color(108, 88, 148, 255),
        FloorEventType.CrownRot => new Color(98, 118, 62, 255),
        FloorEventType.CrownBolt => new Color(255, 215, 80, 255),
        FloorEventType.CrownRing => new Color(168, 128, 48, 255),
        FloorEventType.CrownIsles => new Color(138, 128, 108, 255),
        FloorEventType.CrownStorm => new Color(92, 138, 168, 255),
        FloorEventType.CrownCoronation => new Color(240, 210, 90, 255),
        FloorEventType.CrownUsurpation => new Color(180, 48, 72, 255),
        FloorEventType.CrownReckoning => new Color(128, 88, 168, 255),
        FloorEventType.None => Danger,
        _ => Danger,
    };

    static void SpawnEventShockwave(Vector2 center, Color color, float maxRadius, float life = 0.55f)
    {
        while (eventShockwaves.Count >= MaxEventShockwaves)
        {
            eventShockwaves.RemoveAt(0);
        }

        eventShockwaves.Add(new EventShockwave
        {
            Center = center,
            Radius = 8f,
            MaxRadius = maxRadius,
            Color = color,
            Life = life,
            MaxLife = life,
        });
    }

    static void SpawnEventSkyBeam(Vector2 ground, Color color, float width, float life, bool charging = false)
    {
        while (eventSkyBeams.Count >= MaxEventSkyBeams)
        {
            eventSkyBeams.RemoveAt(0);
        }

        eventSkyBeams.Add(new EventSkyBeam
        {
            Ground = ground,
            Color = color,
            Life = life,
            MaxLife = life,
            Width = width,
            Charging = charging,
        });
    }

    static void CollapseTileWithEventFx(int x, int y, Color _)
    {
        ref Tile tile = ref tiles[x, y];
        if (tile.State == 2) return;
        CollapseTile(ref tile, true, x, y);
    }

    static void BuildEventCollapseQueue(FloorEventType ev)
    {
        eventTileQueue.Clear();
        var list = new List<(int x, int y, float sort)>();

        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if (!tiles[x, y].EventMarked || tiles[x, y].State == 2) continue;
                float sort = ev switch
                {
                    FloorEventType.CrimsonCrumble => Vector2.DistanceSquared(eventEpicenter, TileCenter(x, y)),
                    FloorEventType.Checkerfall => x + y + (((x + y) / 2) % 2) * 0.01f,
                    FloorEventType.RingCollapse => TileEdgeDistance(x, y),
                    FloorEventType.ScatterPits => Rng.NextSingle(),
                    FloorEventType.StoneIslands => Vector2.DistanceSquared(TileCenter(x, y), eventEpicenter),
                    FloorEventType.BlightStorm => MathF.Atan2(y - GridSize / 2f, x - GridSize / 2f),
                    FloorEventType.TideSurge => Vector2.DistanceSquared(eventEpicenter, TileCenter(x, y)),
                    FloorEventType.TideRift => MathF.Abs(x - GridSize / 2f) + MathF.Abs(y - GridSize / 2f),
                    FloorEventType.TideRecede => -TileEdgeDistance(x, y),
                    FloorEventType.TideColumn => x + Rng.NextSingle() * 0.01f,
                    FloorEventType.TideEcho => x - y,
                    FloorEventType.TideUndertow => MathF.Atan2(y - GridSize / 2f, x - GridSize / 2f) + Vector2.DistanceSquared(TileCenter(x, y), eventEpicenter) * 0.001f,
                    FloorEventType.TideCrest => y + MathF.Sin(x * 0.4f) * 0.01f,
                    FloorEventType.TideWall => eventSide switch
                    {
                        0 => x,
                        1 => GridSize - x,
                        2 => y,
                        _ => GridSize - y,
                    },
                    FloorEventType.TideAnchor => Vector2.DistanceSquared(TileCenter(x, y), eventEpicenter),
                    FloorEventType.TideFoam => MathF.Atan2(y - GridSize / 2f, x - GridSize / 2f),
                    FloorEventType.EmberRain => eventSide == 0 ? x : GridSize - 1 - x,
                    FloorEventType.EmberPulse => Vector2.DistanceSquared(eventEpicenter, TileCenter(x, y)),
                    FloorEventType.EmberCross => Math.Abs(x - GridSize / 2f) + Math.Abs(y - GridSize / 2f),
                    FloorEventType.EmberBridge => Math.Min(Math.Abs(y - 5f), Math.Abs(y - (GridSize - 7f))),
                    FloorEventType.EmberHive => Vector2.DistanceSquared(eventEpicenter, TileCenter(x, y)),
                    FloorEventType.EmberFury => Rng.NextSingle(),
                    FloorEventType.EmberSnake => EmberSnakeSortKey(x, y),
                    FloorEventType.EmberQuake => Rng.NextSingle(),
                    FloorEventType.EmberBloom => EmberBloomSortKey(x, y),
                    FloorEventType.CryptSeal => GridCenterDistanceSq(x, y),
                    FloorEventType.CryptWail => eventSide switch
                    {
                        0 => x + y * 0.001f,
                        1 => GridSize - x + y * 0.001f,
                        2 => y + x * 0.001f,
                        _ => GridSize - y + x * 0.001f,
                    },
                    FloorEventType.CryptTorch => CryptTorchCorridorDistance(x, y),
                    FloorEventType.CryptChains => x * 1000f + y,
                    FloorEventType.CryptMist => MathF.Atan2(y - GridSize / 2f, x - GridSize / 2f),
                    FloorEventType.CryptTomb => Vector2.DistanceSquared(eventEpicenter, TileCenter(x, y)),
                    FloorEventType.CryptShroud => CryptShroudDiagonalDistance(x, y),
                    FloorEventType.CryptGlimpse => Rng.NextSingle(),
                    FloorEventType.CryptRattle => x + y,
                    FloorEventType.CryptEcho => TileEdgeDistance(x, y) * 2f + (TileEdgeDistance(x, y) % 2) * 0.001f,
                    FloorEventType.CryptLantern => -GridCenterDistanceSq(x, y),
                    FloorEventType.CrownTrial => Vector2.DistanceSquared(eventEpicenter, TileCenter(x, y)),
                    FloorEventType.CrownFall => x + y + (((x + y) / 2) % 2) * 0.01f,
                    FloorEventType.CrownShard => Rng.NextSingle(),
                    FloorEventType.CrownRing => TileEdgeDistance(x, y),
                    FloorEventType.CrownIsles => Vector2.DistanceSquared(TileCenter(x, y), eventEpicenter),
                    FloorEventType.CrownCoronation => TileEdgeDistance(x, y),
                    FloorEventType.CrownUsurpation => Math.Min(Math.Abs(x - GridSize / 2f), Math.Abs(y - GridSize / 2f)),
                    FloorEventType.CrownReckoning => x + y + (x + y) % 3 * 0.001f,
                    FloorEventType.CrownThrone => TileEdgeDistance(x, y),
                    _ => Rng.NextSingle(),
                };
                list.Add((x, y, sort));
            }
        }

        list.Sort((a, b) => a.sort.CompareTo(b.sort));
        foreach ((int x, int y, _) in list)
        {
            eventTileQueue.Enqueue((x, y));
        }
    }

    static float EmberSnakeSortKey(int x, int y)
    {
        int idx = x + y * GridSize;
        for (int i = 0; i < markedStrikeCount; i++)
        {
            if (markedStrikeOrder[i] == idx) return i;
        }
        return markedStrikeCount + Rng.NextSingle();
    }

    static float EmberBloomSortKey(int x, int y)
    {
        float best = float.MaxValue;
        for (int s = 0; s < markedStrikeCount; s++)
        {
            int idx = markedStrikeOrder[s];
            int sx = idx % GridSize;
            int sy = idx / GridSize;
            float d = Math.Max(Math.Abs(x - sx), Math.Abs(y - sy));
            if (d < best) best = d;
        }
        return best;
    }

    static void BeginEventCollapsePhase(FloorEventType ev)
    {
        eventPhase = 1;
        eventActionTimer = 0f;
        BuildEventCollapseQueue(ev);
        Color accent = EventAccentColor(ev);
        AddFlash(accent, 0.12f);
        AddTrauma(0.12f);
    }

    static float EventCollapseInterval(FloorEventType ev) => ev switch
    {
        FloorEventType.CrimsonCrumble => 0.028f,
        FloorEventType.Checkerfall => 0.022f,
        FloorEventType.RingCollapse => 0.026f,
        FloorEventType.StoneIslands => 0.032f,
        FloorEventType.ScatterPits => 0.05f,
        FloorEventType.BlightStorm => 0.02f,
        FloorEventType.TideSurge => 0.026f,
        FloorEventType.TideRift => 0.024f,
        FloorEventType.TideRecede => 0.028f,
        FloorEventType.TideColumn => 0.034f,
        FloorEventType.TideEcho => 0.023f,
        FloorEventType.TideUndertow => 0.021f,
        FloorEventType.TideCrest => 0.03f,
        FloorEventType.TideWall => 0.027f,
        FloorEventType.TideAnchor => 0.032f,
        FloorEventType.TideFoam => 0.02f,
        FloorEventType.EmberRain => 0.024f,
        FloorEventType.EmberPulse => 0.026f,
        FloorEventType.EmberCross => 0.02f,
        FloorEventType.EmberBridge => 0.028f,
        FloorEventType.EmberHive => 0.034f,
        FloorEventType.EmberFury => 0.03f,
        FloorEventType.EmberSnake => 0.022f,
        FloorEventType.EmberQuake => 0.032f,
        FloorEventType.EmberBloom => 0.025f,
        FloorEventType.CryptSeal => 0.026f,
        FloorEventType.CryptWail => 0.024f,
        FloorEventType.CryptTorch => 0.03f,
        FloorEventType.CryptChains => 0.034f,
        FloorEventType.CryptMist => 0.02f,
        FloorEventType.CryptTomb => 0.018f,
        FloorEventType.CryptShroud => 0.028f,
        FloorEventType.CryptGlimpse => 0.016f,
        FloorEventType.CryptRattle => 0.022f,
        FloorEventType.CryptEcho => 0.025f,
        FloorEventType.CryptLantern => 0.03f,
        FloorEventType.CrownTrial => 0.026f,
        FloorEventType.CrownFall => 0.021f,
        FloorEventType.CrownShard => 0.048f,
        FloorEventType.CrownRing => 0.025f,
        FloorEventType.CrownIsles => 0.031f,
        FloorEventType.CrownCoronation => 0.024f,
        FloorEventType.CrownUsurpation => 0.027f,
        FloorEventType.CrownReckoning => 0.023f,
        FloorEventType.CrownThrone => 0.026f,
        FloorEventType.None => 0.028f,
        _ => 0.028f,
    };

    static int EventCollapseBurst(FloorEventType ev) => ev switch
    {
        FloorEventType.CrimsonCrumble => 3,
        FloorEventType.Checkerfall => 3,
        FloorEventType.RingCollapse => 2,
        FloorEventType.StoneIslands => 2,
        FloorEventType.ScatterPits => 2,
        FloorEventType.BlightStorm => 4,
        FloorEventType.TideSurge => 3,
        FloorEventType.TideRift => 3,
        FloorEventType.TideRecede => 2,
        FloorEventType.TideColumn => 2,
        FloorEventType.TideEcho => 3,
        FloorEventType.TideUndertow => 4,
        FloorEventType.TideCrest => 2,
        FloorEventType.TideWall => 3,
        FloorEventType.TideAnchor => 2,
        FloorEventType.TideFoam => 4,
        FloorEventType.EmberRain => 3,
        FloorEventType.EmberPulse => 2,
        FloorEventType.EmberCross => 3,
        FloorEventType.EmberBridge => 2,
        FloorEventType.EmberHive => 2,
        FloorEventType.EmberFury => 2,
        FloorEventType.EmberSnake => 3,
        FloorEventType.EmberQuake => 1,
        FloorEventType.EmberBloom => 3,
        FloorEventType.CryptSeal => 3,
        FloorEventType.CryptWail => 4,
        FloorEventType.CryptTorch => 2,
        FloorEventType.CryptChains => 3,
        FloorEventType.CryptMist => 4,
        FloorEventType.CryptTomb => 5,
        FloorEventType.CryptShroud => 2,
        FloorEventType.CryptGlimpse => 5,
        FloorEventType.CryptRattle => 3,
        FloorEventType.CryptEcho => 2,
        FloorEventType.CryptLantern => 2,
        FloorEventType.CrownTrial => 3,
        FloorEventType.CrownFall => 3,
        FloorEventType.CrownShard => 2,
        FloorEventType.CrownRing => 2,
        FloorEventType.CrownIsles => 2,
        FloorEventType.CrownCoronation => 2,
        FloorEventType.CrownUsurpation => 3,
        FloorEventType.CrownReckoning => 3,
        FloorEventType.CrownThrone => 2,
        FloorEventType.None => 2,
        _ => 2,
    };

    static float ProcessStaggeredCollapse(float dt, FloorEventType ev, float interval)
    {
        eventActionTimer -= dt;
        Color accent = EventAccentColor(ev);
        int burst = EventCollapseBurst(ev);
        while (eventActionTimer <= 0f && eventTileQueue.Count > 0)
        {
            for (int b = 0; b < burst && eventTileQueue.Count > 0; b++)
            {
                (int x, int y) = eventTileQueue.Dequeue();
                CollapseTileWithEventFx(x, y, accent);
            }

            eventActionTimer += interval;
        }

        if (eventTileQueue.Count == 0)
        {
            EndFloorEvent();
            return 0f;
        }

        return interval;
    }

    static void UpdateEventVfx(float dt)
    {
        for (int i = eventShockwaves.Count - 1; i >= 0; i--)
        {
            EventShockwave sw = eventShockwaves[i];
            sw.Life -= dt;
            float t = 1f - sw.Life / sw.MaxLife;
            sw.Radius = 8f + t * sw.MaxRadius;
            if (sw.Life <= 0f) eventShockwaves.RemoveAt(i);
            else eventShockwaves[i] = sw;
        }

        for (int i = eventSkyBeams.Count - 1; i >= 0; i--)
        {
            EventSkyBeam beam = eventSkyBeams[i];
            beam.Life -= dt;
            if (beam.Life <= 0f) eventSkyBeams.RemoveAt(i);
            else eventSkyBeams[i] = beam;
        }
    }

    static float CurrentSafeRushBandPx() => SafeRushBandPx();

    static float CurrentCenterSnareMarginPx()
    {
        if (eventStartCountdown <= 0.01f) return CenterSnareMarginPx();
        float urgency = 1f - Math.Clamp(eventCountdown / eventStartCountdown, 0f, 1f);
        float tiles = CenterSnareMarginTiles + (1.75f - CenterSnareMarginTiles) * (urgency * 0.55f);
        return tiles * TileSize;
    }

    static void RefreshSafeZoneRushRect()
    {
        float band = CurrentSafeRushBandPx();
        eventSafeRect = eventSide switch
        {
            0 => new Rectangle(0f, 0f, band, WindowHeight),
            1 => new Rectangle(WindowWidth - band, 0f, band, WindowHeight),
            2 => new Rectangle(0f, 0f, WindowWidth, band),
            3 => new Rectangle(0f, WindowHeight - band, WindowWidth, band),
            _ => throw new UnreachableException(),
        };
    }

    static void RefreshCenterSnareRects()
    {
        float m = CurrentCenterSnareMarginPx();
        eventDangerRect = new Rectangle(m, m, WindowWidth - m * 2f, WindowHeight - m * 2f);
    }

    static void SpawnSafeZoneRushTrail()
    {
        if (playerInEventSafeZone) return;
        Vector2 target = new Vector2(
            eventSafeRect.X + eventSafeRect.Width * 0.5f,
            eventSafeRect.Y + eventSafeRect.Height * 0.5f);
        Vector2 dir = SafeNormalize(target - playerPos);
        for (int i = 0; i < 3; i++)
        {
            AddParticle(new Particle
            {
                Position = playerPos + dir * (PlayerRadius + i * 6f),
                Velocity = dir * Rng.Next(120, 220) + new Vector2(Rng.NextSingle() - 0.5f, Rng.NextSingle() - 0.5f) * 40f,
                Color = UiAccent,
                Alpha = 0.85f,
                Fade = 1f / 0.35f,
                Size = 3f + Rng.NextSingle() * 2f,
                Drag = 3f,
                Glow = true,
            });
        }
    }

    static readonly string[] FloorEventNames =
    {
        "", "CRIMSON CRUMBLE", "SAFE ZONE RUSH", "CHECKERFALL", "RING COLLAPSE",
        "STONE ISLANDS", "SCATTER PITS", "MOSS ROT", "MARKED STRIKE", "CENTER SNARE", "BLIGHT STORM",
        "TIDE SURGE", "TIDE RIFT", "TIDE RECEDE", "TIDE COLUMN", "TIDE ECHO",
        "TIDE UNDERTOW", "TIDE CREST", "TIDE WALL", "TIDE ANCHOR", "TIDE FOAM",
        "EMBER RAIN", "EMBER GATE", "EMBER PULSE", "EMBER CROSS", "EMBER BRIDGE", "EMBER FURY",
        "EMBER SNAKE", "EMBER HIVE", "EMBER TIDE", "EMBER CAGE", "EMBER QUAKE", "EMBER BLOOM", "EMBER ALTAR",
        "CRYPT SEAL", "CRYPT WAIL", "CRYPT TORCH", "CRYPT CHAINS", "CRYPT MIST",
        "CRYPT TOMB", "CRYPT SHROUD", "CRYPT GLIMPSE", "CRYPT RATTLE", "CRYPT ECHO",
        "CRYPT GRAVE", "CRYPT LANTERN", "CRYPT VEIL",
        "TIDE WHIRLPOOL", "TIDE BEACON", "TIDE STRIKE",
        "CROWN TRIAL", "CROWN FALL", "CROWN SHARD", "CROWN THRONE", "CROWN EDICT",
        "CROWN ROT", "CROWN BOLT", "CROWN RING", "CROWN ISLES", "CROWN STORM",
        "CROWN CORONATION", "CROWN USURPATION", "CROWN RECKONING",
        "SALLY FORTH", "PORTCULLIS", "HERALD'S CALL",
    };

    static readonly FloorEventType[] WiredFloorEventPool =
    {
        FloorEventType.CrimsonCrumble, FloorEventType.SafeZoneRush, FloorEventType.Checkerfall,
        FloorEventType.RingCollapse, FloorEventType.StoneIslands, FloorEventType.ScatterPits,
        FloorEventType.MossRot, FloorEventType.MarkedStrike, FloorEventType.CenterSnare,
        FloorEventType.BlightStorm,
        FloorEventType.TideSurge, FloorEventType.TideRift, FloorEventType.TideRecede,
        FloorEventType.TideColumn, FloorEventType.TideEcho, FloorEventType.TideUndertow,
        FloorEventType.TideCrest, FloorEventType.TideWall, FloorEventType.TideAnchor,
        FloorEventType.TideFoam, FloorEventType.TideWhirlpool, FloorEventType.TideBeacon,
        FloorEventType.TideStrike,
        FloorEventType.EmberRain, FloorEventType.EmberGate, FloorEventType.EmberPulse,
        FloorEventType.EmberCross, FloorEventType.EmberBridge, FloorEventType.EmberFury,
        FloorEventType.EmberSnake, FloorEventType.EmberHive, FloorEventType.EmberTide,
        FloorEventType.EmberCage, FloorEventType.EmberQuake, FloorEventType.EmberBloom,
        FloorEventType.EmberAltar,
        FloorEventType.CryptSeal, FloorEventType.CryptWail, FloorEventType.CryptTorch,
        FloorEventType.CryptChains, FloorEventType.CryptMist, FloorEventType.CryptTomb,
        FloorEventType.CryptShroud, FloorEventType.CryptGlimpse, FloorEventType.CryptRattle,
        FloorEventType.CryptEcho, FloorEventType.CryptGrave, FloorEventType.CryptLantern,
        FloorEventType.CryptVeil,
        FloorEventType.CrownTrial, FloorEventType.CrownFall, FloorEventType.CrownShard,
        FloorEventType.CrownThrone, FloorEventType.CrownEdict, FloorEventType.CrownRot,
        FloorEventType.CrownBolt, FloorEventType.CrownRing, FloorEventType.CrownIsles,
        FloorEventType.CrownStorm, FloorEventType.CrownCoronation, FloorEventType.CrownUsurpation,
        FloorEventType.CrownReckoning, FloorEventType.SallyForth, FloorEventType.Portcullis,
        FloorEventType.HeraldsCall,
    };

    static void UpdateFloorEvents(float dt)
    {
        if (verdictHaltTimer > 0f)
        {
            verdictHaltTimer = Math.Max(0f, verdictHaltTimer - dt);
            return;
        }

        if (floorRotTimer > 0f)
        {
            floorRotTimer -= dt;
            if (floorRotTimer <= 0f) floorRotSide = 0f;
        }

        if (activeEvent != FloorEventType.None)
        {
            eventTimer += dt;
            if (activeDifficulty.EventSurgeChance > 0.001f)
            {
                eventSurgeTimer += dt;
                if (eventSurgeTimer >= 7f)
                {
                    eventSurgeTimer = 0f;
                    if (Rng.NextSingle() < activeDifficulty.EventSurgeChance)
                    {
                        TriggerEventSurge();
                    }
                }
            }

            UpdateActiveFloorEvent(dt);
            return;
        }

        if (waveNumber < activeDifficulty.MinWaveForEvents) return;

        nextFloorEventTimer += dt;
        if (nextFloorEventTimer < nextFloorEventCooldown) return;

        // Don't open a deadly event while the wave-intro banner is still covering the
        // screen - give the player a fair moment to read the arena first.
        if (waveBannerTimer > 0f) return;

        nextFloorEventTimer = 0f;
        nextFloorEventCooldown = NextEventCooldownSpan();
        StartFloorEvent(PickNextFloorEvent());
    }

    static void TriggerEventSurge()
    {
        MarkRandomTiles(0.12f * activeDifficulty.EventIntensityMult);
        int extras = Rng.Next(2, 5);
        float speedBonus = WaveSpeedBonus(waveNumber) * activeDifficulty.EnemySpeedMult;
        for (int i = 0; i < extras; i++)
        {
            EnemyType type = PickGruntType();
            enemies.Add(CreateEnemy(type, RandomEdgePosition(GetEnemyRadius(type)), speedBonus));
        }

        eventCountdown = Math.Max(1.6f, eventCountdown * (1f - 0.12f * activeDifficulty.EventIntensityMult));
        Color surgeCol = activeDifficulty.Accent.A > 0 ? activeDifficulty.Accent : Danger;
        SpawnFloatingText(new Vector2(WindowWidth / 2f, 96f), "CATASTROPHE STACKS", surgeCol, 22);
        AddTrauma(0.18f * activeDifficulty.EventIntensityMult);
    }

    static void PushFloorEventHistory(FloorEventType ev)
    {
        for (int i = Math.Min(floorEventHistoryCount, FloorEventRepeatCooldown - 1); i > 0; i--)
        {
            floorEventHistory[i] = floorEventHistory[i - 1];
        }

        floorEventHistory[0] = ev;
        floorEventHistoryCount = Math.Min(floorEventHistoryCount + 1, FloorEventRepeatCooldown);
    }

    static bool WasFloorEventPlayedRecently(FloorEventType ev)
    {
        for (int i = 0; i < floorEventHistoryCount; i++)
        {
            if (floorEventHistory[i] == ev) return true;
        }

        return false;
    }

    static FloorEventType PickNextFloorEvent()
    {
        ReadOnlySpan<FloorEventType> pool = activeDifficulty.EasyEventsOnly
            ? EasyFloorEventPool
            : WiredFloorEventPool;

        if (floorEventHistoryCount > 0)
        {
            FloorEventType last = floorEventHistory[0];
            foreach ((FloorEventType precursor, FloorEventType followUp) in EventChains)
            {
                if (precursor != last) continue;
                foreach (FloorEventType ev in pool)
                {
                    if (ev == followUp && Rng.NextSingle() < 0.55f)
                    {
                        eventChainActive = true;
                        return followUp;
                    }
                }
            }
        }

        Span<FloorEventType> candidates = stackalloc FloorEventType[pool.Length];
        int count = 0;

        foreach (FloorEventType ev in pool)
        {
            if (!WasFloorEventPlayedRecently(ev))
            {
                candidates[count++] = ev;
            }
        }

        if (count == 0)
        {
            return pool[Rng.Next(pool.Length)];
        }

        return candidates[Rng.Next(count)];
    }

    static void StartFloorEvent(FloorEventType ev)
    {
        if (EventsHaltedByVerdict()) return;

        PushFloorEventHistory(ev);
        activeEvent = ev;
        eventTimer = 0f;
        eventStep = 0;
        eventPhase = 0;
        eventActionTimer = 0f;
        eventBlightBoltTimer = 0f;
        emberFuryTileIdx = -1;
        emberFuryStandTimer = 0f;
        eventTileQueue.Clear();
        eventShockwaves.Clear();
        eventSkyBeams.Clear();
        eventSurgeTimer = 0f;
        ClearEventMarks();
        Color accent = EventAccentColor(ev);
        AddTrauma(0.38f * activeDifficulty.EventIntensityMult);
        AddFlash(accent, 0.22f * Math.Min(1.2f, activeDifficulty.EventIntensityMult));
        zoomPunch = Math.Max(zoomPunch, 0.055f);
        TriggerImpact(ImpactTier.Medium, accent);
        SpawnEventShockwave(new Vector2(WindowWidth / 2f, WindowHeight / 2f), accent, WindowWidth * 0.45f, 0.85f);
        SpawnFloatingText(new Vector2(WindowWidth / 2f, 88f), FloorEventNames[(int)ev], accent, 26);
        int qx = Math.Clamp((int)(playerPos.X / TileSize), 0, GridSize - 1);
        int qy = Math.Clamp((int)(playerPos.Y / TileSize), 0, GridSize - 1);
        string subtitle = FloorEventSubtitles[(int)ev];
        if (eventChainActive) subtitle = "THE SIEGE DEEPENS - " + subtitle;
        subtitle += " - " + GetArenaQuadrantName(qx, qy);
        SpawnFloatingText(new Vector2(WindowWidth / 2f, 118f), subtitle, WithAlpha(Color.White, 0.85f), 15);
        eventChainActive = false;

        eventEpicenter = new Vector2(
            Rng.Next(2, GridSize - 2) * TileSize + TileSize / 2f,
            Rng.Next(2, GridSize - 2) * TileSize + TileSize / 2f);

        switch (ev)
        {
            case FloorEventType.CrimsonCrumble:
                MarkRandomTiles(0.58f);
                eventCountdown = 5.2f;
                break;
            case FloorEventType.SafeZoneRush:
                eventSide = Rng.Next(4);
                ConfigureSafeZoneRush();
                eventCountdown = 2.5f;
                break;
            case FloorEventType.Checkerfall:
                MarkCheckerboard();
                eventCountdown = 4.8f;
                break;
            case FloorEventType.RingCollapse:
                MarkRing(2);
                eventCountdown = 5.2f;
                break;
            case FloorEventType.StoneIslands:
                MarkSafeIslands(SafeIslandCount);
                eventEpicenter = playerPos;
                eventCountdown = 5.5f;
                break;
            case FloorEventType.ScatterPits:
                MarkRandomTiles(0.3f);
                eventCountdown = 4.6f;
                break;
            case FloorEventType.MossRot:
                floorRotSide = Rng.Next(2);
                floorRotTimer = 10f;
                eventCountdown = 10f;
                break;
            case FloorEventType.MarkedStrike:
                markedStrikeCount = MarkRandomTilesList(16);
                eventCountdown = MarkedStrikeTelegraphTime;
                eventStep = 0;
                break;
            case FloorEventType.CenterSnare:
                ConfigureCenterSnare();
                eventCountdown = 6.2f;
                break;
            case FloorEventType.BlightStorm:
                BlightRandomTiles(0.5f);
                eventCountdown = 5.2f;
                eventBlightBoltTimer = 0.35f;
                break;
            case FloorEventType.TideSurge:
                MarkRandomTiles(0.55f);
                eventCountdown = 5f;
                break;
            case FloorEventType.TideRift:
                MarkTideRift();
                eventCountdown = 4.8f;
                break;
            case FloorEventType.TideRecede:
                MarkInverseRing(3);
                eventCountdown = 5.2f;
                break;
            case FloorEventType.TideColumn:
                MarkTideColumns(5);
                eventCountdown = 4.6f;
                break;
            case FloorEventType.TideEcho:
                MarkTideEcho();
                eventCountdown = 4.8f;
                break;
            case FloorEventType.TideUndertow:
                MarkInverseRing(5);
                eventEpicenter = new Vector2(WindowWidth / 2f, WindowHeight / 2f);
                eventCountdown = 5f;
                break;
            case FloorEventType.TideCrest:
                MarkAllActiveTiles();
                eventCountdown = 5.4f;
                break;
            case FloorEventType.TideWall:
                eventSide = Rng.Next(4);
                MarkTideWallHalf(eventSide);
                eventCountdown = 4.4f;
                break;
            case FloorEventType.TideAnchor:
                MarkSafeIslands(SafeIslandCount);
                eventEpicenter = playerPos;
                eventCountdown = 5.5f;
                break;
            case FloorEventType.TideFoam:
                SoftenTideFoamTiles(0.48f);
                eventCountdown = 5f;
                eventBlightBoltTimer = 0.4f;
                break;
            case FloorEventType.TideWhirlpool:
                ConfigureTideWhirlpool();
                eventCountdown = 5.6f;
                break;
            case FloorEventType.TideBeacon:
                eventSide = Rng.Next(4);
                ConfigureTideBeacon();
                eventCountdown = 2.8f;
                break;
            case FloorEventType.TideStrike:
                markedStrikeCount = MarkTideStrikeChannel();
                eventCountdown = MarkedStrikeTelegraphTime;
                eventStep = 0;
                break;
            case FloorEventType.EmberRain:
                eventSide = Rng.Next(2);
                eventStep = 0;
                eventActionTimer = EmberRainColumnInterval;
                eventCountdown = 5.6f;
                break;
            case FloorEventType.EmberGate:
                ConfigureEmberGate();
                eventCountdown = 4.8f;
                break;
            case FloorEventType.EmberPulse:
                eventStep = 0;
                eventActionTimer = EmberPulseInterval;
                eventCountdown = 5.4f;
                break;
            case FloorEventType.EmberCross:
                MarkEmberCross();
                eventCountdown = 4.6f;
                break;
            case FloorEventType.EmberBridge:
                MarkEmberBridges();
                eventCountdown = 5f;
                break;
            case FloorEventType.EmberFury:
                emberFuryTileIdx = -1;
                emberFuryStandTimer = 0f;
                eventCountdown = 6f;
                break;
            case FloorEventType.EmberSnake:
                InitEmberSnakePath();
                eventActionTimer = EmberSnakeStepInterval;
                eventCountdown = 5.2f;
                break;
            case FloorEventType.EmberHive:
                MarkEmberHive();
                eventCountdown = 5f;
                break;
            case FloorEventType.EmberTide:
                eventSide = Rng.Next(2) + 2;
                RefreshEmberTideRects();
                eventCountdown = 5.5f;
                break;
            case FloorEventType.EmberCage:
                RefreshEmberCageRect();
                eventCountdown = 5.8f;
                break;
            case FloorEventType.EmberQuake:
                MarkRandomTiles(0.42f);
                eventActionTimer = EmberQuakeInterval;
                eventCountdown = 4.8f;
                break;
            case FloorEventType.EmberBloom:
                InitEmberBloomSeeds();
                eventActionTimer = EmberBloomInterval;
                eventCountdown = 5.4f;
                break;
            case FloorEventType.EmberAltar:
                MarkEmberAltar();
                eventCountdown = 6.2f;
                break;
            case FloorEventType.CryptSeal:
                MarkInverseRing(2);
                eventCountdown = 5f;
                break;
            case FloorEventType.CryptWail:
                MarkAllActiveTiles();
                eventSide = Rng.Next(4);
                eventCountdown = 4.8f;
                break;
            case FloorEventType.CryptTorch:
                MarkCryptCrossCorridors();
                eventCountdown = 5f;
                break;
            case FloorEventType.CryptChains:
                MarkCryptChains();
                eventCountdown = 4.6f;
                break;
            case FloorEventType.CryptMist:
                floorRotSide = Rng.Next(2);
                BlightRandomTiles(0.42f);
                eventCountdown = 5.5f;
                eventBlightBoltTimer = 0.38f;
                break;
            case FloorEventType.CryptTomb:
                MarkCryptTomb();
                eventCountdown = 3.2f;
                break;
            case FloorEventType.CryptShroud:
                MarkCryptShroud();
                eventCountdown = 4.6f;
                break;
            case FloorEventType.CryptGlimpse:
                MarkRandomTiles(0.52f);
                eventCountdown = 2.8f;
                break;
            case FloorEventType.CryptRattle:
                MarkAllActiveTiles();
                eventCountdown = 4.4f;
                break;
            case FloorEventType.CryptEcho:
                MarkAllActiveTiles();
                eventCountdown = 5f;
                break;
            case FloorEventType.CryptGrave:
                markedStrikeCount = MarkRandomTilesList(12);
                eventCountdown = CryptGraveTelegraphTime;
                eventStep = 0;
                break;
            case FloorEventType.CryptLantern:
                MarkCryptLanternSpiral();
                eventCountdown = 5.8f;
                break;
            case FloorEventType.CryptVeil:
                eventSide = 0;
                eventActionTimer = CryptVeilRotateInterval;
                ConfigureCryptVeil();
                eventCountdown = 5.5f;
                break;
            case FloorEventType.CrownTrial:
                MarkRandomTiles(0.56f);
                eventCountdown = 5.4f;
                break;
            case FloorEventType.CrownFall:
                MarkCrownGildedCheckerboard();
                eventCountdown = 4.9f;
                break;
            case FloorEventType.CrownShard:
                MarkRandomTiles(0.34f);
                eventCountdown = 4.7f;
                break;
            case FloorEventType.CrownThrone:
                ConfigureCrownThrone();
                eventCountdown = 5.8f;
                break;
            case FloorEventType.CrownEdict:
                eventSide = Rng.Next(4);
                ConfigureSafeZoneRush();
                eventCountdown = 2.7f;
                break;
            case FloorEventType.CrownRot:
                floorRotSide = Rng.Next(2);
                floorRotTimer = 9.2f;
                eventCountdown = 9.2f;
                break;
            case FloorEventType.CrownBolt:
                markedStrikeCount = MarkRandomTilesList(14);
                eventCountdown = MarkedStrikeTelegraphTime;
                eventStep = 0;
                break;
            case FloorEventType.CrownRing:
                MarkRing(2);
                eventCountdown = 5.3f;
                break;
            case FloorEventType.CrownIsles:
                MarkSafeIslands(SafeIslandCount);
                eventEpicenter = playerPos;
                eventCountdown = 5.7f;
                break;
            case FloorEventType.CrownStorm:
                BlightRandomTiles(0.47f);
                eventCountdown = 5.3f;
                eventBlightBoltTimer = 0.32f;
                break;
            case FloorEventType.CrownCoronation:
                MarkOutsideCenter(2);
                eventCountdown = 5.5f;
                break;
            case FloorEventType.CrownUsurpation:
                MarkCrownCross();
                eventCountdown = 4.8f;
                break;
            case FloorEventType.CrownReckoning:
                MarkCrownDiagonalBands();
                eventCountdown = 5f;
                break;
            case FloorEventType.SallyForth:
                eventSide = Rng.Next(4);
                ConfigureSafeZoneRush();
                eventActionTimer = 1.2f;
                eventCountdown = 4.8f;
                break;
            case FloorEventType.Portcullis:
                markedStrikeCount = MarkPortcullisColumns();
                eventCountdown = MarkedStrikeTelegraphTime + BlessingTelegraphBonus();
                eventStep = 0;
                break;
            case FloorEventType.HeraldsCall:
                MarkAllActiveTiles();
                eventEpicenter = playerPos;
                floorRotSide = Rng.Next(2);
                floorRotTimer = 8f;
                eventCountdown = 6f;
                break;
            case FloorEventType.None:
                break;
            default:
                throw new UnreachableException();
        }

        EnsureEventSafeFooting();
        ApplyDifficultyEventScaling();
        eventStartCountdown = eventCountdown;
    }

    static void ApplyDifficultyEventScaling()
    {
        float intensity = Math.Max(0.35f, activeDifficulty.EventIntensityMult);
        eventCountdown = Math.Max(2.2f, eventCountdown / intensity);
        if (eventBlightBoltTimer > 0.01f)
        {
            eventBlightBoltTimer = Math.Max(0.08f, eventBlightBoltTimer / intensity);
        }

        if (eventActionTimer > 0.01f)
        {
            eventActionTimer = Math.Max(0.08f, eventActionTimer / intensity);
        }
    }

    static void ClearEventMarks()
    {
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                tiles[x, y].EventMarked = false;
            }
        }
    }

    static int CountPlayableUnmarkedTiles()
    {
        int count = 0;
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if (tiles[x, y].State != 2 && !tiles[x, y].EventMarked) count++;
            }
        }
        return count;
    }

    static void CarveEventSafeFooting(int centerX, int centerY, int radiusTiles = 1)
    {
        for (int dy = -radiusTiles; dy <= radiusTiles; dy++)
        {
            for (int dx = -radiusTiles; dx <= radiusTiles; dx++)
            {
                int x = centerX + dx;
                int y = centerY + dy;
                if (x < 0 || x >= GridSize || y < 0 || y >= GridSize) continue;
                if (tiles[x, y].State == 2) continue;
                tiles[x, y].EventMarked = false;
            }
        }
    }

    static bool TryCarveSafeIslandBlock(int x, int y, int size)
    {
        for (int dy = 0; dy < size; dy++)
        {
            for (int dx = 0; dx < size; dx++)
            {
                if (tiles[x + dx, y + dy].State == 2) return false;
            }
        }

        for (int dy = 0; dy < size; dy++)
        {
            for (int dx = 0; dx < size; dx++)
            {
                tiles[x + dx, y + dy].EventMarked = false;
            }
        }

        return true;
    }

    static void ForceEventSafeFooting()
    {
        int playerX = GridSize / 2;
        int playerY = GridSize / 2;
        if (TryGetTileUnder(playerPos, out int px, out int py) && tiles[px, py].State != 2)
        {
            playerX = px;
            playerY = py;
            CarveEventSafeFooting(px, py, SafeIslandSize / 2);
            if (CountPlayableUnmarkedTiles() > 0) return;
        }

        int islandRadius = SafeIslandSize / 2;
        int originX = Math.Clamp(playerX - islandRadius, 0, GridSize - SafeIslandSize);
        int originY = Math.Clamp(playerY - islandRadius, 0, GridSize - SafeIslandSize);
        if (TryCarveSafeIslandBlock(originX, originY, SafeIslandSize)) return;

        for (int y = 0; y <= GridSize - SafeIslandSize; y++)
        {
            for (int x = 0; x <= GridSize - SafeIslandSize; x++)
            {
                if (TryCarveSafeIslandBlock(x, y, SafeIslandSize)) return;
            }
        }

        int bestX = -1;
        int bestY = -1;
        int bestScore = int.MinValue;
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if (tiles[x, y].State == 2) continue;
                int score = 0;
                if (x > 0 && tiles[x - 1, y].State != 2) score++;
                if (x < GridSize - 1 && tiles[x + 1, y].State != 2) score++;
                if (y > 0 && tiles[x, y - 1].State != 2) score++;
                if (y < GridSize - 1 && tiles[x, y + 1].State != 2) score++;
                score -= Math.Abs(x - playerX) + Math.Abs(y - playerY);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestX = x;
                    bestY = y;
                }
            }
        }

        if (bestX >= 0)
        {
            CarveEventSafeFooting(bestX, bestY, islandRadius);
            if (CountPlayableUnmarkedTiles() > 0) return;
            tiles[bestX, bestY].EventMarked = false;
        }
    }

    static void EnsureEventSafeFooting()
    {
        if (CountPlayableUnmarkedTiles() > 0) return;
        ForceEventSafeFooting();
    }

    static void MarkRandomTiles(float ratio)
    {
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                ref Tile t = ref tiles[x, y];
                if (t.State != 2 && Rng.NextSingle() < ratio)
                {
                    t.EventMarked = true;
                }
            }
        }
    }

    static int MarkRandomTilesList(int count)
    {
        var coords = new List<(int x, int y)>();
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if (tiles[x, y].State != 2) coords.Add((x, y));
            }
        }
        for (int i = coords.Count - 1; i > 0; i--)
        {
            int j = Rng.Next(i + 1);
            (coords[i], coords[j]) = (coords[j], coords[i]);
        }

        int n = Math.Min(count, coords.Count);
        if (coords.Count > 0 && n >= coords.Count) n = coords.Count - 1;
        for (int i = 0; i < n; i++)
        {
            tiles[coords[i].x, coords[i].y].EventMarked = true;
            markedStrikeOrder[i] = coords[i].x + coords[i].y * GridSize;
        }
        return n;
    }

    static void MarkCheckerboard()
    {
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if (tiles[x, y].State != 2 && ((x + y) / 2) % 2 == 0)
                {
                    tiles[x, y].EventMarked = true;
                }
            }
        }
    }

    static void MarkRing(int thickness)
    {
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                bool edge = x < thickness || y < thickness || x >= GridSize - thickness || y >= GridSize - thickness;
                if (edge && tiles[x, y].State != 2) tiles[x, y].EventMarked = true;
            }
        }
    }

    static void MarkSafeIslands(int islandCount)
    {
        int size = SafeIslandSize;
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if (tiles[x, y].State == 2) continue;
                tiles[x, y].EventMarked = true;
            }
        }

        int placed = 0;
        int attempts = 0;
        int maxAttempts = islandCount * 120;
        while (placed < islandCount && attempts < maxAttempts)
        {
            attempts++;
            int x = Rng.Next(0, GridSize - size + 1);
            int y = Rng.Next(0, GridSize - size + 1);
            if (!CanPlaceSafeIsland(x, y, size)) continue;

            for (int dy = 0; dy < size; dy++)
            {
                for (int dx = 0; dx < size; dx++)
                {
                    tiles[x + dx, y + dy].EventMarked = false;
                }
            }

            placed++;
        }

        if (placed == 0)
        {
            int anchorX = GridSize / 2;
            int anchorY = GridSize / 2;
            if (TryGetTileUnder(playerPos, out int px, out int py) && tiles[px, py].State != 2)
            {
                anchorX = px;
                anchorY = py;
            }

            int originX = Math.Clamp(anchorX - SafeIslandSize / 2, 0, GridSize - SafeIslandSize);
            int originY = Math.Clamp(anchorY - SafeIslandSize / 2, 0, GridSize - SafeIslandSize);
            if (!TryCarveSafeIslandBlock(originX, originY, SafeIslandSize))
            {
                for (int y = 0; y <= GridSize - SafeIslandSize; y++)
                {
                    for (int x = 0; x <= GridSize - SafeIslandSize; x++)
                    {
                        if (TryCarveSafeIslandBlock(x, y, SafeIslandSize))
                        {
                            placed = 1;
                            break;
                        }
                    }

                    if (placed > 0) break;
                }
            }
            else
            {
                placed = 1;
            }
        }

        EnsureEventSafeFooting();
    }

    static bool CanPlaceSafeIsland(int x, int y, int size)
    {
        for (int dy = 0; dy < size; dy++)
        {
            for (int dx = 0; dx < size; dx++)
            {
                ref Tile tile = ref tiles[x + dx, y + dy];
                if (tile.State == 2 || !tile.EventMarked) return false;
            }
        }

        return true;
    }

    static bool PlayerOnSafeIsland()
    {
        if (!TryGetTileUnder(playerPos, out int tx, out int ty)) return false;
        return !tiles[tx, ty].EventMarked && tiles[tx, ty].State != 2;
    }

    static void BlightRandomTiles(float ratio)
    {
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                ref Tile t = ref tiles[x, y];
                if (t.State == 2) continue;
                if (Rng.NextSingle() < ratio)
                {
                    t.State = 1;
                    t.Durability = Math.Min(t.Durability, MaxDurability * 0.38f);
                    t.EventMarked = true;
                }
            }
        }
    }

    static bool IsTideStaggeredCollapse(FloorEventType ev) => ev is
        FloorEventType.TideSurge or FloorEventType.TideRift or FloorEventType.TideRecede
        or FloorEventType.TideColumn or FloorEventType.TideEcho or FloorEventType.TideUndertow
        or FloorEventType.TideCrest or FloorEventType.TideWall or FloorEventType.TideAnchor;

    static void UpdateTideTelegraphCollapse(float dt, FloorEventType ev)
    {
        if (eventPhase == 0)
        {
            eventCountdown -= dt;
            if (eventCountdown <= 0f) BeginEventCollapsePhase(ev);
        }
        else
        {
            ProcessStaggeredCollapse(dt, ev, EventCollapseInterval(ev));
        }
    }

    static void MarkTideRift()
    {
        int cx = GridSize / 2;
        int cy = GridSize / 2;
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if (tiles[x, y].State == 2) continue;
                tiles[x, y].EventMarked = Math.Abs(x - cx) <= 1 || Math.Abs(y - cy) <= 1;
            }
        }
    }

    static void MarkTideColumns(int columnCount)
    {
        var cols = new HashSet<int>();
        while (cols.Count < columnCount)
        {
            cols.Add(Rng.Next(0, GridSize));
        }

        foreach (int col in cols)
        {
            for (int y = 0; y < GridSize; y++)
            {
                if (tiles[col, y].State != 2) tiles[col, y].EventMarked = true;
            }
        }
    }

    static int MarkPortcullisColumns()
    {
        int cols = 4 + Rng.Next(2);
        int order = 0;
        for (int c = 0; c < cols; c++)
        {
            int col = Rng.Next(1, GridSize - 1);
            for (int y = 0; y < GridSize; y++)
            {
                if (tiles[col, y].State == 2) continue;
                tiles[col, y].EventMarked = true;
                if (order < markedStrikeOrder.Length)
                {
                    markedStrikeOrder[order++] = col + y * GridSize;
                }
            }
        }
        return order;
    }

    static void MarkTideEcho()
    {
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if (tiles[x, y].State == 2) continue;
                if ((x - y + GridSize * 3) % 4 == 0) tiles[x, y].EventMarked = true;
            }
        }
    }

    static void MarkTideWallHalf(int side)
    {
        int half = GridSize / 2;
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if (tiles[x, y].State == 2) continue;
                bool marked = side switch
                {
                    0 => x < half,
                    1 => x >= half,
                    2 => y < half,
                    _ => y >= half,
                };
                tiles[x, y].EventMarked = marked;
            }
        }
    }

    static int MarkTideStrikeChannel()
    {
        eventTideRowStrike = Rng.Next(2) == 0;
        int line = Rng.Next(GridSize);
        eventSide = line;
        var coords = new List<(int x, int y, float sort)>();
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if (tiles[x, y].State == 2) continue;
                bool onLine = eventTideRowStrike ? y == line : x == line;
                if (!onLine) continue;
                tiles[x, y].EventMarked = true;
                coords.Add((x, y, eventTideRowStrike ? x : y));
            }
        }

        coords.Sort((a, b) => a.sort.CompareTo(b.sort));
        for (int i = 0; i < coords.Count; i++)
        {
            markedStrikeOrder[i] = coords[i].x + coords[i].y * GridSize;
        }

        return coords.Count;
    }

    static void SoftenTideFoamTiles(float ratio)
    {
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                ref Tile t = ref tiles[x, y];
                if (t.State == 2) continue;
                if (Rng.NextSingle() < ratio)
                {
                    t.State = 1;
                    t.Durability = Math.Min(t.Durability, MaxDurability * 0.35f);
                    t.EventMarked = true;
                }
            }
        }
    }

    static void ConfigureTideBeacon()
    {
        float band = SafeRushBandPx();
        eventSafeRect = eventSide switch
        {
            0 => new Rectangle(0f, 0f, band, band),
            1 => new Rectangle(WindowWidth - band, 0f, band, band),
            2 => new Rectangle(0f, WindowHeight - band, band, band),
            3 => new Rectangle(WindowWidth - band, WindowHeight - band, band, band),
            _ => throw new UnreachableException(),
        };
        eventDangerRect = default;
    }

    static void ConfigureTideWhirlpool()
    {
        float m = CenterSnareMarginPx();
        eventDangerRect = new Rectangle(m, m, WindowWidth - m * 2f, WindowHeight - m * 2f);
        eventSafeRect = default;
    }

    static float CurrentTideWhirlpoolMarginPx()
    {
        float urgency = eventStartCountdown > 0.01f
            ? 1f - Math.Clamp(eventCountdown / eventStartCountdown, 0f, 1f)
            : 1f;
        float tiles = CenterSnareMarginTiles + (GridSize / 2.2f - CenterSnareMarginTiles) * (1f - urgency * 0.6f);
        return tiles * TileSize;
    }

    static void RefreshTideWhirlpoolRect()
    {
        float m = CurrentTideWhirlpoolMarginPx();
        eventDangerRect = new Rectangle(m, m, WindowWidth - m * 2f, WindowHeight - m * 2f);
    }

    static bool PlayerInTideWhirlpoolDanger()
        => Raylib.CheckCollisionCircleRec(playerPos, PlayerRadius, eventDangerRect);

    static string TideBeaconHint() => eventSide switch
    {
        0 => "SAFE: NORTHWEST CORNER",
        1 => "SAFE: NORTHEAST CORNER",
        2 => "SAFE: SOUTHWEST CORNER",
        3 => "SAFE: SOUTHEAST CORNER",
        _ => "",
    };

    static string TideWallHint() => eventSide switch
    {
        0 => "FLEE EAST - WALL FROM WEST",
        1 => "FLEE WEST - WALL FROM EAST",
        2 => "FLEE SOUTH - WALL FROM NORTH",
        3 => "FLEE NORTH - WALL FROM SOUTH",
        _ => "",
    };

    static Vector2 DirectionAwayFromArenaCenter()
    {
        Vector2 center = new Vector2(WindowWidth / 2f, WindowHeight / 2f);
        return SafeNormalize(playerPos - center);
    }

    static void SpawnTideBeaconTrail()
    {
        if (playerInEventSafeZone) return;
        Vector2 target = new Vector2(
            eventSafeRect.X + eventSafeRect.Width * 0.5f,
            eventSafeRect.Y + eventSafeRect.Height * 0.5f);
        Vector2 dir = SafeNormalize(target - playerPos);
        AddParticle(new Particle
        {
            Position = playerPos + dir * (PlayerRadius + 4f),
            Velocity = dir * 150f,
            Color = EventAccentColor(FloorEventType.TideBeacon),
            Alpha = 0.65f,
            Fade = 1f / 0.3f,
            Size = 3f,
            Drag = 3f,
            Glow = true,
        });
    }

    static bool IsEmberStaggeredCollapse(FloorEventType ev) => ev is
        FloorEventType.EmberRain or FloorEventType.EmberPulse or FloorEventType.EmberCross
        or FloorEventType.EmberBridge or FloorEventType.EmberHive or FloorEventType.EmberFury
        or FloorEventType.EmberSnake or FloorEventType.EmberQuake or FloorEventType.EmberBloom;

    static int EmberEpicenterTx() => Math.Clamp((int)(eventEpicenter.X / TileSize), 0, GridSize - 1);
    static int EmberEpicenterTy() => Math.Clamp((int)(eventEpicenter.Y / TileSize), 0, GridSize - 1);

    static void MarkAllThenUnmark(Func<int, int, bool> keepSafe)
    {
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if (tiles[x, y].State == 2) continue;
                tiles[x, y].EventMarked = !keepSafe(x, y);
            }
        }
    }

    static void ConfigureEmberGate()
    {
        eventSide = Rng.Next(2);
        int lane = Rng.Next(3, GridSize - 3);
        if (eventSide == 0)
        {
            MarkAllThenUnmark((x, _) => x >= lane && x < lane + EmberGateCorridorTiles);
            eventSafeRect = new Rectangle(lane * TileSize, 0f, EmberGateCorridorTiles * TileSize, WindowHeight);
        }
        else
        {
            MarkAllThenUnmark((_, y) => y >= lane && y < lane + EmberGateCorridorTiles);
            eventSafeRect = new Rectangle(0f, lane * TileSize, WindowWidth, EmberGateCorridorTiles * TileSize);
        }
    }

    static void MarkEmberCross()
    {
        int mid = GridSize / 2;
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if (tiles[x, y].State == 2) continue;
                bool arm = Math.Abs(x - mid) <= 1 || Math.Abs(y - mid) <= 1;
                tiles[x, y].EventMarked = arm;
            }
        }
    }

    static void MarkEmberBridges()
    {
        int bridgeA = 5;
        int bridgeB = GridSize - 7;
        MarkAllThenUnmark((_, y) => (y >= bridgeA && y < bridgeA + 2) || (y >= bridgeB && y < bridgeB + 2));
    }

    static void MarkEmberHive()
    {
        int cx = GridSize / 2;
        int cy = GridSize / 2;
        eventEpicenter = TileCenter(cx, cy);
        MarkAllThenUnmark((x, y) => Math.Abs(x - cx) <= EmberHiveRadius && Math.Abs(y - cy) <= EmberHiveRadius);
    }

    static void MarkEmberAltar()
    {
        int ax = EmberEpicenterTx();
        int ay = EmberEpicenterTy();
        MarkAllThenUnmark((x, y) => Math.Abs(x - ax) <= 1 && Math.Abs(y - ay) <= 1);
        eventSafeRect = new Rectangle((ax - 1) * TileSize, (ay - 1) * TileSize, TileSize * 3f, TileSize * 3f);
    }

    static void InitEmberSnakePath()
    {
        int x = Rng.Next(0, GridSize);
        int y = Rng.Next(0, GridSize);
        markedStrikeCount = Math.Min(EmberSnakeLength, GridSize * GridSize);
        for (int i = 0; i < markedStrikeCount; i++)
        {
            markedStrikeOrder[i] = x + y * GridSize;
            int dir = Rng.Next(4);
            int nx = x + (dir == 0 ? -1 : dir == 1 ? 1 : 0);
            int ny = y + (dir == 2 ? -1 : dir == 3 ? 1 : 0);
            if (nx < 0 || nx >= GridSize || ny < 0 || ny >= GridSize)
            {
                dir = Rng.Next(4);
                nx = x + (dir == 0 ? -1 : dir == 1 ? 1 : 0);
                ny = y + (dir == 2 ? -1 : dir == 3 ? 1 : 0);
                nx = Math.Clamp(nx, 0, GridSize - 1);
                ny = Math.Clamp(ny, 0, GridSize - 1);
            }
            x = nx;
            y = ny;
        }
        eventStep = 0;
    }

    static void InitEmberBloomSeeds()
    {
        markedStrikeCount = EmberBloomSeedCount;
        var used = new HashSet<int>();
        for (int i = 0; i < markedStrikeCount; i++)
        {
            int idx;
            do idx = Rng.Next(0, GridSize * GridSize);
            while (!used.Add(idx));
            markedStrikeOrder[i] = idx;
        }
        eventStep = 0;
    }

    static void EmberRainAdvanceColumn(Color accent)
    {
        if (eventStep >= GridSize) return;
        int col = eventSide == 0 ? eventStep : GridSize - 1 - eventStep;
        for (int y = 0; y < GridSize; y++)
        {
            if (tiles[col, y].State != 2) tiles[col, y].EventMarked = true;
        }
        Vector2 beamPos = TileCenter(col, GridSize / 2);
        SpawnEventSkyBeam(beamPos, accent, 18f, 0.22f);
        SpawnEventShockwave(beamPos, accent, 48f, 0.35f);
        eventStep++;
    }

    static void EmberPulseExpandRing(Color accent)
    {
        int cx = EmberEpicenterTx();
        int cy = EmberEpicenterTy();
        int ring = eventStep;
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if (tiles[x, y].State == 2) continue;
                int dist = Math.Max(Math.Abs(x - cx), Math.Abs(y - cy));
                if (dist == ring) tiles[x, y].EventMarked = true;
            }
        }
        float ringPx = (ring + 0.5f) * TileSize;
        SpawnEventShockwave(eventEpicenter, accent, ringPx, 0.4f);
        eventStep++;
    }

    static void EmberSnakeAdvance(Color accent)
    {
        if (eventStep >= markedStrikeCount) return;
        int idx = markedStrikeOrder[eventStep];
        int sx = idx % GridSize;
        int sy = idx / GridSize;
        if (tiles[sx, sy].State != 2) tiles[sx, sy].EventMarked = true;
        Vector2 head = TileCenter(sx, sy);
        SpawnGfxLightPulse(head, accent, 36f, 0.8f, 0.14f);
        eventStep++;
    }

    static void EmberBloomExpand(Color accent)
    {
        int radius = eventStep;
        for (int s = 0; s < markedStrikeCount; s++)
        {
            int idx = markedStrikeOrder[s];
            int sx = idx % GridSize;
            int sy = idx / GridSize;
            for (int y = 0; y < GridSize; y++)
            {
                for (int x = 0; x < GridSize; x++)
                {
                    if (tiles[x, y].State == 2) continue;
                    int dist = Math.Max(Math.Abs(x - sx), Math.Abs(y - sy));
                    if (dist <= radius) tiles[x, y].EventMarked = true;
                }
            }
        }
        SpawnEventShockwave(eventEpicenter, accent, (radius + 1) * TileSize * 0.45f, 0.32f);
        eventStep++;
    }

    static void EmberQuakeStrike(Color accent)
    {
        var marked = new List<(int x, int y)>();
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if (tiles[x, y].EventMarked && tiles[x, y].State != 2) marked.Add((x, y));
            }
        }
        if (marked.Count == 0) return;
        (int qx, int qy) = marked[Rng.Next(marked.Count)];
        tiles[qx, qy].EventMarked = false;
        CollapseTileWithEventFx(qx, qy, accent);
        Vector2 pos = TileCenter(qx, qy);
        SpawnEventShockwave(pos, accent, 44f, 0.28f);
        AddTrauma(0.06f);
    }

    static void UpdateEmberFury(float dt, Color accent)
    {
        if (!TryGetTileUnder(playerPos, out int tx, out int ty)) return;
        int idx = tx + ty * GridSize;
        if (idx != emberFuryTileIdx)
        {
            emberFuryTileIdx = idx;
            emberFuryStandTimer = 0f;
            return;
        }

        emberFuryStandTimer += dt;
        if (emberFuryStandTimer < EmberFuryStandTime) return;
        if (tiles[tx, ty].State == 2 || tiles[tx, ty].EventMarked) return;

        tiles[tx, ty].EventMarked = true;
        emberFuryStandTimer = 0f;
        SpawnGfxLightPulse(TileCenter(tx, ty), accent, 52f, 1f, 0.16f);
        SpawnFloatingText(TileCenter(tx, ty), "IGNITE!", accent, 14);
        AddFlash(accent, 0.1f);
    }

    static void RefreshEmberTideRects()
    {
        float urgency = eventStartCountdown > 0.01f
            ? 1f - Math.Clamp(eventCountdown / eventStartCountdown, 0f, 1f)
            : 1f;
        float tidePx = urgency * (WindowHeight - 60f);
        if (eventSide == 2)
        {
            eventDangerRect = new Rectangle(0f, WindowHeight - tidePx, WindowWidth, tidePx);
            eventSafeRect = new Rectangle(0f, 0f, WindowWidth, Math.Max(40f, WindowHeight - tidePx));
        }
        else
        {
            eventDangerRect = new Rectangle(0f, 0f, WindowWidth, tidePx);
            eventSafeRect = new Rectangle(0f, tidePx, WindowWidth, Math.Max(40f, WindowHeight - tidePx));
        }
    }

    static void RefreshEmberCageRect()
    {
        float urgency = eventStartCountdown > 0.01f
            ? 1f - Math.Clamp(eventCountdown / eventStartCountdown, 0f, 1f)
            : 1f;
        float marginTiles = 2.5f + urgency * 5.5f;
        float m = marginTiles * TileSize;
        eventSafeRect = new Rectangle(m, m, WindowWidth - m * 2f, WindowHeight - m * 2f);
    }

    static bool PlayerInEmberAltar()
    {
        if (!TryGetTileUnder(playerPos, out int tx, out int ty)) return false;
        int ax = EmberEpicenterTx();
        int ay = EmberEpicenterTy();
        return Math.Abs(tx - ax) <= 1 && Math.Abs(ty - ay) <= 1 && !tiles[tx, ty].EventMarked;
    }

    static void UpdateEmberTelegraphCollapse(float dt, FloorEventType ev)
    {
        if (eventPhase == 0)
        {
            eventCountdown -= dt;
            if (eventCountdown <= 0f) BeginEventCollapsePhase(ev);
        }
        else
        {
            ProcessStaggeredCollapse(dt, ev, EventCollapseInterval(ev));
        }
    }

    static void CollapseMarkedTiles()
    {
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                ref Tile t = ref tiles[x, y];
                if (t.EventMarked && t.State != 2)
                {
                    CollapseTile(ref t);
                }
            }
        }
        AddTrauma(0.4f);
        AddFlash(Danger, 0.18f);
    }

    static void UpdateActiveFloorEvent(float dt)
    {
        if (BannerFreezesEvents() || EventsHaltedByVerdict()) dt = 0f;
        UpdateEventVfx(dt);
        Color accent = EventAccentColor(activeEvent);
        float urgency = eventStartCountdown > 0.01f
            ? 1f - Math.Clamp(eventCountdown / eventStartCountdown, 0f, 1f)
            : 1f;

        if (urgency > 0.35f && Rng.NextSingle() < dt * (2f + urgency * 6f))
        {
            AddTrauma(dt * 0.08f * urgency);
        }

        switch (activeEvent)
        {
            case FloorEventType.CrimsonCrumble:
            case FloorEventType.Checkerfall:
            case FloorEventType.RingCollapse:
            case FloorEventType.StoneIslands:
            case FloorEventType.ScatterPits:
            case FloorEventType.HeraldsCall:
                if (eventPhase == 0)
                {
                    eventCountdown -= dt;
                    if (eventCountdown <= 0f)
                    {
                        BeginEventCollapsePhase(activeEvent);
                    }
                }
                else
                {
                    ProcessStaggeredCollapse(dt, activeEvent, EventCollapseInterval(activeEvent));
                }
                break;

            case FloorEventType.SafeZoneRush:
            case FloorEventType.SallyForth:
                if (activeEvent == FloorEventType.SallyForth)
                {
                    eventActionTimer -= dt;
                    if (eventActionTimer <= 0f)
                    {
                        eventSide = (eventSide + 1) % 4;
                        ConfigureSafeZoneRush();
                        eventActionTimer = 1.2f;
                    }
                }
                RefreshSafeZoneRushRect();
                eventCountdown -= dt;
                playerInEventSafeZone = PlayerInSafeRect(eventSafeRect);
                if (!playerInEventSafeZone && Rng.NextSingle() < dt * 14f)
                {
                    SpawnSafeZoneRushTrail();
                }

                if (eventCountdown <= 0f)
                {
                    if (!PlayerInSafeRect(eventSafeRect))
                    {
                        TriggerGameOver(DeathCause.SafeZoneFailed);
                        return;
                    }

                    SpawnEventShockwave(playerPos, UiAccent, 180f);
                    SpawnFloatingText(playerPos, "SANCTIFIED!", UiAccent, 22);
                    TriggerImpact(ImpactTier.Medium, UiAccent);
                    EndFloorEvent();
                }
                break;

            case FloorEventType.MossRot:
                eventCountdown -= dt;
                if (Rng.NextSingle() < dt * 18f)
                {
                    float half = WindowWidth / 2f;
                    float rx = floorRotSide < 0.5f ? Rng.NextSingle() * half : half + Rng.NextSingle() * half;
                    float ry = Rng.NextSingle() * WindowHeight;
                    SpawnBlightSpores(new Vector2(rx, ry));
                }

                if (eventCountdown <= 0f)
                {
                    EndFloorEvent();
                }
                break;

            case FloorEventType.MarkedStrike:
            case FloorEventType.Portcullis:
                if (eventStep < markedStrikeCount)
                {
                    int idx = markedStrikeOrder[eventStep];
                    int mx = idx % GridSize;
                    int my = idx / GridSize;
                    Vector2 target = TileCenter(mx, my);

                    if (eventPhase == 0)
                    {
                        eventCountdown -= dt;
                        if (eventSkyBeams.Count == 0 || eventSkyBeams[^1].Life < 0.05f)
                        {
                            SpawnEventSkyBeam(target, accent, 12f, MarkedStrikeTelegraphTime * 0.95f, charging: true);
                        }

                        if (eventCountdown <= 0f)
                        {
                            eventPhase = 1;
                            eventCountdown = MarkedStrikeFireTime;
                            eventSkyBeams.Clear();
                            SpawnEventSkyBeam(target, accent, 38f, 0.16f, charging: false);
                            AddFlash(Lighten(accent, 0.35f), 0.18f);
                            SpawnGfxLightPulse(target, Color.White, 140f, 1.1f, 0.12f);
                            TriggerImpact(ImpactTier.Light, accent);
                        }
                    }
                    else
                    {
                        eventCountdown -= dt;
                        if (eventCountdown <= 0f)
                        {
                            if (tiles[mx, my].State != 2)
                            {
                                CollapseTileWithEventFx(mx, my, accent);
                                SpawnEventShockwave(target, accent, 52f, 0.22f);
                            }

                            eventStep++;
                            eventPhase = 0;
                            eventCountdown = MarkedStrikeStrikeGap;
                        }
                    }
                }
                else
                {
                    EndFloorEvent();
                }
                break;

            case FloorEventType.CenterSnare:
                RefreshCenterSnareRects();
                eventCountdown -= dt;
                playerInEventSafeZone = PlayerInCenterSnareSafe();

                if (eventCountdown <= 0f)
                {
                    if (!PlayerInCenterSnareSafe())
                    {
                        TriggerGameOver(DeathCause.CenterSnare);
                        return;
                    }

                    SpawnEventShockwave(playerPos, UiAccent, 160f);
                    SpawnFloatingText(playerPos, "WALL RUNNER!", UiAccent, 22);
                    EndFloorEvent();
                }
                break;

            case FloorEventType.BlightStorm:
                eventCountdown -= dt;
                eventBlightBoltTimer -= dt;

                for (int y = 0; y < GridSize; y++)
                {
                    for (int x = 0; x < GridSize; x++)
                    {
                        ref Tile t = ref tiles[x, y];
                        if (!t.EventMarked || t.State == 2) continue;
                        ApplyTileDamage(ref t, 14f + urgency * 10f, dt, out _, markTouched: false);
                    }
                }

                if (eventBlightBoltTimer <= 0f)
                {
                    eventBlightBoltTimer = 0.28f + Rng.NextSingle() * 0.22f;
                    int bx = Rng.Next(0, GridSize);
                    int by = Rng.Next(0, GridSize);
                    if (tiles[bx, by].EventMarked && tiles[bx, by].State != 2)
                    {
                        Vector2 boltPos = TileCenter(bx, by);
                        SpawnEventSkyBeam(boltPos, accent, 22f, 0.18f);
                        ApplyTileDamage(ref tiles[bx, by], 28f, 0.16f, out _, markTouched: false);
                        SpawnGfxLightPulse(boltPos, Lighten(accent, 0.4f), 80f, 0.9f, 0.15f);
                        AddFlash(accent, 0.08f);
                    }
                }

                if (eventCountdown <= 0f)
                {
                    if (eventPhase == 0)
                    {
                        BeginEventCollapsePhase(FloorEventType.BlightStorm);
                    }
                    else
                    {
                        ProcessStaggeredCollapse(dt, FloorEventType.BlightStorm, EventCollapseInterval(FloorEventType.BlightStorm));
                    }
                }
                break;

            case FloorEventType.TideSurge:
            case FloorEventType.TideRift:
            case FloorEventType.TideRecede:
            case FloorEventType.TideColumn:
            case FloorEventType.TideEcho:
            case FloorEventType.TideUndertow:
            case FloorEventType.TideCrest:
            case FloorEventType.TideWall:
            case FloorEventType.TideAnchor:
                UpdateTideTelegraphCollapse(dt, activeEvent);
                break;

            case FloorEventType.TideFoam:
                eventCountdown -= dt;
                eventBlightBoltTimer -= dt;

                for (int y = 0; y < GridSize; y++)
                {
                    for (int x = 0; x < GridSize; x++)
                    {
                        ref Tile t = ref tiles[x, y];
                        if (!t.EventMarked || t.State == 2) continue;
                        ApplyTileDamage(ref t, 12f + urgency * 9f, dt, out _, markTouched: false);
                    }
                }

                if (eventBlightBoltTimer <= 0f)
                {
                    eventBlightBoltTimer = 0.32f + Rng.NextSingle() * 0.2f;
                    int bx = Rng.Next(0, GridSize);
                    int by = Rng.Next(0, GridSize);
                    if (tiles[bx, by].EventMarked && tiles[bx, by].State != 2)
                    {
                        Vector2 boltPos = TileCenter(bx, by);
                        SpawnEventSkyBeam(boltPos, accent, 18f, 0.2f, charging: true);
                        ApplyTileDamage(ref tiles[bx, by], 22f, 0.14f, out _, markTouched: false);
                        SpawnGfxLightPulse(boltPos, Lighten(accent, 0.35f), 70f, 0.85f, 0.14f);
                    }
                }

                if (eventCountdown <= 0f)
                {
                    if (eventPhase == 0)
                    {
                        BeginEventCollapsePhase(FloorEventType.TideFoam);
                    }
                    else
                    {
                        ProcessStaggeredCollapse(dt, FloorEventType.TideFoam, EventCollapseInterval(FloorEventType.TideFoam));
                    }
                }
                break;

            case FloorEventType.TideWhirlpool:
                RefreshTideWhirlpoolRect();
                eventCountdown -= dt;
                playerInEventSafeZone = !PlayerInTideWhirlpoolDanger();
                if (eventCountdown <= 0f)
                {
                    if (PlayerInTideWhirlpoolDanger())
                    {
                        TriggerGameOver(DeathCause.TideDrowned);
                        return;
                    }

                    SpawnEventShockwave(playerPos, accent, 150f);
                    SpawnFloatingText(playerPos, "VORTEX ESCAPED!", accent, 22);
                    EndFloorEvent();
                }
                break;

            case FloorEventType.TideBeacon:
                eventCountdown -= dt;
                playerInEventSafeZone = PlayerInSafeRect(eventSafeRect);
                if (!playerInEventSafeZone && Rng.NextSingle() < dt * 14f)
                {
                    SpawnTideBeaconTrail();
                }

                if (eventCountdown <= 0f)
                {
                    if (!PlayerInSafeRect(eventSafeRect))
                    {
                        TriggerGameOver(DeathCause.TideBeaconLost);
                        return;
                    }

                    SpawnEventShockwave(playerPos, accent, 160f);
                    SpawnFloatingText(playerPos, "BEACON REACHED!", accent, 22);
                    EndFloorEvent();
                }
                break;

            case FloorEventType.TideStrike:
                if (eventStep < markedStrikeCount)
                {
                    int tidx = markedStrikeOrder[eventStep];
                    int tmx = tidx % GridSize;
                    int tmy = tidx / GridSize;
                    Vector2 ttarget = TileCenter(tmx, tmy);

                    if (eventPhase == 0)
                    {
                        eventCountdown -= dt;
                        if (eventSkyBeams.Count == 0 || eventSkyBeams[^1].Life < 0.05f)
                        {
                            SpawnEventSkyBeam(ttarget, accent, 12f, MarkedStrikeTelegraphTime * 0.95f, charging: true);
                        }

                        if (eventCountdown <= 0f)
                        {
                            eventPhase = 1;
                            eventCountdown = MarkedStrikeFireTime;
                            eventSkyBeams.Clear();
                            SpawnEventSkyBeam(ttarget, accent, 34f, 0.16f, charging: false);
                            AddFlash(Lighten(accent, 0.35f), 0.16f);
                            SpawnGfxLightPulse(ttarget, Color.White, 120f, 1f, 0.12f);
                            TriggerImpact(ImpactTier.Light, accent);
                        }
                    }
                    else
                    {
                        eventCountdown -= dt;
                        if (eventCountdown <= 0f)
                        {
                            if (tiles[tmx, tmy].State != 2)
                            {
                                CollapseTileWithEventFx(tmx, tmy, accent);
                                SpawnEventShockwave(ttarget, accent, 48f, 0.22f);
                            }

                            eventStep++;
                            eventPhase = 0;
                            eventCountdown = MarkedStrikeStrikeGap;
                        }
                    }
                }
                else
                {
                    EndFloorEvent();
                }
                break;

            case FloorEventType.EmberRain:
                eventActionTimer -= dt;
                while (eventActionTimer <= 0f && eventStep < GridSize)
                {
                    EmberRainAdvanceColumn(accent);
                    eventActionTimer += EmberRainColumnInterval;
                }
                UpdateEmberTelegraphCollapse(dt, FloorEventType.EmberRain);
                break;

            case FloorEventType.EmberGate:
                eventCountdown -= dt;
                playerInEventSafeZone = PlayerInSafeRect(eventSafeRect);
                if (eventCountdown <= 0f)
                {
                    if (!PlayerInSafeRect(eventSafeRect))
                    {
                        TriggerGameOver(DeathCause.SafeZoneFailed);
                        return;
                    }
                    SpawnEventShockwave(playerPos, accent, 160f);
                    SpawnFloatingText(playerPos, "THROUGH!", accent, 22);
                    EndFloorEvent();
                }
                break;

            case FloorEventType.EmberPulse:
                eventActionTimer -= dt;
                while (eventActionTimer <= 0f && eventStep < GridSize / 2 + 2)
                {
                    EmberPulseExpandRing(accent);
                    eventActionTimer += EmberPulseInterval;
                }
                UpdateEmberTelegraphCollapse(dt, FloorEventType.EmberPulse);
                break;

            case FloorEventType.EmberCross:
            case FloorEventType.EmberBridge:
            case FloorEventType.EmberHive:
                UpdateEmberTelegraphCollapse(dt, activeEvent);
                break;

            case FloorEventType.EmberFury:
                UpdateEmberFury(dt, accent);
                UpdateEmberTelegraphCollapse(dt, FloorEventType.EmberFury);
                break;

            case FloorEventType.EmberSnake:
                eventActionTimer -= dt;
                while (eventActionTimer <= 0f && eventStep < markedStrikeCount)
                {
                    EmberSnakeAdvance(accent);
                    eventActionTimer += EmberSnakeStepInterval;
                }
                UpdateEmberTelegraphCollapse(dt, FloorEventType.EmberSnake);
                break;

            case FloorEventType.EmberTide:
                RefreshEmberTideRects();
                eventCountdown -= dt;
                playerInEventSafeZone = PlayerInSafeRect(eventSafeRect);
                if (eventCountdown <= 0f)
                {
                    if (!PlayerInSafeRect(eventSafeRect) || Raylib.CheckCollisionCircleRec(playerPos, PlayerRadius, eventDangerRect))
                    {
                        TriggerGameOver(DeathCause.SafeZoneFailed);
                        return;
                    }
                    SpawnFloatingText(playerPos, "ABOVE THE TIDE!", accent, 20);
                    EndFloorEvent();
                }
                break;

            case FloorEventType.EmberCage:
                RefreshEmberCageRect();
                eventCountdown -= dt;
                playerInEventSafeZone = PlayerInSafeRect(eventSafeRect);
                if (eventCountdown <= 0f)
                {
                    if (!PlayerInSafeRect(eventSafeRect))
                    {
                        TriggerGameOver(DeathCause.CenterSnare);
                        return;
                    }
                    SpawnFloatingText(playerPos, "CAGE HELD!", accent, 20);
                    EndFloorEvent();
                }
                break;

            case FloorEventType.EmberQuake:
                eventActionTimer -= dt;
                while (eventActionTimer <= 0f && eventCountdown > 0.15f)
                {
                    EmberQuakeStrike(accent);
                    eventActionTimer += EmberQuakeInterval;
                }
                UpdateEmberTelegraphCollapse(dt, FloorEventType.EmberQuake);
                break;

            case FloorEventType.EmberBloom:
                eventActionTimer -= dt;
                while (eventActionTimer <= 0f && eventStep < 6)
                {
                    EmberBloomExpand(accent);
                    eventActionTimer += EmberBloomInterval;
                }
                UpdateEmberTelegraphCollapse(dt, FloorEventType.EmberBloom);
                break;

            case FloorEventType.EmberAltar:
                eventCountdown -= dt;
                playerInEventSafeZone = PlayerInEmberAltar();
                if (eventCountdown <= 0f)
                {
                    if (!PlayerInEmberAltar())
                    {
                        TriggerGameOver(DeathCause.SafeZoneFailed);
                        return;
                    }
                    SpawnEventShockwave(playerPos, accent, 140f);
                    SpawnFloatingText(playerPos, "ALTAR CLAIMED!", accent, 22);
                    EndFloorEvent();
                }
                break;

            case FloorEventType.CryptSeal:
            case FloorEventType.CryptWail:
            case FloorEventType.CryptTorch:
            case FloorEventType.CryptChains:
            case FloorEventType.CryptGlimpse:
            case FloorEventType.CryptRattle:
            case FloorEventType.CryptEcho:
            case FloorEventType.CryptShroud:
            case FloorEventType.CryptLantern:
            case FloorEventType.CryptTomb:
                if (activeEvent == FloorEventType.CryptWail && eventPhase == 0 && Rng.NextSingle() < dt * 5f)
                {
                    float wavePos = eventSide switch
                    {
                        0 => eventCountdown / eventStartCountdown * WindowWidth,
                        1 => WindowWidth - eventCountdown / eventStartCountdown * WindowWidth,
                        2 => eventCountdown / eventStartCountdown * WindowHeight,
                        _ => WindowHeight - eventCountdown / eventStartCountdown * WindowHeight,
                    };
                    Vector2 wailCenter = eventSide < 2
                        ? new Vector2(wavePos, WindowHeight / 2f)
                        : new Vector2(WindowWidth / 2f, wavePos);
                    SpawnEventShockwave(wailCenter, accent, 64f, 0.28f);
                }

                if (eventPhase == 0)
                {
                    eventCountdown -= dt;
                    if (eventCountdown <= 0f)
                    {
                        BeginEventCollapsePhase(activeEvent);
                    }
                }
                else
                {
                    ProcessStaggeredCollapse(dt, activeEvent, EventCollapseInterval(activeEvent));
                }
                break;

            case FloorEventType.CryptMist:
                eventCountdown -= dt;
                eventBlightBoltTimer -= dt;
                if (Rng.NextSingle() < dt * 14f)
                {
                    float half = WindowWidth / 2f;
                    float rx = floorRotSide < 0.5f ? Rng.NextSingle() * half : half + Rng.NextSingle() * half;
                    float ry = Rng.NextSingle() * WindowHeight;
                    SpawnBlightSpores(new Vector2(rx, ry));
                }

                for (int y = 0; y < GridSize; y++)
                {
                    for (int x = 0; x < GridSize; x++)
                    {
                        ref Tile t = ref tiles[x, y];
                        if (!t.EventMarked || t.State == 2) continue;
                        ApplyTileDamage(ref t, 12f + urgency * 9f, dt, out _, markTouched: false);
                    }
                }

                if (eventBlightBoltTimer <= 0f)
                {
                    eventBlightBoltTimer = 0.32f + Rng.NextSingle() * 0.2f;
                    int bx = Rng.Next(0, GridSize);
                    int by = Rng.Next(0, GridSize);
                    if (tiles[bx, by].EventMarked && tiles[bx, by].State != 2)
                    {
                        Vector2 boltPos = TileCenter(bx, by);
                        SpawnEventSkyBeam(boltPos, accent, 18f, 0.2f, charging: true);
                        ApplyTileDamage(ref tiles[bx, by], 24f, 0.14f, out _, markTouched: false);
                        SpawnGfxLightPulse(boltPos, Lighten(accent, 0.35f), 70f, 0.85f, 0.14f);
                    }
                }

                if (eventCountdown <= 0f)
                {
                    if (eventPhase == 0)
                    {
                        BeginEventCollapsePhase(FloorEventType.CryptMist);
                    }
                    else
                    {
                        ProcessStaggeredCollapse(dt, FloorEventType.CryptMist, EventCollapseInterval(FloorEventType.CryptMist));
                    }
                }
                break;

            case FloorEventType.CryptGrave:
                if (eventStep < markedStrikeCount)
                {
                    int idx = markedStrikeOrder[eventStep];
                    int mx = idx % GridSize;
                    int my = idx / GridSize;
                    Vector2 target = TileCenter(mx, my);

                    if (eventPhase == 0)
                    {
                        eventCountdown -= dt;
                        if (eventSkyBeams.Count == 0 || eventSkyBeams[^1].Life < 0.05f)
                        {
                            SpawnEventSkyBeam(target, accent, 10f, CryptGraveTelegraphTime * 0.95f, charging: true);
                        }

                        if (eventCountdown <= 0f)
                        {
                            eventPhase = 1;
                            eventCountdown = CryptGraveFireTime;
                            eventSkyBeams.Clear();
                            SpawnEventSkyBeam(target, accent, 32f, 0.14f, charging: false);
                            AddFlash(Lighten(accent, 0.3f), 0.16f);
                            SpawnGfxLightPulse(target, new Color(220, 210, 240, 255), 120f, 1f, 0.1f);
                            TriggerImpact(ImpactTier.Light, accent);
                        }
                    }
                    else
                    {
                        eventCountdown -= dt;
                        if (eventCountdown <= 0f)
                        {
                            if (tiles[mx, my].State != 2)
                            {
                                CollapseTileWithEventFx(mx, my, accent);
                                SpawnEventShockwave(target, accent, 44f, 0.2f);
                            }

                            eventStep++;
                            eventPhase = 0;
                            eventCountdown = CryptGraveStrikeGap;
                        }
                    }
                }
                else
                {
                    EndFloorEvent();
                }
                break;

            case FloorEventType.CryptVeil:
                eventActionTimer -= dt;
                if (eventActionTimer <= 0f)
                {
                    eventActionTimer = CryptVeilRotateInterval;
                    eventSide = (eventSide + 1) % 4;
                    ConfigureCryptVeil();
                    AddFlash(accent, 0.08f);
                    Vector2 veilCenter = new Vector2(
                        eventSafeRect.X + eventSafeRect.Width * 0.5f,
                        eventSafeRect.Y + eventSafeRect.Height * 0.5f);
                    SpawnEventShockwave(veilCenter, accent, 88f, 0.32f);
                }

                eventCountdown -= dt;
                playerInEventSafeZone = PlayerInSafeRect(eventSafeRect);
                if (eventCountdown <= 0f)
                {
                    if (!PlayerInSafeRect(eventSafeRect))
                    {
                        TriggerGameOver(DeathCause.SafeZoneFailed);
                        return;
                    }

                    SpawnEventShockwave(playerPos, accent, 150f);
                    SpawnFloatingText(playerPos, "VEILED!", accent, 22);
                    EndFloorEvent();
                }
                break;

            case FloorEventType.CrownTrial:
            case FloorEventType.CrownFall:
            case FloorEventType.CrownShard:
            case FloorEventType.CrownRing:
            case FloorEventType.CrownIsles:
            case FloorEventType.CrownCoronation:
            case FloorEventType.CrownUsurpation:
            case FloorEventType.CrownReckoning:
                UpdateEmberTelegraphCollapse(dt, activeEvent);
                break;

            case FloorEventType.CrownThrone:
                if (eventPhase == 0)
                {
                    eventCountdown -= dt;
                    playerInEventSafeZone = PlayerInSafeRect(eventSafeRect);
                    if (eventCountdown <= 0f)
                    {
                        if (!PlayerInSafeRect(eventSafeRect))
                        {
                            TriggerGameOver(DeathCause.SafeZoneFailed);
                            return;
                        }

                        SpawnEventShockwave(playerPos, accent, 170f);
                        SpawnFloatingText(playerPos, "CROWNED!", accent, 22);
                        BeginEventCollapsePhase(FloorEventType.CrownThrone);
                    }
                }
                else
                {
                    ProcessStaggeredCollapse(dt, FloorEventType.CrownThrone, EventCollapseInterval(FloorEventType.CrownThrone));
                }
                break;

            case FloorEventType.CrownEdict:
                RefreshSafeZoneRushRect();
                eventCountdown -= dt;
                playerInEventSafeZone = PlayerInSafeRect(eventSafeRect);
                if (!playerInEventSafeZone && Rng.NextSingle() < dt * 14f)
                {
                    SpawnSafeZoneRushTrail();
                }

                if (eventCountdown <= 0f)
                {
                    if (!PlayerInSafeRect(eventSafeRect))
                    {
                        TriggerGameOver(DeathCause.SafeZoneFailed);
                        return;
                    }

                    SpawnEventShockwave(playerPos, accent, 180f);
                    SpawnFloatingText(playerPos, "ROYAL DECREE!", accent, 22);
                    EndFloorEvent();
                }
                break;

            case FloorEventType.CrownRot:
                eventCountdown -= dt;
                if (Rng.NextSingle() < dt * 18f)
                {
                    float half = WindowWidth / 2f;
                    float rx = floorRotSide < 0.5f ? Rng.NextSingle() * half : half + Rng.NextSingle() * half;
                    float ry = Rng.NextSingle() * WindowHeight;
                    SpawnBlightSpores(new Vector2(rx, ry));
                }

                if (eventCountdown <= 0f)
                {
                    EndFloorEvent();
                }
                break;

            case FloorEventType.CrownBolt:
                if (eventStep < markedStrikeCount)
                {
                    int cidx = markedStrikeOrder[eventStep];
                    int cmx = cidx % GridSize;
                    int cmy = cidx / GridSize;
                    Vector2 ctarget = TileCenter(cmx, cmy);

                    if (eventPhase == 0)
                    {
                        eventCountdown -= dt;
                        if (eventSkyBeams.Count == 0 || eventSkyBeams[^1].Life < 0.05f)
                        {
                            SpawnEventSkyBeam(ctarget, accent, 14f, MarkedStrikeTelegraphTime * 0.95f, charging: true);
                        }

                        if (eventCountdown <= 0f)
                        {
                            eventPhase = 1;
                            eventCountdown = MarkedStrikeFireTime;
                            eventSkyBeams.Clear();
                            SpawnEventSkyBeam(ctarget, accent, 42f, 0.16f, charging: false);
                            AddFlash(Lighten(accent, 0.4f), 0.2f);
                            SpawnGfxLightPulse(ctarget, Color.White, 150f, 1.15f, 0.12f);
                            TriggerImpact(ImpactTier.Light, accent);
                        }
                    }
                    else
                    {
                        eventCountdown -= dt;
                        if (eventCountdown <= 0f)
                        {
                            if (tiles[cmx, cmy].State != 2)
                            {
                                CollapseTileWithEventFx(cmx, cmy, accent);
                                SpawnEventShockwave(ctarget, accent, 56f, 0.24f);
                            }

                            eventStep++;
                            eventPhase = 0;
                            eventCountdown = MarkedStrikeStrikeGap;
                        }
                    }
                }
                else
                {
                    EndFloorEvent();
                }
                break;

            case FloorEventType.CrownStorm:
                eventCountdown -= dt;
                eventBlightBoltTimer -= dt;

                for (int cy = 0; cy < GridSize; cy++)
                {
                    for (int cx = 0; cx < GridSize; cx++)
                    {
                        ref Tile ct = ref tiles[cx, cy];
                        if (!ct.EventMarked || ct.State == 2) continue;
                        ApplyTileDamage(ref ct, 13f + urgency * 11f, dt, out _, markTouched: false);
                    }
                }

                if (eventBlightBoltTimer <= 0f)
                {
                    eventBlightBoltTimer = 0.26f + Rng.NextSingle() * 0.2f;
                    int cbx = Rng.Next(0, GridSize);
                    int cby = Rng.Next(0, GridSize);
                    if (tiles[cbx, cby].EventMarked && tiles[cbx, cby].State != 2)
                    {
                        Vector2 cboltPos = TileCenter(cbx, cby);
                        SpawnEventSkyBeam(cboltPos, accent, 24f, 0.18f);
                        ApplyTileDamage(ref tiles[cbx, cby], 30f, 0.16f, out _, markTouched: false);
                        SpawnGfxLightPulse(cboltPos, Lighten(accent, 0.45f), 88f, 0.95f, 0.15f);
                        AddFlash(accent, 0.09f);
                    }
                }

                if (eventCountdown <= 0f)
                {
                    if (eventPhase == 0)
                    {
                        BeginEventCollapsePhase(FloorEventType.CrownStorm);
                    }
                    else
                    {
                        ProcessStaggeredCollapse(dt, FloorEventType.CrownStorm, EventCollapseInterval(FloorEventType.BlightStorm));
                    }
                }
                break;

            case FloorEventType.None:
                break;

            default:
                throw new UnreachableException();
        }
    }

    static void EndFloorEvent(bool fromVerdict = false)
    {
        activeEvent = FloorEventType.None;
        eventTimer = 0f;
        eventPhase = 0;
        eventActionTimer = 0f;
        eventSurgeTimer = 0f;
        eventCountdown = 0f;
        eventStartCountdown = 0f;
        eventBlightBoltTimer = 0f;
        eventStep = 0;
        markedStrikeCount = 0;
        eventTileQueue.Clear();
        eventShockwaves.Clear();
        eventSkyBeams.Clear();
        playerInEventSafeZone = false;
        eventSafeRect = default;
        eventDangerRect = default;
        emberFuryTileIdx = -1;
        emberFuryStandTimer = 0f;
        ClearEventMarks();

        if (!fromVerdict && activeDifficulty.EventStackChance > 0f && Rng.NextSingle() < activeDifficulty.EventStackChance)
        {
            nextFloorEventTimer = Math.Max(0f, nextFloorEventCooldown - 2.2f);
        }
    }

    static float SafeRushBandPx() => SafeRushBandTiles * TileSize;
    static float CenterSnareMarginPx() => CenterSnareMarginTiles * TileSize;

    static void ConfigureSafeZoneRush()
    {
        float band = SafeRushBandPx();
        eventSafeRect = eventSide switch
        {
            0 => new Rectangle(0f, 0f, band, WindowHeight),
            1 => new Rectangle(WindowWidth - band, 0f, band, WindowHeight),
            2 => new Rectangle(0f, 0f, WindowWidth, band),
            3 => new Rectangle(0f, WindowHeight - band, WindowWidth, band),
            _ => throw new UnreachableException(),
        };
        eventDangerRect = default;
    }

    static void ConfigureCenterSnare()
    {
        float m = CenterSnareMarginPx();
        eventDangerRect = new Rectangle(m, m, WindowWidth - m * 2f, WindowHeight - m * 2f);
        eventSafeRect = default;
    }

    static bool PlayerInSafeRect(Rectangle safe)
    {
        if (safe.Width <= 0f || safe.Height <= 0f) return false;
        return Raylib.CheckCollisionCircleRec(playerPos, PlayerRadius, safe);
    }

    static bool PlayerInCenterSnareSafe()
    {
        float m = CurrentCenterSnareMarginPx();
        return playerPos.X - PlayerRadius < m
            || playerPos.X + PlayerRadius > WindowWidth - m
            || playerPos.Y - PlayerRadius < m
            || playerPos.Y + PlayerRadius > WindowHeight - m;
    }

    static string SafeZoneRushHint() => eventSide switch
    {
        0 => "SAFE: LEFT EDGE",
        1 => "SAFE: RIGHT EDGE",
        2 => "SAFE: TOP EDGE",
        3 => "SAFE: BOTTOM EDGE",
        _ => "",
    };

    static void DrawCenterSnareSafeBands(float time)
    {
        float m = CurrentCenterSnareMarginPx();
        float pulse = 0.18f + MathF.Sin(time * 8f) * 0.08f;
        float urgency = eventStartCountdown > 0.01f
            ? 1f - Math.Clamp(eventCountdown / eventStartCountdown, 0f, 1f)
            : 0f;
        Color fill = WithAlpha(UiAccent, 0.06f + pulse * 0.04f + urgency * 0.04f);
        Color border = WithAlpha(UiAccent, 0.12f + pulse * 0.08f + urgency * 0.06f);

        var top = new Rectangle(0f, 0f, WindowWidth, m);
        var bottom = new Rectangle(0f, WindowHeight - m, WindowWidth, m);
        var left = new Rectangle(0f, m, m, WindowHeight - m * 2f);
        var right = new Rectangle(WindowWidth - m, m, m, WindowHeight - m * 2f);

        Raylib.DrawRectangleRec(top, fill);
        Raylib.DrawRectangleRec(bottom, fill);
        Raylib.DrawRectangleRec(left, fill);
        Raylib.DrawRectangleRec(right, fill);

        DrawPulseFrameLite(top, border, 0.02f, 5f, 0.1f);
        DrawPulseFrameLite(bottom, border, 0.02f, 5f, 0.1f);
        DrawPulseFrameLite(left, border, 0.02f, 5f, 0.1f);
        DrawPulseFrameLite(right, border, 0.02f, 5f, 0.1f);
    }

    static void UpdateBlight(float dt)
    {
        blightTimer += dt;
        float blightInterval = 10f * activeDifficulty.WavePauseMult;
        if (blightTimer < blightInterval) return;

        blightTimer = 0f;
        int bx = Rng.Next(1, GridSize - 1);
        int by = Rng.Next(1, GridSize - 1);

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                ref Tile tile = ref tiles[bx + i, by + j];
                if (tile.State == 0)
                {
                    tile.State = 1;
                    tile.Durability = Math.Min(tile.Durability, MaxDurability * 0.45f);
                }
            }
        }

        AddTrauma(0.32f);
        AddFlash(Amber, 0.18f);
        SpawnBlightSpores(new Vector2((bx + 0.5f) * TileSize, (by + 0.5f) * TileSize));
    }

    static bool IsBossWave(int wave) => wave == 10 || (wave >= 25 && wave % 25 == 0);

    static bool IsTitanBossWave(int wave) => wave >= 50 && wave % 50 == 0;

    // Linear ramp: gentle early waves, full pressure by ~wave 100.
    static float WaveEnemyScale(int wave) => MathF.Min(1f, 0.35f + wave * 0.0065f);

    static float WaveSpeedBonus(int wave)
    {
        float eased = MathF.Max(0f, wave - 3f);
        return eased * 1.35f * WaveEnemyScale(wave) * activeDifficulty.EnemySpeedMult;
    }

    static int WaveSwarmCount(int wave)
    {
        int baseCount = 1 + Math.Min(6, Math.Max(0, (wave + 2) / 3));
        return Math.Max(1, (int)MathF.Round(baseCount * activeDifficulty.SwarmCountMult));
    }

    static int WaveGruntsPerSwarm(int wave, int swarmIndex)
    {
        if (runDifficulty == Difficulty.PracticeHall) return 0;
        int baseCount = 1 + (int)(wave * 0.1f) + swarmIndex / 2;
        int cap = 4 + (int)(wave * 0.08f);
        int count = Math.Clamp(baseCount, 1, Math.Min(cap, 14));
        return Math.Max(1, (int)MathF.Round(count * activeDifficulty.GruntCountMult));
    }

    static void UpdateWaves(float dt)
    {
        if (waveInProgress)
        {
            UpdateSiegeObjective(dt);
            if (swarmCooldown > 0f)
            {
                swarmCooldown -= dt;
            }

            if (swarmCooldown <= 0f && waveSwarmIndex < waveSwarmTotal)
            {
                SpawnWaveSwarm();
                waveSwarmIndex++;
                swarmCooldown = SwarmIntervalBase + 1.4f + Rng.NextSingle() * 0.9f - waveNumber * 0.022f;
                swarmCooldown = Math.Max(1.35f, swarmCooldown) * activeDifficulty.SwarmIntervalMult;
                waveSubtext = $"SWARM {waveSwarmIndex}/{waveSwarmTotal}";
            }
            else if (waveSwarmIndex >= waveSwarmTotal && CountLivingEnemies() == 0)
            {
                CompleteWave();
            }

            return;
        }

        betweenWaveTimer += dt;
        UpdateBetweenWaveAmbience(dt);
        float pause = MathF.Max(BetweenWavePause, 5.2f - waveNumber * 0.035f) * activeDifficulty.WavePauseMult;
        if (betweenWaveTimer < pause) return;

        betweenWaveTimer = 0f;
        BeginWave();
    }

    static int CountLivingEnemies()
    {
        int n = 0;
        foreach (Enemy e in enemies)
        {
            if (!e.Dead && e.Spawn >= 0.5f) n++;
        }
        return n;
    }

    static void BeginWave()
    {
        waveNumber++;
        waveInProgress = true;
        waveSwarmIndex = 0;
        waveSwarmTotal = WaveSwarmCount(waveNumber);
        swarmCooldown = 0.35f;
        waveBannerTimer = WaveBannerTime;
        siegeGateOpenTimer = IsBossWave(waveNumber) ? SiegeGateOpenDuration * 1.35f : SiegeGateOpenDuration;
        waveSubtext = IsBossWave(waveNumber)
            ? IsTitanBossWave(waveNumber) ? "THE TITAN WALKS THE WALLS" : "BOSSES AT THE GATE"
            : "FOOTSTEPS ON THE STONE";
        RollSiegeObjective();
        BeginReverseSiege();
        AddFlash(UiAccent, 0.10f);
        AddTrauma(0.12f);
    }

    static void CompleteWave()
    {
        waveInProgress = false;
        betweenWaveTimer = 0f;
        waveSubtext = "A BREATH BETWEEN WAVES";

        if (waveNumber > 0)
        {
            if (!IsPracticeRun()) maxWaveReached = Math.Max(maxWaveReached, waveNumber);
            UpdateDifficultyRecords(false);
            int reward = (int)MathF.Round((10 + waveNumber * 4) * FableMult());
            fables += reward;
            runFablesEarned += reward;
            int waveXp = 18 + waveNumber * 9;
            GrantXp(waveXp);
            SpawnFloatingText(playerPos + new Vector2(0, -PlayerRadius - 18f), "+" + reward + " fables", Gold, 20);
            SpawnFloatingText(playerPos + new Vector2(0, -PlayerRadius - 38f), "+" + waveXp + " xp", UiAccent, 16);
            CompleteSiegeObjectiveBonus();
        }

        OfferBlessings();
    }

    static void SpawnWaveSwarm()
    {
        float speedBonus = WaveSpeedBonus(waveNumber);
        bool lastSwarm = waveSwarmIndex >= waveSwarmTotal - 1;

        if (lastSwarm && IsBossWave(waveNumber))
        {
            if (IsTitanBossWave(waveNumber))
            {
                enemies.Add(CreateEnemy(EnemyType.GroveTitan, RandomEdgePosition(GetEnemyRadius(EnemyType.GroveTitan)), speedBonus));
                SpawnFloatingText(new Vector2(WindowWidth / 2f, 120f), "KEEP COLOSSUS!", Danger, 28);
                AddTrauma(0.45f);
            }
            else
            {
                EnemyType boss = Rng.Next(2) == 0 ? EnemyType.BrambleLord : EnemyType.FoxWarden;
                enemies.Add(CreateEnemy(boss, RandomEdgePosition(GetEnemyRadius(boss)), speedBonus));
                SpawnFloatingText(new Vector2(WindowWidth / 2f, 120f), GetEnemyName(boss).ToUpperInvariant() + "!", Danger, 24);
                string bark = GetBossBark(boss);
                if (!string.IsNullOrEmpty(bark))
                {
                    SpawnFloatingText(new Vector2(WindowWidth / 2f, 148f), bark, WithAlpha(Color.White, 0.85f), 14);
                }
                AddTrauma(0.32f);
            }
        }

        int gruntCount = WaveGruntsPerSwarm(waveNumber, waveSwarmIndex);
        siegeGruntsSpawned += gruntCount;
        for (int i = 0; i < gruntCount; i++)
        {
            EnemyType type = PickGruntType();
            enemies.Add(CreateEnemy(type, RandomEdgePosition(GetEnemyRadius(type)), speedBonus));
        }
    }

    static ref readonly EnemyDef GetDef(EnemyType type) => ref EnemyCatalog[(int)type];

    static Enemy CreateEnemy(EnemyType type, Vector2 position, float speedBonus)
    {
        ref readonly EnemyDef def = ref GetDef(type);
        float scale = WaveEnemyScale(waveNumber);
        var e = new Enemy
        {
            Type = type,
            Position = position,
            Speed = def.Speed + speedBonus,
            Radius = def.Radius,
            MaxHp = (def.HpBase + def.HpPerWave * waveNumber) * scale * activeDifficulty.EnemyHpMult,
            Wobble = Rng.NextSingle() * MathF.PI * 2f,
            Spawn = 0f,
            AbilityTimer = def.Boss ? 5f : BehaviorCooldown(def.Behavior),
            Phase = 0f,
            AbilityStep = 0,
            StrikeTx = 0,
            StrikeTy = 0,
            StrikeDepth = 3,
            ParalyzeTimer = 0f,
        };
        e.Hp = e.MaxHp;

        if (def.Behavior == EnemyBehavior.FastChase || def.Behavior == EnemyBehavior.BossDash)
        {
            float cap = def.Behavior == EnemyBehavior.BossDash ? 10f : 14f;
            e.Speed = MathF.Min(e.Speed, EffMoveSpeed() - cap);
        }

        if (runDifficulty >= Difficulty.Champion && waveNumber >= 15 && !def.Boss && Rng.NextSingle() < 0.12f)
        {
            e.Elite = true;
            e.MaxHp *= 1.25f;
            e.Hp = e.MaxHp;
            e.Speed *= 1.08f;
        }

        e.Position = SnapEnemyToValidTile(e.Position, e.Radius);
        if (IsWithinPlayerSpawnExclusion(e.Position))
        {
            e.Position = RandomSpawnPositionAwayFromPlayer(e.Radius);
            e.Position = SnapEnemyToValidTile(e.Position, e.Radius);
        }

        return e;
    }

    static float BehaviorCooldown(EnemyBehavior b) => b switch
    {
        EnemyBehavior.Hop => 1.1f,
        EnemyBehavior.Phaser => 1.8f,
        EnemyBehavior.Charge => 2.2f,
        EnemyBehavior.PulseBlight => 2.8f,
        EnemyBehavior.BlightTrail => 0.55f,
        EnemyBehavior.CrushTiles => 1.6f,
        EnemyBehavior.Lurker => 0f,
        _ => 0f,
    };

    static bool IsBoss(EnemyType type) => GetDef(type).Boss;

    static float EnemyMaxHp(EnemyType type)
    {
        ref readonly EnemyDef def = ref GetDef(type);
        return (def.HpBase + def.HpPerWave * waveNumber) * WaveEnemyScale(waveNumber) * activeDifficulty.EnemyHpMult;
    }

    static EnemyType PickGruntType()
    {
        EnemyType pick = EnemyType.BrambleStalker;
        int count = 0;
        for (int i = 0; i < EnemyCatalog.Length; i++)
        {
            ref readonly EnemyDef def = ref EnemyCatalog[i];
            if (def.Boss) continue;
            if (def.MinWave + activeDifficulty.GruntMinWaveBonus > waveNumber) continue;
            count++;
            if (Rng.Next(count) == 0) pick = (EnemyType)i;
        }
        return pick;
    }

    static Vector2 ClampToArena(Vector2 pos, float radius)
    {
        return new Vector2(
            Math.Clamp(pos.X, radius, WindowWidth - radius),
            Math.Clamp(pos.Y, radius, WindowHeight - radius));
    }

    static bool IsTileViable(int x, int y) =>
        x >= 0 && x < GridSize && y >= 0 && y < GridSize && tiles[x, y].State != 2;

    static Vector2 SnapEnemyToValidTile(Vector2 pos, float radius)
    {
        if (TryGetTileUnder(pos, out int tx, out int ty) && IsTileViable(tx, ty))
        {
            return ClampToArena(pos, radius);
        }

        Vector2 best = new Vector2(WindowWidth / 2f, WindowHeight / 2f);
        float bestDist = float.MaxValue;
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if (!IsTileViable(x, y)) continue;
                Vector2 center = TileCenter(x, y);
                float d = Vector2.DistanceSquared(pos, center);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = center;
                }
            }
        }

        return ClampToArena(best, radius);
    }

    static Vector2 ComputeEnemyMove(ref Enemy enemy, float dt, float time)
    {
        ref readonly EnemyDef def = ref GetDef(enemy.Type);
        Vector2 toPlayer = playerPos - enemy.Position;
        float dist = toPlayer.Length();
        Vector2 dir = dist > 0.01f ? toPlayer / dist : lastMoveDirection;
        Vector2 perp = new Vector2(-dir.Y, dir.X);
        Vector2 result;

        switch (def.Behavior)
        {
            case EnemyBehavior.Chase:
            case EnemyBehavior.TileLeech:
                result = dir * enemy.Speed;
                break;

            case EnemyBehavior.FastChase:
                result = dir * enemy.Speed;
                break;

            case EnemyBehavior.CrushTiles:
                result = dir * enemy.Speed;
                break;

            case EnemyBehavior.Orbit:
            {
                float ring = 130f;
                if (dist > ring + 40f) result = dir * enemy.Speed;
                else if (dist < ring - 40f) result = -dir * enemy.Speed * 0.85f;
                else
                {
                    float spin = enemy.Wobble > MathF.PI ? 1f : -1f;
                    result = perp * enemy.Speed * spin;
                }
                break;
            }

            case EnemyBehavior.Zigzag:
            {
                float weave = MathF.Sin(time * 7f + enemy.Wobble) * 0.55f;
                Vector2 step = Vector2.Normalize(dir + perp * weave);
                result = step * enemy.Speed;
                break;
            }

            case EnemyBehavior.Hop:
            {
                enemy.AbilityTimer -= dt;
                if (enemy.AbilityTimer <= 0f)
                {
                    enemy.Position = ClampToArena(enemy.Position + dir * 88f, enemy.Radius);
                    enemy.Position = SnapEnemyToValidTile(enemy.Position, enemy.Radius);
                    enemy.AbilityTimer = 1.15f + Rng.NextSingle() * 0.4f;
                    enemy.AbilityStep = 0;
                }
                else if (enemy.AbilityTimer < 0.42f)
                {
                    enemy.AbilityStep = 1;
                    Vector2 dest = ClampToArena(enemy.Position + dir * 88f, enemy.Radius);
                    if (TryGetTileUnder(dest, out int dtx, out int dty))
                    {
                        enemy.StrikeTx = dtx;
                        enemy.StrikeTy = dty;
                    }
                }
                else
                {
                    enemy.AbilityStep = 0;
                }
                result = dir * enemy.Speed * 0.35f;
                break;
            }

            case EnemyBehavior.BlightTrail:
                result = dir * enemy.Speed;
                break;

            case EnemyBehavior.Kite:
            {
                float prefer = 168f;
                if (dist < prefer - 30f) result = -dir * enemy.Speed;
                else if (dist > prefer + 40f) result = dir * enemy.Speed;
                else result = perp * enemy.Speed * 0.7f;
                break;
            }

            case EnemyBehavior.Charge:
            {
                enemy.AbilityTimer -= dt;
                if (enemy.Phase > 0f)
                {
                    enemy.Phase -= dt;
                    enemy.AbilityStep = 3;
                    result = dir * enemy.Speed * 2.1f;
                    break;
                }
                if (enemy.AbilityStep == 2)
                {
                    if (enemy.AbilityTimer <= 0f)
                    {
                        enemy.Phase = 0.35f;
                        enemy.AbilityTimer = 2.4f;
                        enemy.AbilityStep = 3;
                        result = dir * enemy.Speed * 2.1f;
                        break;
                    }
                    result = dir * enemy.Speed * 0.28f;
                    break;
                }
                if (enemy.AbilityTimer <= 0f && dist < 260f)
                {
                    enemy.AbilityStep = 2;
                    enemy.AbilityTimer = ChargeTelegraphTime;
                    result = dir * enemy.Speed * 0.28f;
                    break;
                }
                enemy.AbilityStep = 0;
                result = dir * enemy.Speed * 0.65f;
                break;
            }

            case EnemyBehavior.Lurker:
            {
                if (dist > 200f) result = dir * enemy.Speed * 0.45f;
                else result = dir * enemy.Speed * 1.35f;
                break;
            }

            case EnemyBehavior.Phaser:
            {
                enemy.AbilityTimer -= dt;
                if (enemy.AbilityTimer <= 0f)
                {
                    enemy.Position = SnapEnemyToValidTile(enemy.Position + dir * 72f, enemy.Radius);
                    enemy.AbilityTimer = 1.7f + Rng.NextSingle() * 0.6f;
                    enemy.AbilityStep = 0;
                }
                else if (enemy.AbilityTimer < 0.48f)
                {
                    enemy.AbilityStep = 1;
                    Vector2 dest = ClampToArena(enemy.Position + dir * 72f, enemy.Radius);
                    if (TryGetTileUnder(dest, out int dtx, out int dty))
                    {
                        enemy.StrikeTx = dtx;
                        enemy.StrikeTy = dty;
                    }
                }
                else
                {
                    enemy.AbilityStep = 0;
                }
                result = dir * enemy.Speed * 0.5f;
                break;
            }

            case EnemyBehavior.PulseBlight:
            case EnemyBehavior.Sapper:
            case EnemyBehavior.Rotburst:
            case EnemyBehavior.Splitter:
                result = dir * enemy.Speed * 0.8f;
                break;

            case EnemyBehavior.BossBlight:
            case EnemyBehavior.BossDash:
                result = dir * enemy.Speed;
                break;

            case EnemyBehavior.BossSmash:
                result = enemy.AbilityStep > 0 ? dir * enemy.Speed * 0.22f : dir * enemy.Speed * 0.62f;
                break;

            default:
                result = dir * enemy.Speed;
                break;
        }

        return ApplyFloorStateBias(result, in enemy, def.Behavior);
    }

    static void TickEnemyBehavior(ref Enemy enemy, float dt, float time)
    {
        ref readonly EnemyDef def = ref GetDef(enemy.Type);
        TickBossPhases(ref enemy, in def);

        if (def.Behavior == EnemyBehavior.CrushTiles)
        {
            if (enemy.AbilityStep == 1)
            {
                enemy.AbilityTimer -= dt;
                if (enemy.AbilityTimer <= 0f)
                {
                    if (IsTileViable(enemy.StrikeTx, enemy.StrikeTy))
                    {
                        CollapseTile(ref tiles[enemy.StrikeTx, enemy.StrikeTy]);
                        AddFlash(Danger, 0.06f);
                    }
                    enemy.AbilityStep = 0;
                    enemy.AbilityTimer = CrushCooldown;
                }
            }
            else
            {
                enemy.AbilityTimer -= dt;
                if (enemy.AbilityTimer <= 0f
                    && TryGetTileUnder(enemy.Position, out int tx, out int ty)
                    && IsTileViable(tx, ty))
                {
                    enemy.StrikeTx = tx;
                    enemy.StrikeTy = ty;
                    enemy.AbilityStep = 1;
                    enemy.AbilityTimer = CrushTelegraphTime;
                }
            }
        }

        if (def.Behavior == EnemyBehavior.BlightTrail)
        {
            enemy.AbilityTimer -= dt;
            if (enemy.AbilityTimer <= 0f && TryGetTileUnder(enemy.Position, out int bx, out int by))
            {
                enemy.AbilityTimer = 0.55f;
                ref Tile t = ref tiles[bx, by];
                if (t.State == 0)
                {
                    t.State = 1;
                    t.Durability = Math.Min(t.Durability, MaxDurability * 0.5f);
                }
            }
        }

        if (def.Behavior == EnemyBehavior.PulseBlight || def.Behavior == EnemyBehavior.BossBlight)
        {
            enemy.AbilityTimer -= dt;
            if (enemy.AbilityTimer <= 0f)
            {
                enemy.AbilityTimer = def.Boss ? 5.2f : 2.8f;
                BlightArea(enemy.Position, def.Boss ? 1 : 1);
            }
        }

        if (def.Behavior == EnemyBehavior.Sapper && TryGetTileUnder(enemy.Position, out int sx, out int sy))
        {
            for (int j = -1; j <= 1; j++)
            {
                for (int i = -1; i <= 1; i++)
                {
                    int tx = sx + i, ty = sy + j;
                    if (!IsTileViable(tx, ty)) continue;
                    ref Tile t = ref tiles[tx, ty];
                    ApplyTileDamage(ref t, 8f * TileDecayMultiplier(tx, ty), dt, out _);
                }
            }
        }

        if (def.Behavior == EnemyBehavior.BossDash)
        {
            if (enemy.AbilityStep == 1)
            {
                enemy.AbilityTimer -= dt;
                if (enemy.AbilityTimer <= 0f)
                {
                    Vector2 toPlayer = playerPos - enemy.Position;
                    if (toPlayer.LengthSquared() > 1f)
                    {
                        enemy.Vel = Vector2.Normalize(toPlayer) * enemy.Speed * 1.12f;
                    }
                    enemy.AbilityStep = 0;
                    enemy.AbilityTimer = 3.8f;
                    AddFlash(new Color(220, 120, 60, 255), 0.06f);
                }
            }
            else
            {
                enemy.AbilityTimer -= dt;
                if (enemy.AbilityTimer <= 0f)
                {
                    enemy.AbilityStep = 1;
                    enemy.AbilityTimer = enemy.Phase >= 1f ? BossTelegraphTime * 0.65f : BossTelegraphTime;
                }
            }
        }

        if (def.Behavior == EnemyBehavior.BossSmash)
        {
            TickGroveTitanCuts(ref enemy, dt);
        }
    }

    static void TickGroveTitanCuts(ref Enemy enemy, float dt)
    {
        if (enemy.AbilityStep == 0)
        {
            enemy.AbilityTimer -= dt;
            if (enemy.AbilityTimer > 0f) return;

            if (!TryGetTileUnder(playerPos, out int px, out int py)) return;

            enemy.StrikeTx = px;
            enemy.StrikeTy = py;
            enemy.StrikeDepth = 2 + Rng.Next(2);
            enemy.AbilityStep = 1;
            enemy.AbilityTimer = BossCutTelegraphTime;
            return;
        }

        enemy.AbilityTimer -= dt;
        if (enemy.AbilityTimer > 0f) return;

        switch (enemy.AbilityStep)
        {
            case 1:
                CollapseBossCutLane(enemy.StrikeTx, enemy.StrikeTy, 0, 1, enemy.StrikeDepth);
                enemy.AbilityStep = 2;
                enemy.AbilityTimer = BossCutTelegraphTime;
                AddFlash(Danger, 0.05f);
                break;
            case 2:
                CollapseBossCutLane(enemy.StrikeTx, enemy.StrikeTy, 0, -1, enemy.StrikeDepth);
                enemy.AbilityStep = 3;
                enemy.AbilityTimer = BossCutTelegraphTime;
                AddFlash(Danger, 0.05f);
                break;
            case 3:
                CollapseBossCutLane(enemy.StrikeTx, enemy.StrikeTy, -1, 0, enemy.StrikeDepth);
                CollapseBossCutLane(enemy.StrikeTx, enemy.StrikeTy, 1, 0, enemy.StrikeDepth);
                enemy.AbilityStep = 0;
                enemy.AbilityTimer = BossCutComboCooldown;
                AddTrauma(0.22f);
                AddFlash(Danger, 0.08f);
                break;
            default:
                enemy.AbilityStep = 0;
                enemy.AbilityTimer = BossCutComboCooldown;
                break;
        }
    }

    static void CollapseBossCutLane(int originX, int originY, int dirX, int dirY, int depth)
    {
        for (int i = 1; i <= depth; i++)
        {
            int x = originX + dirX * i;
            int y = originY + dirY * i;
            if (x < 0 || x >= GridSize || y < 0 || y >= GridSize) continue;
            if (tiles[x, y].State == 2) continue;
            CollapseTile(ref tiles[x, y]);
        }
    }

    static void DrawBossCutLaneTelegraph(int originX, int originY, int dirX, int dirY, int depth, float time, float intensity)
    {
        float pulse = MathF.Sin(time * 14f) * 0.5f + 0.5f;
        for (int i = 1; i <= depth; i++)
        {
            int x = originX + dirX * i;
            int y = originY + dirY * i;
            if (x < 0 || x >= GridSize || y < 0 || y >= GridSize) continue;
            if (tiles[x, y].State == 2) continue;
            var r = new Rectangle(x * TileSize + 2f, y * TileSize + 2f, TileSize - 4f, TileSize - 4f);
            Raylib.DrawRectangleRec(r, WithAlpha(Danger, (0.2f + intensity * 0.45f) + pulse * 0.15f));
            DrawPulseFrame(r, Danger, 0.1f, 12f, 0.2f + intensity * 0.35f);
        }
    }

    static void BlightArea(Vector2 pos, int radius)
    {
        if (!TryGetTileUnder(pos, out int bx, out int by)) return;
        for (int j = -radius; j <= radius; j++)
        {
            for (int i = -radius; i <= radius; i++)
            {
                int tx = bx + i, ty = by + j;
                if (!IsTileViable(tx, ty)) continue;
                ref Tile t = ref tiles[tx, ty];
                if (t.State == 0)
                {
                    t.State = 1;
                    t.Durability = Math.Min(t.Durability, MaxDurability * 0.38f);
                }
            }
        }
        SpawnBlightSpores(pos);
    }

    static void UpdateEnemies(float dt)
    {
        float time = (float)Raylib.GetTime();
        bool playerDashing = dashTimer > 0f || abilityIFrameTimer > 0f;

        for (int i = 0; i < enemies.Count; i++)
        {
            Enemy enemy = enemies[i];
            if (enemy.Dead) continue;

            if (enemy.Hit > 0f) enemy.Hit = Math.Max(0f, enemy.Hit - dt);

            if (enemy.ParalyzeTimer > 0f)
            {
                enemy.ParalyzeTimer = Math.Max(0f, enemy.ParalyzeTimer - dt);
                enemies[i] = enemy;
                continue;
            }

            ref readonly EnemyDef def = ref GetDef(enemy.Type);

            if (enemy.Spawn < 1f)
            {
                enemy.Spawn = Math.Min(1f, enemy.Spawn + dt / (def.Boss ? 0.65f : 0.35f));
                enemies[i] = enemy;
                continue;
            }

            TickEnemyBehavior(ref enemy, dt, time);

            Vector2 step = ComputeEnemyMove(ref enemy, dt, time);
            enemy.Vel = Vector2.Lerp(enemy.Vel, step, 1f - MathF.Exp(-12f * dt));
            float bannerMul = IsInBannerZone(enemy.Position) ? BannerEnemySlow : 1f;
            enemy.Position += enemy.Vel * dt * bannerMul;
            enemy.Position = ClampToArena(enemy.Position, enemy.Radius);

            if (TryGetTileUnder(enemy.Position, out int voidX, out int voidY) && tiles[voidX, voidY].State == 2)
            {
                enemy.Position = SnapEnemyToValidTile(enemy.Position, enemy.Radius);
            }

            float touchDistance = PlayerRadius + enemy.Radius;
            if (!playerDashing && Vector2.DistanceSquared(playerPos, enemy.Position) <= touchDistance * touchDistance)
            {
                enemies[i] = enemy;
                TriggerGameOver(DeathCause.EnemyGrasp, GetEnemyName(enemy.Type));
                return;
            }

            if (TryGetTileUnder(enemy.Position, out int tileX, out int tileY))
            {
                ref Tile tile = ref tiles[tileX, tileY];
                if (tile.State == 2)
                {
                    enemy.Position = SnapEnemyToValidTile(enemy.Position, enemy.Radius);
                    enemies[i] = enemy;
                    continue;
                }

                if (def.InstaCollapse)
                {
                    InstantlyCollapseTile(ref tile, out _);
                }
                else if (def.Behavior == EnemyBehavior.CrushTiles)
                {
                    // Tile collapse handled in TickEnemyBehavior with telegraph.
                }
                else if (def.TileDecay > 0f)
                {
                    ApplyTileDamage(ref tile, def.TileDecay * TileDecayMultiplier(tileX, tileY), dt, out _);
                }
            }

            enemies[i] = enemy;
        }
    }

    static void ReapEnemies()
    {
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            if (enemies[i].Dead)
            {
                Enemy dead = enemies[i];
                OnEnemyDeathExtra(dead);
                KillRewards(dead);
                enemies.RemoveAt(i);
            }
        }
    }

    static void OnEnemyDeathExtra(Enemy enemy)
    {
        ref readonly EnemyDef def = ref GetDef(enemy.Type);
        if (def.Behavior == EnemyBehavior.Rotburst)
        {
            BlightArea(enemy.Position, 1);
        }
        else if (def.Behavior == EnemyBehavior.Splitter)
        {
            for (int k = 0; k < 2; k++)
            {
                Vector2 off = new Vector2(Rng.Next(-24, 25), Rng.Next(-24, 25));
                enemies.Add(CreateEnemy(EnemyType.AcornSwarm, enemy.Position + off, 0f));
            }
        }
    }

    static void ApplyTileDamage(ref Tile tile, float decayRate, float dt, out bool collapsed, bool markTouched = true)
    {
        collapsed = false;
        if (tile.State == 2) return;

        if (markTouched) tile.UntouchedTimer = 0f;
        tile.Durability -= decayRate * dt;
        if (tile.Durability <= 0f)
        {
            tile.Durability = 0f;
            CollapseTile(ref tile);
            collapsed = true;
            return;
        }

        if (tile.Durability < MaxDurability * 0.6f) tile.State = 1;
    }

    static void InstantlyCollapseTile(ref Tile tile, out bool collapsed)
    {
        if (tile.State == 2) { collapsed = false; return; }
        CollapseTile(ref tile);
        collapsed = true;
    }

    static void CollapseTileAt(int x, int y, bool regrow = true)
    {
        if (x < 0 || x >= GridSize || y < 0 || y >= GridSize) return;
        CollapseTile(ref tiles[x, y], regrow, x, y);
    }

    static void KillRewards(Enemy enemy)
    {
        ref readonly EnemyDef def = ref GetDef(enemy.Type);
        Color c = def.Color;
        bool boss = def.Boss;
        bool big = boss || def.InstaCollapse || def.Radius >= 19f;

        combo++;
        comboTimer = ComboWindow;
        int gained = PointsPerEnemyKill * Math.Max(1, combo);
        if (boss) gained = (int)(gained * 2.5f);
        else if (big) gained = (int)(gained * 1.4f);
        score += gained;

        int fableGain = (int)MathF.Round((boss ? 8 : big ? 3 : 1) * FableMult());
        if (fableGain < 1) fableGain = 1;
        fables += fableGain;
        runFablesEarned += fableGain;

        int xpGain = boss ? 16 : big ? 7 : 3;
        if (combo >= 3) xpGain += 2;
        GrantXp(xpGain);

        runKillCount++;
        TryUnlockVerdictByKills();
        IncrementBestiary(enemy.Type);
        if (!def.Boss) siegeGruntsKilled++;
        SpawnComboNarration(combo);

        if (runGunAffix == GunAffixType.Leeching && TryGetTileUnder(playerPos, out int ltx, out int lty))
        {
            ref Tile lt = ref tiles[ltx, lty];
            lt.Durability = Math.Min(MaxDurability, lt.Durability + (boss ? 25f : 10f));
        }

        SpawnExplosion(enemy.Position, c, boss ? 72 : big ? 52 : 32);
        SpawnGfxLightPulse(enemy.Position, c, boss ? 320f : big ? 240f : 170f, boss ? 1.85f : 1.15f, boss ? 0.75f : 0.48f);
        SpawnFloatingText(enemy.Position, "+" + gained, combo >= 2 ? ComboColor(combo) : Color.White, combo >= 2 ? 24 : 18);
        if (boss)
        {
            SpawnFloatingText(enemy.Position + new Vector2(0, -22f), "+" + fableGain + " fables", Gold, 18);
        }

        AddTrauma(boss ? 0.95f : big ? 0.68f : 0.38f);
        zoomPunch = Math.Max(zoomPunch, boss ? 0.13f : big ? 0.085f : 0.055f);
        TriggerBossHitStop(boss);
        TriggerImpact(boss ? ImpactTier.Major : big ? ImpactTier.Medium : ImpactTier.Light, c);
    }

    static void TriggerGameOver(DeathCause cause = DeathCause.Unknown, string detail = "")
    {
        if (state == GameState.GameOver) return;
        if (TryOathOfTheBailey(cause)) return;
        if (TryAiHumanEmergencyRescue(cause, detail)) return;

        lastDeathCause = cause;
        lastDeathDetail = detail;
        state = GameState.GameOver;
        EndFloorEvent();
        AddTrauma(1f);
        zoomPunch = Math.Max(zoomPunch, 0.09f);
        impactFlashColor = Danger;
        TriggerImpact(ImpactTier.Major, Danger);
        flash = 0.9f;
        flashColor = Danger;
        SpawnExplosion(playerPos, BodyColor(), 36);

        lastRunNewBest = !IsPracticeRun() && score > highScore;
        if (!IsPracticeRun() && score > highScore) highScore = score;
        if (!IsPracticeRun()) maxWaveReached = Math.Max(maxWaveReached, waveNumber);
        UpdateDifficultyRecords(true);
        int xpGrant = (int)MathF.Round(Math.Max(8, waveNumber * 10) * OathRewardMult());
        GrantXp(xpGrant);
        UnlockMottosForLevel();
        SaveGame();
    }

}
