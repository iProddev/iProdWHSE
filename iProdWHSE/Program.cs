using iProdWHSE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows.Forms;
using UT = iProdWHSE.utility;

namespace iProdWHSE
{
    static class Program
    {


        // user, pwd e data sono cryptati sul file di configurazione config.txt
        public static string ipUSER { get; set; }
        public static string ipPWD { get; set; }
        public static string ipData { get; set; }   // yyyyMMdd

        public static string ipTOKEN { get; set; }  // valorizzato al momento dell'autenticazione
        public static bool DEV { get; set; }

        public static UrlGateway UrlGate { get; set; }

        public static bool forzaExit = false;

        /// <summary>
        /// Punto di ingresso principale dell'applicazione.
        /// </summary>
        [STAThread]

        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            UT.EnableLogToDisk = true;

            UT.setApplicationPaths();



            // carica tutta
            // la configurazione in background

            var f = new Form1();

            UT.mainForm = f;




            // poi verifico se sono disconnesso da iprod
            if (needLogin())
            {
                Application.Run(new frmLogin());
                return;

            }
            else
            {
        //        if (forzaExit) Application.Exit();

                Application.Run(f);
            }


            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }


        /// <summary>
        /// Funzione di autenticazione. Leggo il file di configurazione, alla voce options; come valore c'è una stringa criptata
        /// che non è altro che 'user,pwd, data' dell'ultimo login effettuato.
        /// il cripting è la semplice somma algebrica di un nomero alla rappresentazione ASCII della stringa stessa.
        /// si cripta aggiungendolo, si decripta sottraendolo. Lo scopo è che non si veda in chiaro user/pwd nel file config.txt
        /// la data fa si che per tutto il giorno non chiede piu le credenziali, quindi qui si autentica e prosegue
        /// </summary>
        /// <returns></returns>
        static bool needLogin()
        {
            bool wasdisconnected = true; // assume che gli serve il login

            try
            {
                var lst = UT.LoadTextFile(UT.cfgFile, true);
                string st = "";
                foreach (var s in lst)
                {
                    // info@iprod.it,1234,23/01/2022
                    // per bypassare l'autenticazione nel config metti
                    // options;SKIP;0

                    if (s.StartsWith("iprod-env;"))
                    {
                        UT.EndPointKey = s.Substring(10, s.Length - 10);
                        UrlGate = new UrlGateway(UT.EndPointKey);
                    }

                    if (s.StartsWith("options;"))
                    {
                        if (s.StartsWith("options;SKIP"))
                        {
                            UT.stringLogin = "SKIP";
                            return false;
                        }

                        var cf = s.Split(';');
                        st = cf[1];
                        wasdisconnected = cf[2] == "0";
                        st = UT.MyDecrypt(st);

                        var ar = st.Split(',');
                        ipUSER = ar[0];
                        ipPWD = ar[1];
                        ipData = ar[2];
                        string nou = $"{DateTime.Now:yyyyMMdd}";

                        UT.stringLogin = st;

                    }
                }
            }
            catch (Exception ex)
            {

                UT.MsgBox($"269 Errore grave durante l'inizializzazione dell'applicazione (Program): " + ex.Message, "Attenzione!", "e");
            }

            return wasdisconnected;

        }



    }
}
