using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Statuses;
using XIVSlothCombo.Combos.PvE.Content;
using XIVSlothCombo.Core;
using XIVSlothCombo.CustomComboNS;

namespace XIVSlothCombo.Combos.PvE
{
    internal static class GNB
    {
        public const byte JobID = 37;

        public static int MaxCartridges(byte level) => level >= 88 ? 3 : 2;

        public const uint
            KeenEdge = 16137,
            NoMercy = 16138,
            BrutalShell = 16139,
            DemonSlice = 16141,
            SolidBarrel = 16145,
            GnashingFang = 16146,
            SavageClaw = 16147,
            DemonSlaughter = 16149,
            WickedTalon = 16150,
            SonicBreak = 16153,
            Continuation = 16155,
            JugularRip = 16156,
            AbdomenTear = 16157,
            EyeGouge = 16158,
            BowShock = 16159,
            HeartOfLight = 16160,
            BurstStrike = 16162,
            FatedCircle = 16163,
            Aurora = 16151,
            DoubleDown = 25760,
            DangerZone = 16144,
            BlastingZone = 16165,
            Bloodfest = 16164,
            Hypervelocity = 25759,
            RoughDivide = 16154,
            LightningShot = 16143,

            RoyalGuard = 16142;

        public static class Buffs
        {
            public const ushort
                NoMercy = 1831,
                Aurora = 1835,
                ReadyToRip = 1842,
                ReadyToTear = 1843,
                ReadyToGouge = 1844,

                RoyalGuardBuff = 1833,
                ReadyToBlast = 2686;
        }

        public static class Debuffs
        {
            public const ushort
                BowShock = 1838,
                SonicBreak = 1837;
        }

        public static class Config
        {
            public const string
                GNB_SkS = "GNB_SkS",
                GNB_RoughDivide_HeldCharges = "GNB_RoughDivide_HeldCharges",
                GNB_VariantCure = "GNB_VariantCure";
        }


        internal class GNB_ST_MainCombo : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.GNB_ST_MainCombo;

            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                if (actionID is KeenEdge)
                {
                    var gauge = GetJobGauge<GNBGauge>();
                    var roughDivideChargesRemaining = PluginConfiguration.GetCustomIntValue(Config.GNB_RoughDivide_HeldCharges);
                    var quarterWeave = GetCooldownRemainingTime(actionID) < 1 && GetCooldownRemainingTime(actionID) > 0.6;
                    int SkSChoice = PluginConfiguration.GetCustomIntValue(Config.GNB_SkS);
                    bool slowSkS = SkSChoice is 2 && IsEnabled(CustomComboPreset.GNB_ST_SkSSupport);
                    bool regularSkS = SkSChoice is 0 or 1 || IsNotEnabled(CustomComboPreset.GNB_ST_SkSSupport);

                    if (IsEnabled(CustomComboPreset.GNB_Variant_Cure) && IsEnabled(Variant.VariantCure) && PlayerHealthPercentageHp() <= GetOptionValue(Config.GNB_VariantCure))
                        return Variant.VariantCure;

                    if (IsEnabled(CustomComboPreset.GNB_ST_RangedUptime) && !InMeleeRange() && LevelChecked(LightningShot) && HasBattleTarget())
                        return LightningShot;

                    if (IsEnabled(CustomComboPreset.GNB_ST_MainCombo_CooldownsGroup) && IsEnabled(CustomComboPreset.GNB_ST_NoMercy) && ActionReady(NoMercy))
                    {
                        if (LevelChecked(BurstStrike))
                        {
                            if (regularSkS && quarterWeave)
                            {
                                if ((gauge.Ammo is 1 && CombatEngageDuration().TotalSeconds < 30 && IsOffCooldown(Bloodfest)) || //Opener Conditions
                                   (CombatEngageDuration().Minutes == 2 && GetCooldownRemainingTime(DoubleDown) < 4) || //2 min delay
                                   (CombatEngageDuration().Minutes != 2 && gauge.Ammo == MaxCartridges(level) && GetCooldownRemainingTime(GnashingFang) < 4)) //Regular NMGF
                                    return NoMercy;
                            }

                            if (slowSkS && CanWeave(actionID))
                            {
                                if ((CombatEngageDuration().TotalSeconds < 30 && lastComboMove is BrutalShell) ||
                                    gauge.Ammo == MaxCartridges(level) ||
                                    (CombatEngageDuration().Minutes % 2 == 1 && gauge.Ammo is 2 && WasLastWeaponskill(BurstStrike)))
                                    return NoMercy;
                            }
                        }

                        if (!LevelChecked(BurstStrike) && quarterWeave) //no cartridges unlocked
                            return NoMercy;
                    }

                    //oGCDs
                    if (CanWeave(actionID))
                    {
                        Status? sustainedDamage = FindTargetEffect(Variant.Debuffs.SustainedDamage);
                        if (IsEnabled(CustomComboPreset.GNB_Variant_SpiritDart) &&
                            IsEnabled(Variant.VariantSpiritDart) &&
                            (sustainedDamage is null || sustainedDamage?.RemainingTime <= 3))
                            return Variant.VariantSpiritDart;

                        if (IsEnabled(CustomComboPreset.GNB_Variant_Ultimatum) && IsEnabled(Variant.VariantUltimatum) && IsOffCooldown(Variant.VariantUltimatum))
                            return Variant.VariantUltimatum;

                        if (IsEnabled(CustomComboPreset.GNB_ST_MainCombo_CooldownsGroup))
                        {
                            if (IsEnabled(CustomComboPreset.GNB_ST_Bloodfest) && ActionReady(Bloodfest) && gauge.Ammo is 0 && HasEffect(Buffs.NoMercy))
                            {
                                if ((regularSkS && IsOnCooldown(GnashingFang)) || (slowSkS && IsOnCooldown(NoMercy)))
                                    return Bloodfest;
                            }


                            if (IsEnabled(CustomComboPreset.GNB_ST_BlastingZone) && ActionReady(DangerZone))
                            {
                                //Blasting Zone outside of NM
                                if (!HasEffect(Buffs.NoMercy) && ((IsOnCooldown(GnashingFang) && GetCooldownRemainingTime(NoMercy) > 17) || //Post Gnashing Fang
                                    !LevelChecked(GnashingFang))) //Pre Gnashing Fang
                                    return OriginalHook(DangerZone);

                                //Stops DZ Drift
                                if (HasEffect(Buffs.NoMercy) && ((IsOnCooldown(SonicBreak) && slowSkS) || (IsOnCooldown(DoubleDown) && regularSkS)))
                                    return OriginalHook(DangerZone);
                            }

                            //Continuation
                            if (IsEnabled(CustomComboPreset.GNB_ST_Gnashing) && LevelChecked(Continuation) &&
                                (HasEffect(Buffs.ReadyToRip) || HasEffect(Buffs.ReadyToTear) || HasEffect(Buffs.ReadyToGouge)))
                                return OriginalHook(Continuation);

                            //60s weaves
                            if (HasEffect(Buffs.NoMercy))
                            {
                                //Post DD
                                if ((regularSkS && IsOnCooldown(DoubleDown)) || (slowSkS && IsOnCooldown(SonicBreak)))
                                {
                                    if (IsEnabled(CustomComboPreset.GNB_ST_BlastingZone) && ActionReady(DangerZone))
                                        return OriginalHook(DangerZone);
                                    if (IsEnabled(CustomComboPreset.GNB_ST_BowShock) && ActionReady(BowShock))
                                        return BowShock;
                                }

                                //Pre DD
                                if (IsOnCooldown(SonicBreak) && !LevelChecked(DoubleDown))
                                {
                                    if (IsEnabled(CustomComboPreset.GNB_ST_BowShock) && ActionReady(BowShock))
                                        return BowShock;
                                    if (IsEnabled(CustomComboPreset.GNB_ST_BlastingZone) && ActionReady(DangerZone))
                                        return OriginalHook(DangerZone);
                                }
                            }
                        }
                        
                        // Odd numbered Rough Divide before Burst
                        if (CanWeave(actionID) && LevelChecked(RoughDivide) && GetRemainingCharges(RoughDivide) == 1 && !IsMoving && GetCooldownChargeRemainingTime(RoughDivide) < 10 && GetCooldownRemainingTime(NoMercy) > 20)
                            return RoughDivide;

                        //Rough Divide Feature
                        if (CanWeave(actionID) && LevelChecked(RoughDivide) && IsEnabled(CustomComboPreset.GNB_ST_RoughDivide) && GetRemainingCharges(RoughDivide) > roughDivideChargesRemaining && !IsMoving && !HasEffect(Buffs.ReadyToBlast))
                        {
                            if (IsNotEnabled(CustomComboPreset.GNB_ST_MeleeRoughDivide) ||
                                (IsEnabled(CustomComboPreset.GNB_ST_MeleeRoughDivide) && GetTargetDistance() <= 1 && HasEffect(Buffs.NoMercy) && IsOnCooldown(OriginalHook(DangerZone)) && IsOnCooldown(BowShock)))
                                return RoughDivide;
                        }
                    }

                    if (IsEnabled(CustomComboPreset.GNB_ST_Gnashing) && LevelChecked(Continuation) &&
                        (HasEffect(Buffs.ReadyToRip) || HasEffect(Buffs.ReadyToTear) || HasEffect(Buffs.ReadyToGouge)))
                        return OriginalHook(Continuation);

                    // 60s window features
                    if ((GetCooldownRemainingTime(NoMercy) > 57 || HasEffect(Buffs.NoMercy)) && IsEnabled(CustomComboPreset.GNB_ST_MainCombo_CooldownsGroup))
                    {
                        if (LevelChecked(DoubleDown) && GetCooldownRemainingTime(GnashingFang) > 20)
                        {
                            if (regularSkS)
                            {
                                if (IsEnabled(CustomComboPreset.GNB_ST_DoubleDown) && IsOffCooldown(DoubleDown) && gauge.Ammo >= 2 && !HasEffect(Buffs.ReadyToRip) && gauge.AmmoComboStep >= 1)
                                    return DoubleDown;
                                if (IsEnabled(CustomComboPreset.GNB_ST_SonicBreak) && IsOffCooldown(SonicBreak) && IsOnCooldown(DoubleDown))
                                    return SonicBreak;
                            }

                            if (slowSkS)
                            {
                                if (IsEnabled(CustomComboPreset.GNB_ST_SonicBreak) && IsOffCooldown(SonicBreak) && !HasEffect(Buffs.ReadyToRip) && gauge.AmmoComboStep >= 1)
                                    return SonicBreak;
                                if (IsEnabled(CustomComboPreset.GNB_ST_DoubleDown) && IsOffCooldown(DoubleDown) && gauge.Ammo >= 2 && IsOnCooldown(SonicBreak))
                                    return DoubleDown;
                            }
                        }

                        if (!LevelChecked(DoubleDown))
                        {
                            if (IsEnabled(CustomComboPreset.GNB_ST_SonicBreak) && ActionReady(SonicBreak) && !HasEffect(Buffs.ReadyToRip) && IsOnCooldown(GnashingFang))
                                return SonicBreak;
                            //sub level 54 functionality
                            if (IsEnabled(CustomComboPreset.GNB_ST_BlastingZone) && ActionReady(DangerZone) && !LevelChecked(SonicBreak))
                                return OriginalHook(DangerZone);
                        }
                    }

                    //Pre Gnashing Fang stuff
                    if (IsEnabled(CustomComboPreset.GNB_ST_Gnashing) && LevelChecked(GnashingFang))
                    {
                        bool activeNoMercy = GetCooldownRemainingTime(NoMercy) > 50 || HasEffect(Buffs.NoMercy);
                        if (IsEnabled(CustomComboPreset.GNB_ST_GnashingFang_Starter) && IsOffCooldown(GnashingFang) && gauge.AmmoComboStep == 0 &&
                            ((gauge.Ammo == MaxCartridges(level) && activeNoMercy && ((CombatEngageDuration().Minutes != 2 && regularSkS) || slowSkS)) || //Regular 60 second GF/NM timing
                            (gauge.Ammo == MaxCartridges(level) && activeNoMercy && CombatEngageDuration().Minutes == 2 && GetCooldownRemainingTime(DoubleDown) <= 1 && regularSkS) || //2 min delay for regular SkS
                            (gauge.Ammo == 1 && HasEffect(Buffs.NoMercy) && GetCooldownRemainingTime(DoubleDown) > 50) || //NMDDGF windows/Scuffed windows
                            (gauge.Ammo > 0 && GetCooldownRemainingTime(NoMercy) > 17 && GetCooldownRemainingTime(NoMercy) < 35) || //Regular 30 second window                                                                        
                            (gauge.Ammo == 1 && GetCooldownRemainingTime(NoMercy) > 50 && ((IsOffCooldown(Bloodfest) && LevelChecked(Bloodfest)) || !LevelChecked(Bloodfest))))) //Opener Conditions
                            return GnashingFang;
                        if (gauge.AmmoComboStep is 1 or 2)
                            return OriginalHook(GnashingFang);
                    }

                    if (IsEnabled(CustomComboPreset.GNB_ST_BurstStrike) && IsEnabled(CustomComboPreset.GNB_ST_MainCombo_CooldownsGroup))
                    {
                        if (HasEffect(Buffs.NoMercy) && gauge.AmmoComboStep == 0 && LevelChecked(BurstStrike))
                        {
                            if (LevelChecked(Hypervelocity) && HasEffect(Buffs.ReadyToBlast))
                                return Hypervelocity;
                            if (gauge.Ammo != 0 && GetCooldownRemainingTime(GnashingFang) > 4)
                                return BurstStrike;
                        }

                        //final check if Burst Strike is used right before No Mercy ends
                        if (LevelChecked(Hypervelocity) && HasEffect(Buffs.ReadyToBlast))
                            return Hypervelocity;
                    }

                    // Regular 1-2-3 combo with overcap feature
                    if (comboTime > 0)
                    {
                        if (lastComboMove == KeenEdge && LevelChecked(BrutalShell))
                            return BrutalShell;
                        if (lastComboMove == BrutalShell && LevelChecked(SolidBarrel))
                        {
                            if (LevelChecked(Hypervelocity) && HasEffect(Buffs.ReadyToBlast))
                                return Hypervelocity;
                            if (LevelChecked(BurstStrike) && gauge.Ammo == MaxCartridges(level))
                                return BurstStrike;
                            return SolidBarrel;
                        }
                    }

                    if (IsOnCooldown(DoubleDown) && roughDivideChargesRemaining == 0 && IsOnCooldown(SonicBreak) && !HasEffect(Buffs.RoyalGuardBuff))
                    return RoyalGuard;

                    return KeenEdge;
                }

                return actionID;
            }
        }

                internal class GNB_ST_MainComboNoBuffs : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.GNB_ST_MainCombo;

            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                if (actionID is SolidBarrel)
                {
                    var gauge = GetJobGauge<GNBGauge>();
                    var roughDivideChargesRemaining = PluginConfiguration.GetCustomIntValue(Config.GNB_RoughDivide_HeldCharges);
                    var quarterWeave = GetCooldownRemainingTime(actionID) < 1 && GetCooldownRemainingTime(actionID) > 0.6;
                    int SkSChoice = PluginConfiguration.GetCustomIntValue(Config.GNB_SkS);
                    bool slowSkS = SkSChoice is 2 && IsEnabled(CustomComboPreset.GNB_ST_SkSSupport);
                    bool regularSkS = SkSChoice is 0 or 1 || IsNotEnabled(CustomComboPreset.GNB_ST_SkSSupport);

                    if (IsEnabled(CustomComboPreset.GNB_Variant_Cure) && IsEnabled(Variant.VariantCure) && PlayerHealthPercentageHp() <= GetOptionValue(Config.GNB_VariantCure))
                        return Variant.VariantCure;

                    if (IsEnabled(CustomComboPreset.GNB_ST_RangedUptime) && !InMeleeRange() && LevelChecked(LightningShot) && HasBattleTarget())
                        return LightningShot;

                    // if (IsEnabled(CustomComboPreset.GNB_ST_MainCombo_CooldownsGroup) && IsEnabled(CustomComboPreset.GNB_ST_NoMercy) && ActionReady(NoMercy))
                    // {
                    //     if (LevelChecked(BurstStrike))
                    //     {
                    //         if (regularSkS && quarterWeave)
                    //         {
                    //             if ((gauge.Ammo is 1 && CombatEngageDuration().TotalSeconds < 30 && IsOffCooldown(Bloodfest)) || //Opener Conditions
                    //                (CombatEngageDuration().Minutes == 2 && GetCooldownRemainingTime(DoubleDown) < 4) || //2 min delay
                    //                (CombatEngageDuration().Minutes != 2 && gauge.Ammo == MaxCartridges(level) && GetCooldownRemainingTime(GnashingFang) < 4)) //Regular NMGF
                    //                 return NoMercy;
                    //         }

                    //         if (slowSkS && CanWeave(actionID))
                    //         {
                    //             if ((CombatEngageDuration().TotalSeconds < 30 && lastComboMove is BrutalShell) ||
                    //                 gauge.Ammo == MaxCartridges(level) ||
                    //                 (CombatEngageDuration().Minutes % 2 == 1 && gauge.Ammo is 2 && WasLastWeaponskill(BurstStrike)))
                    //                 return NoMercy;
                    //         }
                    //     }

                    //     if (!LevelChecked(BurstStrike) && quarterWeave) //no cartridges unlocked
                    //         return NoMercy;
                    // }

                    //oGCDs
                    if (CanWeave(actionID))
                    {
                        Status? sustainedDamage = FindTargetEffect(Variant.Debuffs.SustainedDamage);
                        if (IsEnabled(CustomComboPreset.GNB_Variant_SpiritDart) &&
                            IsEnabled(Variant.VariantSpiritDart) &&
                            (sustainedDamage is null || sustainedDamage?.RemainingTime <= 3))
                            return Variant.VariantSpiritDart;

                        if (IsEnabled(CustomComboPreset.GNB_Variant_Ultimatum) && IsEnabled(Variant.VariantUltimatum) && IsOffCooldown(Variant.VariantUltimatum))
                            return Variant.VariantUltimatum;

                        if (IsEnabled(CustomComboPreset.GNB_ST_MainCombo_CooldownsGroup))
                        {
                            if (IsEnabled(CustomComboPreset.GNB_ST_Bloodfest) && ActionReady(Bloodfest) && gauge.Ammo is 0 && HasEffect(Buffs.NoMercy))
                            {
                                if ((regularSkS && IsOnCooldown(GnashingFang)) || (slowSkS && IsOnCooldown(NoMercy)))
                                    return Bloodfest;
                            }


                            if (IsEnabled(CustomComboPreset.GNB_ST_BlastingZone) && ActionReady(DangerZone))
                            {
                                //Blasting Zone outside of NM
                                if (!HasEffect(Buffs.NoMercy) && ((IsOnCooldown(GnashingFang) && GetCooldownRemainingTime(NoMercy) > 17) || //Post Gnashing Fang
                                    !LevelChecked(GnashingFang))) //Pre Gnashing Fang
                                    return OriginalHook(DangerZone);

                                //Stops DZ Drift
                                if (HasEffect(Buffs.NoMercy) && ((IsOnCooldown(SonicBreak) && slowSkS) || (IsOnCooldown(DoubleDown) && regularSkS)))
                                    return OriginalHook(DangerZone);
                            }

                            //Continuation
                            if (IsEnabled(CustomComboPreset.GNB_ST_Gnashing) && LevelChecked(Continuation) &&
                                (HasEffect(Buffs.ReadyToRip) || HasEffect(Buffs.ReadyToTear) || HasEffect(Buffs.ReadyToGouge)))
                                return OriginalHook(Continuation);

                            //60s weaves
                            if (HasEffect(Buffs.NoMercy))
                            {
                                //Post DD
                                if ((regularSkS && IsOnCooldown(DoubleDown)) || (slowSkS && IsOnCooldown(SonicBreak)))
                                {
                                    if (IsEnabled(CustomComboPreset.GNB_ST_BlastingZone) && ActionReady(DangerZone))
                                        return OriginalHook(DangerZone);
                                    if (IsEnabled(CustomComboPreset.GNB_ST_BowShock) && ActionReady(BowShock))
                                        return BowShock;
                                }

                                //Pre DD
                                if (IsOnCooldown(SonicBreak) && !LevelChecked(DoubleDown))
                                {
                                    if (IsEnabled(CustomComboPreset.GNB_ST_BowShock) && ActionReady(BowShock))
                                        return BowShock;
                                    if (IsEnabled(CustomComboPreset.GNB_ST_BlastingZone) && ActionReady(DangerZone))
                                        return OriginalHook(DangerZone);
                                }
                            }
                        }

                        //Rough Divide Feature
                        // if (CanWeave(actionID) && LevelChecked(RoughDivide) && IsEnabled(CustomComboPreset.GNB_ST_RoughDivide) && GetRemainingCharges(RoughDivide) > roughDivideChargesRemaining && !IsMoving && !HasEffect(Buffs.ReadyToBlast))
                        // {
                        //     if (IsNotEnabled(CustomComboPreset.GNB_ST_MeleeRoughDivide) ||
                        //         (IsEnabled(CustomComboPreset.GNB_ST_MeleeRoughDivide) && GetTargetDistance() <= 1 && HasEffect(Buffs.NoMercy) && IsOnCooldown(OriginalHook(DangerZone)) && IsOnCooldown(BowShock)))
                        //         return RoughDivide;
                        // }
                    }

                    if (IsEnabled(CustomComboPreset.GNB_ST_Gnashing) && LevelChecked(Continuation) &&
                        (HasEffect(Buffs.ReadyToRip) || HasEffect(Buffs.ReadyToTear) || HasEffect(Buffs.ReadyToGouge)))
                        return OriginalHook(Continuation);

                    // 60s window features
                    if ((GetCooldownRemainingTime(NoMercy) > 57 || HasEffect(Buffs.NoMercy)) && IsEnabled(CustomComboPreset.GNB_ST_MainCombo_CooldownsGroup))
                    {
                        if (LevelChecked(DoubleDown) && GetCooldownRemainingTime(GnashingFang) > 20)
                        {
                            if (regularSkS)
                            {
                                if (IsEnabled(CustomComboPreset.GNB_ST_DoubleDown) && IsOffCooldown(DoubleDown) && gauge.Ammo >= 2 && !HasEffect(Buffs.ReadyToRip) && gauge.AmmoComboStep >= 1)
                                    return DoubleDown;
                                if (IsEnabled(CustomComboPreset.GNB_ST_SonicBreak) && IsOffCooldown(SonicBreak) && IsOnCooldown(DoubleDown))
                                    return SonicBreak;
                            }

                            if (slowSkS)
                            {
                                if (IsEnabled(CustomComboPreset.GNB_ST_SonicBreak) && IsOffCooldown(SonicBreak) && !HasEffect(Buffs.ReadyToRip) && gauge.AmmoComboStep >= 1)
                                    return SonicBreak;
                                if (IsEnabled(CustomComboPreset.GNB_ST_DoubleDown) && IsOffCooldown(DoubleDown) && gauge.Ammo >= 2 && IsOnCooldown(SonicBreak))
                                    return DoubleDown;
                            }
                        }

                        if (!LevelChecked(DoubleDown))
                        {
                            if (IsEnabled(CustomComboPreset.GNB_ST_SonicBreak) && ActionReady(SonicBreak) && !HasEffect(Buffs.ReadyToRip) && IsOnCooldown(GnashingFang))
                                return SonicBreak;
                            //sub level 54 functionality
                            if (IsEnabled(CustomComboPreset.GNB_ST_BlastingZone) && ActionReady(DangerZone) && !LevelChecked(SonicBreak))
                                return OriginalHook(DangerZone);
                        }
                    }

                    //Pre Gnashing Fang stuff
                    if (IsEnabled(CustomComboPreset.GNB_ST_Gnashing) && LevelChecked(GnashingFang))
                    {
                        bool activeNoMercy = GetCooldownRemainingTime(NoMercy) > 50 || HasEffect(Buffs.NoMercy);
                        if (IsEnabled(CustomComboPreset.GNB_ST_GnashingFang_Starter) && IsOffCooldown(GnashingFang) && gauge.AmmoComboStep == 0 &&
                            ((gauge.Ammo == MaxCartridges(level) && activeNoMercy && ((CombatEngageDuration().Minutes != 2 && regularSkS) || slowSkS)) || //Regular 60 second GF/NM timing
                            (gauge.Ammo == MaxCartridges(level) && activeNoMercy && CombatEngageDuration().Minutes == 2 && GetCooldownRemainingTime(DoubleDown) <= 1 && regularSkS) || //2 min delay for regular SkS
                            (gauge.Ammo == 1 && HasEffect(Buffs.NoMercy) && GetCooldownRemainingTime(DoubleDown) > 50) || //NMDDGF windows/Scuffed windows
                            (gauge.Ammo > 0 && GetCooldownRemainingTime(NoMercy) > 17 && GetCooldownRemainingTime(NoMercy) < 35) || //Regular 30 second window                                                                        
                            (gauge.Ammo == 1 && GetCooldownRemainingTime(NoMercy) > 50 && ((IsOffCooldown(Bloodfest) && LevelChecked(Bloodfest)) || !LevelChecked(Bloodfest))))) //Opener Conditions
                            return GnashingFang;
                        if (gauge.AmmoComboStep is 1 or 2)
                            return OriginalHook(GnashingFang);
                    }

                    if (IsEnabled(CustomComboPreset.GNB_ST_BurstStrike) && IsEnabled(CustomComboPreset.GNB_ST_MainCombo_CooldownsGroup))
                    {
                        if (HasEffect(Buffs.NoMercy) && gauge.AmmoComboStep == 0 && LevelChecked(BurstStrike))
                        {
                            if (LevelChecked(Hypervelocity) && HasEffect(Buffs.ReadyToBlast))
                                return Hypervelocity;
                            if (gauge.Ammo != 0 && GetCooldownRemainingTime(GnashingFang) > 4)
                                return BurstStrike;
                        }

                        //final check if Burst Strike is used right before No Mercy ends
                        if (LevelChecked(Hypervelocity) && HasEffect(Buffs.ReadyToBlast))
                            return Hypervelocity;
                    }

                    // Regular 1-2-3 combo with overcap feature
                    if (comboTime > 0)
                    {
                        if (lastComboMove == KeenEdge && LevelChecked(BrutalShell))
                            return BrutalShell;
                        if (lastComboMove == BrutalShell && LevelChecked(SolidBarrel))
                        {
                            if (LevelChecked(Hypervelocity) && HasEffect(Buffs.ReadyToBlast))
                                return Hypervelocity;
                            if (LevelChecked(BurstStrike) && gauge.Ammo == MaxCartridges(level))
                                return BurstStrike;
                            return SolidBarrel;
                        }
                    }

                    if (IsOnCooldown(DoubleDown) && roughDivideChargesRemaining == 0 && IsOnCooldown(SonicBreak) && !HasEffect(Buffs.RoyalGuardBuff))
                    return RoyalGuard;

                    return KeenEdge;
                }

                return actionID;
            }
        }

        internal class GNB_GF_Continuation : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.GNB_GF_Continuation;

            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                if (actionID == GnashingFang)
                {
                    var gauge = GetJobGauge<GNBGauge>();
                    int SkSChoice = PluginConfiguration.GetCustomIntValue(Config.GNB_SkS);
                    bool slowSkS = SkSChoice is 2 && IsEnabled(CustomComboPreset.GNB_ST_SkSSupport);
                    bool regularSkS = SkSChoice is 0 or 1 || IsNotEnabled(CustomComboPreset.GNB_ST_SkSSupport);

                    if (IsOffCooldown(NoMercy) && CanDelayedWeave(SolidBarrel) && IsOffCooldown(GnashingFang) && IsEnabled(CustomComboPreset.GNB_GF_NoMercy))
                        return NoMercy;

                    if (CanWeave(actionID))
                    {
                        if (IsEnabled(CustomComboPreset.GNB_GF_Cooldowns))
                        {
                            if (ActionReady(Bloodfest) && gauge.Ammo is 0 && HasEffect(Buffs.NoMercy) && ((regularSkS && IsOnCooldown(GnashingFang)) || (slowSkS && IsOnCooldown(NoMercy))))
                                return Bloodfest;

                            if (ActionReady(DangerZone))
                            {
                                //Blasting Zone outside of NM
                                if (!HasEffect(Buffs.NoMercy) && ((IsOnCooldown(GnashingFang) && GetCooldownRemainingTime(NoMercy) > 17) || //Post Gnashing Fang
                                    !LevelChecked(GnashingFang))) //Pre Gnashing Fang
                                    return OriginalHook(DangerZone);

                                //Stops DZ Drift
                                if (HasEffect(Buffs.NoMercy) && ((IsOnCooldown(SonicBreak) && slowSkS) || (IsOnCooldown(DoubleDown) && regularSkS)))
                                    return OriginalHook(DangerZone);
                            }

                            //Continuation
                            if (LevelChecked(Continuation) && (HasEffect(Buffs.ReadyToRip) || HasEffect(Buffs.ReadyToTear) || HasEffect(Buffs.ReadyToGouge)))
                                return OriginalHook(Continuation);

                            //60s weaves
                            if (HasEffect(Buffs.NoMercy))
                            {
                                //Post DD
                                if ((regularSkS && IsOnCooldown(DoubleDown)) || (slowSkS && IsOnCooldown(SonicBreak)))
                                {
                                    if (ActionReady(DangerZone))
                                        return OriginalHook(DangerZone);
                                    if (ActionReady(BowShock))
                                        return BowShock;
                                }

                                //Pre DD
                                if (IsOnCooldown(SonicBreak) && !LevelChecked(DoubleDown))
                                {
                                    if (ActionReady(BowShock))
                                        return BowShock;
                                    if (ActionReady(DangerZone))
                                        return OriginalHook(DangerZone);
                                }
                            }
                        }

                        if (LevelChecked(Continuation) && (HasEffect(Buffs.ReadyToRip) || HasEffect(Buffs.ReadyToTear) || HasEffect(Buffs.ReadyToGouge)))
                            return OriginalHook(Continuation);
                    }

                    // 60s window features
                    if (GetCooldownRemainingTime(NoMercy) > 57 || HasEffect(Buffs.NoMercy))
                    {
                        if (LevelChecked(DoubleDown) && GetCooldownRemainingTime(GnashingFang) > 20)
                        {
                            if (regularSkS)
                            {
                                if (IsEnabled(CustomComboPreset.GNB_GF_DoubleDown) && IsOffCooldown(DoubleDown) && gauge.Ammo >= 2 && !HasEffect(Buffs.ReadyToRip) && gauge.AmmoComboStep >= 1)
                                    return DoubleDown;
                                if (IsEnabled(CustomComboPreset.GNB_GF_Cooldowns) && IsOffCooldown(SonicBreak) && IsOnCooldown(DoubleDown))
                                    return SonicBreak;
                            }

                            if (slowSkS)
                            {
                                if (IsEnabled(CustomComboPreset.GNB_GF_Cooldowns) && IsOffCooldown(SonicBreak) && !HasEffect(Buffs.ReadyToRip) && gauge.AmmoComboStep >= 1)
                                    return SonicBreak;
                                if (IsEnabled(CustomComboPreset.GNB_GF_DoubleDown) && IsOffCooldown(DoubleDown) && gauge.Ammo >= 2 && IsOnCooldown(SonicBreak))
                                    return DoubleDown;
                            }
                        }

                        if (!LevelChecked(DoubleDown) && IsEnabled(CustomComboPreset.GNB_GF_Cooldowns))
                        {
                            if (ActionReady(SonicBreak) && !HasEffect(Buffs.ReadyToRip) && IsOnCooldown(GnashingFang))
                                return SonicBreak;

                            //sub level 54 functionality
                            if (ActionReady(DangerZone) && !LevelChecked(SonicBreak))
                                return OriginalHook(DangerZone);
                        }
                    }

                    if ((gauge.AmmoComboStep == 0 && IsOffCooldown(GnashingFang)) || gauge.AmmoComboStep is 1 or 2)
                        return OriginalHook(GnashingFang);

                    if (IsEnabled(CustomComboPreset.GNB_GF_Cooldowns))
                    {
                        if (HasEffect(Buffs.NoMercy) && gauge.AmmoComboStep == 0 && LevelChecked(BurstStrike))
                        {
                            if (LevelChecked(Hypervelocity) && HasEffect(Buffs.ReadyToBlast))
                                return Hypervelocity;
                            if (gauge.Ammo != 0 && GetCooldownRemainingTime(GnashingFang) > 4)
                                return BurstStrike;
                        }

                        //final check if Burst Strike is used right before No Mercy ends
                        if (LevelChecked(Hypervelocity) && HasEffect(Buffs.ReadyToBlast))
                            return Hypervelocity;
                    }
                }

                return actionID;
            }
        }


        internal class GNB_BS : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.GNB_BS;

            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                if (actionID is BurstStrike)
                {
                    var gauge = GetJobGauge<GNBGauge>();

                    if (IsEnabled(CustomComboPreset.GNB_BS_Continuation) && HasEffect(Buffs.ReadyToBlast) && LevelChecked(Hypervelocity))
                        return Hypervelocity;
                    if (IsEnabled(CustomComboPreset.GNB_BS_Bloodfest) && gauge.Ammo is 0 && LevelChecked(Bloodfest) && !HasEffect(Buffs.ReadyToBlast))
                        return Bloodfest;
                    if (IsEnabled(CustomComboPreset.GNB_BS_DoubleDown) && HasEffect(Buffs.NoMercy) && GetCooldownRemainingTime(DoubleDown) < 2 && gauge.Ammo >= 2 && LevelChecked(DoubleDown))
                        return DoubleDown;
                }

                return actionID;
            }
        }

        internal class GNB_AoE_MainCombo : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.GNB_AoE_MainCombo;

            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {

                if (actionID == DemonSlice)
                {
                    var gauge = GetJobGauge<GNBGauge>();

                    if (IsEnabled(CustomComboPreset.GNB_Variant_Cure) && IsEnabled(Variant.VariantCure) && PlayerHealthPercentageHp() <= GetOptionValue(Config.GNB_VariantCure))
                        return Variant.VariantCure;

                    if (InCombat())
                    {
                        if (CanWeave(actionID))
                        {
                            Status? sustainedDamage = FindTargetEffect(Variant.Debuffs.SustainedDamage);
                            if (IsEnabled(CustomComboPreset.GNB_Variant_SpiritDart) &&
                                IsEnabled(Variant.VariantSpiritDart) &&
                                (sustainedDamage is null || sustainedDamage?.RemainingTime <= 3))
                                return Variant.VariantSpiritDart;

                            if (IsEnabled(CustomComboPreset.GNB_Variant_Ultimatum) && IsEnabled(Variant.VariantUltimatum) && IsOffCooldown(Variant.VariantUltimatum))
                                return Variant.VariantUltimatum;

                            if (IsEnabled(CustomComboPreset.GNB_AoE_NoMercy) && ActionReady(NoMercy))
                                return NoMercy;
                            if (IsEnabled(CustomComboPreset.GNB_AoE_BowShock) && ActionReady(BowShock))
                                return BowShock;
                            if (IsEnabled(CustomComboPreset.GNB_AOE_DangerZone) && ActionReady(DangerZone))
                                return OriginalHook(DangerZone);
                            if (IsEnabled(CustomComboPreset.GNB_AoE_Bloodfest) && gauge.Ammo == 0 && ActionReady(Bloodfest))
                                return Bloodfest;
                        }

                        if (IsEnabled(CustomComboPreset.GNB_AOE_SonicBreak) && ActionReady(SonicBreak))
                            return SonicBreak;
                        if (IsEnabled(CustomComboPreset.GNB_AoE_DoubleDown) && gauge.Ammo >= 2 && ActionReady(DoubleDown))
                            return DoubleDown;
                        if (IsEnabled(CustomComboPreset.GNB_AoE_Bloodfest) && gauge.Ammo != 0 && GetCooldownRemainingTime(Bloodfest) < 6 && LevelChecked(FatedCircle))
                            return FatedCircle;

                    }

                    if (comboTime > 0 && lastComboMove == DemonSlice && LevelChecked(DemonSlaughter))
                    {
                        return (IsEnabled(CustomComboPreset.GNB_AOE_Overcap) && LevelChecked(FatedCircle) && gauge.Ammo == MaxCartridges(level)) ? FatedCircle : DemonSlaughter;
                    }

                    return DemonSlice;
                }

                return actionID;
            }
        }

        internal class GNB_NoMercy_Cooldowns : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.GNB_NoMercy_Cooldowns;

            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                if (actionID == NoMercy)
                {
                    var gauge = GetJobGauge<GNBGauge>().Ammo;
                    if (IsOnCooldown(NoMercy) && InCombat())
                    {
                        if (IsEnabled(CustomComboPreset.GNB_NoMercy_Cooldowns_DD) && GetCooldownRemainingTime(NoMercy) < 60 && IsOffCooldown(DoubleDown) && gauge >= 2 && LevelChecked(DoubleDown))
                            return DoubleDown;
                        if (IsEnabled(CustomComboPreset.GNB_NoMercy_Cooldowns_SonicBreakBowShock))
                        {
                            if (IsOffCooldown(SonicBreak))
                                return SonicBreak;
                            if (IsOffCooldown(BowShock))
                                return BowShock;
                        }
                    }
                }

                return actionID;
            }
        }

        internal class GNB_AuroraProtection : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.GNB_AuroraProtection;

            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                return (actionID is Aurora && HasEffect(Buffs.Aurora)) ? WAR.NascentFlash : actionID;
            }
        }
    }
}