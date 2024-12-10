using DDSS_ConnectionFix.Utils;
using Il2Cpp;
using Il2CppGameManagement;
using Il2CppInterop.Runtime;
using Il2Cppkcp2k;
using Il2CppMirror;
using Il2CppMirror.FizzySteam;
using Il2CppSteamworks;
using Il2CppUMUI;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace DDSS_ConnectionFix.Handlers
{
    internal static class ConnectionHandler
    {
        #region Private Members

        private const float _waitTime = 0.5f;
        private const float _rejoinWaitTime = 1.5f;

        private static Coroutine _coroutine;
        private static MonoBehaviour _coroutineParent;

        private static ulong LobbyId = 0;
        private static CSteamID LobbyIdSteam = new();

        private static Il2CppSystem.Type _transportKCP = Il2CppType.Of<KcpTransport>();
        private static Il2CppSystem.Type _transportSteamworks = Il2CppType.Of<FizzySteamworks>();

        #endregion

        #region Internal Members

        internal enum eJoinType
        {
            STEAM_ID,
            INVITE_CODE,
            DIRECT_IP
        }

        internal static string LobbyAddr;

        #endregion

        #region Internal Methods

        internal static void ReconnectToLobby(MonoBehaviour parent)
        {
            if (!string.IsNullOrEmpty(LobbyAddr))
                JoinLobbyByIP(LobbyAddr, true, parent);
            else
                JoinLobby(LobbyId, true, parent);
        }
        internal static void JoinLobby(CSteamID id, bool isRejoin, MonoBehaviour parent)
            => JoinLobby(id.m_SteamID, isRejoin, parent);
        internal static void JoinLobby(ulong id, bool isRejoin, MonoBehaviour parent)
        {
            if (parent == null
                || parent.WasCollected)
                return;

            Cancel();

            LobbyAddr = null;
            LobbyId = id;
            LobbyIdSteam = new(LobbyId);

            _coroutineParent = parent;
            _coroutine = _coroutineParent.StartCoroutine(JoinLobbyCoroutine(isRejoin, eJoinType.STEAM_ID));
        }

        internal static void JoinLobbyByCode(string code, bool isRejoin, MonoBehaviour parent)
        {
            if (parent == null
                || parent.WasCollected)
                return;

            Cancel();

            LobbyAddr = null;
            SteamLobby.instance.CurrentLobbyCode = code;
            GameManager.oldLobbyCode = code;

            _coroutineParent = parent;
            _coroutine = _coroutineParent.StartCoroutine(JoinLobbyCoroutine(isRejoin, eJoinType.INVITE_CODE));
        }

        internal static void JoinLobbyByIP(string addr, bool isRejoin, MonoBehaviour parent)
        {
            if (parent == null
                || parent.WasCollected)
                return;

            Cancel();

            LobbyAddr = addr;
            SteamLobby.instance.CurrentLobbyCode = addr;
            GameManager.oldLobbyCode = addr;

            _coroutineParent = parent;
            _coroutine = _coroutineParent.StartCoroutine(JoinLobbyCoroutine(isRejoin, eJoinType.DIRECT_IP));
        }

        internal static void OnFailure()
        {
            Cancel();
            LobbyAddr = null;

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
            LobbyIdSteam = new(LobbyId);

            // Fix Lobby Code
            if (!NetworkServer.activeHost)
            {
                SteamLobby.instance.CurrentLobbyCode = SteamMatchmaking.GetLobbyData(LobbyIdSteam, "CODE");
                GameManager.oldLobbyCode = SteamLobby.instance.CurrentLobbyCode;
            }
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
            if (_coroutineParent != null
                && !_coroutineParent.WasCollected
                && _coroutine != null
                && !_coroutine.WasCollected)
                _coroutineParent.StopCoroutine(_coroutine);

            _coroutineParent = null;
            _coroutine = null;
        }

        private static IEnumerator ShowLoadingScreen(bool isRejoin)
        {
            // Show Loading Screen
            UIManager.instance.ShowLoadingScreen(
                $"{(isRejoin ? "Reconnecting to" : "Joining")} Lobby...",
                (UnityAction)Cancel);
            yield return new WaitForSeconds(isRejoin ? _rejoinWaitTime : _waitTime);
            yield break;
        }

        private static IEnumerator JoinLobbyCoroutine(bool isRejoin, eJoinType joinType)
        {
            // Show Loading Screen
            yield return ShowLoadingScreen(isRejoin);

            // Attempt to join Lobby
            yield return ReattemptUntilTimeout(6, 5,
                () => !NetworkClient.active,
                joinType == eJoinType.INVITE_CODE
                ? RequestLobbyByCode
                : joinType == eJoinType.DIRECT_IP
                    ? RequestLobbyByIP
                    : RequestLobbyBySteamID);

            if (!NetworkClient.active)
                OnFailure();

            _coroutineParent = null;
            _coroutine = null;
            yield break;
        }

        private static IEnumerator RequestLobbyByIP()
        {
            // Cancel Current Attempt
            SteamLobby.instance.CancelJoinLobby();

            // Wait until After First Lobby List request
            while (!SteamLobby.instance.receivedLobbyList)
                yield return null;

            // Switch Transport
            yield return SwitchTransport(_transportKCP);

            // Attempt to Join
            NetworkManager.singleton.networkAddress = LobbyAddr;
            NetworkManager.singleton.StartClient();
            yield break;
        }

        private static IEnumerator RequestLobbyBySteamID()
        {
            // Cancel Current Attempt
            SteamLobby.instance.CancelJoinLobby();

            // Wait until After First Lobby List request
            while (!SteamLobby.instance.receivedLobbyList)
                yield return null;

            // Switch Transport
            yield return SwitchTransport(_transportSteamworks);

            // Apply State
            SteamLobby.instance.receivedLobbyList = false;
            SteamLobby.instance.joinByCode = false;

            // Request Server List
            SteamMatchmaking.AddRequestLobbyListStringFilter("STATE", "WAITING", ELobbyComparison.k_ELobbyComparisonEqual);
            SteamMatchmaking.AddRequestLobbyListDistanceFilter(ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide);
            SteamMatchmaking.RequestLobbyList();

            // Await Response
            while (!SteamLobby.instance.receivedLobbyList)
                yield return null;

            // Attempt to Join
            SteamMatchmaking.JoinLobby(LobbyIdSteam);
            yield break;
        }

        private static IEnumerator RequestLobbyByCode()
        {
            // Cancel Current Attempt
            SteamLobby.instance.CancelJoinLobby();

            // Wait until After First Lobby List request
            while (!SteamLobby.instance.receivedLobbyList)
                yield return null;

            // Switch Transport
            yield return SwitchTransport(_transportSteamworks);

            // Apply State
            SteamLobby.instance.receivedLobbyList = false;
            SteamLobby.instance.joinByCode = false;

            // Request Server List
            SteamMatchmaking.AddRequestLobbyListStringFilter("STATE", "WAITING", ELobbyComparison.k_ELobbyComparisonEqual);
            SteamMatchmaking.AddRequestLobbyListStringFilter("CODE", GameManager.oldLobbyCode, ELobbyComparison.k_ELobbyComparisonEqual);
            SteamMatchmaking.AddRequestLobbyListDistanceFilter(ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide);
            SteamMatchmaking.RequestLobbyList();

            // Await Response
            while (!SteamLobby.instance.receivedLobbyList)
                yield return null;

            // Attempt to Join
            if (SteamLobby.instance.lobbyList.m_nLobbiesMatching > 0)
            {
                LobbyIdSteam = SteamMatchmaking.GetLobbyByIndex(0);
                LobbyId = LobbyIdSteam.m_SteamID;
                SteamMatchmaking.JoinLobby(LobbyIdSteam);
            }
            yield break;
        }

        private static IEnumerator SwitchTransport(Il2CppSystem.Type transportType)
        {
            if (TransportSwitcher.instance != null
                && !TransportSwitcher.instance.WasCollected)
            {
                bool shouldSwitch = false;

                if (TransportSwitcher.instance.networkManagerInstance != null
                    && !TransportSwitcher.instance.networkManagerInstance.WasCollected)
                {
                    if (TransportSwitcher.instance.networkManagerInstance.GetIl2CppType() != transportType)
                        shouldSwitch = true;
                    if (shouldSwitch)
                        UnityEngine.Object.Destroy(TransportSwitcher.instance.networkManagerInstance);
                }
                else
                    shouldSwitch = true;

                if (shouldSwitch)
                {
                    yield return new WaitForSeconds(0.1f);

                    TransportSwitcher.instance.networkManagerInstance =
                        UnityEngine.Object.Instantiate(transportType == _transportKCP
                        ? TransportSwitcher.instance.KCPPrefab
                        : TransportSwitcher.instance.FizzyPrefab);

                    yield return new WaitForSeconds(0.1f);
                }
            }
            yield break;
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
            while (true)
            {
                // Increment Attempts
                attemptCount++;

                // Run Attempt
                yield return onAttempt();

                // Iterate Waits
                int waitCount = 0;
                while (true)
                {
                    // Increment Waits
                    waitCount++;

                    // Wait a Second
                    yield return new WaitForSeconds(1f);

                    if (!checkState()
                        || waitCount >= waitPerAttempt)
                        break;
                }

                if (!checkState()
                    || attemptCount >= attempts)
                    break;
            }
            yield break;
        }

        #endregion
    }
}
