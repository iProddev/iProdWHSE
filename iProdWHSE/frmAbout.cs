using System;
using System.Windows.Forms;
using UT = iProdWHSE.utility;

namespace iProdWHSE
{
    public partial class frmAbout : Form
    {
        public frmAbout()
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

        private void frmAbout_Load(object sender, EventArgs e)
        {
            lbVer.Text = "Versione: " + UT.Versione;
            lbVerCommit.Text = UT.Versione_Commit;
        }

      

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            UT.ShellExec("http://www.iprod.it");
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
 
            UT.ShellExec("mailto:info@iprod.it");
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            UT.ShellExec("mailto:assistenza@iprod.it");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            UT.ShellExec("mailto:info@iprod.it");
        }

        private void linkLabel5_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            UT.ShellExec("https://assistenza.iprod.it/iProdWHSE");
        }

        private void linkLabel6_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            UT.ShellExec("mailto:sales@iprod.it");
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            var f = new frmTerms();
            f.ShowDialog(this);
        }

        private void butClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            UT.ShellExec("GuidaiProdWHSE.pdf");
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
