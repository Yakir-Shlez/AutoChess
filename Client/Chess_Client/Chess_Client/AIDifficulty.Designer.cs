namespace Chess_Client
{
    partial class AIDifficulty
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
            this.label1 = new System.Windows.Forms.Label();
            this.confirmButton = new System.Windows.Forms.Button();
            this.difficultyUpDown = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.difficultyUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F);
            this.label1.Location = new System.Drawing.Point(105, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(190, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "Choose AI dificulty";
            // 
            // confirmButton
            // 
            this.confirmButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F);
            this.confirmButton.Location = new System.Drawing.Point(140, 110);
            this.confirmButton.Name = "confirmButton";
            this.confirmButton.Size = new System.Drawing.Size(120, 40);
            this.confirmButton.TabIndex = 1;
            this.confirmButton.Text = "Confirm";
            this.confirmButton.UseVisualStyleBackColor = true;
            this.confirmButton.Click += new System.EventHandler(this.confirmButton_Click);
            // 
            // difficultyUpDown
            // 
            this.difficultyUpDown.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F);
            this.difficultyUpDown.Location = new System.Drawing.Point(175, 60);
            this.difficultyUpDown.Maximum = new decimal(new int[] {
            9,
            0,
            0,
            0});
            this.difficultyUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.difficultyUpDown.Name = "difficultyUpDown";
            this.difficultyUpDown.ReadOnly = true;
            this.difficultyUpDown.Size = new System.Drawing.Size(50, 31);
            this.difficultyUpDown.TabIndex = 2;
            this.difficultyUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.difficultyUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // AIDifficulty
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 171);
            this.Controls.Add(this.difficultyUpDown);
            this.Controls.Add(this.confirmButton);
            this.Controls.Add(this.label1);
            this.Name = "AIDifficulty";
            this.Text = "AIDifficulty";
            ((System.ComponentModel.ISupportInitialize)(this.difficultyUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button confirmButton;
        private System.Windows.Forms.NumericUpDown difficultyUpDown;
    }
}