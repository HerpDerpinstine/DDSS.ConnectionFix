using HarmonyLib;
using Il2Cpp;

namespace DDSS_ConnectionFix.Patches
{
    [HarmonyPatch]
    internal class Patch_JoinLANTab
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(JoinLANTab), nameof(JoinLANTab.Join))]
        private static bool Join_Prefix(JoinLANTab __instance)
        {
            // Validate Code
            string addr = __instance.ipInput.text;
            if (string.IsNullOrEmpty(addr)
                || string.IsNullOrWhiteSpace(addr))
                return false;

            // Join Session
            ConnectionHandler.JoinLobbyByIP(addr, false, __instance);

            // Prevent Original
            return false;
        }
    }
}