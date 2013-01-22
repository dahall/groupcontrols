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
			GroupControls.RadioButtonListItem radioButtonListItem1 = new GroupControls.RadioButtonListItem();
			GroupControls.RadioButtonListItem radioButtonListItem2 = new GroupControls.RadioButtonListItem();
			GroupControls.RadioButtonListItem radioButtonListItem3 = new GroupControls.RadioButtonListItem();
			GroupControls.RadioButtonListItem radioButtonListItem4 = new GroupControls.RadioButtonListItem();
			GroupControls.CheckBoxListItem checkBoxListItem1 = new GroupControls.CheckBoxListItem();
			GroupControls.CheckBoxListItem checkBoxListItem2 = new GroupControls.CheckBoxListItem();
			GroupControls.CheckBoxListItem checkBoxListItem3 = new GroupControls.CheckBoxListItem();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this.radioButtonList1 = new GroupControls.RadioButtonList();
			this.checkBoxList1 = new GroupControls.CheckBoxList();
			this.SuspendLayout();
			// 
			// radioButtonList1
			// 
			this.radioButtonList1.AutoScrollMinSize = new System.Drawing.Size(210, 51);
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
			this.radioButtonList1.ItemSpacing = new System.Drawing.Size(0, 7);
			this.radioButtonList1.Location = new System.Drawing.Point(325, 12);
			this.radioButtonList1.Name = "radioButtonList1";
			this.radioButtonList1.RepeatColumns = 2;
			this.radioButtonList1.Size = new System.Drawing.Size(210, 51);
			this.radioButtonList1.TabIndex = 3;
			// 
			// checkBoxList1
			// 
			this.checkBoxList1.AutoScrollMinSize = new System.Drawing.Size(307, 250);
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
			this.checkBoxList1.ItemSpacing = new System.Drawing.Size(0, 7);
			this.checkBoxList1.Location = new System.Drawing.Point(12, 12);
			this.checkBoxList1.Name = "checkBoxList1";
			this.checkBoxList1.Size = new System.Drawing.Size(307, 250);
			this.checkBoxList1.SubtextFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkBoxList1.TabIndex = 2;
			this.checkBoxList1.Text = "checkBoxList1";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(547, 352);
			this.Controls.Add(this.radioButtonList1);
			this.Controls.Add(this.checkBoxList1);
			this.Name = "Form1";
			this.Text = "Test GroupControls";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private GroupControls.CheckBoxList checkBoxList1;
		private GroupControls.RadioButtonList radioButtonList1;
	}
}

