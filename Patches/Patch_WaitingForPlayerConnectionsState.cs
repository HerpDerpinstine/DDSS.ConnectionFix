using DDSS_ConnectionFix.Utils;
using HarmonyLib;
using Il2Cpp;
using Il2CppGameManagement.StateMachine;
using Il2CppMirror;
using Il2CppPlayer.Lobby;
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

                foreach (NetworkIdentity networkIdentity in LobbyManager.instance.connectedLobbyPlayers)
                {
                    if ((networkIdentity == null)
                        || networkIdentity.WasCollected)
                        continue;

                    LobbyPlayer player = networkIdentity.GetComponent<LobbyPlayer>();
                    if ((player == null)
                        || player.WasCollected)
                        continue;

                    player.NetworkisReadyForGame = true;
                    player.NetworkisReadyForPlayerReplacement = true;
                }

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
                && !TutorialManager.instance.WasCollected
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
