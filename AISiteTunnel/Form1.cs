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
        private const string ResolveDomain = "chatgpt.com";
        private const string StartMarker = "#--AI Site Tunnel START--";
        private const string EndMarker = "#--AI Site Tunnel END--";
        private const string GithubUrl = "https://github.com/ComradeBingo";
        private const string GithubRepoLatestRelease =
            "https://api.github.com/repos/ComradeBingo/AIST-AI_Site_Tunnel/releases/latest";
        private const string GithubReleasesPage =
            "https://github.com/ComradeBingo/AIST-AI_Site_Tunnel/releases/latest";

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

        // ================= DoH =================
        // Порядок = приоритет. При неудаче первого запроса идём к следующему.
        private static readonly string[] DohEndpoints =
        {
            "https://dns.zapret.moe/dns-query",
            "https://dns.magicdns.ru/dns-query"
            
        };

        private static readonly HttpClient Http = CreateHttpClient();

        private static HttpClient CreateHttpClient()
        {
            var c = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            c.DefaultRequestHeaders.Add("User-Agent", "AIST-AI-Site-Tunnel-App");
            return c;
        }

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

            EnableDarkTitleBar(this.Handle);
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
                Log("ВНИМАНИЕ: нет прав администратора. " +
                    "Автоматическая запись hosts может не сработать — " +
                    "будет предложено ручное обновление.");
                SetStatus("Нет прав администратора");
            }

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
                await Task.Run(() => RunUpdateAsync());
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
            // Кнопка «Восстановить hosts» — если она осталась в дизайнере.
            // Пытаемся откатить изменения из бэкапа, но уже без требования прав.
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
        private async Task RunUpdateAsync()
        {
            Log("=== Запуск обновления ===");

            SetStatus("Разрешение через DoH (wireformat)...");
            string ip = await ResolveViaDohWireAsync(ResolveDomain).ConfigureAwait(false);

            if (string.IsNullOrEmpty(ip))
            {
                Log("Не удалось получить IP через DoH. Проверьте сеть или добавьте другой эндпоинт.");
                SetStatus("Не удалось получить IP");
                return;
            }

            Log("Получен IP: " + ip);

            SetStatus("Бэкап hosts...");
            try
            {
                BackupHosts();
            }
            catch (Exception ex)
            {
                Log("Бэкап не удался: " + ex.Message);
            }

            SetStatus("Обновление hosts...");
            if (TryUpdateHosts(ip, out string error))
            {
                SetStatus("Сброс DNS-кэша...");
                FlushDns();
                SetStatus("Готово. IP: " + ip);
                Log("=== Готово ===");
            }
            else
            {
                Log("Автоматическая запись не удалась: " + error);
                SetStatus("Требуется ручное обновление");
                OfferManualEdit(ip, error);
            }
        }

        // ================= DoH wireformat =================
        private async Task<string> ResolveViaDohWireAsync(string domain)
        {
            byte[] queryBytes = BuildDnsQuery(domain);

            foreach (var endpoint in DohEndpoints)
            {
                try
                {
                    Log("DoH wire POST: " + endpoint);

                    using (var content = new ByteArrayContent(queryBytes))
                    {
                        content.Headers.ContentType =
                            new System.Net.Http.Headers.MediaTypeHeaderValue("application/dns-message");

                        using (var request = new HttpRequestMessage(HttpMethod.Post, endpoint))
                        {
                            request.Content = content;
                            request.Headers.Accept.ParseAdd("application/dns-message");

                            using (var resp = await Http.SendAsync(request).ConfigureAwait(false))
                            {
                                if (!resp.IsSuccessStatusCode)
                                {
                                    Log($"  HTTP {(int)resp.StatusCode} от {endpoint}");
                                    continue;
                                }

                                byte[] respBytes = await resp.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                                string ip = ParseDnsWireResponse(respBytes);

                                if (!string.IsNullOrEmpty(ip))
                                {
                                    Log($"  Успех: {domain} -> {ip} ({endpoint})");
                                    return ip;
                                }

                                Log("  Ответ не содержит A-записи.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log($"  Ошибка {endpoint}: {ex.Message}");
                }
            }

            return null;
        }

        private static byte[] BuildDnsQuery(string domain, ushort queryType = 1)
        {
            var labels = domain.Split('.');
            int qnameLen = 0;
            foreach (var label in labels) qnameLen += 1 + label.Length;
            qnameLen += 1; // завершающий 0

            var query = new byte[12 + qnameLen + 4];
            int idx = 0;

            // Header (12 bytes)
            query[idx++] = 0x00; query[idx++] = 0x00; // ID = 0
            query[idx++] = 0x01; query[idx++] = 0x00; // Flags: RD=1
            query[idx++] = 0x00; query[idx++] = 0x01; // QDCOUNT = 1
            query[idx++] = 0x00; query[idx++] = 0x00; // ANCOUNT = 0
            query[idx++] = 0x00; query[idx++] = 0x00; // NSCOUNT = 0
            query[idx++] = 0x00; query[idx++] = 0x00; // ARCOUNT = 0

            // QNAME
            foreach (var label in labels)
            {
                query[idx++] = (byte)label.Length;
                foreach (char c in label) query[idx++] = (byte)c;
            }
            query[idx++] = 0x00;

            // QTYPE
            query[idx++] = (byte)(queryType >> 8);
            query[idx++] = (byte)(queryType & 0xFF);

            // QCLASS = IN (1)
            query[idx++] = 0x00;
            query[idx++] = 0x01;

            return query;
        }

        private static string ParseDnsWireResponse(byte[] resp)
        {
            try
            {
                if (resp == null || resp.Length < 12) return null;

                int qdCount = (resp[4] << 8) | resp[5];
                int anCount = (resp[6] << 8) | resp[7];

                if (anCount == 0) return null;

                int offset = 12;

                // Пропускаем QNAME (поддерживаем указатели сжатия)
                for (int i = 0; i < qdCount; i++)
                {
                    offset = SkipDnsName(resp, offset);
                    offset += 4; // QTYPE + QCLASS
                }

                // Разбираем Answer
                for (int i = 0; i < anCount; i++)
                {
                    offset = SkipDnsName(resp, offset);

                    if (offset + 10 > resp.Length) break;

                    ushort type = (ushort)((resp[offset] << 8) | resp[offset + 1]);
                    ushort klass = (ushort)((resp[offset + 2] << 8) | resp[offset + 3]);
                    ushort rdLength = (ushort)((resp[offset + 8] << 8) | resp[offset + 9]);
                    offset += 10;

                    if (type == 1 && klass == 1 && rdLength == 4 && offset + 4 <= resp.Length)
                    {
                        return $"{resp[offset]}.{resp[offset + 1]}.{resp[offset + 2]}.{resp[offset + 3]}";
                    }

                    offset += rdLength;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ParseDnsWireResponse: " + ex.Message);
            }

            return null;
        }

        private static int SkipDnsName(byte[] resp, int offset)
        {
            while (offset < resp.Length)
            {
                byte len = resp[offset];
                if (len == 0) return offset + 1;
                if ((len & 0xC0) == 0xC0) return offset + 2; // указатель сжатия
                offset += 1 + len;
            }
            return offset;
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

            try
            {
                File.Copy(_backupPath, _hostsPath, overwrite: true);
                Log("hosts восстановлен из бэкапа.");
                FlushDns();
                SetStatus("hosts восстановлен");
            }
            catch (Exception ex)
            {
                Log("Не удалось восстановить hosts: " + ex.Message);
                SetStatus("Не удалось восстановить hosts");
            }
        }

        // ================= HOSTS =================
        private bool TryUpdateHosts(string ip, out string error)
        {
            error = null;
            try
            {
                WriteHosts(ip);
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                error = "Нет прав на запись в hosts: " + ex.Message;
                return false;
            }
            catch (IOException ex)
            {
                error = "Файл hosts заблокирован (возможно, антивирусом): " + ex.Message;
                return false;
            }
            catch (Exception ex)
            {
                error = "Не удалось записать hosts: " + ex.Message;
                return false;
            }
        }

        private void WriteHosts(string ip)
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

        private string BuildHostsBlock(string ip)
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine(StartMarker);
            foreach (var domain in Domains)
                sb.AppendLine(ip + "\t" + domain);
            sb.AppendLine(EndMarker);
            sb.AppendLine();
            return sb.ToString();
        }

        // ================= РУЧНОЕ ОБНОВЛЕНИЕ =================
        private void OfferManualEdit(string ip, string error)
        {
            // Из фонового потока Task.Run переходим в UI-поток
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => OfferManualEdit(ip, error)));
                return;
            }

            string block = BuildHostsBlock(ip);

            using (var form = new ManualHostsForm(_hostsPath, block))
            {
                form.ShowDialog(this);
            }

            Log("Окно ручного обновления закрыто.");
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
            else if (s.Contains("ошибк") || s.Contains("не удалось") || s.Contains("нет прав") ||
                     s.Contains("требуется ручное"))
                dotColor = Color.FromArgb(244, 135, 113);
            else if (s.Contains("резолвинг") || s.Contains("бэкап") ||
                     s.Contains("обновление hosts") || s.Contains("сброс") || s.Contains("разрешение"))
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

            // Кнопку Run больше не блокируем отсутствием прав — программа сама
            // предложит ручное обновление, если автозапись не удастся.
            btnRun.Enabled = !busy;
            btnRestore.Enabled = !busy;
            btnGithub.Enabled = !busy;
            btnExit.Enabled = !busy;
            Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
        }

        private async void btnManual_Click(object sender, EventArgs e)
        {
            SetBusy(true);
            try
            {
                SetStatus("Разрешение через DoH...");
                string ip = await Task.Run(() => ResolveViaDohWireAsync(ResolveDomain));

                if (string.IsNullOrEmpty(ip))
                {
                    Log("DoH не вернул IP — открываю форму с заготовкой для ручного ввода.");
                    ip = "0.0.0.0";
                }
                else
                {
                    Log("Для ручного обновления получен IP: " + ip);
                }

                string block = BuildHostsBlock(ip);

                using (var form = new ManualHostsForm(_hostsPath, block))
                {
                    form.ShowDialog(this);
                }

                Log("Окно ручного обновления закрыто.");
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

    }
}