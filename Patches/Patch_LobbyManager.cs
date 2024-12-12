using DDSS_ConnectionFix.Utils;
using HarmonyLib;
using Il2Cpp;
using Il2CppMirror;
using Il2CppUMUI;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace DDSS_ConnectionFix.Patches
{
    [HarmonyPatch]
    internal class Patch_LobbyManager
    {
        private static bool _startedCoroutine = false;

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
            if (!__instance.isServer
                || _startedCoroutine)
                return false;

            _startedCoroutine = true;

            __instance.gameStarted = true;
            SteamLobby.instance.SetLobbyStarted(true);

            UIManager.instance.OpenTab("LoadingScreen");

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
            _startedCoroutine = false;
            yield break;
        }
    }
}
