﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace KoltoTank
{
    public class koltoTankHealthAIUtility
    { 
        public static bool ShouldSeekkoltoTankRest(Pawn patientPawn, List<HediffDef> alwaysTreatableHediffs, List<HediffDef> neverTreatableHediffs, List<HediffDef> nonCriticalTreatableHediffs)
        {
            List<Hediff> patientHediffs = new();
            patientPawn.health.hediffSet.GetHediffs(ref patientHediffs);

            return
                    // Is downed and not meant to be always downed (e.g. babies)
                    (patientPawn.Downed && !LifeStageUtility.AlwaysDowned(patientPawn))
                    // Has (visible) hediffs requiring tending (excluding those blacklisted or greylisted from MedPod treatment)
                    || patientHediffs.Any(x => x.Visible && x.TendableNow() && !neverTreatableHediffs.Contains(x.def) && !nonCriticalTreatableHediffs.Contains(x.def))
                    // Has tended and healing injuries
                    || patientPawn.health.hediffSet.HasTendedAndHealingInjury()
                    // Has immunizable but not yet immune hediffs
                    || patientPawn.health.hediffSet.HasImmunizableNotImmuneHediff()
                    // Has (visible) hediffs causing sick thoughts (excluding those blacklisted or greylisted from MedPod treatment)
                    || patientHediffs.Any(x => x.def.makesSickThought && x.Visible && !neverTreatableHediffs.Contains(x.def) && !nonCriticalTreatableHediffs.Contains(x.def))
                    // Has missing body parts
                    || (!patientPawn.health.hediffSet.GetMissingPartsCommonAncestors().NullOrEmpty() && !neverTreatableHediffs.Contains(HediffDefOf.MissingBodyPart))
                    // Has permanent injuries (excluding those blacklisted from MedPod treatment)
                    || patientHediffs.Any(x => x.IsPermanent() && !neverTreatableHediffs.Contains(x.def))
                    // Has chronic diseases (excluding those blacklisted or greylisted from MedPod treatment)
                    || patientHediffs.Any(x => x.def.chronic && !neverTreatableHediffs.Contains(x.def) && !nonCriticalTreatableHediffs.Contains(x.def))
                    // Has addictions (excluding those blacklisted or greylisted from MedPod treatment)
                    || patientHediffs.Any(x => x.def.IsAddiction && !neverTreatableHediffs.Contains(x.def) && !nonCriticalTreatableHediffs.Contains(x.def))
                    // Has (visible) hediffs that are always treatable by MedPods
                    || patientHediffs.Any(x => x.Visible && alwaysTreatableHediffs.Contains(x.def));
        }

        public static bool IsValidRaceForMedPod(Pawn patientPawn, List<string> disallowedRaces)
        {
            string patientRace = patientPawn.def.ToString();
            if (!disallowedRaces.NullOrEmpty())
            {
                return !disallowedRaces.Contains(patientRace);
            }
            return true;
        }

        public static bool IsValidXenotypeForMedPod(Pawn patientPawn, List<XenotypeDef> disallowedXenotypes)
        {
            XenotypeDef patientXenotype = patientPawn.genes?.xenotype;

            // For pawns without genes and/or xenotypes (usually animals)
            if (patientXenotype == null)
            {
                return true;
            }

            if (!disallowedXenotypes.NullOrEmpty())
            {
                return !disallowedXenotypes.Contains(patientXenotype);
            }
            return true;
        }

        public static bool HasAllowedMedicalCareCategory(Pawn patientPawn)
        {
            return WorkGiver_DoBill.GetMedicalCareCategory(patientPawn) >= MedicalCareCategory.NormalOrWorse;
        }

        public static bool HasUsageBlockingHediffs(Pawn patientPawn, List<HediffDef> usageBlockingHediffs)
        {
            List<Hediff> patientHediffs = new();
            patientPawn.health.hediffSet.GetHediffs(ref patientHediffs);

            return patientHediffs.Any(x => usageBlockingHediffs.Contains(x.def));
        }

        public static bool HasUsageBlockingTraits(Pawn patientPawn, List<TraitDef> usageBlockingTraits)
        {
            return patientPawn.story?.traits.allTraits.Any(x => usageBlockingTraits.Contains(x.def)) ?? false;
        }
    }
}
    }
}