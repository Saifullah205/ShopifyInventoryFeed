namespace ShopifyInventorySync
{
    partial class ProcessCSVForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProcessCSVForm));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.lblProgressCount = new System.Windows.Forms.Label();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnProcess = new System.Windows.Forms.Button();
            this.lblSelectedAPI = new System.Windows.Forms.Label();
            this.ProcessProgress = new System.Windows.Forms.ProgressBar();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.loadedDataGridView = new System.Windows.Forms.DataGridView();
            this.txtProcessedProducts = new System.Windows.Forms.TextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fetchFragranceXProductsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fetchFragranceNetProductsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.markupSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.restrictedBrandsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.restrictedSKUsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fixedPricesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.walmartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadThePerfumeSpotProductsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fetchFragranceXProductsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.markupSettingsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.restrictedBrandsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.restrictedSKUsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.fixedPricesToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.loadedDataGridView)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.ProcessProgress, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 28);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(800, 422);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 4;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel2.Controls.Add(this.lblProgressCount, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnClear, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnProcess, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.lblSelectedAPI, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 39F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(794, 44);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // lblProgressCount
            // 
            this.lblProgressCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblProgressCount.AutoSize = true;
            this.lblProgressCount.Location = new System.Drawing.Point(3, 24);
            this.lblProgressCount.Name = "lblProgressCount";
            this.lblProgressCount.Size = new System.Drawing.Size(107, 20);
            this.lblProgressCount.TabIndex = 3;
            this.lblProgressCount.Text = "0% Completed";
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(647, 3);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(144, 38);
            this.btnClear.TabIndex = 2;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnProcess
            // 
            this.btnProcess.Enabled = false;
            this.btnProcess.Location = new System.Drawing.Point(497, 3);
            this.btnProcess.Name = "btnProcess";
            this.btnProcess.Size = new System.Drawing.Size(144, 38);
            this.btnProcess.TabIndex = 1;
            this.btnProcess.Text = "Process";
            this.btnProcess.UseVisualStyleBackColor = true;
            this.btnProcess.Click += new System.EventHandler(this.button2_Click);
            // 
            // lblSelectedAPI
            // 
            this.lblSelectedAPI.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSelectedAPI.AutoSize = true;
            this.lblSelectedAPI.Location = new System.Drawing.Point(297, 12);
            this.lblSelectedAPI.Name = "lblSelectedAPI";
            this.lblSelectedAPI.Size = new System.Drawing.Size(92, 20);
            this.lblSelectedAPI.TabIndex = 4;
            this.lblSelectedAPI.Text = "Selected API";
            // 
            // ProcessProgress
            // 
            this.ProcessProgress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ProcessProgress.Location = new System.Drawing.Point(3, 53);
            this.ProcessProgress.Name = "ProcessProgress";
            this.ProcessProgress.Size = new System.Drawing.Size(794, 14);
            this.ProcessProgress.TabIndex = 1;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Controls.Add(this.loadedDataGridView, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.txtProcessedProducts, 1, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 73);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(794, 346);
            this.tableLayoutPanel3.TabIndex = 2;
            // 
            // loadedDataGridView
            // 
            this.loadedDataGridView.BackgroundColor = System.Drawing.SystemColors.Control;
            this.loadedDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.loadedDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.loadedDataGridView.Location = new System.Drawing.Point(3, 3);
            this.loadedDataGridView.Name = "loadedDataGridView";
            this.loadedDataGridView.RowHeadersWidth = 51;
            this.loadedDataGridView.RowTemplate.Height = 29;
            this.loadedDataGridView.Size = new System.Drawing.Size(391, 340);
            this.loadedDataGridView.TabIndex = 0;
            // 
            // txtProcessedProducts
            // 
            this.txtProcessedProducts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtProcessedProducts.Location = new System.Drawing.Point(400, 3);
            this.txtProcessedProducts.Multiline = true;
            this.txtProcessedProducts.Name = "txtProcessedProducts";
            this.txtProcessedProducts.ReadOnly = true;
            this.txtProcessedProducts.Size = new System.Drawing.Size(391, 340);
            this.txtProcessedProducts.TabIndex = 1;
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.walmartToolStripMenuItem,
            this.settingsToolStripMenuItem1,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(800, 28);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.fetchFragranceXProductsToolStripMenuItem,
            this.fetchFragranceNetProductsToolStripMenuItem,
            this.toolStripSeparator1,
            this.markupSettingsToolStripMenuItem,
            this.restrictedBrandsToolStripMenuItem,
            this.restrictedSKUsToolStripMenuItem,
            this.fixedPricesToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(73, 24);
            this.fileToolStripMenuItem.Text = "Shopify";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(307, 26);
            this.openToolStripMenuItem.Text = "Load The Perfume Spot Products";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // fetchFragranceXProductsToolStripMenuItem
            // 
            this.fetchFragranceXProductsToolStripMenuItem.Name = "fetchFragranceXProductsToolStripMenuItem";
            this.fetchFragranceXProductsToolStripMenuItem.Size = new System.Drawing.Size(307, 26);
            this.fetchFragranceXProductsToolStripMenuItem.Text = "Fetch FragranceX Products";
            this.fetchFragranceXProductsToolStripMenuItem.Click += new System.EventHandler(this.fetchFragranceXProductsToolStripMenuItem_Click);
            // 
            // fetchFragranceNetProductsToolStripMenuItem
            // 
            this.fetchFragranceNetProductsToolStripMenuItem.Name = "fetchFragranceNetProductsToolStripMenuItem";
            this.fetchFragranceNetProductsToolStripMenuItem.Size = new System.Drawing.Size(307, 26);
            this.fetchFragranceNetProductsToolStripMenuItem.Text = "Fetch FragranceNet Products";
            this.fetchFragranceNetProductsToolStripMenuItem.Visible = false;
            this.fetchFragranceNetProductsToolStripMenuItem.Click += new System.EventHandler(this.fetchFragranceNetProductsToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(304, 6);
            // 
            // markupSettingsToolStripMenuItem
            // 
            this.markupSettingsToolStripMenuItem.Name = "markupSettingsToolStripMenuItem";
            this.markupSettingsToolStripMenuItem.Size = new System.Drawing.Size(307, 26);
            this.markupSettingsToolStripMenuItem.Text = "Markup Settings";
            this.markupSettingsToolStripMenuItem.Click += new System.EventHandler(this.markupSettingsToolStripMenuItem_Click);
            // 
            // restrictedBrandsToolStripMenuItem
            // 
            this.restrictedBrandsToolStripMenuItem.Name = "restrictedBrandsToolStripMenuItem";
            this.restrictedBrandsToolStripMenuItem.Size = new System.Drawing.Size(307, 26);
            this.restrictedBrandsToolStripMenuItem.Text = "Restricted brands";
            this.restrictedBrandsToolStripMenuItem.Click += new System.EventHandler(this.restrictedBrandsToolStripMenuItem_Click);
            // 
            // restrictedSKUsToolStripMenuItem
            // 
            this.restrictedSKUsToolStripMenuItem.Name = "restrictedSKUsToolStripMenuItem";
            this.restrictedSKUsToolStripMenuItem.Size = new System.Drawing.Size(307, 26);
            this.restrictedSKUsToolStripMenuItem.Text = "Restricted SKUs";
            this.restrictedSKUsToolStripMenuItem.Click += new System.EventHandler(this.restrictedSKUsToolStripMenuItem_Click);
            // 
            // fixedPricesToolStripMenuItem
            // 
            this.fixedPricesToolStripMenuItem.Name = "fixedPricesToolStripMenuItem";
            this.fixedPricesToolStripMenuItem.Size = new System.Drawing.Size(307, 26);
            this.fixedPricesToolStripMenuItem.Text = "Fixed Prices";
            this.fixedPricesToolStripMenuItem.Click += new System.EventHandler(this.fixedPricesToolStripMenuItem_Click);
            // 
            // walmartToolStripMenuItem
            // 
            this.walmartToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadThePerfumeSpotProductsToolStripMenuItem,
            this.fetchFragranceXProductsToolStripMenuItem1,
            this.toolStripSeparator2,
            this.markupSettingsToolStripMenuItem1,
            this.restrictedBrandsToolStripMenuItem1,
            this.restrictedSKUsToolStripMenuItem1,
            this.fixedPricesToolStripMenuItem1});
            this.walmartToolStripMenuItem.Name = "walmartToolStripMenuItem";
            this.walmartToolStripMenuItem.Size = new System.Drawing.Size(79, 24);
            this.walmartToolStripMenuItem.Text = "Walmart";
            // 
            // loadThePerfumeSpotProductsToolStripMenuItem
            // 
            this.loadThePerfumeSpotProductsToolStripMenuItem.Name = "loadThePerfumeSpotProductsToolStripMenuItem";
            this.loadThePerfumeSpotProductsToolStripMenuItem.Size = new System.Drawing.Size(307, 26);
            this.loadThePerfumeSpotProductsToolStripMenuItem.Text = "Load The Perfume Spot Products";
            this.loadThePerfumeSpotProductsToolStripMenuItem.Click += new System.EventHandler(this.loadThePerfumeSpotProductsToolStripMenuItem_Click);
            // 
            // fetchFragranceXProductsToolStripMenuItem1
            // 
            this.fetchFragranceXProductsToolStripMenuItem1.Name = "fetchFragranceXProductsToolStripMenuItem1";
            this.fetchFragranceXProductsToolStripMenuItem1.Size = new System.Drawing.Size(307, 26);
            this.fetchFragranceXProductsToolStripMenuItem1.Text = "Fetch FragranceX Products";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(304, 6);
            // 
            // markupSettingsToolStripMenuItem1
            // 
            this.markupSettingsToolStripMenuItem1.Name = "markupSettingsToolStripMenuItem1";
            this.markupSettingsToolStripMenuItem1.Size = new System.Drawing.Size(307, 26);
            this.markupSettingsToolStripMenuItem1.Text = "Markup Settings";
            this.markupSettingsToolStripMenuItem1.Click += new System.EventHandler(this.markupSettingsToolStripMenuItem1_Click);
            // 
            // restrictedBrandsToolStripMenuItem1
            // 
            this.restrictedBrandsToolStripMenuItem1.Name = "restrictedBrandsToolStripMenuItem1";
            this.restrictedBrandsToolStripMenuItem1.Size = new System.Drawing.Size(307, 26);
            this.restrictedBrandsToolStripMenuItem1.Text = "Restricted Brands";
            this.restrictedBrandsToolStripMenuItem1.Click += new System.EventHandler(this.restrictedBrandsToolStripMenuItem1_Click);
            // 
            // restrictedSKUsToolStripMenuItem1
            // 
            this.restrictedSKUsToolStripMenuItem1.Name = "restrictedSKUsToolStripMenuItem1";
            this.restrictedSKUsToolStripMenuItem1.Size = new System.Drawing.Size(307, 26);
            this.restrictedSKUsToolStripMenuItem1.Text = "Restricted SKUs";
            this.restrictedSKUsToolStripMenuItem1.Click += new System.EventHandler(this.restrictedSKUsToolStripMenuItem1_Click);
            // 
            // fixedPricesToolStripMenuItem1
            // 
            this.fixedPricesToolStripMenuItem1.Name = "fixedPricesToolStripMenuItem1";
            this.fixedPricesToolStripMenuItem1.Size = new System.Drawing.Size(307, 26);
            this.fixedPricesToolStripMenuItem1.Text = "Fixed Prices";
            this.fixedPricesToolStripMenuItem1.Click += new System.EventHandler(this.fixedPricesToolStripMenuItem1_Click);
            // 
            // settingsToolStripMenuItem1
            // 
            this.settingsToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem2});
            this.settingsToolStripMenuItem1.Name = "settingsToolStripMenuItem1";
            this.settingsToolStripMenuItem1.Size = new System.Drawing.Size(76, 24);
            this.settingsToolStripMenuItem1.Text = "Settings";
            // 
            // settingsToolStripMenuItem2
            // 
            this.settingsToolStripMenuItem2.Name = "settingsToolStripMenuItem2";
            this.settingsToolStripMenuItem2.Size = new System.Drawing.Size(226, 26);
            this.settingsToolStripMenuItem2.Text = "Application Settings";
            this.settingsToolStripMenuItem2.Click += new System.EventHandler(this.settingsToolStripMenuItem2_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(55, 24);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(133, 26);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItem_Click);
            // 
            // ProcessCSVForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "ProcessCSVForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Shopify Inventory Feed";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.loadedDataGridView)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private Button btnProcess;
        private ProgressBar ProcessProgress;
        private TableLayoutPanel tableLayoutPanel3;
        private DataGridView loadedDataGridView;
        private TextBox txtProcessedProducts;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem markupSettingsToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private Button btnClear;
        private ToolStripMenuItem restrictedBrandsToolStripMenuItem;
        private ToolStripMenuItem restrictedSKUsToolStripMenuItem;
        private Label lblProgressCount;
        private ToolStripMenuItem fetchFragranceXProductsToolStripMenuItem;
        private Label lblSelectedAPI;
        private ToolStripMenuItem fixedPricesToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem fetchFragranceNetProductsToolStripMenuItem;
        private ToolStripMenuItem walmartToolStripMenuItem;
        private ToolStripMenuItem settingsToolStripMenuItem1;
        private ToolStripMenuItem settingsToolStripMenuItem2;
        private ToolStripMenuItem loadThePerfumeSpotProductsToolStripMenuItem;
        private ToolStripMenuItem fetchFragranceXProductsToolStripMenuItem1;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem markupSettingsToolStripMenuItem1;
        private ToolStripMenuItem restrictedBrandsToolStripMenuItem1;
        private ToolStripMenuItem restrictedSKUsToolStripMenuItem1;
        private ToolStripMenuItem fixedPricesToolStripMenuItem1;
    }
}