using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AISiteTunnel
{
    public partial class Form1 : Form
    {
        // ================= НАСТРОЙКИ =================
        private const string DnsServer = "dns.comss.one";
        private const string ResolveDomain = "chatgpt.com";
        private const string StartMarker = "#--AI Site Tunnel START--";
        private const string EndMarker = "#--AI Site Tunnel END--";
        private const string GithubUrl = "https://github.com/ComradeBingo";
        private const string GithubRepoLatestRelease =
            "https://api.github.com/repos/ComradeBingo/AIST-AI_Site_Tunnel/releases/latest";
        private const string GithubReleasesPage =
            "https://github.com/ComradeBingo/AIST-AI_Site_Tunnel/releases/latest";

        // Версия приложения — берётся из AssemblyInfo.cs (AssemblyVersion)
        private static readonly string APP_VERSION =
            Assembly.GetExecutingAssembly().GetName().Version.ToString();

        private static readonly string[] Domains =
        {
            "elevenlabs.io",
            "chatgpt.com",
            "ab.chatgpt.com",
            "auth.openai.com",
            "auth0.openai.com",
            "platform.openai.com",
            "cdn.oaistatic.com",
            "files.oaiusercontent.com",
            "cdn.auth0.com",
            "tcr9i.chat.openai.com",
            "webrtc.chatgpt.com",
            "gemini.google.com",
            "aistudio.google.com",
            "generativelanguage.googleapis.com",
            "alkalimakersuite-pa.clients6.google.com",
            "copilot.microsoft.com",
            "sydney.bing.com",
            "edgeservices.bing.com",
            "claude.ai",
            "aitestkitchen.withgoogle.com",
            "aisandbox-pa.googleapis.com",
            "x.ai",
            "grok.com",
            "accounts.x.ai",
            "labs.google",
            "anthropic.com",
            "api.anthropic.com",
            "api.openai.com",
            "canva.com"
        };

        // ================= DWM (тёмный заголовок) =================
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;

        // ================= ПОЛЯ =================
        private readonly string _hostsPath;
        private readonly string _backupPath;

        public Form1()
        {
            InitializeComponent();

            // Тёмный заголовок окна
            EnableDarkTitleBar(this.Handle);

            // Заголовок окна с версией из сборки
            this.Text = BuildWindowTitle();

            _hostsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.System),
                @"drivers\etc\hosts");

            _backupPath = _hostsPath + ".aist.bak";

            Log("AI Site Tunnel запущен.");
            Log("Версия: " + APP_VERSION);
            Log("hosts: " + _hostsPath);

            if (!IsAdmin())
            {
                Log("ВНИМАНИЕ: нет прав администратора. Запустите exe от имени администратора.");
                SetStatus("Нет прав администратора");
                btnRun.Enabled = false;
                btnRestore.Enabled = false;
            }

            // Проверка обновлений — асинхронно, не блокирует UI
            _ = CheckForUpdatesAsync();
        }

        // ================= ТЁМНЫЙ ЗАГОЛОВОК =================
        private static void EnableDarkTitleBar(IntPtr handle)
        {
            if (handle == IntPtr.Zero) return;

            int useDarkMode = 1;

            int result = DwmSetWindowAttribute(
                handle,
                DWMWA_USE_IMMERSIVE_DARK_MODE,
                ref useDarkMode,
                sizeof(int));

            if (result != 0)
            {
                DwmSetWindowAttribute(
                    handle,
                    DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1,
                    ref useDarkMode,
                    sizeof(int));
            }
        }

        // ================= ЗАГОЛОВОК ОКНА =================
        private static string BuildWindowTitle()
        {
            var v = Assembly.GetExecutingAssembly().GetName().Version;

            string version;
            if (v.Revision > 0)
                version = $"{v.Major}.{v.Minor}.{v.Build}.{v.Revision}";
            else if (v.Build > 0)
                version = $"{v.Major}.{v.Minor}.{v.Build}";
            else
                version = $"{v.Major}.{v.Minor}";

            return $"AI Site Tunnel v{version}  (c) Comrade Bingo";
        }

        // ================= ПРОВЕРКА ОБНОВЛЕНИЙ =================
        private async Task CheckForUpdatesAsync()
        {
            try
            {
                Log("Проверка обновлений...");

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "AIST-AI-Site-Tunnel-App");

                    var response = await client.GetAsync(GithubRepoLatestRelease);

                    if (!response.IsSuccessStatusCode)
                    {
                        Log("Не удалось проверить обновления: HTTP " + (int)response.StatusCode);
                        return;
                    }

                    var json = await response.Content.ReadAsStringAsync();

                    // Быстрый парсинг tag_name без подключения JSON-парсера
                    var match = Regex.Match(json, "\"tag_name\":\\s*\"([^\"]+)\"");

                    if (!match.Success)
                    {
                        Log("Тег версии не найден в ответе GitHub.");
                        return;
                    }

                    var latest = match.Groups[1].Value.TrimStart('v', 'V');

                    if (new Version(latest) > new Version(APP_VERSION))
                    {
                        Log($"Доступна новая версия: {latest} (у вас {APP_VERSION})");
                        SetStatus("Доступно обновление");

                        var result = MessageBox.Show(
                            $"Доступна новая версия {latest}!\n" +
                            $"Текущая: {APP_VERSION}\n\n" +
                            $"Перейти к загрузке?",
                            "Обновление",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Information);

                        if (result == DialogResult.Yes)
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = GithubReleasesPage,
                                UseShellExecute = true
                            });
                        }
                    }
                    else
                    {
                        Log($"Обновлений нет. Установлена последняя версия ({APP_VERSION}).");
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Ошибка проверки обновлений: " + ex.Message);
                Debug.WriteLine($"Ошибка проверки обновлений: {ex.Message}");
            }
        }

        // ================= КНОПКИ =================
        private async void btnRun_Click(object sender, EventArgs e)
        {
            SetBusy(true);
            try
            {
                await Task.Run(() => RunUpdate());
            }
            catch (Exception ex)
            {
                Log("ОШИБКА: " + ex.Message);
                SetStatus("Ошибка");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async void btnRestore_Click(object sender, EventArgs e)
        {
            SetBusy(true);
            try
            {
                await Task.Run(() => RestoreBackup());
            }
            catch (Exception ex)
            {
                Log("ОШИБКА: " + ex.Message);
                SetStatus("Ошибка");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void btnGithub_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = GithubUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Log("Не удалось открыть браузер: " + ex.Message);
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        // ================= ОСНОВНОЙ СЦЕНАРИЙ =================
        private void RunUpdate()
        {
            Log("=== Запуск обновления ===");

            SetStatus("Резолвинг...");
            string ip = ResolveViaNslookup(ResolveDomain, DnsServer);

            if (string.IsNullOrEmpty(ip))
            {
                Log("Не удалось получить IP. Проверьте доступность DNS-сервера.");
                SetStatus("Не удалось получить IP");
                return;
            }

            Log("Получен IP: " + ip);

            SetStatus("Бэкап hosts...");
            BackupHosts();

            SetStatus("Обновление hosts...");
            UpdateHosts(ip);

            SetStatus("Сброс DNS-кэша...");
            FlushDns();

            SetStatus("Готово. IP: " + ip);
            Log("=== Готово ===");
        }

        // ================= NSLOOKUP =================
        private string ResolveViaNslookup(string host, string dnsServer)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "nslookup",
                Arguments = $"{host} {dnsServer}",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.GetEncoding(866),
                StandardErrorEncoding = Encoding.GetEncoding(866)
            };

            using (var p = Process.Start(psi))
            {
                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();

                return ParseLastIp(output);
            }
        }

        private string ParseLastIp(string output)
        {
            var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string lastIp = null;

            foreach (var line in lines)
            {
                var tokens = line.Split(new[] { ' ', '\t', ':', ',' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var token in tokens)
                {
                    if (IsValidIPv4(token))
                        lastIp = token;
                }
            }

            if (lastIp != null)
                Log("Выбран последний IP: " + lastIp);

            return lastIp;
        }

        private static bool IsValidIPv4(string s)
        {
            if (IPAddress.TryParse(s, out IPAddress addr))
                return addr.AddressFamily == AddressFamily.InterNetwork;
            return false;
        }

        // ================= БЭКАП =================
        private void BackupHosts()
        {
            if (!File.Exists(_hostsPath))
                throw new FileNotFoundException("Файл hosts не найден", _hostsPath);

            File.Copy(_hostsPath, _backupPath, overwrite: true);
            Log("Бэкап сохранён: " + _backupPath);
        }

        private void RestoreBackup()
        {
            if (!File.Exists(_backupPath))
            {
                Log("Бэкап не найден: " + _backupPath);
                SetStatus("Бэкап не найден");
                return;
            }

            File.Copy(_backupPath, _hostsPath, overwrite: true);
            Log("hosts восстановлен из бэкапа.");
            FlushDns();
            SetStatus("hosts восстановлен");
        }

        // ================= HOSTS =================
        private void UpdateHosts(string ip)
        {
            var attrs = File.GetAttributes(_hostsPath);
            if ((attrs & FileAttributes.ReadOnly) != 0)
            {
                File.SetAttributes(_hostsPath, attrs & ~FileAttributes.ReadOnly);
                Log("Снят атрибут read-only.");
            }

            var originalLines = File.ReadAllLines(_hostsPath);
            var result = new List<string>();

            bool insideBlock = false;
            foreach (var line in originalLines)
            {
                string trimmed = line.Trim();

                if (trimmed.Equals(StartMarker, StringComparison.OrdinalIgnoreCase))
                {
                    insideBlock = true;
                    continue;
                }

                if (trimmed.Equals(EndMarker, StringComparison.OrdinalIgnoreCase))
                {
                    insideBlock = false;
                    continue;
                }

                if (insideBlock)
                    continue;

                if (!trimmed.StartsWith("#"))
                {
                    var tokens = trimmed.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    bool isOurDomain = false;
                    foreach (var tok in tokens)
                    {
                        if (Array.IndexOf(Domains, tok.ToLowerInvariant()) >= 0)
                        {
                            isOurDomain = true;
                            break;
                        }
                    }
                    if (isOurDomain)
                        continue;
                }

                result.Add(line);
            }

            while (result.Count > 0 && string.IsNullOrWhiteSpace(result[result.Count - 1]))
                result.RemoveAt(result.Count - 1);

            result.Add(string.Empty);
            result.Add(StartMarker);
            foreach (var domain in Domains)
                result.Add(ip + "\t" + domain);
            result.Add(EndMarker);
            result.Add(string.Empty);

            var utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            File.WriteAllLines(_hostsPath, result, utf8NoBom);

            Log($"Записано {Domains.Length} доменов на IP {ip}.");
        }

        // ================= FLUSH DNS =================
        private void FlushDns()
        {
            var psi = new ProcessStartInfo
            {
                FileName = "ipconfig",
                Arguments = "/flushdns",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.GetEncoding(866),
                StandardErrorEncoding = Encoding.GetEncoding(866)
            };

            using (var p = Process.Start(psi))
            {
                string outp = p.StandardOutput.ReadToEnd();
                string err = p.StandardError.ReadToEnd();
                p.WaitForExit();

                if (!string.IsNullOrWhiteSpace(outp))
                    Log(outp.Trim());

                if (!string.IsNullOrWhiteSpace(err))
                    Log("stderr: " + err.Trim());
            }
        }

        // ================= ПРАВА =================
        private static bool IsAdmin()
        {
            using (var identity = WindowsIdentity.GetCurrent())
            {
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        // ================= UI-ХЕЛПЕРЫ =================
        private void Log(string message)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.BeginInvoke(new Action<string>(Log), message);
                return;
            }

            string line = DateTime.Now.ToString("HH:mm:ss") + "  " + message;
            txtLog.AppendText(line + Environment.NewLine);
        }

        private void SetStatus(string status)
        {
            if (lblStatus.InvokeRequired)
            {
                lblStatus.BeginInvoke(new Action<string>(SetStatus), status);
                return;
            }

            lblStatus.Text = status;

            Color dotColor;
            string s = status.ToLowerInvariant();

            if (s.Contains("готово") || s.Contains("успешно") || s.Contains("обновление"))
                dotColor = Color.FromArgb(78, 201, 176);
            else if (s.Contains("ошибк") || s.Contains("не удалось") || s.Contains("нет прав"))
                dotColor = Color.FromArgb(244, 135, 113);
            else if (s.Contains("резолвинг") || s.Contains("бэкап") ||
                     s.Contains("обновление hosts") || s.Contains("сброс"))
                dotColor = Color.FromArgb(220, 220, 170);
            else
                dotColor = Color.Gray;

            if (lblStatusDot.InvokeRequired)
                lblStatusDot.BeginInvoke(new Action(() => lblStatusDot.ForeColor = dotColor));
            else
                lblStatusDot.ForeColor = dotColor;
        }

        private void SetBusy(bool busy)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<bool>(SetBusy), busy);
                return;
            }

            btnRun.Enabled = !busy && IsAdmin();
            btnRestore.Enabled = !busy && IsAdmin();
            btnGithub.Enabled = !busy;
            btnExit.Enabled = !busy;
            Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
        }
    }
}