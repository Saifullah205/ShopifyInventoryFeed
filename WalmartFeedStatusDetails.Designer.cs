namespace ShopifyInventorySync
{
    partial class WalmartFeedStatusDetails
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WalmartFeedStatusDetails));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.dgvDetails = new System.Windows.Forms.DataGridView();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.lblFeedTypeValue = new System.Windows.Forms.Label();
            this.lblFeedStatusValue = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblFeedStatus = new System.Windows.Forms.Label();
            this.txtStatusSearch = new System.Windows.Forms.TextBox();
            this.lblSearchStatus = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDetails)).BeginInit();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.dgvDetails, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 75F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(800, 451);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // dgvDetails
            // 
            this.dgvDetails.AllowUserToAddRows = false;
            this.dgvDetails.AllowUserToResizeRows = false;
            this.dgvDetails.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDetails.Location = new System.Drawing.Point(3, 79);
            this.dgvDetails.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dgvDetails.Name = "dgvDetails";
            this.dgvDetails.ReadOnly = true;
            this.dgvDetails.RowHeadersWidth = 51;
            this.dgvDetails.RowTemplate.Height = 25;
            this.dgvDetails.Size = new System.Drawing.Size(794, 368);
            this.dgvDetails.TabIndex = 0;
            this.dgvDetails.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.dgvDetails_UserDeletingRow);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 4;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.lblFeedTypeValue, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.lblFeedStatusValue, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.lblFeedStatus, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.txtStatusSearch, 2, 1);
            this.tableLayoutPanel2.Controls.Add(this.lblSearchStatus, 2, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(794, 69);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // lblFeedTypeValue
            // 
            this.lblFeedTypeValue.AutoSize = true;
            this.lblFeedTypeValue.Location = new System.Drawing.Point(3, 34);
            this.lblFeedTypeValue.Name = "lblFeedTypeValue";
            this.lblFeedTypeValue.Size = new System.Drawing.Size(37, 20);
            this.lblFeedTypeValue.TabIndex = 4;
            this.lblFeedTypeValue.Text = "xxxx";
            // 
            // lblFeedStatusValue
            // 
            this.lblFeedStatusValue.AutoSize = true;
            this.lblFeedStatusValue.Location = new System.Drawing.Point(203, 34);
            this.lblFeedStatusValue.Name = "lblFeedStatusValue";
            this.lblFeedStatusValue.Size = new System.Drawing.Size(37, 20);
            this.lblFeedStatusValue.TabIndex = 1;
            this.lblFeedStatusValue.Text = "xxxx";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 20);
            this.label1.TabIndex = 3;
            this.label1.Text = "Feed Type";
            // 
            // lblFeedStatus
            // 
            this.lblFeedStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblFeedStatus.AutoSize = true;
            this.lblFeedStatus.Location = new System.Drawing.Point(203, 14);
            this.lblFeedStatus.Name = "lblFeedStatus";
            this.lblFeedStatus.Size = new System.Drawing.Size(85, 20);
            this.lblFeedStatus.TabIndex = 2;
            this.lblFeedStatus.Text = "Feed Status";
            // 
            // txtStatusSearch
            // 
            this.txtStatusSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtStatusSearch.Location = new System.Drawing.Point(303, 37);
            this.txtStatusSearch.Name = "txtStatusSearch";
            this.txtStatusSearch.Size = new System.Drawing.Size(194, 27);
            this.txtStatusSearch.TabIndex = 5;
            this.txtStatusSearch.TextChanged += new System.EventHandler(this.txtStatusSearch_TextChanged);
            // 
            // lblSearchStatus
            // 
            this.lblSearchStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblSearchStatus.AutoSize = true;
            this.lblSearchStatus.Location = new System.Drawing.Point(303, 14);
            this.lblSearchStatus.Name = "lblSearchStatus";
            this.lblSearchStatus.Size = new System.Drawing.Size(97, 20);
            this.lblSearchStatus.TabIndex = 6;
            this.lblSearchStatus.Text = "Search Status";
            // 
            // WalmartFeedStatusDetails
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 451);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "WalmartFeedStatusDetails";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "WalmartFeedStatusDetails";
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDetails)).EndInit();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private DataGridView dgvDetails;
        private TableLayoutPanel tableLayoutPanel2;
        private Label lblFeedType;
        private Label label1;
        private Label lblFeedStatus;
        private Label lblFeedTypeValue;
        private Label lblFeedStatusValue;
        private TextBox txtStatusSearch;
        private Label lblSearchStatus;
    }
}