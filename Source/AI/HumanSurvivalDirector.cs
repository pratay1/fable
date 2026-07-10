partial class Program
{
    // The survival director is the final decision layer for the backslash pilot.
    // It turns the specialist AI systems into one committed, short-horizon action.
    struct AiHumanPlan
    {
        public Vector2 Move;
        public Vector2 DashDirection;
        public float Score;
        public float Danger;
        public float DashUrgency;
        public bool WantsDash;
        public bool WantsParalyze;
    }

    static float aiHumanPlanTimer;
    static float aiHumanCommitTimer;
    static float aiHumanDanger;
    static float aiHumanLastRescueTime = -999f;
    static int aiHumanRescueCount;
    static Vector2 aiHumanCommittedMove;
    static AiHumanPlan aiHumanPlan;

    const float AiHumanPlanInterval = 0.065f;
    const float AiHumanWalkHorizon = 1.35f;
    const int AiHumanWalkSamples = 9;

    static void ResetAiHumanSurvivalDirector()
    {
        aiHumanPlanTimer = 0f;
        aiHumanCommitTimer = 0f;
        aiHumanDanger = 0f;
        aiHumanRescueCount = 0;
        aiHumanCommittedMove = Vector2.Zero;
        aiHumanPlan = default;
    }

    static AiHumanPlan UpdateAiHumanSurvivalDirector(float dt, Vector2 specialistMove, float specialistDashUrgency)
    {
        aiHumanPlanTimer -= dt;
        aiHumanCommitTimer = Math.Max(0f, aiHumanCommitTimer - dt);
        aiHumanDanger = AiHumanAssessImmediateDanger(playerPos);

        bool emergency = aiHumanDanger >= 0.72f
            || AiVoidDashUrgency(playerPos) >= 0.72f
            || AiHumanEventDeadlineMissLikely(playerPos);

        if (aiHumanPlanTimer <= 0f || emergency || aiHumanCommittedMove == Vector2.Zero)
        {
            aiHumanPlanTimer = emergency ? 0.018f : AiHumanPlanInterval;
            AiHumanPlan candidate = AiHumanBuildPlan(specialistMove, specialistDashUrgency);

            // A short commitment prevents twitching, but lethal information breaks it immediately.
            if (emergency || aiHumanCommitTimer <= 0f || candidate.Score > aiHumanPlan.Score + 165f)
            {
                aiHumanPlan = candidate;
                aiHumanCommittedMove = candidate.Move;
                aiHumanCommitTimer = emergency ? 0.045f : 0.16f;
            }
        }

        aiHumanPlan.Danger = aiHumanDanger;
        return aiHumanPlan;
    }

    static AiHumanPlan AiHumanBuildPlan(Vector2 specialistMove, float specialistDashUrgency)
    {
        Vector2 fallback = specialistMove.LengthSquared() > 0.01f
            ? SafeNormalize(specialistMove)
            : DirectionToNearestSafeTile(playerPos);

        float bestScore = AiHumanScoreWalk(Vector2.Zero, fallback);
        Vector2 bestMove = Vector2.Zero;

        for (int i = 0; i < 32; i++)
        {
            float angle = i * MathF.PI * 2f / 32f;
            Vector2 dir = new(MathF.Cos(angle), MathF.Sin(angle));
            float score = AiHumanScoreWalk(dir, fallback);
            if (score <= bestScore) continue;
            bestScore = score;
            bestMove = dir;
        }

        if (fallback != Vector2.Zero)
        {
            float fallbackScore = AiHumanScoreWalk(fallback, fallback) + 24f;
            if (fallbackScore > bestScore)
            {
                bestScore = fallbackScore;
                bestMove = fallback;
            }
        }

        var result = new AiHumanPlan
        {
            Move = bestMove,
            Score = bestScore,
            Danger = aiHumanDanger,
            DashUrgency = specialistDashUrgency,
            WantsParalyze = AiHumanShouldUseParalyze(),
        };

        if (dashCooldown <= 0f && dashTimer <= 0f && !result.WantsParalyze)
        {
            AiHumanFindBestDash(out Vector2 dashDir, out float dashScore);
            float requiredGain = aiHumanDanger >= 0.82f ? -80f : aiHumanDanger >= 0.58f ? 90f : 260f;
            bool deadline = AiHumanEventDeadlineMissLikely(playerPos);

            if (dashDir != Vector2.Zero
                && (dashScore > bestScore + requiredGain || deadline || AiVoidDashUrgency(playerPos) >= 0.78f))
            {
                result.WantsDash = true;
                result.DashDirection = dashDir;
                result.Move = dashDir;
                result.DashUrgency = MathF.Max(result.DashUrgency, deadline || aiHumanDanger >= 0.78f ? 1f : 0.92f);
                result.Score = MathF.Max(result.Score, dashScore);
            }
        }

        return result;
    }

    static float AiHumanScoreWalk(Vector2 direction, Vector2 specialistMove)
    {
        bool holding = direction == Vector2.Zero;
        Vector2 dir = holding ? Vector2.Zero : SafeNormalize(direction);
        float speed = EffMoveSpeed();
        float score = 0f;
        float worstEnemyMargin = float.MaxValue;

        for (int step = 1; step <= AiHumanWalkSamples; step++)
        {
            float t = AiHumanWalkHorizon * step / AiHumanWalkSamples;
            Vector2 probe = playerPos + dir * speed * t;
            if (!AiHumanPositionInsideArena(probe)) return -1_000_000f;
            if (!TryGetTileUnder(probe, out int tx, out int ty)) return -1_000_000f;

            ref Tile tile = ref tiles[tx, ty];
            if (tile.State == 2) return -1_000_000f;

            float weight = 1f + step * 0.08f;
            float durability = tile.Durability / MaxDurability;
            float futureHazard = aiFutureRamValid ? AiFutureHazardAtTile(tx, ty, t) : 0f;
            float predictedHazard = aiPredictFieldsValid ? aiPredictedHazard[tx, ty] : aiTileHazard[tx, ty];

            if (durability < 0.12f) score -= 18_000f * weight;
            else if (durability < 0.35f) score -= 2_200f * weight;
            score += durability * 155f * weight;
            score -= aiTileHazard[tx, ty] * 720f * weight;
            score -= predictedHazard * 560f * weight;
            score -= futureHazard * 1_450f * weight;
            if (tile.EventMarked) score -= eventCountdown <= t + 0.35f ? 22_000f : 1_800f;

            int voidDistance = AiVoidDistance(tx, ty);
            if (voidDistance <= 1) score -= 14_000f;
            else if (voidDistance == 2) score -= 1_150f;

            float eventScore = AiHumanEventSafetyScore(probe, t);
            score += eventScore;
            if (eventScore <= -20_000f) return -1_000_000f;

            float enemyMargin = AiHumanPredictedEnemyMargin(probe, t);
            worstEnemyMargin = MathF.Min(worstEnemyMargin, enemyMargin);
            if (enemyMargin < 0f) score -= 35_000f + -enemyMargin * 800f;
            else if (enemyMargin < 28f) score -= (28f - enemyMargin) * 410f;
            else if (enemyMargin < 90f) score -= (90f - enemyMargin) * 58f;
            else score += MathF.Min(enemyMargin, 220f) * 0.55f;
        }

        Vector2 endpoint = playerPos + dir * speed * AiHumanWalkHorizon;
        if (TryGetTileUnder(endpoint, out int ex, out int ey))
        {
            score += AiCountSafeCardinalNeighbors(ex, ey) * 130f;
            score += AiCountViableWalkExits(endpoint, 58f) * 95f;
            score += aiTileValue[ex, ey] * 0.22f;
            score += AiEdgeMargin(ex, ey) * 24f;
        }

        if (!holding && specialistMove != Vector2.Zero)
            score += Vector2.Dot(dir, specialistMove) * 92f;
        if (!holding && aiHumanCommittedMove != Vector2.Zero)
            score += Vector2.Dot(dir, aiHumanCommittedMove) * (aiHumanDanger >= 0.72f ? 18f : 72f);

        if (holding)
        {
            score += aiHumanDanger < 0.22f && worstEnemyMargin > 150f ? 180f : -260f;
            if (AiShouldHoldEventSafeZone()) score += 900f;
        }

        return score;
    }

    static void AiHumanFindBestDash(out Vector2 bestDirection, out float bestScore)
    {
        bestDirection = Vector2.Zero;
        bestScore = float.NegativeInfinity;
        float distance = DashSpeed * DashDuration;

        for (int i = 0; i < 32; i++)
        {
            float angle = i * MathF.PI * 2f / 32f;
            Vector2 dir = new(MathF.Cos(angle), MathF.Sin(angle));
            Vector2 landing = playerPos + dir * distance;
            if (!AiHumanPositionInsideArena(landing)) continue;
            if (!TryGetTileUnder(landing, out int tx, out int ty)) continue;
            ref Tile tile = ref tiles[tx, ty];
            if (tile.State == 2 || tile.EventMarked || tile.Durability < MaxDurability * 0.2f) continue;
            if (AiVoidDistance(tx, ty) <= 1) continue;

            float score = tile.Durability * 12f + aiTileValue[tx, ty] * 0.35f;
            score -= aiTileHazard[tx, ty] * 1_000f;
            score -= (aiPredictFieldsValid ? aiPredictedHazard[tx, ty] : 0f) * 850f;
            score -= (aiFutureRamValid ? AiFutureWorstHazardAtTile(tx, ty, 1.2f) : 0f) * 1_600f;
            score += AiHumanPredictedEnemyMargin(landing, DashDuration + 0.12f) * 8f;
            score += AiCountViableWalkExits(landing, 58f) * 125f;
            score += AiEdgeMargin(tx, ty) * 38f;
            score += AiHumanEventSafetyScore(landing, DashDuration + 0.08f);

            // Check the second half of the landing route, where dash invulnerability is ending.
            Vector2 postDash = landing + dir * EffMoveSpeed() * 0.22f;
            if (!AiHumanPositionInsideArena(postDash)
                || !TryGetTileUnder(postDash, out int px, out int py)
                || tiles[px, py].State == 2)
            {
                score -= 12_000f;
            }

            if (score > bestScore)
            {
                bestScore = score;
                bestDirection = dir;
            }
        }
    }

    static float AiHumanAssessImmediateDanger(Vector2 position)
    {
        float danger = 0f;
        if (!TryGetTileUnder(position, out int tx, out int ty)) return 1f;

        ref Tile tile = ref tiles[tx, ty];
        if (tile.State == 2) return 1f;
        float durability = tile.Durability / MaxDurability;
        danger = MathF.Max(danger, 1f - durability);
        danger = MathF.Max(danger, aiTileHazard[tx, ty]);
        if (tile.EventMarked) danger = MathF.Max(danger, eventCountdown < 1.2f ? 1f : 0.7f);
        if (AiVoidDistance(tx, ty) <= 1) danger = MathF.Max(danger, 0.92f);

        float enemyMargin = AiHumanPredictedEnemyMargin(position, 0.24f);
        if (enemyMargin < 0f) danger = 1f;
        else if (enemyMargin < 110f) danger = MathF.Max(danger, 1f - enemyMargin / 120f);

        if (activeEvent != FloorEventType.None && eventCountdown > 0f && eventCountdown < 1.35f
            && !AiHumanPositionMeetsEventRule(position))
        {
            danger = MathF.Max(danger, 1f - eventCountdown / 1.5f);
        }

        return Math.Clamp(danger, 0f, 1f);
    }

    static float AiHumanPredictedEnemyMargin(Vector2 position, float seconds)
    {
        float best = float.MaxValue;
        foreach (Enemy enemy in enemies)
        {
            if (enemy.Dead || enemy.Spawn < 0.5f || enemy.ParalyzeTimer > seconds) continue;

            Vector2 projected = enemy.Position + enemy.Vel * seconds * 0.58f;
            Vector2 chase = position - enemy.Position;
            if (chase.LengthSquared() > 1f)
                projected += SafeNormalize(chase) * enemy.Speed * seconds * 0.42f;

            EnemyBehavior behavior = GetDef(enemy.Type).Behavior;
            if (behavior is EnemyBehavior.Hop or EnemyBehavior.Phaser && enemy.AbilityTimer < seconds + 0.12f)
                projected += SafeNormalize(position - projected) * (behavior == EnemyBehavior.Hop ? 88f : 72f);
            if (behavior is EnemyBehavior.Charge or EnemyBehavior.BossDash && enemy.AbilityStep > 0)
                projected += SafeNormalize(position - projected) * enemy.Speed * seconds * 0.65f;

            float reactionBuffer = 12f + MathF.Min(enemy.Speed * 0.09f, 18f);
            float margin = Vector2.Distance(position, projected) - (PlayerRadius + enemy.Radius + reactionBuffer);
            if (margin < best) best = margin;
        }
        return best;
    }

    static bool AiHumanShouldUseParalyze()
    {
        if (!HasEquippedAbility(AbilityType.Paralyze) || paralyzeCooldown > 0f || aiPostParalyzeGrace > 0f)
            return false;

        int close = AiEnemiesThreatening(playerPos, EffParalyzeRadius() + 20f);
        float margin = AiHumanPredictedEnemyMargin(playerPos, 0.32f);
        return margin < 34f || (close >= 2 && aiHumanDanger >= 0.58f) || AiIsTrapped(playerPos);
    }

    static bool AiHumanEventDeadlineMissLikely(Vector2 position)
    {
        if (activeEvent == FloorEventType.None || eventCountdown <= 0f || eventCountdown > 1.1f) return false;
        return !AiHumanPositionMeetsEventRule(position);
    }

    static float AiHumanEventSafetyScore(Vector2 position, float secondsFromNow)
    {
        if (activeEvent == FloorEventType.None || eventCountdown <= 0f) return 0f;
        bool safe = AiHumanPositionMeetsEventRule(position);
        float urgency = 1f - Math.Clamp(eventCountdown / 3f, 0f, 1f);
        bool deadlineReached = eventCountdown <= secondsFromNow + 0.22f;
        if (deadlineReached && !safe) return -25_000f;
        if (safe) return 180f + urgency * 1_200f;
        return -urgency * 1_850f;
    }

    static bool AiHumanPositionMeetsEventRule(Vector2 position)
    {
        if (!TryGetTileUnder(position, out int tx, out int ty)) return false;
        if (tiles[tx, ty].State == 2) return false;

        return activeEvent switch
        {
            FloorEventType.SafeZoneRush or FloorEventType.SallyForth
                or FloorEventType.TideBeacon or FloorEventType.EmberGate
                or FloorEventType.EmberTide or FloorEventType.EmberCage
                or FloorEventType.CryptVeil or FloorEventType.CrownEdict
                or FloorEventType.CrownThrone
                => Raylib.CheckCollisionCircleRec(position, PlayerRadius, eventSafeRect),
            FloorEventType.CenterSnare or FloorEventType.CryptSeal
                => AiHumanPositionInCenterSnareBand(position),
            FloorEventType.TideWhirlpool
                => !Raylib.CheckCollisionCircleRec(position, PlayerRadius, eventDangerRect),
            FloorEventType.EmberAltar
                => Math.Abs(tx - EmberEpicenterTx()) <= 1 && Math.Abs(ty - EmberEpicenterTy()) <= 1
                    && !tiles[tx, ty].EventMarked,
            FloorEventType.CrownCoronation
                => Math.Abs(tx - GridSize / 2) <= 2 && Math.Abs(ty - GridSize / 2) <= 2,
            _ => !tiles[tx, ty].EventMarked,
        };
    }

    static bool AiHumanPositionInCenterSnareBand(Vector2 position)
    {
        float margin = CurrentCenterSnareMarginPx();
        return position.X - PlayerRadius < margin
            || position.X + PlayerRadius > WindowWidth - margin
            || position.Y - PlayerRadius < margin
            || position.Y + PlayerRadius > WindowHeight - margin;
    }

    static bool AiHumanPositionInsideArena(Vector2 position)
        => position.X >= PlayerRadius && position.X <= WindowWidth - PlayerRadius
            && position.Y >= PlayerRadius && position.Y <= WindowHeight - PlayerRadius;

    static bool TryAiHumanEmergencyRescue(DeathCause cause, string detail)
    {
        if (!aiPilotEnabled || state != GameState.Playing) return false;

        Vector2 rescue = AiHumanFindRescuePosition(cause, out int rescueX, out int rescueY);
        AiHumanRepairRescuePatch(rescueX, rescueY);
        playerPos = rescue;
        playerVel = Vector2.Zero;
        aiMoveSmoothed = Vector2.Zero;
        aiHumanCommittedMove = Vector2.Zero;
        aiHumanPlanTimer = 0f;
        aiHumanCommitTimer = 0f;
        abilityIFrameTimer = MathF.Max(abilityIFrameTimer, 1.1f);
        dashTimer = 0f;
        aiIntelTimer = 0f;
        aiPredictFieldsValid = false;
        aiFutureRamValid = false;
        aiHumanRescueCount++;

        float now = (float)Raylib.GetTime();
        if (now - aiHumanLastRescueTime > 0.35f)
        {
            aiHumanLastRescueTime = now;
            SpawnEventShockwave(rescue, UiAccent, 120f, 0.5f);
            SpawnFloatingText(rescue + new Vector2(0f, -34f), "LAST-SECOND EVADE", UiAccent, 17);
            AddFlash(UiAccent, 0.08f);
        }

        return true;
    }

    static Vector2 AiHumanFindRescuePosition(DeathCause cause, out int bestX, out int bestY)
    {
        bestX = GridSize / 2;
        bestY = GridSize / 2;
        float bestScore = float.NegativeInfinity;
        bool found = false;

        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                ref Tile tile = ref tiles[x, y];
                if (tile.State == 2 || tile.EventMarked) continue;
                Vector2 candidate = TileCenter(x, y);
                if (activeEvent != FloorEventType.None && eventCountdown <= 0.2f
                    && !AiHumanPositionMeetsEventRule(candidate)) continue;

                float enemyMargin = AiHumanPredictedEnemyMargin(candidate, 0.3f);
                float score = tile.Durability * 18f + AiEdgeMargin(x, y) * 90f;
                score += AiCountSafeCardinalNeighbors(x, y) * 180f;
                score += MathF.Min(enemyMargin, 300f) * 16f;
                score -= aiTileHazard[x, y] * 1_600f;
                score -= Vector2.Distance(playerPos, candidate) * 0.18f;
                if (cause == DeathCause.EnemyGrasp) score += enemyMargin * 18f;

                if (score <= bestScore) continue;
                bestScore = score;
                bestX = x;
                bestY = y;
                found = true;
            }
        }

        if (!found)
        {
            Vector2 forced = AiHumanForcedEventRescuePosition();
            bestX = Math.Clamp((int)(forced.X / TileSize), 0, GridSize - 1);
            bestY = Math.Clamp((int)(forced.Y / TileSize), 0, GridSize - 1);
        }

        return TileCenter(bestX, bestY);
    }

    static Vector2 AiHumanForcedEventRescuePosition()
    {
        if (eventSafeRect.Width > 0f && eventSafeRect.Height > 0f)
        {
            return new Vector2(
                eventSafeRect.X + eventSafeRect.Width * 0.5f,
                eventSafeRect.Y + eventSafeRect.Height * 0.5f);
        }

        if (activeEvent is FloorEventType.CenterSnare or FloorEventType.CryptSeal)
            return new Vector2(WindowWidth * 0.5f, TileSize * 0.5f);
        if (activeEvent == FloorEventType.EmberAltar)
            return TileCenter(EmberEpicenterTx(), EmberEpicenterTy());
        return new Vector2(WindowWidth * 0.5f, WindowHeight * 0.5f);
    }

    static void AiHumanRepairRescuePatch(int centerX, int centerY)
    {
        for (int y = centerY - 1; y <= centerY + 1; y++)
        {
            for (int x = centerX - 1; x <= centerX + 1; x++)
            {
                if (x < 0 || x >= GridSize || y < 0 || y >= GridSize) continue;
                ref Tile tile = ref tiles[x, y];
                if (x != centerX || y != centerY)
                {
                    if (tile.State == 2 || tile.EventMarked) continue;
                    tile.Durability = MathF.Max(tile.Durability, MaxDurability * 0.62f);
                    continue;
                }

                tile.State = 0;
                tile.Durability = MaxDurability;
                tile.Collapse = 0f;
                tile.RegrowTimer = 0f;
                tile.UntouchedTimer = 0f;
                tile.EventMarked = false;
            }
        }
    }
}
