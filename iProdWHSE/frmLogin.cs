using System;
using System.Windows.Forms;
using UT = iProdWHSE.utility;

namespace iProdWHSE
{
    public partial class frmLogin : Form
    {
        public frmLogin()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void frmLogin_Load(object sender, EventArgs e)
        {
            //txUser.Text ="pieracci@pieraccimeccanica.it";
            //txPwd.Text = "1234";


            UT.loadConfig();

            if (UT.EndPointKey=="dev" || UT.EndPointKey == "alpha")
            {
               // txUser.Text ="unpluggedmail@gmail.com";
                txUser.Text = "info@iprod.it";
                txPwd.Text = "1234";
            }
            if (UT.EndPointKey.IsNull()) UT.EndPointKey = "prod";
            lbVers.Text = $"Vers. {UT.Versione} ({UT.EndPointKey})";

        }

        private async void button1_Click(object sender, EventArgs e)
        {
            Program.ipUSER = txUser.Text;
            Program.ipPWD = txPwd.Text;
          
            var msg = await UT.iProdLogin(Program.UrlGate.api, Program.ipUSER, Program.ipPWD);
            if (!UT.IsNull(msg.status))
            {
                if (msg.status!="OK")
                {
                    MessageBox.Show("Utente o password non riconosciuti", "ERRORE Autenticazione", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            UT.iProdUsername = Program.ipUSER;
            UT.iProdPassword =Program.ipPWD;
            Program.ipData = $"{DateTime.UtcNow:yyyyMMdd}";


            UT.iProdCFG.LoginAtStartup = true;
            UT.iProdCFG.SaveSettings(UT.cfgFile);

        
            UT.MsgBox("Premi Ok per chiudere l'applicazione e poi riavviala ", "Richiesto restart");

            Application.Exit();

        }

        private void button4_Click(object sender, EventArgs e)
        {
            //UT.ShellExec("https://iprod-test.azurewebsites.net/Login/ForgotPassword");
            UT.ShellExec(Program.UrlGate.lostpassword);
        }

        private void btRegistrati_Click(object sender, EventArgs e)
        {
           // UT.ShellExec("https://iprod-test.azurewebsites.net/Login/NewRegistration");
            UT.ShellExec(Program.UrlGate.register);
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            UT.ShellExec("http://www.iprod.it");
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Control) && ModifierKeys.HasFlag(Keys.Shift))
            {
                UT.ShellExec(UT.pathData);
                return;
            }


            UT.ShellExec("mailto:info@iprod.it");
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            UT.ShellExec("mailto:assistenza@iprod.it");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Control) && ModifierKeys.HasFlag(Keys.Shift))
            {
                UT.ShellExec(UT.pathData);
                return;
            }


            UT.ShellExec("mailto:info@iprod.it");
        }

        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            UT.ShellExec("https://assistenza.iprod.it/iProdWHSE");
        }

        private void linkLabel5_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
 
            if (ModifierKeys.HasFlag(Keys.Control) && ModifierKeys.HasFlag(Keys.Shift))
            {
                UT.ShellExec(UT.pathApp);
                return;
            }


            UT.ShellExec("https://assistenza.iprod.it/iProdWHSE");
        }

        private void linkLabel5_DoubleClick(object sender, EventArgs e)
        {
            
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Control) && ModifierKeys.HasFlag(Keys.Shift))
            {
                UT.ShellExec(UT.pathApp);
                UT.ShellExec(UT.pathData);
                return;
            }
        }
    }
}
