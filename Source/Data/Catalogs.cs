partial class Program
{
    // ---------------------------------------------------------------- Catalog builders

    static Gun[] BuildGunCatalog()
    {
        var list = new List<Gun>(64);
        list.Add(new Gun
        {
            Name = "Squire's Sling", Desc = "A worn sling. Fans stones in a wide arc.",
            Color = new Color(118, 108, 98, 255), FireCooldown = 0.32f, Count = 1, Spread = 0f,
            Speed = 470f, Damage = 2f, Pierce = 1, Homing = 0f,
            Style = GunFireStyle.ArcFan, BurstCount = 5,
        });

        (string name, string desc, int wave, int cost, Color col, float cd, int cnt, float spd, float dmg, int pierce, float homing)[] siege =
        {
            ("Crossbow", "Tight bolt for the outer wall.", 3, 120, new Color(108, 98, 118, 255), 0.44f, 1, 580f, 3f, 1, 0f),
            ("Twin Daggers", "Two knives, no mercy.", 5, 180, new Color(138, 88, 98, 255), 0.30f, 2, 500f, 2f, 1, 0f),
            ("Buckshot Pot", "Clay pot shrapnel at close range.", 8, 260, new Color(92, 88, 108, 255), 0.58f, 5, 420f, 1f, 1, 0f),
            ("Seeking Charm", "A curse that hunts the nearest foe.", 12, 340, new Color(128, 92, 148, 255), 0.38f, 1, 440f, 2f, 1, 3.4f),
            ("Breach Lance", "Punches through a whole rank.", 16, 420, new Color(118, 78, 88, 255), 0.54f, 1, 620f, 4f, 4, 0f),
            ("Repeater", "Iron teeth that never stop chewing.", 20, 520, new Color(98, 108, 128, 255), 0.17f, 1, 540f, 1f, 1, 0f),
            ("Lord's Volley", "Three heavy shots for the throne room.", 25, 680, new Color(148, 82, 92, 255), 0.48f, 3, 510f, 3f, 2, 0.8f),
        };
        GunFireStyle[] siegeStyles =
        {
            GunFireStyle.Sniper, GunFireStyle.Burst, GunFireStyle.Buckshot, GunFireStyle.Homing,
            GunFireStyle.Lance, GunFireStyle.Repeater, GunFireStyle.Volley,
        };
        for (int si = 0; si < siege.Length; si++)
        {
            var s = siege[si];
            list.Add(new Gun
            {
                Name = s.name, Desc = s.desc, WaveReq = s.wave, FableCost = s.cost, Color = s.col,
                FireCooldown = s.cd, Count = s.cnt, Spread = s.cnt > 1 ? 14f : 0f, Speed = s.spd,
                Damage = s.dmg, Pierce = s.pierce, Homing = s.homing,
                Style = siegeStyles[si],
                BurstCount = si == 1 ? 2 : si == 6 ? 3 : s.cnt,
                BurstGap = si == 1 ? 0.05f : si == 6 ? 0.14f : 0.08f,
            });
        }

        string[] rankNames =
        {
            "Rusty Pike", "Guard's Bow", "Militia Arquebus", "Halberd Shot", "Chain Flail",
            "Tower Ballista", "Sergeant's Sidearm", "Siege Needle", "Caltrop Burst", "Knight's Carbine",
            "Bastion Rod", "Gargoyle Spit", "Moat Harpoon", "Rampart Rifle", "Dungeon Handgun",
            "Bell Hammer", "Catapult Shard", "Drawbridge Bolt", "Keep Cannon", "Courtyard Scatter",
            "Barbican Lance", "Portcullis Gun", "Armory Repeater", "Vault Piercer", "Throne Seeker",
            "Banner Volley", "Crypt Spitter", "Battlement Bow", "War Room Repeater", "High Hall Rifle",
            "Iron Saint", "Black Tower", "Stone Warden", "Crown Breaker", "Lord's Wrath",
            "Siege Heart", "Castle Storm", "Rampart King", "Doom Bell", "Last Bastion",
            "Oathkeeper", "Grave Lance", "Night Watch", "Blood Rampart", "King's Verdict",
            "Void Banisher", "Ash Crown", "Iron Requiem", "Fallen Throne", "Eternal Keep",
        };
        string[] rankDesc =
        {
            "Long reach, short patience.", "Drawn from the wall walk.", "Smoke and spite.",
            "Cleaves armor and tile alike.", "Spins ruin in a tight arc.",
            "Bolts big enough to shame a door.", "Issued after your first scar.", "Thin, cruel, fast.",
            "Sprays pain across the stones.", "Noble steel, peasant speed.",
            "Channelled through old masonry.", "Statues wish they could spit this hard.", "Hooks whatever flees.",
            "Fires from the high stone.", "Smuggled out of the oubliette.",
            "Rings the keep and the enemy.", "Splinters from a war machine.", "Crosses the gap in one breath.",
            "Slow. Loud. Final.", "Fills the yard with shot.",
            "Guards the gate with prejudice.", "Grinds through anything raised.",
            "Never stops until the drum does.", "Opens chests and ribcages.", "Finds its mark through smoke.",
            "Three shots under one crest.", "Old bones, new holes.", "Loosed from the parapet.",
            "Faster than the alarm bell.", "Reserved for hall defenders.",
            "Blessed iron, cursed rate of fire.", "Fires from the dark tower.", "Stone remembers this one.",
            "Crowns crack before it.", "A lord's temper made metal.",
            "Built for long sieges.", "Weather in the barrel.", "Owns the high ground.",
            "Rings once. Kills twice.", "When the wall is all that's left.",
            "Sworn steel.", "Risen from the crypts.", "Patrols the outer dark.",
            "Painted in breach dust.", "No appeal after this shot.",
            "Pushes back the abyss.", "Burnt regalia.", "Dirge in iron.",
            "Throne room relic.", "The keep's last word.",
        };

        GunFireStyle[] rankStyles =
        {
            GunFireStyle.Lance, GunFireStyle.ArcFan, GunFireStyle.Repeater, GunFireStyle.CrossBurst,
            GunFireStyle.FlailArc, GunFireStyle.Sniper, GunFireStyle.Standard, GunFireStyle.DriftOrb,
            GunFireStyle.Burst, GunFireStyle.Standard, GunFireStyle.Mortar, GunFireStyle.Lance,
            GunFireStyle.RingPulse, GunFireStyle.Sniper, GunFireStyle.Repeater, GunFireStyle.Laser,
            GunFireStyle.Standard, GunFireStyle.Mortar, GunFireStyle.Sniper, GunFireStyle.Buckshot,
            GunFireStyle.Lance, GunFireStyle.Repeater, GunFireStyle.Burst, GunFireStyle.Lance,
            GunFireStyle.Homing, GunFireStyle.Volley, GunFireStyle.DriftOrb, GunFireStyle.ArcFan,
            GunFireStyle.Repeater, GunFireStyle.Sniper, GunFireStyle.Laser, GunFireStyle.RingPulse,
            GunFireStyle.Standard, GunFireStyle.CrossBurst, GunFireStyle.Lance, GunFireStyle.Mortar,
            GunFireStyle.FlailArc, GunFireStyle.Burst, GunFireStyle.Homing, GunFireStyle.Laser,
            GunFireStyle.Standard, GunFireStyle.Lance, GunFireStyle.Sniper, GunFireStyle.RingPulse,
            GunFireStyle.Volley, GunFireStyle.Laser, GunFireStyle.DriftOrb, GunFireStyle.CrossBurst,
            GunFireStyle.Mortar, GunFireStyle.Burst, GunFireStyle.FlailArc, GunFireStyle.Homing,
        };

        for (int i = 0; i < rankNames.Length; i++)
        {
            int level = i + 2;
            float tier = level / 52f;
            GunFireStyle style = rankStyles[i];
            int count = 1 + level / 11;
            float spread = count > 1 ? 10f + tier * 18f : 0f;
            float homing = level % 7 == 0 ? 1.2f + tier * 4f : level % 5 == 0 ? 0.6f + tier * 2f : 0f;
            int pierce = 1 + level / 14;
            float cd = Math.Max(0.11f, 0.42f - tier * 0.28f);
            if (style == GunFireStyle.Laser) cd = Math.Max(1.2f, cd * 3.5f);
            if (style == GunFireStyle.Sniper) cd = Math.Max(0.55f, cd * 1.65f);
            if (style == GunFireStyle.Mortar) cd = Math.Max(0.62f, cd * 1.4f);
            list.Add(new Gun
            {
                Name = rankNames[i], Desc = rankDesc[i], LevelReq = level,
                Color = RankGunColor(level),
                FireCooldown = cd,
                Count = count, Spread = spread, Speed = 430f + tier * 320f,
                Damage = 1.6f + level * 0.11f, Pierce = pierce, Homing = homing,
                Style = style,
                BurstCount = style is GunFireStyle.Burst ? 4 : style is GunFireStyle.Volley ? 3 : count,
                BurstGap = style is GunFireStyle.Burst ? 0.06f : style is GunFireStyle.Volley ? 0.12f : 0.08f,
            });
        }

        return list.ToArray();
    }

    static Color RankGunColor(int level)
    {
        Color[] palette =
        {
            new(98, 94, 112, 255), new(118, 82, 96, 255), new(88, 98, 122, 255),
            new(108, 72, 108, 255), new(132, 78, 86, 255), new(92, 88, 118, 255),
            new(128, 68, 92, 255), new(82, 98, 118, 255), new(142, 88, 108, 255),
            new(102, 82, 128, 255),
        };
        float tier = Math.Clamp(level / 52f, 0f, 1f);
        int a = (level - 2) % palette.Length;
        int b = (a + 1) % palette.Length;
        return LerpColor(palette[a], palette[b], tier);
    }

    static string[] BuildAccessoryNames()
    {
        return new[]
        {
            "None", "Iron Circlet", "Hood", "Pauldrons", "Cloak Pin", "Torch Bearer",
            "Chain Coif", "Belt Pouch", "Signet Ring", "Warden Cape", "Skull Brooch",
            "Battle Scarf", "Spiked Collar", "Lantern Hook", "Prayer Beads", "Royal Mantle",
            "Black Hood", "Tower Crest", "Ash Veil", "Blood Sash", "Iron Halo",
            "Keep Banner", "Dungeon Keys", "Oath Band", "Siege Gloves", "Grave Shawl",
            "Rampart Pin", "Lord's Mask", "Bell Cord", "Stone Rosary", "War Paint",
            "Crown Fragments", "Bastion Wings", "Crypt Lantern", "Throne Chain", "Last Oath",
            "Cursor Crown", "Storm Glass",
        };
    }

    static int[] BuildAccessoryFableCosts()
    {
        int[] c = new int[38];
        c[0] = 0; c[4] = 70; c[5] = 0; c[7] = 110; c[8] = 150; c[10] = 180;
        c[12] = 0; c[14] = 220; c[16] = 0; c[19] = 280; c[22] = 320; c[25] = 0;
        c[28] = 360; c[31] = 0; c[34] = 420; c[37] = 480;
        return c;
    }

    static int[] BuildAccessoryLevelReqs()
    {
        int[] l = new int[38];
        l[1] = 2; l[2] = 4; l[3] = 0; l[5] = 6; l[6] = 8; l[9] = 10; l[11] = 12;
        l[13] = 14; l[15] = 16; l[17] = 18; l[20] = 20; l[23] = 22; l[26] = 24;
        l[29] = 26; l[32] = 28; l[35] = 30; l[37] = 32;
        return l;
    }

    static int[] BuildAccessoryWaveReqs()
    {
        int[] w = new int[38];
        w[3] = 4; w[7] = 0; w[10] = 10; w[18] = 14; w[21] = 18; w[24] = 22; w[27] = 26; w[30] = 30;
        w[37] = 20;
        return w;
    }

}
