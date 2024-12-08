using HarmonyLib;
using Il2Cpp;
using Il2CppGameManagement;
using Il2CppMirror;

namespace DDSS_ConnectionFix.Patches
{
    [HarmonyPatch]
    internal class Patch_GameManager
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameManager), nameof(GameManager.RestartGame))]
        private static bool RestartGame_Prefix(GameManager __instance)
        {
            // Set State
            LobbyManager.instance.gameStarted = false;
            GameManager.oldLobbyCode = SteamLobby.instance.CurrentLobbyCode;
            SteamLobby.instance.SetLobbyStarted(false);

            // Change Scene to Lobby
            NetworkManager.singleton.ServerChangeScene("Scenes/LobbyScene");

            // Prevent Original
            return false;
        }
    }
}
