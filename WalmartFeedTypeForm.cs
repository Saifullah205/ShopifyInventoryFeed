using ShopifyInventorySync.BusinessLogic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ShopifyInventorySync
{
    public partial class WalmartFeedTypeForm : Form
    {
        public GlobalConstants.WALMARTFEEDTYPEPOST selectedFeedType;

        public WalmartFeedTypeForm()
        {
            InitializeComponent();

            fillWalmartFeedType();
        }

        private void fillWalmartFeedType()
        {

            Dictionary<int, string> walmartFeedTypes = new();

            try
            {
                walmartFeedTypes.Add(1, "Remove products");
                walmartFeedTypes.Add(2, "Setup Products");
                walmartFeedTypes.Add(3, "Push Inventory Feed");
                walmartFeedTypes.Add(4, "Mark Out of Stock");

                cmbWalmartFeedType.DataSource = new BindingSource(walmartFeedTypes, null);
                cmbWalmartFeedType.DisplayMember = "Value";
                cmbWalmartFeedType.ValueMember = "Key";

                cmbWalmartFeedType.DropDownStyle = ComboBoxStyle.DropDownList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                selectedFeedType = (GlobalConstants.WALMARTFEEDTYPEPOST)((KeyValuePair<int, string>)(cmbWalmartFeedType.SelectedItem)).Key;

                this.Close();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
