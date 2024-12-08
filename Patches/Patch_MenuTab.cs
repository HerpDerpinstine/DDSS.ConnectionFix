using DDSS_ConnectionFix.Utils;
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
            try
            {
                TransportSwitcher.instance.SwitchTransportToFizzySteamworks();
                __instance.StartCoroutine(ConnectionHandler.JoinLobbyFromCode(code));
            }
            catch
            {
                ConnectionHandler.OnFailure();
            }

            // Prevent Original
            return false;
        }
    }
}
