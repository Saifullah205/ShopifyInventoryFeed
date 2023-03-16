using static ShopifyInventorySync.BusinessLogic.GlobalConstants;

namespace ShopifyInventorySync
{
    public partial class WalmartFeedTypeForm : Form
    {
        public WALMARTFEEDTYPEPOST selectedFeedType;
        public bool selectionCancelled = true;

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
                walmartFeedTypes.Add(3, "Map Shipping Template");
                walmartFeedTypes.Add(4, "Push Inventory Feed");

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
                selectedFeedType = (WALMARTFEEDTYPEPOST)((KeyValuePair<int, string>)(cmbWalmartFeedType.SelectedItem)).Key;

                selectionCancelled = false;

                this.Close();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
