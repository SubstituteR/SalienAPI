using System;

namespace Saliens
{
    public class GameException : Exception { }
    class GameDownException : GameException { }
    class NoPlanetException : GameException { }

    #region EResult Exceptions
    class GameFail : InvalidGameResponse { public GameFail() : base(Network.SteamResponse.Fail) { } }
    class GameInvalidState : InvalidGameResponse { public GameInvalidState() : base(Network.SteamResponse.InvalidState) { } }
    class GameAccessDenied : InvalidGameResponse { public GameAccessDenied() : base(Network.SteamResponse.AccessDenied){} }
    class GameExpired : InvalidGameResponse { public GameExpired() : base(Network.SteamResponse.Expired) { } }
    class GameValueOutOfRange : InvalidGameResponse { public GameValueOutOfRange() : base(Network.SteamResponse.ValueOutOfRange) { } }
    class GameTimeNotSync : InvalidGameResponse { public GameTimeNotSync() : base(Network.SteamResponse.TimeNotSynced) { } }
    #endregion

    class InvalidParameterCountException : Exception { }

    public class InvalidGameResponse : GameException
    {
        public InvalidGameResponse(Network.SteamResponse EResult)
        {
            this.EResult = EResult;
        }
        public Network.SteamResponse EResult {  get; private set; }
    }
}
