using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;
using System.Configuration;
using System.Net.Mail;
using System.Globalization;
using System.Xml;
using System.Drawing.Imaging;
using System.Drawing;
//using Newtonsoft.Json;
using MongoDB.Bson;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using iProdDataModel.Models;
using MongoDB.Driver;
//using MySql.Data.MySqlClient;
//using MySqlX.XDevAPI;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Security.Claims;
using System.Web;
using EventLog = iProdDataModel.Models.EventLog;
using UT = iProdWHSE.utility;
using System.Net.NetworkInformation;

namespace iProdWHSE
{
    public static class utility
    {

        #region DICHIARAZIONI


        public static string Versione { get; set; } = "4.1.16@22.3.23a"; // versione iprod @ data
        // note relative al commit e/o modifiche rilevanti
        public static string Versione_Commit { get; set; } = "";
        public static Form1 mainForm { get; set; }
        public static iProdCustomers Tenant { get; set; }
        public static Customerusers iprod_loggeduser { get; set; }
        public static WHSEInfo iProdCFG { get; set; }
        public static WHSEInfo curMV { get; set; }      // MV selezionato
        public static List<WHSEInfo> HwdSupportedModels { get; set; } 
        public static string EndPointIPROD { get; set; } = "https://localhost:44375/api/";
        public static string EndPointCOMPANY { get; set; } // -> endpoint-lireco; http://192.168.1.254/jwscom/services/Com
        public static bool MockWS { get; set; }
        public static string LogFile { get; set; }
        public static string HistFile { get; set; }
        public static int HistCount { get; set; }
        public static string cfgFile { get; set; }
        public static string ErrFile { get; set; }
        public static string CsvFile { get; set; }
        public static string stringLogin { get; set; }
        public static string SharedFile { get; set; }
        public static string eMailFile { get; set; }
        public static string KeysFile { get; set; }
        public static string SVCPausedUntil { get; set; }
        public static string SVCStatus { get; set; }
        public static DateTime ControlledLastActivity { get; set; }
        public static DateTime SVCWaitTo { get; set; }
        private static DateTime lastlog { get; set; }
        public static DateTime lastMailSent { get; set; }


        public static string LF = "\r\n";
        public static bool IsNull(this string v) => string.IsNullOrEmpty(v) || v == ObjectId.Empty.ToString();
        public static bool NotNull(string v) => !IsNull(v);
        private static HttpStatusCode isOk = HttpStatusCode.OK;
        public static bool connecting = false;

        #endregion

        #region Configurazione

        // non tracciate


       
        public static string pathApp { get; set; }
        public static string pathData { get; set; }
        public static string pathLog { get; set; }         // path anche dei dati
        public static string pathBackups { get; set; }
        public static string pathCache { get; set; }
        public static string Filexfolderdoc { get; set; }
        public static string eMailNotifiche { get; set; }
        public static bool deleteAll = false;
        public static bool deleteFirst = false;
        public static bool skipDownloads = false;


        public static string EndPointKey { get; set; }  // dev, test, prod, localhost
        public static string iProdUsername { get; set; }
        public static string iProdPassword { get; set; }

        public static bool CacheEnabled { get; set; }
        public static bool CacheRefresh { get; set; }
        public static bool EnableLogToDisk { get; set; }
        public static bool TimerAutostart { get; set; }
        public static bool SendEmail { get; set; }
        public static long myTimerInterval { get; set; }
        public static int MaxDelayInMinutes { get; set; }
        public static int TimerIdleFromHour { get; set; }
        public static int TimerIdleToHour { get; set; }

        public static bool TermsAccepted { get; set; }
      
        public static bool iProdConnected { get; set; }
        public static bool WSConnected { get; set; }
        public static bool ListenerUP { get; set; }
        public static bool checkBackup { get; set; }        // true se in config c'è backup=true controlla i servizi di backup prima di avviare il processo di sync
        public static int MaxHistoryCount { get; set; }
        public static int MaxProcessCount { get; set; }
        public static bool SlowRun { get; set; }
        public static bool StopAtError { get; set; }
        public static bool Interactive { get; set; }  // deve o non deve emettere messaggi a video
        public static string runningmode { get; set; }
        public static bool Simulating { get; set; }  // true se è attiva una simulazione
        public static bool SimAlertDone { get; set; } // true se ha gia avvertito l'utente sulla configurazione ottimale per una simulazione
        public static bool isStartingUp { get; set; }
        public static string FileSpy { get; set; }  // semaforo


        #region APPLICATION OBJECTS



        public static iProdCustomers iprod_user { get; set; } // viene caricato una volta sola all'inizio e resta per tutta la sessione degli imports 
        public static List<Customers> iprod_customers { get; set; }
        public static List<Phase> iprod_phases { get; set; }
        public static List<PhaseInstance> iprod_phaseinstances { get; set; }
        public static List<Items> iprod_items { get; set; }
        public static List<Bom> iprod_boms { get; set; }
        public static List<Contact> iprod_contacts { get; set; }
        public static List<Destination> iprod_destinations { get; set; }
        public static List<Warehouse> warehouses { get; set; }
        public static List<Customermachine> iprod_machines { get; set; }
        public static List<Document> iprod_purchaseorders { get; set; }
        public static List<Posts> iprod_purchaseordersdocs { get; set; }
        public static List<SalesOrder> iprod_salesorders { get; set; }
        public static List<WorkOrders> temp_wos { get; set; }
        public static List<WorkOrders> iprod_workorders { get; set; }
        public static List<Posts> iprod_posts;
        public static Dictionary<string, string> categories = new Dictionary<string, string>();


        #endregion


        public static void setApplicationPaths()
        {
            iProdCFG = new WHSEInfo();

            pathApp = Application.StartupPath;
            pathData = Application.LocalUserAppDataPath + "\\Data\\";

            pathLog = pathData;
            pathBackups = pathLog + "bkLogs\\";

            if (!Directory.Exists(pathLog)) Directory.CreateDirectory(pathLog); 
            if (!Directory.Exists(pathData)) Directory.CreateDirectory(pathData);
            if (!Directory.Exists(pathBackups)) Directory.CreateDirectory(pathBackups);

            string pt = pathData + "Requests\\";
            if (!Directory.Exists(pt)) Directory.CreateDirectory(pt);
            pt = pathData + "Responses\\";
            if (!Directory.Exists(pt)) Directory.CreateDirectory(pt);

            cfgFile = pathData + "config.txt";
            LogFile = pathData + "Lastlog.txt";
            HistFile = pathData + "DataHistory.txt";

            ErrFile = pathData + "exceptions.txt";
            SharedFile = pathData + "semaforo.txt"; // non usato
            FileSpy = pathData + "iprodwhse-semaphore.spy";


            if (!FileExists(cfgFile)) createInitialConfig();

        }


        /// <summary>
        /// Carica spezzoni di testo dal file Snippets.txt
        /// </summary>
        /// <param name="key">nome della chiave senza il cartattere §</param>
        /// <returns></returns>
        public static string loadSnippet(string key)
        {
            try
            {
                string ret = "";
                bool open = false;
                string fn = "snippets.txt";
                if (!File.Exists(fn)) throw new Exception($"Errore grave in loadSnippet: non trovato il file snippets.txt per key '{key}'");


                var ls = LoadTextFile(fn);

                foreach (var line in ls)
                {
                    string s = line.Trim();

                    if (s.StartsWith("§") && open)
                        return ret;
                    else if (s.StartsWith("§"))
                    {
                        string k = s.Substring(1, s.Length - 1);
                        if (k == key)
                        {
                            open = true;
                            continue;
                        }
                    }

                    if (open) ret += s +'\n';
                }

                return ret;
            }
            catch
            {
                throw;
            }
        }


        public static void createInitialConfig()
        {

            var c0 = loadSnippet("default-config");
            var lst = c0.Split('\n').ToList();
            parseConfig(lst);
            iProdCFG.SaveSettings(UT.cfgFile);

        }


        public static string GetLocalIP()
        {




            string strHostName = string.Empty;
            // Getting Ip address of local machine...
            // First get the host name of local machine.
            strHostName = Dns.GetHostName();
            
            // Then using host name, get the IP address list..
            IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);
            IPAddress[] addresses = ipEntry.AddressList;

            var ip = addresses.FirstOrDefault(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
                                                && x.ToString().StartsWith("192"));
            if (ip != null) return ip.ToString();

            foreach (IPAddress addr in addresses)
            {
                if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return addr.ToString();
                }
            }

         











            //IPAddress[] addresses = Dns.GetHostAddresses("localhost");

            //foreach (IPAddress addr in addresses)
            //{
            //    if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            //    {
            //        return addr.ToString();
            //    }
            //}

            return "127.0.0.1";
        }

        public static void parseConfig(List<string> lst)
        {


            // imposta i defaults. se non trova le chiavi scritte sul file
            // tiene questi valori

            iProdCFG = new WHSEInfo();

         
            EnableLogToDisk = true; // default, se non c'è la key resta questa

            foreach (var str in lst)
            {
                iProdCFG.SetProperty(str, ';','#');
            }

            if (iProdCFG.TimerInterval <= 0) iProdCFG.TimerInterval = 900; // 15 min

            EndPointCOMPANY = iProdCFG.Ambiente;


        }


        public static string MyDecrypt(string v)
        {

            // sottrae 5 alla sequenza ascii della stringa
            // se l'operazione genera un ; lo sostituisce con § perche schianterebbe, essendo il separatore
            string ret = "";
            int factorial = 5;
            var bytes = v.Replace("§", ";").ToCharArray();

            foreach (var a in bytes)
            {
                var t = (int)a;
                char character = (char)(t - factorial);
                ret += character.ToString();

            }

            return ret;
        }

        public static string MyCrypt(string v)
        {
            // aggiunge 1 alla sequenza ascii della stringa
            string ret = "";

            var bytes = v.ToCharArray();
            foreach (var a in bytes)
            {
                var t = (int)a;
                char character = (char)(t + 5);
                ret += character.ToString();

            }

            return ret.Replace(";", "§");
        }





        public static void MsgBoxToUser()
        {
            if (Interactive)
                MsgBox("SYNC al momento non disponibile. Controlla i log per maggiori informazioni", "ATTENZIONE", "e");

        }

        /// <summary>
        /// MessageBox in stile VB
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="title"></param>
        /// <param name="icon">i,info,w,warning,e,error,!,exclamation,ex,question,?,q,asterisk,*,a,hand,h,stop,s,=,no,n,</param>
        /// <param name="Buttons">Ok,o,OkCancel,oc,YasNo,yn,YesNoCancel,ync,RetryCancel,rc,AbortRetryIgnore,ari</param>
        /// <returns></returns>
        public static DialogResult MsgBox(string msg, string title = "", string icon = "information", string Buttons = "o")
        {
            var ok = DialogResult.OK;

            MessageBoxButtons b = MessageBoxButtons.OK;
            MessageBoxIcon ic = MessageBoxIcon.Information;

            string bt = Buttons.ToLower();
            string i = icon.ToLower();

            if (bt == "okcancel" || bt == "oc") b = MessageBoxButtons.OKCancel;
            if (bt == "yesno" || bt == "yn") b = MessageBoxButtons.YesNo;
            if (bt == "yesnocancel" || bt == "ync") b = MessageBoxButtons.YesNoCancel;
            if (bt == "retrycancel" || bt == "rc") b = MessageBoxButtons.RetryCancel;
            if (bt == "abortretryignore" || bt == "ari") b = MessageBoxButtons.AbortRetryIgnore;

            if (i == "i") ic = MessageBoxIcon.Information;
            if (i == "warning" || i == "w") ic = MessageBoxIcon.Warning;
            if (i == "error" || i == "e") ic = MessageBoxIcon.Error;
            if (i == "exclamation" || i == "!" || i == "x" || i == "ex") ic = MessageBoxIcon.Exclamation;
            if (i == "question" || i == "?" || i == "q") ic = MessageBoxIcon.Question;
            if (i == "asterisk" || i == "*" || i == "a") ic = MessageBoxIcon.Asterisk;
            if (i == "hand" || i == "h") ic = MessageBoxIcon.Hand;
            if (i == "stop" || i == "s" || i == "=") ic = MessageBoxIcon.Stop;
            if (i == "no" || i == "" || i == "n") ic = MessageBoxIcon.None;

            mainForm.stopTimers();
            ok = MessageBox.Show(msg, title, b, ic);


            return ok;

        }



        public static void WriteErrFile(string msg)
        {
            AppendToFile(ErrFile, $"{DateTime.Now.ToString("MM-dd HH:mm:ss")}   |{msg}");
        }


        /// <summary>
        /// Compatta la memoria
        /// </summary>
        /// <returns></returns>
        public static long CompactMemory()
        {
            var prima = GC.GetTotalMemory(true);
           
            GC.Collect();

            var dopo = GC.GetTotalMemory(true);
            var m = prima - dopo;
            if (m < 0) m *= -1;
            return m;

        }

        // c#7 split programmatico su numero di elementi noti
        public static (string key, string value) SplitTouple(string line, char sep)
        {
            var vals = line.Split(sep);
            return (vals[0], vals[1]);


            /*  
            
            uso in parseConfig
            
              var ar = SplitTouple(s);
                    var c = ar.key;
                    var v = ar.value;
            
            =================================
            usa anche questa che è utilissima
            se un dato sappiamo che puo arrivare da fonti diverse
            e con tipo di dati diverso
            con questa tecnica si evitano un mare di if e conversioni
            con una sola linea di codice si effettua il test e conversione
        
            string ageconsole = "98";
            int agedb = 77;

            object ageval = ageconsole;// ageconsole;

            if (ageval is int age || (ageval is string agetext && int.TryParse(agetext, out age)))
            {
                Console.WriteLine($"your age is { age }.");
            }

            //con le date

            DateTime d1 = DateTime.Now;
            string d2 = "02/02/2021";

            object data = d2;

            var myData = (data is DateTime DATA || (data is string date3 && DateTime.TryParse(date3, out DATA));

            var f = DATA;

            */
        }

        /// <summary>
        /// parsing configurazione da file
        /// </summary>
        /// <returns></returns>
        public static bool loadConfig()
        {

            // inizializzazioni varie
            
            // lettura configurazione e settings
            // inizializzazioni varie

            string fn = cfgFile;
            string cfg = fn;
            

         

            if (!pathData.IsNull() && !Directory.Exists(pathData))
            {
                MsgBox($"108 - Errore: il percorso specificato come cartella di lavoro '{pathData}' non è un percoso valido.", "Errore grave", "e");
                return false;
            }



            if (!File.Exists(cfg))
                createInitialConfig();
          


            // imposta i defaults. se non trova le chiavi scritte sul file
            // tiene questi valori
            try
            {
                var lista = LoadTextFile(cfg, true);
                parseConfig(lista);
            


                return true;

            }
            catch (Exception ex)
            {
                // file di configurazione corrotto, ripristina e riavvia
                if (IsNull(LogFile))
                    MsgBox($"parseConfig lettura configurazione in errore: {ex.Message}", "Errore grave", "e");
                else
                    MsgBox(mainForm.log($"ERRORE GRAVE: parseConfig lettura configurazione in errore: {ex.Message}")); //, "Errore grave", "e");

                Application.Restart();

                return false;
            }


        }


        #region CAST

        public static int ToInt(string v)
        {
            if (v.IsNull()) return 0;
            return Convert.ToInt32(v);
        }



        #endregion


        #region HTTP




        public static async Task<bool> PingWithHttpClient(string hostUrl)
        {
            // string hostUrl = "https://www.code4it.dev/";
            try
            {
                var httpClient = new HttpClient();
                HttpRequestMessage request = new HttpRequestMessage
                {
                    RequestUri = new Uri(hostUrl),
                    Method = HttpMethod.Head
                };
                var result = await httpClient.SendAsync(request);
                return result.IsSuccessStatusCode;
            }
            catch (PingException ex)
            {
                // Discard PingExceptions and return false;
                UT.IOLog($"Errore in PingWithHttp Eccezione '{ex.Message}'");
                return false;

            }
        }


        public static bool PingHost(string nameOrAddress)
        {
            // ping.SendPingAsync(hostUrl);
            bool pingable = false;
            Ping pinger = null;

            try
            {
                pinger = new Ping();
                PingReply reply = pinger.Send(nameOrAddress);
                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException ex)
            {
                // Discard PingExceptions and return false;
                UT.IOLog($"Errore in PingHost Eccezione '{ex.Message}'");

                if (pinger != null) pinger.Dispose();
                return false;

            }
            finally
            {
                if (pinger != null)  pinger.Dispose();
            }

            return pingable;
        }


        public static async Task<bool> PingHostAsync(string nameOrAddress)
        {
            bool pingable = false;
            Ping pinger = null;

            try
            {
                pinger = new Ping();
                PingReply reply = await pinger.SendPingAsync(nameOrAddress);
                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException ex)
            {
                // Discard PingExceptions and return false;
                UT.IOLog($"Errore in PingHostAsync Eccezione '{ex.Message}'");

                if (pinger != null) pinger.Dispose();
                return false;

            }
            finally
            {
                if (pinger != null) pinger.Dispose();
            }

            return pingable;
        }





        public static async Task<httpResponse> APICall(HttpClient httpClient, string APIurl)
        {

            var hr = new httpResponse();
            string m = "";
            string resp = "";
            var content = new HttpResponseMessage();

            try
            {
                
                WriteToEventLog(mainForm, "httpGet API: " + APIurl);
                content = await httpClient.GetAsync(APIurl).ConfigureAwait(false);
                resp = await content.Content.ReadAsStringAsync().ConfigureAwait(false); ;
                if (content.StatusCode != isOk)
                {

                    if (IsNull(resp))
                    {
                        m = $"Wrong API response. Err Code: {content.StatusCode}, msg: esecuzione api fallita, API: {APIurl}";
                        hr = new httpResponse { status = "ERR", response = m, statusCode = content.StatusCode };
                        WriteToEventLog(mainForm, m);
                        return hr;
                    }


                    httpErr objErr = JsonConvert.DeserializeObject<httpErr>(resp);
                    m = $"Wrong API response. Err Code: {objErr.status} - {content.StatusCode}, msg: {objErr.title}, API: {APIurl}";
                    hr = new httpResponse { status = "ERR", response = m, statusCode = content.StatusCode };
                    WriteToEventLog(mainForm, m);
                    return hr;

                }
            }
            catch (Exception ex)
            {
                m = $"Wrong API response. Err Code: {content.StatusCode}, msg: {resp} , Exception: {ex.Message}, API: {APIurl}";
                hr = new httpResponse { status = "ERR", response = m, statusCode = content.StatusCode };
                mainForm.log(m);
                return hr;
            }

            IOLog("httpGet API: OK");
            hr = new httpResponse { status = "OK", response = resp, statusCode = content.StatusCode, content = content };
            return hr;

        }

        public static async Task<httpResponse> APICall(HttpClient httpClient, string APIurl, HttpContent requestContent)
        {
            var hr = new httpResponse();
            string m = "";
            string resp = "";
            var responseContent = new HttpResponseMessage();

            WriteToEventLog(mainForm, "httpPOST API: " + APIurl);

            try
            {

               // IOLog("httpPost API: " + APIurl);
                responseContent = await httpClient.PostAsync(APIurl, requestContent);
                resp = await responseContent.Content.ReadAsStringAsync();
                if (responseContent.StatusCode != isOk)
                {
                    if (IsNull(resp))
                    {
                        m = $"Wrong API response. Err Code: {responseContent.StatusCode}, msg: esecuzione api fallita, API: {APIurl}";
                        hr = new httpResponse { status = "ERR", response = m, statusCode = responseContent.StatusCode };
                        mainForm.log(m);
                        return hr;
                    }


                    httpErr objErr = JsonConvert.DeserializeObject<httpErr>(resp);
                    m = $"Wrong API response. Err Code: {objErr.status} - {responseContent.StatusCode}, msg: {objErr.title}, API: {APIurl}";
                    hr = new httpResponse { status = "ERR", response = m, statusCode = responseContent.StatusCode };
                    mainForm.log(m);
                    return hr;

                }

                //IOLog("httpPost API: OK");
                hr = new httpResponse { status = "OK", response = resp, statusCode = responseContent.StatusCode, content = responseContent };
                return hr;

            }
            catch (Exception ex)
            {
                m = $"Wrong API response. Err Code: {responseContent.StatusCode}, msg: {resp} , Exception: {ex.Message}, API: {APIurl}";
                hr = new httpResponse { status = "ERR", response = m, statusCode = responseContent.StatusCode };
                mainForm.log(m);
                return hr;
            }

        }

        public static async Task<HttpResponseMessage> httppostcall(string EndPoint, string dataparameter, StringContent dataupdate)
        {

            using (var httpClient = new HttpClient())
            {
                //  httpClient.Timeout = Timeout.InfiniteTimeSpan;
                httpClient.BaseAddress = new Uri(EndPoint);
                var result = await httpClient.PostAsync(dataparameter, dataupdate).ConfigureAwait(false);
                //    result.Wait();
                return result;
            }
            //.Result;
        }

        #endregion


        public static void Sleep(int ms)
        {
            var d = DateTime.Now.AddMilliseconds(ms);
            do
            {
                System.Threading.Thread.Sleep(200);
                Application.DoEvents();
            } while (DateTime.Now < d);
        }

        public static async Task<httpResponse> iProdLogin(string url, string usr, string pwd, bool silent = false)
        {


            string sm = "";
            var hr = new httpResponse();
            if (connecting) return hr;  // qui non deve arrivarci mai, se si, c'è un problema di logica di programmazione

           

            if (Program.UrlGate.Key == "prod") url = "https://app.iprod.it/api/";

            connecting = true;
            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(url);
                //httpClient.Timeout=new TimeSpan(0, 0, 30);

                iProdConnected = false;

                var credentials = new UserCredentials();
                credentials.Username = usr;
                credentials.Password = pwd;
                credentials.SourceName = "iProdSync";   // <-- non cambiare, su iProd non c'è iProdWHSE

                var json = JsonConvert.SerializeObject(credentials);
                StringContent requestContent = new StringContent(json, Encoding.UTF8, "application/json");
                string APIurl = "iProdAuthentication/GetTokenV2";

                var resp = await APICall(httpClient, APIurl, requestContent);
                if (resp.status != "OK")
                {
                    if (!silent)
                    {
                        sm = mainForm.log("Login iProd fallito!");
                        mainForm.SetNetStatus("IP-ERR", sm);
                    }
                    connecting = false;
                    return resp;
                }

                Program.ipTOKEN = resp.response;

                // AUTENTICAZIONE OK
         

                APIurl = "Account/GetIprodCustomer?token=" + Program.ipTOKEN;

                resp = await APICall(httpClient, APIurl);
                if (resp.status != "OK")
                {
                    if (!silent)
                    {
                        sm = mainForm.log("Login iProd fallito!");
                        mainForm.SetNetStatus("IP-ERR", sm);
                    }
                    connecting = false;
                    return resp;
                }

                Tenant = JsonConvert.DeserializeObject<iProdCustomers>(resp.response);

                iProdConnected = true;

                mainForm.SetNetStatus("IP-ONLINE", "iProd CONNESSO");
                connecting = false;
                return resp;

            }
            catch (Exception ex)
            {

                connecting = false;
                if (!silent)
                {
                    sm = mainForm.log("Login iProd fallito!");
                    mainForm.SetNetStatus("IP-ERR", sm);
                }
                iProdConnected = false;
                string m = "Rilevata Eccezione: " + ex.Message + "|" + ex.StackTrace;
                mainForm.log(m);
                if (ex.Message == "Si è verificato un errore durante l'invio della richiesta.") // offline
                    hr = new httpResponse { status = "ERR", response = ex.Message, statusCode = HttpStatusCode.BadGateway };
                else
                    hr = new httpResponse { status = "ERR", response = "Utente o Password non riconosciuti", statusCode = HttpStatusCode.Unauthorized };
                return hr;
            }

        }




        public async static Task<MemoryStream> downloadfile(string uri)
        {
            try
            {
                var uriSplitted = uri.Split('/');
                string iprodCustomerId = uriSplitted[3];
                string blobName = HttpUtility.UrlDecode(uriSplitted[uriSplitted.Length - 3] + "/" + uriSplitted[uriSplitted.Length - 2] + "/" + uriSplitted[uriSplitted.Length - 1]);

                var serviceClient = new BlobServiceClient(Program.UrlGate.storageConn);
                var container = serviceClient.GetBlobContainerClient(iprodCustomerId);

                // mettendo a false il parametro trimBlobNameSlashes impediamo che nell'url dei loghi creati con le vecchie api di Azure, in cui il type è vuoto, la doppia slash sia semplificata in una sola
                BlobUriBuilder blobUriBuilder = new BlobUriBuilder(container.GenerateSasUri(BlobContainerSasPermissions.Read, DateTimeOffset.UtcNow.AddMinutes(15)), false)
                {
                    BlobName = blobName
                };
                BlobClient client = new BlobClient(blobUriBuilder.ToUri());

                MemoryStream downloaded = new MemoryStream();
                _ = await client.DownloadToAsync(downloaded);

                return downloaded;
            }
            catch
            {
                return null;
            }
        }


        public static async Task<MemoryStream> GetAvatar()
        {
            try
            {
                var usr = iprod_loggeduser;
                string iniziali = usr.Name.Substring(0, 1) + usr.Surname.Substring(0, 1);

                if (!IsNull(usr.Imgurl))
                {

                    var b = await downloadfile(usr.Imgurl);
                    return b;
                }
                else
                {
                    var bitmap = new Bitmap(150, 150);
                    Graphics g = Graphics.FromImage(bitmap);
                    g.Clear(Color.Transparent);
                    Brush b = new SolidBrush(ColorTranslator.FromHtml("#eeeeee"));
                    g.FillEllipse(b, 0, 0, 149, 149);

                    float emSize = 62;


                    g.DrawString(iniziali,
                        new Font(FontFamily.GenericSansSerif, emSize, FontStyle.Regular),
                        new SolidBrush(Color.Black), 7, 25);

                    var memStream = new MemoryStream();
                    bitmap.Save(memStream, System.Drawing.Imaging.ImageFormat.Png);
                    return memStream;
                }

            }
            catch (Exception)
            {
                return new MemoryStream();
            }
        }


        public static bool DeleteSpyFile()
        {

            FileDelete(FileSpy);
            return true;
        }

        /// <summary>
        /// Crea file semaforo. Usato da SyncIsBusy e AcceptedTerms
        /// </summary>
        /// <param name="f"></param>
        /// <param name="content"></param>
        /// <param name="replace"></param>
        /// <returns></returns>
        public static bool CreateSpyFile(string f, string content, bool replace = true)
        {

            try
            {
                if (FileExists(f))
                {
                    if (!replace) return true;
                    FileDelete(f);
                }

                if (content == "*binary*")
                {

                    // crea un file che assomiglia a un binary, con contenuto random 
                    // questo file fa da spia ad altri metodi

                    string fakeBin = "";
                    var rnd = new Random();
                    char[] chars = new char[564];

                    for (int i = 564 - 1; i >= 0; i--)
                    {
                        chars[i] = (char)(rnd.Next(1000));
                        fakeBin += chars[i];
                    }

                    if (!AppendToFile(f, fakeBin)) return false;
                }
                else
                {
                    if (!AppendToFile(f, content)) return false;
                }

            }
            catch (Exception ex)
            {
                mainForm.log($"Errore in CreateSpyFile: {ex.Message}");
                return false;
            }

            return true;
        }



        public static void AcceptedTerms()
        {
            // crea un file pseudo dll, con contenuto random 
            // questo file fa da spia che le condizioni sono state accettate dall'utente 
            // prima di proseguire con aggiornamenti verso il proprio database

            ManageDataEventAsync("iProdSync.AcceptedTerms", "INF", "L'utente ha accettato le condizioni contrattuali", new Dictionary<string, string>()).ConfigureAwait(true);

            CreateSpyFile("libzstdip.dll", "*binary*");

        }


#region EVENTS LOG

        /// <summary>
        /// Invia un evento strutturato contenente informazioni sulle transazioni
        /// </summary>
        public static async Task ManageDataEventAsync(string title, string action, string data, Dictionary<string, string> diz = null)
        {

            if (Simulating ) return;

            try
            {
                /*
                  schemaName: Customers, Items, ecc
                  action:     READ, INS, UPD, DEL, UNDO
                */

                //       iprod_loggeduser.Username} {UT.LF}{iprod_loggeduser.Name} {iprod_loggeduser.Surname} {UT.LF}{iprod_user.Customerdata.Name}");

                string authorName = $"{iprod_loggeduser.Name} {iprod_loggeduser.Surname}";
                string authorid = iprod_loggeduser._id;

                if (diz is null) diz = new Dictionary<string, string>();

                EventLog eventlog = new EventLog();
                eventlog._id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
                //    eventlog._id = MongoDB.Bson.ObjectId.GenerateNewId();

                int type = 10;

                if (action == "INS") type = 80;
                if (action == "UPD") type = 81;
                if (action == "DEL") type = 82;
                if (action == "UNDO") type = 83;
                if (action == "READ") type = 84;
                if (action == "LNK") type = 85;
                if (action == "UNLNK") type = 86;
                if (action == "ERR") type = 50;
                if (action == "INF") type = 40;


                eventlog.Type = type;  //  int
                eventlog.Title = title;
                eventlog.Textvalue = data;
                eventlog.Tag = "sync-event";
                eventlog.Lastupdate = DateTime.UtcNow;
                //eventlog.Iprodcustomerid = MongoDB.Bson.ObjectId.Parse(iprod_user._id);
                eventlog.Iprodcustomerid = iprod_user._id;
                eventlog.deleted = false;
                eventlog.Creationdate = DateTime.UtcNow;
                //eventlog.AuthorId = MongoDB.Bson.ObjectId.Parse(authorid);
                eventlog.AuthorId = authorid;
                eventlog.AuthorName = authorName;
                eventlog.Additionaldata = new Dictionary<string, string>();

                eventlog.Additionaldata = diz; // assegna le key arrivate dall'esterno

                // x visualizzazione su grid

                eventlog.Additionaldata["Action"] = action;


                await SendEvent(eventlog);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// Aggiunge un evento su customerslog di iprod
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        private static async Task<bool> SendEvent(EventLog evt)
        {

            var gate = Program.UrlGate;
            var resp = new httpResponse();

            try
            {
                var hc = GetHttpClient(gate.home);

                var tokens = Program.UrlGate.SvcGetTokens();
                var body = (tokens.Item1, tokens.Item2, evt);

                var json = JsonConvert.SerializeObject(body);
                StringContent requestContent = new StringContent(json, Encoding.UTF8, "application/json");

                resp = await SvcCall(hc, gate.svcSendEventLog, requestContent);


                if (resp.status != "OK")
                {
                    mainForm.log($"323. SendEvent(evt). Errore di invio richiesta POST al servizio 'Eventi' di iProd. Evento: '{evt.Textvalue}'. Response: '{resp.response}'");
                    MsgBoxToUser();

                    return false;
                }


            }
            catch (Exception ex)
            {

                string m = $"324. SendEvent(evt). Rilevata eccezione durante l'invio di una richiesta in POST al servizio 'Eventi' di iProd. Eccezione: '{ex.Message}', StackTrace: '{ex.StackTrace}', Stato richiesta: '{resp.status}', Evento: '{evt.Textvalue}', Response: '{resp.response}'";
                mainForm.log(m);
                MsgBoxToUser();

                return false;
            }

            return true;

        }

#endregion



#region BACKUPS

        internal static HttpClient GetHttpClient(string url)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(url);
            return httpClient;
        }

        public static async Task<bool> SvcUtcValid()
        {
            try
            {
                var gate = Program.UrlGate;
                var resp = new httpResponse();
                DateTime utcSvc = DateTime.UtcNow;

                var hc = GetHttpClient(gate.home); // get httpClient


                resp = await SvcCall(hc, gate.svcGetSystemTimestamp); // chiamo l'api al servizio che mi restituisce la data di sistema di iProd Web
                if (resp.status == "OK")
                {
                    // Remove leading and trailing quotes (").
                    if (!DateTime.TryParse(resp.response.Substring(1, resp.response.Length - 2), out utcSvc))
                    {
                        resp.status = "ERR";
                        resp.response = $"SvcUtcValid() - Errore di invio richiesta GetSystemTimestamp a iProd: La chiamata è andata a buon fine ma la data restituita non è valida: '{resp.response}'";
                    }
                    else
                    {
                        // verifico la differenza con quella locale

                        var fd = new TimeSpan();  // fault diff
                        if (utcSvc > DateTime.UtcNow)
                            fd = utcSvc.Subtract(DateTime.UtcNow);
                        else
                            fd = DateTime.UtcNow.Subtract(utcSvc);

                        if (!gate.isValidUtcTolerance(fd))
                        {
                            resp.status = "ERR";
                            resp.response = $"SvcUtcValid() - Errore di tolleranza tra data utc di iprod con quella locale: La tolleranza ammessa è di {string.Format("{0:%h} ore {0:%m} minuti, {0:%s} secondi", gate.svcTimeFaultTolerance)}, mentre la differenza rilevata è di {string.Format("{0:%h} ore {0:%m} minuti, {0:%s} secondi", fd)}";
                        }
                    }
                }

                if (resp.status != "OK")
                {
                    mainForm.log(resp.response + ". Contatta l'amministratore di sistema per allineare le date");
                    MsgBoxToUser();
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                string m = "Un eccezione ha interrotto la funzione SvcUtcValid() di controllo tolleranza tra data del Server iProd e quella locale: Il messaggio è " + ex.Message;
                mainForm.log(m);
                MsgBoxToUser();

                return false;
            }
        }

        public static async Task<bool> SvcIsServerBusy()
        {

            var gate = Program.UrlGate;
            var resp = new httpResponse();

            mainForm.log("SvcIsServerBusy: Verifica in corso se ci sono operazioni di backup o sync attivi sul server iProd.. attendere");

            try
            {
                if (!await SvcUtcValid()) return true;

                var hc = GetHttpClient(gate.home);
                resp = await SvcCall(hc, gate.svcBusy);

                if (resp.status != "OK")
                {

                    mainForm.log(resp.response);
                    MsgBoxToUser();

                    return true;
                }

            }
            catch (Exception ex)
            {

                string m = $" SvcIsServerBusy: Rilevata Eccezione " + ex.Message + "|" + ex.StackTrace;
                if (resp.status == "OK")
                    resp.response = m;
                else
                    resp.response += m;

                mainForm.log(resp.response);
                MsgBoxToUser();

                return true;
            }


            return false;

        }


        public static void AddRowHist(string tipo, string dex)
        {
            HistCount++;
            string s = $"{HistCount};{tipo};{DateTime.Now};{dex}";
            AppendToFile(HistFile,s);
        }



        /// <summary>
        /// Invia a iprod il comando di esecuzione backup 
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> SvcDoBackup()
        {
            /*

            Sequenza:
            [A] SvcIsServerBusy() --> si goto [B]
                no
                SvcNeedBackup() -> no goto [C]
                si
                SvcDoBackup()
                andato a buon fine -> no goto [ERR]
                si
            [C] SvcSetOperation("syncro")   <-------------------- avvio --> return false (non eseguita) goto [ERR]
                    avvio il sync .. sync in corso..
                SvcSetOperation("")         <-------------------- completato
                goto [END]
            ------------------------------------


            [B] Interactive? -> si MsgBox("Server occupato, ritenta tra 10 minuti") -> [END]
                no
                 Sleep 10 minuti 
                goto [A]

          [ERR] Interactive? -> si MsgBox("Errore") -> [END]
                no
                 Log "Errore"
                goto [IDLE]
        [IDLE]  attesa next sync da timer

            [END] fine sync

            */

            var gate = Program.UrlGate;
            var resp = new httpResponse();
            var dstart = DateTime.Now;
            string m = "";

            mainForm.log("Esecuzione backup su iProd.. attendere");

            string backupName = $"backup.fromsync.{DateTime.UtcNow:yyyyMMddhhmmss}.bak";
            try
            {
                await ManageDataEventAsync("iProdSync.SvcDoBackup", "INF", "SYNC ha richiesto l'avvio del backup", new Dictionary<string, string>());

                var hc = GetHttpClient(gate.home);
                resp = await SvcCall(hc, gate.svcDoBackup + backupName);

                if (resp.status != "OK")
                {
                    mainForm.log($"321. SvcDoBackup(). Errore di invio richiesta al servizio di backup di iProd. Response: '{resp.response}'");
                    MsgBoxToUser();

                    return false;
                }

                string runningTime = ElapsedTimeToString(DateTime.Now.Subtract(dstart));
                m = $"Backup {backupName} completato in {runningTime}";
                mainForm.log(m);
                await ManageDataEventAsync("iProdSync.SvcDoBackup", "INF", m, new Dictionary<string, string>());

            }
            catch (Exception ex)
            {
                m = $"322. SvcDoBackup(). Rilevata eccezione durante l'invio di una richiesta al servizio di backup di iProd. Eccezione: '{ex.Message}', StackTrace: '{ex.StackTrace}', Stato richiesta: '{resp.status}', Response: '{resp.response}'";
                mainForm.log(m);
                MsgBoxToUser();

                await ManageDataEventAsync("iProdSync.SvcDoBackup", "ERR", m, new Dictionary<string, string>());
                return false;
            }


            return true;
        }

        public static async Task<bool> SvcNeedBackup()
        {
            var gate = Program.UrlGate;
            var resp = new httpResponse();

            mainForm.log("Verifico se l'ultimo backup del tenant è scaduto e va rifatto prima di avviare il sync.. attendere");

            try
            {
                //  if (!await SvcUtcValid()) return true;  // <--- questa l'ho gia fatta prima nella richiesta SvcIsServerBusy

                var hc = GetHttpClient(gate.home);
                resp = await SvcCall(hc, gate.svcGetBackups);

                if (resp.status != "OK") throw new Exception(resp.response);

                List<Posts> backups = JsonConvert.DeserializeObject<List<Posts>>(resp.response);
                if (backups is null) return true;
                if (backups.Count == 0) return true;

                var last = backups.OrderBy(x => x.Creationdate).ToList().Last();  // prende il post con la data piu alta.

                mainForm.log($"Ultimo backup eseguito il {last.Creationdate}: {last.Textvalue}");

                var fd = new TimeSpan();  // fault diff
                fd = DateTime.UtcNow.Subtract(last.Creationdate);

                if (!gate.isValidBackupTime(fd))
                {
                    mainForm.log($"Il backup va rifatto. L'ultimo era stato fatto il {last.Creationdate} e da adesso sono passate {string.Format("{0:%h} ore {0:%m} minuti, {0:%s} secondi", fd)}, mentre la validità massima di un backup è di {string.Format("{0:%h} ore {0:%m} minuti, {0:%s} secondi", gate.svcBackupValidityTime)}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }


            return false;
        }

        public static async Task<bool> SvcSetOperation(string operation)
        {
            var gate = Program.UrlGate;
            var resp = new httpResponse();

            if (operation.IsNull())
                mainForm.log("Comunico a iProd che la sincronizzazione è terminata e possono riprendere eventuali backup");
            else
                mainForm.log("Comunico a iProd che sto sincronizzando e non possono essere eseguiti backup/restore o altri sync");

            try
            {
                //    if (!await SvcUtcValid()) return true;  // <--- questa l'ho gia fatta prima nella richiesta SvcIsServerBusy

                var hc = GetHttpClient(gate.home);
                resp = await SvcCall(hc, gate.svcSetSyncOp + operation);

                if (resp.status != "OK")
                {
                    mainForm.log("321. Errore di invio richiesta a servizio iProd " + resp.response);
                    MsgBoxToUser();
                    return false;
                }

            }
            catch (Exception ex)
            {

                string m = $"320. SvcSetOperation: Rilevata Eccezione " + ex.Message + "|" + ex.StackTrace;
                if (resp.status == "OK")
                    resp.response = m;
                else
                    resp.response += m;

                mainForm.log(resp.response);
                MsgBoxToUser();

                return false;
            }


            return true;
        }



        /// <summary>
        /// Funzione condivisa da tutte le funzioni sul servizio http dedicato ai backups di iProd. call in POST
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<httpResponse> SvcCall(HttpClient httpClient, string url)
        {

            var hr = new httpResponse();

            try
            {
                if (IsNull(url))
                {
                    hr.status = "ERR";
                    hr.response = "354. SvcCall in errore: parametro url nullo";
                    return hr;
                }



                var tokens = Program.UrlGate.SvcGetTokens();

                if (IsNull(tokens.Item1) || IsNull(tokens.Item2))
                {
                    hr.status = "ERR";
                    hr.response = "356. SvcCall in errore: Token di servizio o Token utente nullo";
                    return hr;
                }

                var json = JsonConvert.SerializeObject(tokens);
                StringContent requestContent = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage responseContent = await httpClient.PostAsync(url, requestContent);
                string resp = await responseContent.Content.ReadAsStringAsync();

                string m = "";
                if (responseContent.StatusCode != isOk)
                {
                    try
                    {
                        if (IsNull(resp))
                        {
                            m = $"357. Wrong API response. Err Code: {responseContent.StatusCode}, msg: esecuzione api fallita, response nullo - API: {url}";
                            hr = new httpResponse { status = "ERR", response = m, statusCode = responseContent.StatusCode };
                            mainForm.log(m);
                            return hr;
                        }

                        httpErr objErr = JsonConvert.DeserializeObject<httpErr>(resp);
                        m = $"358. Codice di errore: {objErr.status} - {responseContent.StatusCode}, msg: {objErr.title}, url: {url}";
                        hr = new httpResponse { status = "ERR", response = m, statusCode = responseContent.StatusCode };
                        mainForm.log(m);
                        return hr;
                    }
                    catch (Exception ex)
                    {
                        m = $"354. Codice di errore: {responseContent.StatusCode}, msg: {resp} , Exception: {ex.Message}, url: {url}";
                        hr = new httpResponse { status = "ERR", response = m, statusCode = responseContent.StatusCode };
                        mainForm.log(m);
                        return hr;
                    }
                }


                hr = new httpResponse { status = "OK", response = resp, statusCode = responseContent.StatusCode, content = responseContent };
                return hr;
            }
            catch (Exception ex)
            {

                hr.status = "ERR";
                hr.response = "359. SvcCall in errore:una Eccezione ha interrotto l'esecuzione della funzione. Messaggio: " + ex.Message;
                return hr;
            }

        }




        /// <summary>
        /// Funzione condivisa da tutte le funzioni sul servizio http dedicato ai backups di iProd. call in POST
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<httpResponse> SvcCall(HttpClient httpClient, string url, HttpContent requestContent)
        {

            var hr = new httpResponse();

            try
            {
                if (IsNull(url))
                {
                    hr.status = "ERR";
                    hr.response = "354. SvcCall in errore: parametro url o body nullo";
                    return hr;
                }

                //    mainForm.Log(".. esecuzione httpPost SvcCall(). url: " + url);


                HttpResponseMessage responseContent = await httpClient.PostAsync(url, requestContent);
                string resp = await responseContent.Content.ReadAsStringAsync();

                string m = "";
                if (responseContent.StatusCode != isOk)
                {
                    try
                    {
                        if (IsNull(resp))
                        {
                            m = $"357. Wrong API response. Err Code: {responseContent.StatusCode}, msg: esecuzione api fallita, API: {url}";
                            hr = new httpResponse { status = "ERR", response = m, statusCode = responseContent.StatusCode };
                            mainForm.log(m);
                            return hr;
                        }

                        httpErr objErr = JsonConvert.DeserializeObject<httpErr>(resp);
                        m = $"358. Codice di errore: {objErr.status} - {responseContent.StatusCode}, msg: {objErr.title}, url: {url}";
                        hr = new httpResponse { status = "ERR", response = m, statusCode = responseContent.StatusCode };
                        mainForm.log(m);
                        return hr;
                    }
                    catch (Exception ex)
                    {
                        m = $"354. Codice di errore: {responseContent.StatusCode}, msg: {resp} , Exception: {ex.Message}, url: {url}";
                        hr = new httpResponse { status = "ERR", response = m, statusCode = responseContent.StatusCode };
                        mainForm.log(m);
                        return hr;
                    }
                }

                //   mainForm.Log("httpPost SvcCall result: OK");
                hr = new httpResponse { status = "OK", response = resp, statusCode = responseContent.StatusCode, content = responseContent };
                return hr;
            }
            catch (Exception ex)
            {

                hr.status = "ERR";
                hr.response = "359. SvcCall in errore:una Eccezione ha interrotto l'esecuzione della funzione. Messaggio: " + ex.Message;
                return hr;
            }

        }


#endregion


#region File Handlers

        public static bool FileDelete(string fn)
        {
            try
            {
                if (!FileExists(fn)) return false;
                File.Delete(fn);
            }
            catch (Exception ex)
            {
                mainForm.log($"Errore in FileDelete: {ex.Message}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// elenco files recursive con restituzione FileSystemInfo invece di item string
        /// </summary>
        /// <param name="sDir"></param>
        /// <returns></returns>
        public static List<FileSystemInfo> DirSearchExt(string sDir)
        {

            List<FileSystemInfo> ret = new List<FileSystemInfo>();



            try
            {
                FileSystemInfo dExt = new DirectoryInfo(sDir);

                if (!Directory.Exists(sDir)) throw new Exception("Directory " + sDir + " not found.");

                foreach (string f in Directory.GetFiles(sDir))
                {
                    FileSystemInfo FileExt = new FileInfo(f);

                    ret.Add(FileExt);
                }

                foreach (string d in Directory.GetDirectories(sDir))
                    ret.AddRange(DirSearchExt(d));

                return ret;
            }
            catch (System.Exception excpt)
            {
                throw excpt;
            }

        }

        /// <summary>
        /// Sposta File. Non occorre cancellare destFile prima, il replace è  automatico
        /// </summary>
        /// <param name="srcFile"></param>
        /// <param name="destFile"></param>
        /// <returns></returns>
        public static bool FileMove(string srcFile, string destFile)
        {
            try
            {
                if (!File.Exists(srcFile)) return false;

                File.Copy(srcFile, destFile, true);
                FileDelete(srcFile);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string BackupFile(string f1, bool RetainSource = false, string DestPath = "")
        {

            if (f1.IsNull()) return "";

            if (DestPath.IsNull()) DestPath = pathBackups;                      //  pathBackups = "C:\iProdWHSE\Data\BkLog\";

            if (!DestPath.EndsWith("\\")) DestPath += "\\";

            var fileonly = f1.Split('\\').Last();                               //  LogFile.txt
            var pathOnly = f1.Substring(0, f1.Length - fileonly.Length);         //  C:\iProdWHSE\
            var ext = fileonly.Split('.').Last();
            string est = "." + ext;                                             //  txt
            string fil = fileonly.Replace(est, "");                             //  LogFile             

            var sdata = DateTime.Now.ToString("yyyyMMdd");                      //  20230501  ->  01/05/2023
            var sh = DateTime.Now.ToString("HH");                               //  19
            var sm = DateTime.Now.ToString("mm");                               //  23
            var ss = DateTime.Now.ToString("ss");                               //  18
            var ms = DateTime.Now.ToString("ffff");                             //  1894

            var f2 = fil;
            var sd = $"-{sdata}-h{sh}m{sm}s{ss}ms{ms}.{ext}";                   //  -20230501-h19m23s18ms1894.txt
            f2 += sd;                                                           //  LogFile-20230501-h19m23s18ms1894.txt
            f2 = DestPath + f2;                                                 //  C:\iProdWHSE\Data\BkLog\LogFile-20230501-h19m23s18ms1894.txt

            var ok = BackupFile(f1, f2, RetainSource);

            if (!ok) return "*ERR*";
            return f2;


        }


        public static string cutString(string v, int xLeft, int xRight)
        {

            string ret = v;

            int mx = xLeft + xRight + 3;
            int xLen = v.Length;
            if (xLen > mx)
            {
                string Sx = v.Substring(0, xLeft);
                string Dx = v.Substring(xLen - xRight, xRight);
                ret = Sx + "..." + Dx;

            }

            return ret;
        }

        public static bool BackupFile(string f1, string f2, bool RetainSource = false)
        {
            try
            {
                if (RetainSource)
                    FileCopy(f1, f2, true);
                else
                    FileMove(f1, f2);

                // File.Copy(f1, f2, true);
                return true;
            }
            catch { return false; }

        }

        public static bool FileCopy(string srcFile, string destFile, bool Replace = false)
        {
            try
            {
                File.Copy(srcFile, destFile, Replace);
                return true;
            }
            catch { return false; }
        }

        public static bool FileExists(string file)
        {
            return File.Exists(file);
        }

#endregion


        /// <summary>
        /// se nella Bin c'è il file libzstdip.dll ha gia accettato le condizioni, non le deve chiedere piu
        /// </summary>
        /// <returns></returns>
        public static bool WasAcceptedTerms()
        {

            string f = "libzstdip.dll";
            if (File.Exists(f)) TermsAccepted = true;
            return TermsAccepted;

        }

        public static string Log(string m)
        {
            if (mainForm != null)
                mainForm.log(m);

            return m;
        }

      




#endregion


#region da iProd


        private static CultureInfo _culture;
        public static CultureInfo culture
        {
            get
            {
                if (_culture != null) return _culture;
                _culture = new CultureInfo(CultureInfo.CurrentCulture.LCID);
                return _culture;
            }
        }

        static int cdmp = 0;
        public static void dumpDataObj<T>(List<T> obj, string fn, bool asCsv = true)
        {
            if (obj.Count < 1) return;
            cdmp++;
            var t = obj.GetType();
            AppendToFile(fn, " ");
            AppendToFile(fn, $"{cdmp}) Nome Oggetto:  {t.Name}  ==========================================");
            var ls = ipGetPropertyNames(obj[0]);
            int i = 0;
            string head = "";
            if (asCsv)
            {
                foreach (var e in ls)
                {
                    i++;
                    if (i < ls.Count)
                        head += e.Name + ";";
                    else
                        head += e.Name;

                }
                AppendToFile(fn, head);


                foreach (var r in obj)
                {
                    var lsr = ipGetPropertyNames(r);
                    i = 0;
                    string ret = "";
                    foreach (var e in lsr)
                    {
                        i++;
                        if (i < lsr.Count)
                            ret += e.Value + ";";
                        else
                            ret += e.Value;

                    }
                    AppendToFile(fn, ret);
                }
            }

        }

        /// <summary>
        /// Dato un oggetto generico restituisce tutto le sue proprietà, tipo e valore. Usage: var props = ipGetPropertyNames(myobj);
        /// </summary>
        /// <typeparam name="T">Oggetto</typeparam>
        /// <param name="obj">Oggetto da cui prelevare i nomi delle proprieta</param>
        /// <returns>Lista di Fielescriptor (con nome, tipo, val) /returns>
        public static List<FieldDescriptor> ipGetPropertyNames<T>(T obj)
        {
            var lst = new List<FieldDescriptor>();
            try
            {

                var pi_List = obj.GetType().GetProperties();

                var objtype = obj.GetType();

                foreach (System.Reflection.PropertyInfo pi in pi_List)
                {
                    var v = pi.GetValue(obj);

                    var fl = new FieldDescriptor(
                        pi.Name,
                        pi.PropertyType.Name,
                        v
                        );
                    fl.ObjName = objtype.Name;
                    lst.Add(fl);

                }
            }
            catch (Exception)
            {

            }
            return lst;
        }

#endregion

#region Fabio's own function libraries

        public static string newObjectId()
        {
            return ObjectId.GenerateNewId().ToString();
        }

        public static int RandomInt(int from = int.MinValue, int soglia = int.MaxValue)
        {
            var random = new Random();

            // uncheck disabilita il controllo overflow sulle operazioni numeriche
            unchecked
            {
                var n = from + random.Next(soglia);
                // Console.WriteLine($"RandomInt: {n}");
                return n;
            }

        }


        /// <summary>
        /// Scorre una string a partire da from in direzione direction fino a quando non trova almeno una delle sequenze in strs
        /// </summary>
        /// <param name="st"></param>
        /// <param name="from"></param>
        /// <param name="strs"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static int ScrollTo(string st, int from, List<string> strs, string direction = ">")
        {
            int ln = st.Length;
            if (direction == "<")
            {
                for (int i = from - 1; i >= 0; i--)
                {

                    foreach (var s in strs)
                    {
                        int lc = s.Length;
                        if ((i - lc) <= ln)
                        {
                            if (st.Substring(i, lc) == s)
                                return i;
                        }

                    }

                }
            }
            else
            {
                for (int i = from; i < ln; i++)
                {
                    foreach (var s in strs)
                    {
                        int lc = s.Length;
                        if ((i + lc) <= ln)
                        {
                            if (st.Substring(i, lc) == s)
                                return i;
                        }

                    }
                }
            }
            return -1;
        }


        /// <summary>
        /// Ottiene una stringa ripetuta per il n° di volte dei caratteri passati come parametro
        /// </summary>
        /// <param name="chr">caratteri da replicare</param>
        /// <param name="times">num di volte</param>
        /// <returns></returns>
        public static string Repeat(string chr, int times)
        {
            string t = "";
            for (int i = 0; i < times; i++)
            {
                t += chr;
            }
            return t;
        }






        /// <summary>
        ///  Scorre una string a partire da from in direzione direction fino a quando non trova la sequenza str
        /// </summary>
        /// <param name="st"></param>
        /// <param name="from"></param>
        /// <param name="car"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static int ScrollTo(string st, int from, string str, string direction = ">")
        {
            int lc = str.Length;
            if (direction == "<")
            {
                for (int i = from - (lc + 1); i >= 0; i--)
                {
                    if (st.Substring(i, lc) == str)
                        return i;
                }
            }
            else
            {
                for (int i = from; i < st.Length - lc; i++)
                {
                    if (st.Substring(i, lc) == str)
                        return i;
                }
            }
            return -1;
        }




        public static void storeIamRunning(bool silent = true, bool forza = false)
        {
            // me= AGENT
            // lui= CONTROLLER

            if (!SendEmail && !forza) return;

            string agent = "agent;" + DateTime.Now;
            string controller = "controller;";
            var items = new List<string>();
            int cnt = 0;
         
            bool isC = false;


            try
            {
                if (File.Exists(SharedFile))
                    items = LoadTextFile(SharedFile);

                if (items.Count == 0)
                {
                    // file nuovo, salvo la data del controller ed esco
                    AppendToFile(SharedFile, controller, true, true);
                    return;
                }


                foreach (var e in items)
                {
                    if (NotNull(e))
                    {
                        cnt++;
                        var ar = e.Split(';');

                        // ci assicuriamo che il controller sia su, perche i due si controllano a vicenda
                        if (ar[0] == "controller")
                        {
                            isC = true;
                            // ci rimette quello che c'era perche non è roba nostra
                            controller += ar[1];

                        }
                      

                    }
                }



                // aggiorna il file condiviso con la nuova data per noi e abbiamo verificato il controller
                if (isC)
                {
                    AppendToFile(SharedFile, agent, true, true); // qui cancella e reinizializza il file
                    AppendToFile(SharedFile, controller);        // normale append
                }
                else
                {
                    AppendToFile(SharedFile, agent, true, true);        // il controller non c'ha mai scritto
                }

                WriteToEventLog(mainForm, $"MONITOR  XiAgent.exe scrive: Sono in esecuzione");


            }
            catch (Exception ex)
            {
                WriteToEventLog(mainForm, "Errore in storeIamRunnin: " + ex.Message);
            }


        }


        public static string RandomString(int length, string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789")
        {
            if (length < 0) throw new ArgumentOutOfRangeException("length", @"length cannot be less than zero.");
            if (string.IsNullOrEmpty(allowedChars)) throw new ArgumentException("allowedChars may not be empty.");

            const int byteSize = 0x100;
            var allowedCharSet = new HashSet<char>(allowedChars).ToArray();
            if (byteSize < allowedCharSet.Length) throw new ArgumentException(String.Format("allowedChars may contain no more than {0} characters.", byteSize));

            // Guid.NewGuid and System.Random are not particularly random. By using a
            // cryptographically-secure random number generator, the caller is always
            // protected, regardless of use.
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                var result = new System.Text.StringBuilder();
                var buf = new byte[128];
                while (result.Length < length)
                {
                    rng.GetBytes(buf);
                    for (var i = 0; i < buf.Length && result.Length < length; ++i)
                    {
                        // Divide the byte into allowedCharSet-sized groups. If the
                        // random value falls into the last group and the last group is
                        // too small to choose from the entire allowedCharSet, ignore
                        // the value in order to avoid biasing the result.
                        var outOfRangeStart = byteSize - (byteSize % allowedCharSet.Length);
                        if (outOfRangeStart <= buf[i]) continue;
                        result.Append(allowedCharSet[buf[i] % allowedCharSet.Length]);
                    }
                }
                return result.ToString();
            }
        }


        public static void SaveMemoryStream(MemoryStream ms, string FileName)
        {

            using (FileStream file = new FileStream(FileName, FileMode.Create, System.IO.FileAccess.Write))
            {
                byte[] bytes = new byte[ms.Length];
                ms.Read(bytes, 0, (int)ms.Length);
                file.Write(bytes, 0, bytes.Length);
                ms.Close();
                ms.Dispose();

            }
        }

        // identica alla utility di XAgentController
        // con la differenza di cosa controllano
        public static int checkSemafori(bool silent = true, bool forza = false)
        {
            // me= AGENT
            // lui= CONTROLLER


            string luiNome = "XAgentController.exe";
            string meNome = "XipeAgent.exe";

            if (!SendEmail && !forza) return 0;

            string agent = "agent;" + DateTime.Now;
            string controller = "controller;";
            var items = new List<string>();
            int err = 0;
            int cnt = 0;
            string Eccezione = "";
            
            bool isC = false;
            DateTime dNew;
          

            try
            {
                if (File.Exists(SharedFile))
                    items = LoadTextFile(SharedFile);

                if (items.Count == 0)
                {
                    // file nuovo, salvo la data mia dell'agent ed esco
                    AppendToFile(SharedFile, agent, true, true);
                    WriteToEventLog(mainForm, $"MONITOR  {meNome} scrive: Registrata presenza su file processi attivi");
                    return 0;
                }

                foreach (var e in items)
                {
                    if (!string.IsNullOrEmpty(e))
                    {

                        cnt++;
                        var ar = e.Split(';');

                        // ci assicuriamo che il controller sia su, perche i due si controllano a vicenda
                        if (ar[0] == "controller")
                        {
                            isC = true;

                            if (!DateTime.TryParse(ar[1], out dNew))
                                err = 1;
                            else
                            {
                                ControlledLastActivity = dNew;

                                if ((DateTime.Now - dNew).TotalMinutes > MaxDelayInMinutes)
                                    err = 2;  // Ecceduto il tempo limite che il controller non logga piu, sicuro è spento o bloccato in errore
                               
                            }
                            // ci rimette quello che c'era perche non è roba nostra
                            controller += ar[1];
                        }
                       
                    }
                }



                // aggiorna il file condiviso con la nuova data per noi e abbiamo verificato il controller
                if (isC)
                {
                    AppendToFile(SharedFile, agent, true, true); // qui cancella e reinizializza il file
                    AppendToFile(SharedFile, controller);        // normale append
                }
                else
                {
                    AppendToFile(SharedFile, agent, true, true);        // il controller non c'ha mai scritto
                }
                WriteToEventLog(mainForm, $"MONITOR  {meNome} scrive: Sono in esecuzione");

            }
            catch (Exception ex)
            {
                err = 3;
                Eccezione = ex.Message;
            }



            string te = ElapsedTimeToString(DateTime.Now, ControlledLastActivity, false, 1, "past");

            if (err == 0)
            {
                string textLog = $"MONITOR  {luiNome}: Ultima volta visto in esecuzione il {ControlledLastActivity.ToString("dd/MM/yyyy")} alle ore {ControlledLastActivity.ToString("HH:mm:ss")} ({te}) e risulta regolarmente in esecuzione";

                if (!silent)
                    MessageBox.Show(textLog);

                WriteToEventLog(mainForm, textLog);

                return 0;
            }

            string sub = $"MONITOR  ALERT da {meNome}, ha rilevato anomalie a carico del Controller {luiNome}";
            string body = "";

          
            string lui2 = "il controller";
            string lui3 = "del controller";

            string me1 = "L'agent";
            string me2 = "l'agent";
            string me3 = "dell'agent";

            string valo = controller;


            if (err == 1)
            {
                string textLog = $"MONITOR  {luiNome} ha registrato una data illeggibile dal sistema, controllare il file {SharedFile}";

                body = $"Err01: { me1} ha rilevato una data non valida scritta {lui3} {luiNome}. ";
                body += "(formato o valore) e non è possibile verificarne l'inattività.";
                body += LF + "E' richiesto un intervento manuale al fine di individuare le cause dell'anomalia.";
                body += $"Il file da controllare che condividono agent e controller è {SharedFile} ";
                body += "e deve contenere max due linee, una scritta dall'agent e una dal controller";
                body += LF + $"Entrambi si controllano a vicenda, verificare che le date siano effettivamente congrue";

                WriteToEventLog(mainForm, $" ");
                WriteToEventLog(mainForm, $"Err01: { me1} ha rilevato una data non valida scritta {lui3} {luiNome}. ");
                WriteToEventLog(mainForm, "(formato o valore) e non è possibile verificarne l'inattività.");
                WriteToEventLog(mainForm, $"E' richiesto un intervento manuale sul file  {SharedFile}  al fine di individuare le cause dell'anomalia.");

            }

            if (err == 2)
            {
                var t2 = new List<string>();
                t2.Add($"Err02: {me1} {meNome} ha rilevato il superamento del tempo massimo di inattività {lui3} {luiNome}.");
                t2.Add(LF + "Potrebbe essere fermo su un errore oppure in attesa di un input dall'utente oppure è stato volontariamente chiuso o autonomamente per altre cause.");
                t2.Add(LF + $"L'ultima data/ora che {lui2} ha registrato è stata '{valo}'. ");
                t2.Add(" ");
                t2.Add(LF + LF + $"Il file condiviso dall'agent e dal controller è {SharedFile}. ");
                t2.Add("Esso deve contenere max due linee, una scritta dall'agent e una dal controller");
                t2.Add($"Attraverso questo file si controllano a vicenda, verificare che non ci siano anomalie nel file e ripristinare i due servizi");
                t2.Add(" ");


                // costruzione body
                bool ff = true;
                foreach (var ee in t2)
                {
                    if (ff)
                    {
                        body = ee;
                        ff = false;
                    }
                    else
                        body += ee;

                    WriteToEventLog(mainForm, ee);
                }

            }

            if (err == 3)
            {
                body = $"{me1} è andato in eccezione, il testo del messaggio è il seguente";
                body += $"{Eccezione} il {DateTime.Now}";
                body += LF + $"Dopo l'invio di questa segnalazione {me2} si è chiuso autonomamente, ";
                body += $"Individuare le cause dell'errore e ripristinare il servizio quanto prima {me3} ";

                body += LF + $"Il file condiviso dall'agent e dal controller è {SharedFile}. ";
                body += "Esso deve contenere max due linee, una scritta dall'agent e una dal controller";
                body += LF + $"Attraverso questo file si controllano a vicenda, verificare che non ci siano anomalie nel file e ripristinare i due servizi";

            }


            body += LF + LF + "Questa email non sarà inviata piu di una volta al giorno e sarà comprensiva ";
            body += "di tutte le anomalie pregresse non ancora notificate.";

            WriteToEventLog(mainForm, $" ");
            InviaSegnalazione(sub, body, silent);
            WriteToEventLog(mainForm, $" ");

            return err;

        }


        public static void ShellExec(string cmd, string args = "")
        {
            var p = new System.Diagnostics.Process();
            p.StartInfo.FileName = cmd;
            p.StartInfo.Arguments = args;
            p.Start();
        }


        public static void ViewTextFile(string fl)
        {
            //  var p = new System.Diagnostics.Process();
            System.Diagnostics.Process.Start("notepad", fl);

        }



        public static void InviaSegnalazione(string subject, string body, bool silent = true)
        {

            bool alreadysent = lastMailSent.Day == DateTime.Now.Day && lastMailSent.Month == DateTime.Now.Month;

            try
            {
                string ef = ErrFile;
                var lst = LoadTextFile(ef);
                if (lst.Count > 0)
                {
                    body += LF + "====================================================================";
                    body += LF + "             Eccezioni generate da XIpeAgent non notificate";
                    body += LF + "====================================================================";

                    int u = 0;
                    foreach (var s in lst)
                    {
                        u++;
                        body += LF + $"{u})  {s}";
                    }

                    // una volta notificato va azzerato
                    File.Delete(ef);
                }
                // sostituisci il msg con l'invio mail

                eMailFile = pathLog + "\\mail.txt";

                if (alreadysent)
                    body += LF + LF + $"NELLA GIORNATA DI OGGI UNA MAIL E' GIA STATA INVIATA E QUESTO TESTO NON E' STATO RECAPITATO";

                AppendToFile(eMailFile, subject, true, true);
                AppendToFile(eMailFile, body);

                lastMailSent = DateTime.Now;

         
                iProdCFG.SaveSettings(cfgFile);
                

                string txtFinal = $"Agent e Controller ALERT: Controlla il file {eMailFile}, contiene informazioni sull'anomalia riscontrata";

                if (alreadysent)
                    if (!silent) MessageBox.Show(txtFinal);
                    else
                    {

                        lastMailSent = DateTime.Now;
                        iProdCFG.SaveSettings(cfgFile);

                        if (!IsNull(eMailNotifiche))
                            SendMailOneAttachment(mainForm, "info@pieraccimeccanica.it", eMailNotifiche, subject, body);

                        if (!silent) MessageBox.Show(txtFinal);
                    }
            }
            catch (Exception ex)
            {

                WriteToEventLog(mainForm, "Errore in InvioSegnalazione (monitoraggio): " + ex.Message);
            }

        }


        public static void SendMailOneAttachment(Form1 parent, string da, string sendTo, string Subject, string messaggio, string AttachmentFile = "", string CC = "", string BCC = "", string SMTPServer = "")
        {

            var m = new MailMessage(da, sendTo);
            m.Subject = Subject;
            if (!string.IsNullOrEmpty(AttachmentFile))
            {
                var allegati = AttachmentFile.Split(';');
                foreach (var a in allegati)
                {
                    var allego = new Attachment(a);
                    m.Attachments.Add(allego);
                }
            }

            m.Body = messaggio.Replace(LF, "<br/>");

            if (string.IsNullOrEmpty(SMTPServer))
                SMTPServer = "pieraccimeccanica-it.mail.protection.outlook.com";

            if (CC != "") m.CC.Add(CC);
            if (BCC != "") m.Bcc.Add(BCC);


            var invio = new System.Net.Mail.SmtpClient(SMTPServer);

            try
            {

                invio.Send(m);
                WriteToEventLog(parent, "Invio email a " + sendTo);


            }
            catch (Exception Err)
            {
                WriteToEventLog(parent, $"Errore invio email a {sendTo}: {Err.Message}");
            }


        }


     

#region Operazioni sui nomi di File

        /// <summary>
        /// Restituisce lo stesso nome file ma con il timestamp aggiunto alla fine -yyyyMMdd-HHmmss-fffff es: c:\pippo.txt --> c:\pippo-20210213-183225-24510.txt 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string FileGetAppendTimeStamp(string file)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss-fffff");
            var extOnly = FileGetExtOnly(file);
            var noExt = FileGetNoExt(file);
            
            var newFile = $"{noExt}-{timestamp}.{extOnly}";


            return newFile;
        }

        /// <summary>
        ///  Restituisce il nome del file senza estensione e punto es: c:\pippo.txt --> c:\pippo 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string FileGetNoExt(string file)
        {
            var extOnly = FileGetExtOnly(file);
            var newFile = file.Substring(0, file.Length - (extOnly.Length + 1));

            return newFile;
        }


        /// <summary>
        /// Restituisce la parte path a sinistra del nome file es:  c:\pippo.txt --> c:\
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string FileGetPathOnly(string file)
        {
            var nameOnly = FileGetNameOnly(file);
            var pathOnly = file.Substring(0, file.Length - (nameOnly.Length + 1));
            return pathOnly;

        }

        /// <summary>
        /// Restituisce il solo nome file   c:\pippo.txt --> pippo.txt
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string FileGetNameOnly(string file) => file.Split('\\').Last();

        /// <summary>
        /// Restituisce la sola estensione del file es:   c:\pippo.txt --> txt
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string FileGetExtOnly(string file) => file.Split('.').Last();
        /// <summary>
        ///  Restituisce true se il file è stato creato oggi
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool FileGetIfCreatedToday(string file) => (NotNull(file) && File.Exists(file)) ? new FileInfo(file).CreationTime.Date == DateTime.Now.Date : false;

#endregion

#region Operazioni sui Files


        /// <summary>
        /// Copia o Sposta un file salvandone una copia aggiungendo il timestamp nel nome
        /// </summary>
        /// <param name="f"></param>
        /// <param name="destPath"></param>
        /// <param name="move"></param>
        /// <param name="bytesLimit"></param>
        /// <returns></returns>
        public static bool CopySafe(string f, string destPath, bool move = false, long bytesLimit = 0)
        {

            if (!File.Exists(f)) return false;

            var b = new FileInfo(f).Length;

            if (bytesLimit > 0)
            {
                // se specificato e la dimensione è inferiore non deve salvare niente e se è richiesto il move si cancella e basta, se no si esce
                if (b < bytesLimit)
                {
                    if (move) File.Delete(f);
                    return true;
                }
            }

            var pathOnly = FileGetPathOnly(f);
            var newFile = FileGetAppendTimeStamp(f);

            newFile = newFile.Replace(pathOnly, destPath);
            if (move)
            {
                if(FileWaitLockedFile(f, 5000))
                File.Move(f, newFile);
            }
            else
                File.Copy(f, newFile);

            return true;

        }



        /// <summary>
        /// Attesa file locked per un massimo di millisecondi specificati in timeout, 0 = infinito (non raccomandato)
        /// </summary>
        /// <param name="file"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static bool FileWaitLockedFile(string file, double timeout = 2000)
        {
            try
            {
                bool infinite = timeout == 0;
                var started = DateTime.Now;
                var elpsd = 0D;
                do
                {
                    var locked = FileIsLocked(file, FileAccess.ReadWrite);
                    if (!locked) return true;
                    Application.DoEvents();
                    elpsd = (DateTime.Now - started).TotalMilliseconds;
                } while (infinite || (!infinite && elpsd < timeout));

                if (elpsd > timeout) return false; // superato tempo massimo

                return true;
            }
            catch (Exception ex)
            {
                IOLog(ex.Message + "   " + ex.StackTrace);
                throw ex;
            }

        }

        /// <summary>
        /// Return true if the file is locked for the indicated access. 
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="file_access"></param>
        /// <returns></returns>
        public static bool FileIsLocked(string filename, FileAccess file_access)
        {
            // Try to open the file with the indicated access.
            try
            {
                FileStream fs =
                    new FileStream(filename, FileMode.Open, file_access);
                fs.Close();
                return false;
            }
            catch (IOException)
            {
                return true;
            }
            catch (Exception ex)
            {
                IOLog(ex.Message + "   " + ex.StackTrace);
                throw ex;
            }
        }

#endregion

#region Utility block

        public static string IOLog(string msg, bool init = false, bool renamefirst = false)
        {
            if (string.IsNullOrEmpty(LogFile)) throw new Exception("Tentativo di utilizzare il file di log senza nome");


            if (renamefirst || (init && File.Exists(LogFile)))
            {
                // se esiste il log lo rinomino
                if (File.Exists(LogFile))
                {
                    if (new FileInfo(LogFile).Length > 5000)
                    {
                        CopySafe(LogFile, pathBackups, true);
                       
                    }
                }
            }


            int hh = DateTime.Now.Hour;
            int m = DateTime.Now.Minute;
            int s = DateTime.Now.Second;

            string str = "";
            //if (lastlog > DateTime.MinValue)
            //{
            //    if (lastlog.Hour != hh)
            //        str = hh.ToString("00") + ":";
            //    else
            //        str = "  :";

            //    if (lastlog.Minute != m)
            //        str += m.ToString("00") + ":";
            //    else
            //        str += " :";

            //    if (lastlog.Second != s)
            //        str += s.ToString("00");
            //    else
            //        str += " :";
            //}
            //else
            str = DateTime.Now.ToString("HH:mm:ss.ffff");

            lastlog = DateTime.Now;
            string v = $"{str} | {msg}";
            AppendToFile(LogFile, v, true, init);
            return v;

        }


        public static DialogResult ShowInputDialog(ref string input, string title = "", Form parent = null)
        {
            System.Drawing.Size size = new System.Drawing.Size(300, 130);
            Form inputBox = new Form();

            if (parent != null)
            {
                inputBox.Parent = parent;
                inputBox.StartPosition = FormStartPosition.CenterParent;
            }
            else
                inputBox.StartPosition = FormStartPosition.CenterScreen;
            inputBox.FormBorderStyle = FormBorderStyle.FixedDialog;
            inputBox.ClientSize = size;
            inputBox.Font = new System.Drawing.Font("Seoge UI", 12);
            inputBox.Text = title;

            System.Windows.Forms.TextBox textBox = new TextBox();
            textBox.Size = new System.Drawing.Size(size.Width - 20, 23);
            textBox.Location = new System.Drawing.Point(10, 20);
            textBox.Text = input;
            inputBox.Controls.Add(textBox);

            Button okButton = new Button();
            okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            okButton.Name = "okButton";
            //okButton.Size = new System.Drawing.Size(75, 23);
            okButton.Size = new System.Drawing.Size(75, 50);
            okButton.Text = "&OK";
            okButton.Location = new System.Drawing.Point(size.Width - 80 - 120, 60);
            //okButton.Location = new System.Drawing.Point(size.Width - 80 - 80, 39);
            inputBox.Controls.Add(okButton);

            Button cancelButton = new Button();
            cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(90, 50);
            cancelButton.Text = "&Annulla";
            //cancelButton.Location = new System.Drawing.Point(size.Width - 80, 39);
            cancelButton.Location = new System.Drawing.Point(size.Width - 120, 60);
            inputBox.Controls.Add(cancelButton);

            inputBox.AcceptButton = okButton;
            inputBox.CancelButton = cancelButton;

            DialogResult result = inputBox.ShowDialog();
            input = textBox.Text;
            return result;
        }


        public static string getCacheFileName(string what)
        {
            string cacheFile = "";
            if (what == "iproduser") cacheFile = "profilo.dat";
            if (what == "login") cacheFile = "profilo.dat";
            if (what == "warehouses") cacheFile = "warehouses.dat";
            if (what == "customers") cacheFile = "customers.dat";
            if (what == "items") cacheFile = "items.dat";
            if (what == "phases") cacheFile = "phases.dat";
            if (what == "machines") cacheFile = "machines.dat";
            if (what == "boms") cacheFile = "boms.dat";
            if (what == "orders") cacheFile = "ordini.dat";
            if (what == "categories") cacheFile = "categories.dat";


            if (string.IsNullOrEmpty(cacheFile)) throw new Exception($"Richiesta cache di tipo sconosciuto: {cacheFile}");

            return utility.pathCache + "\\" + cacheFile;
        }

        public static T LoadObjectFromStream<T>(string mFileName, T obj)
        {
            var FileName = pathCache + "\\" + mFileName;
            if (mFileName.StartsWith("C:")) FileName = mFileName;

            if (!File.Exists(FileName)) return default(T);

            // Progetto = new List<ProjectFile>();
            FileStream stream = new FileStream(FileName, FileMode.Open);
            BinaryFormatter formatter = new BinaryFormatter();
            var data = (T)formatter.Deserialize(stream); // as typeof(obj);
            stream.Dispose();
            stream = null;
            return data;
        }

        public static void SaveStream(object data, string mFileName, bool silent = true)
        {
            try
            {
                var FileName = pathCache + "\\" + mFileName; ;
                if (File.Exists(FileName)) File.Delete(FileName);

                using (FileStream stream = new FileStream(FileName, FileMode.Create))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, data);
                    stream.Close();
                    formatter = null;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private static SemaphoreSlim semaphore = new SemaphoreSlim(1);
        public static async Task SaveStreamAsync(object data, string mFileName)
        {
            var FileName = pathCache + "\\" + mFileName;
            if (mFileName.StartsWith("C:\\")) FileName = mFileName;

            await semaphore.WaitAsync();
            try
            {
                await Task.Factory.StartNew(() =>
                {
                    if (File.Exists(FileName)) File.Delete(FileName);
                    using (FileStream stream = new FileStream(FileName, FileMode.Create))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        formatter.Serialize(stream, data);
                        stream.Close();
                        formatter = null;
                    }
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                semaphore.Release();
            }
        }

        public static async Task<T> LoadObjectFromStreamAsync<T>(string mFileName, T obj)
        {

            var FileName = pathCache + "\\" + mFileName;

            if (!File.Exists(FileName)) return default(T);

            //throw new Exception("Tentativo di salvataggio in cac")
            T ddi;
            await semaphore.WaitAsync();
            try
            {
                await Task.Factory.StartNew(() =>
                {
                    if (!File.Exists(FileName)) return default(T);

                    FileStream stream = new FileStream(FileName, FileMode.Open);
                    BinaryFormatter formatter = new BinaryFormatter();
                    var data = (T)formatter.Deserialize(stream);
                    ddi = data;
                    stream.Dispose();
                    stream = null;
                    return data;
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                semaphore.Release();
            }

            return default(T);
        }
        
#endregion

        // non è utilizzato
        public static class html
        {

            // get nome del tag, from è la posizione del carattere < che apre il tag
            public static string getTag(string str, int from = 0)
            {
                int i = from + 1;
                var s = ScrollTo(str, i, ">");
                if (s < from) return "";
                var t = str.Substring(i, str.Length - i);

                return "";
            }

            public static string cleanConst(string str, int from = 0, int len = 0)
            {
                if (len == 0) len = str.Length - from;
                if (len <= 0) return "";

                int i1 = 0;
                int i2 = 0;

                for (int i = from; i < i + len; i++)
                {
                    string ap = str.Substring(i, 1);
                    if (ap == "\"")
                    {
                        if (i1 == 0)
                        {
                            i1 = i;
                            i2 = 0;
                        }
                        else
                        {
                            i2 = i - i1;
                        }


                    }
                }


                return "";
            }


            public static int ClosingIndex(string str, string tag, int from = 0)
            {
                int c = from;
                int cs = c;
           
                string ct = ""; // chiusura tag

                if (tag == "input")
                    ct = "/>";
                else
                    ct = "</" + tag + ">";

                do
                {
                    c = ScrollTo(str, c, "/");
                    if (c > 0)
                    {
                        string c0 = str.Substring(c - 1, 1);
                        string c2 = str.Substring(c + 1, 1);
                        //if (c0 == "<")

                        //    else if (c2 == ">")




                    }
                } while (c >= 0);

                return 1;
            }

        }



#endregion


        public class httpErr
        {
            public string type { get; set; }
            public string title { get; set; }
            public int status { get; set; }
            public string traceId { get; set; }

        }

        public class Contatore
        {
            public string entity { get; set; }
            public double read { get; set; }
            public int processed { get; set; }
            public int RowCount { get; set; }
            public int Added { get; set; }
            public int Updated { get; set; }
            public int Custom { get; set; }
            public int Skipped { get; set; }
            public int Witherror { get; set; }
            public DateTime dStart { get; set; }
            public DateTime dEnd { get; set; }
            public DateTime LastRun { get; set; }
            public string Elapsed { get; set; }
            public bool Completed { get; set; }
            public bool Loaded { get; set; }
            public string ErrFile { get; set; }
            public string Step { get; set; }

            public bool inError { get; set; }
            public string ErrorText { get; set; }

            private bool firstErr = false;

            public Contatore(string who)
            {
                entity = who;
                Clear();
            }

            public string ProcessCompleted(bool success = true, bool manuale = false)
            {
                string m = "";
                dEnd = DateTime.Now;
                Completed = true;

                if(Loaded)
                {
                    m = $"Pronti {RowCount} elementi {entity} in memoria";
                    return m;
                }


                if (manuale)
                {
                    Elapsed = ElapsedTime();
                    m = $"Caricati {RowCount} elementi di {entity} in {Elapsed}";
                }
                else
                    m = getString();

                if (!success) m += " INTERROTTO A CAUSA DI UN ERRORE";
                return m;
            }

            private void InitErrori()
            {
                ErrFile = $"{pathLog}\\{entity}ERRORS{DateTime.Now:MMddhhmm}.txt";
                AppendToFile(ErrFile, $" Log Errori {entity} processo avviato {DateTime.Now:F}");
                AppendToFile(ErrFile, $" ");

            }

            public void LogErr(string msg)
            {
                if (!firstErr) InitErrori();
                firstErr = true;
                AppendToFile(ErrFile, $"{DateTime.Now:dd hh:mm:ss:fff}|{read}| {msg}");
            }


            public void Clear()
            {
                read = 0;
                processed = 0;
                RowCount = 0;
                Added = 0;
                Updated = 0;
                Witherror = 0;
                Elapsed = "";
                Skipped = 0;
                Completed = false;
                Step = "";

                dStart = DateTime.Now;

            }

            public bool isNotEmpty() => Added > 0 || Updated > 0;

            public string getString()
            {
                Elapsed = ElapsedTime();
                return $"Riepilogo {entity}: {RowCount} records caricati, aggiunti {Added} elementi ad iProd, {Updated}  aggiornati, {Witherror} errori in {Elapsed} ";
            }


            public Contatore sum(Contatore b)
            {
                read += b.read;
                processed += b.processed;
                Added += b.Added;
                Updated += b.Updated;
                Witherror += b.Witherror;
                Skipped += b.Skipped;

                return this;

            }


            public string ElapsedTime()
            {
                if ((DateTime.Now - dStart).TotalDays > 2) return "Impossibile stabilire il tempo trascorso per il processo di migrazione. Timer di avvio non impostato";
                string ret = "";
                ret = ElapsedTimeToString(DateTime.Now, dStart, true);
                return ret;
            }

        }

        public class Counters
        {
            public Contatore CLI { get; set; }
            public Contatore ART { get; set; }
            public Contatore FAS { get; set; }
            public Contatore GIA { get; set; }
            public Contatore MAG { get; set; }
            public Contatore MAC { get; set; }
            public Contatore DIS1 { get; set; }
            public Contatore DIS2 { get; set; }
            public Contatore DOCS { get; set; }
            public Contatore FOTO { get; set; }

            public Counters()
            {

                CLI = new Contatore("Customers");
                ART = new Contatore("Items");
                GIA = new Contatore("Stocked(Giacenze)");
                FAS = new Contatore("Phases");
                MAG = new Contatore("Magazzini");
                MAC = new Contatore("Macchinari");
                DIS1 = new Contatore("Boms_step_1");
                DIS2 = new Contatore("Boms_step_2");
                DOCS = new Contatore("Documentale");
                FOTO = new Contatore("Immagini");
            }

            public void Clear()
            {
                new Counters();
            }

            public Contatore sumAll()
            {
                CLI.sum(ART.sum(GIA.sum(FAS.sum(MAG.sum(MAC.sum(DIS1.sum(DIS2.sum(DOCS.sum(FOTO)))))))));
                return CLI;
            }

            public string ValutaInterruzioni()
            {

                List<string> intr = new List<string>();
                var st = "Tutte le sequenze di dati sono stati migrati senza interruzioni";
                if (!ART.Completed) intr.Add("Elaborazione articoli non completata");


                if (intr.Count > 0)
                {
                    st = "Non tutte le attività sono andate a buon fine. Segue il dettaglio dei processi interrotti:";
                    st += LF + string.Join(LF, intr.ToArray());

                }


                return st;

            }

        }

        public static Contatore cntGlobale = new Contatore("Totali");

        // Returns the human-readable file size for an arbitrary, 64-bit file size 
        // The default format is "0.### XB", e.g. "4.2 KB" or "1.434 GB"
        public static string GetBytesReadable(long i, string msk = "", int decimali = 0)
        {
            // Get absolute value
            long absolute_i = (i < 0 ? -i : i);
            // Determine the suffix and readable value
            string suffix;
            double readable;
            if (absolute_i >= 0x1000000000000000) // Exabyte
            {
                suffix = "EB";
                readable = (i >> 50);
            }
            else if (absolute_i >= 0x4000000000000) // Petabyte
            {
                suffix = "Pb";
                readable = (i >> 40);
            }
            else if (absolute_i >= 0x10000000000) // Terabyte
            {
                suffix = "Tb";
                readable = (i >> 30);
            }
            else if (absolute_i >= 0x40000000) // Gigabyte
            {
                suffix = "Gb";
                readable = (i >> 20);
            }
            else if (absolute_i >= 0x100000) // Megabyte
            {
                suffix = "Mb";
                readable = (i >> 10);
            }
            else if (absolute_i >= 0x400) // Kilobyte
            {
                suffix = "Kb";
                readable = i;
            }
            else
            {
                return i.ToString("0 B"); // Byte
            }
            // Divide by 1024 to get fractional value
            readable = (readable / 1024);
            // Return formatted number with suffix
            if (decimali > 0)
                return readable.ToString("0.### ").Replace(",", ".") + suffix;
            else
                return readable.ToString("0").Replace(",", ".") + suffix;
        }


        public static string AssemblyVersion()
        {

            string dex = "";

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            Version version = assembly.GetName().Version;

            //  string myVer = $"{version.Major}.{version.Minor}.{version.Revision}";

            var descriptionAttribute = assembly
                                    .GetCustomAttributes(typeof(System.Reflection.AssemblyDescriptionAttribute), false)
                                    .OfType<System.Reflection.AssemblyDescriptionAttribute>()
                                    .FirstOrDefault();

            if (descriptionAttribute != null) dex =descriptionAttribute.Description;

            return $"{version} {dex}" ;
        }


        public static async Task<HttpResponseMessage> httpdeletecall(string dataparameter)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(EndPointIPROD);
            var result = await httpClient.DeleteAsync(dataparameter);
            //result.Wait();
            return result; //.Result;
        }

        public static bool VerboseMax { get; set; }
        public static bool Ask(string msg)
        {
            msg += "\r Premi Annulla per interrompere, Ok per continuare";
            var ret = MessageBox.Show(msg, "iProdWHSE", MessageBoxButtons.OKCancel);
            if (ret == DialogResult.Cancel) return false;

            return true;
        }

        public static HttpResponseMessage httppostcall(string dataparameter, StringContent dataupdate)
        {


            using (var Client = new HttpClient())
            {
                Client.Timeout = Timeout.InfiniteTimeSpan;
                Client.BaseAddress = new Uri(EndPointIPROD);
                var result = Client.PostAsync(dataparameter, dataupdate).Result;
                return result;
            }


            //var httpClient = new HttpClient();
            //httpClient.Timeout = Timeout.InfiniteTimeSpan;
            //httpClient.BaseAddress = new Uri(EndPointIPROD);
            //var result = httpClient.PostAsync(dataparameter, dataupdate).Result;
            //return result; 
        }


        /// <summary>
        /// Carica un file di testo in una lista di stringhe
        /// </summary>
        /// <param name="FileName">Nome file di testo</param>
        /// <param name="skipComments">true per non includere commenti # e righe nulle</param>
        /// <returns></returns>
        public static List<string> LoadTextFile(string FileName, bool skipComments = false)
        {
            // carica in memoria un intero  file di testo 
            try
            {
                UT.Sleep(200); // unlock file
                var lst = new List<string>();
                if (!File.Exists(FileName)) return lst;
                if (!skipComments)
                    return File.ReadAllText(FileName).Replace("\r", "").Split('\n').ToList();

                var lst2 = new List<string>();
                lst = File.ReadAllText(FileName).Replace("\r", "").Split('\n').ToList();
                foreach (var line in lst)
                    if (line.Length > 0 && !line.StartsWith("#")) lst2.Add(line);

                return lst2;
            }
            catch (Exception ex)
            {

                throw ex;
            }



        }

        private static bool IsFileLocked(FileInfo file)
        {
            try
            {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }

            //file is not locked
            return false;
        }

        public static bool AppendToFile(string nomeFile, string testo, bool addNewline = true, bool deletefirst = false)
        {
            if (!EnableLogToDisk) return true;

            try
            {
                if (nomeFile == "") return false;
                StreamWriter fs;
                if (!File.Exists(nomeFile))
                {
                    string cartella = Path.GetDirectoryName(nomeFile);
                    if (!string.IsNullOrEmpty(cartella) && !Directory.Exists(cartella))
                        Directory.CreateDirectory(cartella);
                    fs = new StreamWriter(nomeFile, false);
                }
                else
                {
                    if (deletefirst)
                        File.Delete(nomeFile);
                    fs = new StreamWriter(nomeFile, true);
                }
                if (addNewline)
                    fs.WriteLine(testo);
                else
                    fs.Write(testo);
                fs.Flush();
                fs.Close();
                fs.Dispose();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }



        public static string ElapsedTimeToString(DateTime recent, DateTime past, bool Formattato = false, int Details = 0, string direction = "neutral")
        {

            if (past > recent) // WriteToEventLog(mainForm, $"ATTENZIONE!!  Errore in ElapsedTimeToString(): Date Errate, la prima passata {recent} alla funzione non è piu recente della seconda {past}");
                return ElapsedTimeToString(past - recent, Formattato, Details, "future");
            else
                return ElapsedTimeToString(recent - past, Formattato, Details, direction);

        }


        public static string ElapsedTimeToString(TimeSpan t, bool Formattato = false, int Details = 0, string direction = "neutral")
        {



            /*   NUOVA VERSIONE v3.1.0121

            - formattato   (HR: human readable o meno) 

              true: applica la formattazione classica di windows.
                    se non deve essere HR ma con formattazione classica di windows
                    1:22:45 returns 01:22:45

              false: se invece sappiamo che lo dovra leggere un essere umano dobbiamo fornirgli qualcosa di piu discorsivo,
                     quindi restituisce una stringa HR formattata in tempistica 
                     01:22:45 returns
                    "un ora, 22 minuti e 45 secondi" con alcune opzioni possibili:

            - Details 
               0: non applica nessuna normalizzazione
               1: se il tempo è effettivamente sotto il minuto restituisce "meno di un minuto"
              >1: se il tempo si trova alla sinistra della metà di Details restituisce "Adesso" 
                  ovvero, se passiamo Details = 10, vogliamo che sotto 5 i minuti restituisca sempre 'Adesso'.
                  Se invece il tempo si trova alla destra della metà di Details e inferiore al suo valore restituisce "poco fa" (in aggiunta a direction: tra poco, poco fa) 
                  ovvero, se passiamo Details=10 e i minuti trascorsi sono 7, verifico la direzione e se è "past" 
                          restituisco "poco fa", se è "future" restituisco "tra poco" (past è di default)

                    es: Details = 6: vogliamo che sotto i 5 minuti arrotondi a tra poco, poco fa, o se è sotto la meta di 6 è sempre adesso
                                Details = 6, t = 0:2:12 restituisce "adesso"
                                      t = 0:5:57 restituisce (tra)"poco"(fa)
                                      t = 2:2:12 restituisce due ore, 2 minuti e 12 secondi (non applica nulla) 



            direction = se il tempo esprime un periodo del passato o del futuro
               "past"    - aggiunge fa:  (un ora) fa
               "future"  - aggiunge tra:  tra (un ora)
               "neutral" - non aggiunge niente

            Attenzione: con Formattat


         nel chiamante :

          xxx.PrintElapsedTime(tStart, DateTime.Now, LB1);

         public static string PrintElapsedTime(DateTime strt, DateTime fine, ListBox LB = null, string title = "Operation")
         {
             var txt = Utils.ElapsedTimeToString(fine, strt, true);
             if (LB != null)
             {
                 LB.Items.Add("Elapsed Time for " + title + ": " + txt);
                 LB.SelectedIndex = LB.Items.Count - 1;
             }
             return txt;

         }

          */

            try
            {

                var isP = direction == "past";
                var isF = direction == "future";
                var isN = direction == "neutral";


                int aa, mm;
                string _a, _m, _g, _h, _mn, _s;



                var h = t.Hours;
                var m = t.Minutes;
                var s = t.Seconds;
                var dd = t.Days;

                //double ta = 31536000; // 1 anno in secondi
                //double tm = 2592000;  // 1 mese (30 gg)
                //double td = 86400; // 1 giorno
                //double th = 3600; // 1 ora
                //double tM = 60;   // 1 min



                // si calcola tutto dalla scomposizione dei secondi





                _a = "";
                _m = "";
                _g = "";
                _mn = "";
                _s = "";
               
                _h = "";
                
              
                aa = 0;
                mm = 0;




                double ta = 0;
                double tm = 0;
                double td = 0;
                double th = 0;
                double tM = 0;
              

                double ca = 0;
                double cm = 0;
                double cd = 0;
                double ch = 0;
                double cM = 0;
               

                double se = t.TotalSeconds;

                // individuazione soglia

                 
                tM = 60;
                th = tM * 60;
                td = th * 24;
                tm = td * 30;
                ta = td * 365;

                if (se > ta)
                {
                    ca = se / ta;
                    var la = Convert.ToInt64(ca);
                    var rm = ca - la;
                    aa = (int)la;
                }

                if (aa > 0) se -= (ta * aa);


                if (se > tm)
                {
                    cm = se / tm;
                    var lm = Convert.ToInt64(cm);
                    var rm = cm - lm;
                    mm = (int)lm;
                }

                if (mm > 0) se -= (tm * mm);

                if (se > td)
                {
                    cd = se / td;
                    var ld = Convert.ToInt64(cd);
                    var rm = cd - ld;
                    dd = (int)ld;
                }

                if (dd > 0) se -= (td * dd);

                if (se > th)
                {
                    ch = se / th;
                    var lh = Convert.ToInt64(ch);
                    var rm = ch - lh;
                    h = (int)lh;
                }

                if (h > 0)
                    se -= (th * h);


                if (se > tM)
                {
                    cM = se / tM;
                    var lm = Convert.ToInt64(cM);
                    var rm = cM - lm;
                    m = (int)lm;
                }

                if (m > 0)
                    se -= (tM * m);

                s = Convert.ToInt32(se); ;  // dovrebbe restare sotto i il 60

                // per debug
                var sRet = $"y{aa}.m{mm}.d{dd}.h{h}.M{m}.s{s}";
                var sFormattato = $"{aa}/{mm}/{dd}/ {h}:{m}:{s}";


          
                bool mmS = false;
                bool ddS = false;
                bool hhS = false;
                bool mS = false;
                bool sS = false;

                int idx = 0;

                if (aa > 0)
                {
                
                    string anni = "anni";
                    if (aa == 1) anni = "anno";

                    if (mm == 0)
                        _a = $"{aa} {anni}";
                    else
                    {

                        if (mm > 6)
                        {
                            if (mm < 11)
                                _a = $"piu di {aa} {anni} e mezzo";
                            else
                                _a = $"quasi {aa + 1} anni";
                        }
                        else
                        {
                            if (mm < 4)
                                _a = $"piu di {aa} {anni}";
                            else
                                _a = $"quasi {aa} {anni} e mezzo";
                        }
                    }

                    // con questa distanza di tempo non ci interessa il dettaglio
                    return _a;
                }


                // calcolo mesi trascorsi (qui il dettaglio ci puo stare, andremo avanti)

                if (mm > 0)
                {
                    idx = 1;
                    mmS = true;
                    string mesi = "mesi";
                    if (mm == 1) mesi = "mese";

                    if (mm > 6) // sopra sei mesi anche qui ci fermiamo con i dettagli
                    {
                        if (mm < 11)
                            return $"piu di 6 mesi";
                        else
                            return $"quasi 1 anno";
                    }

                    _m = $"{mm} {mesi}";
                }


                if (dd > 0)
                {
                    idx++;
                    ddS = true;
                    string giorni = "giorni";
                    if (dd == 1) giorni = "giorno";

                    _g = $"{dd} {giorni}";

                }

                if (h > 0)
                {
                    idx++;
                    hhS = true;
                    string ore = "ore";
                    if (h == 1) ore = "ore";
                    if (h == 24)
                        _h = "1 giorno";
                    else
                        _h = $"{h} {ore}";
                }

                if (m > 0)
                {
                    idx++;
                    mS = true;
                    string minuti = "minuti";
                    if (m == 1) minuti = "minuto";
                    if (m == 60)
                        _mn = "1 ora";
                    else
                        _mn = $"{m} {minuti}";
                }

                if (s > 0)
                {
                    idx++;
                    sS = true;
                    string sec = "secondi";
                    if (s == 1) sec = "secondo";
                    if (s == 60)
                        _s = "1 minuto";
                    else
                        _s = $"{s} {sec}";
                }


                string ret = "";
                if (mmS)
                {
                    ret = _m;
                    if (idx == 2)       // se è due vuol dire che c'è solo un altro vlaore utile
                        ret += " e ";
                    else if (idx > 1)   // x tutti gli altri valori serva la virgola
                        ret += ", ";

                    idx--;
                }

                if (ddS)
                {
                    ret += _g;
                    if (idx == 2)       // se è due vuol dire che c'è solo un altro vlaore utile
                        ret += " e ";
                    else if (idx > 1)   // x tutti gli altri valori serva la virgola
                        ret += ", ";

                    idx--;
                }


                if (hhS)
                {
                    ret += _h;
                    if (idx == 2)       // se è due vuol dire che c'è solo un altro vlaore utile
                        ret += " e ";
                    else if (idx > 1)   // x tutti gli altri valori serva la virgola
                        ret += ", ";

                    idx--;
                }


                if (mS)
                {
                    ret += _mn;
                    if (idx == 2)       // se è due vuol dire che c'è solo un altro vlaore utile
                        ret += " e ";
                    else if (idx > 1)   // x tutti gli altri valori serva la virgola
                        ret += ", ";

                    idx--;
                }

                if (sS) ret += _s;


                if (direction == "past") ret += " fa";
                if (direction == "future") ret = "tra " + ret;

                return ret;

            }
            catch { return ""; }

        }

        public static bool WriteToEventLog(Form1 forma, string Entry, bool ShowUI = true)
        {

            try
            {

                //EventLog objEventLog = new EventLog();

         
                IOLog(Entry);

                //    if (!mainForm.isLogDetailed()) return true;

                if (forma != null && ShowUI)
                { 
                    forma.txtlogger.Text += Entry + Environment.NewLine;
                    forma.txtlogger.SelectionStart = forma.txtlogger.Text.Length;
                    forma.txtlogger.ScrollToCaret();
                }
                return true;
            }
            catch (Exception Ex)
            {
                UT.IOLog($"Errore in WriteEventToLog Eccezione '{Ex.Message}'");

                //if (forma != null)
                //{
                //    IOLog(Entry);

                //    forma.txtlogger.Text += Entry + Environment.NewLine;
                //    forma.txtlogger.SelectionStart = forma.txtlogger.Text.Length;
                //    forma.txtlogger.ScrollToCaret();
                //}
                return false;
            }

        }

        public static bool isNumeric(string str) 
        {
            foreach(var e in str.ToCharArray())
            {
                if (!"0123456789-.".Contains(e)) return false;
            }
            return true;
        }
    }


    public class UrlGateway
    {

        public string Key { get; set; }
        public string idTenant { get; set; }
        public string idUser { get; set; }
        public string NameOfUser { get; set; }
        public string TenantName { get; set; }
        public string WebUser { get; set; }
        public string WebPassword { get; set; }
        public string home { get; set; }
        public string login { get; set; }
        public string register { get; set; }
        public string lostpassword { get; set; }
        public string api { get; set; }
        public string EndPoint { get; set; }
        public string iProdData { get; set; }
        public string storageConn { get; set; }
        public string dbConnectionString { get; set; }

        // url di servizio funzionali al SYNC e BACKUP (non sono API)
        public string svcTOKEN { get; set; } // per le chiamate di verifica backup su iprod, il token è cablato
        public string svcGetBackups { get; set; }   //  "/GetTenantBackups";
        public string svcDoBackup { get; set; }   //  "/TenantBackup?backupname=<name>";
        public string svcSendEventLog { get; set; }   //  "/SendEventLog";
        public string svcBusy { get; set; }         //  "/IsSystemOperationInProgress";
        public string svcSetSyncOp { get; set; }    //  "/SetSystemOperation?operation=";  ('sincro' per informare iprod che è partito il sync, stringa vuota per dirgli che è finito)
        public string svcGetSystemTimestamp { get; set; }    //  "/GetSystemTimestamp";  get UTC date del server iProd da confrontare
        public TimeSpan svcTimeFaultTolerance { get; set; }  // max tempo di tolleranza tra le differenze della data del server e quella locale
        public TimeSpan svcBackupValidityTime { get; set; }  // tempo di validita dell'ultimo backup oltre il quale va rifatto

        public UrlGateway(string key)
        {
            Program.DEV = key == "dev";
            Key = key;

            switch (key)
            {
                case "dev":
                case "alpha":
                    home = "https://alpha.iprod.it/";
                    login = "https://alpha.iprod.it//Login/UserLogin/?ReturnUrl=%2F";
                    register = "https://alpha.iprod.it//Login/NewRegistration";
                    lostpassword = "https://alpha.iprod.it//Login/ForgotPassword";
                    iProdData = "https://iprodapiinternaltest.azurewebsites.net/iproddata/";
                    api = "https://alpha.iprod.it/api/";
                    WebUser = "info@iprod.it";
                    WebPassword = "1234";
                    dbConnectionString = "mongodb+srv://iprod_alpha:gecFfIZukhgSDfrg@iprod-alpha.ebktt.mongodb.net";
                    storageConn = "DefaultEndpointsProtocol=https;AccountName=iproddevelopment;AccountKey=wOadQhvMOfS8vssHCrUSBdHpztaxNpyxiqAIL8yeZUve5aoZTg4p5pPmudS8vaCsGolo38/ZAhhKlflvaW/rHw==;EndpointSuffix=core.windows.net";
                    TenantName = "iProd s.r.l. ALPHA";
                    idUser = "5dad8954b42e7703f47add22";
                    idTenant = "5dad8954b42e7703f47add21";
                    NameOfUser = "AMEDEO BRUNI";
                    break;
                case "test":
                case "beta":
                    home = "https://iprod-test.azurewebsites.net";
                    login = "https://iprod-test.azurewebsites.net/Login/UserLogin/?ReturnUrl=%2F";
                    register = "https://iprod-test.azurewebsites.net/Login/NewRegistration";
                    lostpassword = "https://iprod-test.azurewebsites.net/Login/ForgotPassword";
                    iProdData = "https://iproddeveloperapi.azurewebsites.net/iProdData/";
                    api = "https://iprod-test.azurewebsites.net/api/";
                    WebUser = "info@iprod.it";
                    WebPassword = "1234";
                    dbConnectionString = "mongodb+srv://iprodoutsourcing:Uw5IvFe5qg6F2hMj@developercluster-llkcs.azure.mongodb.net/admin?authSource=admin&replicaSet=developercluster-shard-0&w=majority&readPreference=primary&appname=MongoDB%20Compass&retryWrites=true&ssl=true";
                    storageConn = "DefaultEndpointsProtocol=https;AccountName=iproddevelopment;AccountKey=wOadQhvMOfS8vssHCrUSBdHpztaxNpyxiqAIL8yeZUve5aoZTg4p5pPmudS8vaCsGolo38/ZAhhKlflvaW/rHw==;EndpointSuffix=core.windows.net";
                    TenantName = "iProd s.r.l.";
                    idUser = "5dad8954b42e7703f47add22";
                    idTenant = "5dad8954b42e7703f47add21";
                    NameOfUser = "AMEDEO BRUNI";
                    break;
                case "localhost":
                    home = "https://localhost:44375";
                    login = "https://localhost:44375/Login/UserLogin/?ReturnUrl=%2F";
                    register = "https://localhost:44375/Login/NewRegistration";
                    lostpassword = "https://localhost:44375/Login/ForgotPassword";
                    iProdData = "https://iproddeveloperapi.azurewebsites.net/iProdData/";
                    api = "https://localhost:44375/api/";
                    storageConn = "DefaultEndpointsProtocol=https;AccountName=iproddevelopment;AccountKey=wOadQhvMOfS8vssHCrUSBdHpztaxNpyxiqAIL8yeZUve5aoZTg4p5pPmudS8vaCsGolo38/ZAhhKlflvaW/rHw==;EndpointSuffix=core.windows.net";
                    break;
                default:
                    WebUser = "info@test.iprod.it";
                    WebPassword = "1234";
                    NameOfUser = "AMEDEO BRUNI";
                    TenantName = "iProd System SRL";
                    home = "https://app.iprod.it";
                    login = "https://app.iprod.it/Login/UserLogin/?ReturnUrl=%2F";
                    register = "https://app.iprod.it/Login/NewRegistration";
                    lostpassword = "https://app.iprod.it/Login/ForgotPassword";
                    iProdData = "https://iprodapiv3.azurewebsites.net/iProdData/";
                    api = "https://app.iprod.it/api/";
                    //             api = "https://localhost:5001/api/";
                    dbConnectionString = "mongodb+srv://iproddbuser:ojT7As66QgivDd2T@iprodcluster-prva6.azure.mongodb.net/test?retryWrites=true";
                    storageConn = "DefaultEndpointsProtocol=https;AccountName=iprodv1;AccountKey=HToDsHkHk0wSOFKylK1kw2eBR01dW0zcl8D/k6LsGJmpMuzbfbzKl/608WQS5VEzZx66pOwjkypAxDwRRGX+hQ==;EndpointSuffix=core.windows.net";
                    break;
            }

            // inizializzazione url per richieste ai servizi di backup di iprod
            svcTOKEN = "z%C*F-JaNcRfUjXn2r5u8x/A?D(G+KbPeSgVkYp3s6v9y$B&E)H@McQfTjWmZq4t7w!z%C*F-JaNdRgUkXp2s5u8x/A?D(G+KbPeShVmYq3t6w9y$B&E)H@McQfTjWnZr4u7x!A%C*F-JaNdRgUkXp2s5v8y/B?E(G+KbPeShVmYq3t6w9z$C&F)J@McQfTjWnZr4u7x!A%D*G-KaPdRgUkXp2s5v8y/B?E(H+MbQeThVmYq3t6w9z$C&F)J@NcR";
            svcGetBackups = "/Service/GetTenantBackups";
            svcBusy = "/Service/IsSystemOperationInProgress";
            svcSetSyncOp = "/Service/SetSystemOperation?operation=";
            svcGetSystemTimestamp = "/Service/GetSystemTimestamp";
            svcDoBackup = "/Service/TenantBackup?backupname=";
            svcSendEventLog = "/Service/SendEventLog";


            // init parametri di tolleranza tempi e scadenza backups
            // tolleranza differenza date server/client, default 15 secondi
            string ft = ConfigurationManager.AppSettings["TimeStampFaulTolerance"];
            if (UT.IsNull(ft)) ft = "0:0:15";

            var ar = ft.Split(':');
            if (ar.Length != 3)
            {
                ft = "0:0:15";
                ar = ft.Split(':');
            }

            int h = Convert.ToInt32(ar[0]);
            int m = Convert.ToInt32(ar[1]);
            int s = Convert.ToInt32(ar[2]);

            svcTimeFaultTolerance = new TimeSpan(h, m, s);


            // validita backup
            // tempo in cui considerare un backp valido e quindi non va rifatto (default 24 ore)
            ft = ConfigurationManager.AppSettings["BackupValidityTime"];
            if (UT.IsNull(ft)) ft = "24:0:0";

            ar = ft.Split(':');
            if (ar.Length != 3)
            {
                ft = "24:0:0";
                ar = ft.Split(':');
            }

            h = Convert.ToInt32(ar[0]);
            m = Convert.ToInt32(ar[1]);
            s = Convert.ToInt32(ar[2]);

            svcBackupValidityTime = new TimeSpan(h, m, s);


        }

        /// <summary>
        ///  Get accoppiata dei tokens di accesso ai servizi di backup su iProd
        /// </summary>
        /// <returns></returns>
        public (string, string) SvcGetTokens()
        {
            var tokens = (svcTOKEN, Program.ipTOKEN);
            return tokens;
        }

        /// <summary>
        /// calculated è la differnza dalla utcDate di iProd e la utcDate locale. se il tempo da configurazione (appconfig) è inferiore, la tolleranza è accettabile
        /// </summary>
        /// <param name="calculated"></param>
        /// <returns></returns>
        public bool isValidUtcTolerance(TimeSpan calculated)
        {
            return svcTimeFaultTolerance <= calculated;

        }

        /// <summary>
        /// calculated è la differnza dalla utcDate dell'ultimo backup fatto e la utcDate locale. se il tempo da configurazione (appconfig) è maggiore, il backup è valido, non va rifatto
        /// </summary>
        /// <param name="calculated"></param>
        /// <returns></returns>
        public bool isValidBackupTime(TimeSpan calculated)
        {
            return svcBackupValidityTime >= calculated;
        }
    }

    public class AzureOptions
    {
        public AzureOptions()
        {
            StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=iproddevelopment;AccountKey=wOadQhvMOfS8vssHCrUSBdHpztaxNpyxiqAIL8yeZUve5aoZTg4p5pPmudS8vaCsGolo38/ZAhhKlflvaW/rHw==;EndpointSuffix=core.windows.net";
            //  "DefaultEndpointsProtocol=https;AccountName=iprodv1;AccountKey=HToDsHkHk0wSOFKylK1kw2eBR01dW0zcl8D/k6LsGJmpMuzbfbzKl/608WQS5VEzZx66pOwjkypAxDwRRGX+hQ==;EndpointSuffix=core.windows.net";
        }

        public string StorageConnectionString { get; set; }
    }


    public class WordyFormatProvider : IFormatProvider, ICustomFormatter
    {

        /* 
         *
        //    esempio:

            double n = -123.45;
            IFormatProvider fp = new WordyFormatProvider();
            Console.WriteLine (string.Format (fp, "{0:C} in words is {0:W}", n));

            // -$123.45 in words is minus one two three point four five

        int i = "0123456789-.".IndexOf(digit);

        */




        static readonly string[] _numberWords =
        "zero one two three four five six seven eight nine minus point".Split();
        IFormatProvider _parent; // Allows consumers to chain format providers
        public WordyFormatProvider() : this(CultureInfo.CurrentCulture) { }
        public WordyFormatProvider(IFormatProvider parent)
        {
            _parent = parent;
        }

        public object GetFormat(Type formatType)
        {
            if (formatType == typeof(ICustomFormatter)) return this;
            return null;
        }
        public string Format(string format, object arg, IFormatProvider prov)
        {
            // If it's not our format string, defer to the parent provider:
            if (arg == null || format != "W")
                return string.Format(_parent, "{0:" + format + "}", arg);
            StringBuilder result = new StringBuilder();
            string digitList = string.Format(CultureInfo.InvariantCulture, "{0}", arg);
            foreach (char digit in digitList)
            {
                int i = "0123456789-.".IndexOf(digit);
                if (i == -1) continue;
                if (result.Length > 0) result.Append(' ');
                result.Append(_numberWords[i]);
            }
            return result.ToString();
        }
    }


    public static class BitmapExtensions
    {
        public static void SaveJPG100(this Bitmap bmp, string filename)
        {
            EncoderParameters encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
            bmp.Save(filename, GetEncoder(ImageFormat.Jpeg), encoderParameters);
            bmp.Dispose();
        }

        public static void SaveJPG100(this Bitmap bmp, Stream stream)
        {
            EncoderParameters encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
            bmp.Save(stream, GetEncoder(ImageFormat.Jpeg), encoderParameters);
        }

        public static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }

            return null;
        }
    }

    public class FieldDescriptor
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public string ObjName { get; set; }
        public object Value { get; set; }
        public bool Enabled { get; set; }
        public List<List<FieldDescriptor>> extrn { get; set; }
        public List<string> ListIDS { get; set; }


        public FieldDescriptor()
        {
            extrn = new List<List<FieldDescriptor>>();
            Enabled = true;
        }

        public override string ToString()
        {
            return $"{Name}  : {Value}";
        }

        public List<string> ResolveExtrn()
        {

            int rk = 0;
            var ls = new List<string>();
            if (extrn.Count < 1) return ls;

            string st = "";

            var first = true;
            foreach (var multi in extrn)
            {
                foreach (var f2 in multi)
                {
                    if (f2.Name == "*item*")
                    {
                        var ar = f2.DataType.Split('|');

                        if (first)
                        {
                            st = $"?H|{f2.Value}|0|<tr>"; first = false;
                        }
                        st += $"<th>{ar[1]}</th>";  // nome field
                    }
                    else
                        st += $"<th>{f2.Name}</th>";
                }
            }

            st += "</tr>";
            ls.Add(st);
            first = true;
            foreach (var f1 in extrn)
            {
                rk++;
                st = "";
                first = true;
                foreach (var f2 in f1)
                {
                    if (f2.Name == "*item*") // per precauzione se si usa per altro ma sono tutti cosi
                    {
                        var ar = f2.DataType.Split('|');
                        if (first)
                        {
                            st = $"?D|{f2.Value}|{rk}|<tr>"; first = false;
                        }
                        st += $"<td>{ar[2]}</td>";  // value
                    }
                    else
                        st += $"<td>{f2.Value}</td>";
                }
                st += "</tr>";
                ls.Add(st);
            }
            return ls;
        }

        public FieldDescriptor(string n, string t, object v)
        {
            Name = n;
            DataType = t;
            Value = v;

            extrn = new List<List<FieldDescriptor>>();
            Enabled = true;


        }
    }


    public class httpErr
    {
        public string title { get; set; }
        public int status { get; set; }
    }

    public class httpResponse
    {
        public HttpResponseMessage content { get; set; }
        public string response { get; set; }
        public string status { get; set; }
        public HttpStatusCode statusCode
        {
            get; set;

        }

    }


    public class RowHist
    {

        public int idx { get; set; }
        public string Tipo { get; set; }
        public DateTime Data { get; set; }
        public string Dex { get; set; }


        public RowHist(string st)
        {
            try
            {
                var ar = st.Split(';');

                DateTime.TryParse(ar[2], out DateTime d);

                idx = UT.ToInt(ar[0]);
                Tipo = ar[1];
                Data = d;
                Dex = ar[3];
            }
            catch { }
        }

    }

}
