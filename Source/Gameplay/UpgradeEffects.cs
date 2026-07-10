partial class Program
{
    // ---------------------------------------------------------------- Upgrade effects

    static float EffMoveSpeed() => PlayerSpeed * (1f + upgradeLevels[UpSwift] * 0.05f) * BlessingMoveMult();
    static float EffDashCooldown() => MathF.Max(0.45f, DashCooldown * (1f - upgradeLevels[UpDash] * 0.08f));
    static float FableMult()
    {
        float mult = 1f + upgradeLevels[UpFortune] * 0.15f;
        if (autoFire) mult *= 0.75f;
        mult *= BlessingFableMult() * OathRewardMult();
        return mult;
    }
    static float EffPlayerDecay() => PlayerDurabilityDecayRate * (1f - upgradeLevels[UpFeet] * 0.08f);

    static float CombatFireRateMult()
    {
        float rate = 1.12f + upgradeLevels[UpRapid] * 0.14f;
        if (runDifficulty == Difficulty.FableNightmare) rate *= 1.28f;
        if (IsInBannerZone(playerPos)) rate *= BannerFireRateMul;
        return rate;
    }

    static float EffFireCooldown(in Gun g) => g.FireCooldown / (CombatFireRateMult() * GunAffixFireRateMult());
    static float EffDamage(in Gun g) => (g.Damage + upgradeLevels[UpThorns] * 0.75f) * GunAffixDamageMult();
    static int EffCount(in Gun g) => g.Count + upgradeLevels[UpSplit];
    static int EffPierce(in Gun g) => g.Pierce + upgradeLevels[UpPierce];
    static float EffParalyzeRadius() => ParalyzeRadiusBase + upgradeLevels[UpParalyzeReach] * 22f;
    static float EffParalyzeDuration() => ParalyzeDurationBase + upgradeLevels[UpParalyzeHold] * 0.4f;
    static float EffParalyzeCooldown() => ParalyzeCooldown;

    static void OpenSettings(GameState returnState)
    {
        settingsReturnState = returnState;
        settingsTypeBuffer = "";
        rebindTarget = RebindTarget.None;
        settingsScroll = Math.Clamp(settingsScroll, 0f, SettingsScrollMax());
        uiInputBlockFrames = 2;
        state = GameState.Settings;
    }

    static Color BodyColor()
    {
        int idx = runLockedBodyIndex >= 0 ? runLockedBodyIndex : bodyColorIndex;
        return aiPilotEnabled ? AutoPilotBody : BodyPalette[idx];
    }
    static Color BodyBright()
    {
        int idx = runLockedBodyIndex >= 0 ? runLockedBodyIndex : bodyColorIndex;
        return aiPilotEnabled ? AutoPilotBright : Lighten(BodyPalette[idx], 0.5f);
    }

    static int UpgradeCost(int i) => UpgradeBaseCost[i] * (upgradeLevels[i] + 1);

}
