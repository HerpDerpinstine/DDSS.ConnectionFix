using HarmonyLib;
using Il2Cpp;
using Il2CppSteamworks;

namespace DDSS_ConnectionFix.Patches
{
    [HarmonyPatch]
    internal class Patch_SteamLobby
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SteamLobby), nameof(SteamLobby.Start))]
        private static void Start_Postfix(SteamLobby __instance)
            => __instance.LobbyEntered.add_m_Func((Callback<LobbyEnter_t>.DispatchDelegate)ConnectionHandler.OnClientStart);

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SteamLobby), nameof(SteamLobby.OnJoinRequest))]
        private static bool OnJoinRequest_Prefix(SteamLobby __instance, GameLobbyJoinRequested_t __0)
        {
            // Join Session
            ConnectionHandler.JoinLobby(__0.m_steamIDLobby, false, __instance);

            // Prevent Original
            return false;
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SteamLobby), nameof(SteamLobby.OnLobbyMatchList))]
        private static bool OnLobbyMatchList_Prefix(SteamLobby __instance, LobbyMatchList_t __0)
        {
            // Apply State
            __instance.receivedLobbyList = true;
            __instance.lobbyList = __0;
            __instance.isJoiningLobbyByCode = false;
            __instance.joinByCode = false;

            // Prevent Original
            return false;
        }
    }
}
