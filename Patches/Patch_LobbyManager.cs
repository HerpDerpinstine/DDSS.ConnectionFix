using HarmonyLib;
using Il2Cpp;
using Il2CppMirror;
using Il2CppUMUI;

namespace DDSS_ConnectionFix.Patches
{
    [HarmonyPatch]
    internal class Patch_LobbyManager
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(LobbyManager), nameof(LobbyManager.ShowLoadingScreenRPC))]
        private static bool ShowLoadingScreenRPC_Prefix()
        {
            // Prevent Original
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(LobbyManager), nameof(LobbyManager.InvokeUserCode_ShowLoadingScreenRPC))]
        private static bool InvokeUserCode_ShowLoadingScreenRPC_Prefix(LobbyManager __instance)
        {
            // Force-Show Loading Screen on Host Client
            if (NetworkServer.activeHost)
                UIManager.instance.OpenTab("LoadingScreen");

            // Prevent Original
            return false;
        }
    }
}
