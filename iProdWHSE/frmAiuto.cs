using System;
using System.Collections.Generic;
using System.Windows.Forms;
using UT = iProdWHSE.utility;

namespace iProdWSHE
{
    public partial class frmAiuto : Form
    {

        private string testo { get; set; }
        public frmAiuto()
        {
            InitializeComponent();
        }

        public frmAiuto(string sezione)
        {
            InitializeComponent();

            if(sezione.StartsWith("#")) // non lo deve cercare dal file ma deve mostrare il testo in sezione
            {
                testo = sezione.Substring(1,sezione.Length-1);
                return;
            }

            var lst = new List<string>();
            string fn = "help.txt";
            if (!UT.FileExists(fn)) return;

            try { lst = UT.LoadTextFile(fn); }
            catch { }

            int i = 0;
            foreach(var s in lst)
            {
                if (s.StartsWith("@"))
                {
                    if (i==0)
                    {
                        if (s=="@"+sezione)
                        {
                            i++;
                            continue;
                        }
                    }
                    else return;
                }

                if (i>0)
                    testo += s + UT.LF;
            }

        }

        private void frmAiuto_Load(object sender, EventArgs e)
        {
            txHelp.Text =testo;
        }

        private void btOk_Click(object sender, EventArgs e)
        {
            
        }
    }
}
