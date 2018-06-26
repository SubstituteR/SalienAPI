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
        public enum SteamResponse
        {
            Unknown,
            OK,
            Fail,
            InvalidState = 11,
            AccessDenied = 15,
            Expired = 27,
            NoMatch = 42,
            ValueOutOfRange = 77,
            TimeNotSynced = 93
        }

        private static HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(10)};

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
                throw new HttpRequestException("Response Code Was " + Response.StatusCode);
            }
            Enum.TryParse(Response.Headers.Where(x => x.Key == "X-eresult").FirstOrDefault().Value.FirstOrDefault(), out SteamResponse EResult);
            switch(EResult)
            {
                case SteamResponse.OK:
                    return await Response.Content.ReadAsStringAsync();
                case SteamResponse.Fail:
                    throw new GameFail();
                case SteamResponse.InvalidState:
                    throw new GameInvalidState();
                case SteamResponse.AccessDenied:
                    throw new GameAccessDenied();
                case SteamResponse.Expired:
                    throw new GameExpired();
                case SteamResponse.ValueOutOfRange:
                    throw new GameValueOutOfRange();
                case SteamResponse.TimeNotSynced:
                    throw new GameTimeNotSync();
                default:
                    throw new InvalidGameResponse(EResult);
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
