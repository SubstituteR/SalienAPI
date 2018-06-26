using System;

namespace Saliens
{
    public class GameException : Exception { }
    public class GameDownException : GameException { }
    public class NoPlanetException : GameException { }

    #region EResult Exceptions
    public class GameFail : InvalidGameResponse { public GameFail() : base(Network.SteamResponse.Fail) { } }
    public class GameInvalidState : InvalidGameResponse { public GameInvalidState() : base(Network.SteamResponse.InvalidState) { } }
    public class GameAccessDenied : InvalidGameResponse { public GameAccessDenied() : base(Network.SteamResponse.AccessDenied){} }
    public class GameExpired : InvalidGameResponse { public GameExpired() : base(Network.SteamResponse.Expired) { } }
    public class GameValueOutOfRange : InvalidGameResponse { public GameValueOutOfRange() : base(Network.SteamResponse.ValueOutOfRange) { } }
    public class GameTimeNotSync : InvalidGameResponse { public GameTimeNotSync() : base(Network.SteamResponse.TimeNotSynced) { } }
    #endregion

    public class InvalidParameterCountException : Exception { }

    public class InvalidGameResponse : GameException
    {
        public InvalidGameResponse(Network.SteamResponse EResult)
        {
            this.EResult = EResult;
        }
        public Network.SteamResponse EResult {  get; private set; }
    }
}
