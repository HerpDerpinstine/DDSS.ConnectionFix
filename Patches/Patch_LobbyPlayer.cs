using HarmonyLib;
using Il2CppMirror;
using Il2CppPlayer.Lobby;

namespace DDSS_ConnectionFix.Patches
{
    [HarmonyPatch]
    internal class Patch_LobbyPlayer
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(LobbyPlayer), nameof(LobbyPlayer.NetworkisReadyForGame), MethodType.Getter)]
        private static bool NetworkisReadyForGame_get_Prefix(ref bool __result)
        {
            if (!NetworkServer.activeHost)
                return true;

            __result = true;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(LobbyPlayer), nameof(LobbyPlayer.NetworkisReadyForGame), MethodType.Setter)]
        private static void NetworkisReadyForGame_set_Prefix(ref bool __0)
        {
            if (!NetworkServer.activeHost)
                return;

            __0 = true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(LobbyPlayer), nameof(LobbyPlayer.NetworkisReadyForPlayerReplacement), MethodType.Getter)]
        private static bool NetworkisReadyForPlayerReplacement_get_Prefix(ref bool __result)
        {
            if (!NetworkServer.activeHost)
                return true;

            __result = true;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(LobbyPlayer), nameof(LobbyPlayer.NetworkisReadyForPlayerReplacement), MethodType.Setter)]
        private static void NetworkisReadyForPlayerReplacement_set_Prefix(ref bool __0)
        {
            if (!NetworkServer.activeHost)
                return;

            __0 = true;
        }
    }
}
