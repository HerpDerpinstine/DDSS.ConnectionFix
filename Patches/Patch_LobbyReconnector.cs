using HarmonyLib;
using Il2Cpp;
using Il2CppGameManagement;

namespace DDSS_ConnectionFix.Patches
{
    [HarmonyPatch]
    internal class Patch_LobbyReconnector
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(LobbyReconnector), nameof(LobbyReconnector.Start))]
        private static bool Start_Prefix(LobbyReconnector __instance)
        {
            // Rejoin Session
            if (GameManager.startClientAgain)
            {
                GameManager.startClientAgain = false;
                ConnectionHandler.ReconnectToLobby(__instance);
            }
            else
                ConnectionHandler.LobbyAddr = null;

            // Prevent Original
            return false;
        }
    }
}
