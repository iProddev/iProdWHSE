namespace iProdWSHE
{
    partial class frmAiuto
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txHelp = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // txHelp
            // 
            this.txHelp.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txHelp.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txHelp.Location = new System.Drawing.Point(-1, 1);
            this.txHelp.Multiline = true;
            this.txHelp.Name = "txHelp";
            this.txHelp.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txHelp.Size = new System.Drawing.Size(409, 273);
            this.txHelp.TabIndex = 1;
            // 
            // frmAiuto
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(411, 286);
            this.Controls.Add(this.txHelp);
            this.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmAiuto";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Aiuto";
            this.Load += new System.EventHandler(this.frmAiuto_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox txHelp;
    }
}