using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Text;
using System.Windows.Forms;
using JsonConvert = Newtonsoft.Json.JsonConvert;
using System.Linq;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using System.Configuration;
using System.Globalization;
using System.Reflection;
using UT = iProdWHSE.utility;
using MHE = iProdWHSE.MockHelper;
using System.Drawing.Imaging;
using System.Drawing;
using System.Threading;
using System.Net;
using iProdDataModel.Models;
using System.Net.Sockets;
using System.ServiceModel;
using System.Net.Mail;

/*

    Sistema di automazione magazzino verticale. 03/03/2023
                Informazioni utili

    Assolve a 3 servizi:
    - Get Lista Prelievi dal MV richieste dai Tablets
    - Get Giacenze e le allinea a iProd (cioè aggiorna iProd)
    - Ping

    La Lista prelievi è servita da un httpListener che viene messo in ascolto in localhost sulla porta 8098
    a cui i tablet possono fare richiesta.

    L'allineamento Giacenze invece vengono processate da un timer gestito dal dropdown in basso a sinistra
    ad ogni ciclo prende cosa c'è nel M.V., scarica gli items da iProd e verifica se vanno aggiornate

    il Ping è richiesto dai Tablets per assicurarsi che il dispositivo sia in linea


    Tutti i servizi si basano su 3 tipi di fonti dati.
    1. Web Service SOAP  (magazzini MP-12N e MP-100D)
    2. Web Service REST  (HOFFMANN)
    3. Mock (in mancanza di collegamento fisico ai WS li simula)

    i tipi records si basano tutti e tre su oggetti SOAP, che hanno tutte le caratteristiche che ci servono

    - se WS SOAP viene fatta la richiesta e il response ci da le informazioni che vogliamo.
    - se WS REST viene fatta la rchiesta restituendo ilsuo response specifico, con il quale assegnamo 
      il response SOAP e proseguiamo come se lo avessimo preso dal SOAP stesso
    - idem per i dati mockati, viene caricato un tipo oggetto SOAP come response e proseguiamo allo stesso modo



    Le icone led sono 4
    - Stato connessione a iProd
    - Stato connessione al WS
    - Stato Listener (avviato o fermo)
    - Stato timer stocks


*/

namespace iProdWHSE
{
    public partial class Form1 : Form
    {

        #region DICHIARAZIONI

        private const bool Forza  = true;
        static List<reqSchema> Requests { get; set; }
        public static WHSEInfo iProdCFG { get; set; }
        public static string MockHost { get; set; } = "localhost";
        public static int MockPort { get; set; } = 8088;

        public static string outF { get; set; }
        public static string outCSV { get; set; }
        private static Thread ThreadListener { get; set; }
 
        public static bool isLoading { get; set; }
        public static string NameSurname  { get; set; }
        public static bool inprogress { get; set; }
        public static bool doingtask { get; set; }
        public static string processStatus { get; set; }
        public static string tipoPost { get; set; } = "1";  // in iprod_load("posts") specifica il tipo di post da scaricare
        static HttpResponseMessage content { get; set; }
        static string dataparameter { get; set; }
        static string customerdata { get; set; }
        Font FontBold { get; set; }
        Font FontNormal { get; set; } 
       
        // stato corrente del servizio
        private ImageComboBox.ImageComboBoxItem curSVC { get; set; }
        private List<ImageComboBox.ImageComboBoxItem> SVCstatus { get; set; } 
        private List<ImageComboBox.ImageComboBoxItem> SVCpaused { get; set; } 

        public static iProdCustomers iproduser { get; set; } // viene caricato una volta sola all'inizio e resta per tutta la sessione degli imports 
        public static Customerusers iprod_loggeduser { get; set; }
        public static List<Warehouse> warehouses { get; set; }
        public static List<Customers> iprod_customers { get; set; }
        public static List<Items> iprod_items { get; set; }
        public static List<Customermachine> iprod_machines { get; set; }
        public static List<Phase> iprod_phases { get; set; }
        public static List<Posts> iprod_posts { get; set; }
        public static List<Bom> iprod_boms { get; set; }
        public static List<PhaseInstance> iprod_pi { get; set; }
        public static Dictionary<string, string> categories { get; set; }
        static System.Net.HttpStatusCode isOk { get; set; }
        public static List<fieldSchema> utenti { get; set; }
        static UT.Counters CCN { get; set; }

        static string FASE { get; set; }



        private static string timerstatus { get; set; }
        private static DateTime started { get; set; }
        private static DateTime lastscan { get; set; }
        private static DateTime startSleep;
        private static bool abort_requested { get; set; }
        private static bool aborted { get; set; }
        private static int CicliEseguiti { get; set; }

        #endregion



        public Form1()
        {
            InitializeComponent();

            lbEsitoTestConnessione.Text = "";

            isLoading = true;

            processStatus = "idle";
            SVCstatus = new List<ImageComboBox.ImageComboBoxItem>();
            SVCpaused = new List<ImageComboBox.ImageComboBoxItem>();
            isOk = System.Net.HttpStatusCode.OK;
            CCN = new UT.Counters();
            FASE = "SYS       ";
            FontBold = new Font("Segoe UI", 12, FontStyle.Bold);
            FontNormal = new Font("Segoe UI", 12, FontStyle.Regular);



            UT.HwdSupportedModels = new List<WHSEInfo>() {
                new  WHSEInfo { Name = "MP-12N", Technology = "SOAP"  },
                new  WHSEInfo { Name = "MP-100D", Technology = "SOAP"  },
                new  WHSEInfo { Name = "HOFFMANN", Technology = "REST"  }
             };

            try
            {
                UT.mainForm = this;

                WindowState = FormWindowState.Maximized;



            }
            catch (Exception ex)
            {

                log(ex, "inizializzazione di Form1");
                
            }

        //   AvviaTimer();

        }





        private void Form1_Load(object sender, EventArgs e)
        {

            try
            {

                isLoading = true;

                if (!UT.loadConfig())
                {
                    Program.forzaExit = true;
                    Application.Exit();
                    return;
                }

                iProdCFG = UT.iProdCFG;
                UT.EndPointKey = iProdCFG.Ambiente;

                txtIP.Text = iProdCFG.MP_IP;
                txtPort.Text = iProdCFG.MP_Port;
                txtMPUrl.Text = iProdCFG.MP_Url;
                txtiProdUrl.Text = iProdCFG.iProdUrl;
                txtUser.Text = iProdCFG.iProdUser;
                txtPwd.Text = iProdCFG.iProdPassword;
                ckAutostart.Checked = iProdCFG.LoginAtStartup;
                ckSkipGIAC.Checked = !iProdCFG.ProcessStock;
                txtautoupdate.Text = iProdCFG.TimerInterval.ToString();
                txtMaxHist.Text = iProdCFG.MaxHistCount.ToString();

                if (UT.EndPointKey.IsNull()) UT.EndPointKey = "prod"; // default va su prods
                Program.UrlGate = new UrlGateway(UT.EndPointKey);
                UT.EndPointIPROD = Program.UrlGate.api;

                AgentHelper.BindingEndPoint = $"https://{iProdCFG.MP_IP}:{iProdCFG.MP_Port}/{iProdCFG.MP_Url}";

                timer1.Interval = 1000 * (int)iProdCFG.TimerInterval;

                int y = 0;
                int yf = 0;
                foreach (var hw in UT.HwdSupportedModels)
                {
                    cboHWModels.Items.Add(hw.Name);
                    if (hw.Name == iProdCFG.MP_Active) yf = y;
                    y++;
                }
                cboHWModels.SelectedIndex = yf;
                UT.curMV = UT.HwdSupportedModels[yf];

                SVCstatus.Add(new ImageComboBox.ImageComboBoxItem(9, "Avvia", 0));
                SVCstatus.Add(new ImageComboBox.ImageComboBoxItem(11, "Sospendi 30 minuti", 1));
                SVCstatus.Add(new ImageComboBox.ImageComboBoxItem(11, "Sospendi 1 ora", 0));
                SVCstatus.Add(new ImageComboBox.ImageComboBoxItem(11, "Sospendi fino a domani", 0));
                SVCstatus.Add(new ImageComboBox.ImageComboBoxItem(11, "Sospendi fino a riavvio manuale", 0));
                curSVC = SVCstatus[0];

                SVCpaused.Add(new ImageComboBox.ImageComboBoxItem(11, "In pausa", 0));
                SVCpaused.Add(new ImageComboBox.ImageComboBoxItem(9, "Avvia", 1));

                var v = UT.Versione;

                Text = $"iProdWHSE servizio polling magazzino verticale {v} . Sessione avviata il {DateTime.Now:dd/MM/yyyy} alle ore {DateTime.Now:HH:mm:ss} ";
                UT.IOLog(" ", true, true);
                log("===================================================================================================================================================");
                log("      " + Text);
                log("===================================================================================================================================================");

                PBperc.Text = "";

                LayoutControls();

                lbTimerStatus.Text = "Pronto.";

                //log("Servizio Listener INATTIVO. Premi START per metterlo in ascolto di richieste dai tablet", true);
                //log("Servizio Giacenze INATTIVO. Premi START per avviare la schedulazione", true);


                var f = UT.pathData + "listenerLog.txt";
                if (File.Exists(f) && !UT.FileGetIfCreatedToday(f))
                    UT.CopySafe(f, UT.pathBackups, true);


                loadDataObjects();


            

                pnlHist.Visible = false;
                pnlEvents.Visible = true;

                btHist.Font = FontNormal;
                btEvents.Font = FontBold;

                AgentHelper.Form1 = this;

                setSVCStatus("stopped");

                if (iProdCFG.LoginAtStartup)
                    tmrAutoStart.Enabled = true;

                iProdCFG.LocalIP = UT.GetLocalIP();

                isLoading = false;



           
                log($" ");
                log($"   Path di lavoro: {UT.pathData}");
                log($"         Listener: {iProdCFG.LocalIP}:8098 (localhost myIP)");
                log($"     cicli.(sec).: {iProdCFG.TimerInterval}");
                log($"             Nome: {iProdCFG.MP_Active} ");
                log($"               IP: {iProdCFG.MP_IP} ");
                log($"            Porta: {iProdCFG.MP_Port} ");
                log($"              Url: {iProdCFG.MP_Url} ");
                log($"               WS: '{AgentHelper.BindingEndPoint}'");
                log($"   Ambiente iProd: {iProdCFG.Ambiente} ");
                log($"     Utente iProd: {iProdCFG.iProdUser} ");
                log($"        API iProd: {UT.EndPointIPROD}");
                
                if (!iProdCFG.LoginAtStartup) log($"     iProd NON connesso: (no autologin in configurazione) ");

                if (UT.MockWS)
                {

                    log($"   ATTENZIONE!! DA CONFIGURAZIONE E' STATO IMPOSTATO L'USO DEL WEB SERVICE FAKE PER I TEST");
                    lbSYM.Visible = true;
                }


                ShowHist();
            }
            catch (Exception ex)
            {
                UT.WriteErrFile("Form1_Load  " + ex.Message + " - " + ex.StackTrace);
                MessageBox.Show(ex.Message + " - " + ex.StackTrace);
            }

        }


        void ShowHist()
        {
            TableLayIP.Items.Clear();
            var elenco = new List<RowHist>();

            if(!UT.FileExists(UT.HistFile)) {
                UT.HistCount = 0;
                return;
            }

            var lst = UT.LoadTextFile(UT.HistFile);

            UT.HistCount = lst.Count;

            foreach(var line in lst)
            {
                if (line.IsNull()) continue;
                var row = new RowHist(line);
                elenco.Add(row);
            }


            elenco = elenco.OrderByDescending(x => x.Data).ToList();

            int c = 0;
            if (iProdCFG.MaxHistCount == 0) iProdCFG.MaxHistCount = 150;
            foreach (var e in elenco)
            {
                c++;
                if (c>iProdCFG.MaxHistCount ) break;
                var nod = TableLayIP.Items.Add(e.idx.ToString());
                nod.SubItems.Add(e.Tipo);
                nod.SubItems.Add(e.Data.ToString());
                nod.SubItems.Add(e.Dex);

            }


        }

        


     

        private void btSTOP_Click(object sender, EventArgs e)
        {
            log("Interruzione processi in background richiesta dall'utente");

            timer1.Enabled = false;
            aborted = false;
            abort_requested = false;
            var m = UT.CompactMemory();

            var ms = UT.GetBytesReadable(m);
            log($"Memoria compattata (recuperato {ms})");
            AgentHelper.shutDownListenerRequest = true; // questo interrompe il thread dei prelievi
        }



     


        void LayoutControls()
        {

            var p = new Point(0, 118);

            panelHome.Location = p;
            panelHome.Width = Width;

            panelHome.Visible = true;

            pnlCFG.Location = p;
            pnlCFG.Width = Width;

            pnlCFG.Visible = false;


            pnlEvents.Location = new Point(0, 42);
            pnlHist.Location = new Point(0, 42);

            var MarginBottom = pnlTimerContainer.Top;
            pnlEvents.Height = MarginBottom - 180; // pnlEvents.Top;
            //pnlHist.Height = pnlEvents.Height -130;
            //pnlHist.Width = Width-20;
            pnlEvents.Width = Width-20;

        }



      
        void loadDataObjects()
        {
            Requests = new List<reqSchema>();
            Requests.Add(new reqSchema("Giacenze", "readAllAMDReqV01", "sample_giacenze.txt"));
            Requests.Add(new reqSchema("Lista di Prelievo", "sendJobsReqV01", "sample_lista_prelievi.txt"));
            Requests.Add(new reqSchema("Azzera Lista di prelievo", "deleteJobReqV01", ""));
            Requests.Add(new reqSchema("Stato MP", "MP-status", ""));

        }


     



        void ListenerBackGroundWorker()
        {
            new ThreadPicker();
        }


        // qui mettici tutti i timers del progetto
        public void stopTimers()
        {
         //   timer_progress.Enabled = false;
         //   timerLEDS.Enabled = false;

        }


        void setPB(double conta, double mx = int.MaxValue)
        {


            //int minusTwo = int.Parse("(2)", NumberStyles.Integer |NumberStyles.AllowParentheses);
            //decimal fivePointTwo = decimal.Parse("£5.20", NumberStyles.Currency, CultureInfo.GetCultureInfo("en-GB"));


            if (conta <= 0 || mx <= 0 || mx < conta)
            {
                PB1.Value = 0;
                PB1.Visible = false;
                PBperc.Text = "";
                return;
            }

            if (mx.Equals(int.MaxValue))
            {
                setProgressBar(conta);
                return;
            }

            unchecked { setProgressBar(mx / conta); }
        }

        void setProgressBar(double v)
        {
            if (v <= 0)
            {
                PB1.Value = 0;
                PB1.Visible = false;
                PBperc.Text = "";
                return;
            }

            var tcs = 100 / v;
            if (tcs < int.MinValue) tcs = int.MinValue;
            if (tcs > int.MaxValue) tcs = int.MaxValue;

            int dtcs = Convert.ToInt32(tcs);

            //  int.TryParse($"{tcs}", out int dtcs);

            if (dtcs > 100) dtcs = 100;
            if (dtcs < 0) dtcs = 0;
            PB1.Value = dtcs;
            PBperc.Text = dtcs + "%";
            PB1.Visible = dtcs>0 && dtcs<100;
        }


        void status(string m)
        {
            lbTimerStatus.Text = m;
            Application.DoEvents();
        }

        Items findItem(string id)
        {
            return iprod_items.FirstOrDefault(a => a._id == id);
        }

        static string curBom = "";

        List<itemToGet> fetchItemsToGet(PhaseInstance pi)
        {
            var ret = new List<itemToGet>();

            // find bom
            curBom = "";
            // get id item
            var itm = pi.Linkeddata.Itemid;
            // get 1° bom che ha l'esponente con un produceditem = all'item
            var bom = iprod_boms.FirstOrDefault(a => a.exponents.FirstOrDefault(x => x.produceditems.FirstOrDefault(y => y.itemid == itm) != null) != null);
            if (bom is null) return ret;

            curBom = $"{bom.code}\\{bom.name}";
            // get obj exponent
            var exp = bom.exponents.FirstOrDefault(x => x.produceditems.FirstOrDefault(y => y.itemid == itm) != null);
            if (exp is null) return ret;
            // get 1° nodo con phaseid = Phaseinstance.Phaseid
            //    var btree = exp.bomtree.FirstOrDefault(x => x.type == 0 && x.typeid == pi.Phaseid);

            //  if (btree is null) return 0;
            // somma la qta di tutti i figli di tipo item * la qua de phaseinstance
            foreach (var btree in exp.bomtree)
            {
                foreach (var s in btree.sons)
                {
                    ret = fetchSons(s, pi, ret);
                }

            }

            return ret;
        }




        List<itemToGet> fetchSons(BomTreeNode n, PhaseInstance pi, List<itemToGet> lista)
        {
            var ret = new List<itemToGet>();

            foreach (var s in n.sons)
            {
                if (s.type == 0 && s.typeid == pi.Phaseid)
                {
                    foreach (var sok in s.sons)
                    {
                        if (sok.type == 1 || sok.type == 5 || sok.type == 6)
                        {
                            lista.Add(new itemToGet
                            {
                                itemid = sok.typeid,
                                qty = (sok.qty * pi.Performancedata.Totalqty)
                            });
                        }
                        if (sok.type == 0)
                        {
                            foreach (var s2 in sok.sons)
                            {
                                if (s2.type == 1 || s2.type == 5 || s2.type == 6)
                                {
                                    lista.Add(new itemToGet
                                    {
                                        itemid = s2.typeid,
                                        qty = (s2.qty * pi.Performancedata.Totalqty)
                                    });
                                }
                            }
                        }
                    }
                    return lista;
                }

                ret = fetchSons(s, pi, lista);
                if (ret.Count > 0) return ret;   // se ha trovato qualcosa non deve cercare altro
            }

            return ret;
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

        
      
        public bool isLogDetailed()
        {
            return ckVerbose.Checked;
        }


        bool testWSConnection(bool silent = true, bool prod = false)
        {
            try
            {
                if (prod) return true;

                setFASE("testWS");

                log($"{FASE} test connessione a WS SOAP in corso..");

                if (!AgentHelper.initWS(UT.EndPointCOMPANY))
                {
                    if (AgentHelper.Errors)
                        throw new Exception(AgentHelper.ErrorText);
                }

                var mm = log($"Test connessione a WS SOAP effettuata con successo");
                if (!silent) MessageBox.Show(mm);
                return true;
            }
            catch (Exception ex)
            {
                if (!silent)
                    MessageBox.Show(log(ex.Message + " " + ex.StackTrace), "Test connessione a WS SOAP non riuscito");
                return false;

            }
        }


        #region ExecuteProcess (giac, prel, del, ping mp)



        // get Stocks dal servizio REST, usa gli oggetti del Soap per uniformare i risultati
        async Task<myNameSpace.readAllAMDV01Response> GetStockREST(myNameSpace.readAllAMDV01Request req)
        {

            string sm = "";
            var resp = new myNameSpace.readAllAMDV01Response();


            try
            {
                // richiesta \stock restituisce una array di articoli di tipo RESTStock

                string APIurl = "stock";
                var ret = await UT.APICall(AgentHelper.WSClient, APIurl).ConfigureAwait(false);

                if (ret.status != "OK")
                {
                    sm = log($"BAD REQUEST    ....errore: {ret.response}.");
                    UT.AddRowHist("STOCK-ERR", sm);

                }
                else
                {
                    // li converto in SOAP AMDTypeV01 e li restituisco al chiamante

                    var articles = JsonConvert.DeserializeObject<warehousedata>(ret.response);
                    log("Initializing AMDTypeV01");
                    var amd = new List<myNameSpace.AMDTypeV01>();


                    log("Before Location Article Cycle");
                    foreach (var art in articles.locationarticle)                    
                    {
                        var e  = new myNameSpace.AMDTypeV01();
                        e.articleNumber = art.idArticle.ToString();
                        e.articleName = art.articleNumber;
                        e.inventoryAtStorageLocation = art.availableQuantity.ToString("N3");
                        amd.Add(e);
                        log($"{e.articleNumber} {e.articleName} = {e.inventoryAtStorageLocation}", false);

                    }
                    if (resp.@return == null)
                    {
                        resp.@return = new myNameSpace.RetReadAllAMDV01();
                    }
                    resp.@return.returnValue = 1;
                    resp.@return.article = amd.ToArray();

                    //sm = $"PICK -  #{xi} articleNumber {job.articleNumber}, quantity {job.quantity}";
                    //log(sm);
                    //UT.AddRowHist("PICK-OK", $"Rich da {retByUser.Requester}:" + sm);

                }

                return resp;
            }
            catch (Exception ex)
            {

                sm = log($"BAD REQUEST  Rilevata eccezione: {ex.Message + " " +  ex.StackTrace}.");
                UT.AddRowHist("STOCK-ERR", sm);
                return resp;
            }

        }


        bool EseguiRichiestaGiacenze(reqSchema rq)
        {

            if (ckSkipGIAC.Checked) return true;
            

            UT.Contatore CC = null;
            var sm = "";
            try
            {
                SetNetStatus("LEDS-TIMER-ON");

                CicliEseguiti++;
                log($"Lettura giacenze n° {CicliEseguiti} {DateTime.Now:ddd dd alle HH:mm:ss}", Forza);

                System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };


                setPB(40, 100);
                if (iprod_items is null || iprod_items.Count == 0)
                {
                    iprod_items = new List<Items>();
                    sm = log("GIAC -        ....connessione a iProd e download Prodotti");
                }



                CC = load_iProd("items");
                CC.RowCount = iprod_items.Count;

                var resp = new myNameSpace.readAllAMDV01Response();

                if (UT.MockWS)
                {

                    MHE.Slow = ckSlow.Checked;
                    resp = MHE.GetStockResponse(iprod_items, 5);

                }
                else
                {
                    AgentHelper.Slow = ckSlow.Checked;
                    var req = new myNameSpace.readAllAMDV01Request();


                    if (UT.curMV.Technology == "SOAP")
                    {
                        log("  ...recupero catalogo prodotti in magazzino da servizio SOAP");
                        var cli = AgentHelper.WSSoapClient;
                        resp = cli.readAllAMDV01(req);
                    }
                    else if (UT.curMV.Technology == "REST")
                    {
                        log("  ...recupero catalogo prodotti in magazzino da servizio REST");
                        resp = GetStockREST(req).Result;
                    }
                    

                }


                if (resp is null) throw new Exception("readAllAMDV01 FALLITO: la funzione ha restituito il response = null");

                if (UT.NotNull(resp.@return.returnErrorMessage)) throw new Exception($"readAllAMDV01 FALLITO: la funzione ha restituito l'errore '{resp.@return.returnErrorMessage}'");
                if (resp.@return.article.Length == 0)
                {
                    setPB(0, 100);
                    SetNetStatus("LEDS-TIMER-OFF");

                    return true;
                }

                setPB(60, 100);
                sm = log($"             ...parsing response. {resp.@return.article.Length} articoli ricevuti da MP", true);

                // ok, articoli scaricati
                var listArt = resp.@return.article;

                int u = 0;
                var m = "#;cod;v.articleName;containerSize;containerSizeSpecified;compartmentNumber;compartmentDepthNumber";
                m += ";minimumInventory;fifo;inventoryAtStorageLocation";
                m += ";shelfNumber;liftNumber";
                m += ";special01;02;03;04;05;06;07;08;09;10;11;12;13;14;15;16;17;18;19;20;21;22;23;24;25";
                log(m);
                foreach (var art in listArt)
                {
                    u++;
                    dumpArt(art, u);
                }

                setPB(70,100);

                sm = log($"               ...allineo le giacenze su iProd.",true);
                log(" ", true);

                checkStockIprod(listArt);

                preload.Visible = false;

                sm = log($"GIAC - Richiesta completata", true);

            }
            catch (Exception ex)
            {

                SetNetStatus("LEDS-TIMER-OFF");

                sm = log("GIAC - Errore: " + ex.Message.Replace("\r\n", "") + ", " + ex.StackTrace.Replace("\r\n", ""), Forza);
                    if (!UT.Ask(sm)) return true;

            }


            return true;
        }

        bool EseguiRichiestaDelete(reqSchema rq)
        {

            try
            {
                outCSV = $"{UT.pathData}\\Requests\\DeleteJob_{DateTime.Now:HHmmssfff}.txt";

                UT.AppendToFile(outCSV, "JobName");

                var rk = new recordSchema("-");
                rk.Fields.Add(new fieldSchema("jobNumber", rq.baseTemplate)); // su basetemplate ci abbiamo messo il nome del job
                rq.records.Add(rk);

                string sm = log($"Avvio DeleteJob. file di output per verifiche: {outCSV}");
                if (VerboseMax)
                    if (!UT.Ask(sm)) return true;


                System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                log("Connessione a WS, setup action sendJobsReqV01 (Prelievi)"); // deleteJobReqV01
                var req = new myNameSpace.deleteAllAPDV01Request();
                var cli = AgentHelper.WSSoapClient;
                //  var cli = new myNameSpace.ComPortTypeClient();

                sm = log("Esegue richiesta di azzeramento job");
                if (VerboseMax)
                    if (!UT.Ask(sm)) return true;

                var resp = cli.deleteAllAPDV01(req);
                if (resp.@return.returnValue == 1)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {

                preload.Visible = false;
                var sm = log("Errore " + ex.Message + ", " + ex.StackTrace);
                if (VerboseMax)
                    MessageBox.Show(sm);
                return false;
            }
        }

        // questa in realta non viene mai eseguita per sono i tablets a richiederla ed è processata dentro ThreadPicker in AgentHelper
        bool EseguiRichiestaPrelievi(reqSchema rq)
        {

            try
            {
                outCSV = $"{UT.pathLog}\\Requests\\Lista_Prelievi_{DateTime.Now:HHmmssfff}_csv.txt";

                UT.AppendToFile(outCSV, "Codice;Prodotto;BOM;Qty;Operazione");


                string sm = log($"Avvio GET Lista Prelievi. file di output per verifiche: {outCSV}");
                if (VerboseMax)
                    if (!UT.Ask(sm)) return true;


                System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                log("Connessione a MP, setup action sendJobsReqV01 (Prelievi)");
                var req = new myNameSpace.sendJobsV01Request();
                var cli = AgentHelper.WSSoapClient;
                //   var cli = new myNameSpace.ComPortTypeClient();

                sm = log("Setup richiesta con articoli da prelevare");
                if (VerboseMax)
                    if (!UT.Ask(sm)) return true;


                req.param = new myNameSpace.JobTypeV01[1];

                var x = new myNameSpace.JobTypeV01();
                x.jobNumber = NameSurname;

                sm = log("Caricamento BOMs da iProd");
                if (VerboseMax)
                    if (!UT.Ask(sm)) return true;

                var jobs = new List<reqSchema>();
                var prelievi = new List<itemToGet>();

                setPB(40, 100);
                var CB = load_iProd("boms");

                rq.isPrelievo = true;
                rq.pickUpJobName = NameSurname; // utente connesso 

                sm = log($"{CB.RowCount} BOMs in memoria. Caricamento active-phaseinstances");
                if (VerboseMax)
                    if (!UT.Ask(sm)) return true;

                setPB(50, 100);
                var CPI = load_iProd("active-phaseinstances");


                sm = log($"Trovati {CPI.RowCount} elementi da prelevare. Generazione richiesta per ciascun item");
                if (VerboseMax)
                    if (!UT.Ask(sm)) return true;


                if (iprod_pi.Count == 0)
                {

                    sm = log("Nessun prelievo da eseguire");
                    if (VerboseMax)
                        if (!UT.Ask(sm)) return true;

                    UT.AppendToFile(outCSV, $"null;null;{curBom};0;NO-ACTIVE-PI");
                    preload.Visible = false;

                    return true;
                }

                string st = "";
             


                x.JobPosition = new myNameSpace.JobPositionTypeV01[iprod_pi.Count];
                int xi = 0;
                foreach (var pi in iprod_pi)
                {
                    st = pi.Linkeddata.Itemid;
                    var item = findItem(st);
                    if (item is null)
                    {

                        sm = log($"Articolo con id {st} non trovato");
                        //if (VerboseMax)
                        //    if (!UT.Ask(sm)) return;
                        UT.AppendToFile(outCSV, $"ID ITEM={st};null;{curBom};0;ITEM-NOT-FOUND");
                        continue;
                    }

                    prelievi = new List<itemToGet>();
                    prelievi = fetchItemsToGet(pi);
                    if (prelievi.Count == 0)
                    {

                        sm = log($"Richiesto prelievo per {item.name} ma non risultano giacenze consistenti per poter essere prelevato dal magazzino");
                        //if (VerboseMax)
                        //    if (!UT.Ask(sm)) return;

                        // generiamo comunque il record csv con qty = 0,  non lo invieremo a soap ma noi possiamo sapere cosa non ha trovato
                        UT.AppendToFile(outCSV, $"{item.code};{item.name};{curBom};0;ND");

                        continue;
                    }

                    var job = new reqSchema(item.code + " " + item.name, "", "");
                    job.isPrelievo = true;

                    sm = log($"Caricamento richiesta per n. {prelievi.Count} prelievi per il prodotto {item.code} {item.name} ");
                    if (VerboseMax)
                        if (!UT.Ask(sm)) return true;

                    foreach (var pr in prelievi)
                    {

                        item = findItem(pr.itemid);

                        sm = log($"da prelevare {pr.qty} pezzi di {item.name} per {curBom}");
                        //if (VerboseMax)
                        //    if (!UT.Ask(sm)) return;

                        int nl = x.JobPosition.Length;
                        sm = log($"JobPosition array of {nl} elements, corrente = {xi}");
                        //if (VerboseMax)
                        //    if (!UT.Ask(sm)) return;

                        x.JobPosition[xi] = new myNameSpace.JobPositionTypeV01();
                        var jb = x.JobPosition[xi];

                        //if (VerboseMax)
                        //    if (!UT.Ask(sm)) return;
                        // store cod o nome se null, se entrambi null non li gestisce e prosegue
                        if (UT.NotNull(item.code))
                            jb.articleNumber = item.code;
                        else if (UT.NotNull(item.name))
                            jb.articleNumber = item.name;
                        else
                            continue;

                        jb.operation = "-";
                        jb.nominalQuantity = $"{pr.qty:#.000}";

                        var rk = new recordSchema("JobPosition");
                        rk.Fields.Add(new fieldSchema("articleNumber", item.code));
                        rk.Fields.Add(new fieldSchema("operation", "-"));
                        rk.Fields.Add(new fieldSchema("nominalQuantity", $"{pr.qty:#.000}"));
                        job.records.Add(rk);
                        UT.AppendToFile(outCSV, $"{item.code};{item.name};{curBom};{pr.qty};Scarico");
                        xi++;
                        break; // ne puo gestire uno solo per articolo, di prelievi
                    }
                    jobs.Add(job);

                }

                setPB(0);

                sm = log("Richiesta pronta per l'invio al WS");
                if (VerboseMax)
                    if (!UT.Ask(sm)) return true;

                req.param[0] = x;
                var resp = cli.sendJobsV01(req);
                if (resp is null) throw new Exception("sendJobsV01 (Prelievi) non eseguito: la funzione ha restituito il response nullo");


                /*
                    var listaPrelievi = getXmlJobs(jobs);
                    SendToWS(listaPrelievi);   // <-- da qui la inviamo a SOAP aspettiamo il response e lo elaboriamo
                */
                preload.Visible = false;

                sm = log($"Il servizio richiesto si è concluso correttamente e senza errori. Generati {jobs.Count} elementi. Premi Ok per visualizzarne un riepilogo della richiesta");
                if (VerboseMax)
                    if (!UT.Ask(sm)) return true;

                UT.ShellExec(outF);
                UT.ShellExec(outCSV);
            }
            catch (Exception ex)
            {

                preload.Visible = false;
                var sm = log("Errore " + ex.Message + ", " + ex.StackTrace);
                if (Interactive)
                    MessageBox.Show(sm);

            }

            return true;
        }



        /// <summary>
        /// Controlla lo stato di MP richiedendo le giacenze e valuta i vari comportamenti 
        /// </summary>
        /// <returns></returns>
        ipDispatcher EseguiPingMP()
        {
            var ret = new ipDispatcher();
            var retByUser = new ipDispatcher();

            retByUser.StatusCode = "ABORTED";
            retByUser.Message = "L'utente ha scelto annulla a una conferma task";

            ret.Action = "ping";

            try
            {
                outCSV = $"{UT.pathData}Requests\\Ping_MP.txt";
                string sm = log($"Richiesto controllo operatività magazzino. File per le verifiche cronologiche: {outCSV}");
                if (VerboseMax)
                    if (!UT.Ask(sm)) return retByUser;


                System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                log("Connessione a MP..");
                var req = new myNameSpace.readAllAMDV01Request();
                var cli = AgentHelper.WSSoapClient;

                sm = log("Invio Ping..");
                if (VerboseMax)
                    if (!UT.Ask(sm)) return retByUser;

                var resp = cli.readAllAMDV01(req);

                if (resp is null)
                {
                    ret.StatusCode = "MUTE";
                    ret.Message = log("MP raggiungibile in rete ma non risponde alle richieste");
                }
                else if (UT.NotNull(resp.@return.returnErrorMessage))
                {
                    ret.StatusCode = "RESPONSEINERROR";
                    ret.Message = log("MP raggiungibile in rete ma alle richieste risponde con errore: " + resp.@return.returnErrorMessage);
                }
                else
                {

                    // ok, articoli scaricati
                    var listArt = resp.@return.article;

                    var nban = listArt.Select(a => a.shelfNumber).Distinct().Count();
                    sm = log($"MP Operativo. In uso {listArt.Length} scomparti e {nban} bancali.");
                    UT.AppendToFile(outCSV, $"{DateTime.Now} {sm}");
                    if (VerboseMax)
                        if (!UT.Ask(sm)) return retByUser;

                    ret.StatusCode = "ONLINE";
                    ret.Message = sm;

                }

                return ret;

            }
            catch (Exception ex)
            {

                var sm = log("Errore " + ex.Message.Replace("\r\n", "") + ", " + ex.StackTrace.Replace("\r\n", ""));
                if (VerboseMax)
                    if (!UT.Ask(sm)) return retByUser;

                ret.StatusCode = "OFFLINE";
                ret.Message = sm;
                log("MP Spento");
                UT.AppendToFile(outCSV, $"{DateTime.Now} MP OFFLINE: {sm}");
                return ret;
            }
        }



        public bool ExecuteProcess(string taskName)
        {

            doingtask = true;


            if (abort_requested)
            {
                SetNetStatus("LEDS-TIMER-OFF");

                aborted = true;
                doingtask = false;
                return false;
            }

            try
            {


                if(!UT.WSConnected)
                {
                    log($"Task '{taskName}' non eseguito per mancanza di connessione con il WS", true);
                    setSVCStatus("stopped");
                    return false;
                }

                VerboseMax = ckVerbose.Checked;

                System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                SetNetStatus("LEDS-TIMER-ON");



                setPB(20, 100);

                var rq = Requests.FirstOrDefault(x => x.dex == taskName);

                AgentHelper.Request = rq;




                // RICHIESTA GIACENZE
                if (rq.mainName == "readAllAMDReqV01") EseguiRichiestaGiacenze(rq);

                // LISTA PRELIEVO
                if (rq.mainName == "sendJobsReqV01") EseguiRichiestaPrelievi(rq);

                // DELETE JOB PRELIEVI
                if (rq.mainName == "deleteJobReqV01") EseguiRichiestaDelete(rq);

                // GET STATO MAGAZZINO (ON/OFF)
                if (rq.mainName == "MP-status")
                {
                    var dsp = EseguiPingMP();
                    log($"ping Response:");
                    log($"Action={dsp.Action}");
                    log($"StatusCode={dsp.StatusCode}");
                    log($"Message={dsp.Message}");
                    log($"Pronto.");
                }



                setPB(0);

                SetNetStatus("LEDS-TIMER-OFF");


                doingtask = false;

            }
            catch (Exception ex)
            {

                SetNetStatus("LEDS-TIMER-OFF");

                if (Interactive)
                    MessageBox.Show(log("Errore " + ex.Message + ", " + ex.StackTrace));


            }


            return true;
        }

        #endregion

        bool VerboseMax = false;
        bool Interactive = false;
    
 

        void dumpArt(myNameSpace.AMDTypeV01 v, int idx)
        {
            var m = $"{idx};{v.articleNumber};{v.articleName};{v.containerSize};{v.containerSizeSpecified}";
            m += $";{v.compartmentNumber};{v.compartmentDepthNumber};{v.minimumInventory};{v.fifo};{v.inventoryAtStorageLocation}";
            m += $";{v.shelfNumber};{v.liftNumber};{v.h01SpecialField};{v.h02SpecialField};{v.h03SpecialField};{v.h04SpecialField};{v.h05SpecialField}";
            m += $";{v.h06SpecialField};{v.h07SpecialField};{v.h08SpecialField};{v.h09SpecialField};{v.h10SpecialField};{v.h11SpecialField};{v.h12SpecialField};{v.h13SpecialField};{v.h14SpecialField};{v.h15SpecialField};{v.h16SpecialField}";
            m += $";{v.h17SpecialField};{v.h18SpecialField};{v.h19SpecialField};{v.h20SpecialField};{v.h21SpecialField};{v.h22SpecialField};{v.h23SpecialField};{v.h24SpecialField};{v.h25SpecialField}";
            log(m);
        }

        void checkStockIprod(myNameSpace.AMDTypeV01[] arts)
        {
            int pos = 0;
            if (arts is null) return;
            try
            {
                var sm = "";
               
                if (arts is null)
                {
                    sm = log($"Error: AMDTypeV01[] arts passed as Null. Aborted.");
                    return;
                }

                var ora = DateTime.UtcNow;
                var rigo = new StockData();
           

                var u = 0;
                foreach (var e in arts)
                {
                    pos = 1;
                    u++;
                    var LrcQty = Convert.ToDouble(e.inventoryAtStorageLocation);
                    var itm = iprod_items.FirstOrDefault(a => a.code == e.articleNumber || a.name == e.articleName
                                                      || a.name == e.articleNumber || a.code == e.articleName);
                    rigo = null;
                    // trovato l'articolo, quello in import 
                    if (itm != null)
                    {
                        pos = 2;
                        
                        if (itm.stockinformation is null) itm.stockinformation = new List<StockData>();
                        var stocks = itm.stockinformation;

                        if (stocks.Count == 0)
                        {
                            sm = log($"      ...item n.{u}-{e?.articleNumber} da allineare: giacenza iProd=0  MP={LrcQty:N3}");
                            UT.AddRowHist("STOCK", sm);
                            pos = 3;
                            itm.stockinformation.Add(newStockObj(LrcQty));
                            pos = 4;
                            saveItem(itm);
                            continue;
                        }
                        else
                        {
                            pos = 5;
                            bool equal = false;
                            bool found = false;
                            foreach (var stock in stocks)
                            {
                               
                                equal = false;
                                if (stock?.date.Date == ora.Date)
                                {
                                    rigo = stock;
                                    equal = true;
                                    break;
                                }
                                if (stock.date.Date > ora.Date)
                                {
                                    if (rigo is null) rigo = stock; // c'è un solo stock (rigo null) e successivo ad oggi.
                                    found = true;
                                    break; // la data supera oggi, il rigo prec è quello da comparare
                                }
                                rigo = stock;
                            }


                            pos = 6;
                            if (rigo != null)
                            {
                                
                                
                                if (equal)
                                {
                                    pos = 7;
                                    sm = log($"      ...item n.{u}-{e?.articleNumber} da allineare: giacenza iProd={rigo?.balance:N3}  MP={LrcQty:N3}");
                                    UT.AddRowHist("STOCK", sm);
                                    rigo.balance = LrcQty;
                                    rigo.inventory = true;
                                    rigo.lastupdate = DateTime.UtcNow.Date;

                                    saveItem(itm);
                                    continue;
                                }

                                if (found && LrcQty == rigo.balance)
                                {
                                    pos = 8;
                                    continue; // sono uguali, non deve fare niente 
                                }

                                if (found && LrcQty != rigo.balance)
                                {
                                    pos = 9;
                                    sm = log($"      ...item n.{u}-{e?.articleNumber} da allineare: giacenza iProd={rigo?.balance:N3}  MP={LrcQty:N3}");
                                    UT.AddRowHist("STOCK", sm);
                                    itm.stockinformation.Add(newStockObj(LrcQty)); // aggiunge lo stock ad oggi
                                    saveItem(itm);
                                    continue;
                                }
                                if (!found) // caso 3: tutte le date sono inferirio ad oggi, deve inserire la giacenza
                                {
                                    pos = 10;
                                    sm = log($"      ...item n.{u}-{e?.articleNumber} da allineare: giacenza iProd retrodatata set su MP={LrcQty:N3}");
                                    UT.AddRowHist("STOCK", sm);
                                    itm.stockinformation.Add(newStockObj(LrcQty)); // aggiunge lo stock ad oggi
                                    saveItem(itm);
                                }
                            }
                        }
                    }
                    else
                    {
                        sm = log($"      ...item n.{u}-{e?.articleNumber}  - {e?.articleName}  **WARNING**: articolo presente su MP non trovato su iProd", true);
                        UT.AddRowHist("STOCK-ERR", sm);
                    }
                }

                log("Allineamento completato");
            }
            catch (Exception ex)
            {
                var sm = log($"GIAC - **ERROR** a pos {pos}: {ex.Message.Replace("\r\n", "")}, {ex.StackTrace.Replace("\r\n", "")} ",true);
            }
        }


        StockData newStockObj(double qty)
        {
            string idm = "";
            if (iproduser?.warehouses?.Count > 0) idm = iproduser?.warehouses[0].ID;

            return new StockData
            {
                _id = MongoDB.Bson.ObjectId.GenerateNewId().ToString(),
                date = DateTime.UtcNow.Date,
                lastupdate = DateTime.UtcNow.Date,
                warehouseid = idm,
                inventory = true,
                balance = qty
            };
        }

        // salva la richiesta su disco e la restituisce al chiamante per l'invio al WS
        string GetRichiesta(reqSchema rq, string fout = "")
        {

            string f = $"{UT.pathLog}\\Requests\\{rq.dex.Replace(" ", "_")}_{DateTime.Now:HHmmssfff}_xml.txt";
            var xml = rq.getXML(UT.pathLog);
            if (UT.NotNull(fout)) f = fout;

            UT.AppendToFile(f, xml);
            return xml;
        }

        private void GetPIActive()
        {
            setPB(20, 100);
            load_iProd("items");
            setPB(75, 100);
            load_iProd("warehouses");
            setPB(100, 100);

            setPB(0, 0);

            load_iProd("active-phaseinstances");

            foreach (var pi in iprod_pi)
            {
                // per ogni pi va fatta una richiesta al service e invio lista prelievo
                // ** TODO **
            }
        }



        private bool refresh(bool fast = false)
        {
            Application.DoEvents();
            var ds = DateTime.Now;

            if (!fast)
            {
                do { Application.DoEvents(); } while ((DateTime.Now - ds).TotalMilliseconds < 200); // sleep .2 secondi
            }


            if (abort_requested)
            {
                abort_requested = false;
                aborted = true;
                timer1.Enabled = false;
                //   EnableAllButtons(true);

                timerstatus = $"Interrotto dall'utente. ultimo pool eseguito il {lastscan}";
                log(timerstatus);

                lbTimerStatus.Text = timerstatus;
                doingtask = false;
                inprogress = false;
                //    preload.Visible = false;
                PB1.Value = 0;
                return false; // l'utente ha chiesto di stoppare il processo
            }
            return true;

        }




        /// <summary>
        /// Set icona terminalini, se connesso è pulita, o con una x rossa se fallito
        /// </summary>
        /// <param name="what"></param>
        public void SetNetStatus(string what, string tip = "")
        {
           // log($"%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%   command --->   '{what}'  ");

            // iprod

            if (what == "IP-OFF" || what == "IP-OFFLINE" )
            {
                LEDiProdON.ImageKey = "LEDGOFF";
                LedIP.ImageKey = "LEDGOFF";
                LedSQL.ImageKey = "LEDGOFF";
                toolTip1.SetToolTip(LEDiProdON, tip);
            }
            if (what == "IP-ON" || what == "IP-ONLINE")
            {
                LEDiProdON.ImageKey = "LEDGON";
                toolTip1.SetToolTip(LEDiProdON, tip);
            }
            if (what == "IP-ERR")
            {
                LEDiProdON.ImageKey = "LEDRON";
                LedIP.ImageKey = "LEDGOFF";
                LedSQL.ImageKey = "LEDGOFF";
                toolTip1.SetToolTip(LEDiProdON, tip);
                SetNetStatus("LEDS-TIMER-OFF");
            }


            // WS

            if (what == "WS-OFF" || what == "WS-OFFLINE")
            {
                LEDWSOn.ImageKey = "LEDGOFF";
                LedIP.ImageKey = "LEDGOFF";
                LedSQL.ImageKey = "LEDGOFF";
                toolTip1.SetToolTip(LEDWSOn, tip);
            }
            if (what == "WS-ERR")
            {
                LEDWSOn.ImageKey = "LEDRON";
                LedIP.ImageKey = "LEDGOFF";
                LedSQL.ImageKey = "LEDGOFF";
                toolTip1.SetToolTip(LEDWSOn, tip);
                SetNetStatus("LEDS-TIMER-OFF");
            }



            // STOCK

            if (what == "STK-OFF" || what == "STK-OFFLINE")
            {
                LEDStock.ImageKey = "LEDGOFF";
                LedIP.ImageKey = "LEDGOFF";
                LedSQL.ImageKey = "LEDGOFF";
                toolTip1.SetToolTip(LEDStock, tip);
            }
            if (what == "STK-ERR")
            {
                LEDStock.ImageKey = "LEDRON";
                LedIP.ImageKey = "LEDGOFF";
                LedSQL.ImageKey = "LEDGOFF";
                toolTip1.SetToolTip(LEDStock, tip);
                SetNetStatus("LEDS-TIMER-OFF");
            }



            // small leds trasmissions
            if (what.ToUpper() == "IDLE" || what == "ONLINE" || what == "ON")
            {
                LedIP.ImageKey = "LEDGOFF";
                LedSQL.ImageKey = "LEDGOFF";
            }


            if (what == "SEND")
            {
                LedIP.ImageKey = "LEDGON";
                LedSQL.ImageKey = "LEDGOFF";
            }
            if (what == "REC")
            {
                LedIP.ImageKey = "LEDGOFF";
                LedSQL.ImageKey = "LEDGON";
            }


            // timerLEDS
            if (what == "LEDS-TIMER-ON")
            {
                timerLEDS.Enabled = true;
                preload.Visible = true;
            }
            if (what == "LEDS-TIMER-OFF")
            {
                timerLEDS.Enabled = false;
                preload.Visible = false;
                LedIP.ImageKey = "LEDGOFF";
                LedSQL.ImageKey = "LEDGOFF";
            }

            Application.DoEvents();


        }



        public string log(Exception ex, string func = "")
        {
            // if (!ckLogGiac.Checked && !forza && !inDebug) return m;

            // standard log, semplifica la codifica

            string m = $"Rilevata Eccezione in {func}: '{ex.Message}', stack '{ex.StackTrace}'";

            UT.WriteToEventLog(this, m);
            Application.DoEvents();
            return m;
        }


        public string log(string m, bool forza = true)
        {

            // standard log, semplifica la codifica
            UT.WriteToEventLog(this, m, forza);
            Application.DoEvents();
            return m;
        }

         
        void setFASE(string v = "SYS")
        {
            string space = "                     ";

            var i = v.Length;
            if (i > 10) { FASE = v.Substring(0, 10); return; }
            i = 10 - i;
            FASE = v + space.Substring(0, i);
        }

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

            if (!refresh()) return CC; // abort *******************************************************************************

            FASE = "LGET - ";
            
            try
            {
                var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(UT.EndPointIPROD);





                curWhat = prevWhat;

                /* ==============================================================================================================

                                CARICAMENTO OGGETTI iProd. 
                            La sorgente sono le API dell'applicazione su Azure

                   ==============================================================================================================*/

                // scarica la lista richiesta 


                switch (curWhat)
                {
                    case "customers":

                        setFASE("CLI");

                        //preload.Top = wheel.Clienti;
                        //preload.Visible = true;

                        // GET Clienti

                        isEmpty = (iprod_customers is null || iprod_customers.Count == 0);
                        if (!isEmpty)
                        {
                            CC.RowCount = iprod_customers.Count;
                            CC.ProcessCompleted(true, true);
                            return CC;
                        }

                        iprod_customers = new List<Customers>();

                        log($"{FASE}download Clienti in corso..");

                        dataparameter = "Customers/GetCustomersTable?token=" + Program.ipTOKEN;
                        content = httpClient.GetAsync(dataparameter).Result;
                        customerdata = content.Content.ReadAsStringAsync().Result;

                        if (content.StatusCode != isOk)
                        {
                            CC.Witherror++;

                            msg = content.Content.ReadAsStringAsync().Result;
                            UT.httpErr objErr = JsonConvert.DeserializeObject<UT.httpErr>(msg);
                            log($"{FASE}Wrong API response. download iprod_customers, Err Code: {objErr.status} - {content.StatusCode}, msg: {objErr.title}, API: {dataparameter} ");
                            if (iprod_customers is null) iprod_customers = new List<Customers>();
                        }
                        else
                            iprod_customers = JsonConvert.DeserializeObject<List<Customers>>(customerdata);

                        txtContatore = $"Scaricati da iProd {iprod_customers.Count} clienti ";
                        CC.read = iprod_customers.Count;

                        break;

                    //  I T E M S =================================================================================================================================

                    case "items":

                        setFASE("ITEMS");


                        Application.DoEvents();

                        isEmpty = (iprod_items is null || iprod_items.Count == 0);
                        if (!isEmpty)
                        {
                            CC.RowCount = iprod_items.Count;
                            CC.ProcessCompleted(true, true);
                            return CC;
                        }

                        iprod_items = new List<Items>();

                        log($"{FASE}download Articoli in corso..");

                        // GET Articoli
                        dataparameter = "Items/GetItemsTable?token=" + Program.ipTOKEN;
                        content = httpClient.GetAsync(dataparameter).Result;
                        customerdata = content.Content.ReadAsStringAsync().Result;

                        if (content.StatusCode != isOk)
                        {
                            CC.Witherror++;

                            msg = content.Content.ReadAsStringAsync().Result;
                            UT.httpErr objErr = JsonConvert.DeserializeObject<UT.httpErr>(msg);
                            log($"{FASE}Wrong API response. Err Code: {objErr.status} - {content.StatusCode}, msg: {objErr.title}, API: {dataparameter} ");
                            iprod_items = new List<Items>();
                        }
                        else
                            iprod_items = JsonConvert.DeserializeObject<List<Items>>(customerdata);

                        txtContatore = $"Scaricati da iProd {iprod_items.Count} Prodotti ";
                        CC.read = iprod_items.Count;
                        break;

                    // P H A S E I N S T A N C E S *****************************************************************************************************************************************************

                    case "phaseinstances":

                        setFASE("PI");


                        Application.DoEvents();

                        isEmpty = (iprod_pi is null || iprod_pi.Count == 0);

                        iprod_pi = new List<PhaseInstance>();

                        log($"{FASE}  download Istanze di fase (tutte) in corso..");

                        // GET Phaseinstances table
                        dataparameter = "PhasesInstances/GetPhaseInstancesTable?token=" + Program.ipTOKEN;
                        content = httpClient.GetAsync(dataparameter).Result;
                        customerdata = content.Content.ReadAsStringAsync().Result;

                        if (content.StatusCode != isOk)
                        {
                            CC.Witherror++;

                            msg = content.Content.ReadAsStringAsync().Result;
                            UT.httpErr objErr = JsonConvert.DeserializeObject<UT.httpErr>(msg);
                            log($"{FASE}Wrong API response. Err Code: {objErr.status} - {content.StatusCode}, msg: {objErr.title}, API: {dataparameter} ");
                            iprod_pi = new List<PhaseInstance>();
                        }
                        else
                            iprod_pi = JsonConvert.DeserializeObject<List<PhaseInstance>>(customerdata);

                        txtContatore = $"Scaricati da iProd {iprod_pi.Count} istanze di fase ";
                        CC.read = iprod_pi.Count;
                        break;


                    // ACTIVE PHASEINSTANCES *****************************************************************************************************************************************************

                    case "active-phaseinstances":

                        setFASE("PI-ACTIVE ");

                        Application.DoEvents();

                        isEmpty = (iprod_pi is null || iprod_pi.Count == 0);

                        iprod_pi = new List<PhaseInstance>();

                        log($"{FASE}  download Istanze di fase attive in corso..");

                        // GET Articoli
                        dataparameter = "PhasesInstances/GetPhaseInstanceActive?token=" + Program.ipTOKEN;
                        content = httpClient.GetAsync(dataparameter).Result;
                        customerdata = content.Content.ReadAsStringAsync().Result;

                        if (content.StatusCode != isOk)
                        {
                            CC.Witherror++;

                            msg = content.Content.ReadAsStringAsync().Result;
                            UT.httpErr objErr = JsonConvert.DeserializeObject<UT.httpErr>(msg);
                            log($"{FASE}Wrong API response. Err Code: {objErr.status} - {content.StatusCode}, msg: {objErr.title}, API: {dataparameter} ");
                            iprod_pi = new List<PhaseInstance>();
                        }
                        else
                            iprod_pi = JsonConvert.DeserializeObject<List<PhaseInstance>>(customerdata);

                        if (iprod_pi.Count == 0)
                        {
                            txtContatore = $"iProd non ha fornito nessuna istanza di fase attiva per richiedere le liste di prelievo. Elaborazione interrotta.";
                            aborted = true;
                            break;
                        }
                        txtContatore = $"Scaricati da iProd {iprod_pi.Count} istanze di fase ";
                        CC.read = iprod_pi.Count;
                        CC.RowCount = iprod_pi.Count;
                        break;


                    // P H A S E S  ==================================================================================================================================


                    case "phases":

                        setFASE("FASE");


                        // GET fasi (Phases) e macchines

                        isEmpty = (iprod_phases is null || iprod_phases.Count == 0);
                        if (!isEmpty)
                        {
                            CC.RowCount = iprod_phases.Count;
                            CC.ProcessCompleted(true, true);
                            return CC;
                        }
                        iprod_phases = new List<Phase>();

                        log($"{FASE}  download Fasi in corso..");

                        dataparameter = "Phases/GetPhaseTable?token=" + Program.ipTOKEN;
                        content = httpClient.GetAsync(dataparameter).Result;
                        customerdata = content.Content.ReadAsStringAsync().Result;
                        if (content.StatusCode != isOk)
                        {
                            CC.Witherror++;

                            msg = content.Content.ReadAsStringAsync().Result;
                            UT.httpErr objErr = JsonConvert.DeserializeObject<UT.httpErr>(msg);
                            log($"{FASE}Wrong API response. carica iprod_phases, Err Code: {objErr.status} - {content.StatusCode}, msg: {objErr.title}, API: {dataparameter} ");
                            iprod_phases = new List<Phase>();
                        }
                        else
                            iprod_phases = JsonConvert.DeserializeObject<List<Phase>>(customerdata);

                        txtContatore = $"Scaricati da iProd {iprod_phases.Count} Fasi ";
                        CC.read = iprod_phases.Count;
                        break;

                    // P O S T S ==================================================================================================================================

                    case "posts":

                        setFASE("POSTS");

                        isEmpty = (iprod_posts is null || iprod_posts.Count == 0);
                        if (!isEmpty)
                        {
                            CC.RowCount = iprod_posts.Count;
                            CC.ProcessCompleted(true, true);
                            return CC;
                        }
                        iprod_posts = new List<Posts>();

                        log($"{FASE}download Posts di tipo 1 in corso..");

                        if (UT.IsNull(tipoPost)) tipoPost = "1";
                        dataparameter = $"Posts/GetPosts?token={Program.ipTOKEN}&type={tipoPost}";
                        content = httpClient.GetAsync(dataparameter).Result;
                        customerdata = content.Content.ReadAsStringAsync().Result;
                        if (content.StatusCode != isOk)
                        {
                            CC.Witherror++;

                            msg = content.Content.ReadAsStringAsync().Result;
                            UT.httpErr objErr = JsonConvert.DeserializeObject<UT.httpErr>(msg);
                            log($"{FASE}Wrong API response. carica iprod_phases, Err Code: {objErr.status} - {content.StatusCode}, msg: {objErr.title}, API: {dataparameter} ");
                            iprod_posts = new List<Posts>();
                        }
                        else
                            iprod_posts = JsonConvert.DeserializeObject<List<Posts>>(customerdata);



                        txtContatore = $"Scaricati da iProd {iprod_posts.Count} POST ";
                        CC.read = iprod_posts.Count;

                        break;


                    // C A T E G O R I E S  ==================================================================================================================================

                    case "categories":

                        setFASE("CAT");


                        isEmpty = (categories is null || categories.Count == 0);
                        if (!isEmpty)
                        {
                            CC.RowCount = categories.Count;
                            CC.ProcessCompleted(true, true);
                            return CC;
                        }
                        categories = new Dictionary<string, string>();


                        log($"{FASE}download categorie in corso..");


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
                            categories = JsonConvert.DeserializeObject<Dictionary<string, string>>(customerdata);


                        txtContatore = $"Scaricate da iProd {categories.Count} categorie ";

                        break;

                    // W A R E H O U S E S  ==================================================================================================================================


                    case "warehouses":

                        setFASE("MAG");


                        isEmpty = (warehouses is null || warehouses.Count == 0);
                        if (!isEmpty)
                        {
                            CC.RowCount = warehouses.Count;
                            CC.ProcessCompleted(true, true);
                            return CC;
                        }
                        warehouses = new List<Warehouse>();


                        log($"{FASE}download Magazzini in corso..");

                        warehouses = new List<Warehouse>();

                        dataparameter = "WareHouses/GetWareHousesTable?token=" + Program.ipTOKEN;
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
                            warehouses = JsonConvert.DeserializeObject<List<Warehouse>>(customerdata);

                        txtContatore = $"Scaricati da iProd {warehouses.Count} Magazzini ";
                        CC.read = warehouses.Count;
                        break;

                    // M A C H I N E S ==================================================================================================================================


                    case "machines":

                        setFASE("MACH");

                        isEmpty = (iprod_machines is null || iprod_machines.Count == 0);
                        if (!isEmpty)
                        {
                            CC.RowCount = iprod_machines.Count;
                            CC.ProcessCompleted(true, true);
                            return CC;
                        }
                        iprod_machines = new List<Customermachine>();

                        log($"{FASE}download Macchinari in corso..");

                        dataparameter = "Machines/GetMachineTable?token=" + Program.ipTOKEN;
                        content = httpClient.GetAsync(dataparameter).Result;
                        customerdata = content.Content.ReadAsStringAsync().Result;
                        if (content.StatusCode != isOk)
                        {
                            CC.Witherror++;

                            msg = content.Content.ReadAsStringAsync().Result;
                            UT.httpErr objErr = JsonConvert.DeserializeObject<UT.httpErr>(msg);
                            log($"{FASE}Wrong API response. carica MACHINES, Err Code: {objErr.status} - {content.StatusCode}, msg: {objErr.title}, API: {dataparameter} ");
                            iprod_machines = new List<Customermachine>();
                        }
                        else
                            iprod_machines = JsonConvert.DeserializeObject<List<Customermachine>>(customerdata);

                        txtContatore = $"Scaricati da iProd {iprod_machines.Count} macchinari ";
                        CC.read = iprod_machines.Count;
                        break;

                    // B O M S ==================================================================================================================================


                    case "boms":
                        setFASE("BOMS");

                        isEmpty = (iprod_boms is null || iprod_boms.Count == 0);
                        if (!isEmpty)
                        {
                            CC.RowCount = iprod_boms.Count;
                            CC.ProcessCompleted(true, true);
                            return CC;
                        }
                        iprod_boms = new List<Bom>();

                        log($"{FASE}download Distinte in corso..");

                        dataparameter = "Boms/GetBomsTable?token=" + Program.ipTOKEN;
                        content = httpClient.GetAsync(dataparameter).Result;
                        customerdata = content.Content.ReadAsStringAsync().Result;

                        if (content.StatusCode != isOk)
                        {
                            CC.Witherror++;
                            msg = content.Content.ReadAsStringAsync().Result;
                            UT.httpErr objErr = JsonConvert.DeserializeObject<UT.httpErr>(msg);
                            log($"{FASE}Wrong API response. GetBomsTable, Err Code: {objErr.status} - {content.StatusCode}, msg: {objErr.title}, API: {dataparameter} ");
                            iprod_boms = new List<Bom>();
                        }
                        else
                            iprod_boms = JsonConvert.DeserializeObject<List<Bom>>(customerdata);

                        txtContatore = $"Scaricati da iProd {iprod_boms.Count}  Boms (distinte basi) ";
                        CC.read = iprod_boms.Count;
                        break;

                    default:
                        throw new Exception($"Attenzione, parametro 'what' sconosciuto: {what}");
                }

                //if (txtContatore != "")
                //{
                //    ctemp = UT.ElapsedTimeToString(DateTime.Now, dStart, true);
                //    log($"{FASE}{txtContatore} in {ctemp} ");
                //}

                //setFASE();
                CC.RowCount = Convert.ToInt32(CC.read);
                log(CC.ProcessCompleted(true, true));
                return CC;
            }
            catch (Exception ex)
            {
                string mm = $"load_iprod('{curWhat}') Err: " + ex.Message;
                UT.WriteErrFile(mm);
                log(mm);
                throw ex;
               // return CC;
            }
        }


        private void btTestConnWS_Click(object sender, EventArgs e)
        {
            AgentHelper.Request = new reqSchema();
            AgentHelper.Request.Offline = false;
            testWSConnection(false);
        }

        private void btTestConnAPI_Click(object sender, EventArgs e)
        {
            //testAPIConnection(false);
        }

        private void apriToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UT.ShellExec(UT.pathLog + "\\config.txt");
        }

        private void utentiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UT.ShellExec(UT.pathLog + "\\utenti.txt");
        }

        private void apriFileDiLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UT.ShellExec(UT.LogFile);
        }

        private void apriCartellaDiLavoroToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UT.ShellExec(UT.pathLog);
        }

        void AvviaTimer()
        {
            try
            {
                timer1.Enabled = true;

                lastscan = DateTime.Now;
                startSleep = lastscan;


                if (!aborted)
                {
                    timer1.Enabled = true;
                }
                else
                {
                    timer1.Enabled = false;
                    aborted = false;
                    abort_requested = false;
                    return;
                }

                Listener_start();
                log("****");
                ExecuteProcess("Giacenze");


            }
            catch (Exception ex)
            {
                var st = "LSTNR - Errore all'avvio del servizio: Messaggio di errore: " + ex.Message;
                log(st);
                MessageBox.Show(st);

            }
        }


        bool Listener_start()
        {
            if (ThreadListener != null) return true;
            if (!UT.WSConnected)
            {
                log("Non è possibile avviare il Listener per mancanza di collegamento con il WS del M.V.", true);
                setSVCStatus("stopped");
                return false;
            }

            //SetNetStatus("LEDS-TIMER-ON");

            ThreadListener = new Thread(new ThreadStart(ListenerBackGroundWorker));
            ThreadListener.IsBackground = true;
            ThreadListener.Start();
            timer3.Enabled = true; // avvia lo scambiatore di log, dal thread del listener al log del form

            return true;
        }

        bool Listener_stop()
        {
            if (ThreadListener is null) return false;
            if (ThreadListener.IsAlive)
            {
                ThreadListener.Abort();
                log("LSTRN - Il servizio listener è stato fermato.", Forza);
            } 
            else
            {
                log("LSTRN - Tentativo di arresto servizio listener mentre non era attivo", Forza);
            }

            ThreadListener = null;
            timer3.Enabled = false;

            setPB(0);

            LEDLSTNR.ImageKey = "LEDROFF";
            SetNetStatus("LEDS-TIMER-OFF");


            return true;
        }

        public void setSVCStatus(string how)
        {

            cbControlProcess.Items.Clear();
            if (how == "active")
            {
                SetNetStatus("LEDS-TIMER-ON");

                SetNetStatus("STK-ONLINE", "Stock timer running");
                SVCstatus[0].Text = "In Esecuzione";

                foreach (var sy in SVCstatus)
                    cbControlProcess.Items.Add(sy);
            }
            else if (how == "paused")
            {
                SetNetStatus("LEDS-TIMER-OFF");

                foreach (var sy in SVCpaused)
                    cbControlProcess.Items.Add(sy);
            }
            else
            {
                SetNetStatus("LEDS-TIMER-OFF");

                cbControlProcess.Items.Add(new ImageComboBox.ImageComboBoxItem(10, "Fermo", 0));
                cbControlProcess.Items.Add(new ImageComboBox.ImageComboBoxItem(9, "Avvia", 0));
                UT.SVCPausedUntil = "Inattivo.";
            }


            cbControlProcess.SelectedIndex = 0;

            lblSync.Text = UT.SVCPausedUntil;
            UT.SVCStatus = how;
        }


        bool Listener_restart()
        {
            return Listener_stop() && Listener_start();
        }


     
        private void btHelp_Click(object sender, EventArgs e)
        {
            var txtf = UT.pathLog + "\\help-agent.txt";
            UT.ShellExec(txtf);

        }


        #region COMUNICAZIONI/TRANSAZIONI DA E VERSO IPROD



        private bool saveItem(Items item, bool isNew = false)
        {
            string sm = "";
            
            if(UT.MockWS)
            {
            //    var sm = log($"     ...simulo il salvataggio di {item.code}");
                return true;
            }


            try
            {
                SetNetStatus("LEDS-TIMER-ON");

                var json = JsonConvert.SerializeObject(item);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var dataparameter = "Items/SaveProduct?token=" + Program.ipTOKEN;
                log($"call API '{dataparameter}'");
                var resp = UT.httppostcall(dataparameter, content);
                log("API 'Items/SaveProduct' completed.");
                if (resp.StatusCode != isOk)
                {
                    var msg = resp.Content.ReadAsStringAsync().Result;
                    UT.httpErr objErr = JsonConvert.DeserializeObject<UT.httpErr>(msg);

                    sm = log($"***Errore Bad Request*** Err Code: {objErr.status} - {resp.StatusCode}, msg: {objErr.title}, API Items/SaveProduct");
                    SetNetStatus("LEDS-TIMER-OFF");

                    return false;
                }

                SetNetStatus("LEDS-TIMER-OFF");
                return true;

            }
            catch (Exception ex)
            {
                SetNetStatus("LEDS-TIMER-OFF");

                sm = log($"**ERRORE** Rilevata Eccezione durante il salvataggio: {ex.Message.Replace("\r\n", "")} - {ex.StackTrace.Replace("\r\n", "")}");
                return false;
            }
        }




        #endregion



        void CalcolaPausa(string s)
        {
            if (!UT.iProdConnected) return;

            var t = new TimeSpan();

            UT.SVCWaitTo = new DateTime(1970, 1, 1);

            if (s.Contains("30 minuti")) t = new TimeSpan(0, 30, 0);
            if (s.Contains("1 ora")) t = new TimeSpan(1, 0, 0);
            if (s.Contains("3 ore")) t = new TimeSpan(3, 0, 0);
            if (s.Contains("domani")) t = new TimeSpan(24, 0, 0);
            if (s.Contains("riavvio utente"))
            {
                UT.SVCPausedUntil = "Inattivo";
                timer1.Enabled = false;
                return;
            }
            if (s == "Avvia")
            {
                int sec = Convert.ToInt32(iProdCFG.TimerInterval);
                UT.SVCPausedUntil = $"Attivo";
                timer1.Enabled = false;
                timer1.Interval = 1000 * sec;
                timer1.Enabled = true;
                var dd = DateTime.Now + new TimeSpan(0, 0, sec);
                UT.SVCPausedUntil = $"Prossimo pool alle ore {dd:HH:mm:ss}";
                UT.SVCWaitTo = dd;
            }
            else if (s == "Fermo")
            {
                UT.SVCPausedUntil = $"Inattivo";
                timer1.Enabled = false;
            }
            else
            {
                var d = DateTime.Now + t;
                UT.SVCPausedUntil = $"Fino al {d:dd/MM alle ore HH:mm:ss}";
                UT.SVCWaitTo = d;
            }

            lblSync.Text = UT.SVCPausedUntil;


        }




        //  bool Notte = false;
        bool forzaExitApp = false;
 
        string SVCState = "wait";
        private void timer1_Tick(object sender, EventArgs e)
        {

            try
            {

                if (aborted || abort_requested)
                {
                    log("Interruzione richiesta dall'utente");

                    timer1.Enabled = false;
                    aborted = false;
                    abort_requested = false;

                    UT.CompactMemory();

                    return;
                }


                if (DateTime.Now < UT.SVCWaitTo)
                {
                    return;
                }
                else
                {
                    if (SVCState == "wait")
                    {
                        setSVCStatus("active");
                        SVCState = "active";
                    }
                }

                if (forzaExitApp)
                {
 
                    Application.Exit();
                }

                if (inprogress == false)
                {
                    while (doingtask == true)
                    {
                        Application.DoEvents();
                    }

               

                    log($"Avviato servizio alle ore {DateTime.Now:hh:mm:ss}");
                    preload.Visible = true;

                    var dStart = DateTime.Now;
                    started = dStart;
                    UT.cntGlobale.dStart = dStart;
              
                    ExecuteProcess("Giacenze");

                    CalcolaPausa("Attivo");

                    UT.Interactive = true;

                    while (doingtask == true)
                    {
                        Application.DoEvents();
                    }

                    inprogress = false;

                    var ctemp = UT.ElapsedTimeToString(DateTime.Now, dStart, true);
                    log($"Processo eseguito in {ctemp}");

                    setPB(0);

                }

            }
            catch (Exception ex)
            {

                preload.Visible = false;
                if (Interactive)
                    MessageBox.Show(log("Errore " + ex.Message + ", " + ex.StackTrace));

            }
        }

    
        bool sem3 = false;
        private void timer3_Tick(object sender, EventArgs e)
        {

            // legge il file di log del thread parallelo Llistener
            // lo aggiunge al log su interfaccia e azzera il file 

            if (sem3) return;
            sem3 = true;

            var fc = UT.pathLog + "\\listenerLog.txt";

            if (!File.Exists(fc))
            {
                sem3 = false;
                return;
            }
            var lstc = UT.LoadTextFile(fc);

            foreach (var cmd in lstc)
            {
                if (cmd.StartsWith("@CMD@"))
                    parseCommand(cmd);
                else
                    log(cmd, Forza);
            }

            // se non riesce a sbloccarlo non prova nemmeno a cancellarlo in quanto andrebbe in eccezione con interruzione servizio
            if (!UT.FileWaitLockedFile(fc, 3000))
            {
                log("Scaduto il tmpo attesa file locked");
                sem3 = false;
                return;
            }

            
            File.Delete(fc);

            sem3 = false;

        }

        void parseCommand(string cmd)
        {

        //    log(cmd, true);

            var ar = cmd.Split('|');
            if (ar.Length < 1) return;
            

            switch (ar[1])
            {
                case "PERCENT":
                    int p = Convert.ToInt32(ar[2]);
                    if (ar.Length>3)
                    {
                        int max = Convert.ToInt32(ar[3]);
                        setPB(p, max);
                    } else
                        setPB(p);

                    break;
                case "STOP LSTRN":
                    Listener_stop();
                    break;
                case "START LSTRN":
                    LEDLSTNR.ImageKey = "LEDGON";
                    setPB(0);
                    break;
                default:
                    break;
            }

        }

     

        void ClearLog()
        {
            var f1 = UT.pathLog + "\\LirecoLog.txt";
            var f2 = UT.pathLog + "\\listenerLog.txt";

            var r1 = UT.CopySafe(f1, UT.pathBackups, true, 1000);
            var r2 = UT.CopySafe(f2, UT.pathBackups, true, 1000);

            txtlogger.Text = "";

            if (r1)
                log("log salvato in backup e svuotato");
        }

        private void btRestart_Click(object sender, EventArgs e)
        {

            
        }

        private void btClearLog_Click(object sender, EventArgs e)
        {
           
        }

        private void apriFilePingMPtxtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var t = $"{UT.pathLog}\\Requests\\Ping_MP.txt";
            UT.ShellExec(t);
        }

        private void apriLirecoLogtxtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var t = $"{UT.pathLog}\\LirecoLog.txt";
            UT.ShellExec(t);
        }



        public async Task<bool> Autentica_iProd()
        {
            if (UT.iProdConnected) return true;

            //Program.ipUSER= "fabio.guerrazzi@iprod.it";
            //Program.ipPWD="4321";

            SetNetStatus("LEDS-TIMER-ON");

            var spl = new Splash();
            spl.Show();

            var msg = await UT.iProdLogin(Program.UrlGate.api, Program.ipUSER, Program.ipPWD);
            spl.Close();
            spl.Dispose();
            spl = null;

            if (!UT.IsNull(msg.status))
            {
                if (msg.status != "OK")
                {
                    UT.MsgBox(log("iProd: Utente o passord errati o connessione assente"), "Accesso a sistema iProd", "e");
                    var gf = new frmLogin();
                    gf.Show();
                    SetNetStatus("LEDS-TIMER-OFF");

                    return false;
                }
            }


            SetNetStatus("LEDS-TIMER-OFF");

            if (!UT.iProdConnected) return false;

            iproduser = UT.Tenant;
          
            iprod_loggeduser = iproduser.Customerusers.FirstOrDefault(x => x.Username == Program.ipUSER);
            if (iprod_loggeduser is null) return false;

            UT.iprod_loggeduser = iprod_loggeduser;

            toolTip1.SetToolTip(btLogin,
                $"Connesso come {UT.LF}{iprod_loggeduser.Username} {UT.LF}{iprod_loggeduser.Name} {iprod_loggeduser.Surname} {UT.LF}{iproduser.Customerdata.Name}");



            return true;
        }


        private async void tmrAutoStart_Tick(object sender, EventArgs e)
        {

            tmrAutoStart.Enabled = false;

            if (UT.connecting) return;

            if (UT.stringLogin == "SKIP")
            {
                UT.iProdConnected = true;
                return;
            }


            if (!UT.iProdConnected)
            {
                var ok = await Autentica_iProd();
                if (!ok)
                {
                    UT.iProdConnected = false;
                    var fs = new frmLogin();
                    fs.Show();
                }

            }

            string ipc = "Connessione a iProd FALLITA - OFFLINE ";
            

            // ok, possiamo procedere

            if (UT.iProdConnected)
            {

                if (AgentHelper.ConnectToWS())
                {
                    LEDWSOn.ImageKey = "LEDGON";
                    lbEsitoTestConnessione.Text = "Connesso";
                    string wsc = $"Connessione a {iProdCFG.MP_Active} OK";
                    if (!UT.WSConnected) wsc = iProdCFG.MP_Active + " non raggiungibile - OFFLINE";
                    log(wsc);
                }


                //                timerLEDS.Enabled = true;

                Text += " " + iproduser.Customerdata.Name;

                ipc = $"Connessione a iProd effettuata. Tenant '{iproduser.Customerdata.Name}'";

                string imgFile = UT.pathData + "image_avatar.png";

                try
                {
                    MemoryStream _file = await UT.GetAvatar();
                    _file.Position = 0;
                    FileStream newimage = new FileStream(imgFile, FileMode.Create);
                    _file.CopyTo(newimage);
                    newimage.Close();
                }
                catch { }

                btLogin.Image = Image.FromFile(imgFile);
                SetNetStatus("IP-ONLINE", "iProd CONNESSO");
            }

            log($"      {ipc}", true);
            log(" ", true);

            Listener_start();

            //  AvviaTimer();
        }

        private void cbotenant_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btHist_CheckedChanged(object sender, EventArgs e)
        {
            pnlHist.Visible = true;
            pnlEvents.Visible = false;

            btHist.Font = FontBold;
            btEvents.Font = FontNormal;
        }

        private void btEvents_CheckedChanged(object sender, EventArgs e)
        {
            pnlHist.Visible = false;
            pnlEvents.Visible = true;

            btHist.Font = FontNormal;
            btEvents.Font = FontBold;
        }

        private void btCFG_Click(object sender, EventArgs e)
        {
            pnlCFG.Visible = true;
            panelHome.Visible = false;
        }

        private void btSync_Click(object sender, EventArgs e)
        {
            pnlCFG.Visible = false;
            panelHome.Visible = true;

        }

        private void btSaveCFG_Click(object sender, EventArgs e)
        {
            if(
                !UT.isNumeric(txtMaxHist.Text) ||
                 !UT.isNumeric(txtautoupdate.Text) ||
                 !UT.isNumeric(txtPort.Text) 
               )
            {
                UT.MsgBox("044 - Valore previsto come numerico non valido. Configurazione non salvata","ATTENZIONE","e");
                return;
            }

            if (txtMaxHist.Text.IsNull()) iProdCFG.MaxHistCount = 150;

                if (txtIP.Text.IsNull())
            {
                UT.MsgBox("045 - Indirizzo IP/Nome Host Web Service (Magazzino V.) mancante. Configurazione non salvata", "ATTENZIONE!","e");
                return;
            }

            if (txtPort.Text.IsNull())
            {
                UT.MsgBox("046 - Porta Host Web Service (Magazzino V.) mancante. Configurazione non salvata", "ATTENZIONE!", "e");
                return;
            }

            if (txtUser.Text.IsNull())
            {
                UT.MsgBox("047 - Use Name iProd mancante. Configurazione non salvata", "ATTENZIONE!", "e");
                return;
            }


            iProdCFG.MP_Active= cboHWModels.Text;
            iProdCFG.MP_IP = txtIP.Text;
            iProdCFG.MP_Port = txtPort.Text;
            iProdCFG.MP_Url = txtMPUrl.Text;
            iProdCFG.iProdUrl = txtiProdUrl.Text;
            iProdCFG.iProdUser = txtUser.Text;
            iProdCFG.iProdPassword = txtPwd.Text;
            iProdCFG.LoginAtStartup = ckAutostart.Checked;
            iProdCFG.TimerInterval = Convert.ToInt64(txtautoupdate.Text);
            iProdCFG.MaxHistCount = Convert.ToInt32(txtMaxHist.Text);
            iProdCFG.ProcessStock = !ckSkipGIAC.Checked;

            iProdCFG.SaveSettings(UT.cfgFile);

            UT.MsgBox("Impostazioni salvate correttamente su file", "Salva");

        }

        private void pnlEvents_Paint(object sender, PaintEventArgs e)
        {

        }

        private void cbControlProcess_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (noloop) return;
            scheduleStatus();
        }


        bool noloop = false;
        void scheduleStatus()
        {
            lblSync.Text = "";
            int i = cbControlProcess.SelectedIndex;
            if (i < 0) return;

            if (isLoading) return;

            string s = cbControlProcess.Text;

            noloop = true;

            if (s.StartsWith("Fermo"))
            {
                LEDStock.ImageKey = "LEDROFF";
                UT.SVCPausedUntil = "Inattivo";
                lblSync.Text = UT.SVCPausedUntil;
                timer1.Enabled = false;
            }

            if (s.StartsWith("Avvia"))
            {
                LEDStock.ImageKey = "LEDGON";
                toolTip1.SetToolTip(LEDStock, "Timer stock in esecusione");

                CalcolaPausa(s);
                setSVCStatus("active");
                AvviaTimer();
            }

            if (s.Contains("riavvio manuale"))
            {
                LEDStock.ImageKey = "LEDROFF";
                toolTip1.SetToolTip(LEDStock, "Timer stock inattivo");

                CalcolaPausa(s);
                setSVCStatus("stopped");
                timer1.Enabled = false;
                noloop = false;
                return;
            }


            if (s.StartsWith("Sospendi"))
            {
                LEDStock.ImageKey = "LEDRON";
                toolTip1.SetToolTip(LEDStock, "Timer stock in pausa");
                CalcolaPausa(s);
                setSVCStatus("paused");
                timer1.Enabled = false;
            }

         
            noloop = false;
        }

        private void cboHWModels_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isLoading) return;

            int i = cboHWModels.SelectedIndex;
            if (i < 0) return;

            UT.curMV = UT.HwdSupportedModels[i];
            iProdCFG.MP_Active = UT.curMV.Name;

            iProdCFG.SaveSettings(UT.LogFile);

        }

        private void btLogin_Click(object sender, EventArgs e)
        {
            if (UT.iProdConnected)
                return;

            var f = new frmLogin();
            f.Show(this);
        }

        private void btCheckMP_Click(object sender, EventArgs e)
        {



            try
            {
                string a, b, c, d;

                UT.iProdCFG.MP_IP = a = txtIP.Text;
                UT.iProdCFG.MP_Port = b = txtPort.Text;
                UT.iProdCFG.MP_Url = c = txtMPUrl.Text;
                
                UT.iProdCFG.MP_Url = d = $"https://{a}:{b}/{c}";
                AgentHelper.BindingEndPoint = d;
                var ok = AgentHelper.ConnectToWS();

                if(ok)
                {
                    UT.MsgBox(log("Servizio disponibile"), "Connessione a WS..");
                    lbEsitoTestConnessione.Text = log("Web Service ONLINE");
                }
                else
                {
                    UT.MsgBox(log("Servizio non disponibile"), "Connessione a WS..");
                    lbEsitoTestConnessione.Text = log("Web Service OFFLINE");

                }
                //                if (UT.MockWS)
                //                {

                //                    MHE.Ping();
                //                    lbEsitoTestConnessione.Text= log("Servizio attivo.");
                //                    // si interroga il servizio REST per i test sia soap che REST

                //                    //string url = "hoffmann/reservation";

                //                    //string host = "https://localhost:7147/api/";
                //                    //HttpClient cli = new HttpClient();
                //                    //cli.BaseAddress = new Uri(host);

                //                    //var resp = UT.APICall(cli, url).Result;
                //                    //if (resp.status != "OK")
                //                    //{
                //                    //    log("Login iProd fallito");
                //                    //    mainForm.SetNetStatus("OFFLINE");
                //                    //    connecting = false;
                //                    //    return resp;
                //                    //}

                ////                    Tenant = JsonConvert.DeserializeObject<iProdCustomers>(resp.response);

                //                    //var req = new myEndPoint.deleteJobV01Request();
                //                    //req.param = new myEndPoint.ParDeleteJobV01();
                //                    //req.param.jobNumber = "dummy";
                //                    //var cl = new myEndPoint.ComPortTypeClient();
                //                    //var t = cl.deleteJobV01(req.param);
                //                }
                //                else if(iProdCFG.MP_Active=="HOFFMANN")
                //                {


                //                }
                //                else // SOAP
                //                {

                //                    var reqd = new myNameSpace.deleteJobV01Request();
                //                    reqd.param = new myNameSpace.ParDeleteJobV01();
                //                    reqd.param.jobNumber = "dummy";
                //                    var cli = AgentHelper.WSSoapClient;
                //                    var respd = cli.deleteJobV01(reqd);
                //                    lbEsitoTestConnessione.Text = log("Servizio attivo.");
                //                }
            }
            catch (Exception ex)
            {

                UT.MsgBox(log(ex.Message + UT.LF + ex.StackTrace),"Errore");
                lbEsitoTestConnessione.Text = log("Web Service non connesso");
            }
        }

        private void LEDLSTNR_Click(object sender, EventArgs e)
        {
            Listener_start();
            LEDLSTNR.ImageKey = "LEDGON";
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            UT.ShellExec(UT.pathApp);
            UT.ShellExec(UT.pathData);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            UT.ShellExec(UT.cfgFile);
        }

        private void btClearHistory_Click(object sender, EventArgs e)
        {
            UT.FileDelete(UT.HistFile);
            ShowHist();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ShowHist();
        }

        private void timerLEDS_Tick(object sender, EventArgs e)
        {
            Random rnd = new Random();
            var i = rnd.Next(1, 4);
            var on = rnd.Next(10, 300);

            timerLEDS.Interval = on;

            LedIP.ImageKey = "LEDGOFF";
            LedSQL.ImageKey = "LEDGOFF";

            switch (i)
            {
                case 1:
                    LedIP.ImageKey = "LEDGON";
                    break;
                case 2:
                    LedSQL.ImageKey = "LEDGON";
                    break;
                case 3:
                    LedIP.ImageKey = "LEDGON";
                    LedSQL.ImageKey = "LEDGON";
                    break;
                default:
                    break;
            }
            Application.DoEvents();
        }
    }
}
