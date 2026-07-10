partial class Program
{
    // ---------------------------------------------------------------- AI pilot

    static float AiSteeringUrgency()
    {
        float voidUrg = AiVoidDashUrgency(playerPos);
        float phaseUrg = aiPhase switch
        {
            AiSurvivalPhase.LastStand => 1f,
            AiSurvivalPhase.TileCritical => 0.92f,
            AiSurvivalPhase.EventActive => 0.82f,
            AiSurvivalPhase.BossFight => 0.72f,
            AiSurvivalPhase.Combat => 0.52f,
            AiSurvivalPhase.EventPrep => 0.46f,
            AiSurvivalPhase.Patrol => 0.32f,
            AiSurvivalPhase.Calm => 0.26f,
            _ => 0.35f,
        };
        return Math.Clamp(MathF.Max(phaseUrg, voidUrg * 0.95f) + AiFutureRamUrgencyBoost(), 0f, 1f);
    }

    static Vector2 AiSmoothMoveDirection(Vector2 target, float dt)
    {
        if (target == Vector2.Zero)
        {
            aiMoveSmoothed = Vector2.Lerp(aiMoveSmoothed, Vector2.Zero, 1f - MathF.Exp(-9f * dt));
            return aiMoveSmoothed.LengthSquared() > 0.025f ? SafeNormalize(aiMoveSmoothed) : Vector2.Zero;
        }

        target = SafeNormalize(target);
        float urgency = AiSteeringUrgency();
        if (aiMoveSmoothed.LengthSquared() < 0.025f)
        {
            aiMoveSmoothed = target;
            return target;
        }

        Vector2 current = SafeNormalize(aiMoveSmoothed);
        float curAngle = MathF.Atan2(current.Y, current.X);
        float tgtAngle = MathF.Atan2(target.Y, target.X);
        float delta = tgtAngle - curAngle;
        while (delta > MathF.PI) delta -= MathF.PI * 2f;
        while (delta < -MathF.PI) delta += MathF.PI * 2f;

        float maxTurn = (1.4f + urgency * 4.2f) * dt;
        delta = Math.Clamp(delta, -maxTurn, maxTurn);
        float newAngle = curAngle + delta;
        Vector2 turned = new Vector2(MathF.Cos(newAngle), MathF.Sin(newAngle));
        float follow = 1f - MathF.Exp(-(3.5f + urgency * 7f) * dt);
        aiMoveSmoothed = SafeNormalize(Vector2.Lerp(current, turned, follow));
        return aiMoveSmoothed;
    }

    static void UpdateAiPilot(float dt)
    {
        aiDashRequest = false;
        aiParalyzeRequest = false;
        if (aiPostParalyzeGrace > 0f) aiPostParalyzeGrace -= dt;

        aiIntelTimer -= dt;
        if (aiIntelTimer <= 0f)
        {
            aiIntelTimer = 0.1f;
            RebuildAiBoardIntel();
        }

        RunAiGrandmasterBrain(dt);
        UpdateAiZoneTimers(dt);
        aiParalyzeRequest = HasEquippedAbility(AbilityType.Paralyze) && AiShouldParalyze(playerPos);
        ComputeHeuristicPilotMove(dt, out Vector2 move, out float dashUrgency);
        move = AiGrandmasterBlendSteering(playerPos, move, dt);
        AiHumanPlan humanPlan = UpdateAiHumanSurvivalDirector(dt, move, dashUrgency);
        move = humanPlan.Move;
        dashUrgency = MathF.Max(dashUrgency, humanPlan.DashUrgency);
        aiParalyzeRequest |= humanPlan.WantsParalyze;
        aiMove = AiSmoothMoveDirection(move, dt);

        if (AiShouldDashToHealthyGround(playerPos))
        {
            if (AiTryGetCrumblingFootingEscape(playerPos, out Vector2 crumbleDir, out bool wantDash, out _))
            {
                if (wantDash && crumbleDir != Vector2.Zero) lastMoveDirection = crumbleDir;
            }
        }

        if (AiShouldDashForCollapseEvent(playerPos))
        {
            if (!aiLargestClusterValid) RebuildLargestHealthyCluster();
            Vector2 toCluster = aiLargestClusterTarget - playerPos;
            if (toCluster.LengthSquared() > 1f) lastMoveDirection = SafeNormalize(toCluster);
        }

        // The final planner owns the dash vector after specialist systems submit advice.
        if (humanPlan.WantsDash && humanPlan.DashDirection != Vector2.Zero)
        {
            lastMoveDirection = SafeNormalize(humanPlan.DashDirection);
        }

        if (!aiParalyzeRequest && aiPostParalyzeGrace <= 0f)
        {
            UpdateAiGrandmasterDashLogic(dt, ref dashUrgency);
            aiDashUrgency = Approach(aiDashUrgency, dashUrgency, dashUrgency >= 0.9f ? 2.8f : 1.6f, dt);
            bool collapseDash = AiShouldDashForCollapseEvent(playerPos) && dashUrgency >= 0.72f;
            bool crumbleDash = AiShouldDashToHealthyGround(playerPos) && dashUrgency >= 0.72f;
            aiDashRequest = dashCooldown <= 0f && dashTimer <= 0f
                && (humanPlan.WantsDash
                    || dashUrgency >= 0.99f && AiMustDashThroughEnemies(playerPos)
                    || AiVoidDashUrgency(playerPos) >= 0.92f
                    || collapseDash
                    || crumbleDash
                    || (aiPhase == AiSurvivalPhase.LastStand && dashUrgency >= 0.85f));
        }
        else
        {
            aiDashUrgency = Approach(aiDashUrgency, 0f, 6f, dt);
        }
    }

    static void ComputeHeuristicPilotMove(float dt, out Vector2 move, out float dashUrgency)
    {
        dashUrgency = 0f;
        float nearestThreat = NearestEnemyDistance(playerPos);
        float nearestBoss = NearestBossDistance(playerPos);
        bool threatsNear = nearestThreat < 200f || nearestBoss < 300f;

        Vector2 voidFlee = ComputeUrgentVoidFlee(playerPos);
        if (voidFlee != Vector2.Zero)
        {
            move = RefineAiDirection(voidFlee, playerPos, true);
            if (move != Vector2.Zero) lastMoveDirection = move;
            aiSteerVel = Vector2.Lerp(aiSteerVel, voidFlee * EffMoveSpeed(), 1f - MathF.Exp(-20f * dt));
            dashUrgency = AiVoidDashUrgency(playerPos);
            aiCrumbleDashUrgency = 0f;
            return;
        }

        if (AiTryGetCrumblingFootingEscape(playerPos, out Vector2 crumbleRun, out bool crumbleDash, out Vector2 crumbleDashDir))
        {
            move = RefineAiDirection(crumbleRun, playerPos, true);
            if (move != Vector2.Zero) lastMoveDirection = move;
            if (crumbleDash && crumbleDashDir != Vector2.Zero) lastMoveDirection = crumbleDashDir;
            aiSteerVel = Vector2.Lerp(aiSteerVel, crumbleRun * EffMoveSpeed(), 1f - MathF.Exp(-18f * dt));
            dashUrgency = crumbleDash ? 1f : 0.8f;
            aiCrumbleDashUrgency = dashUrgency;
            return;
        }

        aiCrumbleDashUrgency = 0f;

        Vector2 urgent = GetUrgentAiDirection();
        if (urgent != Vector2.Zero)
        {
            move = RefineAiDirection(urgent, playerPos, true);
            if (move != Vector2.Zero) lastMoveDirection = move;
            dashUrgency = Math.Max(AiCollapseEventDashUrgency(playerPos), AiVoidDashUrgency(playerPos));
            if (AiIsMassCollapseEvent() && !AiPlayerInLargestClusterInterior())
            {
                dashUrgency = Math.Max(dashUrgency, 0.88f);
            }
            if (dashUrgency < 0.5f) UpdateHeuristicDashUrgency(threatsNear, nearestThreat, out dashUrgency);
            return;
        }

        if (AiShouldHoldEventSafeZone())
        {
            move = Vector2.Zero;
            aiSteerVel = Vector2.Lerp(aiSteerVel, Vector2.Zero, 1f - MathF.Exp(-14f * dt));
            UpdateHeuristicDashUrgency(threatsNear, nearestThreat, out dashUrgency);
            return;
        }

        Vector2 enemyPanic = ComputeUrgentEnemyFlee(playerPos);
        if (enemyPanic != Vector2.Zero)
        {
            move = RefineAiDirection(enemyPanic, playerPos, true);
            if (move != Vector2.Zero) lastMoveDirection = move;
            aiSteerVel = Vector2.Lerp(aiSteerVel, enemyPanic * EffMoveSpeed(), 1f - MathF.Exp(-18f * dt));
            UpdateHeuristicDashUrgency(threatsNear, nearestThreat, out dashUrgency);
            return;
        }

        if (TryGetTileUnder(playerPos, out int feetX, out int feetY) && AiIsTileTooDamaged(feetX, feetY))
        {
            Vector2 fleeTile = activeEvent != FloorEventType.None && eventCountdown > 0f
                ? DirectionToSafeZoneInterior(playerPos)
                : DirectionToNearestSafeTile(playerPos);
            if (fleeTile != Vector2.Zero)
            {
                move = RefineAiDirection(fleeTile, playerPos, threatsNear);
                if (move != Vector2.Zero) lastMoveDirection = move;
                aiSteerVel = Vector2.Lerp(aiSteerVel, fleeTile * EffMoveSpeed(), 1f - MathF.Exp(-14f * dt));
                UpdateHeuristicDashUrgency(threatsNear, nearestThreat, out dashUrgency);
                return;
            }
        }

        aiAnchorPos = FindGlobalBestTileAnchor(playerPos, threatsNear);

        if (ShouldAiHoldPosition(threatsNear))
        {
            move = Vector2.Zero;
            aiSteerVel = Vector2.Lerp(aiSteerVel, Vector2.Zero, 1f - MathF.Exp(-14f * dt));
            UpdateHeuristicDashUrgency(threatsNear, nearestThreat, out dashUrgency);
            return;
        }

        Vector2 boardPull = ComputeGlobalBoardSteering(playerPos, threatsNear);
        Vector2 hazardPush = ComputeGlobalHazardRepulsion(playerPos);
        Vector2 voidPush = ComputeVoidRepulsion(playerPos);
        Vector2 damagedPush = ComputeDamagedTileRepulsion(playerPos);
        Vector2 enemySteer = ComputeEnemySteering(playerPos);
        Vector2 edgeAvoid = ComputeEdgeAvoidance(playerPos);
        Vector2 topAvoid = ComputeTopBandAvoidance(playerPos);
        Vector2 healthyPull = ComputeHealthyTilePull(playerPos);
        Vector2 anchorPull = SafeNormalize(aiAnchorPos - playerPos);

        bool feetWeak = IsFeetTileWeak();
        float anchorW = feetWeak ? 2.4f : threatsNear ? 0.85f : 0f;
        float boardW = feetWeak ? 2f : threatsNear ? 1.15f : 0f;
        float hazardW = 2.5f;
        float voidW = 4f;
        float damagedW = 3.8f;
        float enemyW = threatsNear ? 2.8f : 1.35f;
        float edgeW = 3.4f;
        float topW = 1.6f + Math.Clamp(aiTopBandTimer, 0f, 3f) * 0.55f;
        float healthyW = feetWeak ? 2.2f : 1.35f;

        Vector2 desired = boardPull * boardW + hazardPush * hazardW + voidPush * voidW
            + damagedPush * damagedW + enemySteer * enemyW + anchorPull * anchorW
            + edgeAvoid * edgeW + topAvoid * topW + healthyPull * healthyW;

        if (desired.LengthSquared() < 0.04f)
        {
            move = Vector2.Zero;
            aiSteerVel = Vector2.Lerp(aiSteerVel, Vector2.Zero, 1f - MathF.Exp(-14f * dt));
        }
        else
        {
            aiSteerVel = Vector2.Lerp(aiSteerVel, desired, 1f - MathF.Exp(-(6.5f + AiSteeringUrgency() * 5f) * dt));
            Vector2 moveDir = aiSteerVel.LengthSquared() > 0.03f ? SafeNormalize(aiSteerVel) : Vector2.Zero;
            move = moveDir != Vector2.Zero ? RefineAiDirection(moveDir, playerPos, threatsNear) : Vector2.Zero;
            if (move != Vector2.Zero) lastMoveDirection = move;
        }

        UpdateHeuristicDashUrgency(threatsNear, nearestThreat, out dashUrgency);
    }

    static void UpdateHeuristicDashUrgency(bool inCombat, float nearestEnemy, out float urgency)
    {
        aiDashRequest = false;
        urgency = 0f;
        if (dashCooldown > 0f || dashTimer > 0f)
        {
            aiDashUrgency = Approach(aiDashUrgency, 0f, 6f, Raylib.GetFrameTime());
            urgency = aiDashUrgency;
            return;
        }

        if (!AiMustDashThroughEnemies(playerPos))
        {
            urgency = 0f;
            aiDashUrgency = Approach(aiDashUrgency, 0f, 6f, Raylib.GetFrameTime());
            return;
        }

        Vector2 escapeDir = AiPickEscapeDashDirection(playerPos, throughEnemies: true);
        if (escapeDir == Vector2.Zero)
        {
            urgency = 0f;
            aiDashUrgency = Approach(aiDashUrgency, 0f, 6f, Raylib.GetFrameTime());
            return;
        }

        lastMoveDirection = SafeNormalize(escapeDir);
        urgency = 1f;
        aiDashUrgency = 1f;
    }

    static int AiEnemiesThreatening(Vector2 pos, float range)
    {
        int count = 0;
        foreach (Enemy e in enemies)
        {
            if (e.Dead || e.Spawn < 0.5f) continue;
            if (Vector2.Distance(pos, e.Position) <= range + e.Radius) count++;
        }
        return count;
    }

    static bool AiHasCleanWalkEscape(Vector2 pos)
    {
        for (int i = 0; i < 8; i++)
        {
            float angle = i * (MathF.PI / 4f);
            Vector2 dir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            Vector2 probe = pos + dir * 72f;
            if (AiScoreWalkExit(probe, pos) > 60f && AiNearestEnemyZoneMargin(probe) > TileSize * 0.55f)
            {
                return true;
            }
        }
        return false;
    }

    static bool AiMustDashThroughEnemies(Vector2 pos)
    {
        if (paralyzeCooldown <= 0f) return false;
        if (aiPostParalyzeGrace > 0f) return false;
        if (AiShouldParalyze(pos)) return false;

        bool inDanger = AiIsInsideEnemyDangerZone(pos);
        bool trapped = AiIsTrapped(pos);
        if (!inDanger && !trapped) return false;
        if (AiHasCleanWalkEscape(pos)) return false;

        Vector2 dashDir = AiPickEscapeDashDirection(pos, throughEnemies: true);
        if (dashDir == Vector2.Zero) return false;

        Vector2 land = pos + SafeNormalize(dashDir) * DashSpeed * DashDuration;
        float zoneGain = AiNearestEnemyZoneMargin(land) - AiNearestEnemyZoneMargin(pos);
        return zoneGain > TileSize * 0.15f || (trapped && AiEnemiesThreatening(pos, 150f) >= 2);
    }

    static bool AiShouldParalyze(Vector2 pos)
    {
        if (paralyzeCooldown > 0f) return false;
        if (aiPostParalyzeGrace > 0f) return false;

        float radius = EffParalyzeRadius();
        int inRange = 0;
        foreach (Enemy e in enemies)
        {
            if (e.Dead || e.Spawn < 0.5f) continue;
            if (Vector2.Distance(pos, e.Position) <= radius + e.Radius) inRange++;
        }

        if (inRange == 0) return false;
        if (AiIsTrapped(pos)) return true;
        if (AiIsInsideEnemyDangerZone(pos) && inRange >= 2) return true;
        if (inRange >= 3 && AiNearestEnemyZoneMargin(pos) < TileSize * 0.75f) return true;
        return false;
    }

    static int AiCountViableWalkExits(Vector2 pos, float dist)
    {
        int count = 0;
        for (int i = 0; i < 8; i++)
        {
            float angle = i * (MathF.PI / 4f);
            Vector2 dir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            if (AiScoreWalkExit(pos + dir * dist, pos) > 0f) count++;
        }
        return count;
    }

    static float AiScoreWalkExit(Vector2 probe, Vector2 from)
    {
        float score = AiScoreProbePosition(probe, from);
        if (score < -500f) return -1f;
        if (AiNearestEnemyZoneMargin(probe) < TileSize * 0.2f) score -= 500f;
        return score;
    }

    static bool AiIsTrapped(Vector2 pos)
    {
        int exits = AiCountViableWalkExits(pos, 70f);
        int threats = AiEnemiesThreatening(pos, 170f);

        if (exits == 0 && threats >= 1) return true;
        if (AiIsInsideEnemyDangerZone(pos) && threats >= 2 && exits <= 1) return true;
        if (threats >= 3 && exits <= 1) return true;
        return false;
    }

    static Vector2 AiEnemyCentroidNear(Vector2 from, float range)
    {
        Vector2 sum = Vector2.Zero;
        int count = 0;
        foreach (Enemy e in enemies)
        {
            if (e.Dead || e.Spawn < 0.5f) continue;
            if (Vector2.Distance(from, e.Position) > range) continue;
            sum += e.Position;
            count++;
        }
        return count > 0 ? sum / count : Vector2.Zero;
    }

    static bool AiDashImprovesSituation(Vector2 from, Vector2 dashDir)
    {
        if (dashDir == Vector2.Zero) return false;

        Vector2 land = from + SafeNormalize(dashDir) * DashSpeed * DashDuration;
        if (!TryGetTileUnder(from, out int fx, out int fy)) return true;
        if (!TryGetTileUnder(land, out int lx, out int ly)) return false;
        if (tiles[lx, ly].State == 2 || AiVoidDistance(lx, ly) <= 1) return false;
        if (AiIsTileTooDamaged(lx, ly) && !AiIsTileTooDamaged(fx, fy)) return false;

        float hazardGain = aiTileHazard[fx, fy] - aiTileHazard[lx, ly];
        float valueGain = aiTileValue[lx, ly] - aiTileValue[fx, fy];
        float zoneGain = AiNearestEnemyZoneMargin(land) - AiNearestEnemyZoneMargin(from);

        if (tiles[fx, fy].EventMarked && !tiles[lx, ly].EventMarked) return true;
        return hazardGain > 0.18f || valueGain > 50f || zoneGain > TileSize * 0.35f;
    }

    static Vector2 AiPickEscapeDashDirection(Vector2 from, bool throughEnemies)
    {
        float dashDist = DashSpeed * DashDuration;
        float bestScore = float.NegativeInfinity;
        Vector2 best = lastMoveDirection != Vector2.Zero ? lastMoveDirection : Vector2.UnitY;
        Vector2 centroid = AiEnemyCentroidNear(from, 300f);

        for (int i = 0; i < 16; i++)
        {
            float angle = i * (MathF.PI * 2f / 16f);
            Vector2 dir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            Vector2 land = from + dir * dashDist;

            if (!TryGetTileUnder(land, out int tx, out int ty)) continue;
            if (tiles[tx, ty].State == 2) continue;
            if (AiIsTileTooDamaged(tx, ty)) continue;
            if (AiVoidDistance(tx, ty) <= 1) continue;

            float score = aiTileValue[tx, ty];
            score += tiles[tx, ty].Durability * 14f;
            score -= aiTileHazard[tx, ty] * 180f;
            score += AiNearestEnemyZoneMargin(land) * (throughEnemies ? 10f : 14f);
            score += AiCountViableWalkExits(land, 54f) * 50f;

            if (centroid != Vector2.Zero)
            {
                score += (Vector2.Distance(land, centroid) - Vector2.Distance(from, centroid)) * 3f;
            }

            float landMargin = AiNearestEnemyZoneMargin(land);
            if (landMargin < 0f)
            {
                score -= throughEnemies ? 90f : 450f;
            }
            else if (landMargin < TileSize * 0.35f)
            {
                score -= throughEnemies ? 40f : 160f;
            }

            if (score > bestScore)
            {
                bestScore = score;
                best = dir;
            }
        }

        return best;
    }

    static int AiEdgeMargin(int x, int y)
        => Math.Min(Math.Min(x, GridSize - 1 - x), Math.Min(y, GridSize - 1 - y));

    static bool AiIsPreferredMiddleTile(int x, int y)
    {
        int margin = AiEdgeMargin(x, y);
        return margin >= 3 && margin <= 5;
    }

    static bool ShouldAiHoldPosition(bool threatsNear)
    {
        if (!threatsNear) return false;
        if (!TryGetTileUnder(playerPos, out int tx, out int ty)) return false;
        if (activeEvent != FloorEventType.None && eventCountdown > 0f) return false;
        if (threatsNear && NearestEnemyDistance(playerPos) < 95f) return false;
        if (AiIsInsideEnemyDangerZone(playerPos)) return false;
        if (AiNearestEnemyZoneMargin(playerPos) < TileSize * 0.6f) return false;
        if (NearestBossDistance(playerPos) < 160f) return false;
        if (IsFeetTileWeak()) return false;
        if (tiles[tx, ty].EventMarked) return false;
        if (AiVoidDistance(tx, ty) < 3) return false;
        if (tiles[tx, ty].Durability < MaxDurability * AiMaxWalkDurabilityRatio) return false;
        if (Vector2.Distance(playerPos, aiAnchorPos) > 60f) return false;
        return true;
    }

    static bool AiIsCriticallyDamagedTile(int x, int y)
    {
        if (x < 0 || x >= GridSize || y < 0 || y >= GridSize) return false;
        ref Tile t = ref tiles[x, y];
        if (t.State == 2) return false;
        return t.Durability < MaxDurability * AiCriticalTileHealthRatio;
    }

    static void AiAccumulateDangerZoneFlee(ref Vector2 push, ref bool threatened, Vector2 from, Vector2 hazardPos, float senseRadius)
    {
        Vector2 away = from - hazardPos;
        float dist = away.Length();
        if (dist >= senseRadius) return;

        threatened = true;
        if (dist < 0.01f)
        {
            away = Vector2.UnitY;
            dist = 1f;
        }
        else
        {
            away /= dist;
        }

        float t = 1f - dist / senseRadius;
        push += away * (0.65f + t * t * 2.8f);
    }

    static Vector2 ComputeVoidRepulsion(Vector2 from)
    {
        Vector2 push = Vector2.Zero;

        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if (tiles[x, y].State != 2) continue;

                Vector2 voidPos = TileCenter(x, y);
                Vector2 away = from - voidPos;
                float dist = away.Length();
                if (dist < 8f) dist = 8f;

                float weight = 42000f / (dist * dist + 40f);
                push += (dist > 0.01f ? away / dist : Vector2.UnitY) * weight;
            }
        }

        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                int voidDist = AiVoidDistance(x, y);
                if (voidDist > 2 || tiles[x, y].State == 2) continue;

                Vector2 tilePos = TileCenter(x, y);
                Vector2 away = from - tilePos;
                float dist = away.Length();
                if (dist < 6f) dist = 6f;

                float weight = (3 - voidDist) * 9000f / (dist * dist + 60f);
                push += (dist > 0.01f ? away / dist : Vector2.UnitY) * weight;
            }
        }

        if (push.LengthSquared() < 0.001f) return Vector2.Zero;
        return SafeNormalize(push) * MathF.Min(push.Length() * 0.007f, 1.7f);
    }

    static Vector2 ComputeUrgentVoidFlee(Vector2 from)
    {
        float senseRadius = AiVoidDangerRadius + PlayerRadius;
        Vector2 push = Vector2.Zero;
        bool threatened = false;

        if (TryGetTileUnder(from, out int feetX, out int feetY))
        {
            if (tiles[feetX, feetY].State == 2 || AiIsCriticallyDamagedTile(feetX, feetY))
            {
                threatened = true;
            }
        }

        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                ref Tile tile = ref tiles[x, y];
                if (tile.State == 2)
                {
                    AiAccumulateDangerZoneFlee(ref push, ref threatened, from, TileCenter(x, y), senseRadius);
                    continue;
                }

                if (AiIsCriticallyDamagedTile(x, y))
                {
                    AiAccumulateDangerZoneFlee(ref push, ref threatened, from, TileCenter(x, y), senseRadius);
                }
            }
        }

        if (!threatened || push.LengthSquared() < 0.001f) return Vector2.Zero;
        return SafeNormalize(push);
    }

    static float AiVoidDashUrgency(Vector2 from)
    {
        float senseRadius = AiVoidDangerRadius + PlayerRadius * 0.65f;
        float closest = float.MaxValue;

        if (TryGetTileUnder(from, out int feetX, out int feetY))
        {
            if (tiles[feetX, feetY].State == 2 || AiIsCriticallyDamagedTile(feetX, feetY))
            {
                return 1f;
            }
        }

        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if (tiles[x, y].State != 2 && !AiIsCriticallyDamagedTile(x, y)) continue;
                float dist = Vector2.Distance(from, TileCenter(x, y));
                if (dist < closest) closest = dist;
            }
        }

        if (closest >= senseRadius) return 0f;
        return Math.Clamp(1f - closest / senseRadius, 0f, 1f);
    }

    static Vector2 ComputeDamagedTileRepulsion(Vector2 from)
    {
        Vector2 push = Vector2.Zero;

        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if (!AiIsTileTooDamaged(x, y)) continue;

                Vector2 tilePos = TileCenter(x, y);
                Vector2 away = from - tilePos;
                float dist = away.Length();
                if (dist < 8f) dist = 8f;

                float weight = 32000f / (dist * dist + 45f);
                push += (dist > 0.01f ? away / dist : Vector2.UnitY) * weight;
            }
        }

        if (push.LengthSquared() < 0.001f) return Vector2.Zero;
        return SafeNormalize(push) * MathF.Min(push.Length() * 0.007f, 1.6f);
    }

    static void UpdateAiZoneTimers(float dt)
    {
        if (!TryGetTileUnder(playerPos, out int tx, out int ty))
        {
            aiTopBandTimer = Math.Max(0f, aiTopBandTimer - dt * 2f);
            return;
        }

        if (ty < AiTopBandRows)
        {
            aiTopBandTimer += dt;
        }
        else
        {
            aiTopBandTimer = Math.Max(0f, aiTopBandTimer - dt * 2f);
        }
    }

    static bool AiIsNearGridEdge(int x, int y)
        => AiEdgeMargin(x, y) < AiEdgeMarginTiles;

    static bool AiIsHealthyTile(int x, int y)
    {
        if (x < 0 || x >= GridSize || y < 0 || y >= GridSize) return false;
        ref Tile t = ref tiles[x, y];
        if (t.State == 2 || t.EventMarked) return false;
        if (AiIsTileTooDamaged(x, y)) return false;
        return t.Durability >= MaxDurability * AiHealthyTileRatio;
    }

    static bool AiTileAboutToCollapse(int x, int y)
    {
        if (x < 0 || x >= GridSize || y < 0 || y >= GridSize) return false;
        ref Tile t = ref tiles[x, y];
        if (t.State == 2) return false;
        if (t.EventMarked) return true;
        if (AiIsCriticallyDamagedTile(x, y)) return true;
        if (t.RegrowTimer > 0f && t.Collapse >= 1f) return true;
        if (t.Collapse > 0.6f) return true;
        return t.Durability < MaxDurability * (AiMaxWalkDurabilityRatio + 0.06f);
    }

    static int AiCountConnectedCollapsingTilesAt(int seedX, int seedY)
    {
        if (!AiTileAboutToCollapse(seedX, seedY)) return 0;

        Array.Clear(aiSafeVisited, 0, aiSafeVisited.Length);
        aiSafeBfsQueue.Clear();
        aiSafeVisited[seedX, seedY] = true;
        aiSafeBfsQueue.Enqueue((seedX, seedY));
        int count = 0;

        while (aiSafeBfsQueue.Count > 0)
        {
            (int cx, int cy) = aiSafeBfsQueue.Dequeue();
            count++;
            if (count > 2) return count;

            TryEnqueue(cx - 1, cy);
            TryEnqueue(cx + 1, cy);
            TryEnqueue(cx, cy - 1);
            TryEnqueue(cx, cy + 1);
        }

        return count;

        void TryEnqueue(int nx, int ny)
        {
            if (nx < 0 || nx >= GridSize || ny < 0 || ny >= GridSize) return;
            if (aiSafeVisited[nx, ny] || !AiTileAboutToCollapse(nx, ny)) return;
            aiSafeVisited[nx, ny] = true;
            aiSafeBfsQueue.Enqueue((nx, ny));
        }
    }

    static bool AiTryFindNearestHealthyTile(Vector2 from, out Vector2 target, float maxDistTiles)
    {
        target = from;
        float maxDist = maxDistTiles * TileSize;
        float bestScore = float.NegativeInfinity;
        bool found = false;

        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if (!AiIsHealthyTile(x, y)) continue;

                Vector2 pos = TileCenter(x, y);
                float dist = Vector2.Distance(from, pos);
                if (dist > maxDist) continue;

                float score = -dist + tiles[x, y].Durability * 0.08f;
                if (AiIsNearGridEdge(x, y)) score -= 120f;
                if (y < AiTopBandRows) score -= 80f;

                if (score > bestScore)
                {
                    bestScore = score;
                    target = pos;
                    found = true;
                }
            }
        }

        return found;
    }

    static bool AiWouldDashIntoVoid(Vector2 from, Vector2 dir, float dashDist)
    {
        Vector2 land = from + dir * dashDist;
        if (!TryGetTileUnder(land, out int lx, out int ly)) return true;
        if (tiles[lx, ly].State == 2) return true;
        if (AiVoidDistance(lx, ly) <= 1) return true;
        if (AiIsNearGridEdge(lx, ly)) return true;

        if (!TryGetTileUnder(from, out int fx, out int fy)) return true;
        if (AiVoidDistance(lx, ly) < AiVoidDistance(fx, fy)) return true;

        for (int step = 1; step <= 5; step++)
        {
            Vector2 p = from + dir * (dashDist * step / 5f);
            if (!TryGetTileUnder(p, out int tx, out int ty)) return true;
            if (tiles[tx, ty].State == 2 || AiVoidDistance(tx, ty) <= 1) return true;
        }

        return false;
    }

    static bool AiDashPathToHealthyIsSafe(Vector2 from, Vector2 dashDir, Vector2 healthyTarget)
    {
        if (dashDir == Vector2.Zero) return false;

        Vector2 n = SafeNormalize(dashDir);
        float dashDist = DashSpeed * DashDuration;
        if (AiWouldDashIntoVoid(from, n, dashDist)) return false;

        Vector2 land = from + n * dashDist;
        if (!TryGetTileUnder(land, out int lx, out int ly)) return false;
        if (tiles[lx, ly].State == 2 || AiVoidDistance(lx, ly) <= 1) return false;
        if (AiTileAboutToCollapse(lx, ly)) return false;

        float before = Vector2.Distance(from, healthyTarget);
        float after = Vector2.Distance(land, healthyTarget);
        return after < before - TileSize * 0.1f;
    }

    static bool AiTryGetCrumblingFootingEscape(Vector2 from, out Vector2 runDir, out bool wantDash, out Vector2 dashDir)
    {
        runDir = Vector2.Zero;
        dashDir = Vector2.Zero;
        wantDash = false;

        if (!TryGetTileUnder(from, out int fx, out int fy)) return false;
        if (!AiTileAboutToCollapse(fx, fy)) return false;
        if (AiCountConnectedCollapsingTilesAt(fx, fy) > 2) return false;
        if (!AiTryFindNearestHealthyTile(from, out Vector2 healthyPos, 9f)) return false;

        runDir = SafeNormalize(healthyPos - from);
        if (runDir == Vector2.Zero) return false;

        Vector2 toHealthy = SafeNormalize(healthyPos - from);
        if (AiDashPathToHealthyIsSafe(from, toHealthy, healthyPos))
        {
            wantDash = true;
            dashDir = toHealthy;
            return true;
        }

        float bestScore = float.NegativeInfinity;
        Vector2 bestDash = Vector2.Zero;
        for (int i = 0; i < 16; i++)
        {
            float angle = i * (MathF.PI * 2f / 16f);
            Vector2 dir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            if (!AiDashPathToHealthyIsSafe(from, dir, healthyPos)) continue;

            Vector2 land = from + dir * DashSpeed * DashDuration;
            if (!TryGetTileUnder(land, out int lx, out int ly)) continue;

            float score = Vector2.Dot(dir, runDir) * 120f;
            score += aiTileValue[lx, ly] * 0.4f;
            score += tiles[lx, ly].Durability * 0.2f;
            score -= aiTileHazard[lx, ly] * 80f;
            score -= Vector2.Distance(land, healthyPos) * 0.35f;

            if (score > bestScore)
            {
                bestScore = score;
                bestDash = dir;
            }
        }

        if (bestDash != Vector2.Zero)
        {
            wantDash = true;
            dashDir = bestDash;
        }

        return true;
    }

    static bool AiShouldDashToHealthyGround(Vector2 pos)
    {
        return AiTryGetCrumblingFootingEscape(pos, out _, out bool wantDash, out Vector2 dashDir)
            && wantDash && dashDir != Vector2.Zero;
    }

    static Vector2 ComputeEdgeAvoidance(Vector2 from)
    {
        if (!TryGetTileUnder(from, out int tx, out int ty)) return Vector2.Zero;

        Vector2 push = Vector2.Zero;
        int margin = AiEdgeMargin(tx, ty);
        if (margin < AiEdgeMarginTiles)
        {
            if (tx < AiEdgeMarginTiles) push += Vector2.UnitX * (AiEdgeMarginTiles - tx + 0.5f);
            if (tx >= GridSize - AiEdgeMarginTiles) push += -Vector2.UnitX * (tx - (GridSize - AiEdgeMarginTiles) + 1f);
            if (ty < AiEdgeMarginTiles) push += Vector2.UnitY * (AiEdgeMarginTiles - ty + 0.5f);
            if (ty >= GridSize - AiEdgeMarginTiles) push += -Vector2.UnitY * (ty - (GridSize - AiEdgeMarginTiles) + 1f);
        }

        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if (!AiIsNearGridEdge(x, y)) continue;

                Vector2 tilePos = TileCenter(x, y);
                Vector2 away = from - tilePos;
                float dist = away.Length();
                if (dist < 8f) dist = 8f;

                int edgeDist = AiEdgeMargin(x, y);
                float weight = (AiEdgeMarginTiles - edgeDist + 1) * 14000f / (dist * dist + 55f);
                push += (dist > 0.01f ? away / dist : Vector2.UnitY) * weight;
            }
        }

        if (push.LengthSquared() < 0.001f) return Vector2.Zero;
        return SafeNormalize(push) * MathF.Min(push.Length() * 0.008f, 1.85f);
    }

    static Vector2 ComputeTopBandAvoidance(Vector2 from)
    {
        if (!TryGetTileUnder(from, out int tx, out int ty)) return Vector2.Zero;
        if (ty >= AiTopBandRows && aiTopBandTimer <= 0.05f) return Vector2.Zero;

        float rowPressure = ty < AiTopBandRows ? 1f - ty / (float)AiTopBandRows : 0.35f;
        float linger = Math.Clamp(aiTopBandTimer / 1.2f, 0f, 1f);
        return Vector2.UnitY * (0.55f + rowPressure * 0.95f + linger * 0.9f);
    }

    static Vector2 ComputeHealthyTilePull(Vector2 from)
    {
        if (!AiTryFindNearestHealthyTile(from, out Vector2 target, 11f)) return Vector2.Zero;

        Vector2 pull = target - from;
        float dist = pull.Length();
        if (dist < 0.01f) return Vector2.Zero;

        float strength = Math.Clamp(1f - dist / (TileSize * 11f), 0.2f, 1f);
        if (TryGetTileUnder(from, out int tx, out int ty) && !AiIsHealthyTile(tx, ty))
        {
            strength = MathF.Max(strength, 0.6f);
        }

        return pull / dist * strength;
    }

    static float NearestBossDistance(Vector2 from)
    {
        float best = float.MaxValue;
        foreach (Enemy e in enemies)
        {
            if (e.Dead || e.Spawn < 0.5f || !GetDef(e.Type).Boss) continue;
            float d = Vector2.Distance(from, e.Position);
            if (d < best) best = d;
        }
        return best;
    }

    static Vector2 DirectionToNearestSafeTile(Vector2 from)
    {
        float bestScore = float.NegativeInfinity;
        Vector2 bestPos = from;

        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                ref Tile t = ref tiles[x, y];
                if (t.State == 2 || t.EventMarked) continue;
                if (AiIsTileTooDamaged(x, y)) continue;
                if (AiIsNearGridEdge(x, y)) continue;
                if (AiVoidDistance(x, y) <= 2) continue;

                float score = t.Durability * 28f + aiTileValue[x, y] * 0.35f;
                if (AiIsHealthyTile(x, y)) score += 220f;
                score -= Vector2.Distance(from, TileCenter(x, y)) * 0.14f;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestPos = TileCenter(x, y);
                }
            }
        }

        if (Vector2.DistanceSquared(from, bestPos) < 36f) return Vector2.Zero;
        return SafeNormalize(bestPos - from);
    }

    static bool IsTileSafeForEvent(int x, int y)
    {
        if (x < 0 || x >= GridSize || y < 0 || y >= GridSize) return false;
        ref Tile t = ref tiles[x, y];
        return t.State != 2 && !t.EventMarked && !AiIsTileTooDamaged(x, y);
    }

    static int AiCountSafeCardinalNeighbors(int x, int y)
    {
        int count = 0;
        if (IsTileSafeForEvent(x - 1, y)) count++;
        if (IsTileSafeForEvent(x + 1, y)) count++;
        if (IsTileSafeForEvent(x, y - 1)) count++;
        if (IsTileSafeForEvent(x, y + 1)) count++;
        return count;
    }

    static bool AiHasUnsafeCardinalNeighbor(int x, int y)
    {
        if (x > 0)
        {
            ref Tile t = ref tiles[x - 1, y];
            if (t.State == 2 || t.EventMarked) return true;
        }
        else return true;

        if (x < GridSize - 1)
        {
            ref Tile t = ref tiles[x + 1, y];
            if (t.State == 2 || t.EventMarked) return true;
        }
        else return true;

        if (y > 0)
        {
            ref Tile t = ref tiles[x, y - 1];
            if (t.State == 2 || t.EventMarked) return true;
        }
        else return true;

        if (y < GridSize - 1)
        {
            ref Tile t = ref tiles[x, y + 1];
            if (t.State == 2 || t.EventMarked) return true;
        }
        else return true;

        return false;
    }

    static readonly List<(int X, int Y)> aiSafeComponent = new();
    static readonly List<(int X, int Y)> aiLargestCluster = new();
    static readonly Queue<(int X, int Y)> aiSafeBfsQueue = new();
    static readonly bool[,] aiSafeVisited = new bool[GridSize, GridSize];
    static readonly bool[,] aiLargestClusterMask = new bool[GridSize, GridSize];
    static Vector2 aiLargestClusterTarget;
    static bool aiLargestClusterValid;

    static bool IsTileHealthyForCluster(int x, int y)
    {
        if (!IsTileSafeForEvent(x, y)) return false;
        return tiles[x, y].Durability >= MaxDurability * 0.42f;
    }

    static bool AiIsMassCollapseEvent()
    {
        if (activeEvent == FloorEventType.None || eventCountdown <= 0f) return false;
        return activeEvent is FloorEventType.CrimsonCrumble or FloorEventType.Checkerfall
            or FloorEventType.RingCollapse or FloorEventType.StoneIslands
            or FloorEventType.ScatterPits or FloorEventType.BlightStorm
            or FloorEventType.MarkedStrike
            or FloorEventType.TideSurge or FloorEventType.TideRift or FloorEventType.TideRecede
            or FloorEventType.TideColumn or FloorEventType.TideEcho or FloorEventType.TideUndertow
            or FloorEventType.TideCrest or FloorEventType.TideWall or FloorEventType.TideAnchor
            or FloorEventType.TideFoam or FloorEventType.TideStrike
            or FloorEventType.EmberRain or FloorEventType.EmberPulse or FloorEventType.EmberCross
            or FloorEventType.EmberBridge or FloorEventType.EmberHive or FloorEventType.EmberFury
            or FloorEventType.EmberSnake or FloorEventType.EmberQuake or FloorEventType.EmberBloom
            or FloorEventType.CrownTrial or FloorEventType.CrownFall or FloorEventType.CrownShard
            or FloorEventType.CrownRing or FloorEventType.CrownIsles or FloorEventType.CrownCoronation
            or FloorEventType.CrownUsurpation or FloorEventType.CrownReckoning or FloorEventType.CrownBolt
            or FloorEventType.CrownStorm;
    }

    static void CollectConnectedHealthyTiles(int seedX, int seedY)
    {
        aiSafeComponent.Clear();
        aiSafeBfsQueue.Clear();

        aiSafeVisited[seedX, seedY] = true;
        aiSafeBfsQueue.Enqueue((seedX, seedY));

        while (aiSafeBfsQueue.Count > 0)
        {
            (int cx, int cy) = aiSafeBfsQueue.Dequeue();
            aiSafeComponent.Add((cx, cy));

            TryEnqueue(cx - 1, cy);
            TryEnqueue(cx + 1, cy);
            TryEnqueue(cx, cy - 1);
            TryEnqueue(cx, cy + 1);
        }

        void TryEnqueue(int nx, int ny)
        {
            if (nx < 0 || nx >= GridSize || ny < 0 || ny >= GridSize) return;
            if (aiSafeVisited[nx, ny] || !IsTileHealthyForCluster(nx, ny)) return;
            aiSafeVisited[nx, ny] = true;
            aiSafeBfsQueue.Enqueue((nx, ny));
        }
    }

    static void RebuildLargestHealthyCluster()
    {
        aiLargestClusterValid = false;
        aiLargestCluster.Clear();
        Array.Clear(aiSafeVisited, 0, aiSafeVisited.Length);
        Array.Clear(aiLargestClusterMask, 0, aiLargestClusterMask.Length);

        int bestSize = 0;
        float bestHealth = float.NegativeInfinity;

        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if (!IsTileHealthyForCluster(x, y) || aiSafeVisited[x, y]) continue;

                CollectConnectedHealthyTiles(x, y);
                if (aiSafeComponent.Count == 0) continue;

                float healthSum = 0f;
                foreach ((int cx, int cy) in aiSafeComponent)
                {
                    healthSum += tiles[cx, cy].Durability;
                }

                bool better = aiSafeComponent.Count > bestSize
                    || (aiSafeComponent.Count == bestSize && healthSum > bestHealth);
                if (!better) continue;

                bestSize = aiSafeComponent.Count;
                bestHealth = healthSum;
                aiLargestCluster.Clear();
                aiLargestCluster.AddRange(aiSafeComponent);
            }
        }

        if (aiLargestCluster.Count == 0) return;

        foreach ((int x, int y) in aiLargestCluster)
        {
            aiLargestClusterMask[x, y] = true;
        }

        Vector2 centroid = Vector2.Zero;
        foreach ((int cx, int cy) in aiLargestCluster)
        {
            centroid += TileCenter(cx, cy);
        }
        centroid /= aiLargestCluster.Count;

        float bestScore = float.NegativeInfinity;
        Vector2 bestPos = centroid;
        foreach ((int x, int y) in aiLargestCluster)
        {
            int clusterNeighbors = AiCountHealthyClusterNeighbors(x, y);
            float score = clusterNeighbors * 95f;
            if (clusterNeighbors < 2) score -= 220f;
            score -= Vector2.Distance(TileCenter(x, y), centroid) * 0.5f;
            score += tiles[x, y].Durability * 9f;
            if (score > bestScore)
            {
                bestScore = score;
                bestPos = TileCenter(x, y);
            }
        }

        aiLargestClusterTarget = bestPos;
        aiLargestClusterValid = true;
    }

    static int AiCountHealthyClusterNeighbors(int x, int y)
    {
        int count = 0;
        if (x > 0 && aiLargestClusterMask[x - 1, y]) count++;
        if (x < GridSize - 1 && aiLargestClusterMask[x + 1, y]) count++;
        if (y > 0 && aiLargestClusterMask[x, y - 1]) count++;
        if (y < GridSize - 1 && aiLargestClusterMask[x, y + 1]) count++;
        return count;
    }

    static bool AiPlayerInLargestClusterInterior()
    {
        if (!aiLargestClusterValid) return false;
        if (!TryGetTileUnder(playerPos, out int tx, out int ty)) return false;
        if (!aiLargestClusterMask[tx, ty]) return false;
        return AiCountHealthyClusterNeighbors(tx, ty) >= 3;
    }

    static float AiCollapseEventDashUrgency(Vector2 from)
    {
        if (!AiIsMassCollapseEvent()) return 0f;

        if (!aiLargestClusterValid) RebuildLargestHealthyCluster();
        if (!aiLargestClusterValid) return 0.85f;

        if (AiPlayerInLargestClusterInterior()) return 0f;

        float timePressure = 1f - Math.Clamp(eventCountdown / 5f, 0f, 1f);
        float dist = Vector2.Distance(from, aiLargestClusterTarget);
        float distUrgency = Math.Clamp((dist - TileSize) / (TileSize * 5f), 0f, 1f);

        if (TryGetTileUnder(from, out int tx, out int ty))
        {
            if (tiles[tx, ty].EventMarked || tiles[tx, ty].State == 2) return 1f;
            if (!aiLargestClusterMask[tx, ty]) return 0.92f;
        }

        return Math.Clamp(0.55f + distUrgency * 0.35f + timePressure * 0.4f, 0f, 1f);
    }

    static bool AiShouldDashForCollapseEvent(Vector2 pos)
    {
        if (!AiIsMassCollapseEvent()) return false;
        if (dashCooldown > 0f || dashTimer > 0f) return false;
        if (AiCollapseEventDashUrgency(pos) < 0.58f) return false;

        if (!aiLargestClusterValid) RebuildLargestHealthyCluster();
        if (!aiLargestClusterValid) return true;

        Vector2 toCluster = aiLargestClusterTarget - pos;
        if (toCluster.LengthSquared() < 1f) return false;

        Vector2 dashDir = SafeNormalize(toCluster);
        Vector2 land = pos + dashDir * DashSpeed * DashDuration;
        float gain = Vector2.Distance(pos, aiLargestClusterTarget) - Vector2.Distance(land, aiLargestClusterTarget);
        return gain > TileSize * 0.25f;
    }

    static bool TryFindNearestSafeTileSeed(Vector2 from, out int seedX, out int seedY)
    {
        seedX = 0;
        seedY = 0;
        float bestDist = float.MaxValue;
        bool found = false;

        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if (!IsTileSafeForEvent(x, y)) continue;
                float dist = Vector2.DistanceSquared(from, TileCenter(x, y));
                if (dist < bestDist)
                {
                    bestDist = dist;
                    seedX = x;
                    seedY = y;
                    found = true;
                }
            }
        }

        return found;
    }

    static void CollectConnectedSafeTiles(int seedX, int seedY)
    {
        aiSafeComponent.Clear();
        aiSafeBfsQueue.Clear();
        Array.Clear(aiSafeVisited, 0, aiSafeVisited.Length);

        aiSafeVisited[seedX, seedY] = true;
        aiSafeBfsQueue.Enqueue((seedX, seedY));

        while (aiSafeBfsQueue.Count > 0)
        {
            (int cx, int cy) = aiSafeBfsQueue.Dequeue();
            aiSafeComponent.Add((cx, cy));

            TryEnqueue(cx - 1, cy);
            TryEnqueue(cx + 1, cy);
            TryEnqueue(cx, cy - 1);
            TryEnqueue(cx, cy + 1);
        }

        void TryEnqueue(int nx, int ny)
        {
            if (nx < 0 || nx >= GridSize || ny < 0 || ny >= GridSize) return;
            if (aiSafeVisited[nx, ny] || !IsTileSafeForEvent(nx, ny)) return;
            aiSafeVisited[nx, ny] = true;
            aiSafeBfsQueue.Enqueue((nx, ny));
        }
    }

    static Vector2 DirectionToSafeZoneInterior(Vector2 from)
    {
        if (AiIsMassCollapseEvent())
        {
            if (!aiLargestClusterValid) RebuildLargestHealthyCluster();
            if (!aiLargestClusterValid) return DirectionToNearestSafeTile(from);

            if (AiPlayerInLargestClusterInterior()) return Vector2.Zero;

            if (Vector2.DistanceSquared(from, aiLargestClusterTarget) < 30f) return Vector2.Zero;
            return SafeNormalize(aiLargestClusterTarget - from);
        }

        if (!TryFindNearestSafeTileSeed(from, out int seedX, out int seedY))
        {
            return DirectionToNearestSafeTile(from);
        }

        CollectConnectedSafeTiles(seedX, seedY);
        if (aiSafeComponent.Count == 0) return Vector2.Zero;

        Vector2 centroid = Vector2.Zero;
        foreach ((int cx, int cy) in aiSafeComponent)
        {
            centroid += TileCenter(cx, cy);
        }
        centroid /= aiSafeComponent.Count;

        if (TryGetTileUnder(from, out int px, out int py) && IsTileSafeForEvent(px, py))
        {
            int neighbors = AiCountSafeCardinalNeighbors(px, py);
            if (neighbors >= 3 && Vector2.Distance(from, centroid) < TileSize * 1.35f)
            {
                return Vector2.Zero;
            }
        }

        float bestScore = float.NegativeInfinity;
        Vector2 bestPos = centroid;
        foreach ((int x, int y) in aiSafeComponent)
        {
            int safeNeighbors = AiCountSafeCardinalNeighbors(x, y);
            float score = safeNeighbors * 90f;
            if (safeNeighbors < 2) score -= 240f;
            if (AiHasUnsafeCardinalNeighbor(x, y)) score -= 110f;
            score -= Vector2.Distance(TileCenter(x, y), centroid) * 0.55f;
            score -= Vector2.Distance(from, TileCenter(x, y)) * 0.09f;
            score += tiles[x, y].Durability * 8f;

            if (score > bestScore)
            {
                bestScore = score;
                bestPos = TileCenter(x, y);
            }
        }

        if (Vector2.DistanceSquared(from, bestPos) < 28f) return Vector2.Zero;
        return SafeNormalize(bestPos - from);
    }

    static bool AiPlayerInSafeZoneInterior()
    {
        if (AiIsMassCollapseEvent())
        {
            return AiPlayerInLargestClusterInterior();
        }

        if (!TryGetTileUnder(playerPos, out int tx, out int ty)) return false;
        if (!IsTileSafeForEvent(tx, ty)) return false;
        return AiCountSafeCardinalNeighbors(tx, ty) >= 3;
    }

    static bool AiPlayerDeepInSafeRushBand()
    {
        if (!PlayerInSafeRect(eventSafeRect)) return false;
        float inset = SafeRushBandPx() * 0.32f;
        return eventSide switch
        {
            0 => playerPos.X + PlayerRadius < eventSafeRect.X + inset,
            1 => playerPos.X - PlayerRadius > eventSafeRect.X + eventSafeRect.Width - inset,
            2 => playerPos.Y + PlayerRadius < eventSafeRect.Y + inset,
            3 => playerPos.Y - PlayerRadius > eventSafeRect.Y + eventSafeRect.Height - inset,
            _ => false,
        };
    }

    static Vector2 DirectionToSafeZoneRushInterior()
    {
        const float inset = 0.35f;
        Vector2 target = eventSide switch
        {
            0 => new Vector2(eventSafeRect.X + eventSafeRect.Width * inset, playerPos.Y),
            1 => new Vector2(eventSafeRect.X + eventSafeRect.Width * (1f - inset), playerPos.Y),
            2 => new Vector2(playerPos.X, eventSafeRect.Y + eventSafeRect.Height * inset),
            3 => new Vector2(playerPos.X, eventSafeRect.Y + eventSafeRect.Height * (1f - inset)),
            _ => playerPos,
        };

        if (Vector2.DistanceSquared(playerPos, target) < 36f) return Vector2.Zero;
        return SafeNormalize(target - playerPos);
    }

    static bool AiShouldHoldEventSafeZone()
    {
        if (activeEvent == FloorEventType.None || eventCountdown <= 0f) return false;

        return activeEvent switch
        {
            FloorEventType.SafeZoneRush => AiPlayerDeepInSafeRushBand(),
            FloorEventType.CenterSnare => PlayerInCenterSnareSafe(),
            FloorEventType.CrimsonCrumble => AiPlayerInSafeZoneInterior(),
            FloorEventType.Checkerfall => AiPlayerInSafeZoneInterior(),
            FloorEventType.RingCollapse => AiPlayerInSafeZoneInterior(),
            FloorEventType.StoneIslands => AiPlayerInSafeZoneInterior(),
            FloorEventType.ScatterPits => AiPlayerInSafeZoneInterior(),
            FloorEventType.BlightStorm => AiPlayerInSafeZoneInterior(),
            FloorEventType.MarkedStrike => TryGetTileUnder(playerPos, out int mx, out int my)
                && !tiles[mx, my].EventMarked
                && AiPlayerInSafeZoneInterior(),
            FloorEventType.TideAnchor => AiPlayerInSafeZoneInterior(),
            FloorEventType.TideStrike => TryGetTileUnder(playerPos, out int tx, out int ty)
                && !tiles[tx, ty].EventMarked
                && AiPlayerInSafeZoneInterior(),
            FloorEventType.TideWhirlpool => !PlayerInTideWhirlpoolDanger(),
            FloorEventType.TideBeacon => PlayerInSafeRect(eventSafeRect),
            FloorEventType.TideSurge or FloorEventType.TideRift or FloorEventType.TideRecede
                or FloorEventType.TideColumn or FloorEventType.TideEcho or FloorEventType.TideUndertow
                or FloorEventType.TideCrest or FloorEventType.TideWall or FloorEventType.TideFoam
                => AiPlayerInSafeZoneInterior(),
            FloorEventType.EmberGate => PlayerInSafeRect(eventSafeRect),
            FloorEventType.EmberHive => PlayerOnSafeIsland(),
            FloorEventType.EmberTide => PlayerInSafeRect(eventSafeRect),
            FloorEventType.EmberCage => PlayerInSafeRect(eventSafeRect),
            FloorEventType.EmberAltar => PlayerInEmberAltar(),
            FloorEventType.EmberRain or FloorEventType.EmberPulse or FloorEventType.EmberCross
                or FloorEventType.EmberBridge or FloorEventType.EmberFury or FloorEventType.EmberSnake
                or FloorEventType.EmberQuake or FloorEventType.EmberBloom => AiPlayerInSafeZoneInterior(),
            FloorEventType.CryptSeal => PlayerInCenterSnareSafe(),
            FloorEventType.CryptVeil => PlayerInSafeRect(eventSafeRect),
            FloorEventType.CryptTorch or FloorEventType.CryptLantern => AiPlayerInSafeZoneInterior(),
            FloorEventType.CryptGrave => TryGetTileUnder(playerPos, out int gx, out int gy)
                && !tiles[gx, gy].EventMarked && AiPlayerInSafeZoneInterior(),
            FloorEventType.CryptWail or FloorEventType.CryptChains or FloorEventType.CryptMist
                or FloorEventType.CryptGlimpse or FloorEventType.CryptRattle or FloorEventType.CryptEcho
                or FloorEventType.CryptShroud or FloorEventType.CryptTomb => AiPlayerInSafeZoneInterior(),
            FloorEventType.CrownEdict => PlayerInSafeRect(eventSafeRect),
            FloorEventType.CrownThrone => PlayerInSafeRect(eventSafeRect),
            FloorEventType.CrownIsles => PlayerOnSafeIsland(),
            FloorEventType.CrownCoronation => PlayerInCrownCenter(2),
            FloorEventType.CrownBolt => TryGetTileUnder(playerPos, out int cx, out int cy)
                && !tiles[cx, cy].EventMarked && AiPlayerInSafeZoneInterior(),
            FloorEventType.CrownTrial or FloorEventType.CrownFall or FloorEventType.CrownShard
                or FloorEventType.CrownRing or FloorEventType.CrownUsurpation or FloorEventType.CrownReckoning
                or FloorEventType.CrownStorm => AiPlayerInSafeZoneInterior(),
            FloorEventType.None => false,
            _ => false,
        };
    }

    static Vector2 DirectionToSafeRotHalf()
    {
        float targetX = floorRotSide < 0.5f ? WindowWidth * 0.72f : WindowWidth * 0.28f;
        Vector2 target = new Vector2(targetX, WindowHeight * 0.5f);
        return SafeNormalize(target - playerPos);
    }

    static void RebuildAiBoardIntel()
    {
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                aiTileValue[x, y] = ComputeAiTileValue(x, y);
                aiTileHazard[x, y] = ComputeAiTileHazard(x, y);
            }
        }
    }

    static bool AiIsTileTooDamaged(int x, int y)
    {
        if (x < 0 || x >= GridSize || y < 0 || y >= GridSize) return true;
        ref Tile tile = ref tiles[x, y];
        if (tile.State == 2 || tile.EventMarked) return true;
        return tile.Durability < MaxDurability * AiMaxWalkDurabilityRatio;
    }

    static bool AiIsTileSafeToWalk(int x, int y) => !AiIsTileTooDamaged(x, y);

    static float ComputeAiTileValue(int x, int y)
    {
        if (x < 0 || x >= GridSize || y < 0 || y >= GridSize) return -99999f;

        ref Tile tile = ref tiles[x, y];
        if (tile.State == 2) return -99999f;
        if (tile.EventMarked) return -600f;
        if (AiIsTileTooDamaged(x, y)) return -99999f;
        if (AiIsNearGridEdge(x, y)) return -99999f;

        float durRatio = tile.Durability / MaxDurability;
        float value = tile.Durability * 18f;
        if (durRatio >= AiHealthyTileRatio) value += 160f + (durRatio - AiHealthyTileRatio) * 320f;
        if (y < AiTopBandRows) value -= 700f + (AiTopBandRows - y) * 180f;
        if (tile.State == 0 && durRatio > 0.98f) value += 120f;
        else if (tile.State == 1) value *= 0.25f + durRatio * 0.45f;

        if (tile.RegrowTimer > 0f && tile.Collapse >= 1f) value *= 0.08f;

        int voidDist = AiVoidDistance(x, y);
        if (voidDist <= 1) return -99999f;
        if (voidDist == 2) value -= 1800f;
        else if (voidDist == 3) value -= 350f;
        else value += MathF.Min(voidDist, 6) * 22f;

        float localHealth = AiLocalDurabilityAvg(x, y, 1);
        value += localHealth * 1.1f;
        value += tile.Durability * 6f;
        if (localHealth >= MaxDurability * 0.92f) value += 120f;

        if (floorRotTimer > 0f)
        {
            bool left = x < GridSize / 2;
            bool rotting = floorRotSide < 0.5f ? left : !left;
            if (rotting) value *= 0.25f;
        }

        return value;
    }

    static float ComputeAiTileHazard(int x, int y)
    {
        if (x < 0 || x >= GridSize || y < 0 || y >= GridSize) return 1f;

        ref Tile tile = ref tiles[x, y];
        if (tile.State == 2) return 1f;
        if (tile.EventMarked) return 0.9f;
        if (AiIsTileTooDamaged(x, y)) return 1f;

        float durRatio = tile.Durability / MaxDurability;
        float hazard = MathF.Max(0f, 0.7f - durRatio) * 1.15f;

        if (tile.State == 1) hazard += 0.28f + (1f - durRatio) * 0.45f;
        if (tile.RegrowTimer > 0f && tile.Collapse >= 1f) hazard += 0.65f;
        if (tile.Collapse > 0f && tile.Collapse < 1f) hazard += tile.Collapse * 0.4f;

        hazard += AiVoidNeighborCount(x, y) * 0.35f;
        int voidDist = AiVoidDistance(x, y);
        if (voidDist <= 1) hazard += 0.55f;
        else if (voidDist == 2) hazard += 0.22f;
        hazard += AiWeakNeighborCount(x, y) * 0.08f;

        return Math.Clamp(hazard, 0f, 1f);
    }

    static float AiLocalDurabilityAvg(int cx, int cy, int radius)
    {
        float sum = 0f;
        int count = 0;
        for (int y = cy - radius; y <= cy + radius; y++)
        {
            for (int x = cx - radius; x <= cx + radius; x++)
            {
                if (x < 0 || x >= GridSize || y < 0 || y >= GridSize) continue;
                ref Tile t = ref tiles[x, y];
                if (t.State == 2)
                {
                    sum -= 30f;
                }
                else
                {
                    sum += t.Durability;
                    if (t.State == 1) sum -= 20f;
                    if (t.EventMarked) sum -= 50f;
                }
                count++;
            }
        }
        return count > 0 ? sum / count : 0f;
    }

    static int AiVoidNeighborCount(int cx, int cy)
    {
        int voids = 0;
        for (int y = cy - 1; y <= cy + 1; y++)
        {
            for (int x = cx - 1; x <= cx + 1; x++)
            {
                if (x < 0 || x >= GridSize || y < 0 || y >= GridSize) continue;
                if (tiles[x, y].State == 2) voids++;
            }
        }
        return voids;
    }

    static int AiVoidDistance(int cx, int cy)
    {
        if (cx < 0 || cx >= GridSize || cy < 0 || cy >= GridSize) return 0;
        if (tiles[cx, cy].State == 2) return 0;

        for (int dist = 1; dist <= 4; dist++)
        {
            for (int y = cy - dist; y <= cy + dist; y++)
            {
                for (int x = cx - dist; x <= cx + dist; x++)
                {
                    if (x < 0 || x >= GridSize || y < 0 || y >= GridSize) continue;
                    if (Math.Max(Math.Abs(x - cx), Math.Abs(y - cy)) != dist) continue;
                    if (tiles[x, y].State == 2) return dist;
                }
            }
        }

        return 99;
    }

    static float AiVoidNeighborPenalty(int cx, int cy)
    {
        int d = AiVoidDistance(cx, cy);
        if (d <= 1) return 6f;
        if (d == 2) return 2.5f;
        return MathF.Max(0f, 3f - d * 0.4f);
    }

    static int AiWeakNeighborCount(int cx, int cy)
    {
        int weak = 0;
        for (int y = cy - 1; y <= cy + 1; y++)
        {
            for (int x = cx - 1; x <= cx + 1; x++)
            {
                if (x < 0 || x >= GridSize || y < 0 || y >= GridSize) continue;
                ref Tile t = ref tiles[x, y];
                if (t.State == 2 || t.EventMarked) weak++;
                else if (t.State == 1 || t.Durability < MaxDurability * AiMaxWalkDurabilityRatio) weak++;
            }
        }
        return weak;
    }

    static Vector2 ComputeGlobalBoardSteering(Vector2 from, bool inCombat)
    {
        Vector2 pull = Vector2.Zero;
        float reach = inCombat ? 300f : 520f;

        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                float value = aiTileValue[x, y];
                if (value < 80f) continue;

                Vector2 tilePos = TileCenter(x, y);
                Vector2 delta = tilePos - from;
                float dist = delta.Length();
                if (dist < 12f) continue;

                float falloff = value / (1f + dist * dist * 0.00032f);
                if (dist > reach) falloff *= reach / dist;
                pull += delta * falloff;
            }
        }

        if (pull.LengthSquared() < 0.001f) return Vector2.Zero;
        return SafeNormalize(pull) * MathF.Min(pull.Length() * 0.007f, 1.35f);
    }

    static Vector2 ComputeGlobalHazardRepulsion(Vector2 from)
    {
        Vector2 push = Vector2.Zero;

        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                float hazard = aiTileHazard[x, y];
                if (hazard < 0.12f) continue;

                Vector2 tilePos = TileCenter(x, y);
                Vector2 away = from - tilePos;
                float dist = away.Length();
                if (dist < 6f) dist = 6f;

                float weight = hazard * hazard * 22000f / (dist * dist + 90f);
                push += (dist > 0.01f ? away / dist : Vector2.UnitY) * weight;
            }
        }

        if (push.LengthSquared() < 0.001f) return Vector2.Zero;
        return SafeNormalize(push) * MathF.Min(push.Length() * 0.006f, 1.5f);
    }

    static Vector2 FindGlobalBestTileAnchor(Vector2 from, bool inCombat)
    {
        float bestScore = float.NegativeInfinity;
        Vector2 best = from;

        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                float value = aiTileValue[x, y];
                float hazard = aiTileHazard[x, y];
                if (value < -500f) continue;
                if (AiIsTileTooDamaged(x, y)) continue;
                if (AiVoidDistance(x, y) <= 2) continue;

                Vector2 tilePos = TileCenter(x, y);
                float dist = Vector2.Distance(from, tilePos);
                float health = tiles[x, y].Durability;
                float score = health * 32f + value * 0.45f - hazard * 160f - dist * (inCombat ? 0.12f : 0.06f);
                score -= AiEnemyLanePenalty(from, tilePos) * (inCombat ? 0.65f : 0.25f);
                score += AiRouteDurability(from, tilePos) * 0.4f;

                if (score > bestScore)
                {
                    bestScore = score;
                    best = tilePos;
                }
            }
        }

        return best;
    }

    static float AiEnemyLanePenalty(Vector2 from, Vector2 to)
    {
        float penalty = 0f;
        Vector2 seg = to - from;
        float segLen = seg.Length();
        if (segLen < 1f) return 0f;
        Vector2 dir = seg / segLen;

        foreach (Enemy e in enemies)
        {
            if (e.Dead || e.Spawn < 0.5f) continue;

            Vector2 rel = e.Position - from;
            float t = Vector2.Dot(rel, dir);
            if (t < 0f || t > segLen) continue;

            Vector2 closest = from + dir * t;
            float lateral = Vector2.Distance(e.Position, closest);
            if (lateral < 70f) penalty += (70f - lateral) * (GetDef(e.Type).Boss ? 1.4f : 1f);
        }

        return penalty;
    }

    static float AiRouteDurability(Vector2 from, Vector2 to)
    {
        float sum = 0f;
        int steps = 5;
        for (int i = 1; i <= steps; i++)
        {
            Vector2 p = Vector2.Lerp(from, to, i / (float)steps);
            if (!TryGetTileUnder(p, out int tx, out int ty)) continue;
            sum += aiTileValue[tx, ty];
            sum -= aiTileHazard[tx, ty] * 60f;
        }
        return sum / steps;
    }

    static Vector2 RefineAiDirection(Vector2 desired, Vector2 from, bool inCombat)
    {
        float baseAngle = MathF.Atan2(desired.Y, desired.X);
        float bestScore = float.NegativeInfinity;
        Vector2 best = desired;
        float probeDist = inCombat ? 72f : 98f;
        float urgency = AiSteeringUrgency();
        int sweep = urgency > 0.75f ? 5 : 3;
        float angleStep = urgency > 0.75f ? MathF.PI / 18f : MathF.PI / 28f;
        Vector2 stick = lastMoveDirection.LengthSquared() > 0.01f ? SafeNormalize(lastMoveDirection) : desired;

        for (int i = -sweep; i <= sweep; i++)
        {
            float angle = baseAngle + i * angleStep;
            Vector2 dir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            Vector2 probe = from + dir * probeDist;
            float score = AiScoreProbePosition(probe, from);
            if (urgency < 0.8f) score += Vector2.Dot(dir, stick) * 55f;
            if (score > bestScore)
            {
                bestScore = score;
                best = dir;
            }
        }

        return best;
    }

    static float AiScoreProbePosition(Vector2 probe, Vector2 from)
    {
        if (!TryGetTileUnder(probe, out int tx, out int ty)) return -100000f;
        if (tiles[tx, ty].State == 2) return -100000f;
        if (AiIsTileTooDamaged(tx, ty)) return -100000f;
        if (AiIsNearGridEdge(tx, ty)) return -100000f;
        if (activeEvent != FloorEventType.None && eventCountdown > 0f)
        {
            if (tiles[tx, ty].EventMarked || !IsTileSafeForEvent(tx, ty)) return -100000f;
        }

        int voidDist = AiVoidDistance(tx, ty);
        if (voidDist <= 1) return -100000f;

        float score = aiTileValue[tx, ty];
        if (activeEvent != FloorEventType.None && eventCountdown > 0f)
        {
            score += AiCountSafeCardinalNeighbors(tx, ty) * 130f;
        }
        score += tiles[tx, ty].Durability * 14f;
        if (AiIsHealthyTile(tx, ty)) score += 420f;
        if (ty < AiTopBandRows) score -= 2200f + (AiTopBandRows - ty) * 500f;
        score -= aiTileHazard[tx, ty] * 220f;
        if (voidDist == 2) score -= 3500f;
        else if (voidDist == 3) score -= 600f;
        score += AiLocalDurabilityAvg(tx, ty, 1) * 0.75f;

        for (int step = 1; step <= 5; step++)
        {
            Vector2 p = Vector2.Lerp(from, probe, step / 5f);
            if (!TryGetTileUnder(p, out int sx, out int sy)) return -100000f;
            if (tiles[sx, sy].State == 2 || AiVoidDistance(sx, sy) <= 1) return -100000f;
            if (AiIsTileTooDamaged(sx, sy)) return -100000f;
            score += aiTileValue[sx, sy] * 0.18f;
            score -= aiTileHazard[sx, sy] * 55f;
        }

        score -= AiEnemyZonePenalty(probe);
        score -= NearestEnemyDistance(probe) * 0.08f;
        score -= NearestBossDistance(probe) * 0.18f;
        return score;
    }

    static float AiEnemyDangerRadiusPx() => AiEnemyDangerTiles * TileSize;

    static float AiEnemyZoneOuterRadius(Enemy e) => AiEnemyDangerRadiusPx() + e.Radius;

    static float AiEnemyZoneMargin(Vector2 pos, Enemy e)
        => Vector2.Distance(pos, e.Position) - AiEnemyZoneOuterRadius(e);

    static float AiNearestEnemyZoneMargin(Vector2 pos)
    {
        float best = float.MaxValue;
        foreach (Enemy e in enemies)
        {
            if (e.Dead || e.Spawn < 0.5f) continue;
            float margin = AiEnemyZoneMargin(pos, e);
            if (margin < best) best = margin;
        }
        return best;
    }

    static bool AiIsInsideEnemyDangerZone(Vector2 pos) => AiNearestEnemyZoneMargin(pos) < 0f;

    static float AiEnemyZonePenalty(Vector2 pos)
    {
        float penalty = 0f;
        float keepClear = TileSize * 0.75f;

        foreach (Enemy e in enemies)
        {
            if (e.Dead || e.Spawn < 0.5f) continue;

            float margin = AiEnemyZoneMargin(pos, e);
            if (margin >= keepClear) continue;

            var def = GetDef(e.Type);
            float w = def.Boss ? 3f : def.InstaCollapse ? 2.2f : 1f;

            if (margin < 0f)
            {
                penalty += (-margin + 8f) * 420f * w;
            }
            else
            {
                penalty += (1f - margin / keepClear) * 180f * w;
            }
        }

        return penalty;
    }

    static Vector2 ComputeUrgentEnemyFlee(Vector2 from)
    {
        Vector2 flee = Vector2.Zero;
        bool panic = false;
        float panicBuffer = TileSize * 0.5f;

        foreach (Enemy e in enemies)
        {
            if (e.Dead || e.Spawn < 0.5f) continue;

            Vector2 away = from - e.Position;
            float d = away.Length();
            Vector2 dir = d > 0.01f ? away / d : new Vector2(1f, 0f);

            float margin = AiEnemyZoneMargin(from, e);
            var def = GetDef(e.Type);
            float w = def.Boss ? 2.4f : def.InstaCollapse ? 1.7f : 1f;

            if (margin < 0f)
            {
                panic = true;
                flee += dir * (w * (3f + (-margin / TileSize) * 6f));
            }
            else if (margin < panicBuffer)
            {
                panic = true;
                flee += dir * (w * (2.2f + (1f - margin / panicBuffer)));
            }
        }

        if (!panic || flee.LengthSquared() < 0.001f) return Vector2.Zero;
        return SafeNormalize(flee);
    }

    static Vector2 ComputeEnemySteering(Vector2 from)
    {
        Vector2 steer = Vector2.Zero;
        float keepClear = TileSize * 1.25f;

        foreach (Enemy e in enemies)
        {
            if (e.Dead || e.Spawn < 0.5f) continue;

            Vector2 away = from - e.Position;
            float d = away.Length();
            if (d < 0.01f) continue;
            Vector2 dir = away / d;

            float margin = AiEnemyZoneMargin(from, e);
            var def = GetDef(e.Type);
            float w = def.Boss ? 2.6f : def.InstaCollapse ? 1.9f : 1f;
            float outer = AiEnemyZoneOuterRadius(e);

            if (margin < 0f)
            {
                steer += dir * (w * (5f + (-margin / TileSize) * 4f));
                continue;
            }

            float buffer = keepClear + e.Radius;
            if (d < outer + buffer)
            {
                float t = 1f - margin / (buffer + outer * 0.15f);
                t = Math.Clamp(t, 0f, 1f);
                steer += dir * (w * t * t * 2.2f);
            }
            else
            {
                steer += dir * (w * 95f / (d + 28f));
            }
        }

        if (steer.LengthSquared() < 0.001f) return Vector2.Zero;
        return SafeNormalize(steer) * MathF.Min(steer.Length(), 1.55f);
    }

    static bool IsFeetTileWeak()
    {
        if (!TryGetTileUnder(playerPos, out int tx, out int ty)) return true;
        if (AiIsTileTooDamaged(tx, ty)) return true;
        if (aiTileHazard[tx, ty] > 0.42f) return true;
        ref Tile feet = ref tiles[tx, ty];
        if (feet.State == 2 || feet.EventMarked) return true;
        return feet.Durability < MaxDurability * (AiMaxWalkDurabilityRatio + 0.12f);
    }

    static Vector2 SafeNormalize(Vector2 v)
    {
        float len = v.Length();
        return len > 0.001f ? v / len : Vector2.UnitY;
    }

    static Vector2 GetUrgentAiDirection()
    {
        if (activeEvent == FloorEventType.None || eventCountdown <= 0f)
        {
            return Vector2.Zero;
        }

        switch (activeEvent)
        {
            case FloorEventType.SafeZoneRush:
                if (AiPlayerDeepInSafeRushBand()) break;
                return DirectionToSafeZoneRushInterior();

            case FloorEventType.CenterSnare:
                if (!PlayerInCenterSnareSafe())
                {
                    return DirectionToNearestEdge();
                }
                break;

            case FloorEventType.MossRot:
                if (floorRotTimer > 0f && TryGetTileUnder(playerPos, out int rtx, out int rty))
                {
                    bool left = rtx < GridSize / 2;
                    bool onRotSide = floorRotSide < 0.5f ? left : !left;
                    if (onRotSide) return DirectionToSafeRotHalf();
                }
                break;

            case FloorEventType.CrimsonCrumble:
            case FloorEventType.Checkerfall:
            case FloorEventType.RingCollapse:
            case FloorEventType.StoneIslands:
            case FloorEventType.ScatterPits:
            case FloorEventType.BlightStorm:
                return DirectionToSafeZoneInterior(playerPos);

            case FloorEventType.TideSurge:
            case FloorEventType.TideRift:
            case FloorEventType.TideRecede:
            case FloorEventType.TideColumn:
            case FloorEventType.TideEcho:
            case FloorEventType.TideUndertow:
            case FloorEventType.TideCrest:
            case FloorEventType.TideWall:
            case FloorEventType.TideAnchor:
            case FloorEventType.TideFoam:
                return DirectionToSafeZoneInterior(playerPos);

            case FloorEventType.TideWhirlpool:
                if (PlayerInTideWhirlpoolDanger()) return DirectionAwayFromArenaCenter();
                break;

            case FloorEventType.TideBeacon:
                if (!PlayerInSafeRect(eventSafeRect))
                {
                    Vector2 beacon = new Vector2(
                        eventSafeRect.X + eventSafeRect.Width * 0.5f,
                        eventSafeRect.Y + eventSafeRect.Height * 0.5f);
                    return SafeNormalize(beacon - playerPos);
                }
                break;

            case FloorEventType.TideStrike:
                if (TryGetTileUnder(playerPos, out int tx, out int ty) && tiles[tx, ty].EventMarked)
                {
                    return DirectionToSafeZoneInterior(playerPos);
                }
                if (eventCountdown < 2.2f) return DirectionToSafeZoneInterior(playerPos);
                break;

            case FloorEventType.MarkedStrike:
                if (TryGetTileUnder(playerPos, out int mx, out int my) && tiles[mx, my].EventMarked)
                {
                    return DirectionToSafeZoneInterior(playerPos);
                }
                if (eventCountdown < 2.4f)
                {
                    return DirectionToSafeZoneInterior(playerPos);
                }
                break;

            case FloorEventType.EmberGate:
                if (!PlayerInSafeRect(eventSafeRect))
                {
                    Vector2 gateCenter = new Vector2(
                        eventSafeRect.X + eventSafeRect.Width * 0.5f,
                        eventSafeRect.Y + eventSafeRect.Height * 0.5f);
                    return SafeNormalize(gateCenter - playerPos);
                }
                break;

            case FloorEventType.EmberTide:
                if (!PlayerInSafeRect(eventSafeRect))
                {
                    Vector2 safeCenter = new Vector2(
                        eventSafeRect.X + eventSafeRect.Width * 0.5f,
                        eventSafeRect.Y + eventSafeRect.Height * 0.5f);
                    return SafeNormalize(safeCenter - playerPos);
                }
                break;

            case FloorEventType.EmberCage:
                if (!PlayerInSafeRect(eventSafeRect))
                {
                    Vector2 cageCenter = new Vector2(WindowWidth / 2f, WindowHeight / 2f);
                    return SafeNormalize(cageCenter - playerPos);
                }
                break;

            case FloorEventType.EmberAltar:
                if (!PlayerInEmberAltar())
                {
                    return SafeNormalize(eventEpicenter - playerPos);
                }
                break;

            case FloorEventType.EmberFury:
                if (emberFuryStandTimer > EmberFuryStandTime * 0.5f)
                {
                    return lastMoveDirection.LengthSquared() > 0.01f
                        ? SafeNormalize(lastMoveDirection)
                        : DirectionToSafeZoneInterior(playerPos);
                }
                break;

            case FloorEventType.EmberRain:
            case FloorEventType.EmberPulse:
            case FloorEventType.EmberCross:
            case FloorEventType.EmberBridge:
            case FloorEventType.EmberHive:
            case FloorEventType.EmberSnake:
            case FloorEventType.EmberQuake:
            case FloorEventType.EmberBloom:
                return DirectionToSafeZoneInterior(playerPos);

            case FloorEventType.CryptSeal:
                if (!PlayerInCenterSnareSafe()) return DirectionToNearestEdge();
                break;

            case FloorEventType.CryptVeil:
                if (!PlayerInSafeRect(eventSafeRect))
                {
                    Vector2 veilCenter = new Vector2(
                        eventSafeRect.X + eventSafeRect.Width * 0.5f,
                        eventSafeRect.Y + eventSafeRect.Height * 0.5f);
                    return SafeNormalize(veilCenter - playerPos);
                }
                break;

            case FloorEventType.CryptTomb:
                if (TryGetTileUnder(playerPos, out int ttx, out int tty) && tiles[ttx, tty].EventMarked)
                {
                    return DirectionToSafeZoneInterior(playerPos);
                }
                break;

            case FloorEventType.CryptGrave:
                if (TryGetTileUnder(playerPos, out int gx, out int gy) && tiles[gx, gy].EventMarked)
                {
                    return DirectionToSafeZoneInterior(playerPos);
                }
                if (eventCountdown < 2f) return DirectionToSafeZoneInterior(playerPos);
                break;

            case FloorEventType.CryptTorch:
            case FloorEventType.CryptLantern:
            case FloorEventType.CryptWail:
            case FloorEventType.CryptChains:
            case FloorEventType.CryptGlimpse:
            case FloorEventType.CryptRattle:
            case FloorEventType.CryptEcho:
            case FloorEventType.CryptShroud:
            case FloorEventType.CryptMist:
                return DirectionToSafeZoneInterior(playerPos);

            case FloorEventType.CrownEdict:
                if (!PlayerInSafeRect(eventSafeRect)) return DirectionToSafeZoneRushInterior();
                break;

            case FloorEventType.CrownThrone:
                if (!PlayerInSafeRect(eventSafeRect)) return DirectionToArenaCenter();
                break;

            case FloorEventType.CrownCoronation:
                if (!PlayerInCrownCenter(2)) return DirectionToArenaCenter();
                break;

            case FloorEventType.CrownRot:
                if (floorRotTimer > 0f && TryGetTileUnder(playerPos, out int crx, out int cry))
                {
                    bool left = crx < GridSize / 2;
                    bool onRotSide = floorRotSide < 0.5f ? left : !left;
                    if (onRotSide) return DirectionToSafeRotHalf();
                }
                break;

            case FloorEventType.CrownBolt:
                if (TryGetTileUnder(playerPos, out int cbx, out int cby) && tiles[cbx, cby].EventMarked)
                {
                    return DirectionToSafeZoneInterior(playerPos);
                }
                if (eventCountdown < 2.4f) return DirectionToSafeZoneInterior(playerPos);
                break;

            case FloorEventType.CrownTrial:
            case FloorEventType.CrownFall:
            case FloorEventType.CrownShard:
            case FloorEventType.CrownRing:
            case FloorEventType.CrownIsles:
            case FloorEventType.CrownUsurpation:
            case FloorEventType.CrownReckoning:
            case FloorEventType.CrownStorm:
                return DirectionToSafeZoneInterior(playerPos);

            case FloorEventType.None:
                break;

            default:
                throw new UnreachableException();
        }

        return Vector2.Zero;
    }

    static Vector2 DirectionToNearestEdge()
    {
        Vector2 push = Vector2.Zero;
        float m = CenterSnareMarginPx() + 50f;

        if (playerPos.X < m) push.X -= 1f - playerPos.X / m;
        if (playerPos.X > WindowWidth - m) push.X += (playerPos.X - (WindowWidth - m)) / m;
        if (playerPos.Y < m) push.Y -= 1f - playerPos.Y / m;
        if (playerPos.Y > WindowHeight - m) push.Y += (playerPos.Y - (WindowHeight - m)) / m;

        return push.LengthSquared() > 0.001f ? SafeNormalize(push) : Vector2.UnitY;
    }

    static float NearestEnemyDistance(Vector2 from)
    {
        float best = float.MaxValue;
        foreach (Enemy e in enemies)
        {
            if (e.Dead || e.Spawn < 0.5f) continue;
            float d = Vector2.Distance(from, e.Position);
            if (d < best) best = d;
        }
        return best;
    }

}
