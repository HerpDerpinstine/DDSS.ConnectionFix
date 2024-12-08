using HarmonyLib;
using Il2Cpp;

namespace DDSS_ConnectionFix.Patches
{
    [HarmonyPatch]
    internal class Patch_LobbyItem
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(LobbyItem), nameof(LobbyItem.JoinLobby))]
        private static bool JoinLobby_Prefix(LobbyItem __instance)
        {
            // Join Session
            ConnectionHandler.JoinLobby(__instance.lobbyID, false, 0.5f, __instance);

            // Prevent Original
            return false;
        }
    }
}