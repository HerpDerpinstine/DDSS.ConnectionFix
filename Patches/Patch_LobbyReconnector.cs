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
                ConnectionHandler.JoinLobby(ConnectionHandler.LobbyIdSteam, true, 0.5f, __instance);
            }

            // Prevent Original
            return false;
        }
    }
}
