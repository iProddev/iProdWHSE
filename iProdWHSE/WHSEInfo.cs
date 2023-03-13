using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UT = iProdWHSE.utility;

namespace iProdWHSE
{
    [Serializable]
    public  class WHSEInfo
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }        // tipo di 
        public string LocalIP { get; set; }
        public string Technology { get; set; } // SOAP,SERIAL
        public string SerialPort { get; set; } 
        public string Brand { get; set; }
        public string Model { get; set; }
        public string SerialNumber { get; set; }
        public int SlotNumber { get; set; }         // quantita di cassetti
        public string HwdActive { get; set; } //;GT24S
        public string HwdDescription { get; set; } //;
        public string HwdLocation { get; set; } //;
        public string EndpointCompany { get; set; } //;http://192.168.1.254/jwscom/services/Com
        public bool LoginAtStartup { get; set; } //;false
        public bool ProcessStock { get; set; } // elabora giacenze
        public long TimerInterval { get; set; } //;20
        public int MaxHistCount { get; set; }

        public int TimeridleFrom { get; set; } //;19 // inattività dalle ore
        public int TimeridleTo { get; set; } //;09  // ialle 
        public DateTime LastMailSent { get; set; } //;01/01/1970 13:19:39
        public string MailNotifiche { get; set; } //; fabio.guerrazzi @iprod.it

        public string iProdUrl { get; set; } //; SQLServer
        public string TenantId { get; set; }        //
        public string TenantName { get; set; }       //
        public string MP_Active { get; set; }       // 
        public string MP_IP { get; set; }           //
        public string MP_Port { get; set; }         // 
        public string MP_Url { get; set; }
        public string iProdUser { get; set; }       // 
        public string iProdPassword { get; set; }   //  
        public string Options { get; set; }         // qui ci sono le credenziali di IPROD criptate
        public string Ambiente { get; set; }        //; dev
     

        public WHSEInfo() {

            								
            ID = Guid.NewGuid().ToString();
         
        }



        public void SetProperty(string str, char charSep = ';', char charComment = '|')
        {
            string s = str.Trim();

            var t = s.IndexOf(charComment.ToString());
            if (t>=0 && t< 2) return; // spurga

            if (s.Length < 1) return;

            var ar = UT.SplitTouple(s, charSep);
            var c = ar.key;
            var v = ar.value;

            string sd = "";
            if (c == "max-hist") MaxHistCount = Convert.ToInt32(v); 
            if (c == "timerinterval") TimerInterval = Convert.ToInt64(v);
            if (c == "timeridle-from") TimeridleFrom = Convert.ToInt32(v);
            if (c == "timeridle-to") TimeridleTo = Convert.ToInt32(v);
            if (c == "mailnotifiche") MailNotifiche = v;
            if (c == "autostart") LoginAtStartup = v == "true" || v == "1";
            if(c == "stock-enabled") ProcessStock = v == "true" || v == "1";

            if (c == "options" && UT.stringLogin != "SKIP")
            {
                UT.stringLogin = UT.MyDecrypt(v);
                var lg = UT.stringLogin.Split(',');
                iProdUser = lg[0];
                iProdPassword = lg[1];

                Options = v;

            }
            if (c == "disablelogin")UT. stringLogin = "SKIP";
            if (c == "iprod-env") Ambiente = v;

            if (c == "mp-active") MP_Active = v;
            if (c == "mp-ip") MP_IP = v;
            if (c == "mp-port") MP_Port = v;
            if (c == "mp-url") MP_Url = v;
            if (c == "mp-mock") UT.MockWS = v == "true";


            if (c == "lastmailsent") sd = v;

            if (!string.IsNullOrEmpty(sd))
                if (DateTime.TryParse(sd, out DateTime d1))
                    LastMailSent = d1;
        }

        string Log(string msg)
        {
            return UT.Log(msg);
        }
        public void SaveSettings(string fn)
        {
            
            if (fn.IsNull())
            {
                MessageBox.Show($"068 - Errore Grave: il file di configurazione non è valorizzato. Variabile fileCFG in saveConfig(..)", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            if (UT.stringLogin != "SKIP")
            {
                UT.stringLogin = $"{Program.ipUSER},{Program.ipPWD},{DateTime.Now:yyyyMMdd}";
                UT.stringLogin = UT.MyCrypt(UT.stringLogin);
            }

            if (UT.FileExists(fn)) UT.FileDelete(fn);


            UT.AppendToFile(fn, Log($"# =============================================================="), true, true);
            UT.AppendToFile(fn, Log($"#    Configurazione iProdWHSE."));
            UT.AppendToFile(fn, Log($"#    Salvata il {DateTime.Now} "));
            UT.Log($"#         su file '{fn}'");
            UT.AppendToFile(fn, Log($"# =============================================================="));
            UT.AppendToFile(fn, Log(" "));

            string las = LoginAtStartup ? "true" : "false";
            string mck = UT.MockWS ? "true" : "false";
            string stocks = ProcessStock ? "true" : "false";

            UT.AppendToFile(fn, $"pathdata;{UT.pathData}");
            string ip_connected = "0";
            if (UT.iProdConnected) ip_connected = "1";
            UT.AppendToFile(fn, $"iprod-env;{Ambiente}");
            UT.AppendToFile(fn, $"options;{UT.stringLogin};{ip_connected}");
            UT.AppendToFile(fn, $"autostart;{las}");
            UT.AppendToFile(fn, $"stock-enabled;{stocks}");
            UT.AppendToFile(fn, $"iprod-url;{iProdUrl}");
            UT.AppendToFile(fn, $"mp-active;{MP_Active}");
            UT.AppendToFile(fn, $"mp-ip;{MP_IP}");
            UT.AppendToFile(fn, $"mp-port;{MP_Port}");
            UT.AppendToFile(fn, $"mp-url;{MP_Url}");
            UT.AppendToFile(fn, $"mp-mock;{mck}");

            UT.AppendToFile(fn, $"max-hist;{MaxHistCount}");
            UT.AppendToFile(fn, $"timerinterval;{TimerInterval}");
            UT.AppendToFile(fn, $"timeridle-from;{TimeridleFrom:00}");
            UT.AppendToFile(fn, $"timeridle-to;{TimeridleTo :00}");
            UT.AppendToFile(fn, $"lastmailsent;{LastMailSent}");
            UT.AppendToFile(fn, $"mailnotifiche;{MailNotifiche}");

 

        }


    }
}
