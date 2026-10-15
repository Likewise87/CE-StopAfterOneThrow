using HarmonyLib;
using Verse;

namespace CE_StopAfterOneThrow
{
    public class ModInit : Mod
    {
        public ModInit(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("TSA.CE.StopAfterOneThrow");
            harmony.PatchAll();
            Log.Message("[CE.StopAfterOneThrow] Loaded and patches applied.");
        }
    }
}
