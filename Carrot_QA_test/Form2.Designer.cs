
namespace Carrot_QA_test
{
    partial class Form2
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.QA1_label = new System.Windows.Forms.Label();
            this.QA2_label = new System.Windows.Forms.Label();
            this.QA3_label = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.Sn_label = new System.Windows.Forms.Label();
            this.did_label = new System.Windows.Forms.Label();
            this.prodata_labal = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Font = new System.Drawing.Font("굴림", 16F);
            this.textBox1.Location = new System.Drawing.Point(242, 13);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(552, 44);
            this.textBox1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("굴림", 20F);
            this.label1.Location = new System.Drawing.Point(12, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(228, 40);
            this.label1.TabIndex = 1;
            this.label1.Text = "QR CODE :";
            // 
            // QA1_label
            // 
            this.QA1_label.AutoSize = true;
            this.QA1_label.Font = new System.Drawing.Font("굴림", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.QA1_label.Location = new System.Drawing.Point(12, 265);
            this.QA1_label.Name = "QA1_label";
            this.QA1_label.Size = new System.Drawing.Size(289, 40);
            this.QA1_label.TabIndex = 2;
            this.QA1_label.Text = "QA1 : No Data";
            // 
            // QA2_label
            // 
            this.QA2_label.AutoSize = true;
            this.QA2_label.Font = new System.Drawing.Font("굴림", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.QA2_label.Location = new System.Drawing.Point(12, 325);
            this.QA2_label.Name = "QA2_label";
            this.QA2_label.Size = new System.Drawing.Size(289, 40);
            this.QA2_label.TabIndex = 3;
            this.QA2_label.Text = "QA2 : No Data";
            // 
            // QA3_label
            // 
            this.QA3_label.AutoSize = true;
            this.QA3_label.Font = new System.Drawing.Font("굴림", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.QA3_label.Location = new System.Drawing.Point(12, 384);
            this.QA3_label.Name = "QA3_label";
            this.QA3_label.Size = new System.Drawing.Size(289, 40);
            this.QA3_label.TabIndex = 4;
            this.QA3_label.Text = "QA3 : No Data";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(815, 18);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(148, 39);
            this.button1.TabIndex = 5;
            this.button1.Text = "Close";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Sn_label
            // 
            this.Sn_label.AutoSize = true;
            this.Sn_label.Font = new System.Drawing.Font("굴림", 20F);
            this.Sn_label.Location = new System.Drawing.Point(12, 143);
            this.Sn_label.Name = "Sn_label";
            this.Sn_label.Size = new System.Drawing.Size(304, 40);
            this.Sn_label.TabIndex = 6;
            this.Sn_label.Text = "Serial Number :";
            // 
            // did_label
            // 
            this.did_label.AutoSize = true;
            this.did_label.Font = new System.Drawing.Font("굴림", 20F);
            this.did_label.Location = new System.Drawing.Point(12, 84);
            this.did_label.Name = "did_label";
            this.did_label.Size = new System.Drawing.Size(224, 40);
            this.did_label.TabIndex = 7;
            this.did_label.Text = "Devcie ID :";
            // 
            // prodata_labal
            // 
            this.prodata_labal.AutoSize = true;
            this.prodata_labal.Font = new System.Drawing.Font("굴림", 20F);
            this.prodata_labal.Location = new System.Drawing.Point(12, 205);
            this.prodata_labal.Name = "prodata_labal";
            this.prodata_labal.Size = new System.Drawing.Size(289, 40);
            this.prodata_labal.TabIndex = 8;
            this.prodata_labal.Text = "Product Date :";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(19, 444);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(934, 71);
            this.progressBar1.TabIndex = 9;
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(975, 542);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.prodata_labal);
            this.Controls.Add(this.did_label);
            this.Controls.Add(this.Sn_label);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.QA3_label);
            this.Controls.Add(this.QA2_label);
            this.Controls.Add(this.QA1_label);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox1);
            this.Name = "Form2";
            this.Text = "QA Code Scan";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label QA1_label;
        private System.Windows.Forms.Label QA2_label;
        private System.Windows.Forms.Label QA3_label;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label Sn_label;
        private System.Windows.Forms.Label did_label;
        private System.Windows.Forms.Label prodata_labal;
        private System.Windows.Forms.ProgressBar progressBar1;
    }
}