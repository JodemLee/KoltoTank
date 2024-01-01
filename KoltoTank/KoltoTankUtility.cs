using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;


namespace KoltoTank
{
    internal class KoltoTankUtility
    {
         
        public static string NoPathTrans;

        public static void ResetStaticData()
        {
            NoPathTrans = "NoPath".Translate();
        }

        public static bool IsValidkoltoTankFor(Building_KoltoTank koltoTank, Pawn patientPawn, Pawn travelerPawn, GuestStatus? guestStatus = null)
        {
            if (koltoTank == null)
            {
                return false;
            }
            if (!koltoTank.powerComp.PowerOn)
            {
                return false;
            }
            if (koltoTank.IsForbidden(travelerPawn))
            {
                return false;
            }
            if (!travelerPawn.CanReserve(koltoTank))
            {
                Pawn otherPawn = travelerPawn.Map.reservationManager.FirstRespectedReserver(koltoTank, patientPawn);
                if (otherPawn != null)
                {
                    JobFailReason.Is("ReservedBy".Translate(otherPawn.LabelShort, otherPawn));
                }
                return false;
            }
            if (!travelerPawn.CanReach(koltoTank, PathEndMode.OnCell, Danger.Deadly))
            {
                JobFailReason.Is(NoPathTrans);
                return false;
            }
            if (travelerPawn.Map.designationManager.DesignationOn(koltoTank, DesignationDefOf.Deconstruct) != null)
            {
                return false;
            }
            if (!RestUtility.CanUseBedEver(patientPawn, koltoTank.def))
            {
                return false;
            }
            if (guestStatus == GuestStatus.Prisoner)
            {
                if (!koltoTank.Position.IsInPrisonCell(koltoTank.Map))
                {
                    return false;
                }
            }

            if (!koltoTankHealthAIUtility.ShouldSeekkoltoTankRest(patientPawn, koltoTank.AlwaysTreatableHediffs, koltoTank.NeverTreatableHediffs, koltoTank.NonCriticalTreatableHediffs))
            {
                return false;
            }
            if (!koltoTankHealthAIUtility.HasAllowedMedicalCareCategory(patientPawn))
            {
                return false;
            }

            if (koltoTank.IsBurning())
            {
                return false;
            }
            if (koltoTank.IsBrokenDown())
            {
                return false;
            }
            return true;
        }

        public static Building_KoltoTank FindBestkoltoTank(Pawn pawn, Pawn patient)
        {
            List<ThingDef> koltoTankDefsBestToWorst = RestUtility.bedDefsBestToWorst_Medical.Where(x => x.thingClass == typeof(Building_KoltoTank)).ToList();

            float initialSearchDistance = 10f;

            // Prioritize searching for usable koltoTanks by distance, followed by koltoTank type and path danger level
            while (initialSearchDistance <= 9999f)
            {

                for (int i = 0; i < koltoTankDefsBestToWorst.Count; i++)
                {
                    ThingDef thingDef = koltoTankDefsBestToWorst[i];

                    if (!RestUtility.CanUseBedEver(patient, thingDef))
                    {
                        continue;
                    }

                    for (int j = 0; j < 2; j++)
                    {
                        Danger maxDanger2 = (j == 0) ? Danger.None : Danger.Deadly;

                        bool validator(Thing t)
                        {
                            Building_KoltoTank koltoTank = t as Building_KoltoTank;

                            bool patientDangerCheck = (int)koltoTank.Position.GetDangerFor(patient, patient.Map) <= (int)maxDanger2;

                            bool isValidBedFor = IsValidkoltoTankFor(koltoTank, patient, pawn, patient.GuestStatus);

                            bool result =   patientDangerCheck ;

                            return result;
                        }

                        Building_KoltoTank koltoTank = (Building_KoltoTank)GenClosest.ClosestThingReachable(patient.Position, patient.Map, ThingRequest.ForDef(thingDef), PathEndMode.OnCell, TraverseParms.For(pawn), initialSearchDistance, validator);

                        if (koltoTank != null)
                        {
                            return koltoTank;
                        }
                    }
                }

                // Double our search range for each iteration
                initialSearchDistance *= 2;
            }

            return null;
        }
    }
}
