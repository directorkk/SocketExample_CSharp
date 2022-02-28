namespace SocketExample
{
    partial class MainForm
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
            this.mBtnServerSend = new System.Windows.Forms.Button();
            this.mBtnClientSend = new System.Windows.Forms.Button();
            this.mTextServerSend = new System.Windows.Forms.TextBox();
            this.mTextClientSend = new System.Windows.Forms.TextBox();
            this.mTextClientRecv = new System.Windows.Forms.TextBox();
            this.mTextServerRecv = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // mBtnServerSend
            // 
            this.mBtnServerSend.Location = new System.Drawing.Point(13, 12);
            this.mBtnServerSend.Name = "mBtnServerSend";
            this.mBtnServerSend.Size = new System.Drawing.Size(75, 23);
            this.mBtnServerSend.TabIndex = 0;
            this.mBtnServerSend.Text = "ServerSend";
            this.mBtnServerSend.UseVisualStyleBackColor = true;
            this.mBtnServerSend.Click += new System.EventHandler(this.mBtnServerSend_Click);
            // 
            // mBtnClientSend
            // 
            this.mBtnClientSend.Location = new System.Drawing.Point(291, 12);
            this.mBtnClientSend.Name = "mBtnClientSend";
            this.mBtnClientSend.Size = new System.Drawing.Size(75, 23);
            this.mBtnClientSend.TabIndex = 1;
            this.mBtnClientSend.Text = "ClientSend";
            this.mBtnClientSend.UseVisualStyleBackColor = true;
            this.mBtnClientSend.Click += new System.EventHandler(this.mBtnClientSend_Click);
            // 
            // mTextServerSend
            // 
            this.mTextServerSend.Location = new System.Drawing.Point(12, 41);
            this.mTextServerSend.MaxLength = 131070;
            this.mTextServerSend.Multiline = true;
            this.mTextServerSend.Name = "mTextServerSend";
            this.mTextServerSend.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.mTextServerSend.Size = new System.Drawing.Size(174, 105);
            this.mTextServerSend.TabIndex = 2;
            // 
            // mTextClientSend
            // 
            this.mTextClientSend.Location = new System.Drawing.Point(192, 41);
            this.mTextClientSend.MaxLength = 131070;
            this.mTextClientSend.Multiline = true;
            this.mTextClientSend.Name = "mTextClientSend";
            this.mTextClientSend.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.mTextClientSend.Size = new System.Drawing.Size(174, 105);
            this.mTextClientSend.TabIndex = 2;
            // 
            // mTextClientRecv
            // 
            this.mTextClientRecv.Location = new System.Drawing.Point(192, 152);
            this.mTextClientRecv.MaxLength = 131070;
            this.mTextClientRecv.Multiline = true;
            this.mTextClientRecv.Name = "mTextClientRecv";
            this.mTextClientRecv.ReadOnly = true;
            this.mTextClientRecv.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.mTextClientRecv.Size = new System.Drawing.Size(174, 105);
            this.mTextClientRecv.TabIndex = 3;
            // 
            // mTextServerRecv
            // 
            this.mTextServerRecv.Location = new System.Drawing.Point(12, 152);
            this.mTextServerRecv.MaxLength = 131070;
            this.mTextServerRecv.Multiline = true;
            this.mTextServerRecv.Name = "mTextServerRecv";
            this.mTextServerRecv.ReadOnly = true;
            this.mTextServerRecv.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.mTextServerRecv.Size = new System.Drawing.Size(174, 105);
            this.mTextServerRecv.TabIndex = 4;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(377, 270);
            this.Controls.Add(this.mTextClientRecv);
            this.Controls.Add(this.mTextServerRecv);
            this.Controls.Add(this.mTextClientSend);
            this.Controls.Add(this.mTextServerSend);
            this.Controls.Add(this.mBtnClientSend);
            this.Controls.Add(this.mBtnServerSend);
            this.Name = "MainForm";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button mBtnServerSend;
        private System.Windows.Forms.Button mBtnClientSend;
        private System.Windows.Forms.TextBox mTextServerSend;
        private System.Windows.Forms.TextBox mTextClientSend;
        private System.Windows.Forms.TextBox mTextClientRecv;
        private System.Windows.Forms.TextBox mTextServerRecv;
    }
}

