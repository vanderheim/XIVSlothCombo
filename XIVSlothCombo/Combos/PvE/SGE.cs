using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using System.Collections.Generic;
using XIVSlothCombo.Combos.PvE.Content;
using XIVSlothCombo.CustomComboNS;
using XIVSlothCombo.CustomComboNS.Functions;

namespace XIVSlothCombo.Combos.PvE
{
    internal static class SGE
    {
        internal const byte JobID = 40;

        // Actions
        internal const uint
            // Heals and Shields
            Diagnosis = 24284,
            Prognosis = 24286,
            Physis = 24288,
            Druochole = 24296,
            Kerachole = 24298,
            Ixochole = 24299,
            Pepsis = 24301,
            Physis2 = 24302,
            Taurochole = 24303,
            Haima = 24305,
            Panhaima = 24311,
            Holos = 24310,
            EukrasianDiagnosis = 24291,
            EukrasianPrognosis = 24292,
            Egeiro = 24287,

            // DPS
            Dosis = 24283,
            Dosis2 = 24306,
            Dosis3 = 24312,
            EukrasianDosis = 24293,
            EukrasianDosis2 = 24308,
            EukrasianDosis3 = 24314,
            Phlegma = 24289,
            Phlegma2 = 24307,
            Phlegma3 = 24313,
            Dyskrasia = 24297,
            Dyskrasia2 = 24315,
            Toxikon = 24304,
            Pneuma = 24318,

            // Buffs
            Soteria = 24294,
            Zoe = 24300,
            Krasis = 24317,

            // Other
            Kardia = 24285,
            Eukrasia = 24290,
            Rhizomata = 24309;

        // Action Groups
        internal static readonly List<uint>
            AddersgallList = new() { Taurochole, Druochole, Ixochole, Kerachole },
            PhlegmaList = new() { Phlegma, Phlegma2, Phlegma3 };

        // Action Buffs
        internal static class Buffs
        {
            internal const ushort
                Kardia = 2604,
                Kardion = 2605,
                Eukrasia = 2606,
                EukrasianDiagnosis = 2607,
                EukrasianPrognosis = 2609;
        }

        internal static class Debuffs
        {
            internal const ushort
                EukrasianDosis = 2614,
                EukrasianDosis2 = 2615,
                EukrasianDosis3 = 2616;
        }

        // Debuff Pairs of Actions and Debuff
        internal static readonly Dictionary<uint, ushort>
            DosisList = new()
            {
                { Dosis,  Debuffs.EukrasianDosis  },
                { Dosis2, Debuffs.EukrasianDosis2 },
                { Dosis3, Debuffs.EukrasianDosis3 }
            };

        // Sage Gauge & Extensions
        private static SGEGauge Gauge => CustomComboFunctions.GetJobGauge<SGEGauge>();
        private static bool HasAddersgall(this SGEGauge gauge) => gauge.Addersgall > 0;
        private static bool HasAddersting(this SGEGauge gauge) => gauge.Addersting > 0;

        internal static class Config
        {
            #region DPS
            internal static UserBool
                SGE_ST_DPS_Adv = new("SGE_ST_DPS_Adv"),
                SGE_ST_DPS_Adv_D2 = new("SGE_ST_Dosis_AltMode"),
                SGE_ST_DPS_Adv_GroupInstants = new("SGE_ST_DPS_Adv_GroupInstants"),
                SGE_ST_DPS_EDosis_Adv = new("SGE_ST_Dosis_EDosis_Adv");
            internal static UserBoolArray
                SGE_ST_DPS_Adv_GroupInstants_Addl = new("SGE_ST_DPS_Adv_GroupInstants_Addl"),
                SGE_ST_DPS_Movement = new("SGE_ST_DPS_Movement");
            internal static UserInt
                SGE_ST_DPS_EDosisHPPer = new("SGE_ST_Dosis_EDosisHPPer"),
                SGE_ST_DPS_Lucid = new("SGE_ST_DPS_Lucid"),
                SGE_ST_DPS_Rhizo = new("SGE_ST_DPS_Rhizo"),
                SGE_AoE_DPS_Lucid = new("SGE_AoE_Phlegma_Lucid"),
                SGE_AoE_DPS_Rhizo = new("SGE_AoE_DPS_Rhizo");
            internal static UserFloat
                SGE_ST_DPS_EDosisThreshold = new("SGE_ST_Dosis_EDosisThreshold");
            #endregion

            #region Healing
            internal static UserBool
                SGE_ST_Heal_Adv = new("SGE_ST_Heal_Adv"),
                SGE_ST_Heal_UIMouseOver = new("SGE_ST_Heal_UIMouseOver"),
                SGE_AoE_Heal_KeracholeTrait = new("SGE_AoE_Heal_KeracholeTrait");
            internal static UserInt
                SGE_ST_Heal_Zoe = new("SGE_ST_Heal_Zoe"),
                SGE_ST_Heal_Haima = new("SGE_ST_Heal_Haima"),
                SGE_ST_Heal_Krasis = new("SGE_ST_Heal_Krasis"),
                SGE_ST_Heal_Pepsis = new("SGE_ST_Heal_Pepsis"),
                SGE_ST_Heal_Soteria = new("SGE_ST_Heal_Soteria"),
                SGE_ST_Heal_Diagnosis = new("SGE_ST_Heal_Diagnosis"),
                SGE_ST_Heal_Druochole = new("SGE_ST_Heal_Druochole"),
                SGE_ST_Heal_Taurochole = new("SGE_ST_Heal_Taurochole");                
            #endregion

            internal static UserInt
                SGE_Eukrasia_Mode = new("SGE_Eukrasia_Mode");
        }

        internal static class Traits
        {
            internal const ushort 
                EnhancedKerachole = 375;
        }


        /*
         * SGE_Kardia
         * Soteria becomes Kardia when Kardia's Buff is not active or Soteria is on cooldown.
         */
        internal class SGE_Kardia : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.SGE_Kardia;
            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
                => actionID is Soteria && (!HasEffect(Buffs.Kardia) || IsOnCooldown(Soteria)) ? Kardia : actionID;
        }

        /*
         * SGE_Rhizo
         * Replaces all Addersgal using Abilities (Taurochole/Druochole/Ixochole/Kerachole) with Rhizomata if out of Addersgall stacks
         * (Scholar speak: Replaces all Aetherflow abilities with Aetherflow when out)
         */
        internal class SGE_Rhizo : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.SGE_Rhizo;
            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
                => AddersgallList.Contains(actionID) && ActionReady(Rhizomata) && !Gauge.HasAddersgall() && IsOffCooldown(actionID) ? Rhizomata : actionID;
        }

        /*
         * Druo/Tauro
         * Druochole Upgrade to Taurochole (like a trait upgrade)
         * Replaces Druocole with Taurochole when Taurochole is available
         * (As of 6.0) Taurochole (single target massive insta heal w/ cooldown), Druochole (Single target insta heal)
         */
        internal class SGE_DruoTauro : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.SGE_DruoTauro;
            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
                => actionID is Druochole && ActionReady(Taurochole) ? Taurochole : actionID;
        }

        /*
         * SGE_ZoePneuma (Zoe to Pneuma Combo)
         * Places Zoe on top of Pneuma when both are available.
         */
        internal class SGE_ZoePneuma : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.SGE_ZoePneuma;
            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
                => actionID is Pneuma && ActionReady(Pneuma) && IsOffCooldown(Zoe) ? Zoe : actionID;
        }

        /*
         * SGE_AoE_Phlegma (Phlegma AoE Feature)
         * Replaces Zero Charges/Stacks of Phlegma with various options
         * Lucid Dreaming, Toxikon, or Dyskrasia
         */
        internal class SGE_AoE_DPS : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.SGE_AoE_DPS;
            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                if (PhlegmaList.Contains(actionID))
                {
                    bool NoPhlegmaToxikon = IsEnabled(CustomComboPreset.SGE_AoE_DPS_NoPhlegmaToxikon);
                    bool OutOfRangeToxikon = IsEnabled(CustomComboPreset.SGE_AoE_DPS_OutOfRangeToxikon);
                    bool NoPhlegmaDyskrasia = IsEnabled(CustomComboPreset.SGE_AoE_DPS_NoPhlegmaDyskrasia);
                    bool NoTargetDyskrasia = IsEnabled(CustomComboPreset.SGE_AoE_DPS_NoTargetDyskrasia);
                    uint phlegma = OriginalHook(Phlegma); //Level appropriate Phlegma

                    if (IsEnabled(CustomComboPreset.SGE_DPS_Variant_Rampart) &&
                        IsEnabled(Variant.VariantRampart) &&
                        IsOffCooldown(Variant.VariantRampart) &&
                        CanSpellWeave(actionID))
                        return Variant.VariantRampart;

                    Status? sustainedDamage = FindTargetEffect(Variant.Debuffs.SustainedDamage);
                    if (IsEnabled(CustomComboPreset.SGE_DPS_Variant_SpiritDart) &&
                        IsEnabled(Variant.VariantSpiritDart) &&
                        (sustainedDamage is null || sustainedDamage?.RemainingTime <= 3) &&
                        CanSpellWeave(actionID))
                        return Variant.VariantSpiritDart;

                    // Lucid Dreaming
                    if (IsEnabled(CustomComboPreset.SGE_AoE_DPS_Lucid) &&
                        ActionReady(All.LucidDreaming) && CanSpellWeave(Dosis) &&
                        LocalPlayer.CurrentMp <= Config.SGE_AoE_DPS_Lucid)
                        return All.LucidDreaming;

                    // Rhizomata
                    if (IsEnabled(CustomComboPreset.SGE_AoE_DPS_Rhizo) && CanSpellWeave(Dosis) &&
                        ActionReady(Rhizomata) && Gauge.Addersgall <= Config.SGE_AoE_DPS_Rhizo)
                        return Rhizomata;

                    // Toxikon
                    if (LevelChecked(Toxikon) && HasBattleTarget() && Gauge.HasAddersting())
                    {
                        if ((NoPhlegmaToxikon && !HasCharges(phlegma)) ||
                            (OutOfRangeToxikon && !InActionRange(phlegma)))
                            return OriginalHook(Toxikon);
                    }

                    // Dyskrasia
                    if (LevelChecked(Dyskrasia))
                    {
                        if ((NoPhlegmaDyskrasia && !HasCharges(phlegma)) ||
                            (NoTargetDyskrasia && CurrentTarget is null))
                            return OriginalHook(Dyskrasia);
                    }
                }

                return actionID;
            }
        }

        /*
         * SGE_ST_DPS (Single Target DPS Combo)
         * Currently Replaces Dosis with Eukrasia when the debuff on the target is < 3 seconds or not existing
         * Kardia reminder, Lucid Dreaming, & Toxikon optional
         */
        internal class SGE_ST_DPS : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.SGE_ST_DPS;
            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                bool ActionFound;
                bool GroupInstants = false;

                if (Config.SGE_ST_DPS_Adv)
                {
                    GroupInstants = actionID is Toxikon && Config.SGE_ST_DPS_Adv_GroupInstants;
                    ActionFound = (!Config.SGE_ST_DPS_Adv_D2 && DosisList.ContainsKey(actionID)) || //not restricted to Dosis 2
                                  actionID is Dosis2 ||                                             //Dosis 2 is always allowed
                                  GroupInstants;                                                    //Group Instants on Toxikon
                } else ActionFound = DosisList.ContainsKey(actionID); //default handling

                if (ActionFound)
                {
                    // Kardia Reminder
                    if (IsEnabled(CustomComboPreset.SGE_ST_DPS_Kardia) && LevelChecked(Kardia) &&
                        FindEffect(Buffs.Kardia) is null)
                        return Kardia;

                    // Lucid Dreaming
                    if (IsEnabled(CustomComboPreset.SGE_ST_DPS_Lucid) &&
                        ActionReady(All.LucidDreaming) && CanSpellWeave(actionID) &&
                        LocalPlayer.CurrentMp <= Config.SGE_ST_DPS_Lucid)
                        return All.LucidDreaming;

                    if (IsEnabled(CustomComboPreset.SGE_DPS_Variant_Rampart) &&
                        IsEnabled(Variant.VariantRampart) &&
                        IsOffCooldown(Variant.VariantRampart) &&
                        CanSpellWeave(actionID))
                        return Variant.VariantRampart;

                    // Rhizomata
                    if (IsEnabled(CustomComboPreset.SGE_ST_DPS_Rhizo) && CanSpellWeave(actionID) &&
                        ActionReady(Rhizomata) && Gauge.Addersgall <= Config.SGE_ST_DPS_Rhizo)
                        return Rhizomata;

                    if (HasBattleTarget() && (!HasEffect(Buffs.Eukrasia)))
                    // Buff check Above. Without it, Toxikon and any future option will interfere in the Eukrasia->Eukrasia Dosis combo
                    {
                        // Eukrasian Dosis.
                        // If we're too low level to use Eukrasia, we can stop here.
                        if (IsEnabled(CustomComboPreset.SGE_ST_DPS_EDosis) && LevelChecked(Eukrasia) && InCombat())
                        {
                            // Grab current Dosis via OriginalHook, grab it's fellow debuff ID from Dictionary, then check for the debuff
                            // Using TryGetValue due to edge case where the actionID would be read as Eukrasian Dosis instead of Dosis
                            // EDosis will show for half a second if the buff is removed manually or some other act of God
                            if (DosisList.TryGetValue(OriginalHook(actionID), out ushort dotDebuffID))
                            {
                                Status? sustainedDamage = FindTargetEffect(Variant.Debuffs.SustainedDamage);
                                if (IsEnabled(CustomComboPreset.SGE_DPS_Variant_SpiritDart) &&
                                    IsEnabled(Variant.VariantSpiritDart) &&
                                    (sustainedDamage is null || sustainedDamage?.RemainingTime <= 3) &&
                                    CanSpellWeave(actionID))
                                    return Variant.VariantSpiritDart;

                                Status? dotDebuff = FindTargetEffect(dotDebuffID);
                                float refreshtimer = Config.SGE_ST_DPS_EDosis_Adv ? Config.SGE_ST_DPS_EDosisThreshold : 3;

                                if ((dotDebuff is null || dotDebuff.RemainingTime <= refreshtimer) &&
                                    GetTargetHPPercent() > Config.SGE_ST_DPS_EDosisHPPer)
                                    return Eukrasia;
                            }
                        }

                        // Phlegma
                        if (IsEnabled(CustomComboPreset.SGE_ST_DPS_Phlegma) && InCombat())
                        {
                            uint phlegma = OriginalHook(Phlegma);
                            if (InActionRange(phlegma) && ActionReady(phlegma)) return phlegma;
                        }


                        // Movement Options
                        if (IsEnabled(CustomComboPreset.SGE_ST_DPS_Movement) && InCombat() && IsMoving)
                        {
                            if (Config.SGE_ST_DPS_Movement.Count == 3)
                            {
                                // Toxikon
                                if (Config.SGE_ST_DPS_Movement[0] && LevelChecked(Toxikon) && Gauge.HasAddersting()) return OriginalHook(Toxikon);
                                // Dyskrasia
                                if (Config.SGE_ST_DPS_Movement[1] && LevelChecked(Dyskrasia) && InActionRange(Dyskrasia)) return OriginalHook(Dyskrasia);
                                // Eukrasia
                                if (Config.SGE_ST_DPS_Movement[2] && LevelChecked(Eukrasia)) return Eukrasia;
                            }
                        }
                    }

                    //Group Instant GCDs
                    if (GroupInstants)
                    {
                        if (HasEffect(Buffs.Eukrasia)) return OriginalHook(Dosis);
                        
                        if (Config.SGE_ST_DPS_Adv_GroupInstants_Addl.Count == 2)
                        {
                            // Toxikon
                            if (Config.SGE_ST_DPS_Adv_GroupInstants_Addl[0] && LevelChecked(Toxikon) && Gauge.HasAddersting()) return OriginalHook(Toxikon);
                            // Dyskrasia
                            if (Config.SGE_ST_DPS_Adv_GroupInstants_Addl[1] && LevelChecked(Dyskrasia) && InActionRange(Dyskrasia)) return OriginalHook(Dyskrasia);
                        }
                        return Eukrasia;
                    }
                }
                return actionID;
            }
        }

        /*
         * SGE_Raise (Swiftcast Raise)
         * Swiftcast becomes Egeiro when on cooldown
         */
        internal class SGE_Raise : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.SGE_Raise;
            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
                    => actionID is All.Swiftcast && IsOnCooldown(All.Swiftcast) ? Egeiro : actionID;
        }

        /* 
         * SGE_Eukrasia (Eukrasia combo)
         * Normally after Eukrasia is used and updates the abilities, it becomes disabled
         * This will "combo" the action to user selected action
         */
        internal class SGE_Eukrasia : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.SGE_Eukrasia;
            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                if (actionID is Eukrasia && HasEffect(Buffs.Eukrasia))
                {
                    switch ((int)Config.SGE_Eukrasia_Mode)
                    {
                        case 0: return OriginalHook(Dosis);
                        case 1: return OriginalHook(Diagnosis);
                        case 2: return OriginalHook(Prognosis);
                        default: break;
                    }
                }

                return actionID;
            }
        }

        /* 
         * SGE_ST_Heal (Diagnosis Single Target Heal)
         * Replaces Diagnosis with various Single Target healing options, 
         * Pseudo priority set by various custom user percentages
         */
        internal class SGE_ST_Heal : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.SGE_ST_Heal;
            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                if (actionID is Diagnosis)
                {
                    if (HasEffect(Buffs.Eukrasia))
                        return EukrasianDiagnosis;

                    GameObject? healTarget = GetHealTarget(Config.SGE_ST_Heal_Adv && Config.SGE_ST_Heal_UIMouseOver);

                    if (IsEnabled(CustomComboPreset.SGE_ST_Heal_Esuna) && ActionReady(All.Esuna) &&
                        HasCleansableDebuff(healTarget))
                        return All.Esuna;

                    if (IsEnabled(CustomComboPreset.SGE_ST_Heal_Druochole) && ActionReady(Druochole) &&
                        Gauge.HasAddersgall() &&
                        GetTargetHPPercent(healTarget) <= Config.SGE_ST_Heal_Druochole)
                        return Druochole;

                    if (IsEnabled(CustomComboPreset.SGE_ST_Heal_Taurochole) && ActionReady(Taurochole) &&
                        Gauge.HasAddersgall() &&
                        GetTargetHPPercent(healTarget) <= Config.SGE_ST_Heal_Taurochole)
                        return Taurochole;

                    if (IsEnabled(CustomComboPreset.SGE_ST_Heal_Rhizomata) && ActionReady(Rhizomata) &&
                        !Gauge.HasAddersgall())
                        return Rhizomata;

                    if (IsEnabled(CustomComboPreset.SGE_ST_Heal_Kardia) && LevelChecked(Kardia) &&
                        FindEffect(Buffs.Kardia) is null &&
                        FindEffect(Buffs.Kardion, healTarget, LocalPlayer?.ObjectId) is null)
                        return Kardia;

                    if (IsEnabled(CustomComboPreset.SGE_ST_Heal_Soteria) && ActionReady(Soteria) &&
                        GetTargetHPPercent(healTarget) <= Config.SGE_ST_Heal_Soteria)
                        return Soteria;

                    if (IsEnabled(CustomComboPreset.SGE_ST_Heal_Zoe) && ActionReady(Zoe) &&
                        GetTargetHPPercent(healTarget) <= Config.SGE_ST_Heal_Zoe)
                        return Zoe;

                    if (IsEnabled(CustomComboPreset.SGE_ST_Heal_Krasis) && ActionReady(Krasis) &&
                        GetTargetHPPercent(healTarget) <= Config.SGE_ST_Heal_Krasis)
                        return Krasis;

                    if (IsEnabled(CustomComboPreset.SGE_ST_Heal_Pepsis) && ActionReady(Pepsis) &&
                        GetTargetHPPercent(healTarget) <= Config.SGE_ST_Heal_Pepsis &&
                        FindEffect(Buffs.EukrasianDiagnosis, healTarget, LocalPlayer?.ObjectId) is not null)
                        return Pepsis;

                    if (IsEnabled(CustomComboPreset.SGE_ST_Heal_Haima) && ActionReady(Haima) &&
                        GetTargetHPPercent(healTarget) <= Config.SGE_ST_Heal_Haima)
                        return Haima;

                    if (IsEnabled(CustomComboPreset.SGE_ST_Heal_Diagnosis) && LevelChecked(Eukrasia) &&
                        GetTargetHPPercent(healTarget) <= Config.SGE_ST_Heal_Diagnosis &&
                        (IsEnabled(CustomComboPreset.SGE_ST_Heal_Diagnosis_IgnoreShield) ||
                         FindEffect(Buffs.EukrasianDiagnosis, healTarget, LocalPlayer?.ObjectId) is null))
                        return Eukrasia;
                }

                return actionID;
            }
        }

        /* 
         * SGE_AoE_Heal (Prognosis AoE Heal)
         * Replaces Prognosis with various AoE healing options, 
         * Pseudo priority set by various custom user percentages
         */
        internal class SGE_AoE_Heal : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.SGE_AoE_Heal;
            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                if (actionID is Prognosis)
                {
                    if (HasEffect(Buffs.Eukrasia))
                        return EukrasianPrognosis;

                    if (IsEnabled(CustomComboPreset.SGE_AoE_Heal_Rhizomata) && ActionReady(Rhizomata) &&
                        !Gauge.HasAddersgall())
                        return Rhizomata;

                    if (IsEnabled(CustomComboPreset.SGE_AoE_Heal_Kerachole) &&
                        ActionReady(Kerachole) &&
                        (!Config.SGE_AoE_Heal_KeracholeTrait || (Config.SGE_AoE_Heal_KeracholeTrait && TraitLevelChecked(Traits.EnhancedKerachole))) &&
                        Gauge.HasAddersgall())
                        return Kerachole;

                    if (IsEnabled(CustomComboPreset.SGE_AoE_Heal_Ixochole) && ActionReady(Ixochole) &&
                        Gauge.HasAddersgall())
                        return Ixochole;

                    if (IsEnabled(CustomComboPreset.SGE_AoE_Heal_Physis))
                    {
                        uint physis = OriginalHook(Physis);
                        if (ActionReady(physis)) return physis;
                    }

                    if (IsEnabled(CustomComboPreset.SGE_AoE_Heal_Holos) && ActionReady(Holos))
                        return Holos;

                    if (IsEnabled(CustomComboPreset.SGE_AoE_Heal_Panhaima) && ActionReady(Panhaima))
                        return Panhaima;

                    if (IsEnabled(CustomComboPreset.SGE_AoE_Heal_Pepsis) && ActionReady(Pepsis) &&
                        FindEffect(Buffs.EukrasianPrognosis) is not null)
                        return Pepsis;

                    if (IsEnabled(CustomComboPreset.SGE_AoE_Heal_EPrognosis) && LevelChecked(Eukrasia) &&
                        (IsEnabled(CustomComboPreset.SGE_AoE_Heal_EPrognosis_IgnoreShield) ||
                         FindEffect(Buffs.EukrasianPrognosis) is null))
                        return Eukrasia;
                }

                return actionID;
            }
        }
    }
}