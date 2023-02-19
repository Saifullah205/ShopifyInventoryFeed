namespace ShopifyInventorySync
{
    partial class WalmartFeedStatus
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.DGVFeedStatus = new System.Windows.Forms.DataGridView();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.txtFeedResponse = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.btnPreview = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.chkIncludeDetails = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DGVFeedStatus)).BeginInit();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.DGVFeedStatus, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(800, 450);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // DGVFeedStatus
            // 
            this.DGVFeedStatus.AllowUserToAddRows = false;
            this.DGVFeedStatus.AllowUserToDeleteRows = false;
            this.DGVFeedStatus.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DGVFeedStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DGVFeedStatus.Location = new System.Drawing.Point(3, 3);
            this.DGVFeedStatus.Name = "DGVFeedStatus";
            this.DGVFeedStatus.ReadOnly = true;
            this.DGVFeedStatus.RowHeadersWidth = 51;
            this.DGVFeedStatus.Size = new System.Drawing.Size(394, 444);
            this.DGVFeedStatus.TabIndex = 0;
            this.DGVFeedStatus.CellContentDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DGVFeedStatus_CellContentDoubleClick);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.txtFeedResponse, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(403, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(394, 444);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // txtFeedResponse
            // 
            this.txtFeedResponse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtFeedResponse.Location = new System.Drawing.Point(3, 53);
            this.txtFeedResponse.Multiline = true;
            this.txtFeedResponse.Name = "txtFeedResponse";
            this.txtFeedResponse.Size = new System.Drawing.Size(388, 388);
            this.txtFeedResponse.TabIndex = 0;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 3;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Controls.Add(this.btnPreview, 2, 0);
            this.tableLayoutPanel3.Controls.Add(this.btnClear, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.chkIncludeDetails, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(388, 44);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // btnPreview
            // 
            this.btnPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnPreview.Location = new System.Drawing.Point(222, 3);
            this.btnPreview.Name = "btnPreview";
            this.btnPreview.Size = new System.Drawing.Size(163, 38);
            this.btnPreview.TabIndex = 1;
            this.btnPreview.Text = "Preview";
            this.btnPreview.UseVisualStyleBackColor = true;
            this.btnPreview.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // btnClear
            // 
            this.btnClear.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnClear.Location = new System.Drawing.Point(53, 3);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(163, 38);
            this.btnClear.TabIndex = 2;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // chkIncludeDetails
            // 
            this.chkIncludeDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkIncludeDetails.AutoSize = true;
            this.chkIncludeDetails.Location = new System.Drawing.Point(3, 17);
            this.chkIncludeDetails.Name = "chkIncludeDetails";
            this.chkIncludeDetails.Size = new System.Drawing.Size(42, 24);
            this.chkIncludeDetails.TabIndex = 3;
            this.chkIncludeDetails.Text = "D";
            this.chkIncludeDetails.UseVisualStyleBackColor = true;
            this.chkIncludeDetails.CheckedChanged += new System.EventHandler(this.chkIncludeDetails_CheckedChanged);
            // 
            // WalmartFeedStatus
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "WalmartFeedStatus";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Walmart Feed Status";
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.DGVFeedStatus)).EndInit();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private DataGridView DGVFeedStatus;
        private TableLayoutPanel tableLayoutPanel2;
        private TextBox txtFeedResponse;
        private TableLayoutPanel tableLayoutPanel3;
        private Button btnPreview;
        private Button btnClear;
        private CheckBox chkIncludeDetails;
    }
}