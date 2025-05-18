using System.Windows.Forms;
using System.Drawing;

namespace FlipFix
{
    partial class FixForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FixForm));
            toolTip = new ToolTip(components);
            btnClear = new Button();
            btnSwap = new Button();
            btnDownload = new Button();
            btnGenerate = new Button();
            lblStatus = new Label();
            lblPageCount = new Label();
            lblPageCountTitle = new Label();
            lblFileName = new Label();
            lblFileNameTitle = new Label();
            panelDropZone = new Panel();
            lblDropText = new Label();
            btnRedo = new Button();
            btnUndo = new Button();
            lblZoomPercentage = new Label();
            documentPreviewPanel = new Panel();
            previewTableLayout = new TableLayoutPanel();
            lblPreviewTitle = new Label();
            btnZoomOut = new Button();
            btnZoomIn = new Button();
            toolPanel = new Panel();
            progressBar = new ProgressBar();
            panelDropZone.SuspendLayout();
            documentPreviewPanel.SuspendLayout();
            toolPanel.SuspendLayout();
            SuspendLayout();
            // 
            // toolTip
            // 
            toolTip.AutoPopDelay = 5000;
            toolTip.InitialDelay = 500;
            toolTip.ReshowDelay = 100;
            toolTip.ShowAlways = true;
            // 
            // btnClear
            // 
            btnClear.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClear.BackColor = Color.FromArgb(233, 163, 25);
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.FlatStyle = FlatStyle.Flat;
            btnClear.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnClear.ForeColor = Color.White;
            btnClear.Location = new Point(779, 74);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(154, 32);
            btnClear.TabIndex = 18;
            btnClear.Text = "Clear";
            toolTip.SetToolTip(btnClear, "Clear the current PDF and reset the application");
            btnClear.UseVisualStyleBackColor = false;
            btnClear.Click += BtnClear_Click;
            // 
            // btnSwap
            // 
            btnSwap.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSwap.BackColor = Color.FromArgb(133, 25, 60);
            btnSwap.FlatAppearance.BorderSize = 0;
            btnSwap.FlatStyle = FlatStyle.Flat;
            btnSwap.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnSwap.ForeColor = Color.White;
            btnSwap.Location = new Point(779, 34);
            btnSwap.Name = "btnSwap";
            btnSwap.Size = new Size(154, 32);
            btnSwap.TabIndex = 17;
            btnSwap.Text = "Swap Page";
            toolTip.SetToolTip(btnSwap, "Swap the orientation of the selected page");
            btnSwap.UseVisualStyleBackColor = false;
            btnSwap.Click += BtnSwap_Click;
            // 
            // btnDownload
            // 
            btnDownload.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDownload.BackColor = Color.FromArgb(63, 125, 88);
            btnDownload.FlatAppearance.BorderSize = 0;
            btnDownload.FlatStyle = FlatStyle.Flat;
            btnDownload.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnDownload.ForeColor = Color.White;
            btnDownload.Location = new Point(609, 74);
            btnDownload.Name = "btnDownload";
            btnDownload.Size = new Size(154, 32);
            btnDownload.TabIndex = 14;
            btnDownload.Text = "Download";
            toolTip.SetToolTip(btnDownload, "Download the modified PDF file");
            btnDownload.UseVisualStyleBackColor = false;
            btnDownload.Click += BtnDownload_Click;
            // 
            // btnGenerate
            // 
            btnGenerate.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnGenerate.BackColor = Color.FromArgb(13, 115, 119);
            btnGenerate.FlatAppearance.BorderSize = 0;
            btnGenerate.FlatStyle = FlatStyle.Flat;
            btnGenerate.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnGenerate.ForeColor = Color.White;
            btnGenerate.Location = new Point(609, 34);
            btnGenerate.Name = "btnGenerate";
            btnGenerate.Size = new Size(154, 32);
            btnGenerate.TabIndex = 13;
            btnGenerate.Text = "Generate";
            toolTip.SetToolTip(btnGenerate, "Generate the preview of the modified PDF");
            btnGenerate.UseVisualStyleBackColor = false;
            btnGenerate.Click += BtnGenerate_Click;
            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 10F);
            lblStatus.ForeColor = Color.FromArgb(52, 73, 94);
            lblStatus.Location = new Point(303, 66);
            lblStatus.MaximumSize = new Size(500, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(151, 19);
            lblStatus.TabIndex = 15;
            lblStatus.Text = "Status: Waiting for file...";
            toolTip.SetToolTip(lblStatus, "Current status of the PDF processing");
            // 
            // lblPageCount
            // 
            lblPageCount.AutoSize = true;
            lblPageCount.Font = new Font("Segoe UI", 10F);
            lblPageCount.ForeColor = Color.FromArgb(52, 73, 94);
            lblPageCount.Location = new Point(394, 41);
            lblPageCount.Name = "lblPageCount";
            lblPageCount.Size = new Size(15, 19);
            lblPageCount.TabIndex = 8;
            lblPageCount.Text = "-";
            toolTip.SetToolTip(lblPageCount, "Number of pages in the loaded PDF");
            // 
            // lblPageCountTitle
            // 
            lblPageCountTitle.AutoSize = true;
            lblPageCountTitle.Font = new Font("Segoe UI", 10F);
            lblPageCountTitle.ForeColor = Color.FromArgb(52, 73, 94);
            lblPageCountTitle.Location = new Point(303, 41);
            lblPageCountTitle.Name = "lblPageCountTitle";
            lblPageCountTitle.Size = new Size(84, 19);
            lblPageCountTitle.TabIndex = 7;
            lblPageCountTitle.Text = "Page Count:";
            toolTip.SetToolTip(lblPageCountTitle, "Label for the page count");
            // 
            // lblFileName
            // 
            lblFileName.AutoSize = true;
            lblFileName.Font = new Font("Segoe UI", 10F);
            lblFileName.ForeColor = Color.FromArgb(52, 73, 94);
            lblFileName.Location = new Point(394, 13);
            lblFileName.Name = "lblFileName";
            lblFileName.Size = new Size(15, 19);
            lblFileName.TabIndex = 2;
            lblFileName.Text = "-";
            toolTip.SetToolTip(lblFileName, "Name of the loaded PDF file");
            // 
            // lblFileNameTitle
            // 
            lblFileNameTitle.AutoSize = true;
            lblFileNameTitle.Font = new Font("Segoe UI", 10F);
            lblFileNameTitle.ForeColor = Color.FromArgb(52, 73, 94);
            lblFileNameTitle.Location = new Point(303, 13);
            lblFileNameTitle.Name = "lblFileNameTitle";
            lblFileNameTitle.Size = new Size(72, 19);
            lblFileNameTitle.TabIndex = 1;
            lblFileNameTitle.Text = "File Name:";
            toolTip.SetToolTip(lblFileNameTitle, "Label for the file name");
            // 
            // panelDropZone
            // 
            panelDropZone.AllowDrop = true;
            panelDropZone.BackColor = Color.FromArgb(240, 244, 248);
            panelDropZone.BorderStyle = BorderStyle.FixedSingle;
            panelDropZone.Controls.Add(lblDropText);
            panelDropZone.Cursor = Cursors.Hand;
            panelDropZone.Location = new Point(12, 8);
            panelDropZone.Name = "panelDropZone";
            panelDropZone.Size = new Size(280, 110);
            panelDropZone.TabIndex = 0;
            toolTip.SetToolTip(panelDropZone, "Drag and drop a PDF file here or click to browse");
            panelDropZone.Click += PanelDropZone_Click;
            panelDropZone.DragDrop += Panel_DragDrop;
            panelDropZone.DragEnter += Panel_DragEnter;
            // 
            // lblDropText
            // 
            lblDropText.BackColor = Color.FromArgb(240, 244, 248);
            lblDropText.Dock = DockStyle.Fill;
            lblDropText.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblDropText.ForeColor = Color.FromArgb(74, 144, 226);
            lblDropText.Location = new Point(0, 0);
            lblDropText.Name = "lblDropText";
            lblDropText.Size = new Size(278, 108);
            lblDropText.TabIndex = 0;
            lblDropText.Text = "📄 Drag and drop a PDF file here\r\nor click to browse files";
            lblDropText.TextAlign = ContentAlignment.MiddleCenter;
            toolTip.SetToolTip(lblDropText, "Drag and drop a PDF file here or click to browse");
            lblDropText.Click += PanelDropZone_Click;
            // 
            // btnRedo
            // 
            btnRedo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRedo.BackColor = Color.FromArgb(57, 62, 70);
            btnRedo.FlatAppearance.BorderSize = 0;
            btnRedo.FlatStyle = FlatStyle.Flat;
            btnRedo.ForeColor = Color.White;
            btnRedo.Image = (Image)resources.GetObject("btnRedo.Image");
            btnRedo.Location = new Point(771, 127);
            btnRedo.Name = "btnRedo";
            btnRedo.Size = new Size(40, 24);
            btnRedo.TabIndex = 20;
            toolTip.SetToolTip(btnRedo, "Redo the last undone action");
            btnRedo.UseVisualStyleBackColor = false;
            btnRedo.Click += BtnRedo_Click;
            // 
            // btnUndo
            // 
            btnUndo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnUndo.BackColor = Color.FromArgb(57, 62, 70);
            btnUndo.FlatAppearance.BorderSize = 0;
            btnUndo.FlatStyle = FlatStyle.Flat;
            btnUndo.ForeColor = Color.White;
            btnUndo.Image = (Image)resources.GetObject("btnUndo.Image");
            btnUndo.Location = new Point(726, 127);
            btnUndo.Name = "btnUndo";
            btnUndo.Size = new Size(40, 24);
            btnUndo.TabIndex = 19;
            toolTip.SetToolTip(btnUndo, "Undo the last action");
            btnUndo.UseVisualStyleBackColor = false;
            btnUndo.Click += BtnUndo_Click;
            // 
            // lblZoomPercentage
            // 
            lblZoomPercentage.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblZoomPercentage.AutoSize = true;
            lblZoomPercentage.Font = new Font("Segoe UI", 9F);
            lblZoomPercentage.ForeColor = Color.FromArgb(52, 73, 94);
            lblZoomPercentage.Location = new Point(907, 130);
            lblZoomPercentage.Name = "lblZoomPercentage";
            lblZoomPercentage.Size = new Size(35, 15);
            lblZoomPercentage.TabIndex = 21;
            lblZoomPercentage.Text = "100%";
            toolTip.SetToolTip(lblZoomPercentage, "Current zoom level of the preview");
            // 
            // documentPreviewPanel
            // 
            documentPreviewPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            documentPreviewPanel.AutoScroll = true;
            documentPreviewPanel.BackColor = Color.White;
            documentPreviewPanel.BorderStyle = BorderStyle.FixedSingle;
            documentPreviewPanel.Controls.Add(previewTableLayout);
            documentPreviewPanel.Location = new Point(12, 155);
            documentPreviewPanel.Name = "documentPreviewPanel";
            documentPreviewPanel.Padding = new Padding(10);
            documentPreviewPanel.Size = new Size(932, 518);
            documentPreviewPanel.TabIndex = 12;
            toolTip.SetToolTip(documentPreviewPanel, "Preview of the PDF pages");
            // 
            // previewTableLayout
            // 
            previewTableLayout.ColumnCount = 2;
            previewTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            previewTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            previewTableLayout.Dock = DockStyle.Fill;
            previewTableLayout.Location = new Point(10, 10);
            previewTableLayout.Name = "previewTableLayout";
            previewTableLayout.RowCount = 1;
            previewTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            previewTableLayout.Size = new Size(910, 496);
            previewTableLayout.TabIndex = 0;
            toolTip.SetToolTip(previewTableLayout, "Layout for displaying PDF page previews");
            // 
            // lblPreviewTitle
            // 
            lblPreviewTitle.AutoSize = true;
            lblPreviewTitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblPreviewTitle.ForeColor = Color.FromArgb(74, 144, 226);
            lblPreviewTitle.Location = new Point(12, 133);
            lblPreviewTitle.Name = "lblPreviewTitle";
            lblPreviewTitle.Size = new Size(131, 20);
            lblPreviewTitle.TabIndex = 9;
            lblPreviewTitle.Text = "Document Pages:";
            toolTip.SetToolTip(lblPreviewTitle, "Title for the document preview section");
            // 
            // btnZoomOut
            // 
            btnZoomOut.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnZoomOut.BackColor = Color.FromArgb(74, 144, 226);
            btnZoomOut.FlatAppearance.BorderSize = 0;
            btnZoomOut.FlatStyle = FlatStyle.Flat;
            btnZoomOut.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnZoomOut.ForeColor = Color.White;
            btnZoomOut.Location = new Point(817, 127);
            btnZoomOut.Name = "btnZoomOut";
            btnZoomOut.Size = new Size(40, 24);
            btnZoomOut.TabIndex = 23;
            btnZoomOut.Text = "-";
            toolTip.SetToolTip(btnZoomOut, "Zoom out to reduce the preview size");
            btnZoomOut.UseVisualStyleBackColor = false;
            btnZoomOut.Click += BtnZoomOut_Click;
            // 
            // btnZoomIn
            // 
            btnZoomIn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnZoomIn.BackColor = Color.FromArgb(74, 144, 226);
            btnZoomIn.FlatAppearance.BorderSize = 0;
            btnZoomIn.FlatStyle = FlatStyle.Flat;
            btnZoomIn.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnZoomIn.ForeColor = Color.White;
            btnZoomIn.Location = new Point(862, 127);
            btnZoomIn.Name = "btnZoomIn";
            btnZoomIn.Size = new Size(40, 24);
            btnZoomIn.TabIndex = 22;
            btnZoomIn.Text = "+";
            toolTip.SetToolTip(btnZoomIn, "Zoom in to enlarge the preview size");
            btnZoomIn.UseVisualStyleBackColor = false;
            btnZoomIn.Click += BtnZoomIn_Click;
            // 
            // toolPanel
            // 
            toolPanel.BackColor = Color.FromArgb(240, 244, 248);
            toolPanel.Controls.Add(btnClear);
            toolPanel.Controls.Add(btnSwap);
            toolPanel.Controls.Add(btnDownload);
            toolPanel.Controls.Add(btnGenerate);
            toolPanel.Controls.Add(progressBar);
            toolPanel.Controls.Add(lblStatus);
            toolPanel.Controls.Add(lblPageCount);
            toolPanel.Controls.Add(lblPageCountTitle);
            toolPanel.Controls.Add(lblFileName);
            toolPanel.Controls.Add(lblFileNameTitle);
            toolPanel.Controls.Add(panelDropZone);
            toolPanel.Dock = DockStyle.Top;
            toolPanel.Location = new Point(0, 0);
            toolPanel.Name = "toolPanel";
            toolPanel.Size = new Size(956, 130);
            toolPanel.TabIndex = 18;
            // 
            // progressBar
            // 
            progressBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            progressBar.ForeColor = Color.SeaGreen;
            progressBar.Location = new Point(303, 88);
            progressBar.MarqueeAnimationSpeed = 30;
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(191, 23);
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.TabIndex = 16;
            progressBar.Visible = false;
            // 
            // FixForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(240, 244, 248);
            ClientSize = new Size(956, 685);
            Controls.Add(btnZoomIn);
            Controls.Add(btnZoomOut);
            Controls.Add(lblZoomPercentage);
            Controls.Add(btnUndo);
            Controls.Add(btnRedo);
            Controls.Add(lblPreviewTitle);
            Controls.Add(documentPreviewPanel);
            Controls.Add(toolPanel);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "FixForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "PDF Flip Fix";
            panelDropZone.ResumeLayout(false);
            documentPreviewPanel.ResumeLayout(false);
            toolPanel.ResumeLayout(false);
            toolPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel toolPanel;
        private Panel panelDropZone;
        private Label lblDropText;
        private Label lblFileNameTitle;
        private Label lblFileName;
        private Label lblPageCountTitle;
        private Label lblPageCount;
        private Label lblPreviewTitle;
        private Panel documentPreviewPanel;
        private TableLayoutPanel previewTableLayout;
        private Button btnGenerate;
        private Button btnDownload;
        private Label lblStatus;
        private ProgressBar progressBar;
        private Button btnSwap;
        private Button btnClear;
        private Button btnUndo;
        private Button btnRedo;
        private Label lblZoomPercentage;
        private Button btnZoomOut;
        private Button btnZoomIn;
        private ToolTip toolTip;
    }
}