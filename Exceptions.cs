using System;

namespace Saliens
{
    public class GameException :Exception
    {

    }
    class GameDownException : GameException
    {

    }
    class InvalidParameterCountException : Exception
    {

    }
    class NoPlanetException : GameException
    {

    }

    class InvalidHTTPResponse : Exception
    {

    }

    public class InvalidGameResponse : GameException
    {
        public InvalidGameResponse(int EResult)
        {
            this.EResult = EResult;
        }
        public int EResult {  get; private set; }
    }
}
