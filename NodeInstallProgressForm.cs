using System;
using System.Drawing;
using System.Windows.Forms;

namespace McpManager
{
    public enum StepStatus { Waiting, InProgress, Done, Failed }

    public partial class NodeInstallProgressForm : Form
    {
        private Label lblStep1 = null!;
        private Label lblStep2 = null!;
        private Label lblStep3 = null!;
        private Button btnClose = null!;

        public NodeInstallProgressForm()
        {
            InitializeComponent();
            SetStepStatus(1, StepStatus.Waiting);
            SetStepStatus(2, StepStatus.Waiting);
            SetStepStatus(3, StepStatus.Waiting);
        }

        private void InitializeComponent()
        {
            this.lblStep1 = new Label() { Left = 30, Top = 20, Width = 320, Text = "1) Node.js 설치 파일 다운로드" };
            this.lblStep2 = new Label() { Left = 30, Top = 50, Width = 320, Text = "2) Node.js 설치" };
            this.lblStep3 = new Label() { Left = 30, Top = 80, Width = 320, Text = "3) Node.js 설치 확인" };
            this.btnClose = new Button() { Left = 120, Top = 120, Width = 120, Text = "닫기", Enabled = false };
            this.btnClose.Click += (s, e) => this.Close();
            this.ClientSize = new System.Drawing.Size(380, 170);
            this.Controls.Add(lblStep1);
            this.Controls.Add(lblStep2);
            this.Controls.Add(lblStep3);
            this.Controls.Add(btnClose);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Node.js 설치 진행상황";
        }

        public void SetStepStatus(int step, StepStatus status)
        {
            Label? targetLabel = null;
            string baseText = "";
            switch (step)
            {
                case 1: targetLabel = lblStep1; baseText = "1) Node.js 설치 파일 다운로드"; break;
                case 2: targetLabel = lblStep2; baseText = "2) Node.js 설치"; break;
                case 3: targetLabel = lblStep3; baseText = "3) Node.js 설치 확인"; break;
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
