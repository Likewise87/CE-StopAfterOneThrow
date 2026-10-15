using HarmonyLib;
using Verse;
using Verse.AI;
using System.Reflection;

namespace CE_StopAfterOneThrow.Patches
{
    /// <summary>
    /// End the current job after a single successful *thrown* projectile (CE's Verb_LaunchProjectileCE).
    /// Firearms are unaffected (they use Verb_ShootCE).
    /// Equipped launchers are ignored (we only stop inventory throws).
    /// </summary>
    [HarmonyPatch(typeof(CombatExtended.Verb_LaunchProjectileCE),
                  nameof(CombatExtended.Verb_LaunchProjectileCE.TryCastShot))]
    public static class Patch_Verb_LaunchProjectileCE_TryCastShot
    {
        public static void Postfix(CombatExtended.Verb_LaunchProjectileCE __instance, bool __result)
        {
            if (!__result) return; // No projectile launched

            var pawn = __instance.CasterPawn;
            if (pawn == null) return;

            var jobTracker = pawn.jobs;
            var curJob = GetCurrentJob(pawn);

            // Treat as "equipped" only if verb source equals pawn's equipped primary
            var primary = pawn.equipment?.Primary;
            bool fromEquippedPrimary = primary != null && __instance.EquipmentSource == primary;
            if (fromEquippedPrimary) return;

            // Clear post-attack busy stance so queued jobs (e.g., Move) can start
            pawn.stances?.CancelBusyStanceSoft();

            // End current job, prefer overload that starts the next queued job immediately
            try
            {
                jobTracker?.EndCurrentJob(JobCondition.Succeeded, true);
            }
            catch
            {
                jobTracker?.EndCurrentJob(JobCondition.Succeeded);
            }
        }

        /// <summary>
        /// Works across RW versions: tries Pawn.CurJob property, then falls back to pawn.jobs.curJob field.
        /// </summary>
        private static Job GetCurrentJob(Pawn pawn)
        {
            var pType = typeof(Pawn);
            var prop = pType.GetProperty("CurJob", BindingFlags.Instance | BindingFlags.Public);
            if (prop != null)
            {
                try { return prop.GetValue(pawn) as Job; } catch { }
            }

            var jt = pawn.jobs;
            if (jt != null)
            {
                var f = jt.GetType().GetField("curJob", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (f != null)
                {
                    try { return f.GetValue(jt) as Job; } catch { }
                }
            }
            return null;
        }
    }
}
