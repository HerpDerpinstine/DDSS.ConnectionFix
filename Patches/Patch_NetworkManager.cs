using HarmonyLib;
using Il2CppMirror;
using Il2CppUMUI;

namespace DDSS_ConnectionFix.Patches
{
    [HarmonyPatch]
    internal class Patch_NetworkManager
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(NetworkManager), nameof(NetworkManager.ClientChangeScene))]
        private static void ClientChangeScene_Prefix(string __0)
        {
            if (string.IsNullOrEmpty(__0)
                || string.IsNullOrWhiteSpace(__0)
                || (__0 == "Scenes/MainMenuScene")
                || (__0 == "Scenes/LobbyScene"))
                return;

            UIManager.instance.OpenTab("LoadingScreen");

            ConnectionHandler.SetPlayersAsReady();
        }
    }
}
