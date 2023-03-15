using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using UT = iProdWHSE.utility;
using AH = iProdWHSE.AgentHelper;
using System.Windows.Forms;
using System.Threading;
using System.Configuration;
using System.Net.Http;
using iProdDataModel.Models;
using System.ServiceModel;
using System.Net.Sockets;
using System.Windows;
using System.Net.Mail;
using Newtonsoft.Json;
using System.Security.Policy;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Collections;
//using Newtonsoft.Json;

namespace iProdWHSE
{
    public static class AgentHelper
    {
        public static bool shutDownListenerRequest { get; set; }


        public static HttpClient WSClient { get; set; }
        public static myNameSpace.ComPortTypeClient WSSoapClient { get; set; }

        public static string BindingEndPoint { get; set; }  // ip magazzino verticale e url del servizio SOAP
        public static string ListenerEndPoint { get; set; }
        public static string inputXml { get; set; }

        public static bool Slow { get; set; }

        private static string _resp = "";
        public static bool Processing { get; set; } // richiesta inviata in attesa di risposta
        public static bool Ready { get; set; }  // true se il response xml è arrivato 
        public static bool Errors { get; set; } // true se ci sono errori
        public static string ErrorText { get; set; }
        public static DateTime reqTime { get; set; }
        public static reqSchema Request { get; set; }
        public static Form1 Form1 { get; set; }

        public static string OutputResponse
        {
            get
            {
                Ready = false;
                return _resp;
            }
        }


        public static bool httpListenerSupported()
        {
            return HttpListener.IsSupported;
        }



        public static async Task<HttpClient> createHttpClient(string url, string usr, string pwd)
        {
            //setup reusable http client
            HttpClient client = new HttpClient();
            try
            {

                Uri baseUri = new Uri(url);
                client.BaseAddress = baseUri;
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.ConnectionClose = true;
                client.Timeout = new TimeSpan(0, 0, 15);

                //Post body content
                var values = new List<KeyValuePair<string, string>>();
                values.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
                var content = new FormUrlEncodedContent(values);

                var authenticationString = $"{usr}:{pwd}";
                var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(authenticationString));

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/oauth2/token");
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
                requestMessage.Content = content;

                //make the request
                var response = await client.SendAsync(requestMessage);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                log($"createHttpClient('{url}','{usr}','{pwd}') ret: {responseBody}");

              //  log(responseBody);
            }
            catch (Exception ex)
            {
                log($"createHttpClient('{url}','{usr}','{pwd}') ERR: {ex.Message}");
                throw ex;
            }

            return client;

        }

        /// <summary>
        /// Stabilisce la connessione con il Service SOAP o REST
        /// </summary>
        /// <returns></returns>
        public static bool ConnectToWS()
        {
            if (UT.MockWS)
            { 
                UT.WSConnected = true;
                return true;
            }

            // ping (via socket)
            if (!CheckHostAlive(UT.iProdCFG.MP_IP, UT.ToInt(UT.iProdCFG.MP_Port))) return false;


            // se l'host è UP, tenta di connettersi al servizio

            
            if (UT.curMV.Technology == "REST")
            {
                try
                {
                    log("..connessione a WS REST in corso.. ");

                    //WSClient = createHttpClient(BindingEndPoint, "iotuser", "iotuser").Result;


                    if(WSClient != null)
                    {
                        WSClient.DefaultRequestHeaders.Clear();
                        WSClient.DefaultRequestHeaders.ConnectionClose = true;
                    }

                    WSClient = new HttpClient();
                    WSClient.DefaultRequestHeaders.Clear();
                    WSClient.DefaultRequestHeaders.ConnectionClose = true;
                    WSClient.Timeout = new TimeSpan(0, 0, 15);
                    WSClient.BaseAddress = new Uri(BindingEndPoint);
                    WSClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.ASCIIEncoding.UTF8.GetBytes("iotuser:iotuser")));
                    log($"ConnectToWS->WSClient('{BindingEndPoint}'");
                 
                    UT.WSConnected = true;
                }
                catch (Exception ex)
                {
                    UT.MsgBox(log(ex.Message),"Connessione a WS REST fallita");
                }
            }
            else if (UT.curMV.Technology == "SOAP")
            {
                log("..connessione a WS SOAP in corso.. ");
                WSSoapClient = new myNameSpace.ComPortTypeClient("ComHttpsSoap11Endpoint", BindingEndPoint);

                UT.WSConnected = WSSoapClient.State != CommunicationState.Faulted;

            }

            if (UT.WSConnected)
                log("ONLINE");
            else
                log("OFFLINE");

            return UT.WSConnected;
        }


        /// <summary>
        /// PING via socked di un IP + porta
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static bool CheckHostAlive(string host, int port = 80)
        {

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var result = socket.BeginConnect(host, port, null, null);

            // test the connection for 3 seconds
            bool success = result.AsyncWaitHandle.WaitOne(3000, false);

            var resturnVal = socket.Connected;
            if (socket.Connected)
                socket.Disconnect(true);

            socket.Dispose();
            log($"CheckHostAlive('{host}','{port}') ret: {resturnVal}");
            return resturnVal;

        }


        private static string log(string m, bool forza=true)
        {
            if (m.Contains("-QUERY")) return m;
            // standard log, semplifica la codifica
            UT.WriteToEventLog(Form1, m, forza);
            Application.DoEvents();
            return m;
        }


        #region NON USATI AL MOMENTO

        public static bool initWS(string url, bool forzaChiusuraRisorsa = false)
        {

           


            // ComHttpsSoap11Endpoint 
            // var pp =  ComSoap11Binding;

            WSSoapClient = new myNameSpace.ComPortTypeClient("ComSoap11Binding",url);
            



            if (Request is null) return false;

            if (Request.Offline) return true;

            if (forzaChiusuraRisorsa)
            {
                Processing = false;
                Ready = false;
            }



            if (UT.IsNull(url))
            {
                ErrorText = "Url non valido";
                Errors = true;
                return false;
            }



            if (Processing)
            {
                ErrorText = $"Tentativo di inizializzare il WS mentre esiste una richiesta pendente. Chiudere o rilasciare la risorsa. In attesa da {UT.ElapsedTimeToString(DateTime.Now - reqTime)} ";
                Errors = true;
                return false;
            }

            //if (1 == 1) // quando conosceremo l'url questo sparira
            //{
            //    ErrorText = "WS offline, impossibile verificare la connessione";
            //    Errors = true;
            //    return false;
            //}

            //if (1 == 2) // esito del test di connessione
            //{
            //    ErrorText = "Connessione non riuscita: richiesta scaduta";
            //    Errors = true;
            //    return false;
            //}


            ListenerEndPoint = url;
            Errors = false;
            ErrorText = "";
            inputXml = "";
            _resp = "";


            


            return true;
        }
 


        internal static void ExecuteLocalWS()
        {
            UT.IOLog("run ExecuteLocalWS()");

            var rq = Request;

            if (UT.IsNull(rq.sampleRespFile))
            {
                UT.IOLog("Lo schema corrente non ha responses locali di test");
                return;
            }


            if (!File.Exists(rq.sampleRespFile))
            {
                UT.IOLog($"file {rq.sampleRespFile} non trovato.");
                return;
            }

            UT.IOLog($"Parsing sample response xml per {rq.dex}, file {rq.sampleRespFile}");

            var lines = UT.LoadTextFile(rq.sampleRespFile);

            UT.IOLog($"Simulata risposta http con {lines.Count} elementi di tipo {rq.responseDefinition.Name}:");
            _resp = string.Join("", lines.ToArray());


            // qui abbiamo finito di interpretare il file di output di simulazione response e lo rendiamo disponibile 
            // su OutputResponse (_resp) come se fosse stato generato dall'http

        }

        public static void OutputToLog(List<recordSchema> Objects)
        {

            UT.IOLog("CSV:");
            int i = 0;
            string st = "#;";
            if (Objects.Count > 0)
            {
                foreach (var f in Objects[0].Fields)
                    st += f.Name + ";";

                UT.IOLog(st);
            }



            foreach (var rk in Objects)
            {
                i++;

                st = $"{i};";
                foreach (var f in rk.Fields)
                    st += f.Value + ";";
                UT.IOLog(st);
            }

        }
        /// <summary>
        /// entry point. assegna inputXml con la richiesta e passa a questa chiamata il metodo da eseguire nell'xml
        /// </summary>
        /// <param name="cmd">nome tag del body nell'xml</param>
        public static void CallWebService(string cmd)
        {
            try
            {
                Ready = false;
                Errors = false;
                ErrorText = "";

                if (Request.Offline || !UT.WSConnected)
                {
                    ExecuteLocalWS();
                    return;
                }

                if (UT.IsNull(ListenerEndPoint))
                {
                    Errors = true;
                    ErrorText = "Url del webservice mancante";
                    return;
                }

                if (UT.IsNull(inputXml))
                {
                    Errors = true;
                    ErrorText = "parametro inputXml assente. Assegnare l'xml della richiesta e riprovare";
                    return;
                }

                if (UT.IsNull(cmd))
                {
                    Errors = true;
                    ErrorText = "parametro metodo assente. chiamare la funzione CallWebService passandogli la procedura contenuta nella richiesta xml in main:xxxx. Es: CallWebService(\"sendJobsReqV01\") per la lista prelievi";
                    return;
                }


                Processing = true;
                reqTime = DateTime.Now;

                var _url = ListenerEndPoint; //  "http://xxxxxxxxx/Service1.asmx";
                var _action = $"{ListenerEndPoint}?op={cmd}";  // "http://xxxxxxxx/Service1.asmx?op=HelloWorld";

                XmlDocument soapEnvelopeXml = CreateSoapEnvelope(inputXml);
                log("Esegue CreateWebRequest:" + inputXml);
                HttpWebRequest webRequest = CreateWebRequest(_url, _action);
                log("Esegue InsertSoapEnvelopeIntoWebRequest");
                InsertSoapEnvelopeIntoWebRequest(soapEnvelopeXml, webRequest);

                log("Esegue BeginGetResponse");
                // begin async call to web request.
                IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);

                log("Esegue WaitOne..");
                // suspend this thread until call is complete. You might want to
                // do something usefull here like update your UI.
                asyncResult.AsyncWaitHandle.WaitOne();

                // get the response from the completed web request.
                string soapResult;
                log("Esegue EndGetResponse");
                using (WebResponse webResponse = webRequest.EndGetResponse(asyncResult))
                {
                    using (StreamReader rd = new StreamReader(webResponse.GetResponseStream()))
                    {
                        log("Esegue ReadToEnd");
                        soapResult = rd.ReadToEnd();
                    }

                    _resp = soapResult; // <---------- qui ha finito e mette il response su OutputResponse

                    log(soapResult);
                    Processing = false;
                    Ready = true;
                    Errors = false;
                    ErrorText = "";
                }
            }
            catch (Exception ex)
            {
                log($"Eccezione: {ex.Message.Replace("\r\n", "")}, Stack: {ex.StackTrace.Replace("\r\n", "")}");
            }
        }

        private static HttpWebRequest CreateWebRequest(string url, string action)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Headers.Add("SOAPAction", action);
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
            return webRequest;
        }

        private static XmlDocument CreateSoapEnvelope(string xml)
        {
            XmlDocument soapEnvelopeDocument = new XmlDocument();
            soapEnvelopeDocument.LoadXml(xml);
            //        @"<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"" 
            //           xmlns:xsi=""http://www.w3.org/1999/XMLSchema-instance"" 
            //           xmlns:xsd=""http://www.w3.org/1999/XMLSchema"">
            //    <SOAP-ENV:Body>
            //        <HelloWorld xmlns=""http://tempuri.org/"" 
            //            SOAP-ENV:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
            //            <int1 xsi:type=""xsd:integer"">12</int1>
            //            <int2 xsi:type=""xsd:integer"">32</int2>
            //        </HelloWorld>
            //    </SOAP-ENV:Body>
            //</SOAP-ENV:Envelope>");
            return soapEnvelopeDocument;
        }

        private static void InsertSoapEnvelopeIntoWebRequest(XmlDocument soapEnvelopeXml, HttpWebRequest webRequest)
        {
            using (Stream stream = webRequest.GetRequestStream())
            {
                soapEnvelopeXml.Save(stream);
            }
        }


        public static List<recordSchema> GetXMLResponseObject(recordSchema def, string xml)
        {
            var ret = new List<recordSchema>();

            var doc = XDocument.Parse(xml);


            if (def.Name == "article")
            {
                int n = def.Fields.Count;
                ret = doc.Descendants().Where(x => x.Name.LocalName == def.Name).Select(x => new recordSchema()
                {
                    Name = def.Name,
                    Fields = new List<fieldSchema>
                { new fieldSchema(def.Fields[0].Name, (string)x.Element(x.Name.Namespace + def.Fields[0].Name)),
                  new fieldSchema(def.Fields[1].Name, (string)x.Element(x.Name.Namespace + def.Fields[1].Name)),
                  new fieldSchema(def.Fields[2].Name, (string)x.Element(x.Name.Namespace + def.Fields[2].Name)),
                  new fieldSchema(def.Fields[3].Name, (string)x.Element(x.Name.Namespace + def.Fields[3].Name)),
                  new fieldSchema(def.Fields[4].Name, (string)x.Element(x.Name.Namespace + def.Fields[4].Name)),
                  new fieldSchema(def.Fields[5].Name, (string)x.Element(x.Name.Namespace + def.Fields[5].Name)),
                  new fieldSchema(def.Fields[6].Name, (string)x.Element(x.Name.Namespace + def.Fields[6].Name)),
                  new fieldSchema(def.Fields[7].Name, (string)x.Element(x.Name.Namespace + def.Fields[7].Name)),
                  new fieldSchema(def.Fields[8].Name, (string)x.Element(x.Name.Namespace + def.Fields[8].Name)),
                  new fieldSchema(def.Fields[9].Name, (string)x.Element(x.Name.Namespace + def.Fields[9].Name))
                }
                }).ToList();
            }

            return ret;
        }

    }
    #endregion


    #region Schema (old ma in uso)

    public class fieldSchema
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public List<fieldSchema> Childs { get; set; }

        public fieldSchema(string n, string v)
        {
            Name = n;
            Value = v;
        }

    }

    public class recordSchema
    {
        public string Name { get; set; }
        public string NameSpace { get; set; }

        public List<fieldSchema> Fields { get; set; }

        public recordSchema()
        {
            Fields = new List<fieldSchema>();
        }
        public recordSchema(string n)
        {
            Name = n;
            NameSpace = "xsd:"; // default per le richieste (i response invece non li hanno mai
            Fields = new List<fieldSchema>();
        }


        /// <summary>
        /// genera l'xml di un elemento composto da piu campi
        /// </summary>
        /// <returns>stringa xml in formato Envelope SOAP</returns>
        public string getXML()
        {
            string ret = "";
            string tagOpen = $"<{NameSpace}{Name}>";
            string tagClose = $"</{NameSpace}{Name}>";

            ret = tagOpen;

            foreach (var fld in Fields)
            {
                ret += $"<{NameSpace}{fld.Name}>{fld.Value}</{NameSpace}{fld.Name}>";
            }


            ret += tagClose;

            return ret;

        }
    }

    public class itemToGet
    {
        public string itemid { get; set; }
        public string name { get; set; }
        public double qty { get; set; }
    }

    public class reqSchema
    {
        public string IDPhaseInstance { get; set; }

        public bool Offline { get; set; }
        public string dex { get; set; }  // va sul dropdown a video
        public string mainName { get; set; }
        public bool isPrelievo { get; set; }
        public string pickUpJobName { get; set; }
        public string baseTemplate { get; set; }
        public string sampleRespFile { get; set; }
        public List<recordSchema> records { get; set; }
        public recordSchema responseDefinition { get; set; }

        public reqSchema()
        {
            records = new List<recordSchema>();
            responseDefinition = new recordSchema();
        }







        public reqSchema(string n, string progname, string responseFileName)
        {
            dex = n;
            mainName = progname;
            sampleRespFile = responseFileName;
            records = new List<recordSchema>();
            responseDefinition = new recordSchema();
            baseTemplate = "schema_base.txt";
            initDataResponse();
        }


        void initDataResponse()
        {
            var r = responseDefinition;

            r.Name = "article";
            r.NameSpace = "";   // nel responses non ci sono namespaces soap
            r.Fields.Add(new fieldSchema("articleNumber", "string"));                // COD ARTICOLO
            r.Fields.Add(new fieldSchema("articleName", "string"));                 // DESCRIZIONE
            r.Fields.Add(new fieldSchema("liftNumber", "string"));                  // NUMERO DI MAGAZZINO
            r.Fields.Add(new fieldSchema("shelfNumber", "string"));                 // NUMERO DI CASSETTO
            r.Fields.Add(new fieldSchema("compartmentNumber", "string"));           // POSIZIONE X
            r.Fields.Add(new fieldSchema("compartmentDepthNumber", "string"));      // POSIZIONE Y
            r.Fields.Add(new fieldSchema("containerSize", "string"));               // DIMENSIONE
            r.Fields.Add(new fieldSchema("fifo", "string"));                        // VALORIZZAZIONE FIFO
            r.Fields.Add(new fieldSchema("inventoryAtStorageLocation", "string"));  // QUANTITA’
            r.Fields.Add(new fieldSchema("minimumInventory", "string"));            // QUANTITA’ MINIMA



        }

        public string getXML(string basePath)
        {
            string ret = "";

            if (isPrelievo)
                return genPrelievo(basePath);

            // genera l'xml su schema base da filename in basetemplate


            var tpl = basePath + "\\xmlTemplates";
            string tpl1 = tpl + "\\" + baseTemplate;     // il template (riprodotto nei commenti sottostanti)

            var righe = UT.LoadTextFile(tpl1);
            foreach (var row in righe)
            {
                string line = row;
                if (line.Contains("%method_name%"))
                {
                    ret += line.Replace("%method_name%", mainName);
                }
                else if (line.Contains("%body%"))
                {
                    ret += line.Replace("%body%", getXmlRecords());     // genera l'xml di ogni record
                }
                else
                    ret += line;
            }


            return ret;

            /*

            il template reale che usa. lo prende da disco per praticità ma potrebbe essere hardcodato non cambia nulla

            <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" 
            xmlns:main="http://main.jws.com.hanel.de" xmlns:xsd="http://main.jws.com.hanel.de/xsd">
             <soapenv:Header/>
             <soapenv:Body>
                 <main:%method_name%>
                     <main:param>
                            %body%
                     </main:param>
                 </main:%method_name%>
             </soapenv:Body>
            </soapenv:Envelope>



            */
        }

        public string genPrelievo(string basePath)
        {
            string ret = "";

            var tpl = basePath + "\\xmlTemplates";
            string tpl1 = tpl + "\\schema_base_lista_prelievi.txt";     // il template (riprodotto nei commenti sottostanti)


            string xml = "<xsd:job>";
            xml += $"<xsd:jobNumber>{pickUpJobName}</xsd:jobNumber>";  // qui viene generato un job per ogni phaseinstance da prelevare
            xml += getXmlRecords();                                    // e poi risolve tutti i JobPosition per ogni item che compone l'item da produrre 
            xml += "</xsd:job>";


            // la base del template contiene poche righe fisse
            var righe = UT.LoadTextFile(tpl1);
            foreach (var row in righe)
            {
                string line = row;
                if (line.Contains("%jobs%"))
                    ret += line.Replace("%jobs%", xml);     // e il rigo %jobs% viene sostituito con tutto l'xml ottenuto prima
                else
                    ret += line;
            }

            return ret;

            /*
                il template reale che usa. lo prende da disco per praticità ma potrebbe essere hardcodato non cambia nulla

                <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" 
                xmlns:main="http://main.jws.com.hanel.de" xmlns:xsd="http://main.jws.com.hanel.de/xsd">
                 <soapenv:Header/>
                 <soapenv:Body>
                     <main:sendJobsReqV01>
                         <main:param>
	                        %jobs%
                         </main:param>
                     </main:sendJobsReqV01>
                 </soapenv:Body>
                </soapenv:Envelope>


            */


        }


        private string getXmlRecords()
        {
            string ret = "";

            foreach (var rk in records)
            {
                ret += rk.getXML();
            }

            return ret;
        }

    }

    #endregion

    [Serializable]
    public class RESTReservation
    {
        public string articleNumber { get; set; }
        public string userIdentifier { get; set; }
        public string actionType { get; set; }
        public string quantity { get; set; }

    }


    public class RESTStock
    {
        public string articleNumber { get; set; }
        public int availableQuantity { get; set; }
        public int criticalQuantity { get; set; }
        public int currentQuantity { get; set; }
        public int idArticle { get; set; }
        public int maxQuantity { get; set; }
        public int reorderQuantity { get; set; }
        public int reservedQuantity { get; set; }
        public int warningQuantity { get; set; }

    }


    public class warehousedata
    {
        public List<RESTStock> locationarticle { get; set; }
    }

    /// <summary>
    /// Oggetto che sta tra la richiesta in arrivo dai tablet e il response che gli dobbiamo restituire. Usato anche per lo stato del Ping MP
    /// </summary>
    public class ipDispatcher
    {
        public string Action { get; set; } = "";             // tipo di querystring "PHASEINSTANCE", "PING"
        public string Requester { get; set; }           // macchinario che ha fatto la richiesta
        public string StatusCode { get; set; }          // PING su MP: OFFLINE, NORESPONSE, RESPONSEINERROR, MUTE, ONLINE, ABORTED
        public string Message { get; set; }
        public string AdditionalInfo { get; set; }
        public string IdPhaseInstance { get; set; }
        public bool inError { get; set; }
        public int PickedItems { get; set; }

        public ipDispatcher()
        {
            Requester = "Anonimo";
        }

        public void sendResponse(HttpListenerContext context)
        {
            HttpListenerResponse response = context.Response;

            // Construct a response.

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            string responseString = json;                                               // "<HTML><BODY> Hello world!</BODY></HTML>";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            // Get a response stream and write the response to it.
            response.ContentLength64 = buffer.Length;
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);                     // <---- qui invia il response con lo stato del MP a chi ha fatto la richiesta
                                                                        // You must close the output stream.
            output.Close();
        }

    }



    /// <summary>
    ///  LISTENER che processa le richieste in arrivo dai tablet
    /// </summary>
    public class ThreadPicker : IDisposable
    {

        public static string curBom { get; set; }
        public static string outCSV { get; set; }
        public string EndPoint { get; set; }

        #region Private Properties


        public bool verbose { get; set; }

        private List<reqSchema> Requests { get; set; }
        private bool disposedValue;
        private string logFile { get; set; }
        private string cmdFile { get; set; }
        private int logCnt { get; set; }

        #endregion

        #region iProd Objects



        // collections iProd

         

        public iProdCustomers iproduser { get; set; } // viene caricato una volta sola all'inizio e resta per tutta la sessione degli imports 

        public List<Warehouse> warehouses { get; set; }
        public List<Customers> iprod_customers { get; set; }
        public List<Items> iprod_items { get; set; }
        public List<Customermachine> iprod_machines { get; set; }
        public List<Phase> iprod_phases { get; set; }
        public List<Posts> iprod_posts { get; set; }
        public List<Bom> iprod_boms { get; set; }
        public List<PhaseInstance> iprod_pi { get; set; }
        public Dictionary<string, string> categories { get; set; }

        public List<OrderedItems> soordereritems { get; set; }
        public List<SalesOrder> iprod_solist { get; set; }
        public OrderedItems iprod_orderitem { get; set; }

        System.Net.HttpStatusCode isOk = System.Net.HttpStatusCode.OK;

        public string userguid = "";
        public string NameSurname = "";
        public bool inprogress = false;
        public bool doingtask = false;
        public string processStatus = "idle";
        public bool forzaStop = false;
        public string tipoPost = "1";  // in iprod_load("posts") specifica il tipo di post da scaricare
        HttpResponseMessage content;
        string dataparameter;
        string customerdata;

  
        #endregion


        #region 1 COSTRUTTORE 


        public ThreadPicker()
        {

           // ListenerEndPoint = $"http://{UT.iProdCFG.MP_IP}:{UT.iProdCFG.MP_Port}/";
            EndPoint = $"http://{UT.iProdCFG.LocalIP}:8080/";  // non va, quando è sistemato va tolto il localhost
            EndPoint = $"http://127.0.0.1:8098/";  

            if (!UT.iProdConnected)
            {
                log("Connessione a iProd mancante. Impossibile avviare servizi in questo momento,");
                return;
            }


          //  var t = EseguiPingMP();
            // if(t.StatusCode != "OK")
            //if(!CheckConnection())
            //{
            //    log("Connessione al magazzino verticale assente. Impossibile avviare servizi in questo momento.");
            //    return;
            //}

         //   UT.WSConnected = true;


            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };


            loadDataObjects();

            logFile = UT.pathLog + "\\listenerLog.txt";
            if (File.Exists(logFile)) File.Delete(logFile);
            cmdFile = UT.pathLog + "\\listenerNotifyer.txt";
            if (File.Exists(cmdFile)) File.Delete(cmdFile);


    

           // log("Il Thread parallelo per la lista prelievi è stato avviato regolarmente");

       

            UT.AppendToFile(logFile, $"LSNR Listener avviato {DateTime.Now}");

            log($"LSNR httpListener in ascolto sulla porta 8098");

            httpListener();  // <------------ AVVIA IL LISTENER

        }

        #endregion


        #region Listener handler


        /// <summary>
        /// Prende la QueryString proveniente dalla richiesta, contiene l'id della phaseinstance da prelevare a magazzino
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private ipDispatcher getBodyfromClient(HttpListenerRequest request)
        {


           
            var cnt = request.QueryString.Count;

            var ret = new ipDispatcher();
            if (cnt == 0)
            {
                log($"Ricevuta richiesta senza parametri: {request.Url}");
                ret.Action = "missing-parms";
                return ret;
            }

            var keys = request.QueryString.AllKeys.ToList();
            if (keys.Contains("action"))
            {
                ret.Action = request.QueryString.Get("action");

                if (ret.Action == "picker")
                {
                    if (keys.Contains("id"))
                        ret.IdPhaseInstance = request.QueryString.Get("id");
                    else
                        ret.Action = "missing-id";
                }
                else if (ret.Action == "ping")
                {
                    // nothing to do
                }
                else
                    ret.Action = "missing-parms";
            }

            if (keys.Contains("sender"))
            {
                var nome = request.QueryString.Get("sender").ToString();
                if (nome == "Biglia B750") nome = "BigliaB750";
                log($"Ricevuta richiesta da {nome}");
                ret.Requester = nome.Replace(" ", "-").Replace("?", "-").Replace(";", "-").Replace("\"", "-");
               
            }


            ret.inError = ret.Action.StartsWith("missing");
            if (ret.inError) ret.Message = "La richiesta ricevuta era incompleta, manca il parametro id o action";

            return ret;

        }


        bool listenerRunning = false;

        public void httpListener()
        {

            if (listenerRunning) return;

            listenerRunning = true;

            if (!httpListenerSupported())
            {
                log("LSNR Errore Listener: Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return;
            }

            string host = EndPoint; // ConfigurationManager.AppSettings["ListenerURI"];

            // Create a listener.


            log("LSNR   Richieste ammesse:");
            log($"LSNR   http://{UT.iProdCFG.LocalIP}:8098?action=picker&id=<phaseinstanceid>&sender=<nome macchinario>");
            log($"LSNR   http://{UT.iProdCFG.LocalIP}:8098?action=ping&sender=<nome macchinario>");


            bool skipPing = false;
            int cnt = 0;
            do
            {
                UT.ListenerUP = false;


                if (AH.shutDownListenerRequest) return;
                
                if(!skipPing)
                    cnt++;

                using (var listener = new HttpListener())
                {
                    listener.Prefixes.Add("http://*:8098/");

                    try
                    {
                        listener.Start();
                    }
                    catch (Exception ex)
                    {
                        var sm = log($"LSNR  Attenzione, l'url utilizzato per il servizio di listener richieste non è valido: {host}. Message: '{ex.Message}'");
                        SendAction("STOP LSTRN");
                        System.Threading.Thread.Sleep(3000);
                        return;
                    }
                    if (!skipPing)
                    {
                        SendAction("START LSTRN");
                        log($"LSNR  Listener in attesa richiesta da tablet. Pool #{cnt} ...");
                        UT.CompactMemory();
                    }

                    UT.ListenerUP = true; 

                    // ==========================================================================================

                    // Note: The GetContext method blocks while waiting for a request.
                    HttpListenerContext context = listener.GetContext();   // <-- si ferma qui in attesa di richieste, avanza alla prima che arriva

                    // ==========================================================================================

                    if(!UT.iProdConnected)
                    {
                        log($"LSNR  ATTENZIONE IPROD DISCONNESSO IMPOSSIBILE ELABORARE RICHIESTE IN QUESTO MOMENTO...");
                        continue;
                    }

                    if (!UT.WSConnected)
                    {
                        log($"LSNR  ATTENZIONE IL DISPOSITIVO M.V NON E' RAGGIUNGIBILE. IMPOSSIBILE ELABORARE RICHIESTE IN QUESTO MOMENTO...");
                        continue;
                    }


                    HttpListenerRequest request = context.Request; 

                    if (AH.shutDownListenerRequest) break;

                    var dsp = getBodyfromClient(request);
                    //skipPing = (dsp.Requester== "BigliaB750" && dsp.Action == "ping");
                    //if (skipPing) continue;


                    string dxa = "";
                    if (dsp.Action == "missing-parms") dxa = " (ATT!: La richiesta ricevuta non contiene alcuni dei parametri essenziali e viene ignorata)";
                    if (dsp.Action == "missing-id") dxa = " (ATT!: La richiesta ricevuta non contiene l'ID e viene ignorata)";

                    
                    log($"MACCH - Ricevuta richiesta n.{cnt} di tipo {dsp.Action} dal macchinario {dsp.Requester}. {dxa}");
                    if (!string.IsNullOrEmpty(dsp.AdditionalInfo))
                        log($"MACCH  additional Info: {dsp.AdditionalInfo}");
                    log(" ");

                    if (dsp.inError)
                        dsp.sendResponse(context);
                    else if (dsp.Action == "ping")
                    {
                        var retPing = EseguiPingMP(dsp.Requester);
                        retPing.sendResponse(context);
                    }
                    else if (dsp.Action == "picker")
                    {
                        log($"LSNR PICK - ..esecuzione prelievo di IdPhaseInstance = '{dsp.IdPhaseInstance}'");
                        var ret = ExecuteProcess("Lista di Prelievo", dsp.IdPhaseInstance);
                        ret.sendResponse(context);
                    }

                    listener.Stop(); // stoppa e a Start() si rimette in ascolto
                    UT.Sleep(3000);
                }
                //Busy = false;
            } while (AH.shutDownListenerRequest == false);

            log("LSNR Richiesto ShutDown listener..");
            listenerRunning = false;

            SendAction("STOP LSTRN");
            UT.Sleep(3000);

        }

        public bool httpListenerSupported() => HttpListener.IsSupported;


        void loadDataObjects()
        {
            Requests = new List<reqSchema>();
            Requests.Add(new reqSchema("Giacenze", "readAllAMDReqV01", "sample_giacenze.txt"));
            Requests.Add(new reqSchema("Lista di Prelievo", "sendJobsReqV01", "sample_lista_prelievi.txt"));
            Requests.Add(new reqSchema("Azzera Lista di prelievo", "deleteJobReqV01", ""));
            Requests.Add(new reqSchema("Stato MP", "MP-status", ""));

        }

        #endregion


        #region ELABORA RICHIESTE


        bool VerboseMax = false;
        public ipDispatcher ExecuteProcess(string taskName, string ObjId = "", string richiedente="")
        {

            var ret = new ipDispatcher();
            string suffix = "PICK -";
            UT.Contatore CC = null;
            VerboseMax = UT.VerboseMax;
            log($"{suffix} Executeprocess('{taskName}')");

           
            try
            {

                // ricarica tutto ad ogni richiesta
                iprod_items = new List<Items>();
                iprod_pi = new List<PhaseInstance>();
                iprod_boms = new List<Bom>();

                setPB(5, 100);

                var rq = Requests.FirstOrDefault(x => x.dex == taskName);
                AH.Request = rq;


                #region caricamento dati da iProd
                
                // ITEMS
                setPB(10, 100);
                CC = load_iProd("items");

                // BOMS
                setPB(15, 100);
                var CB = load_iProd("boms");


                // PHASEINSTANCES
                setPB(30, 100);
                var CPI = load_iProd("phaseinstances");

                #endregion


                // LISTA PRELIEVO
                if (rq.mainName == "sendJobsReqV01")
                    ret = EseguiRichiestaPrelievi(ObjId, richiedente);

                setPB(0);

            }
            catch (Exception ex)
            {
                setPB(0);
                UT.Sleep(1000);
                log("Errore " + ex.Message.Replace("\r\n", "") + ", " + ex.StackTrace.Replace("\r\n", ""));
                AH.shutDownListenerRequest = true;

            }


            return ret;
        }





        /// <summary>
        /// Controlla lo stato di MP richiedendo le giacenze e valuta i vari comportamenti 
        /// </summary>
        /// <returns></returns>
        ipDispatcher EseguiPingMP(string richiedente)
        {
            var ret = new ipDispatcher();
            var retByUser = new ipDispatcher();
            var resp = new myNameSpace.readAllAMDV01Response();

            retByUser.StatusCode = "ABORTED";
            retByUser.Message = "L'utente ha scelto annulla a una conferma task";

            ret.Action = "ping";

            try
            {
                outCSV = $"{UT.pathData}\\Requests\\Ping_MP.txt";
                string sm = log($"Richiesto controllo operatività magazzino. File per le verifiche cronologiche: {outCSV}");
                if (VerboseMax)
                    if (!UT.Ask(sm)) return retByUser;



                log("PING - Connessione a MP..");
                var req = new myNameSpace.readAllAMDV01Request();

                if (UT.MockWS)
                {
                    resp.@return = new myNameSpace.RetReadAllAMDV01();
                    resp.@return.returnValue = 1;
                }
                else
                {


                    if (UT.curMV.Technology == "SOAP")
                    {

                        var cli = AH.WSSoapClient;
                        cli.Open();

                        sm = log("PING - Connesso: Invio Ping..");
                        if (VerboseMax)
                            if (!UT.Ask(sm)) return retByUser;
                        resp = cli.readAllAMDV01(req);
                    }
                    else if (UT.curMV.Technology == "REST")
                    {
                        string APIurl = "system";
                        var ret0 = UT.APICall(AH.WSClient, APIurl).Result;

                        if (ret0.status != "OK")
                        {
                            ret.StatusCode = "RESPONSE-IN-ERROR";
                            sm = log($"BAD REQUEST    ....errore: {ret0.response}.");
                            ret.Message = sm;
                            UT.AddRowHist("PING-ERR", $"Rich da {richiedente}: " + sm);
                            return ret;

                        }
                        else
                        {
                            resp.@return = new myNameSpace.RetReadAllAMDV01();
                            resp.@return.returnValue = 1;

                            sm = log($"MV Operativo e in linea.");
                            UT.AddRowHist("PING-OK", $"Rich da {richiedente}:" + sm);
                            
                            ret.StatusCode = "ONLINE";
                            ret.Message = sm;
                            return ret;
                        }
                    }



                }

                if (resp is null)
                {
                    ret.StatusCode = "MUTE";
                    ret.Message = log("PING - MP raggiungibile in rete ma non risponde alle richieste");
                    UT.AddRowHist("PING-ERR",$"Rich da {richiedente}:" + ret.Message);
                }
                else if (UT.NotNull(resp.@return.returnErrorMessage))
                {
                    ret.StatusCode = "RESPONSEINERROR";
                    ret.Message = log("PING - MP raggiungibile in rete ma alle richieste risponde con errore: " + resp.@return.returnErrorMessage);
                    UT.AddRowHist("PING-ERR", $"Rich da {richiedente}:" + ret.Message);
                }
                else
                {
                    int totArt = 40; // se mock assume questi
                    int nban = 25;
                    if (!UT.MockWS)
                    {
                        var listArt = resp.@return.article;
                        nban = listArt.Select(a => a.shelfNumber).Distinct().Count();
                        totArt = listArt.Length;
                    } 
                    // ok, articoli scaricati

              
                    sm = log($"PING - MP Operativo. In uso {totArt} scomparti e {nban} bancali.");
                    UT.AddRowHist("PING-OK", $"Rich da {richiedente}:" + sm);

                    UT.AppendToFile(outCSV, $"{DateTime.Now} {sm}");
                    if (VerboseMax)
                        if (!UT.Ask(sm)) return retByUser;

                    ret.StatusCode = "ONLINE";
                    ret.Message = sm;

                }

                log("PING");
                return ret;

            }
            catch (Exception ex)
            {

                var sm = log("PING - Errore " + ex.Message.Replace("\r\n", "") + ", " + ex.StackTrace.Replace("\r\n", ""));
                if (VerboseMax)
                    if (!UT.Ask(sm)) return retByUser;

                ret.StatusCode = "OFFLINE";
                ret.Message = sm;
                log("PING - MP Spento");
                UT.AppendToFile(outCSV, $"{DateTime.Now} MP OFFLINE: {sm}");
                return ret;
            }
        }






       // Elabora BOMS PhaseInstances e Itemss


        ipDispatcher EseguiRichiestaPrelievi(string phaseInstanceId, string richiedente)
        {
            try
            {
                var ret = new ipDispatcher();
                var retByUser = new ipDispatcher();

                retByUser.Action = "picker";
                retByUser.StatusCode = "ABORTED";
                retByUser.Message = "L'utente ha scelto annulla a una conferma task";
                retByUser.Requester = richiedente;

                if (AH.shutDownListenerRequest) return retByUser;

                ret.IdPhaseInstance = phaseInstanceId;
                ret.StatusCode = "INCOMPLETE";
                ret.Action = "picker";


                outCSV = $"{UT.pathData}\\Requests\\Lista_Prelievi_{DateTime.Now:HHmmssfff}_csv.txt";
                UT.AppendToFile(outCSV, "Codice;Prodotto;BOM;Qty;Operazione");

                string sm = log($"Avvio GET Lista Prelievi. file di output per verifiche: {outCSV}");
                if (VerboseMax)
                    if (!UT.Ask(sm)) return retByUser;


                var PI = iprod_pi.FirstOrDefault(a => a._id == phaseInstanceId);
                if (PI is null) throw new Exception($"Richiesta non valida: Il parametro phaseInstanceId ({phaseInstanceId}) non è stato trovato nell'archivio PhaseInstances.");


                sm = log($"PICK -   Elabora IDPhaseInstance = {phaseInstanceId}");
                if (VerboseMax)
                    if (!UT.Ask(sm)) return retByUser;


                if (UT.MockWS)
                {
                    return PrelievoMOCK(PI, ret, retByUser);
                }
                else
                {

                    if (UT.curMV.Technology == "SOAP")
                        return PrelievoSOAP(PI, ret, retByUser);
                    else if (UT.curMV.Technology == "REST")
                        return PrelievoREST(PI, ret, retByUser);
                    else return ret;

                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }



        ipDispatcher PrelievoMOCK(PhaseInstance PI, ipDispatcher ret, ipDispatcher retByUser)
        {

            try
            {

                string sm = "";
                string st = "";
                var req = new myNameSpace.sendJobsV01Request();
                //var cli = AH.WSSoapClient;

                req.param = new myNameSpace.JobTypeV01[1];
                var x = new myNameSpace.JobTypeV01();
                x.jobNumber = PI.Summarydata.Wordordername;

                var jobs = new List<reqSchema>();
                var prelievi = new List<itemToGet>();


                int xi = 0;
                var pi = PI;
                prelievi = new List<itemToGet>();
                prelievi = fetchItemsToGet(pi);

                st = pi.Linkeddata.Itemid;
                var item = findItem(st);
                if (item is null)
                {
                    sm = log($"PICK - **WARN** Articolo con id '{st}' della PhaseInstance '{PI._id}' non trovato su iProd");
                    if (VerboseMax)
                        if (!UT.Ask(sm)) return retByUser;

                    UT.AppendToFile(outCSV, $"ID ITEM={st};null;{curBom};0;ITEM-NOT-FOUND");

                    ret.Message = sm;
                    return ret;
                }
                if (prelievi.Count == 0)
                {

                    sm = log($"PICK -  ... richiesto prelievo per {item.code} {item.name} ma non risultano giacenze consistenti per poter essere prelevato dal magazzino");
                    if (VerboseMax)
                        if (!UT.Ask(sm)) return retByUser;

                    // generiamo comunque il record csv con qty = 0,  non lo invieremo a soap ma noi possiamo sapere cosa non ha trovato
                    UT.AppendToFile(outCSV, $"{item.code};{item.name};{curBom};0;ND");

                    ret.Message = sm;
                    return ret;
                }

                x.JobPosition = new myNameSpace.JobPositionTypeV01[prelievi.Count()];


                sm = $"PICK -             ...jobNumber {x.jobNumber}, jobTime {x.jobTime}, jobDate {x.jobDate}, jobStatus {x.jobStatus}, tot. prelievi {prelievi.Count}";
                log(sm);

                foreach (var pr in prelievi)
                {
                    if (AH.shutDownListenerRequest) return retByUser;
                    item = findItem(pr.itemid);

                    sm = log($"PICK -  Da prelevare {pr.qty} pezzi di {item.name} per la produzione di {curBom}");
                    if (VerboseMax)
                        if (!UT.Ask(sm)) return retByUser;

                    int nl = x.JobPosition.Length;
 

                    x.JobPosition[xi] = new myNameSpace.JobPositionTypeV01();
                    var jb = x.JobPosition[xi];


                    // store cod o nome se null, se entrambi null non li gestisce e prosegue
                    if (UT.NotNull(item.code))
                        jb.articleNumber = item.code;
                    else if (UT.NotNull(item.name))
                        jb.articleNumber = item.name;
                    else
                        continue;

                    jb.operation = "-";
                    jb.nominalQuantity = $"{pr.qty}";

                   
                    UT.AppendToFile(outCSV, $"{item.code};{item.name};{curBom};{pr.qty};Scarico");
                    xi++;
                    sm = $"PICK -  ... #{xi} articleNumber {jb.articleNumber}, actualQuantity {jb.actualQuantity}, nominalQuantity {jb.nominalQuantity}, positionStatus {jb.positionStatus}";
                    log(sm);
                    UT.AddRowHist("PICK-OK", $"Rich da {retByUser.Requester}:" + sm);
                }


                setPB(0);

                //sm = log("PICK -   Risorse caricate da iProd, invio richiesta prelievi al M.V. ");
                //if (VerboseMax)
                //    if (!UT.Ask(sm)) return retByUser;


                req.param[0] = x;
                var resp = MockHelper.GetPickResponse(req); //  ; // cli.sendJobsV01(req);
                if (resp is null) throw new Exception("PICK - ***ERRORE*** sendJobsV01 (Prelievi) non eseguito: la funzione ha restituito il response nullo");

            //    sm = log($"PICK -  ..response: {resp.@return.returnValue}");

                if (UT.NotNull(resp.@return.returnErrorMessage))
                {
                    sm = log($"PICK - **ERRORE** ResponseValue: {resp.@return.returnValue}, ResponseError: '{resp.@return.returnErrorMessage}'");
                    ret.Message = sm;
                    ret.StatusCode = "ERRORRESPONSE";
                    ret.inError = true;
                    UT.AddRowHist("PICK-ERR", $"Rich da {retByUser.Requester}:" + sm);
                }
                else
                {
                    ret.PickedItems = prelievi.Count();
                    sm = log($"PICK - Richiesta conclusa correttamente e senza errori. Inviata lista di {prelievi.Count} elementi.");
                    ret.Message = sm;
                    ret.StatusCode = "COMPLETED";
                    UT.AddRowHist("PICK-OK", $"Rich da {retByUser.Requester}:" + sm);
                }

                return ret;
            }
            catch (Exception ex)
            {

                var sm = log("PICK - ***ERRORE** Eccezione " + ex.Message.Replace("\r\n", "") + ", " + ex.StackTrace.Replace("\r\n", ""));
                if (VerboseMax)
                    if (!UT.Ask(sm)) return retByUser;

                ret.inError = true;
                ret.StatusCode = "OFFLINE";
                ret.Message = sm;

                UT.AddRowHist("PICK-ERR", $"Rich da {retByUser.Requester}:" + sm);

                return ret;

            }

        }



        ipDispatcher PrelievoSOAP(PhaseInstance PI, ipDispatcher ret, ipDispatcher retByUser)
        {

            try
            {

                string sm = "";
                string st = "";
                var req = new myNameSpace.sendJobsV01Request();
                var cli = AH.WSSoapClient;

                req.param = new myNameSpace.JobTypeV01[1];

                var x = new myNameSpace.JobTypeV01();
                x.jobNumber = PI.Summarydata.Wordordername;

                // proviamo a cancellarlo se fosse gia stato inviato
                #region CANCELLAZIONE PREVENTIVA

                var reqd = new myNameSpace.deleteJobV01Request();
                reqd.param = new myNameSpace.ParDeleteJobV01();
                reqd.param.jobNumber = x.jobNumber;

                var respd = cli.deleteJobV01(reqd);

                //   sm = log($"Response MP: {respd.@return.returnValue}");

                if (UT.NotNull(respd.@return.returnErrorMessage))
                    ret.AdditionalInfo = "Esito cancellazione job già presente: " + respd.@return.returnErrorMessage;
                else
                    ret.AdditionalInfo = "Il Job era già stato inviato ed è stato eliminato per il reinvio";

                #endregion

                var jobs = new List<reqSchema>();
                var prelievi = new List<itemToGet>();


                int xi = 0;
                var pi = PI;
                prelievi = new List<itemToGet>();
                prelievi = fetchItemsToGet(pi);

                st = pi.Linkeddata.Itemid;
                var item = findItem(st);
                if (item is null)
                {
                    sm = log($"PICK - **WARN** Articolo con id '{st}' della PhaseInstance '{PI._id}' non trovato su iProd");
                    if (VerboseMax)
                        if (!UT.Ask(sm)) return retByUser;

                    UT.AppendToFile(outCSV, $"ID ITEM={st};null;{curBom};0;ITEM-NOT-FOUND");

                    ret.Message = sm;
                    return ret;
                }
                if (prelievi.Count == 0)
                {

                    sm = log($"PICK -  ... richiesto prelievo per {item.code} {item.name} ma non risultano giacenze consistenti per poter essere prelevato dal magazzino");
                    if (VerboseMax)
                        if (!UT.Ask(sm)) return retByUser;

                    // generiamo comunque il record csv con qty = 0,  non lo invieremo a soap ma noi possiamo sapere cosa non ha trovato
                    UT.AppendToFile(outCSV, $"{item.code};{item.name};{curBom};0;ND");

                    ret.Message = sm;
                    return ret;
                }

                x.JobPosition = new myNameSpace.JobPositionTypeV01[prelievi.Count()];


                sm = $"PICK -             ...jobNumber {x.jobNumber}, jobTime {x.jobTime}, jobDate {x.jobDate}, jobStatus {x.jobStatus}, tot. prelievi {prelievi.Count}";
                log(sm);



                foreach (var pr in prelievi)
                {
                    if (AH.shutDownListenerRequest) return retByUser;
                    item = findItem(pr.itemid);

                    sm = log($"PICK -  Da prelevare {pr.qty} pezzi di {item.name} per la produzione di {curBom}");
                    if (VerboseMax)
                        if (!UT.Ask(sm)) return retByUser;

                    int nl = x.JobPosition.Length;
                    x.JobPosition[xi] = new myNameSpace.JobPositionTypeV01();
                    var jb = x.JobPosition[xi];


                    // store cod o nome se null, se entrambi null non li gestisce e prosegue
                    if (UT.NotNull(item.code))
                        jb.articleNumber = item.code;
                    else if (UT.NotNull(item.name))
                        jb.articleNumber = item.name;
                    else
                        continue;

                    jb.operation = "-";
                    jb.nominalQuantity = $"{pr.qty}";

                    UT.AppendToFile(outCSV, $"{item.code};{item.name};{curBom};{pr.qty};Scarico");
                    xi++;
                    sm = $"PICK -  #{xi} articleNumber {jb.articleNumber}, actualQuantity {jb.actualQuantity}, nominalQuantity {jb.nominalQuantity}, positionStatus {jb.positionStatus}";
                    log(sm);
                    UT.AddRowHist("PICK-OK", $"Rich da {retByUser.Requester}:" + sm);
                }



                setPB(0);

                //sm = log("PICK -   Risorse caricate da iProd, invio richiesta prelievi al M.V. ");
                //if (VerboseMax)
                //    if (!UT.Ask(sm)) return retByUser;


                req.param[0] = x;
                var resp = cli.sendJobsV01(req);
                if (resp is null) throw new Exception("sendJobsV01 (Prelievi) non eseguito: la funzione ha restituito il response nullo");

                //   sm = log($"Response MP: {resp.@return.returnValue}");

                if (UT.NotNull(resp.@return.returnErrorMessage))
                {
                    sm = log($"PICK - **ERRORE** ResponseValue: {resp.@return.returnValue}, ResponseError: '{resp.@return.returnErrorMessage}'");
                    ret.Message = sm;
                    ret.StatusCode = "ERRORRESPONSE";
                    ret.inError = true;
                    UT.AddRowHist("PICK-ERR", $"Rich da {retByUser.Requester}:" + sm);
                }
                else
                {
                    ret.PickedItems = prelievi.Count();
                    sm = log($"PICK - Richiesta conclusa correttamente e senza errori. Inviata lista di {prelievi.Count} elementi.");
                    ret.Message = sm;
                    ret.StatusCode = "COMPLETED";
                    UT.AddRowHist("PICK-OK", $"Rich da {retByUser.Requester}:" + sm);
                }

                return ret;
            }
            catch (Exception ex)
            {

                var sm = log("Errore " + ex.Message.Replace("\r\n", "") + ", " + ex.StackTrace.Replace("\r\n", ""));
                if (VerboseMax)
                    if (!UT.Ask(sm)) return retByUser;

                ret.inError = true;
                ret.StatusCode = "OFFLINE";
                ret.Message = sm;

                return ret;

            }

        }


        ipDispatcher PrelievoREST(PhaseInstance PI, ipDispatcher ret, ipDispatcher retByUser)
        {

            /*

            POST
            url: https://127.0.0.1:12121/reservation
            body:

                }          
                  "articleNumber": "TA24451", 
                  "userIdentifier": "admin", 
                  "actionType": 3, 
                  "quantity": 5 
                } 
               
            dove actionType: 2 = Store, 3 = Prelievo

            */


            try
            {

                string sm = "";
                string st = "";

                string apiHost = $"https://{UT.iProdCFG.MP_IP}:{UT.iProdCFG.MP_Port}/";

                HttpClient httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(apiHost);

                var job = new RESTReservation();
              
                var jobs = new List<reqSchema>();
                var prelievi = new List<itemToGet>();


                int xi = 0;
                var pi = PI;
                prelievi = new List<itemToGet>();
                prelievi = fetchItemsToGet(pi);

                st = pi.Linkeddata.Itemid;
                var item = findItem(st);
                if (item is null)
                {
                    sm = log($"PICK - **WARN** Articolo con id '{st}' della PhaseInstance '{PI._id}' non trovato su iProd");
                    if (VerboseMax)
                        if (!UT.Ask(sm)) return retByUser;

                    UT.AppendToFile(outCSV, $"ID ITEM={st};null;{curBom};0;ITEM-NOT-FOUND");

                    ret.Message = sm;
                    return ret;
                }

                if (prelievi.Count == 0)
                {

                    sm = log($"PICK -  ... richiesto prelievo per {item.code} {item.name} ma non risultano giacenze consistenti per poter essere prelevato dal magazzino");
                    if (VerboseMax)
                        if (!UT.Ask(sm)) return retByUser;

                    // generiamo comunque il record csv con qty = 0,  non lo invieremo a soap ma noi possiamo sapere cosa non ha trovato
                    UT.AppendToFile(outCSV, $"{item.code};{item.name};{curBom};0;ND");

                    ret.Message = sm;
                    return ret;
                }

             
                sm = log($"PICK -     .... caricamento richieste http a HOFFMANN per un totale di {prelievi.Count} prelievi.");
                if (VerboseMax)
                    if (!UT.Ask(sm)) return retByUser;


                string errors = "";
                foreach (var pr in prelievi)
                {
                    if (AH.shutDownListenerRequest) return retByUser;
                    item = findItem(pr.itemid);

                    sm = log($"PICK -  Da prelevare {pr.qty} pezzi di {item.name} per la produzione di {curBom}");
                    if (VerboseMax)
                        if (!UT.Ask(sm)) return retByUser;

                    job.articleNumber = "";
                    job.userIdentifier = "admin";
                    job.actionType = "3";           // 3-pick, 2-send
                    job.quantity = "1";
                    
                    // store cod o nome se null, se entrambi null non li gestisce e prosegue
                    if (!item.code.IsNull())
                        job.articleNumber = item.code;
                    else if (!item.name.IsNull())
                        job.articleNumber = item.name;
                    else
                        continue;

                    job.quantity = $"{pr.qty}";


                

                    var json = JsonConvert.SerializeObject(job);
                    StringContent requestContent = new StringContent(json, Encoding.UTF8, "application/json");
                    string APIurl = "reservation";

                    var resp = UT.APICall(httpClient, APIurl, requestContent).Result;
                    if (resp.status != "OK")
                    {
                        sm = $"BAD REQUEST    ....#{xi}, art. '{job.articleNumber}', errore: {resp.response}. {UT.LF}";
                        errors += sm;
                        UT.AppendToFile(outCSV, $"{item.code};{item.name};{curBom};{pr.qty};**SCARICO IN ERRORE**");
                        UT.AddRowHist("PICK-ERR", $"Rich da {retByUser.Requester}:" + sm);

                    }
                    else
                    {
                        UT.AppendToFile(outCSV, $"{item.code};{item.name};{curBom};{pr.qty};Scarico");
                        sm = $"PICK -  #{xi} articleNumber {job.articleNumber}, quantity {job.quantity}";
                        log(sm);
                        UT.AddRowHist("PICK-OK", $"Rich da {retByUser.Requester}:" + sm);

                    }

                    xi++;
                }


                if (!errors.IsNull())
                {
                    ret.Message = errors;
                    ret.StatusCode = "ERRORRESPONSE";
                    ret.inError = true;
                }
                else
                {
                    ret.PickedItems = prelievi.Count();
                    sm = log($"PICK - Richiesta conclusa correttamente e senza errori. Inviata lista di {prelievi.Count} elementi.");
                    ret.Message = sm;
                    ret.StatusCode = "COMPLETED";
                }

                return ret;
            }
            catch (Exception ex)
            {

                var sm = log("Errore " + ex.Message.Replace("\r\n", "") + ", " + ex.StackTrace.Replace("\r\n", ""));
                if (VerboseMax)
                    if (!UT.Ask(sm)) return retByUser;

                ret.inError = true;
                ret.StatusCode = "OFFLINE";
                ret.Message = sm;

                return ret;

            }

        }


        Items findItem(string id)
        {
            return iprod_items.FirstOrDefault(a => a._id == id);
        }

        List<itemToGet> fetchItemsToGet(PhaseInstance pi)
        {
            //log($"fetchItemsToGet pi._id = {pi._id} ");
            //log($"fetchItemsToGet pi.Phaseid = {pi.Phaseid} ");
            var ret = new List<itemToGet>();

            // find bom
            curBom = "";
            // get id item
            var itm = pi.Linkeddata.Itemid;

            //log($"fetchItemsToGet pi.Linkeddata.Itemid = {pi.Linkeddata.Itemid} ");
            // get 1° bom che ha l'esponente con un produceditem = all'item
            var bom = iprod_boms.FirstOrDefault(a => a.exponents.FirstOrDefault(x => x.produceditems.FirstOrDefault(y => y.itemid == itm) != null) != null);
            if (bom is null) return ret;

            curBom = $"{bom.code}\\{bom.name}";

            //log($"fetchItemsToGet bom found = {curBom}, bom._id {bom._id} ");

            // get obj exponent
            var exp = bom.exponents.FirstOrDefault(x => x.produceditems.FirstOrDefault(y => y.itemid == itm) != null);
            if (exp is null) return ret;
            // get 1° nodo con phaseid = Phaseinstance.Phaseid
            //    var btree = exp.bomtree.FirstOrDefault(x => x.type == 0 && x.typeid == pi.Phaseid);

            //  if (btree is null) return 0;
            // somma la qta di tutti i figli di tipo item * la qua de phaseinstance

        //    log($"fetchItemsToGet exp._id = {exp._id}");


            foreach (var btree in exp.bomtree)
            {
          //      log($"fetchItemsToGet foreach.exp.bomtree.type = {btree.type}, typeid = {btree.typeid}");

                if (btree.type == 0 && btree.typeid == pi.Phaseid)
                {
                  //  log($"fetchItemsToGet foreach.exp.bomtree.typeid = {btree.typeid} FOUND (deve essere uguale alla id fase)");

                    foreach (var s in btree.sons)
                    {
                        if (s.type == 1 || s.type == 5 || s.type == 6)
                        {
                            ret.Add(new itemToGet
                            {
                                itemid = s.typeid,
                                qty = (s.qty * pi.Performancedata.Totalqty)
                            });
                        //    log($"fetchItemsToGet foreach.exp.btree.sons s = ADDED itemid = {s.typeid}, giacenza {(s.qty * pi.Performancedata.Totalqty)}, prelievi.Count = {ret.Count}");

                        }

                        ret = fetchSons(s, pi, ret);
                    }
                }
            }

            return ret;
        }



        List<itemToGet> fetchSons(BomTreeNode n, PhaseInstance pi, List<itemToGet> lista)
        {
            var ret = new List<itemToGet>();

            
            foreach (var s in n.sons)
            {
                if (s.type == 1 || s.type == 5 || s.type == 6)
                {
                   // log($"fetchSons foreach.exp.btree.sons s = ADDED itemid = {s.typeid}, giacenza {(s.qty * pi.Performancedata.Totalqty)}");
                    lista.Add(new itemToGet
                    {
                        itemid = s.typeid,
                        qty = (s.qty * pi.Performancedata.Totalqty)
                    });
                }
               

                lista = fetchSons(s, pi, lista);

            }

            return lista;
        }



        double sumson(BomTreeNode n, PhaseInstance pi)
        {
            double ret = 0;

            foreach (var s in n.sons)
            {
                if (s.type == 0 && s.typeid == pi.Phaseid)
                {
                    foreach (var sok in s.sons)
                    {
                        if (sok.type == 1 || sok.type == 5 || sok.type == 6)
                            ret += (s.qty * pi.Performancedata.Totalqty);
                        if (sok.type == 0)
                        {
                            foreach (var s2 in sok.sons)
                            {
                                if (s2.type == 1 || s2.type == 5 || s2.type == 6)
                                    ret += (s.qty * pi.Performancedata.Totalqty);
                            }
                        }
                    }
                    return ret;
                }

                ret += sumson(s, pi);
                if (ret > 0) return ret;   // se ha trovato qualcosa non deve cercare altro
            }

            return ret;
        }

        #endregion

     


        /// <summary>
        /// Scarica da azure.test.iprod una specifica entità
        /// </summary>
        /// <param name="what"></param>
        /// <param name="initLog"></param>
        /// <returns>true</returns>
        public UT.Contatore load_iProd(string what)
        {

            string txtContatore = "";
            var dStart = DateTime.Now;
            string prevWhat = what;
            string curWhat = what;
            string msg = "";

            bool isEmpty = false;
            var CC = new UT.Contatore(what); // avvia timer 

            Application.DoEvents();

            try
            {
                var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(UT.EndPointIPROD);


                // AUTENTICA SE NON L'AVEVA GIA FATTO
                //if (string.IsNullOrEmpty(usertoken) || what == "login")
                //{

                //    log($"ACCS - Login a iProd in corso..");

                //    curWhat = "iproduser";
                //    dataparameter = "iProdAuthentication/GetToken?username=" + username + "&password=" + password;
                //    httpClient.BaseAddress = new Uri(UT.EndPointIPROD);
                //    content = httpClient.GetAsync(dataparameter).Result;

                //    if (content.StatusCode != isOk)
                //    {
                //        msg = content.Content.ReadAsStringAsync().Result;
                //        UT.httpErr objErr = Newtonsoft.Json.JsonConvert.DeserializeObject<UT.httpErr>(msg);
                //        var st = log($"ACCS - Wrong API response. Err Code: {objErr.status} - {content.StatusCode}, msg: {objErr.title}, API: {dataparameter} ");

                //        throw new Exception(st);
                //    }

                //    usertoken = content.Content.ReadAsStringAsync().Result;
                //    log($"ACCS -  Autenticazione effettuata con successo: token {usertoken}");
                //    log($"ACCS -  Connesso ad iProd. Get Profilo");
                //    // AUTENTICAZIONE OK

                //    // GET Profilo
                //    dataparameter = "Account/GetIprodCustomer?token=" + usertoken;
                //    content = httpClient.GetAsync(dataparameter).Result;
                //    log($"ACCS -  API Account/GetIprodCustomer eseguita");
                //    customerdata = content.Content.ReadAsStringAsync().Result;
                //    iproduser = Newtonsoft.Json.JsonConvert.DeserializeObject<iProdCustomers>(customerdata);
                //    if (iproduser is null)
                //        log($"ACCS - oggetto iprod user nullo");
                //    else
                //    {
                //        var loggedUser = iproduser.Customerusers.Find(x => x.Username == username);
                //        if (loggedUser != null)
                //        {
                //            userguid = loggedUser._id;
                //            NameSurname = loggedUser.Name + " " + loggedUser.Surname;
                //        }
                //    }
                //}

                //if (what == "login")
                //{
                //    CC.RowCount = 1;
                //    CC.ProcessCompleted(true);
                //    return CC;
                //}



                curWhat = prevWhat;

                /* ==============================================================================================================

                                CARICAMENTO OGGETTI iProd. 
                            La sorgente sono le API dell'applicazione su Azure

                   ==============================================================================================================*/

                // scarica la lista richiesta 


                switch (curWhat)
                {
                    case "customers":

                        // GET Clienti

                        isEmpty = (iprod_customers is null || iprod_customers.Count == 0);
                        if (!isEmpty)
                        {
                            CC.RowCount = iprod_customers.Count;
                            CC.Loaded = true;
                            CC.ProcessCompleted(true, true);
                            return CC;
                        }

                        iprod_customers = new List<Customers>();


                        dataparameter = "Customers/GetCustomersTable?token=" + Program.ipTOKEN;
                        log($"GET - download Clienti in corso.. (url: ../{dataparameter.Substring(0, dataparameter.Length - 15)}**********)");
                        content = httpClient.GetAsync(dataparameter).Result;
                        customerdata = content.Content.ReadAsStringAsync().Result;

                        if (content.StatusCode != isOk)
                        {
                            CC.Witherror++;

                            msg = content.Content.ReadAsStringAsync().Result;
                            UT.httpErr objErr = Newtonsoft.Json.JsonConvert.DeserializeObject<UT.httpErr>(msg);
                            log($"GET -  Wrong API response. download iprod_customers, Err Code: {objErr.status} - {content.StatusCode}, msg: {objErr.title}, API: {dataparameter} ");
                            if (iprod_customers is null) iprod_customers = new List<Customers>();
                        }
                        else
                            iprod_customers = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Customers>>(customerdata);

                        txtContatore = $"                   ....scaricati da iProd {iprod_customers.Count} clienti ";
                        CC.read = iprod_customers.Count;

                        break;

                    //  I T E M S =================================================================================================================================

                    case "items":

                        Application.DoEvents();

                        isEmpty = (iprod_items is null || iprod_items.Count == 0);
                        if (!isEmpty)
                        {
                            CC.RowCount = iprod_items.Count;
                            CC.Loaded = true;
                            CC.ProcessCompleted(true, true);
                            return CC;
                        }

                        iprod_items = new List<Items>();


                        // GET Articoli
                        dataparameter = "Items/GetItemsTable?token=" + Program.ipTOKEN;
                        log($"GET -  download Articoli in corso.. (url: ../{dataparameter.Substring(0, dataparameter.Length - 15)}**********)");
                        content = httpClient.GetAsync(dataparameter).Result;
                        customerdata = content.Content.ReadAsStringAsync().Result;

                        if (content.StatusCode != isOk)
                        {
                            CC.Witherror++;

                            msg = content.Content.ReadAsStringAsync().Result;
                            UT.httpErr objErr = Newtonsoft.Json.JsonConvert.DeserializeObject<UT.httpErr>(msg);
                            log($"GET -  Wrong API response. Err Code: {objErr.status} - {content.StatusCode}, msg: {objErr.title}, API: {dataparameter} ");
                            iprod_items = new List<Items>();
                        }
                        else
                            iprod_items = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Items>>(customerdata);

                        txtContatore = $"                   ....scaricati da iProd {iprod_items.Count} Prodotti ";
                        CC.read = iprod_items.Count;
                        break;

                    // P H A S E I N S T A N C E S *****************************************************************************************************************************************************

                    case "phaseinstances":

                        Application.DoEvents();

                        isEmpty = (iprod_pi is null || iprod_pi.Count == 0);
                        if (!isEmpty)
                        {
                            CC.RowCount = iprod_pi.Count;
                            CC.Loaded = true;
                            CC.ProcessCompleted(true, true);
                            return CC;
                        }

                        iprod_pi = new List<PhaseInstance>();


                        // GET Phaseinstances table
                        dataparameter = "PhasesInstances/GetPhaseInstancesTable?token=" + Program.ipTOKEN;
                        log($"GET - download Istanze di fase (tutte) in corso.. (url: {dataparameter.Substring(0, dataparameter.Length - 15)}**********)");
                        content = httpClient.GetAsync(dataparameter).Result;
                        customerdata = content.Content.ReadAsStringAsync().Result;

                        if (content.StatusCode != isOk)
                        {
                            CC.Witherror++;

                            msg = content.Content.ReadAsStringAsync().Result;
                            UT.httpErr objErr = Newtonsoft.Json.JsonConvert.DeserializeObject<UT.httpErr>(msg);
                            log($"GET -  Wrong API response. Err Code: {objErr.status} - {content.StatusCode}, msg: {objErr.title}, API: {dataparameter} ");
                            iprod_pi = new List<PhaseInstance>();
                        }
                        else
                            iprod_pi = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PhaseInstance>>(customerdata);

                        txtContatore = $"                   ....scaricati da iProd {iprod_pi.Count} istanze di fase ";
                        CC.read = iprod_pi.Count;
                        break;



                    // P H A S E S  ==================================================================================================================================


                    case "phases":


                        // GET fasi (Phases) e macchines

                        isEmpty = (iprod_phases is null || iprod_phases.Count == 0);
                        if (!isEmpty)
                        {
                            CC.RowCount = iprod_phases.Count;
                            CC.Loaded = true;
                            CC.ProcessCompleted(true, true);
                            return CC;
                        }
                        iprod_phases = new List<Phase>();


                        dataparameter = "Phases/GetPhaseTable?token=" + Program.ipTOKEN;
                        log($"GET - download Fasi in corso..  (url: ../{dataparameter.Substring(0, dataparameter.Length - 15)}**********)");
                        content = httpClient.GetAsync(dataparameter).Result;
                        customerdata = content.Content.ReadAsStringAsync().Result;
                        if (content.StatusCode != isOk)
                        {
                            CC.Witherror++;

                            msg = content.Content.ReadAsStringAsync().Result;
                            UT.httpErr objErr = Newtonsoft.Json.JsonConvert.DeserializeObject<UT.httpErr>(msg);
                            log($"GET -  Wrong API response. carica iprod_phases, Err Code: {objErr.status} - {content.StatusCode}, msg: {objErr.title}, API: {dataparameter} ");
                            iprod_phases = new List<Phase>();
                        }
                        else
                            iprod_phases = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Phase>>(customerdata);

                        txtContatore = $"                   ....scaricati da iProd {iprod_phases.Count} Fasi ";
                        CC.read = iprod_phases.Count;
                        break;

                    // P O S T S ==================================================================================================================================

                    case "posts":

                        isEmpty = (iprod_posts is null || iprod_posts.Count == 0);
                        if (!isEmpty)
                        {
                            CC.RowCount = iprod_posts.Count;
                            CC.ProcessCompleted(true, true);
                            return CC;
                        }
                        iprod_posts = new List<Posts>();


                        if (UT.IsNull(tipoPost)) tipoPost = "1";
                        dataparameter = $"Posts/GetPosts?token={Program.ipTOKEN}&type={tipoPost}";
                        log($"GET - download Posts di tipo 1 in corso.. (url: ../{dataparameter.Substring(0, dataparameter.Length - 15)}**********)");
                        content = httpClient.GetAsync(dataparameter).Result;
                        customerdata = content.Content.ReadAsStringAsync().Result;
                        if (content.StatusCode != isOk)
                        {
                            CC.Witherror++;

                            msg = content.Content.ReadAsStringAsync().Result;
                            UT.httpErr objErr = Newtonsoft.Json.JsonConvert.DeserializeObject<UT.httpErr>(msg);
                            log($"GET -  Wrong API response. carica iprod_phases, Err Code: {objErr.status} - {content.StatusCode}, msg: {objErr.title}, API: {dataparameter} ");
                            iprod_posts = new List<Posts>();
                        }
                        else
                            iprod_posts = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Posts>>(customerdata);



                        txtContatore = $"                   ....scaricati da iProd {iprod_posts.Count} POST ";
                        CC.read = iprod_posts.Count;

                        break;


                    // C A T E G O R I E S  ==================================================================================================================================

                    case "categories":


                        isEmpty = (categories is null || categories.Count == 0);
                        if (!isEmpty)
                        {
                            CC.RowCount = categories.Count;
                            CC.Loaded = true;
                            CC.ProcessCompleted(true, true);
                            return CC;
                        }
                        categories = new Dictionary<string, string>();


                        log($"GET -  download categorie in corso..");


                        dataparameter = "Home/GetCategories?token=" + Program.ipTOKEN;
                        content = httpClient.GetAsync(dataparameter).Result;
                        customerdata = content.Content.ReadAsStringAsync().Result;

                        if (content.StatusCode != isOk)
                        {
                            MessageBox.Show("Errore durante il download delle categorie");
                            categories = new Dictionary<string, string>();
                            CC.Witherror++;
                            return CC;
                        }
                        else
                            categories = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(customerdata);


                        txtContatore = $"                   ....scaricate da iProd {categories.Count} categorie ";

                        break;

                    // W A R E H O U S E S  ==================================================================================================================================


                    case "warehouses":

                        isEmpty = (warehouses is null || warehouses.Count == 0);
                        if (!isEmpty)
                        {
                            CC.RowCount = warehouses.Count;
                            CC.Loaded = true;
                            CC.ProcessCompleted(true, true);
                            return CC;
                        }
                        warehouses = new List<Warehouse>();



                        warehouses = new List<Warehouse>();

                        dataparameter = "WareHouses/GetWareHousesTable?token=" + Program.ipTOKEN;
                        log($"GET - download Magazzini in corso.. (url ../{dataparameter.Substring(0, dataparameter.Length-15)}**********)");

                        content = httpClient.GetAsync(dataparameter).Result;
                        customerdata = content.Content.ReadAsStringAsync().Result;

                        if (content.StatusCode != isOk)
                        {
                            MessageBox.Show("Errore durante il download dei magazzini");
                            warehouses = new List<Warehouse>();
                            CC.Witherror++;
                            return CC;
                        }
                        else
                            warehouses = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Warehouse>>(customerdata);

                        txtContatore = $"                   ....scaricati da iProd {warehouses.Count} Magazzini ";
                        CC.read = warehouses.Count;
                        break;

                    // M A C H I N E S ==================================================================================================================================


                    case "machines":

                        isEmpty = (iprod_machines is null || iprod_machines.Count == 0);
                        if (!isEmpty)
                        {
                            CC.RowCount = iprod_machines.Count;
                            CC.Loaded = true;
                            CC.ProcessCompleted(true, true);
                            return CC;
                        }
                        iprod_machines = new List<Customermachine>();


                        dataparameter = "Machines/GetMachineTable?token=" + Program.ipTOKEN;
                        log($"GET -  download Macchinari in corso.. (url ../{dataparameter.Substring(0, dataparameter.Length - 15)}**********)");
                        content = httpClient.GetAsync(dataparameter).Result;
                        customerdata = content.Content.ReadAsStringAsync().Result;
                        if (content.StatusCode != isOk)
                        {
                            CC.Witherror++;

                            msg = content.Content.ReadAsStringAsync().Result;
                            UT.httpErr objErr = Newtonsoft.Json.JsonConvert.DeserializeObject<UT.httpErr>(msg);
                            log($" Wrong API response. carica MACHINES, Err Code: {objErr.status} - {content.StatusCode}, msg: {objErr.title}, API: {dataparameter} ");
                            iprod_machines = new List<Customermachine>();
                        }
                        else
                            iprod_machines = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Customermachine>>(customerdata);

                        txtContatore = $"                   ....scaricati da iProd {iprod_machines.Count} macchinari ";
                        CC.read = iprod_machines.Count;
                        break;

                    // B O M S ==================================================================================================================================


                    case "boms":

                        isEmpty = (iprod_boms is null || iprod_boms.Count == 0);
                        if (!isEmpty)
                        {
                            CC.RowCount = iprod_boms.Count;
                            CC.Loaded = true;
                            CC.ProcessCompleted(true, true);
                            return CC;
                        }
                        iprod_boms = new List<Bom>();


                        dataparameter = "Boms/GetBomsTable?token=" + Program.ipTOKEN;
                        log($"GET - download Distinte in corso.. (url ../{dataparameter.Substring(0, dataparameter.Length - 15)}**********)");
                        content = httpClient.GetAsync(dataparameter).Result;
                        customerdata = content.Content.ReadAsStringAsync().Result;

                        if (content.StatusCode != isOk)
                        {
                            CC.Witherror++;
                            msg = content.Content.ReadAsStringAsync().Result;
                            UT.httpErr objErr = Newtonsoft.Json.JsonConvert.DeserializeObject<UT.httpErr>(msg);
                            log($" Wrong API response. GetBomsTable, Err Code: {objErr.status} - {content.StatusCode}, msg: {objErr.title}, API: {dataparameter} ");
                            iprod_boms = new List<Bom>();
                        }
                        else
                            iprod_boms = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Bom>>(customerdata);

                        txtContatore = $"                   ....scaricate da iProd {iprod_boms.Count}  Boms (distinte base) ";
                        CC.read = iprod_boms.Count;
                        CC.RowCount = iprod_boms.Count;
                        break;

                    default:
                        throw new Exception($"Attenzione, parametro 'what' sconosciuto: {what}");
                }



                //setFASE();

                if (CC.RowCount == 0)
                    CC.RowCount = Convert.ToInt32(CC.read);

                log(CC.ProcessCompleted(true, true));
                return CC;
            }
            catch (Exception ex)
            {
                string mm = $"load_iprod('{curWhat}') Err: {ex.Message.Replace("\r\n", "")}, stack {ex.StackTrace.Replace("\r\n", "")} ";
                CC.inError = true;
                CC.ErrorText = mm;

                log("GET - " +mm);

                return CC;
            }
        }

 


        #region utility

        void setPB(int v, int max = 99999)
        {
            if (max < 99999)
                SendAction($"PERCENT|{v}|{max}");
            else
                SendAction($"PERCENT|{v}");
        }

        private string log(string m)
        {
            // standard log

            logCnt++;
           // UT.AppendToFile(logFile, logCnt + "| " + m);
            UT.AppendToFile(logFile, m);
            //   Application.DoEvents();
            return m;
        }


        // invia dei comandi al logger che dovranno essere interpretati come azioni
        private string SendAction(string cmd)
        {
            log("@CMD@|" + cmd);

            return cmd;
        }

        #endregion


        #region Dispose memory

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: eliminare lo stato gestito (oggetti gestiti)
                }



                // TODO: liberare risorse non gestite (oggetti non gestiti) ed eseguire l'override del finalizzatore
                // TODO: impostare campi di grandi dimensioni su Null
                disposedValue = true;
            }
        }

        // // TODO: eseguire l'override del finalizzatore solo se 'Dispose(bool disposing)' contiene codice per liberare risorse non gestite
        // ~ThreadPicker()
        // {
        //     // Non modificare questo codice. Inserire il codice di pulizia nel metodo 'Dispose(bool disposing)'
        //     Dispose(disposing: false);
        // }

        void IDisposable.Dispose()
        {
            // Non modificare questo codice. Inserire il codice di pulizia nel metodo 'Dispose(bool disposing)'
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion

    }

    public class ServiceClientFactory<TChannel> : ClientBase<TChannel> where TChannel : class
    {
        // implementation
        // var client = new ServiceClientFactory<yourServiceChannelInterface>().Create(newUrl);
        public TChannel Create(string url)
        {
            this.Endpoint.Address = new EndpointAddress(new Uri(url));
            return this.Channel;
        }
    }

}
