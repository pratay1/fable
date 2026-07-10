partial class Program
{
    // ---------------------------------------------------------------- Spawners

    static void SpawnExplosion(Vector2 origin, Color baseColor, int count)
    {
        SpawnGfxLightPulse(origin, baseColor, 90f + count * 2.8f, 0.75f + count * 0.014f, 0.32f + Math.Min(count, 50) * 0.005f);

        for (int i = 0; i < count; i++)
        {
            float angle = Rng.NextSingle() * MathF.PI * 2f;
            float speed = Rng.Next(90, 340);
            bool glow = i % 3 == 0;
            AddParticle(new Particle
            {
                Position = origin,
                Velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed,
                Color = glow ? Lighten(baseColor, 0.5f) : baseColor,
                Alpha = 1f,
                Fade = 1f / (0.42f + Rng.NextSingle() * 0.55f),
                Rotation = Rng.NextSingle() * 360f,
                Spin = (Rng.NextSingle() - 0.5f) * 420f,
                Size = glow ? 8f + Rng.NextSingle() * 7f : 3.5f + Rng.NextSingle() * 5f,
                Drag = 2.2f,
                Glow = glow,
            });
        }
    }

    static void SpawnHitSpark(Vector2 origin, Color color)
    {
        for (int i = 0; i < 12; i++)
        {
            float angle = Rng.NextSingle() * MathF.PI * 2f;
            AddParticle(new Particle
            {
                Position = origin,
                Velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * Rng.Next(70, 200),
                Color = Lighten(color, 0.35f + Rng.NextSingle() * 0.2f),
                Alpha = 1f,
                Fade = 1f / (0.22f + Rng.NextSingle() * 0.18f),
                Size = 2.5f + Rng.NextSingle() * 4f,
                Drag = 3.5f,
                Glow = i % 2 == 0,
            });
        }
    }

    static float GunStyleTrauma(GunFireStyle style) => style switch
    {
        GunFireStyle.Standard => 0.12f,
        GunFireStyle.ArcFan => 0.16f,
        GunFireStyle.Buckshot => 0.18f,
        GunFireStyle.Homing => 0.14f,
        GunFireStyle.Lance => 0.2f,
        GunFireStyle.Repeater => 0.1f,
        GunFireStyle.Volley => 0.15f,
        GunFireStyle.Burst => 0.13f,
        GunFireStyle.Laser => 0.32f,
        GunFireStyle.Sniper => 0.22f,
        GunFireStyle.RingPulse => 0.19f,
        GunFireStyle.CrossBurst => 0.17f,
        GunFireStyle.Mortar => 0.24f,
        GunFireStyle.DriftOrb => 0.15f,
        GunFireStyle.FlailArc => 0.18f,
        _ => throw new UnreachableException(),
    };

    static float ProjectileTrailChance(GunFireStyle style) => style switch
    {
        GunFireStyle.Repeater => 0.92f,
        GunFireStyle.Burst => 0.88f,
        GunFireStyle.Volley => 0.86f,
        GunFireStyle.Buckshot => 0.78f,
        GunFireStyle.Sniper => 0.72f,
        GunFireStyle.Lance => 0.8f,
        GunFireStyle.FlailArc => 0.85f,
        GunFireStyle.DriftOrb => 0.95f,
        GunFireStyle.Homing => 0.9f,
        GunFireStyle.RingPulse => 0.82f,
        GunFireStyle.CrossBurst => 0.84f,
        GunFireStyle.Mortar => 0f,
        GunFireStyle.Laser => 0f,
        _ => 0.75f,
    };

    static int GunStyleSeed(string gunName) => gunName.GetHashCode(StringComparison.Ordinal);

    static void SpawnWeaponFireFx(GunFireStyle style, Vector2 pos, Vector2 dir, Color color, string gunName)
    {
        int seed = GunStyleSeed(gunName);
        Color hot = Lighten(color, 0.42f);
        Color core = Lighten(color, 0.65f);
        float aim = MathF.Atan2(dir.Y, dir.X);

        switch (style)
        {
            case GunFireStyle.Standard:
                SpawnConeBurst(pos, dir, color, hot, 10 + seed % 4, 80f, 160f);
                SpawnGfxLightPulse(pos, hot, 48f + seed % 20, 0.55f, 0.14f);
                break;
            case GunFireStyle.ArcFan:
                for (int i = -2; i <= 2; i++)
                {
                    float a = aim + i * 0.22f;
                    var d = new Vector2(MathF.Cos(a), MathF.Sin(a));
                    SpawnConeBurst(pos, d, color, hot, 4, 60f, 120f);
                }
                SpawnGfxLightPulse(pos, hot, 72f, 0.7f, 0.16f);
                break;
            case GunFireStyle.Buckshot:
                SpawnConeBurst(pos, dir, color, hot, 18 + seed % 6, 50f, 190f);
                for (int i = 0; i < 6; i++)
                {
                    float a = aim + (Hash(seed + i) - 0.5f) * 0.9f;
                    SpawnSpark(pos, a, 90f, 220f, WithAlpha(hot, 0.85f), 3f);
                }
                break;
            case GunFireStyle.Homing:
                SpawnGfxLightPulse(pos, hot, 58f, 0.65f, 0.2f);
                for (int i = 0; i < 8; i++)
                {
                    float a = i * MathF.PI * 2f / 8f;
                    SpawnSpark(pos, a, 20f, 45f, WithAlpha(color, 0.7f), 2.5f);
                }
                SpawnConeBurst(pos, dir, color, core, 8, 70f, 130f);
                break;
            case GunFireStyle.Lance:
                SpawnLanceMuzzle(pos, dir, color, hot);
                SpawnGfxLightPulse(pos, core, 95f, 0.95f, 0.2f);
                AddFlash(hot, 0.1f);
                break;
            case GunFireStyle.Repeater:
                SpawnConeBurst(pos, dir, color, hot, 6, 90f, 150f);
                SpawnSpark(pos, aim, 120f, 180f, core, 2.5f);
                break;
            case GunFireStyle.Volley:
                SpawnConeBurst(pos, dir, color, hot, 9, 70f, 140f);
                SpawnGfxLightPulse(pos, hot, 52f, 0.5f, 0.12f);
                break;
            case GunFireStyle.Burst:
                SpawnConeBurst(pos, dir, color, hot, 12, 75f, 165f);
                SpawnGfxLightPulse(pos, core, 64f, 0.62f, 0.15f);
                break;
            case GunFireStyle.Laser:
                SpawnLaserMuzzle(pos, dir, color, hot, core);
                break;
            case GunFireStyle.Sniper:
                SpawnSniperMuzzle(pos, dir, color, hot, core);
                AddFlash(core, 0.12f);
                zoomPunch = Math.Max(zoomPunch, 0.035f);
                break;
            case GunFireStyle.RingPulse:
                for (int i = 0; i < 12; i++)
                {
                    float a = i * MathF.PI * 2f / 12f;
                    SpawnSpark(pos, a, 55f, 110f, color, 3.5f);
                }
                SpawnGfxLightPulse(pos, hot, 88f, 0.85f, 0.22f);
                break;
            case GunFireStyle.CrossBurst:
                for (int i = 0; i < 4; i++)
                {
                    float a = aim + i * MathF.PI * 0.5f;
                    SpawnSpark(pos, a, 80f, 150f, hot, 4f);
                }
                SpawnGfxLightPulse(pos, core, 76f, 0.78f, 0.18f);
                break;
            case GunFireStyle.Mortar:
                SpawnGfxLightPulse(pos, hot, 68f, 0.72f, 0.2f);
                SpawnConeBurst(pos, dir, color, hot, 14, 40f, 90f);
                break;
            case GunFireStyle.DriftOrb:
                SpawnGfxLightPulse(pos, hot, 62f, 0.8f, 0.24f);
                for (int i = 0; i < 10; i++)
                {
                    float a = i * MathF.PI * 2f / 10f + aim;
                    SpawnSpark(pos, a, 25f, 55f, WithAlpha(core, 0.75f), 3f);
                }
                break;
            case GunFireStyle.FlailArc:
                for (int i = -1; i <= 1; i++)
                {
                    float a = aim + i * 0.5f;
                    SpawnSpark(pos, a, 70f, 130f, color, 4f);
                }
                SpawnGfxLightPulse(pos, hot, 70f, 0.68f, 0.17f);
                break;
            default:
                throw new UnreachableException();
        }
    }

    static void SpawnConeBurst(Vector2 pos, Vector2 dir, Color color, Color hot, int count, float minSpd, float maxSpd)
    {
        float aim = MathF.Atan2(dir.Y, dir.X);
        for (int i = 0; i < count; i++)
        {
            float spread = (Hash(i * 991 + (int)(pos.X + pos.Y)) - 0.5f) * 0.55f;
            float a = aim + spread;
            float spd = minSpd + Rng.NextSingle() * (maxSpd - minSpd);
            AddParticle(new Particle
            {
                Position = pos,
                Velocity = new Vector2(MathF.Cos(a), MathF.Sin(a)) * spd,
                Color = i % 3 == 0 ? hot : color,
                Alpha = 1f,
                Fade = 1f / (0.14f + Rng.NextSingle() * 0.16f),
                Size = 3f + Rng.NextSingle() * 4f,
                Drag = 4.5f,
                Glow = true,
            });
        }
    }

    static void SpawnSpark(Vector2 pos, float angle, float minSpd, float maxSpd, Color color, float size)
    {
        float spd = minSpd + Rng.NextSingle() * (maxSpd - minSpd);
        AddParticle(new Particle
        {
            Position = pos,
            Velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * spd,
            Color = color,
            Alpha = 1f,
            Fade = 1f / 0.2f,
            Size = size,
            Drag = 4f,
            Glow = true,
        });
    }

    static void SpawnLanceMuzzle(Vector2 pos, Vector2 dir, Color color, Color hot)
    {
        float aim = MathF.Atan2(dir.Y, dir.X);
        for (int i = 0; i < 16; i++)
        {
            float t = i / 15f;
            SpawnSpark(pos + dir * (8f + t * 22f), aim, 40f + t * 80f, 90f + t * 120f, i % 2 == 0 ? hot : color, 2.5f + t * 2f);
        }
    }

    static void SpawnSniperMuzzle(Vector2 pos, Vector2 dir, Color color, Color hot, Color core)
    {
        float aim = MathF.Atan2(dir.Y, dir.X);
        SpawnSpark(pos, aim, 200f, 320f, core, 2f);
        SpawnSpark(pos, aim, 140f, 200f, hot, 3f);
        SpawnGfxLightPulse(pos, hot, 110f, 1.1f, 0.18f);
        for (int i = 0; i < 5; i++)
        {
            float a = aim + (i - 2) * 0.08f;
            SpawnSpark(pos, a, 60f, 100f, WithAlpha(color, 0.6f), 2f);
        }
    }

    static void SpawnLaserMuzzle(Vector2 pos, Vector2 dir, Color color, Color hot, Color core)
    {
        SpawnGfxLightPulse(pos, core, 120f, 1.4f, 0.24f);
        SpawnConeBurst(pos, dir, color, hot, 14, 30f, 110f);
    }

    static void SpawnLaserBeamFx(Vector2 start, Vector2 end, Color color)
    {
        Vector2 seg = end - start;
        float len = seg.Length();
        if (len < 1f) return;

        int sparks = (int)(len / 28f);
        for (int i = 0; i < sparks; i++)
        {
            float t = (i + Rng.NextSingle() * 0.4f) / sparks;
            Vector2 p = start + seg * t;
            SpawnGfxLightPulse(p, Lighten(color, 0.35f), 24f, 0.35f, 0.08f);
        }
        SpawnGfxLightPulse(end, Lighten(color, 0.5f), 64f, 0.9f, 0.16f);
    }

    static void SpawnProjectileBirthFx(GunFireStyle style, Vector2 pos, Vector2 vel, Color color)
    {
        switch (style)
        {
            case GunFireStyle.Mortar:
                SpawnGfxLightPulse(pos, Lighten(color, 0.3f), 36f, 0.45f, 0.12f);
                break;
            case GunFireStyle.RingPulse:
            case GunFireStyle.CrossBurst:
                SpawnGfxLightPulse(pos, color, 28f, 0.4f, 0.1f);
                break;
            case GunFireStyle.Sniper:
            case GunFireStyle.Lance:
                SpawnSpark(pos, MathF.Atan2(vel.Y, vel.X), 30f, 60f, Lighten(color, 0.5f), 2f);
                break;
            default:
                break;
        }
    }

    static void SpawnProjectileTrailFx(Projectile p)
    {
        Color trail = WithAlpha(Lighten(p.Color, 0.25f), 0.75f);
        switch (p.Style)
        {
            case GunFireStyle.Buckshot:
                trail = WithAlpha(p.Color, 0.55f);
                AddParticle(new Particle
                {
                    Position = p.Position,
                    Velocity = -p.Velocity * 0.08f,
                    Color = trail,
                    Alpha = 0.8f,
                    Fade = 1f / 0.18f,
                    Size = p.Size * 0.55f,
                    Drag = 6f,
                    Glow = false,
                });
                break;
            case GunFireStyle.Homing:
            case GunFireStyle.DriftOrb:
                AddParticle(new Particle
                {
                    Position = p.Position,
                    Velocity = Vector2.Zero,
                    Color = trail,
                    Alpha = 0.85f,
                    Fade = 1f / 0.35f,
                    Size = p.Size * (p.Style == GunFireStyle.DriftOrb ? 1.1f : 0.85f),
                    Drag = 3f,
                    Glow = true,
                });
                break;
            case GunFireStyle.FlailArc:
                AddParticle(new Particle
                {
                    Position = p.Position,
                    Velocity = new Vector2(-p.Velocity.Y, p.Velocity.X) * 0.04f,
                    Color = trail,
                    Alpha = 0.7f,
                    Fade = 1f / 0.2f,
                    Size = p.Size * 0.7f,
                    Drag = 5f,
                    Glow = true,
                });
                break;
            default:
                AddParticle(new Particle
                {
                    Position = p.Position,
                    Velocity = Vector2.Zero,
                    Color = p.Color,
                    Alpha = 0.65f,
                    Fade = 1f / 0.22f,
                    Size = p.Size * 0.85f,
                    Drag = 4f,
                    Glow = true,
                });
                break;
        }
    }

    static void SpawnProjectileHitFx(GunFireStyle style, Vector2 pos, Vector2 vel, Color color, float damage)
    {
        SpawnHitSpark(pos, color);
        switch (style)
        {
            case GunFireStyle.Sniper:
                SpawnGfxLightPulse(pos, Lighten(color, 0.5f), 80f, 1f, 0.2f);
                for (int i = 0; i < 6; i++)
                {
                    float a = MathF.Atan2(vel.Y, vel.X) + MathF.PI + (i - 2.5f) * 0.25f;
                    SpawnSpark(pos, a, 100f, 200f, Lighten(color, 0.4f), 2f);
                }
                break;
            case GunFireStyle.Lance:
                SpawnGfxLightPulse(pos, color, 70f, 0.85f, 0.18f);
                break;
            case GunFireStyle.Buckshot:
                SpawnConeBurst(pos, SafeNormalize(vel), color, Lighten(color, 0.4f), 6, 40f, 100f);
                break;
            case GunFireStyle.FlailArc:
                SpawnConeBurst(pos, SafeNormalize(vel), color, Lighten(color, 0.35f), 8, 50f, 130f);
                break;
            case GunFireStyle.Homing:
            case GunFireStyle.DriftOrb:
                SpawnGfxLightPulse(pos, Lighten(color, 0.45f), 55f, 0.7f, 0.16f);
                break;
            case GunFireStyle.RingPulse:
            case GunFireStyle.CrossBurst:
                SpawnGfxLightPulse(pos, color, 48f, 0.65f, 0.14f);
                break;
            default:
                if (damage >= 18f)
                {
                    SpawnGfxLightPulse(pos, color, 42f, 0.5f, 0.12f);
                }
                break;
        }
    }

    static void SpawnMuzzle(Vector2 pos, Color color)
    {
        SpawnWeaponFireFx(GunFireStyle.Standard, pos, Vector2.UnitX, color, "fallback");
    }

    static void SpawnDashBurst()
    {
        for (int i = 0; i < 28; i++)
        {
            float angle = Rng.NextSingle() * MathF.PI * 2f;
            float speed = Rng.Next(60, 220);
            AddParticle(new Particle
            {
                Position = playerPos,
                Velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed - lastMoveDirection * 160f,
                Color = BodyColor(),
                Alpha = 1f,
                Fade = 1f / 0.5f,
                Size = 4f + Rng.NextSingle() * 6f,
                Drag = 2.8f,
                Glow = true,
            });
        }
        SpawnGfxLightPulse(playerPos, BodyBright(), 90f, 0.85f, 0.2f);
        AddTrauma(0.18f);
        zoomPunch = Math.Max(zoomPunch, 0.04f);
    }

    static AbilityType SanitizeAbilitySlot(AbilityType slot) =>
        abilityUnlocked[(int)slot] ? slot : AbilityType.Paralyze;

    static void AssignAbilityToSlot(int slot, AbilityType ability)
    {
        if (!abilityUnlocked[(int)ability]) return;

        ref AbilityType target = ref slot == 0 ? ref abilitySlot1 : ref abilitySlot2;
        ref AbilityType other = ref slot == 0 ? ref abilitySlot2 : ref abilitySlot1;
        if (target == ability) return;

        if (other == ability) other = target;
        target = ability;
        abilitySlotTracked[slot] = ability;
        abilityFillVis[slot] = AbilityReadiness(ability);
        AddFlash(AbilityAccent(ability), 0.14f);
        SaveGame();
    }

    static void SyncAbilitySlotHud(int slot)
    {
        AbilityType equipped = slot == 0 ? abilitySlot1 : abilitySlot2;
        if (abilitySlotTracked[slot] == equipped) return;
        abilitySlotTracked[slot] = equipped;
        abilityFillVis[slot] = AbilityReadiness(equipped);
    }

    static void TryUseAbilityFromSlots(AbilityType ability, bool blockDash)
    {
        if (abilitySlot1 == ability) TryUseAbility(ability, blockDash);
        else if (abilitySlot2 == ability) TryUseAbility(ability, false);
    }

    static bool HasEquippedAbility(AbilityType ability) =>
        abilitySlot1 == ability || abilitySlot2 == ability;

    static bool IsInBannerZone(Vector2 pos) =>
        bannerActive && bannerTimer > 0f && Vector2.Distance(pos, bannerPos) <= BannerRadius;

    static bool BannerFreezesEvents() =>
        bannerActive && bannerTimer > 0f && IsInBannerZone(playerPos);

    static bool EventsHaltedByVerdict() => verdictHaltTimer > 0f;

    static float AbilityReadiness(AbilityType ability)
    {
        return ability switch
        {
            AbilityType.Paralyze => paralyzeCooldown <= 0f ? 1f : 1f - paralyzeCooldown / EffParalyzeCooldown(),
            AbilityType.WindStep => dashCooldown <= 0f && dashTimer <= 0f && bannerPlantTimer <= 0f
                ? 1f : 1f - dashCooldown / EffDashCooldown(),
            AbilityType.OathOfTheBailey => oathUsedThisRun ? 0f : 1f,
            AbilityType.Verdict => verdictCooldown <= 0f && verdictHaltTimer <= 0f && abilityUnlocked[(int)AbilityType.Verdict]
                ? 1f : verdictCooldown > 0f ? 1f - verdictCooldown / VerdictCooldown : 0.15f,
            AbilityType.BannerOfStillness => bannerCooldown <= 0f && bannerPlantTimer <= 0f && !bannerActive
                ? 1f : 1f - bannerCooldown / BannerCooldown,
            _ => 0f,
        };
    }

    static bool IsVerdictUnlocked() => abilityUnlocked[(int)AbilityType.Verdict];

    static void TryUnlockVerdictByKills()
    {
        if (IsVerdictUnlocked() || runKillCount < VerdictUnlockKills) return;
        abilityUnlocked[(int)AbilityType.Verdict] = true;
        if (abilitySlot1 != AbilityType.Verdict && abilitySlot2 != AbilityType.Verdict)
        {
            AssignAbilityToSlot(1, AbilityType.Verdict);
        }
        SaveGame();
        SpawnFloatingText(playerPos + new Vector2(0, -52f), "VERDICT UNLOCKED", AbilityAccent(AbilityType.Verdict), 24);
        SpawnFloatingText(playerPos + new Vector2(0, -28f), "30 FOES JUDGED", Gold, 16);
        AddFlash(AbilityAccent(AbilityType.Verdict), 0.22f);
        TriggerImpact(ImpactTier.Medium, AbilityAccent(AbilityType.Verdict));
    }

    static Color AbilityAccent(AbilityType ability) => ability switch
    {
        AbilityType.Paralyze => new Color(168, 188, 212, 255),
        AbilityType.WindStep => BodyBright(),
        AbilityType.OathOfTheBailey => new Color(196, 176, 120, 255),
        AbilityType.Verdict => new Color(214, 188, 96, 255),
        AbilityType.BannerOfStillness => new Color(148, 196, 168, 255),
        _ => UiAccent,
    };

    static bool TryUnlockAbility(AbilityType ability)
    {
        int i = (int)ability;
        if (abilityUnlocked[i]) return true;
        if (AbilityFableCost[i] < 0) return false;
        int cost = AbilityFableCost[i];
        if (fables < cost) return false;
        fables -= cost;
        abilityUnlocked[i] = true;
        SaveGame();
        AddFlash(Gold, 0.22f);
        return true;
    }

    static void PollAbilityHotkeys()
    {
        if (state != GameState.Playing) return;

        if (Raylib.IsKeyPressed(abilityKey1)) TryUseAbility(abilitySlot1, blockDash: true);
        if (Raylib.IsKeyPressed(abilityKey2)) TryUseAbility(abilitySlot2, blockDash: false);
    }

    static void UpdateAbilities(float dt)
    {
        dashBlockedThisFrame = false;
        paralyzeCooldown = Math.Max(0f, paralyzeCooldown - dt);
        verdictCooldown = Math.Max(0f, verdictCooldown - dt);
        bannerCooldown = Math.Max(0f, bannerCooldown - dt);
        abilityIFrameTimer = Math.Max(0f, abilityIFrameTimer - dt);
        oathRescueFlashTimer = Math.Max(0f, oathRescueFlashTimer - dt);
        verdictWaveTimer = Math.Max(0f, verdictWaveTimer - dt);
        if (paralyzeBurstTimer > 0f) paralyzeBurstTimer -= dt;

        if (bannerPlantTimer > 0f)
        {
            bannerPlantTimer -= dt;
            if (bannerPlantTimer <= 0f) FinishBannerPlant();
        }

        if (bannerActive)
        {
            bannerTimer -= dt;
            if (bannerTimer <= 0f) bannerActive = false;
        }

        if (state != GameState.Playing) return;

        SyncAbilitySlotHud(0);
        SyncAbilitySlotHud(1);

        if (aiPilotEnabled)
        {
            if (aiParalyzeRequest) TryUseAbilityFromSlots(AbilityType.Paralyze, true);
            if (aiDashRequest && !dashBlockedThisFrame)
            {
                TryUseAbilityFromSlots(AbilityType.WindStep, true);
            }
            if (aiBannerRequest && !dashBlockedThisFrame)
            {
                TryUseAbilityFromSlots(AbilityType.BannerOfStillness, false);
            }
            if (aiVerdictRequest && !dashBlockedThisFrame)
            {
                TryUseAbilityFromSlots(AbilityType.Verdict, false);
            }
        }

        if (windDashVfxTimer > 0f) windDashVfxTimer = Math.Max(0f, windDashVfxTimer - dt);
        if (oathTilePulseTimer > 0f) oathTilePulseTimer = Math.Max(0f, oathTilePulseTimer - dt);
    }

    static void TryUseAbility(AbilityType ability, bool blockDash)
    {
        switch (ability)
        {
            case AbilityType.Paralyze:
                if (paralyzeCooldown <= 0f)
                {
                    TriggerParalyze(playerPos);
                    if (blockDash) dashBlockedThisFrame = true;
                }
                break;
            case AbilityType.WindStep:
                TryWindStep();
                break;
            case AbilityType.OathOfTheBailey:
                if (!oathUsedThisRun && HasEquippedAbility(AbilityType.OathOfTheBailey))
                {
                    SpawnFloatingText(playerPos + new Vector2(0, -30f), "OATH STANDS", AbilityAccent(ability), 18);
                    oathRescueFlashTimer = 0.35f;
                }
                break;
            case AbilityType.Verdict:
                TryVerdict();
                break;
            case AbilityType.BannerOfStillness:
                TryPlantBanner();
                break;
            default:
                throw new UnreachableException();
        }
    }

    static void TryWindStep()
    {
        if (dashCooldown > 0f || dashTimer > 0f || bannerPlantTimer > 0f) return;
        if (lastMoveDirection == Vector2.Zero) return;
        dashTimer = DashDuration;
        windDashVfxTimer = DashDuration + 0.12f;
        playerVel = lastMoveDirection * DashSpeed;
        SpawnWindStepBurst();
        AddTrauma(0.16f);
        TriggerImpact(ImpactTier.Light, BodyBright());
    }

    static void SpawnWindStepBurst()
    {
        Color wind = new Color(196, 228, 238, 255);
        Color body = BodyBright();
        for (int i = 0; i < 34; i++)
        {
            float angle = Rng.NextSingle() * MathF.PI * 2f;
            float speed = Rng.Next(80, 280);
            Vector2 back = lastMoveDirection == Vector2.Zero ? Vector2.Zero : -SafeNormalize(lastMoveDirection);
            AddParticle(new Particle
            {
                Position = playerPos + back * PlayerRadius * 0.5f,
                Velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed + back * 120f,
                Color = i % 3 == 0 ? wind : body,
                Alpha = 1f,
                Fade = 1f / (0.35f + Rng.NextSingle() * 0.35f),
                Size = 4f + Rng.NextSingle() * 7f,
                Drag = 2.4f,
                Glow = i % 2 == 0,
            });
        }
        for (int streak = 0; streak < 5; streak++)
        {
            Vector2 off = new Vector2(-lastMoveDirection.Y, lastMoveDirection.X) * (streak - 2f) * 8f;
            AddParticle(new Particle
            {
                Position = playerPos + off,
                Velocity = lastMoveDirection * 420f + off * 2f,
                Color = wind,
                Alpha = 0.9f,
                Fade = 1f / 0.22f,
                Size = 3f + streak,
                Drag = 1.2f,
                Glow = true,
            });
        }
        SpawnGfxLightPulse(playerPos, wind, 110f, 0.95f, 0.22f);
        zoomPunch = Math.Max(zoomPunch, 0.045f);
    }

    static void TryPlantBanner()
    {
        if (bannerCooldown > 0f || bannerPlantTimer > 0f || bannerActive) return;
        bannerPlantTimer = BannerPlantTime;
        bannerPos = playerPos;
        playerVel *= 0.2f;
        SpawnBannerPlantVfx(bannerPos);
        AddTrauma(0.14f);
    }

    static void FinishBannerPlant()
    {
        bannerActive = true;
        bannerTimer = EffBannerDuration();
        bannerCooldown = BannerCooldown;
        SpawnFloatingText(bannerPos + new Vector2(0, -36f), "STILLNESS", AbilityAccent(AbilityType.BannerOfStillness), 22);
        SpawnEventShockwave(bannerPos, AbilityAccent(AbilityType.BannerOfStillness), BannerRadius * 1.35f, 0.75f);
        AddTrauma(0.22f);
        TriggerImpact(ImpactTier.Medium, AbilityAccent(AbilityType.BannerOfStillness));
    }

    static void TryVerdict()
    {
        if (verdictCooldown > 0f || verdictHaltTimer > 0f) return;
        if (!HasEquippedAbility(AbilityType.Verdict)) return;
        if (OathBlocksVerdict())
        {
            SpawnFloatingText(playerPos + new Vector2(0, -34f), "OATH FORBIDS VERDICT", Danger, 16);
            return;
        }

        if (!IsVerdictUnlocked())
        {
            if (runKillCount < VerdictUnlockKills)
            {
                SpawnFloatingText(playerPos + new Vector2(0, -34f), runKillCount + "/" + VerdictUnlockKills + " KILLS", Gold, 15);
                return;
            }

            abilityUnlocked[(int)AbilityType.Verdict] = true;
            SaveGame();
        }

        verdictCooldown = VerdictCooldown;
        verdictHaltTimer = VerdictHaltDuration;
        abilityIFrameTimer = VerdictIFrameTime;
        verdictWaveOrigin = playerPos;
        verdictWaveTimer = VerdictWaveTime;

        EndFloorEvent(fromVerdict: true);
        floorRotTimer = 0f;
        floorRotSide = 0f;
        nextFloorEventTimer = 0f;

        SpawnVerdictVfx(playerPos);
        SpawnFloatingText(playerPos + new Vector2(0, -40f), "VERDICT", AbilityAccent(AbilityType.Verdict), 24);
        SpawnFloatingText(playerPos + new Vector2(0, -18f), "EVENTS HALTED 30s", Color.White, 15);
        AddTrauma(0.42f);
        zoomPunch = Math.Max(zoomPunch, 0.07f);
        TriggerImpact(ImpactTier.Major, AbilityAccent(AbilityType.Verdict));
    }

    static bool TryOathOfTheBailey(DeathCause cause)
    {
        if (cause != DeathCause.FellThrough && cause != DeathCause.FloorGaveWay) return false;
        if (OathBlocksOath() || !HasEquippedAbility(AbilityType.OathOfTheBailey) || oathUsedThisRun) return false;
        if (!TryFindNearestSolidTile(playerPos, out Vector2 snap, out int tx, out int ty)) return false;

        oathUsedThisRun = true;
        playerPos = snap;
        playerVel = Vector2.Zero;
        abilityIFrameTimer = OathIFrameTime;
        oathRescueFlashTimer = OathRescueFlashTime;

        ref Tile tile = ref tiles[tx, ty];
        tile.State = 0;
        tile.Durability = MaxDurability;
        tile.Collapse = 0f;
        tile.EventMarked = false;
        tile.RegrowTimer = 0f;
        oathReinforcedTx = tx;
        oathReinforcedTy = ty;
        oathTilePulseTimer = 3.2f;

        SpawnOathRescueVfx(snap, tx, ty);
        SpawnFloatingText(snap + new Vector2(0, -44f), "OATH OF THE BAILEY", AbilityAccent(AbilityType.OathOfTheBailey), 22);
        SpawnFloatingText(snap + new Vector2(0, -20f), "STONE HOLDS", Color.White, 16);
        AddTrauma(0.55f);
        zoomPunch = Math.Max(zoomPunch, 0.09f);
        TriggerBossHitStop(true);
        TriggerImpact(ImpactTier.Major, HeraldryAccent(AbilityAccent(AbilityType.OathOfTheBailey)));
        return true;
    }

    static bool TryFindNearestSolidTile(Vector2 from, out Vector2 snapPos, out int tileX, out int tileY)
    {
        snapPos = from;
        tileX = tileY = 0;
        float best = float.MaxValue;
        bool found = false;
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                ref Tile t = ref tiles[x, y];
                if (t.State == 2 || t.EventMarked) continue;
                if (t.Durability < MaxDurability * 0.12f) continue;
                Vector2 c = TileCenter(x, y);
                float d = Vector2.DistanceSquared(from, c);
                if (d >= best) continue;
                best = d;
                snapPos = c;
                tileX = x;
                tileY = y;
                found = true;
            }
        }
        return found;
    }

    static void SpawnBannerPlantVfx(Vector2 origin)
    {
        Color banner = AbilityAccent(AbilityType.BannerOfStillness);
        for (int i = 0; i < 22; i++)
        {
            float angle = i * (MathF.PI * 2f / 22f);
            AddParticle(new Particle
            {
                Position = origin,
                Velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * Rng.Next(60, 180),
                Color = i % 2 == 0 ? banner : Color.White,
                Alpha = 1f,
                Fade = 1f / 0.45f,
                Size = 4f + Rng.NextSingle() * 4f,
                Drag = 3.2f,
                Glow = i % 3 == 0,
            });
        }
        SpawnGfxLightPulse(origin, banner, 70f, 0.7f, 0.18f);
    }

    static void SpawnVerdictVfx(Vector2 origin)
    {
        Color gold = AbilityAccent(AbilityType.Verdict);
        for (int ring = 0; ring < 16; ring++)
        {
            float angle = ring * (MathF.PI * 2f / 16f);
            AddParticle(new Particle
            {
                Position = origin,
                Velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 420f,
                Color = gold,
                Alpha = 1f,
                Fade = 1f / 0.55f,
                Size = 6f + Rng.NextSingle() * 5f,
                Drag = 2.4f,
                Glow = true,
            });
        }
        SpawnEventShockwave(origin, gold, 240f, 0.9f);
        SpawnGfxLightPulse(origin, Color.White, 160f, 1.1f, 0.22f);
    }

    static void SpawnOathRescueVfx(Vector2 origin, int tx, int ty)
    {
        Color gold = AbilityAccent(AbilityType.OathOfTheBailey);
        SpawnEventShockwave(origin, gold, 200f, 1f);
        SpawnGfxLightPulse(origin, Color.White, 150f, 1.2f, 0.28f);
        for (int i = 0; i < 36; i++)
        {
            float angle = i * (MathF.PI * 2f / 36f);
            AddParticle(new Particle
            {
                Position = origin + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 18f,
                Velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * Rng.Next(90, 260),
                Color = i % 3 == 0 ? gold : new Color(148, 140, 128, 255),
                Alpha = 1f,
                Fade = 1f / (0.5f + Rng.NextSingle() * 0.35f),
                Size = 5f + Rng.NextSingle() * 6f,
                Drag = 2.6f,
                Glow = i % 2 == 0,
            });
        }
        SpawnRegrowBurst(TileCenter(tx, ty));
    }

    static void TriggerParalyze(Vector2 origin)
    {
        float radius = EffParalyzeRadius();
        float duration = EffParalyzeDuration();
        int hit = 0;

        for (int i = 0; i < enemies.Count; i++)
        {
            Enemy enemy = enemies[i];
            if (enemy.Dead || enemy.Spawn < 0.5f) continue;
            if (Vector2.Distance(origin, enemy.Position) > radius + enemy.Radius) continue;

            ref readonly EnemyDef def = ref GetDef(enemy.Type);
            float hold = def.Boss ? duration * 0.55f : duration;
            enemy.ParalyzeTimer = Math.Max(enemy.ParalyzeTimer, hold);
            enemies[i] = enemy;
            hit++;
        }

        paralyzeCooldown = EffParalyzeCooldown();
        paralyzeBurstTimer = ParalyzeBurstTime;
        paralyzeBurstOrigin = origin;
        paralyzeBurstRadius = radius;

        if (aiPilotEnabled)
        {
            aiPostParalyzeGrace = MathF.Max(EffParalyzeDuration() * 0.9f, 1.2f);
        }

        SpawnParalyzeVfx(origin, radius, hit);
        SpawnFloatingText(origin + new Vector2(0, -34f), hit > 0 ? "PARALYZE" : "PARALYZE", new Color(196, 214, 232, 255), 20);
        AddTrauma(hit > 0 ? 0.28f : 0.12f);
        zoomPunch = Math.Max(zoomPunch, 0.045f);
        TriggerImpact(hit > 0 ? ImpactTier.Medium : ImpactTier.Light, new Color(168, 188, 210, 255));
    }

    static void SpawnParalyzeVfx(Vector2 origin, float radius, int hits)
    {
        Color core = new Color(214, 226, 238, 255);
        Color arc = new Color(148, 168, 196, 255);
        int burst = 28 + hits * 4;

        for (int i = 0; i < burst; i++)
        {
            float angle = Rng.NextSingle() * MathF.PI * 2f;
            float dist = radius * (0.25f + Rng.NextSingle() * 0.75f);
            Vector2 pos = origin + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * dist;
            Vector2 vel = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * Rng.Next(80, 240);
            bool glow = i % 3 == 0;
            AddParticle(new Particle
            {
                Position = pos,
                Velocity = vel,
                Color = glow ? core : arc,
                Alpha = 1f,
                Fade = 1f / (0.35f + Rng.NextSingle() * 0.45f),
                Rotation = Rng.NextSingle() * 360f,
                Spin = (Rng.NextSingle() - 0.5f) * 420f,
                Size = glow ? 6f + Rng.NextSingle() * 5f : 3f + Rng.NextSingle() * 3f,
                Drag = 2.8f,
                Glow = glow,
            });
        }

        for (int ring = 0; ring < 12; ring++)
        {
            float angle = ring * (MathF.PI * 2f / 12f);
            AddParticle(new Particle
            {
                Position = origin + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius * 0.92f,
                Velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 60f,
                Color = core,
                Alpha = 0.95f,
                Fade = 1f / 0.55f,
                Size = 5f,
                Drag = 4.5f,
                Glow = true,
            });
        }
    }

    static void DrawParalyzeBurst()
    {
        if (paralyzeBurstTimer <= 0f) return;

        float t = 1f - paralyzeBurstTimer / ParalyzeBurstTime;
        float expand = paralyzeBurstRadius * (0.35f + t * 1.15f);
        float fade = 1f - t;
        Color ring = new Color(186, 204, 224, 255);
        Color core = new Color(228, 236, 244, 255);
        Color arc = new Color(132, 168, 214, 255);
        float time = (float)Raylib.GetTime();

        DrawGlow(paralyzeBurstOrigin, expand * 0.55f, core, 0.08f * fade);
        DrawGlowFast(paralyzeBurstOrigin, expand * 0.35f, arc, 0.06f * fade);
        Raylib.DrawRing(paralyzeBurstOrigin, expand * 0.82f, expand, 0f, 360f, 64, WithAlpha(ring, 0.55f * fade));
        Raylib.DrawRing(paralyzeBurstOrigin, expand * 0.55f, expand * 0.72f, 0f, 360f, 48, WithAlpha(Color.White, 0.35f * fade));
        Raylib.DrawCircleV(paralyzeBurstOrigin, 10f + t * 18f, WithAlpha(core, 0.25f * fade));

        for (int i = 0; i < 8; i++)
        {
            float a = time * 9f + i * 1.047f;
            Vector2 end = paralyzeBurstOrigin + new Vector2(MathF.Cos(a), MathF.Sin(a)) * expand * 0.9f;
            Raylib.DrawLineEx(paralyzeBurstOrigin, end, 2.5f, WithAlpha(ring, 0.45f * fade));
            Raylib.DrawCircleV(end, 4f, WithAlpha(core, 0.35f * fade));
        }

        for (int bolt = 0; bolt < 4; bolt++)
        {
            float a0 = time * 11f + bolt * 1.57f;
            Vector2 p0 = paralyzeBurstOrigin + new Vector2(MathF.Cos(a0), MathF.Sin(a0)) * expand * 0.35f;
            Vector2 p1 = paralyzeBurstOrigin + new Vector2(MathF.Cos(a0 + 0.4f), MathF.Sin(a0 + 0.4f)) * expand * 0.75f;
            Vector2 mid = (p0 + p1) * 0.5f + new Vector2(MathF.Cos(a0 + 1.2f), MathF.Sin(a0 + 1.2f)) * 18f;
            Raylib.DrawLineEx(p0, mid, 2f, WithAlpha(Color.White, 0.55f * fade));
            Raylib.DrawLineEx(mid, p1, 2f, WithAlpha(arc, 0.65f * fade));
        }
    }

    static void DrawWindStepAura()
    {
        if (windDashVfxTimer <= 0f && dashTimer <= 0f) return;
        if (!HasEquippedAbility(AbilityType.WindStep)) return;

        float time = (float)Raylib.GetTime();
        Color wind = new Color(186, 224, 236, 255);
        float intensity = dashTimer > 0f ? 1f : windDashVfxTimer / 0.12f;
        Vector2 dir = lastMoveDirection == Vector2.Zero ? Vector2.UnitY : SafeNormalize(lastMoveDirection);
        Vector2 perp = new Vector2(-dir.Y, dir.X);

        for (int i = 0; i < 4; i++)
        {
            float along = -PlayerRadius - i * 14f - MathF.Sin(time * 18f + i) * 4f;
            Vector2 p = playerPos + dir * along + perp * MathF.Sin(time * 12f + i * 1.3f) * 6f;
            float alpha = (0.35f - i * 0.06f) * intensity;
            Raylib.DrawCircleV(p, 7f - i, WithAlpha(wind, alpha));
            DrawGlow(p, 16f, wind, alpha * 0.12f);
        }

        Raylib.DrawCircleLinesV(playerPos, PlayerRadius + 6f + MathF.Sin(time * 14f) * 2f, WithAlpha(wind, 0.45f * intensity));
    }

    static void DrawBannerOfStillness()
    {
        if (!bannerActive && bannerPlantTimer <= 0f) return;
        float time = (float)Raylib.GetTime();
        Color banner = HeraldryAccent(AbilityAccent(AbilityType.BannerOfStillness));
        Color rune = new Color(214, 232, 220, 255);
        Vector2 pos = bannerPos;
        float pulse = MathF.Sin(time * 3.2f) * 0.5f + 0.5f;

        if (bannerPlantTimer > 0f)
        {
            float plantT = 1f - bannerPlantTimer / BannerPlantTime;
            float shaftH = 18f + plantT * 42f;
            Vector2 basePos = pos + new Vector2(0, 8f);
            Vector2 topPos = pos + new Vector2(0, -shaftH);
            Raylib.DrawCircleV(basePos, 10f + plantT * 8f, WithAlpha(banner, 0.2f * plantT));
            Raylib.DrawLineEx(basePos, topPos, 4f, new Color(92, 78, 58, 255));
            Raylib.DrawLineEx(basePos, topPos, 2f, WithAlpha(banner, 0.55f * plantT));
            Raylib.DrawCircleV(topPos, 6f + plantT * 4f, WithAlpha(Color.White, 0.5f + plantT * 0.35f));
            DrawGlow(pos, 24f + plantT * 28f, banner, 0.08f * plantT);
            return;
        }

        float radius = BannerRadius * (0.92f + pulse * 0.04f);
        DrawGlowFast(pos, radius * 0.55f, banner, 0.06f + pulse * 0.04f);
        Raylib.DrawRing(pos, radius * 0.78f, radius, 0f, 360f, 72, WithAlpha(banner, 0.24f + pulse * 0.1f));
        Raylib.DrawRing(pos, radius * 0.42f, radius * 0.58f, 0f, 360f, 48, WithAlpha(Color.White, 0.1f + pulse * 0.06f));

        for (int i = 0; i < 12; i++)
        {
            float a = i * (MathF.PI * 2f / 12f) + time * 0.25f;
            Vector2 rim = pos + new Vector2(MathF.Cos(a), MathF.Sin(a)) * radius * 0.9f;
            Raylib.DrawCircleV(rim, 3f, WithAlpha(rune, 0.25f + pulse * 0.2f));
        }

        float shaft = 58f;
        Vector2 flagPoleBase = pos + new Vector2(0, 10f);
        Vector2 flagTop = pos + new Vector2(0, -shaft);
        Raylib.DrawLineEx(flagPoleBase, flagTop, 5f, new Color(72, 62, 48, 255));
        Raylib.DrawLineEx(flagPoleBase, flagTop, 2f, WithAlpha(new Color(148, 132, 108, 255), 0.8f));
        Vector2 flagMid = flagTop + new Vector2(30f + MathF.Sin(time * 4f) * 5f, 12f);
        Vector2 flagBot = flagTop + new Vector2(26f + MathF.Sin(time * 4f + 0.8f) * 4f, 30f);
        Raylib.DrawTriangle(flagTop, flagMid, flagBot, WithAlpha(HeraldryAccent(banner, 0.15f), 0.92f));
        Raylib.DrawTriangle(flagTop, flagMid, flagBot, WithAlpha(Color.White, 0.08f));
        Raylib.DrawLineEx(flagTop, flagMid, 1.5f, WithAlpha(Color.White, 0.5f));
        Raylib.DrawLineEx(flagMid, flagBot, 1.5f, WithAlpha(Color.White, 0.28f));
        Raylib.DrawCircleV(flagTop + new Vector2(8f, 14f), 4f, WithAlpha(rune, 0.65f));

        for (int i = 0; i < 10; i++)
        {
            float a = time * 0.65f + i * (MathF.PI * 2f / 10f);
            Vector2 mote = pos + new Vector2(MathF.Cos(a), MathF.Sin(a)) * radius * (0.3f + Hash(i) * 0.5f);
            Raylib.DrawCircleV(mote, 2.5f, WithAlpha(Color.White, 0.12f + pulse * 0.22f));
        }

        if (IsInBannerZone(playerPos))
        {
            DrawGlow(playerPos, PlayerRadius + 22f, banner, 0.05f + pulse * 0.04f);
            Raylib.DrawCircleLinesV(playerPos, PlayerRadius + 10f + pulse * 4f, WithAlpha(banner, 0.35f));
        }
    }

    static void DrawOathRescueFlash()
    {
        if (oathRescueFlashTimer <= 0f) return;
        float t = 1f - oathRescueFlashTimer / OathRescueFlashTime;
        float fade = 1f - t;
        Color gold = AbilityAccent(AbilityType.OathOfTheBailey);
        Color stone = new Color(148, 140, 128, 255);
        float ring = 40f + t * 240f;
        DrawGlow(playerPos, ring * 0.45f, gold, 0.14f * fade);
        Raylib.DrawRing(playerPos, ring * 0.72f, ring, 0f, 360f, 64, WithAlpha(gold, 0.58f * fade));
        Raylib.DrawRing(playerPos, ring * 0.35f, ring * 0.55f, 0f, 360f, 48, WithAlpha(Color.White, 0.38f * fade));

        for (int i = 0; i < 8; i++)
        {
            float a = i * (MathF.PI / 4f) + t * 2.4f;
            Vector2 p = playerPos + new Vector2(MathF.Cos(a), MathF.Sin(a)) * (24f + t * 42f);
            Raylib.DrawRectanglePro(
                new Rectangle(p.X, p.Y, 10f, 6f),
                new Vector2(5f, 3f), a * (180f / MathF.PI), WithAlpha(stone, 0.55f * fade));
        }

        Raylib.DrawCircleV(playerPos, 14f + t * 8f, WithAlpha(gold, 0.22f * fade));
    }

    static void DrawOathReinforcedTile()
    {
        if (oathTilePulseTimer <= 0f || oathReinforcedTx < 0) return;
        float t = oathTilePulseTimer / 3.2f;
        float pulse = MathF.Sin((1f - t) * MathF.PI * 6f) * 0.5f + 0.5f;
        Vector2 center = TileCenter(oathReinforcedTx, oathReinforcedTy);
        Color gold = AbilityAccent(AbilityType.OathOfTheBailey);
        float half = TileSize * 0.46f;
        var tileRect = new Rectangle(center.X - half, center.Y - half, half * 2f, half * 2f);
        Raylib.DrawRectangleLinesEx(tileRect, 2f, WithAlpha(gold, 0.35f + pulse * 0.45f));
        DrawGlow(center, TileSize * 0.55f, gold, 0.04f + pulse * 0.05f);
        Raylib.DrawCircleV(center, 5f + pulse * 4f, WithAlpha(Color.White, 0.2f + pulse * 0.25f));
    }

    static void DrawVerdictWave()
    {
        if (verdictWaveTimer <= 0f) return;
        float t = 1f - verdictWaveTimer / VerdictWaveTime;
        float fade = 1f - t;
        Color gold = AbilityAccent(AbilityType.Verdict);
        float expand = 60f + t * 380f;
        DrawGlowFast(verdictWaveOrigin, expand * 0.38f, gold, 0.06f * fade);
        Raylib.DrawRing(verdictWaveOrigin, expand * 0.82f, expand, 0f, 360f, 72, WithAlpha(gold, 0.5f * fade));
        Raylib.DrawRing(verdictWaveOrigin, expand * 0.55f, expand * 0.72f, 0f, 360f, 56, WithAlpha(Color.White, 0.32f * fade));
        Raylib.DrawCircleV(verdictWaveOrigin, 12f + t * 20f, WithAlpha(Color.White, 0.25f * fade));
    }

    static void DrawVerdictHaltOverlay()
    {
        if (verdictHaltTimer <= 0f) return;
        float time = (float)Raylib.GetTime();
        float pulse = MathF.Sin(time * 3f) * 0.5f + 0.5f;
        Color gold = AbilityAccent(AbilityType.Verdict);

        Raylib.DrawRectangle(0, 0, WindowWidth, WindowHeight, WithAlpha(gold, 0.03f + pulse * 0.02f));
        Raylib.DrawRectangleLinesEx(new Rectangle(8f, 8f, WindowWidth - 16f, WindowHeight - 16f), 2f, WithAlpha(gold, 0.22f + pulse * 0.18f));

        int cx = WindowWidth / 2;
        string label = "VERDICT  �  EVENTS HALTED  " + Math.Ceiling(verdictHaltTimer).ToString("0") + "s";
        int lw = Raylib.MeasureText(label, 14);
        var banner = new Rectangle(cx - lw / 2f - 16f, 18f, lw + 32f, 28f);
        DrawRichPanel(banner, WithAlpha(UiPanel, 0.88f), gold, 0.2f, accentStripe: true);
        Raylib.DrawText(label, cx - lw / 2, 24, 14, Color.White);
    }

    static void DrawVerdictTelegraph()
    {
        if (!IsVerdictUnlocked() || !HasEquippedAbility(AbilityType.Verdict)) return;
        if (verdictCooldown > 0f || verdictHaltTimer > 0f) return;
        if (activeEvent == FloorEventType.None && floorRotTimer <= 0f) return;

        float time = (float)Raylib.GetTime();
        float pulse = MathF.Sin(time * 5f) * 0.5f + 0.5f;
        Color gold = AbilityAccent(AbilityType.Verdict);
        ShadowTextCentered("VERDICT READY - END THE EVENT", WindowWidth / 2, WindowHeight - 118, 12, WithAlpha(gold, 0.55f + pulse * 0.35f), 1f);
    }

    static void SpawnWalkDust(float dt)
    {
        if (playerVel.LengthSquared() < 1600f) return;

        stepTimer -= dt;
        if (stepTimer > 0f) return;

        stepTimer = 0.07f;
        Vector2 back = playerVel == Vector2.Zero ? Vector2.Zero : -Vector2.Normalize(playerVel);
        AddParticle(new Particle
        {
            Position = playerPos + back * PlayerRadius,
            Velocity = back * 30f,
            Color = Lighten(Emerald, 0.2f),
            Alpha = 0.5f,
            Fade = 1f / 0.4f,
            Size = 3f,
            Drag = 4f,
            Glow = true,
        });
    }

    static void SpawnBlightSpores(Vector2 origin)
    {
        int count = activeEvent == FloorEventType.BlightStorm ? Rng.Next(10, 16) : Rng.Next(16, 24);
        for (int i = 0; i < count; i++)
        {
            float angle = Rng.NextSingle() * MathF.PI * 2f;
            float speed = Rng.Next(20, 70);
            AddParticle(new Particle
            {
                Position = origin + new Vector2(Rng.Next(-20, 20), Rng.Next(-20, 20)),
                Velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed,
                Color = new Color(138, 96, 48, 255),
                Alpha = 1f,
                Fade = 1f / (0.8f + Rng.NextSingle() * 0.6f),
                Rotation = Rng.NextSingle() * 360f,
                Spin = (Rng.NextSingle() - 0.5f) * 200f,
                Size = 4f + Rng.NextSingle() * 4f,
                Drag = 1.8f,
                Glow = false,
            });
        }
    }

    static void SpawnFloatingText(Vector2 pos, string text, Color color, int size)
    {
        if (!floatingTextEnabled) return;
        floaters.Add(new FloatingText
        {
            Position = pos,
            Velocity = new Vector2((Rng.NextSingle() - 0.5f) * 30f, -55f),
            Text = text,
            Color = color,
            Life = 0.9f,
            MaxLife = 0.9f,
            Size = size,
        });
    }

}
