using CSOneDriveAccess;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Configuration;

namespace MVC.Controllers
{
    public class HomeController : Controller
    {
        //private const string CallbackUri = "http://iihs-onedrive.azurewebsites.net/Home/OnAuthComplate";
        //private const string ClientId = "00000000441C5F4E";
        //private const string Secret = "jkHwpOzCcXkXpYocXwLMsmz";

        

        public O365RestSession OfficeAccessSession
        {
            get
            {
                var officeAccess = Session["OfficeAccess"];
                if (officeAccess == null)
                {
                    
                    officeAccess = new O365RestSession(ConfigurationManager.AppSettings["ClientId"].ToString(), ConfigurationManager.AppSettings["Secret"].ToString(), ConfigurationManager.AppSettings["CallbackUri"].ToString());
                    Session["OfficeAccess"] = officeAccess;
                }
                return officeAccess as O365RestSession;
            }
        }

        public ActionResult Index(string id)
        {
            if (string.IsNullOrEmpty(OfficeAccessSession.AccessCode))
            {
                Session["UserCode"] = id;

                string url = OfficeAccessSession.GetLoginUrl("onedrive.readwrite offline_access");

                return new RedirectResult(url);
            }
            else
            {
                if (id != "0")
                    Session["UserCode"] = id;
            }

            if (id == "0")
            {
                return View();
            }
            else
            {
                return RedirectToAction("UploadFileAndGetShareUri");
            }
        }

        public async Task<ActionResult> UploadFileAndGetShareUri()
        {
            string strUserCode = Session["UserCode"].ToString();

            DownloadFile(ConfigurationManager.AppSettings["DownloadUrl"].ToString() + strUserCode.ToString() + ".docx", System.IO.Path.Combine(Server.MapPath("./docs/"), strUserCode.ToString() + ".docx"));

            string result = await OfficeAccessSession.UploadFileAsync(System.IO.Path.Combine(Server.MapPath("./docs/"), strUserCode.ToString() + ".docx"), strUserCode.ToString() + ".docx");

            JObject jo = JObject.Parse(result);
            string fileId = jo.SelectToken("id").Value<string>();

            string shareLink = await OfficeAccessSession.GetShareLinkAsync(fileId, OneDriveShareLinkType.edit, OneDrevShareScopeType.anonymous);

            return new RedirectResult(shareLink);
        }

        public async Task<RedirectResult> OnAuthComplate(string code)
        {
            await OfficeAccessSession.RedeemTokensAsync(code);

            return new RedirectResult("Index");
        }

        public static int DownloadFile(string remoteFilename, string localFilename)
        {
            int bytesProcessed = 0;
            Stream remoteStream = null;
            Stream localStream = null;
            WebResponse response = null;
            try
            {
                WebRequest request = WebRequest.Create(remoteFilename);
                if (request != null)
                {
                    response = request.GetResponse();
                    if (response != null)
                    {
                        remoteStream = response.GetResponseStream();
                        localStream = System.IO.File.Create(localFilename);

                        byte[] buffer = new byte[1024];
                        int bytesRead;
                        do
                        {
                            bytesRead = remoteStream.Read(buffer, 0, buffer.Length);
                            localStream.Write(buffer, 0, bytesRead);
                            bytesProcessed += bytesRead;
                        } while (bytesRead > 0);
                    }
                }
            }
            catch (System.Exception e)
            {
            }
            finally
            {
                if (response != null) response.Close();
                if (remoteStream != null) remoteStream.Close();
                if (localStream != null) localStream.Close();
            }
            return bytesProcessed;
        }
    }
}