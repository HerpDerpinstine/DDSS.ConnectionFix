using DDSS_ConnectionFix.Utils;
using Il2Cpp;
using Il2CppGameManagement;
using Il2CppMirror;
using Il2CppSteamworks;
using Il2CppUMUI;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace DDSS_ConnectionFix
{
    internal static class ConnectionHandler
    {
        #region Private Members

        private static Coroutine _coroutine;
        private static MonoBehaviour _coroutineParent;

        #endregion

        #region Internal Members

        internal static ulong LobbyId = 0;
        internal static CSteamID LobbyIdSteam = new();

        #endregion

        #region Internal Methods

        internal static void JoinLobby(CSteamID id, bool isRejoin, float waitTime, MonoBehaviour parent)
            => JoinLobby(id.m_SteamID, isRejoin, waitTime, parent);
        internal static void JoinLobby(ulong id, bool isRejoin, float waitTime, MonoBehaviour parent)
        {
            if ((parent == null)
                || parent.WasCollected)
                return;

            NullAttempt();

            // Apply Requested SteamID
            LobbyId = id;
            LobbyIdSteam = new(LobbyId);

            // Fix Lobby Code
            SteamLobby.instance.CurrentLobbyCode = SteamMatchmaking.GetLobbyData(LobbyIdSteam, "CODE");
            GameManager.oldLobbyCode = SteamLobby.instance.CurrentLobbyCode;

            _coroutineParent = parent;
            _coroutine = _coroutineParent.StartCoroutine(JoinLobbyCoroutine(isRejoin, waitTime));
        }

        internal static IEnumerator JoinLobbyFromCode(string code)
        {
            SteamLobby.instance.CancelJoinLobby();
            ShowLoadingScreen(false);

            while (!SteamLobby.instance.receivedLobbyList)
                yield return null;

            SteamLobby.instance.isJoiningLobbyByCode = true;
            SteamLobby.instance.CurrentLobbyCode = code;
            SteamLobby.instance.joinByCode = true;

            SteamMatchmaking.AddRequestLobbyListStringFilter("STATE", "WAITING", ELobbyComparison.k_ELobbyComparisonEqual);
            SteamMatchmaking.AddRequestLobbyListStringFilter("CODE", code, ELobbyComparison.k_ELobbyComparisonEqual);
            SteamMatchmaking.AddRequestLobbyListDistanceFilter(ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide);
            SteamMatchmaking.RequestLobbyList();

            yield break;
        }

        internal static void OnFailure()
        {
            Cancel();
            UIManager.instance.CloseLoadingScreen();
            UIManager.instance.ShowPopUp(
                LocalizationManager.instance.GetLocalizedValue("Error"),
                LocalizationManager.instance.GetLocalizedValue("Could not join lobby!"),
                LocalizationManager.instance.GetLocalizedValue("Ok!"),
                "error");
        }

        internal static void OnClientStart(LobbyEnter_t lobby)
        {
            // Cache Lobby ID
            LobbyId = lobby.m_ulSteamIDLobby;
            LobbyIdSteam = new(lobby.m_ulSteamIDLobby);

            // Fix Lobby Code
            SteamLobby.instance.CurrentLobbyCode = SteamMatchmaking.GetLobbyData(LobbyIdSteam, "CODE");
            GameManager.oldLobbyCode = SteamLobby.instance.CurrentLobbyCode;
        }

        #endregion

        #region Private Methods

        private static void Cancel()
        {
            NullAttempt();
            SteamLobby.instance.CancelJoinLobby();
        }

        private static void NullAttempt()
        {
            if ((_coroutineParent != null)
                && !_coroutineParent.WasCollected
                && (_coroutine != null)
                && !_coroutine.WasCollected)
                _coroutineParent.StopCoroutine(_coroutine);

            _coroutineParent = null;
            _coroutine = null;
        }

        private static void ShowLoadingScreen(bool isRejoin)
		=> UIManager.instance.ShowLoadingScreen(
			$"{(isRejoin ? "Reconnecting to" : "Joining")} Lobby...", 
			(UnityAction)Cancel);

        private static IEnumerator JoinLobbyCoroutine(bool isRejoin, float waitTime)
        {
            // Show Loading Screen
            ShowLoadingScreen(isRejoin);

            // Provide Time
            yield return new WaitForSeconds(waitTime);

            // Attempt to join Lobby
            yield return ReattemptUntilTimeout(6, 5, () => !NetworkClient.active, JoinLobbyBySteamID);

            if (!NetworkClient.active)
                OnFailure();

            _coroutineParent = null;
            _coroutine = null;

            yield break;
        }

        private static IEnumerator JoinLobbyBySteamID()
        {
            SteamLobby.instance.CancelJoinLobby();

            // Wait until After First Lobby List request
            while (!SteamLobby.instance.receivedLobbyList)
                yield return null;

            // Apply State
            SteamLobby.instance.receivedLobbyList = false;
            SteamLobby.instance.joinByCode = false;
            SteamLobby.instance.CurrentLobbyCode = GameManager.oldLobbyCode;

            // Request Server List
            SteamMatchmaking.AddRequestLobbyListStringFilter("STATE", "WAITING", ELobbyComparison.k_ELobbyComparisonEqual);
            SteamMatchmaking.AddRequestLobbyListStringFilter("CODE", GameManager.oldLobbyCode, ELobbyComparison.k_ELobbyComparisonEqual);
            SteamMatchmaking.AddRequestLobbyListDistanceFilter(ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide);
            SteamMatchmaking.RequestLobbyList();

            // Await Response
            while (!SteamLobby.instance.receivedLobbyList)
                yield return null;

            // Attempt to Join
            SteamMatchmaking.JoinLobby(LobbyIdSteam);
        }

        private static IEnumerator ReattemptUntilTimeout(
            int attempts,
            int waitPerAttempt,
            Func<bool> checkState,
            Func<IEnumerator> onAttempt)
        {
            // Iterate Attempts
            // Attempts * SecondsPerAttempt = Timeout
            int attemptCount = 0;
            while (checkState()
                && (attemptCount < attempts))
            {
                // Increment Attempts
                attemptCount++;

                // Run Attempt
                yield return onAttempt();

                // Iterate Waits
                int waitCount = 0;
                while (checkState()
                    && (waitCount < waitPerAttempt))
                {
                    // Increment Waits
                    waitCount++;

                    // Wait a Second
                    yield return new WaitForSeconds(1f);
                }
            }
        }

        #endregion
    }
}
