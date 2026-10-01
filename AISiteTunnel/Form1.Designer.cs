using System;

namespace AISiteTunnel
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblStatusDot;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Button btnRestore;
        private System.Windows.Forms.Button btnGithub;
        private System.Windows.Forms.Button btnExit;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblStatusDot = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.btnRun = new System.Windows.Forms.Button();
            this.btnRestore = new System.Windows.Forms.Button();
            this.btnGithub = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.SuspendLayout();

            // ============ ЦВЕТА (VS Code Dark) ============
            var bgForm = System.Drawing.Color.FromArgb(30, 30, 30);
            var bgLog = System.Drawing.Color.FromArgb(37, 37, 38);
            var fgText = System.Drawing.Color.FromArgb(212, 212, 212);
            var fgStatus = System.Drawing.Color.FromArgb(204, 204, 204);
            var btnBg = System.Drawing.Color.FromArgb(60, 60, 60);
            var btnHover = System.Drawing.Color.FromArgb(80, 80, 80);
            var btnAccent = System.Drawing.Color.FromArgb(14, 99, 156);
            var btnAccentHover = System.Drawing.Color.FromArgb(17, 119, 187);

            // ============ lblStatusDot (индикатор-кружок) ============
            this.lblStatusDot.AutoSize = true;
            this.lblStatusDot.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblStatusDot.ForeColor = System.Drawing.Color.Gray;
            this.lblStatusDot.Location = new System.Drawing.Point(12, 9);
            this.lblStatusDot.Name = "lblStatusDot";
            this.lblStatusDot.Size = new System.Drawing.Size(20, 21);
            this.lblStatusDot.TabIndex = 0;
            this.lblStatusDot.Text = "●";

            // ============ lblStatus ============
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblStatus.ForeColor = fgStatus;
            this.lblStatus.Location = new System.Drawing.Point(32, 14);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(100, 17);
            this.lblStatus.TabIndex = 1;
            this.lblStatus.Text = "Готов к работе";

            // ============ txtLog ============
            this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top
                | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLog.BackColor = bgLog;
            this.txtLog.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtLog.Font = new System.Drawing.Font("Consolas", 9.5F);
            this.txtLog.ForeColor = fgText;
            this.txtLog.Location = new System.Drawing.Point(12, 40);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(560, 310);
            this.txtLog.TabIndex = 2;

            // ============ btnRun (акцентная) ============
            this.btnRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom
                | System.Windows.Forms.AnchorStyles.Left)));
            this.btnRun.BackColor = btnAccent;
            this.btnRun.FlatAppearance.BorderSize = 0;
            this.btnRun.FlatAppearance.MouseOverBackColor = btnAccentHover;
            this.btnRun.FlatAppearance.MouseDownBackColor = btnAccentHover;
            this.btnRun.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRun.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnRun.ForeColor = System.Drawing.Color.White;
            this.btnRun.Location = new System.Drawing.Point(12, 360);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(170, 32);
            this.btnRun.TabIndex = 3;
            this.btnRun.Text = "Обновить hosts";
            this.btnRun.UseVisualStyleBackColor = false;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);

            // ============ btnRestore ============
            this.btnRestore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom
                | System.Windows.Forms.AnchorStyles.Left)));
            this.btnRestore.BackColor = btnBg;
            this.btnRestore.FlatAppearance.BorderSize = 0;
            this.btnRestore.FlatAppearance.MouseOverBackColor = btnHover;
            this.btnRestore.FlatAppearance.MouseDownBackColor = btnHover;
            this.btnRestore.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRestore.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnRestore.ForeColor = fgText;
            this.btnRestore.Location = new System.Drawing.Point(192, 360);
            this.btnRestore.Name = "btnRestore";
            this.btnRestore.Size = new System.Drawing.Size(170, 32);
            this.btnRestore.TabIndex = 4;
            this.btnRestore.Text = "Восстановить hosts";
            this.btnRestore.UseVisualStyleBackColor = false;
            this.btnRestore.Click += new System.EventHandler(this.btnRestore_Click);

            // ============ btnGithub ============
            this.btnGithub.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom
                | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGithub.BackColor = btnBg;
            this.btnGithub.FlatAppearance.BorderSize = 0;
            this.btnGithub.FlatAppearance.MouseOverBackColor = btnHover;
            this.btnGithub.FlatAppearance.MouseDownBackColor = btnHover;
            this.btnGithub.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGithub.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnGithub.ForeColor = fgText;
            this.btnGithub.Location = new System.Drawing.Point(372, 360);
            this.btnGithub.Name = "btnGithub";
            this.btnGithub.Size = new System.Drawing.Size(90, 32);
            this.btnGithub.TabIndex = 5;
            this.btnGithub.Text = "GitHub";
            this.btnGithub.UseVisualStyleBackColor = false;
            this.btnGithub.Click += new System.EventHandler(this.btnGithub_Click);

            // ============ btnExit ============
            this.btnExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom
                | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExit.BackColor = btnBg;
            this.btnExit.FlatAppearance.BorderSize = 0;
            this.btnExit.FlatAppearance.MouseOverBackColor = btnHover;
            this.btnExit.FlatAppearance.MouseDownBackColor = btnHover;
            this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExit.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnExit.ForeColor = fgText;
            this.btnExit.Location = new System.Drawing.Point(472, 360);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(100, 32);
            this.btnExit.TabIndex = 6;
            this.btnExit.Text = "Выход";
            this.btnExit.UseVisualStyleBackColor = false;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);

            // ============ Form1 ============
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = bgForm;
            this.ClientSize = new System.Drawing.Size(584, 401);
            this.Controls.Add(this.lblStatusDot);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.btnRestore);
            this.Controls.Add(this.btnGithub);
            this.Controls.Add(this.btnExit);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ForeColor = fgText;
            this.Icon = new System.Drawing.Icon(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "aist.ico"));
            this.MinimumSize = new System.Drawing.Size(520, 380);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AI Site Tunnel";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}