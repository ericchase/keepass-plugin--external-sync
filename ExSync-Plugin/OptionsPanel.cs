using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ExSyncPlugin
{
  public partial class OptionsPanel : UserControl
  {
    public OptionsPanel()
    {
      InitializeComponent();
    }

    public delegate void EmptyEventHandler();
    public event EmptyEventHandler OnRestart;
    public event EmptyEventHandler OnStop;
    public event EmptyEventHandler OnClearLogs;

    public string Executable
    {
      get
      {
        return textBox_Executable.Text;
      }
      set
      {
        textBox_Executable.Text = value;
      }
    }

    public string Args
    {
      get
      {
        return textBox_Args.Text;
      }
      set
      {
        textBox_Args.Text = value;
      }
    }

    public string WorkingDirectory
    {
      get
      {
        return textBox_WorkingDirectory.Text;
      }
      set
      {
        textBox_WorkingDirectory.Text = value;
      }
    }

    public void InitLog(List<string> lines)
    {
      for (int index_plus1 = lines.Count; index_plus1 > 0; index_plus1--)
      {
        listView_Logs.Items.Add(lines[index_plus1 - 1]);
      }
    }

    public void AddLine(string line)
    {
      listView_Logs.Items.Insert(0, line);
    }

    public void Status(string status)
    {
      label_Status.Text = status;
    }

    private void Button_Restart_Click(object sender, EventArgs e)
    {
      OnRestart?.Invoke();
    }

    private void Button_Stop_Click(object sender, EventArgs e)
    {
      OnStop?.Invoke();
    }

    private void Button_ClearLogs_Click(object sender, EventArgs e)
    {
      OnClearLogs?.Invoke();
      listView_Logs.Items.Clear();
    }
  }
}
