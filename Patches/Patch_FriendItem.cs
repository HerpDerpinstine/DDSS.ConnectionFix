using HarmonyLib;
using Il2Cpp;

namespace DDSS_ConnectionFix.Patches
{
    [HarmonyPatch]
    internal class Patch_FriendItem
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FriendItem), nameof(FriendItem.JoinPlayer))]
        private static bool JoinPlayer_Prefix(FriendItem __instance)
        {
            // Join Session
            ConnectionHandler.JoinLobby(__instance.lobbySteamID, false, __instance);

            // Prevent Original
            return false;
        }
    }
}