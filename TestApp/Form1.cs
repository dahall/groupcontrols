using System.Windows.Forms;

namespace TestApp;

public partial class Form1 : Form
{
	public Form1()
	{
		InitializeComponent();
		/*radioButtonList1.Items.Add("&Eddie", null, "&Frank", null, "&Gerald", null, "&Henry", null, "&Irvin", null,
			"&James", null, "&Kent", null, "&Larry", null, "&Matt", null, "&Norman", null, "&Oscar", null,
			"&Peter", null, "&Quinton", null, "&Ralph", null, "&Steve", null, "&Tom", null, "&Ungar", null,
			"&Victor", null, "&Walter", null, "&Xen", null, "&Yves", null, "&Zephyr", null);*/
	}

	private void checkBoxList1_ItemCheckStateChanged(object sender, GroupControls.CheckBoxListItemCheckStateChangedEventArgs e)
	{
		statusLabel.Text = $"{((Control)sender).Name} item {e.ItemIndex} checked = {e.Item.Checked}";
	}

	private void radioButtonList2_SelectedIndexChanged(object sender, System.EventArgs e)
	{
		statusLabel.Text = $"{((Control)sender).Name} selectedIndex {radioButtonList2.SelectedIndex}";
	}
}
