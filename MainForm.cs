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
                    new McpServerInfo { Name = "Google Drive MCP", PackageId = "@smithery-ai/google-drive" },
                    new McpServerInfo { Name = "Slack MCP", PackageId = "@smithery-ai/slack" }
                };

                InitializeComponent();

                // Gemini CLI 버튼은 전제 조건이 충족될 때까지 비활성화
                btnInstallGemini.Enabled = false;
                btnRunGemini.Enabled = false;

                // 폼 로드 시 초기 상태를 반영하도록 이벤트 연결
                this.Load += MainForm_Load;

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

            try
            {
                string settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".gemini", "settings.json");
                var root = new System.Collections.Generic.Dictionary<string, object>();

                if (File.Exists(settingsPath))
                {
                    string json = await File.ReadAllTextAsync(settingsPath);
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        root = JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(json, new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
                    }
                }

                var mcpServersDict = new System.Collections.Generic.Dictionary<string, object>();
                if (root.ContainsKey("mcpServers") && root["mcpServers"] is JsonElement mcpServersElement)
                {
                    mcpServersDict = mcpServersElement.Deserialize<System.Collections.Generic.Dictionary<string, object>>() ?? new System.Collections.Generic.Dictionary<string, object>();
                }

                mcpServersDict[mcpServer.PackageId] = new
                {
                    httpUrl = "https://api.githubcopilot.com/mcp/",
                    trust = true,
                    headers = new { Authorization = $"Bearer {apiKey}" }
                };

                root["mcpServers"] = mcpServersDict;

                var newJson = JsonSerializer.Serialize(root, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(settingsPath, newJson);

                lblStatus.Text = $"{mcpServer.Name} 설치 완료.";
                row.Cells[1].Value = "설치됨";
                row.Cells[2].Value = "삭제";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{mcpServer.Name} 설치 실패.\n오류: {ex.Message}", "설치 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    var root = JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(json, new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });

                    if (root != null && root.ContainsKey("mcpServers"))
                    {
                        var mcpServers = (System.Collections.Generic.Dictionary<string, object>)((JsonElement)root["mcpServers"]).Deserialize<System.Collections.Generic.Dictionary<string, object>>();
                        if (mcpServers.ContainsKey(mcpServer.PackageId))
                        {
                            mcpServers.Remove(mcpServer.PackageId);
                            root["mcpServers"] = mcpServers;
                            var newJson = JsonSerializer.Serialize(root, new JsonSerializerOptions { WriteIndented = true });
                            File.WriteAllText(settingsPath, newJson);
                        }
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
                if (string.IsNullOrWhiteSpace(json)) return;

                var doc = JsonDocument.Parse(json, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });

                if (doc.RootElement.TryGetProperty("mcpServers", out var mcpServersElement))
                {
                    foreach (DataGridViewRow row in dgvMcpList.Rows)
                    {
                        string? mcpName = row.Cells[0].Value?.ToString();
                        if (string.IsNullOrEmpty(mcpName)) continue;

                        var mcpServer = _mcpServers.Find(s => s.Name == mcpName);
                        if (mcpServer == null) continue;

                        if (mcpServersElement.TryGetProperty(mcpServer.PackageId, out _))
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

        private async void BtnInstallNode_Click(object sender, EventArgs e)
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

                lblStatus.Text = "Node.js 설치 중... (관리자 권한)";
                btnInstallNode.Enabled = false;

                var exitCode = await RunElevatedProcessAsync("msiexec.exe", $"/i \"{msiPath}\" /quiet /norestart");

                if (exitCode == 0)
                {
                    lblStatus.Text = "Node.js 설치 완료.";
                }
                else if (exitCode == 1223) // UAC Cancelled
                {
                    lblStatus.Text = "Node.js 설치가 사용자에 의해 취소되었습니다.";
                }
                else
                {
                    lblStatus.Text = $"Node.js 설치 실패 (코드: {exitCode}).";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Node.js 설치 실행 오류: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "준비";
            }
            finally
            {
                btnInstallNode.Enabled = true;
                await CheckPrerequisites();
            }
        }

        private async void BtnInstallGemini_Click(object sender, EventArgs e)
        {
            try
            {
                lblStatus.Text = "Gemini CLI 설치 중... (PowerShell 관리자 창)";
                btnInstallGemini.Enabled = false;

                string psCmd = "Set-ExecutionPolicy RemoteSigned -Scope CurrentUser -Force; npm install -g @google/gemini-cli";
                var bytes = System.Text.Encoding.Unicode.GetBytes(psCmd);
                var encodedCommand = Convert.ToBase64String(bytes);

                var exitCode = await RunElevatedProcessAsync("powershell.exe", $"-Command \"Set-ExecutionPolicy RemoteSigned -Scope CurrentUser -Force; npm install -g @google/gemini-cli\"");

                if (exitCode == 0)
                {
                    lblStatus.Text = "Gemini CLI 설치 완료.";
                }
                else if (exitCode == 1223) // UAC Cancelled
                {
                    lblStatus.Text = "Gemini CLI 설치가 사용자에 의해 취소되었습니다.";
                }
                else
                {
                    // Note: PowerShell might not return a specific error code for npm failures.
                    // We rely on IsGeminiCliInstalled check.
                    lblStatus.Text = "Gemini CLI 설치가 완료되었거나 실패했습니다.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gemini CLI 설치 실행 오류: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "준비";
            }
            finally
            {
                await CheckPrerequisites();
            }
        }

        private void BtnRunGemini_Click(object sender, EventArgs e)
        {
            try
            {
                var psi = new ProcessStartInfo("powershell.exe", "-Command gemini")
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

        private void BtnManual_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo("https://github.com/nohseongmin/McpManager/blob/main/readme.md") { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("매뉴얼 링크 열기 오류: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    Verb = "runas",
                    WindowStyle = ProcessWindowStyle.Hidden, // 창 숨기기
                    CreateNoWindow = true, // 창 생성 안함
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
            catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 1223)
            {
                // UAC dialog was cancelled by the user.
                tcs.SetResult(1223);
            }
            catch(Exception)
            {
                tcs.SetResult(-1); // Other errors
            }


            return tcs.Task;
        }

        // 폼이 로드될 때 MCP 상태를 확인함
        private async void MainForm_Load(object? sender, EventArgs e)
        {
            try
            {
                await CheckPrerequisites();
                CheckMcpStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"초기 MCP 상태 확인 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task CheckPrerequisites()
        {
            bool nodeInstalled = await IsNodeInstalled();
            // Gemini CLI 설치 여부는 버튼 활성화에 직접적인 영향을 주지 않도록 수정
            // 대신, 설치 후 상태를 다시 확인하여 UI 업데이트
            bool geminiCliInstalled = await IsGeminiCliInstalledAsync();

            // Node.js 설치 여부에 따라 Gemini CLI 설치 버튼 활성화
            btnInstallGemini.Enabled = nodeInstalled;

            // Node.js와 Gemini CLI가 모두 설치되어야 실행 버튼 활성화
            btnRunGemini.Enabled = nodeInstalled && geminiCliInstalled;

            // 설치 상태에 따라 Node.js 버튼 텍스트 변경
            btnInstallNode.Text = nodeInstalled ? "Node.js (설치됨)" : "Node.js 설치";
            btnInstallGemini.Text = geminiCliInstalled ? "Gemini CLI (설치됨)" : "Gemini CLI 설치";
        }


        private async Task<bool> IsNodeInstalled()
        {
            var result = await RunProcessAsync("cmd.exe", "/c where node");
            return result.ExitCode == 0 && !string.IsNullOrWhiteSpace(result.Output);
        }

        private async Task<bool> IsGeminiCliInstalledAsync()
        {
            var result = await RunProcessAsync("cmd.exe", "/c where gemini");
            return result.ExitCode == 0 && !string.IsNullOrWhiteSpace(result.Output);
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
// No code changes required to run the project locally.