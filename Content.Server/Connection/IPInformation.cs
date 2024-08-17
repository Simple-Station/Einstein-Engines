using System.Collections.Immutable;
using System.Threading.Tasks;
using Content.Server.Database;
using Content.Server.GameTicking;
using Content.Server.Preferences.Managers;
using Content.Shared.CCVar;
using Content.Shared.GameTicking;
using Content.Shared.Players.PlayTimeTracking;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Log;
using System.Net.Http;
using System.Web;
using Newtonsoft.Json.Linq;

namespace Content.Server.Connection
{
    /// <summary>
    /// Gets (& caches) information about IP addresses
    /// </summary>
    public interface IIPInformation
    {
        void Initialize();
        Task<IPData> GetIPInformationAsync(System.Net.IPAddress IP);
    }

    public class IPData
    {
        /// <summary>
        /// Two letter country code
        /// </summary>
        public string country = "";

        /// <summary>
        /// Should the IP be considered suspicious/VPN?
        /// </summary>
        public bool suspiciousIP = false;

        /// <summary>
        ///  Different services will likely implement this score differently, so rely on the bool instead.
        /// </summary>
        public double suspiciousScore = -1;
    }

    /// <summary>
    /// Get & Cache IP information via free IP Intel service
    /// </summary>
    public sealed class IPInformationIPIntel : IIPInformation
    {
        [Dependency] private readonly ILogManager _logManager = default!;
        [Dependency] private readonly IConfigurationManager _cfg = default!;

        private HttpClient Client = new();
        private ISawmill _logger = default!;


        private Dictionary<System.Net.IPAddress, IPData> cache = new();

        public void Initialize()
        {
            _logger = _logManager.GetSawmill("ip_information");
            _logger.Level = LogLevel.Debug;

            //Client = new HttpClient(HappyEyeballsHttp.CreateHttpHandler());  // FYI: Robust blocks access to HappyEyeballsHttp
            Client.Timeout = TimeSpan.FromSeconds(10); // Might want to cvar this?  This may increase the amount of time new player sits on connecting... screen.  5 sec is normal and expected
        }

        public async Task<IPData> GetIPInformationAsync(System.Net.IPAddress IP)
        {
            if (cache.ContainsKey(IP))
                return cache[IP];

            // Debug code to test high VPN score:
            /*
            return new IPData()
            {
                country = "RU",
                suspiciousScore = 1,
                suspiciousIP = true
            };
            */

            double blockIPThreshold = _cfg.GetCVar(CCVars.VPNBlockThresholdToBlockIP);
            string adminEmailForIpIntelService = _cfg.GetCVar(CCVars.VPNBlockAdminEmail);

            if (string.IsNullOrEmpty(adminEmailForIpIntelService))
            {
                throw new Exception("Can't get IP Intel for VPN Block with no admin email set.");
            }

            string url = "http://check.getipintel.net/check.php?ip=" +
                HttpUtility.UrlEncode(IP.ToString()) +
                "&contact=" +
                HttpUtility.UrlEncode(adminEmailForIpIntelService) +
                "&flags=f&oflags=c&format=json";

            // Normally wouldn't want to log this as it does include admin e-mail
            //_logger.Debug("Sending to... " + url);

            var ipInformationRequest = new HttpRequestMessage(HttpMethod.Get, url);

            var ipInformationResponse = await Client.SendAsync(ipInformationRequest);

            ipInformationResponse.EnsureSuccessStatusCode();

            string responseContent = await ipInformationResponse.Content.ReadAsStringAsync();

            // Normally wouldn't want to log this as it does include admin e-mail
            //_logger.Debug("Got response: " + responseContent);

            // Parse json and fill into IPData
            JObject json = JObject.Parse(responseContent);

            if (json == null) // (Robust forcing pedantic exception handling)
            {
                throw new Exception("Json parse error");
            }

            // Additionally, since robust forces pedantic error checking, the json extraction code is a bit ugly.
            // (Normally can just type cast inline and rely on exception handling...  You may wish to convert this over
            // to however robust normally deserializes json instead of using JSON.Net)
            string jsonStatus = json["status"]?.ToObject<string>() ?? "";
            string jsonCountry = json["Country"]?.ToObject<string>() ?? "";
            double jsonSuspiciousScore = json["result"]?.ToObject<double>() ?? -1;

            if (jsonStatus != "success")
            {
                throw new Exception("Unexpected status from getipintel: " + jsonStatus);
            }

            IPData ipData = new()
            {
                country = jsonCountry,
                suspiciousScore = jsonSuspiciousScore
            };

            // Compute if suspicious
            if (ipData.suspiciousScore >= blockIPThreshold)
                ipData.suspiciousIP = true;

            // Cache for later
            cache.Add(IP, ipData);

            return ipData;

        }
    }
}
