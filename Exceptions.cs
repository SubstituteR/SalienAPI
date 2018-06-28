using System;

namespace Saliens
{
    public class GameException : Exception { }
    public class GameDownException : GameException { }
    public class NoPlanetException : GameException { }

    #region EResult Exceptions
    public class GameFail : InvalidGameResponse { public GameFail(string EReason) : base(Network.EResult.Fail, EReason) { } }
    public class GameInvalidState : InvalidGameResponse { public GameInvalidState(string EReason) : base(Network.EResult.InvalidState, EReason) { } }
    public class GameAccessDenied : InvalidGameResponse { public GameAccessDenied(string EReason) : base(Network.EResult.AccessDenied, EReason) {} }
    public class GameExpired : InvalidGameResponse { public GameExpired(string EReason) : base(Network.EResult.Expired, EReason) { } }
    public class GameValueOutOfRange : InvalidGameResponse { public GameValueOutOfRange(string EReason) : base(Network.EResult.ValueOutOfRange, EReason) { } }
    public class GameTimeNotSync : InvalidGameResponse { public GameTimeNotSync(string EReason) : base(Network.EResult.TimeNotSynced, EReason) { } }
    #endregion

    public class InvalidParameterCountException : Exception { }
    public class RateLimitException : Exception
    {
        public int WaitTime { get; private set; }
        public RateLimitException(int WaitTime)
        {
            this.WaitTime = WaitTime;
        }
    }
    public class InvalidGameResponse : GameException
    {
        public InvalidGameResponse(Network.EResult EResult, string EReason)
        {
            this.EResult = EResult;
            this.EReason = EReason;
        }
        public Network.EResult EResult {  get; private set; }
        public string EReason { get; private set; }
    }
}
