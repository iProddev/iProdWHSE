using System;
using System.Windows.Forms;

namespace iProdWHSE
{
    public partial class Splash : Form
    {
        int cnt = 0;
        public Splash()
        {
            InitializeComponent();
        }

        private void Splash_Load(object sender, EventArgs e)
        {
            lbCounter.Text = "";
            //timer1.Start();
            //timer1_Tick(null, null);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            cnt++;
            lbCounter.Text = cnt.ToString();    
            Application.DoEvents();
        }
    }
}
