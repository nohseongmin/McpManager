using System;
using System.Drawing;
using System.Windows.Forms;

namespace McpManager
{
    public partial class GeminiInstallProgressForm : Form
    {
        private Label lblStep1 = null!;
        private Label lblStep2 = null!;
        private Button btnClose = null!;

        public GeminiInstallProgressForm()
        {
            InitializeComponent();
            SetStepStatus(1, StepStatus.Waiting);
            SetStepStatus(2, StepStatus.Waiting);
        }

        private void InitializeComponent()
        {
            this.lblStep1 = new Label() { Left = 30, Top = 20, Width = 320, Text = "1) PowerShell 실행 정책 설정" };
            this.lblStep2 = new Label() { Left = 30, Top = 50, Width = 320, Text = "2) Gemini CLI 설치 (npm)" };
            this.btnClose = new Button() { Left = 120, Top = 90, Width = 120, Text = "닫기", Enabled = false };
            this.btnClose.Click += (s, e) => this.Close();
            this.ClientSize = new System.Drawing.Size(380, 140);
            this.Controls.Add(lblStep1);
            this.Controls.Add(lblStep2);
            this.Controls.Add(btnClose);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Gemini CLI 설치 진행상황";
        }

        public void SetStepStatus(int step, StepStatus status)
        {
            Label? targetLabel = null;
            string baseText = "";
            switch (step)
            {
                case 1: targetLabel = lblStep1; baseText = "1) PowerShell 실행 정책 설정"; break;
                case 2: targetLabel = lblStep2; baseText = "2) Gemini CLI 설치 (npm)"; break;
            }

            if (targetLabel == null) return;

            string statusText = "";
            Color statusColor = Color.Black;

            switch (status)
            {
                case StepStatus.Waiting:
                    statusText = "... 대기중";
                    statusColor = Color.Gray;
                    break;
                case StepStatus.InProgress:
                    statusText = "... 진행중";
                    statusColor = Color.Blue;
                    break;
                case StepStatus.Done:
                    statusText = "... 완료";
                    statusColor = Color.Green;
                    break;
                case StepStatus.Failed:
                    statusText = "... 실패";
                    statusColor = Color.Red;
                    break;
            }

            targetLabel.Text = baseText + statusText;
            targetLabel.ForeColor = statusColor;
        }

        public void EnableCloseButton()
        {
            btnClose.Enabled = true;
            btnClose.Text = "닫기";
        }
    }
}
