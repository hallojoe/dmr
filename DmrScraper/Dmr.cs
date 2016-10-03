using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dmr
{
    public static class Scraper
    {
        const string TOKEN_URL = "https://motorregister.skat.dk/dmr-front/appmanager/skat/dmr?_nfpb=true&_nfpb=true&_pageLabel=vis_koeretoej_side&_nfls=false";
        const string DATA_URL = "https://motorregister.skat.dk/dmr-front/appmanager/skat/dmr?_nfpb=true&_windowLabel=kerne_vis_koeretoej&kerne_vis_koeretoej_actionOverride=%2Fdk%2Fskat%2Fdmr%2Ffront%2Fportlets%2Fkoeretoej%2Fnested%2FfremsoegKoeretoej%2Fsearch&_pageLabel=vis_koeretoej_side";

        const string HIDDEN_TOKEN_NAME = "dmrFormToken";
        const string SEARCH_FORM_NAME = "kerne_vis_koeretoej{actionForm.soegeord}";

        private static Dmr.WebClient _webClient = new Dmr.WebClient();
        private static Parser _parser = new Parser();
        private static string _token = string.Empty;

        public static string Token { get { return _token; } }
        public static bool IsAuthenticated { get { return !string.IsNullOrEmpty(_token); } }

        private static void Authenticate(string token = "")
        {
            if (!string.IsNullOrEmpty(token))
            {
                _token = token;
                return;
            }
            string result = string.Empty;
            try
            {
                byte[] b = _webClient.DownloadData(TOKEN_URL);
                _parser.LoadHtml(Encoding.UTF8.GetString(b));
                _token = _parser.GetAuthenticationToken();
                if (string.IsNullOrEmpty(_token))
                    throw new Exception("Form token not found error");
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static string GetVehicleHtml(string licencePlate)
        {
            string result = string.Empty;
            try
            {
                Authenticate();
                NameValueCollection payload = new NameValueCollection() 
                { 
                     { HIDDEN_TOKEN_NAME, _token },  
                     { SEARCH_FORM_NAME, licencePlate }  
                };
                byte[] b = _webClient.UploadValues(DATA_URL, "POST", payload);
                result = Encoding.UTF8.GetString(b);
            }
            catch (Exception)
            {
                throw;
            }
            return result;
        }

        public static Request LookupVehicle(string licencePlate)
        {
            _parser.LoadHtml(GetVehicleHtml(licencePlate));
            
            var result = _parser.GetVehicle(); 
            var message = "OK";
            var success = true;
            if(result == null)
            {
                message = "Ingen køretøjer fundet";
                success = false;
            }
            return new Request()
            {
                Token = _token,
                Success = success,
                Message = message,
                Result = result
            };
        }

    }

}
