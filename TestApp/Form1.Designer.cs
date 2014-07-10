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
			GroupControls.RadioButtonListItem radioButtonListItem1 = new GroupControls.RadioButtonListItem();
			GroupControls.RadioButtonListItem radioButtonListItem2 = new GroupControls.RadioButtonListItem();
			GroupControls.RadioButtonListItem radioButtonListItem3 = new GroupControls.RadioButtonListItem();
			GroupControls.RadioButtonListItem radioButtonListItem4 = new GroupControls.RadioButtonListItem();
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
			this.radioButton1 = new System.Windows.Forms.RadioButton();
			this.checkBoxList1 = new GroupControls.CheckBoxList();
			this.radioButtonList2 = new GroupControls.RadioButtonList();
			this.radioButtonList1 = new GroupControls.RadioButtonList();
			this.checkBoxList1.SuspendLayout();
			this.radioButtonList2.SuspendLayout();
			this.radioButtonList1.SuspendLayout();
			this.SuspendLayout();
			// 
			// radioButton1
			// 
			this.radioButton1.AutoSize = true;
			this.radioButton1.Location = new System.Drawing.Point(451, 210);
			this.radioButton1.Name = "radioButton1";
			this.radioButton1.Size = new System.Drawing.Size(85, 17);
			this.radioButton1.TabIndex = 8;
			this.radioButton1.TabStop = true;
			this.radioButton1.Text = "radioButton1";
			this.radioButton1.UseVisualStyleBackColor = true;
			// 
			// checkBoxList1
			// 
			this.checkBoxList1.AutoScrollMinSize = new System.Drawing.Size(293, 267);
			this.checkBoxList1.AutoSize = false;
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
			this.checkBoxList1.Location = new System.Drawing.Point(0, 0);
			this.checkBoxList1.Name = "checkBoxList1";
			this.checkBoxList1.Padding = new System.Windows.Forms.Padding(3);
			this.checkBoxList1.Size = new System.Drawing.Size(310, 227);
			this.checkBoxList1.SubtextFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkBoxList1.TabIndex = 6;
			this.checkBoxList1.Text = "checkBoxList1";
			// 
			// radioButtonList2
			// 
			this.radioButtonList2.AutoScrollMinSize = new System.Drawing.Size(612, 59);
			this.radioButtonList2.AutoSize = false;
			this.radioButtonList2.CheckAlign = System.Drawing.ContentAlignment.TopCenter;
			this.radioButtonList2.Dock = System.Windows.Forms.DockStyle.Bottom;
			radioButtonListItem1.Subtext = "Very important";
			radioButtonListItem1.Text = "&1";
			radioButtonListItem2.Text = "&2";
			radioButtonListItem3.Text = "&3";
			radioButtonListItem4.Text = "&4";
			radioButtonListItem5.Text = "&5";
			radioButtonListItem6.Text = "&6";
			radioButtonListItem7.Text = "&7";
			radioButtonListItem8.Text = "&8";
			radioButtonListItem9.Text = "&9";
			radioButtonListItem10.Subtext = "Very unimportant";
			radioButtonListItem10.Text = "1&0";
			this.radioButtonList2.Items.Add(radioButtonListItem1);
			this.radioButtonList2.Items.Add(radioButtonListItem2);
			this.radioButtonList2.Items.Add(radioButtonListItem3);
			this.radioButtonList2.Items.Add(radioButtonListItem4);
			this.radioButtonList2.Items.Add(radioButtonListItem5);
			this.radioButtonList2.Items.Add(radioButtonListItem6);
			this.radioButtonList2.Items.Add(radioButtonListItem7);
			this.radioButtonList2.Items.Add(radioButtonListItem8);
			this.radioButtonList2.Items.Add(radioButtonListItem9);
			this.radioButtonList2.Items.Add(radioButtonListItem10);
			this.radioButtonList2.Location = new System.Drawing.Point(0, 260);
			this.radioButtonList2.Name = "radioButtonList2";
			this.radioButtonList2.RepeatColumns = 10;
			this.radioButtonList2.RepeatDirection = GroupControls.RepeatDirection.Horizontal;
			this.radioButtonList2.Size = new System.Drawing.Size(612, 68);
			this.radioButtonList2.TabIndex = 7;
			this.radioButtonList2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// radioButtonList1
			// 
			this.radioButtonList1.AutoScrollMinSize = new System.Drawing.Size(252, 56);
			radioButtonListItem11.Enabled = false;
			radioButtonListItem11.Subtext = "A hip dude";
			radioButtonListItem11.Text = "&Al";
			radioButtonListItem11.ToolTipText = "No, really!";
			radioButtonListItem12.Text = "&Bob";
			radioButtonListItem13.Enabled = false;
			radioButtonListItem13.Text = "&Chuck";
			radioButtonListItem14.Subtext = "Even hipper";
			radioButtonListItem14.Text = "&Dave";
			this.radioButtonList1.Items.Add(radioButtonListItem11);
			this.radioButtonList1.Items.Add(radioButtonListItem12);
			this.radioButtonList1.Items.Add(radioButtonListItem13);
			this.radioButtonList1.Items.Add(radioButtonListItem14);
			this.radioButtonList1.Location = new System.Drawing.Point(348, 0);
			this.radioButtonList1.Name = "radioButtonList1";
			this.radioButtonList1.Padding = new System.Windows.Forms.Padding(3);
			this.radioButtonList1.RepeatColumns = 2;
			this.radioButtonList1.RepeatDirection = GroupControls.RepeatDirection.Horizontal;
			this.radioButtonList1.Size = new System.Drawing.Size(252, 56);
			this.radioButtonList1.TabIndex = 3;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(612, 328);
			this.Controls.Add(this.radioButton1);
			this.Controls.Add(this.checkBoxList1);
			this.Controls.Add(this.radioButtonList2);
			this.Controls.Add(this.radioButtonList1);
			this.Name = "Form1";
			this.Text = "Test GroupControls";
			this.checkBoxList1.ResumeLayout(true);
			this.radioButtonList2.ResumeLayout(true);
			this.radioButtonList1.ResumeLayout(true);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private GroupControls.RadioButtonList radioButtonList1;
		private GroupControls.CheckBoxList checkBoxList1;
		private GroupControls.RadioButtonList radioButtonList2;
		private System.Windows.Forms.RadioButton radioButton1;
	}
}

