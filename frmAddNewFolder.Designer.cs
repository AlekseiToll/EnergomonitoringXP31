namespace EnergomonitoringXP
{
    partial class frmAddNewFolder
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmAddNewFolder));
            this.lblEnterNewFolderName = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.gbMain = new System.Windows.Forms.GroupBox();
            this.txtInfo = new System.Windows.Forms.TextBox();
            this.lblInfo = new System.Windows.Forms.Label();
            this.gbButtons = new System.Windows.Forms.GroupBox();
            this.gbMain.SuspendLayout();
            this.gbButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblEnterNewFolderName
            // 
            resources.ApplyResources(this.lblEnterNewFolderName, "lblEnterNewFolderName");
            this.lblEnterNewFolderName.Name = "lblEnterNewFolderName";
            // 
            // txtName
            // 
            resources.ApplyResources(this.txtName, "txtName");
            this.txtName.Name = "txtName";
            this.txtName.TextChanged += new System.EventHandler(this.txtFolderName_TextChanged);
            // 
            // btnOK
            // 
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Name = "btnOK";
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Name = "btnCancel";
            // 
            // gbMain
            // 
            this.gbMain.Controls.Add(this.txtInfo);
            this.gbMain.Controls.Add(this.txtName);
            this.gbMain.Controls.Add(this.lblInfo);
            this.gbMain.Controls.Add(this.lblEnterNewFolderName);
            resources.ApplyResources(this.gbMain, "gbMain");
            this.gbMain.Name = "gbMain";
            this.gbMain.TabStop = false;
            // 
            // txtInfo
            // 
            resources.ApplyResources(this.txtInfo, "txtInfo");
            this.txtInfo.Name = "txtInfo";
            this.txtInfo.TextChanged += new System.EventHandler(this.txtInfo_TextChanged);
            // 
            // lblInfo
            // 
            resources.ApplyResources(this.lblInfo, "lblInfo");
            this.lblInfo.Name = "lblInfo";
            // 
            // gbButtons
            // 
            this.gbButtons.Controls.Add(this.btnCancel);
            this.gbButtons.Controls.Add(this.btnOK);
            resources.ApplyResources(this.gbButtons, "gbButtons");
            this.gbButtons.Name = "gbButtons";
            this.gbButtons.TabStop = false;
            // 
            // frmAddNewFolder
            // 
            this.AcceptButton = this.btnOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ControlBox = false;
            this.Controls.Add(this.gbMain);
            this.Controls.Add(this.gbButtons);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "frmAddNewFolder";
            this.ShowInTaskbar = false;
            this.gbMain.ResumeLayout(false);
            this.gbMain.PerformLayout();
            this.gbButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblEnterNewFolderName;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
		 private System.Windows.Forms.GroupBox gbMain;
		 private System.Windows.Forms.GroupBox gbButtons;
		 private System.Windows.Forms.Label lblInfo;
		 private System.Windows.Forms.TextBox txtInfo;
    }
}