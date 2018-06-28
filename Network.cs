using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Reflection;

namespace Saliens
{

    public static class HelperExtensions
    {
        public static string AsInt(this bool val)
        {
            return (val ? 1 : 0).ToString();
        }
    }


    public class Network
    {
        public enum EResult
        {
            Unknown = 0,
            OK = 1,
            Fail = 2,
            InvalidParam = 8,
            Busy = 10,
            InvalidState = 11,
            AccessDenied = 15,
            Expired = 27,
            NoMatch = 42,
            ValueOutOfRange = 78,
            UnexpectedError = 79,
            TimeNotSynced = 93
        }

        private static HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

        /// <summary>
        /// Which API Endpoint to use.
        /// </summary>
        public enum EndPoint
        {
            ITerritoryControlMinigameService,
            IMiniGameService
        }

        /// <summary>
        /// Generic Response Class
        /// </summary>
        /// <typeparam name="T">The JSON Data Type</typeparam>
        internal class Response<T>
        {
            [JsonProperty(PropertyName = "response", Required = Required.Always)]
            internal T Data { get; private set; }
        }

        /// <summary>
        /// Processes the HttpResponseMessage and checks for errors.
        /// </summary>
        /// <param name="Response">HttpResponse from a HttpClient's request.</param>
        /// <returns>The content of the HttpResponse, or an error for invalid response.</returns>
        private static async Task<string> ProcessRequest(HttpResponseMessage Response)
        {
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                if (Response.StatusCode == HttpStatusCode.InternalServerError) throw new GameDownException();
                if (Response.StatusCode == HttpStatusCode.ServiceUnavailable || Response.StatusCode == (HttpStatusCode) 429)
                {
                    int.TryParse(Response.Headers.Where(x => x.Key == "Retry-After").FirstOrDefault().Value?.FirstOrDefault(), out int WaitTime);
                    throw new RateLimitException(WaitTime);
                }
                    throw new HttpRequestException("Response Code Was " + Response.StatusCode);
            }
            Enum.TryParse(Response.Headers.Where(x => x.Key == "X-eresult").FirstOrDefault().Value?.FirstOrDefault(), out EResult EResult);
            string EReason = Response.Headers.Where(x => x.Key == "X-error_message").FirstOrDefault().Value?.FirstOrDefault();
            switch (EResult)
            {
                case EResult.OK:
                    return await Response.Content.ReadAsStringAsync();
                case EResult.Fail:
                    throw new GameFail(EReason);
                case EResult.InvalidState:
                    throw new GameInvalidState(EReason);
                case EResult.AccessDenied:
                    throw new GameAccessDenied(EReason);
                case EResult.Expired:
                    throw new GameExpired(EReason);
                case EResult.NoMatch:
                    throw new GameNoMatch(EReason);
                case EResult.ValueOutOfRange:
                    throw new GameValueOutOfRange(EReason);
                case EResult.TimeNotSynced:
                    throw new GameTimeNotSync(EReason);
                default:
                    throw new InvalidGameResponse(EResult, EReason);
            }
        }


        private static async Task<string> Get(string method, EndPoint endpoint, int waittime = 0, params object[] data)
        {
            try
            {
                if (waittime > 0) await Task.Delay(waittime);
                if (data.Length % 2 != 0)
                {
                    throw new Exception("Invalid Parameter Length");
                }
                string request_string = "https://community.steam-api.com/" + endpoint + "/" + method + "/v0001/?";

                for (int i = 0; i < data.Length; i += 2)
                {
                    if (i > 0)
                    {
                        request_string += "&";
                    }
                    request_string += data[i].ToString() + "=" + data[i + 1].ToString();
                }
                return await ProcessRequest(await client.GetAsync(request_string));
            }catch (RateLimitException ratelimit)
            {
                Console.WriteLine($"[GET] Got Ratelimited -> Waiting {ratelimit.WaitTime} Until Retry");
                return await Get(method, endpoint, ratelimit.WaitTime, data);
            }
        }

        /// <summary>
        /// Sends a GET request to Valve's servers.
        /// </summary>
        /// <param name="method">The game method to call.</param>
        /// <param name="endpoint">The game endpoint.</param>
        /// <param name="data">The data to send.</param>
        /// <returns>The content of the HttpResponse, or an error for invalid response.</returns>
        public static async Task<string> Get(string method, EndPoint endpoint, params object[] data)
        {
            return await Get(method, endpoint, 0, data);
        }


        public static async Task<string> Post(string method, EndPoint endpoint, int waittime = 0, params object[] data)
        {
            try
            {
                if (waittime > 0) await Task.Delay(waittime);
                List<KeyValuePair<string, string>> Content = new List<KeyValuePair<string, string>> { };
                if (data.Length % 2 != 0)
                {
                    throw new InvalidParameterCountException();
                }
                for (int i = 0; i < data.Length; i += 2)
                {
                    Content.Add(new KeyValuePair<string, string>(data[i].ToString(), data[i + 1].ToString()));
                }

                return await ProcessRequest(await client.PostAsync("https://community.steam-api.com/" + endpoint + "/" + method + "/v0001", new FormUrlEncodedContent(Content)));
            }
            catch (RateLimitException ratelimit)
            {
                Console.WriteLine($"[POST] Got Ratelimited -> Waiting {ratelimit.WaitTime} Until Retry");
                return await Post(method, endpoint, ratelimit.WaitTime, data);
            }

        }

        /// <summary>
        /// Sends a POST request to Valve's servers.
        /// </summary>
        /// <param name="method">The game method to call.</param>
        /// <param name="endpoint">The game endpoint.</param>
        /// <param name="data">The data to send.</param>
        /// <returns></returns>
        public static async Task<string> Post(string method, EndPoint endpoint, params object[] data)
        {
            return await Post(method, endpoint, 0, data);
        }

        /// <summary>
        /// Deserialzes JSON into an object.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="JSON">The JSON to deserialze</param>
        /// <returns>An instance of the object with the data from the JSON.</returns>
        public static T Deserialize<T>(string JSON)
        {
            return JsonConvert.DeserializeObject<Response<T>>(JSON).Data;
        }
    }
}
