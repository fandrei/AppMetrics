namespace AppMetrics.AgentService
{
	partial class ConfigForm
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
			this.OkControl = new System.Windows.Forms.Button();
			this.PasswordEdit = new System.Windows.Forms.TextBox();
			this.UserNameEdit = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.CancelControl = new System.Windows.Forms.Button();
			this.NodeNameEdit = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.ConfigServerEdit = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// OkControl
			// 
			this.OkControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OkControl.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OkControl.Location = new System.Drawing.Point(183, 170);
			this.OkControl.Name = "OkControl";
			this.OkControl.Size = new System.Drawing.Size(75, 23);
			this.OkControl.TabIndex = 4;
			this.OkControl.Text = "OK";
			this.OkControl.UseVisualStyleBackColor = true;
			this.OkControl.Click += new System.EventHandler(this.OkButton_Click);
			// 
			// PasswordEdit
			// 
			this.PasswordEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.PasswordEdit.Location = new System.Drawing.Point(96, 61);
			this.PasswordEdit.Name = "PasswordEdit";
			this.PasswordEdit.PasswordChar = '*';
			this.PasswordEdit.Size = new System.Drawing.Size(243, 20);
			this.PasswordEdit.TabIndex = 2;
			// 
			// UserNameEdit
			// 
			this.UserNameEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.UserNameEdit.Location = new System.Drawing.Point(96, 35);
			this.UserNameEdit.Name = "UserNameEdit";
			this.UserNameEdit.Size = new System.Drawing.Size(243, 20);
			this.UserNameEdit.TabIndex = 1;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(10, 61);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(53, 13);
			this.label2.TabIndex = 30;
			this.label2.Text = "Password";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(8, 35);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(58, 13);
			this.label1.TabIndex = 29;
			this.label1.Text = "User name";
			// 
			// CancelControl
			// 
			this.CancelControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelControl.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelControl.Location = new System.Drawing.Point(264, 170);
			this.CancelControl.Name = "CancelControl";
			this.CancelControl.Size = new System.Drawing.Size(75, 23);
			this.CancelControl.TabIndex = 5;
			this.CancelControl.Text = "Cancel";
			this.CancelControl.UseVisualStyleBackColor = true;
			this.CancelControl.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// NodeNameEdit
			// 
			this.NodeNameEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.NodeNameEdit.Location = new System.Drawing.Point(97, 100);
			this.NodeNameEdit.Name = "NodeNameEdit";
			this.NodeNameEdit.Size = new System.Drawing.Size(243, 20);
			this.NodeNameEdit.TabIndex = 3;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(9, 100);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(82, 33);
			this.label3.TabIndex = 32;
			this.label3.Text = "Node friendly name";
			// 
			// ConfigServerEdit
			// 
			this.ConfigServerEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ConfigServerEdit.Location = new System.Drawing.Point(97, 9);
			this.ConfigServerEdit.Name = "ConfigServerEdit";
			this.ConfigServerEdit.Size = new System.Drawing.Size(243, 20);
			this.ConfigServerEdit.TabIndex = 0;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(9, 9);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(69, 13);
			this.label4.TabIndex = 34;
			this.label4.Text = "Config server";
			// 
			// ConfigForm
			// 
			this.AcceptButton = this.OkControl;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(354, 205);
			this.Controls.Add(this.ConfigServerEdit);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.NodeNameEdit);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.CancelControl);
			this.Controls.Add(this.PasswordEdit);
			this.Controls.Add(this.UserNameEdit);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.OkControl);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "ConfigForm";
			this.ShowIcon = false;
			this.Text = "ConfigForm";
			this.TopMost = true;
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button OkControl;
		private System.Windows.Forms.TextBox PasswordEdit;
		private System.Windows.Forms.TextBox UserNameEdit;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button CancelControl;
		private System.Windows.Forms.TextBox NodeNameEdit;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox ConfigServerEdit;
		private System.Windows.Forms.Label label4;
	}
}