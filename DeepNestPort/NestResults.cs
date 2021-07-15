using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeepNestSharp
{
  public partial class NestResults : UserControl
  {
    private MessageBoxService messageBoxService = MessageBoxService.Default;
    private readonly Form1 form;

    public NestResults()
    {
      InitializeComponent();
      this.form = form;
    }

    public void UpdateNestsList()
    {
      try
      {
        if (IsHandleCreated)
        {
          if (InvokeRequired)
          {
            _ = this.Invoke((MethodInvoker)UpdateNestsList);
          }
          else
          {
            if (form.Context.Nest != null)
            {
              listViewTopNests.Invoke((Action)(() =>
              {
                listViewTopNests.BeginUpdate();
                int selectedIndex = listViewTopNests.FocusedItem?.Index ?? 0;
                listViewTopNests.Items.Clear();
                int i = 0;
                if (form.Context?.Nest != null)
                {
                  foreach (var item in form.Context.Nest.TopNestResults)
                  {
                    var listItem = new ListViewItem(new string[] { item.Fitness.ToString("N0"), item.CreatedAt.ToString("HH:mm:ss.fff") }) { Tag = item };
                    listViewTopNests.Items.Add(listItem);
                    if (i == selectedIndex)
                    {
                      listItem.Selected = true;
                      listItem.Focused = true;
                    }

                    i++;
                  }
                }

                listViewTopNests.EndUpdate();
              }));
            }
          }
        }
      }
      catch (InvalidOperationException)
      {
        //NOP
      }
      catch (InvalidAsynchronousStateException)
      {
        //NOP
      }
      catch (Exception ex)
      {
        messageBoxService.ShowMessage(ex);
      }
    }

    public void ContextualiseRunStopButtons(bool isRunning)
    {
      if (IsHandleCreated)
      {
        if (InvokeRequired)
        {
          _ = this.Invoke((MethodInvoker)(() => ContextualiseRunStopButtons(isRunning)));
        }
        else
        {
          runButton.Enabled = !isRunning;
          this.stopButton.Enabled = isRunning;
          Application.DoEvents();
        }
      }
    }

  }
}
