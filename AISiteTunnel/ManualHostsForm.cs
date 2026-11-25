using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace AISiteTunnel
{
    public class ManualHostsForm : Form
    {
        private readonly string _hostsPath;
        private readonly string _blockText;

        public ManualHostsForm(string hostsPath, string blockText)
        {
            _hostsPath = hostsPath;
            _blockText = blockText;

            InitializeUi();
        }

        private void InitializeUi()
        {
            this.Text = "Ручное обновление hosts";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(680, 560);
            this.MinimumSize = new Size(520, 420);

            var lblInfo = new Label
            {
                Dock = DockStyle.Top,
                AutoSize = false,
                Height = 110,
                Padding = new Padding(12, 12, 12, 8),
                Text =
                    "Если автоматически обновить файл hosts не удалось " +
                    "(нет прав или файл заблокирован антивирусом).\r\n\r\n" +
                    "1. Нажмите «Скопировать» — текст ниже попадёт в буфер обмена.\r\n" +
                    "2. Нажмите «Открыть hosts в Блокноте» — откроется файл с правами администратора.\r\n" +
                    "3. Перейдите в самый конец файла, вставьте (Ctrl+V) и сохраните (Ctrl+S).\r\n" +
                    "4. Закройте Блокнот и это окно."
            };

            var txtBlock = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                Font = new Font("Consolas", 9.5f),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.Gainsboro,
                Text = _blockText
            };

            var panelButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 56,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(12, 8, 12, 8)
            };

            var btnClose = new Button { Text = "Закрыть", Width = 120, Height = 32 };
            btnClose.Click += (s, e) => this.Close();

            var btnOpen = new Button { Text = "Открыть hosts в Блокноте", Width = 220, Height = 32 };
            btnOpen.Click += BtnOpen_Click;

            var btnCopy = new Button { Text = "Скопировать", Width = 140, Height = 32 };
            btnCopy.Click += BtnCopy_Click;

            panelButtons.Controls.Add(btnClose);
            panelButtons.Controls.Add(btnOpen);
            panelButtons.Controls.Add(btnCopy);

            var panelTextBox = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
            panelTextBox.Controls.Add(txtBlock);

            this.Controls.Add(panelTextBox);
            this.Controls.Add(panelButtons);
            this.Controls.Add(lblInfo);
        }

        private void BtnCopy_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(_blockText);
                MessageBox.Show(this, "Текст скопирован в буфер обмена.",
                    "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Не удалось скопировать в буфер:\n" + ex.Message,
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnOpen_Click(object sender, EventArgs e)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "notepad.exe",
                    Arguments = "\"" + _hostsPath + "\"",
                    UseShellExecute = true,
                    Verb = "runas" // UAC
                };
                Process.Start(psi);
            }
            catch (System.ComponentModel.Win32Exception)
            {
                MessageBox.Show(this,
                    "Запуск от имени администратора отменён.\r\n" +
                    "Без повышения прав Блокнот не сможет сохранить изменения в hosts.",
                    "Отменено", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Не удалось запустить Блокнот:\n" + ex.Message,
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}