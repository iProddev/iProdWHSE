using System;
using System.Windows.Forms;

namespace iProdWHSE
{
    public partial class frmTerms : Form
    {
        public frmTerms()
        {
            InitializeComponent();
        }

        private void frmTerms_Load(object sender, EventArgs e)
        {
            string f = "terms.rtf";
            rtf.LoadFile(f);
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            this.rtf.Dispose();
            this.Close();
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            utility.AcceptedTerms();
            this.rtf.Dispose();
            this.Close();

        }
    }
}
