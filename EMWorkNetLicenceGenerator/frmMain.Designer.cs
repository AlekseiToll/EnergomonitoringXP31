namespace EMWorkNetLicenceGenerator
{
	partial class frmMain
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
			this.btnAdd = new System.Windows.Forms.Button();
			this.lblEnterDeviceNumber = new System.Windows.Forms.Label();
			this.mtxtDeviceNumber = new System.Windows.Forms.MaskedTextBox();
			this.listboxDeviceNumbers = new System.Windows.Forms.ListBox();
			this.btnGenerateLicence = new System.Windows.Forms.Button();
			this.lblDeviceNumbers = new System.Windows.Forms.Label();
			this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
			this.SuspendLayout();
			// 
			// btnAdd
			// 
			this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.btnAdd.Location = new System.Drawing.Point(12, 47);
			this.btnAdd.Name = "btnAdd";
			this.btnAdd.Size = new System.Drawing.Size(249, 23);
			this.btnAdd.TabIndex = 1;
			this.btnAdd.Text = "Add to the list";
			this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
			// 
			// lblEnterDeviceNumber
			// 
			this.lblEnterDeviceNumber.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.lblEnterDeviceNumber.AutoSize = true;
			this.lblEnterDeviceNumber.Location = new System.Drawing.Point(9, 5);
			this.lblEnterDeviceNumber.Name = "lblEnterDeviceNumber";
			this.lblEnterDeviceNumber.Size = new System.Drawing.Size(108, 13);
			this.lblEnterDeviceNumber.TabIndex = 100;
			this.lblEnterDeviceNumber.Text = "Enter device number:";
			// 
			// mtxtDeviceNumber
			// 
			this.mtxtDeviceNumber.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.mtxtDeviceNumber.BeepOnError = true;
			this.mtxtDeviceNumber.Culture = new System.Globalization.CultureInfo("");
			this.mtxtDeviceNumber.CutCopyMaskFormat = System.Windows.Forms.MaskFormat.IncludePromptAndLiterals;
			this.mtxtDeviceNumber.Location = new System.Drawing.Point(12, 21);
			this.mtxtDeviceNumber.Mask = "00000000";
			this.mtxtDeviceNumber.Name = "mtxtDeviceNumber";
			this.mtxtDeviceNumber.PromptChar = '0';
			this.mtxtDeviceNumber.Size = new System.Drawing.Size(249, 20);
			this.mtxtDeviceNumber.TabIndex = 0;
			this.mtxtDeviceNumber.TextMaskFormat = System.Windows.Forms.MaskFormat.IncludePromptAndLiterals;
			// 
			// listboxDeviceNumbers
			// 
			this.listboxDeviceNumbers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.listboxDeviceNumbers.FormattingEnabled = true;
			this.listboxDeviceNumbers.Location = new System.Drawing.Point(12, 91);
			this.listboxDeviceNumbers.Name = "listboxDeviceNumbers";
			this.listboxDeviceNumbers.Size = new System.Drawing.Size(249, 147);
			this.listboxDeviceNumbers.TabIndex = 2;
			// 
			// btnGenerateLicence
			// 
			this.btnGenerateLicence.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.btnGenerateLicence.Location = new System.Drawing.Point(12, 252);
			this.btnGenerateLicence.Name = "btnGenerateLicence";
			this.btnGenerateLicence.Size = new System.Drawing.Size(249, 23);
			this.btnGenerateLicence.TabIndex = 3;
			this.btnGenerateLicence.Text = "Generate licence file";
			this.btnGenerateLicence.Click += new System.EventHandler(this.btnGenerateLicence_Click);
			// 
			// lblDeviceNumbers
			// 
			this.lblDeviceNumbers.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.lblDeviceNumbers.AutoSize = true;
			this.lblDeviceNumbers.Location = new System.Drawing.Point(11, 75);
			this.lblDeviceNumbers.Name = "lblDeviceNumbers";
			this.lblDeviceNumbers.Size = new System.Drawing.Size(87, 13);
			this.lblDeviceNumbers.TabIndex = 101;
			this.lblDeviceNumbers.Text = "Device numbers:";
			// 
			// saveFileDialog
			// 
			this.saveFileDialog.DefaultExt = "licence";
			this.saveFileDialog.FileName = "EmWorkNet.licence";
			this.saveFileDialog.Filter = "Licence files|*.licence|All files|*.*";
			this.saveFileDialog.SupportMultiDottedExtensions = true;
			// 
			// frmMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(273, 287);
			this.Controls.Add(this.listboxDeviceNumbers);
			this.Controls.Add(this.mtxtDeviceNumber);
			this.Controls.Add(this.lblDeviceNumbers);
			this.Controls.Add(this.btnGenerateLicence);
			this.Controls.Add(this.btnAdd);
			this.Controls.Add(this.lblEnterDeviceNumber);
			this.MaximizeBox = false;
			this.Name = "frmMain";
			this.Text = "Licence generator";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnAdd;
		private System.Windows.Forms.Label lblEnterDeviceNumber;
		private System.Windows.Forms.MaskedTextBox mtxtDeviceNumber;
		private System.Windows.Forms.ListBox listboxDeviceNumbers;
		private System.Windows.Forms.Button btnGenerateLicence;
		private System.Windows.Forms.Label lblDeviceNumbers;
		private System.Windows.Forms.SaveFileDialog saveFileDialog;
	}
}

