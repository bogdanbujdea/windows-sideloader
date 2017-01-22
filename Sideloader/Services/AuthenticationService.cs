using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Sideloader.Settings;

namespace Sideloader.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        public async Task<Report> LoginAsync(string username, string password)
        {
            var report = new Report();
            try
            {
                var handler = new HttpClientHandler()
                {
                    AllowAutoRedirect = true,
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };

                HttpClient httpClient = new HttpClient(handler);
                var pairs = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("username", username),
                    new KeyValuePair<string, string>("password", password),
                    new KeyValuePair<string, string>("Login", "Submit")
                };
                var stringContent = new FormUrlEncodedContent(pairs);
                httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
                httpClient.DefaultRequestHeaders.Add("Referer",
                    "http://builds.showpad.com/showpad/showpad/windows?download=1");
                httpClient.DefaultRequestHeaders.Add("Accept",
                    "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                var response = await httpClient.PostAsync(new Uri("http://builds.showpad.com/login.php"), stringContent);
                report.IsSuccessful = response.IsSuccessStatusCode;
            }
            catch (WebException webException)
            {
                if (webException.Status == WebExceptionStatus.ConnectFailure)
                {
                    report.ErrorMessage = "Verifica conexiunea la internet";
                }
            }
            catch (Exception exception)
            {
                Logger.Instance.Error("LoginAsync", exception);
                report.ErrorMessage = exception.Message;
            }
            return report;
        }
    }
}