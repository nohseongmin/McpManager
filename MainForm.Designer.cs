namespace McpManager
{
    partial class MainForm
    {
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.GroupBox groupBoxGemini;
    private System.Windows.Forms.Button btnInstallNode;
    private System.Windows.Forms.Button btnInstallGemini;
    private System.Windows.Forms.Button btnRunGemini;
    private System.Windows.Forms.GroupBox groupBoxMcp;
    private System.Windows.Forms.DataGridView dgvMcpList;
    private System.Windows.Forms.ComboBox comboBoxLang;
    private System.Windows.Forms.Label lblStatus;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.groupBoxGemini = new System.Windows.Forms.GroupBox();
            this.btnInstallNode = new System.Windows.Forms.Button();
            this.btnInstallGemini = new System.Windows.Forms.Button();
            this.btnRunGemini = new System.Windows.Forms.Button();
            this.groupBoxMcp = new System.Windows.Forms.GroupBox();
            this.dgvMcpList = new System.Windows.Forms.DataGridView();
        this.comboBoxLang = new System.Windows.Forms.ComboBox();
        this.lblStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // groupBoxGemini
            // 
            this.groupBoxGemini.Controls.Add(this.btnInstallNode);
            this.groupBoxGemini.Controls.Add(this.btnInstallGemini);
            this.groupBoxGemini.Controls.Add(this.btnRunGemini);
            this.groupBoxGemini.Location = new System.Drawing.Point(12, 12);
            this.groupBoxGemini.Name = "groupBoxGemini";
            this.groupBoxGemini.Size = new System.Drawing.Size(560, 70);
            this.groupBoxGemini.TabIndex = 0;
            this.groupBoxGemini.TabStop = false;
            this.groupBoxGemini.Text = "Gemini CLI 설정";
            // 
            // btnInstallNode
            // 
            this.btnInstallNode.Location = new System.Drawing.Point(6, 25);
            this.btnInstallNode.Name = "btnInstallNode";
            this.btnInstallNode.Size = new System.Drawing.Size(110, 30);
            this.btnInstallNode.TabIndex = 0;
            this.btnInstallNode.Text = "Node.js 설치";
            this.btnInstallNode.UseVisualStyleBackColor = true;
            // 
            // btnInstallGemini
            // 
            this.btnInstallGemini.Location = new System.Drawing.Point(122, 25);
            this.btnInstallGemini.Name = "btnInstallGemini";
            this.btnInstallGemini.Size = new System.Drawing.Size(120, 30);
            this.btnInstallGemini.TabIndex = 1;
            this.btnInstallGemini.Text = "Gemini CLI 설치";
            this.btnInstallGemini.UseVisualStyleBackColor = true;
            // 
            // btnRunGemini
            // 
            this.btnRunGemini.Location = new System.Drawing.Point(248, 25);
            this.btnRunGemini.Name = "btnRunGemini";
            this.btnRunGemini.Size = new System.Drawing.Size(120, 30);
            this.btnRunGemini.TabIndex = 2;
            this.btnRunGemini.Text = "Gemini CLI 실행";
            this.btnRunGemini.UseVisualStyleBackColor = true;
            this.btnInstallNode.Click += new System.EventHandler(this.BtnInstallNode_Click);
            this.btnInstallGemini.Click += new System.EventHandler(this.BtnInstallGemini_Click);
            this.btnRunGemini.Click += new System.EventHandler(this.BtnRunGemini_Click);
            // 
            // groupBoxMcp
            // 
            this.groupBoxMcp.Controls.Add(this.dgvMcpList);
            this.groupBoxMcp.Location = new System.Drawing.Point(12, 88);
            this.groupBoxMcp.Name = "groupBoxMcp";
            this.groupBoxMcp.Size = new System.Drawing.Size(560, 250);
            this.groupBoxMcp.TabIndex = 1;
            this.groupBoxMcp.TabStop = false;
            this.groupBoxMcp.Text = "MCP 서버 관리";
            // 
            // dgvMcpList
            // 
            this.dgvMcpList.AllowUserToAddRows = false;
            this.dgvMcpList.AllowUserToDeleteRows = false;
            this.dgvMcpList.ReadOnly = true;
            this.dgvMcpList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvMcpList.MultiSelect = false;
            this.dgvMcpList.RowHeadersVisible = false;
            this.dgvMcpList.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvMcpList.Location = new System.Drawing.Point(6, 25);
            this.dgvMcpList.Name = "dgvMcpList";
            this.dgvMcpList.Size = new System.Drawing.Size(548, 180);
            this.dgvMcpList.TabIndex = 0;
            this.dgvMcpList.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DgvMcpList_CellContentClick);
            // 
            // comboBoxLang
            // 
            this.comboBoxLang.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLang.FormattingEnabled = true;
            this.comboBoxLang.Items.AddRange(new object[] {"한국어", "English"});
            this.comboBoxLang.Location = new System.Drawing.Point(451, 344);
            this.comboBoxLang.Name = "comboBoxLang";
            this.comboBoxLang.Size = new System.Drawing.Size(121, 23);
            this.comboBoxLang.TabIndex = 2;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(12, 347);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(39, 15);
            this.lblStatus.TabIndex = 3;
            this.lblStatus.Text = "준비";
            // 
        this.Controls.Add(this.lblStatus);
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 401);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.comboBoxLang);
            this.Controls.Add(this.groupBoxMcp);
            this.Controls.Add(this.groupBoxGemini);
            this.Name = "MainForm";
            this.Text = "Mcp Manager";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

    }
}
