using DDSS_ConnectionFix.Handlers;
using HarmonyLib;
using Il2Cpp;

namespace DDSS_ConnectionFix.Patches
{
    [HarmonyPatch]
    internal class Patch_MenuTab
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MenuTab), nameof(MenuTab.JoinLobbyByCode))]
        private static bool JoinLobbyByCode_Prefix(MenuTab __instance)
        {
            // Validate Code
            string code = __instance.codeInput.text.ToUpper();
            if (string.IsNullOrEmpty(code)
                || string.IsNullOrWhiteSpace(code))
                return false;

            // Join Session
            ConnectionHandler.JoinLobbyByCode(code, false, __instance);

            // Prevent Original
            return false;
        }
    }
}
