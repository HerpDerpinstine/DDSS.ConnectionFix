using HarmonyLib;
using Il2Cpp;

namespace DDSS_ConnectionFix.Patches
{
    [HarmonyPatch]
    internal class Patch_CustomNetworkManager
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CustomNetworkManager), nameof(CustomNetworkManager.OnClientDisconnect))]
        private static bool OnClientDisconnect_Prefix()
        {
            // Prevent Original
            return false;
        }
    }
}
