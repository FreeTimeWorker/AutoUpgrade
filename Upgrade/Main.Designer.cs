
namespace Upgrade
{
    partial class Main
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
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.btnStartExe = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.jump = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(12, 40);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(606, 68);
            this.progressBar.TabIndex = 0;
            // 
            // btnStartExe
            // 
            this.btnStartExe.Location = new System.Drawing.Point(497, 133);
            this.btnStartExe.Name = "btnStartExe";
            this.btnStartExe.Size = new System.Drawing.Size(121, 38);
            this.btnStartExe.TabIndex = 1;
            this.btnStartExe.Text = "启动";
            this.btnStartExe.UseVisualStyleBackColor = true;
            this.btnStartExe.Click += new System.EventHandler(this.btnStartExe_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "下载文件中...";
            // 
            // jump
            // 
            this.jump.Location = new System.Drawing.Point(12, 133);
            this.jump.Name = "jump";
            this.jump.Size = new System.Drawing.Size(121, 38);
            this.jump.TabIndex = 1;
            this.jump.Text = "跳过本次更新";
            this.jump.UseVisualStyleBackColor = true;
            this.jump.Click += new System.EventHandler(this.btnStartExe_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(630, 187);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.jump);
            this.Controls.Add(this.btnStartExe);
            this.Controls.Add(this.progressBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Main";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "新版本更新";
            this.Load += new System.EventHandler(this.Main_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Button btnStartExe;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button jump;
    }
}

