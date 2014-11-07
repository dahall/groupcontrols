namespace TestApp
{
	partial class Form1
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
			GroupControls.CheckBoxListItem checkBoxListItem1 = new GroupControls.CheckBoxListItem();
			GroupControls.CheckBoxListItem checkBoxListItem2 = new GroupControls.CheckBoxListItem();
			GroupControls.CheckBoxListItem checkBoxListItem3 = new GroupControls.CheckBoxListItem();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			GroupControls.RadioButtonListItem radioButtonListItem5 = new GroupControls.RadioButtonListItem();
			GroupControls.RadioButtonListItem radioButtonListItem6 = new GroupControls.RadioButtonListItem();
			GroupControls.RadioButtonListItem radioButtonListItem7 = new GroupControls.RadioButtonListItem();
			GroupControls.RadioButtonListItem radioButtonListItem8 = new GroupControls.RadioButtonListItem();
			GroupControls.RadioButtonListItem radioButtonListItem9 = new GroupControls.RadioButtonListItem();
			GroupControls.RadioButtonListItem radioButtonListItem10 = new GroupControls.RadioButtonListItem();
			GroupControls.RadioButtonListItem radioButtonListItem11 = new GroupControls.RadioButtonListItem();
			GroupControls.RadioButtonListItem radioButtonListItem12 = new GroupControls.RadioButtonListItem();
			GroupControls.RadioButtonListItem radioButtonListItem13 = new GroupControls.RadioButtonListItem();
			GroupControls.RadioButtonListItem radioButtonListItem14 = new GroupControls.RadioButtonListItem();
			GroupControls.RadioButtonListItem radioButtonListItem1 = new GroupControls.RadioButtonListItem();
			GroupControls.RadioButtonListItem radioButtonListItem2 = new GroupControls.RadioButtonListItem();
			GroupControls.RadioButtonListItem radioButtonListItem3 = new GroupControls.RadioButtonListItem();
			GroupControls.RadioButtonListItem radioButtonListItem4 = new GroupControls.RadioButtonListItem();
			this.radioButton1 = new System.Windows.Forms.RadioButton();
			this.checkBoxList1 = new GroupControls.CheckBoxList();
			this.radioButtonList2 = new GroupControls.RadioButtonList();
			this.radioButtonList1 = new GroupControls.RadioButtonList();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.checkBoxList1.SuspendLayout();
			this.radioButtonList2.SuspendLayout();
			this.radioButtonList1.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.statusStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// radioButton1
			// 
			this.radioButton1.AutoSize = true;
			this.radioButton1.Location = new System.Drawing.Point(335, 153);
			this.radioButton1.Name = "radioButton1";
			this.radioButton1.Size = new System.Drawing.Size(85, 17);
			this.radioButton1.TabIndex = 8;
			this.radioButton1.TabStop = true;
			this.radioButton1.Text = "radioButton1";
			this.radioButton1.UseVisualStyleBackColor = true;
			// 
			// checkBoxList1
			// 
			this.checkBoxList1.AutoScrollMinSize = new System.Drawing.Size(309, 254);
			this.checkBoxList1.AutoSize = false;
			this.checkBoxList1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.checkBoxList1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			checkBoxListItem1.CheckState = System.Windows.Forms.CheckState.Checked;
			checkBoxListItem1.Subtext = "Some very long text to test the subtext property and to ensure that it wraps as i" +
    "t should and is properly positioned, selected and displayed.";
			checkBoxListItem1.Text = "&One";
			checkBoxListItem1.ToolTipText = "1";
			checkBoxListItem2.Enabled = false;
			checkBoxListItem2.Subtext = "Shorter text";
			checkBoxListItem2.Text = "&Two";
			checkBoxListItem2.ToolTipText = "2";
			checkBoxListItem3.Subtext = resources.GetString("checkBoxListItem3.Subtext");
			checkBoxListItem3.Text = "T&hree";
			checkBoxListItem3.ToolTipText = "3";
			this.checkBoxList1.Items.Add(checkBoxListItem1);
			this.checkBoxList1.Items.Add(checkBoxListItem2);
			this.checkBoxList1.Items.Add(checkBoxListItem3);
			this.checkBoxList1.Location = new System.Drawing.Point(3, 3);
			this.checkBoxList1.Name = "checkBoxList1";
			this.checkBoxList1.Padding = new System.Windows.Forms.Padding(3);
			this.tableLayoutPanel1.SetRowSpan(this.checkBoxList1, 3);
			this.checkBoxList1.Size = new System.Drawing.Size(326, 190);
			this.checkBoxList1.SubtextFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkBoxList1.TabIndex = 6;
			this.checkBoxList1.Text = "checkBoxList1";
			this.checkBoxList1.ItemCheckStateChanged += new System.EventHandler<GroupControls.CheckBoxListItemCheckStateChangedEventArgs>(this.checkBoxList1_ItemCheckStateChanged);
			// 
			// radioButtonList2
			// 
			this.radioButtonList2.AutoScrollMinSize = new System.Drawing.Size(566, 59);
			this.radioButtonList2.AutoSize = false;
			this.radioButtonList2.CheckAlign = System.Drawing.ContentAlignment.TopCenter;
			this.radioButtonList2.Dock = System.Windows.Forms.DockStyle.Bottom;
			radioButtonListItem5.Subtext = "Very important";
			radioButtonListItem5.Text = "&1";
			radioButtonListItem5.ToolTipText = "Mucho importante";
			radioButtonListItem6.Text = "&2";
			radioButtonListItem7.Text = "&3";
			radioButtonListItem8.Text = "&4";
			radioButtonListItem9.Text = "&5";
			radioButtonListItem10.Text = "&6";
			radioButtonListItem11.Text = "&7";
			radioButtonListItem12.Text = "&8";
			radioButtonListItem13.Text = "&9";
			radioButtonListItem14.Subtext = "Very unimportant";
			radioButtonListItem14.Text = "1&0";
			radioButtonListItem14.ToolTipText = "Poco importante";
			this.radioButtonList2.Items.Add(radioButtonListItem5);
			this.radioButtonList2.Items.Add(radioButtonListItem6);
			this.radioButtonList2.Items.Add(radioButtonListItem7);
			this.radioButtonList2.Items.Add(radioButtonListItem8);
			this.radioButtonList2.Items.Add(radioButtonListItem9);
			this.radioButtonList2.Items.Add(radioButtonListItem10);
			this.radioButtonList2.Items.Add(radioButtonListItem11);
			this.radioButtonList2.Items.Add(radioButtonListItem12);
			this.radioButtonList2.Items.Add(radioButtonListItem13);
			this.radioButtonList2.Items.Add(radioButtonListItem14);
			this.radioButtonList2.Location = new System.Drawing.Point(0, 200);
			this.radioButtonList2.Name = "radioButtonList2";
			this.radioButtonList2.RepeatColumns = 10;
			this.radioButtonList2.RepeatDirection = GroupControls.RepeatDirection.Horizontal;
			this.radioButtonList2.Size = new System.Drawing.Size(566, 68);
			this.radioButtonList2.TabIndex = 7;
			this.radioButtonList2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.radioButtonList2.SelectedIndexChanged += new System.EventHandler(this.radioButtonList2_SelectedIndexChanged);
			// 
			// radioButtonList1
			// 
			this.radioButtonList1.AutoScrollMinSize = new System.Drawing.Size(228, 56);
			this.radioButtonList1.Dock = System.Windows.Forms.DockStyle.Top;
			radioButtonListItem1.Enabled = false;
			radioButtonListItem1.Subtext = "A hip dude";
			radioButtonListItem1.Text = "&Al";
			radioButtonListItem1.ToolTipText = "No, really!";
			radioButtonListItem2.Text = "&Bob";
			radioButtonListItem3.Enabled = false;
			radioButtonListItem3.Text = "&Chuck";
			radioButtonListItem4.Subtext = "Even hipper";
			radioButtonListItem4.Text = "&Dave";
			this.radioButtonList1.Items.Add(radioButtonListItem1);
			this.radioButtonList1.Items.Add(radioButtonListItem2);
			this.radioButtonList1.Items.Add(radioButtonListItem3);
			this.radioButtonList1.Items.Add(radioButtonListItem4);
			this.radioButtonList1.Location = new System.Drawing.Point(335, 3);
			this.radioButtonList1.Name = "radioButtonList1";
			this.radioButtonList1.Padding = new System.Windows.Forms.Padding(3);
			this.radioButtonList1.RepeatColumns = 2;
			this.radioButtonList1.RepeatDirection = GroupControls.RepeatDirection.Horizontal;
			this.radioButtonList1.Size = new System.Drawing.Size(228, 56);
			this.radioButtonList1.TabIndex = 3;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 58.82353F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 41.17647F));
			this.tableLayoutPanel1.Controls.Add(this.checkBoxList1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.radioButton1, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.radioButtonList1, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.checkBox1, 1, 2);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(0, 0, 0, 4);
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(566, 200);
			this.tableLayoutPanel1.TabIndex = 9;
			// 
			// statusStrip1
			// 
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
			this.statusStrip1.Location = new System.Drawing.Point(0, 268);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(566, 22);
			this.statusStrip1.TabIndex = 10;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// statusLabel
			// 
			this.statusLabel.Name = "statusLabel";
			this.statusLabel.Size = new System.Drawing.Size(0, 17);
			// 
			// checkBox1
			// 
			this.checkBox1.AutoSize = true;
			this.checkBox1.Location = new System.Drawing.Point(335, 176);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size(80, 17);
			this.checkBox1.TabIndex = 9;
			this.checkBox1.Text = "checkBox1";
			this.checkBox1.UseVisualStyleBackColor = true;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(566, 290);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Controls.Add(this.radioButtonList2);
			this.Controls.Add(this.statusStrip1);
			this.Name = "Form1";
			this.Text = "Test GroupControls";
			this.checkBoxList1.ResumeLayout(true);
			this.radioButtonList2.ResumeLayout(true);
			this.radioButtonList1.ResumeLayout(true);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private GroupControls.RadioButtonList radioButtonList1;
		private GroupControls.CheckBoxList checkBoxList1;
		private GroupControls.RadioButtonList radioButtonList2;
		private System.Windows.Forms.RadioButton radioButton1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ToolStripStatusLabel statusLabel;
		private System.Windows.Forms.CheckBox checkBox1;
	}
}

