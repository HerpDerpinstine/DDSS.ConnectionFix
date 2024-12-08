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
            ConnectionHandler.JoinLobby(__0.m_steamIDLobby, false, 0.5f, __instance);

            // Prevent Original
            return false;
        }
    }
}
