using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DeepNestSharp;
using DeepNestLib.Ui;
using DeepNestLib;
using System.IO;
using System.Drawing.Drawing2D;

namespace DeepNestPort
{
  public partial class PartsList : UserControl
  {
    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);

      try
      {
        sheetsList.SetObjects(sheetsInfos);
        sheetsInfos.Add(new SheetLoadInfo() { Width = SvgNest.Config.SheetWidth, Height = SvgNest.Config.SheetHeight, Quantity = SvgNest.Config.SheetQuantity });

        this.multiplierUpDown.Value = SvgNest.Config.Multiplier;
      }
      catch (Exception)
      {
        //NOP
      }
    }

    List<ISheetLoadInfo> sheetsInfos => Form1.DeepNestState.SheetInfos;

    public List<DetailLoadInfo> Infos => Form1.DeepNestState.PartInfos;

    private object preview;
    private DrawingContext ctx3;

    private MessageBoxService messageBoxService = MessageBoxService.Default;
    private PreviewService previewService = PreviewService.Default;
    private FileService fileService = FileService.Default;

    public DrawingContext Ctx3
    {
      get
      {
        if (ctx3 == null)
        {
          ctx3 = new DrawingContext(partsPreview);
          ctx3.FocusOnMove = false;
        }

        return ctx3;
      }
    }

    public PartsList()
    {
      InitializeComponent();

      //hack
      toolStripButton9.BackgroundImageLayout = ImageLayout.None;
      toolStripButton9.BackgroundImage = new Bitmap(1, 1);
      toolStripButton9.BackColor = Color.LightGreen;
    }

    public void setToToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (objectListView.SelectedObjects.Count == 0) return;
      QntDialog q = new QntDialog();
      q.ShowDialog();

      foreach (var item in objectListView.SelectedObjects)
      {
        (item as DetailLoadInfo).Quantity = q.Qnt;
      }

      objectListView.RefreshObjects(objectListView.SelectedObjects);
    }

    public void multiplyToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (objectListView.SelectedObjects.Count == 0) return;
      QntDialog q = new QntDialog();
      q.ShowDialog();

      foreach (var item in objectListView.SelectedObjects)
      {
        (item as DetailLoadInfo).Quantity *= q.Qnt;
      }

      objectListView.RefreshObjects(objectListView.SelectedObjects);
    }

    public void divideToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (objectListView.SelectedObjects.Count == 0) return;
      QntDialog q = new QntDialog();
      q.ShowDialog();
      if (q.Qnt == 0) return;
      foreach (var item in objectListView.SelectedObjects)
      {
        (item as DetailLoadInfo).Quantity /= q.Qnt;
      }

      objectListView.RefreshObjects(objectListView.SelectedObjects);
    }

    public void UpdateInfos()
    {
      try
      {
        objectListView.SetObjects(Infos);
      }
      catch (Exception ex)
      {
        messageBoxService.ShowMessage(ex);
      }
    }

    private void objectListView1_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (objectListView.SelectedObject == null) return;
      Cursor.Current = Cursors.WaitCursor;
      preview = fileService.LoadRawDetail(new FileInfo((objectListView.SelectedObject as DetailLoadInfo).Path));
      if (autoFit) fitAll();
      Cursor.Current = Cursors.Default;
    }

    public void clearToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (Infos.Count == 0) { messageBoxService.ShowMessage("There are no parts.", MessageBoxIcon.Warning); return; }
      if (messageBoxService.ShowQuestion("Are you to sure to delete all items?") == DialogResult.No) return;
      Infos.Clear();
      objectListView.SetObjects(Infos);
      preview = null;
    }

    private void deleteToolStripMenuItem2_Click(object sender, EventArgs e)
    {
      if (objectListView.SelectedObjects.Count == 0) return;
      if (messageBoxService.ShowQuestion($"Are you to sure to delete {objectListView.SelectedObjects.Count} items?") == DialogResult.No) return;
      foreach (var item in objectListView.SelectedObjects)
      {
        if (preview != null && (item as DetailLoadInfo).Path == (preview as RawDetail).Name)
        {
          preview = null;
        }

        Infos.Remove(item as DetailLoadInfo);
      }

      objectListView.SetObjects(Infos);
    }

    public void RedrawPreview()
    {
      previewService.RedrawPreview(this.Ctx3, this.preview);
    }

    void fitAll()
    {
      if (preview == null) return;
      if (!(preview is RawDetail raw)) return;

      GraphicsPath gp = new GraphicsPath();
      foreach (var item in raw.Outers)
      {
        gp.AddPolygon(item.Points.ToArray());
      }

      if (raw.Outers.Count > 0)
      {
        Ctx3.FitToPoints(gp.PathPoints, 5);
      }
    }

    bool autoFit = true;

    private void toolStripButton9_CheckedChanged(object sender, EventArgs e)
    {
      autoFit = toolStripButton9.Checked;
      if (autoFit)
      {
        fitAll();
        toolStripButton9.BackColor = Color.LightGreen;
      }
      else
      {
        toolStripButton9.BackColor = Color.Transparent;
      }
    }

    private void toolStripButton8_Click(object sender, EventArgs e)
    {
      fitAll();
    }

    private void multiplierUpDown_ValueChanged(object sender, EventArgs e)
    {
      SvgNest.Config.Multiplier = (int)multiplierUpDown.Value;
    }
  }
}
