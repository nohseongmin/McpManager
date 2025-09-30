using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Text.Json;


namespace McpManager
{
        public partial class MainForm : Form
        {
            // 서버별 API 키 저장용 (메모리 캐시)
            private System.Collections.Generic.Dictionary<string, string> _apiKeys = new System.Collections.Generic.Dictionary<string, string>();
        private readonly System.Collections.Generic.List<McpServerInfo> _mcpServers;

        public MainForm()
        {
            try
            {
                _mcpServers = new System.Collections.Generic.List<McpServerInfo>
                {
                    new McpServerInfo { Name = "Github MCP", PackageId = "@smithery-ai/github" },
                    new McpServerInfo { Name = "Google Drive MCP", PackageId = "@smithery-ai/google-drive" }
                };

                InitializeComponent();
                // 언어 콤보박스 기본값을 한국어로 고정
                if (comboBoxLang.Items.Count > 0)
                    comboBoxLang.SelectedIndex = 0;
                // DataGridView 컬럼 및 MCP 서버 목록 명확히 추가
                if (dgvMcpList.Columns.Count == 0)
                {
                    dgvMcpList.Columns.Add("Name", "MCP 서버");
                    dgvMcpList.Columns.Add("Status", "상태");
                    var btnCol = new DataGridViewButtonColumn();
                    btnCol.Name = "Action";
                    btnCol.HeaderText = "작업";
                    btnCol.Text = "";
                    btnCol.UseColumnTextForButtonValue = false;
                    dgvMcpList.Columns.Add(btnCol);
                }
                dgvMcpList.Rows.Clear();
                foreach (var server in _mcpServers)
                {
                    dgvMcpList.Rows.Add(server.Name, "미설치", "설치");
                }
                UpdateUI("ko");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"MainForm 생성 중 오류 발생: {ex.Message}\n{ex.StackTrace}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        private void DgvMcpList_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 2) return;
            var row = dgvMcpList.Rows[e.RowIndex];
            string? mcpName = row.Cells[0].Value?.ToString();
            string? status = row.Cells[1].Value?.ToString();

            if (string.IsNullOrEmpty(mcpName) || string.IsNullOrEmpty(status)) return;

            var mcpServer = _mcpServers.Find(s => s.Name == mcpName);
            if (mcpServer == null) return;

            if (status == "설치됨")
            {
                UninstallMcp(mcpServer, row);
            }
            else if (status == "미설치")
            {
                // 진행중 표시 및 비동기 설치
                _ = InstallMcpAsync(mcpServer, row);
            }
        }


        private async Task InstallMcpAsync(McpServerInfo mcpServer, DataGridViewRow row)
        {
            string apiKey = PromptForApiKey(mcpServer.Name);
            if (string.IsNullOrEmpty(apiKey)) return;

            lblStatus.Text = $"{mcpServer.Name} 설치 중...";
            row.Cells[2].Value = "설치중...";

            string command = $"npx -y @smithery/cli@latest install {mcpServer.PackageId} --client gemini-cli --key {apiKey}";

            var result = await RunProcessAsync("cmd.exe", $"/c {command}");

            if (result.ExitCode == 0)
            {
                lblStatus.Text = $"{mcpServer.Name} 설치 완료.";
                row.Cells[1].Value = "설치됨";
                row.Cells[2].Value = "삭제";
            }
            else
            {
                MessageBox.Show($"{mcpServer.Name} 설치 실패.\n오류: {result.Error}", "설치 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "준비";
                row.Cells[2].Value = "설치"; // Revert button
            }
        }

    private void UninstallMcp(McpServerInfo mcpServer, DataGridViewRow row)
        {
            var result = MessageBox.Show($"{mcpServer.Name}을(를) 정말 삭제하시겠습니까?", "MCP 삭제", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.No) return;

            lblStatus.Text = $"{mcpServer.Name} 삭제 중...";

            try
            {
                string settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".gemini", "settings.json");
                if (File.Exists(settingsPath))
                {
                    string json = File.ReadAllText(settingsPath);

                    var options = new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true };
                    var settings = JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, JsonElement>>(json, options);

                    if (settings != null && settings.ContainsKey(mcpServer.PackageId))
                    {
                        settings.Remove(mcpServer.PackageId);
                        var newJson = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                        File.WriteAllText(settingsPath, newJson);
                    }
                }
                lblStatus.Text = $"{mcpServer.Name} 삭제 완료.";
                row.Cells[1].Value = "미설치";
                row.Cells[2].Value = "설치";
            }
            catch (Exception ex)
            {
                MessageBox.Show("MCP 삭제 실패: " + ex.Message);
                lblStatus.Text = "준비";
            }
            finally
            {
            }
        }

        private string PromptForApiKey(string mcpName)
        {
            string prevKey = _apiKeys.ContainsKey(mcpName) ? _apiKeys[mcpName] : "";
            string apiKey = prevKey;
            while (true)
            {
                Form prompt = new Form()
                {
                    Width = 500,
                    Height = 180,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    Text = $"{mcpName} API 키 입력 또는 수정",
                    StartPosition = FormStartPosition.CenterScreen
                };
                Label textLabel = new Label() { Left = 50, Top = 20, Width = 400, Text = "MCP 서버 연동을 위해 API 키를 입력해 주세요." };
                TextBox textBox = new TextBox() { Left = 50, Top = 50, Width = 400, Text = prevKey };
                Button confirmation = new Button() { Text = "확인", Left = 350, Width = 100, Top = 90, DialogResult = DialogResult.OK };
                LinkLabel linkLabel = new LinkLabel() { Left = 50, Top = 95, Text = "API 키 발급 방법 안내" };
                linkLabel.Click += (sender, e) => {
                    Process.Start(new ProcessStartInfo("https://github.com/settings/tokens") { UseShellExecute = true });
                };
                confirmation.Click += (sender, e) => { prompt.Close(); };
                prompt.Controls.Add(textBox);
                prompt.Controls.Add(confirmation);
                prompt.Controls.Add(textLabel);
                prompt.Controls.Add(linkLabel);
                prompt.AcceptButton = confirmation;
                var result = prompt.ShowDialog();
                apiKey = textBox.Text.Trim();
                if (result != DialogResult.OK) return "";
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    MessageBox.Show("API 키를 반드시 입력해 주세요.", "입력 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    prevKey = "";
                    continue;
                }
                _apiKeys[mcpName] = apiKey;
                return apiKey;
            }
        }

        private void CheckMcpStatus()
        {
            string settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".gemini", "settings.json");
            if (!File.Exists(settingsPath)) return;

            try
            {
                string json = File.ReadAllText(settingsPath);
                var options = new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true };
                var settings = JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, JsonElement>>(json, options);

                if (settings == null) return;

                foreach (DataGridViewRow row in dgvMcpList.Rows)
                {
                    string? mcpName = row.Cells[0].Value?.ToString();
                    if (string.IsNullOrEmpty(mcpName)) continue;

                    var mcpServer = _mcpServers.Find(s => s.Name == mcpName);
                    if (mcpServer == null) continue;

                    if (settings.ContainsKey(mcpServer.PackageId))
                    {
                        row.Cells[1].Value = "설치됨";
                        row.Cells[2].Value = "삭제";
                    }
                    else
                    {
                        row.Cells[1].Value = "미설치";
                        row.Cells[2].Value = "설치";
                    }
                }
            }
            catch (JsonException ex)
            {
                // Handle cases where settings.json is malformed
                MessageBox.Show($"settings.json 파일을 읽는 중 오류가 발생했습니다: {ex.Message}", "JSON 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"MCP 상태 확인 중 오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // DataGridView에서는 별도 액션 필요 없음 (CheckMcpStatus에서 처리)


        private void ComboBoxLang_SelectedIndexChanged(object sender, EventArgs e)
        {
            string? langSelection = comboBoxLang.SelectedItem?.ToString();
            if (langSelection == null) return;

            string lang = langSelection == "English" ? "en" : "ko";
            UpdateUI(lang);
        }

        private void UpdateUI(string lang)
        {
            if (lang == "ko")
            {
                this.Text = "Mcp 관리자";
                groupBoxGemini.Text = "Gemini CLI 설정";
                btnInstallNode.Text = "Node.js 설치";
                btnInstallGemini.Text = "Gemini CLI 설치";
                btnRunGemini.Text = "Gemini CLI 실행";
                groupBoxMcp.Text = "MCP 서버 관리";
                lblStatus.Text = "준비";
                dgvMcpList.Columns[0].HeaderText = "MCP 서버";
                dgvMcpList.Columns[1].HeaderText = "상태";
                dgvMcpList.Columns[2].HeaderText = "작업";
            }
            else
            {
                this.Text = "Mcp Manager";
                groupBoxGemini.Text = "Gemini CLI Setup";
                btnInstallNode.Text = "Install Node.js";
                btnInstallGemini.Text = "Install Gemini CLI";
                btnRunGemini.Text = "Run Gemini CLI";
                groupBoxMcp.Text = "MCP Server Management";
                lblStatus.Text = "Ready";
                dgvMcpList.Columns[0].HeaderText = "MCP Server";
                dgvMcpList.Columns[1].HeaderText = "Status";
                dgvMcpList.Columns[2].HeaderText = "Action";
            }
        }

        private const string NodeVersion = "v22.20.0";
        private const string NodeInstallerUrl = "https://nodejs.org/dist/v22.20.0/node-v22.20.0-x64.msi";

        private void BtnInstallNode_Click(object sender, EventArgs e)
        {
            try
            {
                string exeDir = AppDomain.CurrentDomain.BaseDirectory;
                string msiPath = Directory.GetFiles(exeDir, "*.msi").FirstOrDefault();
                if (msiPath == null)
                {
                    MessageBox.Show("현재 폴더에 .msi 파일이 없습니다.", "설치 파일 없음", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                var psi = new ProcessStartInfo("msiexec.exe", $"/i \"{msiPath}\" /quiet")
                {
                    UseShellExecute = true,
                    Verb = "runas"
                };
                Process.Start(psi);
                lblStatus.Text = "Node.js 설치 파일 실행됨 (관리자 권한)";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Node.js 설치 실행 오류: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnInstallGemini_Click(object sender, EventArgs e)
        {
            try
            {
                string psCmd = @"
$signature = @'
[DllImport(""user32.dll"", SetLastError = true)]
public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
'@
$user32 = Add-Type -MemberDefinition $signature -Name ""User32"" -Namespace ""Win32"" -PassThru
$hwnd = (Get-Process -Id $PID).MainWindowHandle
if ($hwnd -ne [System.IntPtr]::Zero) {
    Add-Type -AssemblyName System.Windows.Forms
    $screen = [System.Windows.Forms.Screen]::PrimaryScreen.WorkingArea
    $x = ($screen.Width - 800) / 2
    $y = ($screen.Height - 600) / 2
    $user32::SetWindowPos($hwnd, 0, $x, $y, 800, 600, 0)
}

Set-ExecutionPolicy RemoteSigned -Scope CurrentUser -Force
npm install -g @google/gemini-cli
Write-Host ""Installation complete. Press any key to exit.""
$Host.UI.RawUI.ReadKey(""NoEcho,IncludeKeyDown"") | Out-Null
";

                var bytes = System.Text.Encoding.Unicode.GetBytes(psCmd);
                var encodedCommand = Convert.ToBase64String(bytes);

                var psi = new ProcessStartInfo("powershell.exe", $"-NoExit -EncodedCommand {encodedCommand}")
                {
                    UseShellExecute = true,
                    Verb = "runas"
                };
                Process.Start(psi);
                lblStatus.Text = "Gemini CLI 설치 명령 실행됨 (PowerShell 관리자 창)";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gemini CLI 설치 실행 오류: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRunGemini_Click(object sender, EventArgs e)
        {
            try
            {
                var psi = new ProcessStartInfo("powershell.exe", "-NoExit -Command gemini")
                {
                    UseShellExecute = true
                };
                Process.Start(psi);
                lblStatus.Text = "Gemini CLI 실행됨 (PowerShell 창)";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gemini CLI 실행 오류: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Task<ProcessResult> RunProcessAsync(string fileName, string arguments)
        {
            var tcs = new TaskCompletionSource<ProcessResult>();

            var process = new Process
            {
                StartInfo =
                {
                    FileName = fileName,
                    Arguments = arguments,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                },
                EnableRaisingEvents = true
            };

            var stdOut = new StringBuilder();
            var stdErr = new StringBuilder();

            process.OutputDataReceived += (sender, args) => {
                if (args.Data != null) stdOut.AppendLine(args.Data);
            };
            process.ErrorDataReceived += (sender, args) => {
                if (args.Data != null) stdErr.AppendLine(args.Data);
            };

            process.Exited += (sender, args) =>
            {
                tcs.SetResult(new ProcessResult 
                { 
                    ExitCode = process.ExitCode, 
                    Output = stdOut.ToString(), 
                    Error = stdErr.ToString() 
                });
                process.Dispose();
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return tcs.Task;
        }
        
        private Task<int> RunElevatedProcessAsync(string fileName, string arguments)
        {
            var tcs = new TaskCompletionSource<int>();

            var process = new Process
            {
                StartInfo =
                {
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = true,
                    Verb = "runas"
                },
                EnableRaisingEvents = true
            };

            process.Exited += (sender, args) =>
            {
                tcs.SetResult(process.ExitCode);
                process.Dispose();
            };

            try
            {
                process.Start();
            }
            catch (System.ComponentModel.Win32Exception)
            {
                // UAC dialog was cancelled by the user.
                tcs.SetResult(1223); 
            }

            return tcs.Task;
        }
    }

    public class McpServerInfo
    {
        public string Name { get; set; } = string.Empty;
        public string PackageId { get; set; } = string.Empty;
    }

    public class ProcessResult
    {
        public int ExitCode { get; set; }
        public string Output { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
    }
}