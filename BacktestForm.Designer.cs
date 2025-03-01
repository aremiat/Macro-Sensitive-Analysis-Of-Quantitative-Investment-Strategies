using System;
namespace BacktestUI
{
    partial class BacktestForm
    {
        private System.ComponentModel.IContainer components = null;

        // Déclaration des contrôles
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelTop;
        private System.Windows.Forms.CheckedListBox clbStrategies;
        private System.Windows.Forms.DateTimePicker dtpStart;
        private System.Windows.Forms.DateTimePicker dtpEnd;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.TextBox txtResults;
        private System.Windows.Forms.DataGridView dgvResults;

        /// <summary>
        /// Nettoyage des ressources.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanelTop = new System.Windows.Forms.FlowLayoutPanel();
            this.clbStrategies = new System.Windows.Forms.CheckedListBox();
            this.dtpStart = new System.Windows.Forms.DateTimePicker();
            this.dtpEnd = new System.Windows.Forms.DateTimePicker();
            this.btnRun = new System.Windows.Forms.Button();
            this.txtResults = new System.Windows.Forms.TextBox();
            this.dgvResults = new System.Windows.Forms.DataGridView();

            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvResults)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanelTop, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.txtResults, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.dgvResults, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.RowCount = 3;
            // Ligne 0 : AutoSize (pour la hauteur des contrôles du haut)
            this.tableLayoutPanel1.RowStyles.Add(
                new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            // Ligne 1 : 30 % de la hauteur pour le TextBox
            this.tableLayoutPanel1.RowStyles.Add(
                new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            // Ligne 2 : 70 % de la hauteur pour le DataGridView
            this.tableLayoutPanel1.RowStyles.Add(
                new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(800, 450);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // flowLayoutPanelTop
            // 
            this.flowLayoutPanelTop.AutoSize = true;
            this.flowLayoutPanelTop.Controls.Add(this.clbStrategies);
            this.flowLayoutPanelTop.Controls.Add(this.dtpStart);
            this.flowLayoutPanelTop.Controls.Add(this.dtpEnd);
            this.flowLayoutPanelTop.Controls.Add(this.btnRun);
            this.flowLayoutPanelTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelTop.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanelTop.Name = "flowLayoutPanelTop";
            this.flowLayoutPanelTop.Size = new System.Drawing.Size(794, 100);
            this.flowLayoutPanelTop.TabIndex = 0;
            // 
            // clbStrategies
            // 
            this.clbStrategies.CheckOnClick = true;
            this.clbStrategies.FormattingEnabled = true;
            this.clbStrategies.Location = new System.Drawing.Point(3, 3);
            this.clbStrategies.Name = "clbStrategies";
            this.clbStrategies.Size = new System.Drawing.Size(200, 72);
            this.clbStrategies.TabIndex = 0;
            // dtpStart
            this.dtpStart.Location = new System.Drawing.Point(209, 3);
            this.dtpStart.Name = "dtpStart";
            this.dtpStart.Size = new System.Drawing.Size(200, 22);
            // On limite entre l'an 2000 et aujourd'hui
            this.dtpStart.MinDate = new DateTime(2000, 1, 1);
            this.dtpStart.MaxDate = DateTime.Today;

            // dtpEnd
            this.dtpEnd.Location = new System.Drawing.Point(415, 3);
            this.dtpEnd.Name = "dtpEnd";
            this.dtpEnd.Size = new System.Drawing.Size(200, 22);
            // On limite entre l'an 2000 et aujourd'hui
            this.dtpEnd.MinDate = new DateTime(2000, 1, 1);
            this.dtpEnd.MaxDate = DateTime.Today;
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(621, 3);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(100, 30);
            this.btnRun.TabIndex = 3;
            this.btnRun.Text = "Lancer";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // txtResults
            // 
            this.txtResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtResults.Location = new System.Drawing.Point(3, 109);
            this.txtResults.Multiline = true;
            this.txtResults.Name = "txtResults";
            this.txtResults.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtResults.Size = new System.Drawing.Size(794, 129);
            this.txtResults.TabIndex = 1;
            // 
            // dgvResults
            // 
            this.dgvResults.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvResults.Location = new System.Drawing.Point(3, 244);
            this.dgvResults.Name = "dgvResults";
            this.dgvResults.RowHeadersWidth = 51;
            this.dgvResults.RowTemplate.Height = 24;
            this.dgvResults.Size = new System.Drawing.Size(794, 203);
            this.dgvResults.TabIndex = 2;
            // 
            // BacktestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "BacktestForm";
            this.Text = "Interface Backtest";
            this.Load += new System.EventHandler(this.BacktestForm_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanelTop.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvResults)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
    }
}
