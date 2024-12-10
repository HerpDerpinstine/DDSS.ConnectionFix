using Il2Cpp;
using Il2CppMirror;
using Il2CppPlayer.Lobby;

namespace DDSS_ConnectionFix.Handlers
{
    internal static class MatchHandler
    {
        internal static bool HasAllPlayersLoaded()
        {
            foreach (NetworkIdentity networkIdentity in LobbyManager.instance.connectedLobbyPlayers)
            {
                if ((networkIdentity == null)
                    || networkIdentity.WasCollected)
                    continue;

                LobbyPlayer player = networkIdentity.GetComponent<LobbyPlayer>();
                if ((player == null)
                    || player.WasCollected
                    || !player.isReadyForGame)
                    return false;
            }

            return true;
        }
    }
}
