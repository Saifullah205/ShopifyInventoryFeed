namespace ShopifyInventorySync
{
    partial class ApplicationSettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ApplicationSettingsForm));
            this.DGVApplicationSettings = new System.Windows.Forms.DataGridView();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.BtnResetData = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.DGVApplicationSettings)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // DGVApplicationSettings
            // 
            this.DGVApplicationSettings.AllowUserToAddRows = false;
            this.DGVApplicationSettings.AllowUserToDeleteRows = false;
            this.DGVApplicationSettings.AllowUserToResizeColumns = false;
            this.DGVApplicationSettings.AllowUserToResizeRows = false;
            this.DGVApplicationSettings.BackgroundColor = System.Drawing.SystemColors.Control;
            this.DGVApplicationSettings.ColumnHeadersHeight = 29;
            this.DGVApplicationSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DGVApplicationSettings.GridColor = System.Drawing.SystemColors.Control;
            this.DGVApplicationSettings.Location = new System.Drawing.Point(3, 53);
            this.DGVApplicationSettings.Name = "DGVApplicationSettings";
            this.DGVApplicationSettings.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToDisplayedHeaders;
            this.DGVApplicationSettings.RowTemplate.Height = 29;
            this.DGVApplicationSettings.Size = new System.Drawing.Size(794, 394);
            this.DGVApplicationSettings.TabIndex = 0;
            this.DGVApplicationSettings.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.DGVApplicationSettings_CellEndEdit);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.DGVApplicationSettings, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(800, 450);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel2.Controls.Add(this.BtnResetData, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(794, 44);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // BtnResetData
            // 
            this.BtnResetData.BackColor = System.Drawing.Color.Red;
            this.BtnResetData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BtnResetData.Location = new System.Drawing.Point(747, 3);
            this.BtnResetData.Name = "BtnResetData";
            this.BtnResetData.Size = new System.Drawing.Size(44, 38);
            this.BtnResetData.TabIndex = 0;
            this.BtnResetData.Text = "X";
            this.BtnResetData.UseVisualStyleBackColor = false;
            this.BtnResetData.Click += new System.EventHandler(this.BtnResetData_Click);
            // 
            // ApplicationSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ApplicationSettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ApplicationSettings";
            ((System.ComponentModel.ISupportInitialize)(this.DGVApplicationSettings)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DataGridView DGVApplicationSettings;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private Button BtnResetData;
    }
}