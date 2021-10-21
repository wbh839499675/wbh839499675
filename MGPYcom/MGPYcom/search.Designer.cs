
namespace MGPYcom
{
    partial class searchForm
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
            this.button_searchNext = new System.Windows.Forms.Button();
            this.button_cancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_keywords = new System.Windows.Forms.TextBox();
            this.groupBox_searchDirection = new System.Windows.Forms.GroupBox();
            this.radioButton_down = new System.Windows.Forms.RadioButton();
            this.radioButton_up = new System.Windows.Forms.RadioButton();
            this.checkBox_wholeWord = new System.Windows.Forms.CheckBox();
            this.checkBox_upperLower = new System.Windows.Forms.CheckBox();
            this.groupBox_searchDirection.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_searchNext
            // 
            this.button_searchNext.Enabled = false;
            this.button_searchNext.Location = new System.Drawing.Point(416, 26);
            this.button_searchNext.Name = "button_searchNext";
            this.button_searchNext.Size = new System.Drawing.Size(105, 34);
            this.button_searchNext.TabIndex = 0;
            this.button_searchNext.Text = "查找下一个(&F)";
            this.button_searchNext.UseVisualStyleBackColor = true;
            this.button_searchNext.Click += new System.EventHandler(this.button_searchNext_Click);
            // 
            // button_cancel
            // 
            this.button_cancel.Location = new System.Drawing.Point(416, 75);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(105, 34);
            this.button_cancel.TabIndex = 1;
            this.button_cancel.Text = "取消";
            this.button_cancel.UseVisualStyleBackColor = true;
            this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(12, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "查找内容(N)";
            // 
            // textBox_keywords
            // 
            this.textBox_keywords.Location = new System.Drawing.Point(90, 35);
            this.textBox_keywords.Name = "textBox_keywords";
            this.textBox_keywords.Size = new System.Drawing.Size(297, 21);
            this.textBox_keywords.TabIndex = 3;
            // 
            // groupBox_searchDirection
            // 
            this.groupBox_searchDirection.Controls.Add(this.radioButton_down);
            this.groupBox_searchDirection.Controls.Add(this.radioButton_up);
            this.groupBox_searchDirection.Location = new System.Drawing.Point(233, 87);
            this.groupBox_searchDirection.Name = "groupBox_searchDirection";
            this.groupBox_searchDirection.Size = new System.Drawing.Size(177, 66);
            this.groupBox_searchDirection.TabIndex = 4;
            this.groupBox_searchDirection.TabStop = false;
            this.groupBox_searchDirection.Text = "方向";
            // 
            // radioButton_down
            // 
            this.radioButton_down.AutoSize = true;
            this.radioButton_down.Checked = true;
            this.radioButton_down.Location = new System.Drawing.Point(89, 37);
            this.radioButton_down.Name = "radioButton_down";
            this.radioButton_down.Size = new System.Drawing.Size(65, 16);
            this.radioButton_down.TabIndex = 7;
            this.radioButton_down.TabStop = true;
            this.radioButton_down.Text = "向下(D)";
            this.radioButton_down.UseVisualStyleBackColor = true;
            this.radioButton_down.CheckedChanged += new System.EventHandler(this.radioButton_down_CheckedChanged);
            // 
            // radioButton_up
            // 
            this.radioButton_up.AutoSize = true;
            this.radioButton_up.Location = new System.Drawing.Point(7, 37);
            this.radioButton_up.Name = "radioButton_up";
            this.radioButton_up.Size = new System.Drawing.Size(65, 16);
            this.radioButton_up.TabIndex = 0;
            this.radioButton_up.TabStop = true;
            this.radioButton_up.Text = "向上(U)";
            this.radioButton_up.UseVisualStyleBackColor = true;
            this.radioButton_up.CheckedChanged += new System.EventHandler(this.radioButton_up_CheckedChanged);
            // 
            // checkBox_wholeWord
            // 
            this.checkBox_wholeWord.AutoSize = true;
            this.checkBox_wholeWord.Location = new System.Drawing.Point(14, 87);
            this.checkBox_wholeWord.Name = "checkBox_wholeWord";
            this.checkBox_wholeWord.Size = new System.Drawing.Size(90, 16);
            this.checkBox_wholeWord.TabIndex = 5;
            this.checkBox_wholeWord.Text = "全词匹配(W)";
            this.checkBox_wholeWord.UseVisualStyleBackColor = true;
            this.checkBox_wholeWord.CheckedChanged += new System.EventHandler(this.checkBox_wholeWord_CheckedChanged);
            // 
            // checkBox_upperLower
            // 
            this.checkBox_upperLower.AutoSize = true;
            this.checkBox_upperLower.Location = new System.Drawing.Point(14, 125);
            this.checkBox_upperLower.Name = "checkBox_upperLower";
            this.checkBox_upperLower.Size = new System.Drawing.Size(102, 16);
            this.checkBox_upperLower.TabIndex = 6;
            this.checkBox_upperLower.Text = "区分大小写(C)";
            this.checkBox_upperLower.UseVisualStyleBackColor = true;
            this.checkBox_upperLower.CheckedChanged += new System.EventHandler(this.checkBox_upperLower_CheckedChanged);
            // 
            // searchForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(533, 182);
            this.Controls.Add(this.checkBox_upperLower);
            this.Controls.Add(this.checkBox_wholeWord);
            this.Controls.Add(this.groupBox_searchDirection);
            this.Controls.Add(this.textBox_keywords);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.button_searchNext);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "searchForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Search";
            this.groupBox_searchDirection.ResumeLayout(false);
            this.groupBox_searchDirection.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_searchNext;
        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_keywords;
        private System.Windows.Forms.GroupBox groupBox_searchDirection;
        private System.Windows.Forms.CheckBox checkBox_wholeWord;
        private System.Windows.Forms.CheckBox checkBox_upperLower;
        private System.Windows.Forms.RadioButton radioButton_down;
        private System.Windows.Forms.RadioButton radioButton_up;
    }
}