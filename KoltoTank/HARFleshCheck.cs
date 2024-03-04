using AlienRace;

using Verse;

namespace KoltoTank
{
    public static class HARFleshCheck
    {
        public static bool IsItFlesh(this Pawn pawn)
        {
            return (pawn.def as ThingDef_AlienRace)?.alienRace.compatibility?.IsFleshPawn(pawn) ?? pawn.RaceProps.IsFlesh;
        }
    }
}
    

