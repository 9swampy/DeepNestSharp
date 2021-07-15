namespace DeepNestSharp
{
  partial class NestResults
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NestResults));
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.listViewTopNests = new System.Windows.Forms.ListView();
      this.columnHeaderFitness = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnHeaderDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.toolStrip1 = new System.Windows.Forms.ToolStrip();
      this.runButton = new System.Windows.Forms.ToolStripButton();
      this.stopButton = new System.Windows.Forms.ToolStripButton();
      this.exportButton = new System.Windows.Forms.ToolStripButton();
      this.showHideButton = new System.Windows.Forms.ToolStripButton();
      this.nestPreview = new System.Windows.Forms.PictureBox();
      this.panel1 = new System.Windows.Forms.Panel();
      this.tableLayoutPanel1.SuspendLayout();
      this.toolStrip1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nestPreview)).BeginInit();
      this.SuspendLayout();
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.ColumnCount = 2;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 277F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.Controls.Add(this.listViewTopNests, 0, 1);
      this.tableLayoutPanel1.Controls.Add(this.toolStrip1, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.nestPreview, 1, 2);
      this.tableLayoutPanel1.Controls.Add(this.panel1, 1, 1);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 3;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size(1222, 654);
      this.tableLayoutPanel1.TabIndex = 2;
      // 
      // listViewTopNests
      // 
      this.listViewTopNests.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderFitness,
            this.columnHeaderDate});
      this.listViewTopNests.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listViewTopNests.FullRowSelect = true;
      this.listViewTopNests.GridLines = true;
      this.listViewTopNests.HideSelection = false;
      this.listViewTopNests.Location = new System.Drawing.Point(3, 29);
      this.listViewTopNests.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.listViewTopNests.MultiSelect = false;
      this.listViewTopNests.Name = "listViewTopNests";
      this.tableLayoutPanel1.SetRowSpan(this.listViewTopNests, 2);
      this.listViewTopNests.Size = new System.Drawing.Size(271, 623);
      this.listViewTopNests.TabIndex = 5;
      this.listViewTopNests.UseCompatibleStateImageBehavior = false;
      this.listViewTopNests.View = System.Windows.Forms.View.Details;
      // 
      // columnHeaderFitness
      // 
      this.columnHeaderFitness.Text = "Fitness";
      this.columnHeaderFitness.Width = 85;
      // 
      // columnHeaderDate
      // 
      this.columnHeaderDate.Text = "Date";
      this.columnHeaderDate.Width = 152;
      // 
      // toolStrip1
      // 
      this.tableLayoutPanel1.SetColumnSpan(this.toolStrip1, 2);
      this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
      this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.runButton,
            this.stopButton,
            this.exportButton,
            this.showHideButton});
      this.toolStrip1.Location = new System.Drawing.Point(0, 0);
      this.toolStrip1.Name = "toolStrip1";
      this.toolStrip1.Size = new System.Drawing.Size(1222, 27);
      this.toolStrip1.TabIndex = 0;
      this.toolStrip1.Text = "toolStrip1";
      // 
      // runButton
      // 
      this.runButton.Image = ((System.Drawing.Image)(resources.GetObject("runButton.Image")));
      this.runButton.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.runButton.Name = "runButton";
      this.runButton.Size = new System.Drawing.Size(58, 24);
      this.runButton.Text = "Run";
      // 
      // stopButton
      // 
      this.stopButton.Image = ((System.Drawing.Image)(resources.GetObject("stopButton.Image")));
      this.stopButton.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.stopButton.Name = "stopButton";
      this.stopButton.Size = new System.Drawing.Size(64, 24);
      this.stopButton.Text = "Stop";
      // 
      // exportButton
      // 
      this.exportButton.Image = ((System.Drawing.Image)(resources.GetObject("exportButton.Image")));
      this.exportButton.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.exportButton.Name = "exportButton";
      this.exportButton.Size = new System.Drawing.Size(76, 24);
      this.exportButton.Text = "Export";
      // 
      // showHideButton
      // 
      this.showHideButton.Image = ((System.Drawing.Image)(resources.GetObject("showHideButton.Image")));
      this.showHideButton.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.showHideButton.Name = "showHideButton";
      this.showHideButton.Size = new System.Drawing.Size(137, 24);
      this.showHideButton.Text = "Show/Hide Info";
      // 
      // nestPreview
      // 
      this.nestPreview.Dock = System.Windows.Forms.DockStyle.Fill;
      this.nestPreview.Location = new System.Drawing.Point(277, 59);
      this.nestPreview.Margin = new System.Windows.Forms.Padding(0);
      this.nestPreview.Name = "nestPreview";
      this.nestPreview.Size = new System.Drawing.Size(945, 595);
      this.nestPreview.TabIndex = 0;
      this.nestPreview.TabStop = false;
      // 
      // panel1
      // 
      this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel1.Location = new System.Drawing.Point(277, 27);
      this.panel1.Margin = new System.Windows.Forms.Padding(0);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(945, 32);
      this.panel1.TabIndex = 1;
      // 
      // NestResults
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tableLayoutPanel1);
      this.Name = "NestResults";
      this.Size = new System.Drawing.Size(1222, 654);
      this.tableLayoutPanel1.ResumeLayout(false);
      this.tableLayoutPanel1.PerformLayout();
      this.toolStrip1.ResumeLayout(false);
      this.toolStrip1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nestPreview)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.ListView listViewTopNests;
    private System.Windows.Forms.ColumnHeader columnHeaderFitness;
    private System.Windows.Forms.ColumnHeader columnHeaderDate;
    private System.Windows.Forms.ToolStrip toolStrip1;
    private System.Windows.Forms.ToolStripButton runButton;
    private System.Windows.Forms.ToolStripButton stopButton;
    private System.Windows.Forms.ToolStripButton exportButton;
    private System.Windows.Forms.ToolStripButton showHideButton;
    private System.Windows.Forms.PictureBox nestPreview;
    private System.Windows.Forms.Panel panel1;
  }
}
