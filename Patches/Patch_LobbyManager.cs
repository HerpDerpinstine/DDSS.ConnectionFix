using DDSS_ConnectionFix.Utils;
using HarmonyLib;
using Il2Cpp;
using Il2CppMirror;
using Il2CppPlayer.Lobby;
using Il2CppUMUI;
using System.Collections;
using UnityEngine;

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
            // Prevent Original
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(LobbyManager), nameof(LobbyManager.StartGame))]
        private static bool StartGame_Prefix(LobbyManager __instance)
        {
            if (!__instance.isServer)
                return false;

            __instance.gameStarted = true;
            SteamLobby.instance.SetLobbyStarted(true);

            UIManager.instance.OpenTab("LoadingScreen");

            ConnectionHandler.SetPlayersAsReady();

            __instance.StartCoroutine(StartGameCoroutine(__instance.GetCurrentLevel().sceneName));

            // Prevent Original
            return false;
        }

        private static IEnumerator StartGameCoroutine(string sceneName)
        {
            // Wait for Initialization
            yield return new WaitForSeconds(1f);

            // Change Scene
            NetworkManager.singleton.ServerChangeScene(sceneName);

            // Finish Coroutine
            yield break;
        }
    }
}
