using DDSS_ConnectionFix.Utils;
using HarmonyLib;
using Il2Cpp;
using Il2CppGameManagement.StateMachine;
using System.Collections;
using UnityEngine;

namespace DDSS_ConnectionFix.Patches
{
    [HarmonyPatch]
    internal class Patch_WaitingForPlayerConnectionsState
    {
        private static bool _startedCoroutine = false;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(WaitingForPlayerConnectionsState), nameof(WaitingForPlayerConnectionsState.ServerUpdate))]
        private static bool ServerUpdate_Prefix(WaitingForPlayerConnectionsState __instance)
        {
            // Start Game
            if (!_startedCoroutine)
            {
                _startedCoroutine = true;
                __instance.gameManager.StartCoroutine(StartGameCoroutine(__instance));
            }

            // Prevent Original
            return false;
        }

        private static IEnumerator StartGameCoroutine(WaitingForPlayerConnectionsState __instance)
        {
            // Wait for Initialization
            yield return new WaitForSeconds(1f);

            // Change State
            if ((TutorialManager.instance != null)
                && TutorialManager.instance.WasCollected
                && TutorialManager.instance.isTutorialActive)
            {
                __instance.ChangeState(GameStates.Tutorial);
                __instance.gameManager.NetworktargetGameState = (int)GameStates.Tutorial;
            }
            else
            {
                __instance.ChangeState(GameStates.StartGame);
                __instance.gameManager.NetworktargetGameState = (int)GameStates.StartGame;
            }

            // Finish Coroutine
            _startedCoroutine = false;
            yield break;
        }
    }
}
