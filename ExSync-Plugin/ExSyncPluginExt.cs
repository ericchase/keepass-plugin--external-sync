using KeePass.App.Configuration;
using KeePass.DataExchange;
using KeePass.Forms;
using KeePass.Plugins;
using KeePass.UI;
using KeePassLib.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

// The namespace name must be the same as the file name of the plugin without its extension.
// For example, if you compile a plugin 'ExSyncPlugin.dll', the namespace must be named 'ExSyncPlugin'.
namespace ExSyncPlugin
{
  // Namespace name 'ExSyncPlugin' + 'Ext' = 'ExSyncPluginExt'
  public sealed class ExSyncPluginExt : Plugin
  {
    // Custom Config Keys
    private const string CFG_EXECUTABLE = "ExSyncPlugin.Executable";
    private const string CFG_ARGS = "ExSyncPlugin.Args";
    private const string CFG_WORKING_DIRECTORY = "ExSyncPlugin.WorkingDirectory";

    private AceCustomConfig m_customConfig;
    private IPluginHost m_host = null;
    private OptionsPanel m_optionsPanel = null;
    private Subprocess m_subprocess = null;

    private System.Windows.Forms.Form m_syncModal = null;
    private System.Windows.Forms.Label m_syncLabel = null;
    private bool m_syncing = false;
    private readonly List<string> m_logs = new List<string>();

    public override bool Initialize(IPluginHost host)
    {
      if (host == null)
      {
        return false;
      }

      m_host = host;
      m_customConfig = m_host.CustomConfig;

      // Add Event Listeners
      m_host.MainWindow.FileClosed += OnFileClosed;
      m_host.MainWindow.FileOpened += OnFileOpened;
      m_host.MainWindow.FileSaved += OnFileSaved;
      m_host.MainWindow.Shown += OnMainWindowShown;
      GlobalWindowManager.WindowAdded += OnWindowAdded;

      return true;
    }

    public override void Terminate()
    {
      // Remove Event Listeners
      GlobalWindowManager.WindowAdded -= OnWindowAdded;
      m_host.MainWindow.Shown -= OnMainWindowShown;
      m_host.MainWindow.FileSaved -= OnFileSaved;
      m_host.MainWindow.FileOpened -= OnFileOpened;
      m_host.MainWindow.FileClosed -= OnFileClosed;

      StopSubprocess();
    }

    public void AddLog(string message)
    {
      m_logs.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
      m_optionsPanel?.AddLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
    }

    private void OnMainWindowShown(object sender, EventArgs e)
    {
      // Get the main window's toolstrip
      if (m_host.MainWindow.Controls.Find("m_toolMain", true).FirstOrDefault() is ToolStrip m_toolMain)
      {
        System.Windows.Forms.ToolStripButton button = new ToolStripButton
        {
          Text = "Trigger Ex Sync"
        };
        button.Click += new EventHandler(OnToolStripButtonClick);
        m_toolMain.Items.Add(button);
      }
    }

    private void OnToolStripButtonClick(object sender, EventArgs e)
    {
      if (m_subprocess != null)
      {
        m_syncing = true;
        m_host.MainWindow.Invoke(new Action(() =>
        {
          m_host.MainWindow.SaveDatabase(null, EventArgs.Empty);
          ShowModalPopup();
        }));
      }
      else
      {
        MessageService.ShowInfo("Please start subprocess via Options menu first.");
      }
    }

    private void OnFileClosed(object sender, FileClosedEventArgs e)
    {
      StopSubprocess();
    }

    private void OnFileOpened(object sender, FileOpenedEventArgs e)
    {
      RestartSubprocess();
    }

    private void OnFileSaved(object sender, FileSavedEventArgs e)
    {
      if (m_syncing)
      {
        ChangeModalText("Pulling Remote Database");
        m_subprocess?.SendMessage("pull");
      }
    }

    private void OnWindowAdded(object sender, GwmWindowEventArgs e)
    {
      try
      {
        if (e.Form is OptionsForm optionsForm)
        {
          if (optionsForm.Name == "OptionsForm" && optionsForm.Visible == true)
          {
            // Enable Resizing
            if (optionsForm.Controls.Find("m_bannerImage", true).FirstOrDefault() is System.Windows.Forms.PictureBox m_bannerImage)
            {
              m_bannerImage.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            }
            if (optionsForm.Controls.Find("m_tbSearch", true).FirstOrDefault() is KeePass.UI.UIElementSearchBoxEx m_tbSearch)
            {
              m_tbSearch.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            }
            if (optionsForm.Controls.Find("m_btnOK", true).FirstOrDefault() is System.Windows.Forms.Button m_btnOK)
            {
              m_btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            }
            if (optionsForm.Controls.Find("m_btnCancel", true).FirstOrDefault() is System.Windows.Forms.Button m_btnCancel)
            {
              m_btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            }

            if (optionsForm.Controls.Find("m_tabMain", true).FirstOrDefault() is TabControl m_tabMain)
            {
              m_tabMain.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

              // Add Options Tab
              TabPage optionsTab = new TabPage("Ex Sync")
              {
                UseVisualStyleBackColor = true,
              };

              m_optionsPanel = new OptionsPanel
              {
                Dock = DockStyle.Fill
              };

              optionsTab.Controls.Add(m_optionsPanel);
              m_tabMain.TabPages.Add(optionsTab);
              optionsForm.FormClosing += OnFormClosing;

              m_optionsPanel.InitLog(m_logs);
              m_optionsPanel.Status(m_subprocess != null ? "Started" : "Stopped");
              m_optionsPanel.OnRestart += RestartSubprocess;
              m_optionsPanel.OnStop += StopSubprocess;
              m_optionsPanel.OnClearLogs += m_logs.Clear;

              AddLog("@plugin Options Opened");
              LoadSettings();
            }

            // Resize and Position the Form
            optionsForm.Size = new System.Drawing.Size(700, 600);
            optionsForm.FormBorderStyle = FormBorderStyle.Sizable;
            Screen screen = Screen.FromControl(optionsForm);
            optionsForm.Location = new Point(
              screen.WorkingArea.X + (screen.WorkingArea.Width - optionsForm.Width) / 2,
              screen.WorkingArea.Y + (screen.WorkingArea.Height - optionsForm.Height) / 2
            );
          }
        }
      }
      catch (Exception) { }
    }

    private void OnFormClosing(object sender, FormClosingEventArgs e)
    {
      if (sender is OptionsForm optionsForm)
      {
        optionsForm.FormClosing -= OnFormClosing;

        m_optionsPanel.OnClearLogs -= m_logs.Clear;
        m_optionsPanel.OnStop -= StopSubprocess;
        m_optionsPanel.OnRestart -= RestartSubprocess;

        SaveSettings();
      }
    }

    private void LoadSettings()
    {
      try
      {
        if (m_optionsPanel != null)
        {
          m_optionsPanel.Executable = m_customConfig.GetString(CFG_EXECUTABLE);
          m_optionsPanel.Args = m_customConfig.GetString(CFG_ARGS);
          m_optionsPanel.WorkingDirectory = m_customConfig.GetString(CFG_WORKING_DIRECTORY);
        }
      }
      catch (Exception ex)
      {
        MessageService.ShowInfo("Error: LoadSettings:", ex);
      }
    }

    private void SaveSettings()
    {
      try
      {
        if (m_optionsPanel != null)
        {
          m_customConfig.SetString(CFG_EXECUTABLE, m_optionsPanel.Executable);
          m_customConfig.SetString(CFG_ARGS, m_optionsPanel.Args);
          m_customConfig.SetString(CFG_WORKING_DIRECTORY, m_optionsPanel.WorkingDirectory);
        }
      }
      catch (Exception ex)
      {
        MessageService.ShowInfo("Error: SaveSettings:", ex);
      }
    }

    private void ReloadDatabase()
    {
      if (m_syncing)
      {
        m_syncing = false;
        m_host.MainWindow.Invoke(new Action(() =>
        {
          ChangeModalText("Reloading Database");
          m_host.MainWindow.OpenDatabase(m_host.Database.IOConnectionInfo, null, false);
          m_syncModal?.Close();
        }));
      }
    }

    private void SyncDatabase()
    {
      m_host.MainWindow.Invoke(new Action(() =>
      {
        try
        {
          ChangeModalText("Synchronizing Database");
          if (ImportUtil.Synchronize(m_host.Database, new UIOperations(m_host.Database), m_host.Database.IOConnectionInfo, true, null) == true)
          {
            AddLog("Synchronized database.");
            ChangeModalText("Pushing Database");
            m_subprocess?.SendMessage("push");
          }
          else
          {
            AddLog("Failed to synchronize with database file.");
          }
        }
        catch (Exception ex)
        {
          MessageService.ShowInfo("Error: Could not send sync message to subprocess:", ex);
        }
      }));
    }

    private void ShowModalPopup()
    {
      m_syncModal = new System.Windows.Forms.Form
      {
        Text = "Synchronizing",
        Size = new Size(300, 200),
        StartPosition = FormStartPosition.CenterParent
      };
      m_syncLabel = new System.Windows.Forms.Label
      {
        Text = "Please wait...",
        Dock = DockStyle.Fill,
        TextAlign = ContentAlignment.MiddleCenter
      };
      m_syncModal.Controls.Add(m_syncLabel);
      m_syncModal.ShowDialog(m_host.MainWindow);
    }
    private void ChangeModalText(string text)
    {
      if (m_syncLabel != null)
      {
        m_syncLabel.Text = text;
      }
    }

    private void SubprocessExited(Subprocess sender)
    {
      if (sender == m_subprocess)
      {
        m_optionsPanel?.Status("Stopped");
        AddLog("External Process Stopped");
        UnsubscribeFromSubprocessEvents();
        m_subprocess = null;
      }
    }
    private void SubprocessStarted(Subprocess sender)
    {
      if (sender == m_subprocess)
      {
        m_optionsPanel?.Status("Started");
        AddLog("External Process Started");
      }
    }
    private void SubprocessInternalError(Subprocess sender, Exception ex, string cause)
    {
      if (sender == m_subprocess)
      {
        AddLog($"@error @plugin @{cause} {ex}");
      }
    }
    private void SubprocessErrorDataReceived(Subprocess sender, string message)
    {
      if (sender == m_subprocess)
      {
        AddLog($"@error {message}");
      }
    }
    private void SubprocessOutputDataReceived(Subprocess sender, string message)
    {
      if (sender == m_subprocess)
      {
        switch (message.Trim())
        {
          case "pulled":
            AddLog("Database pulled.");
            SyncDatabase();
            break;
          case "pushed":
            AddLog("Database pushed.");
            ReloadDatabase();
            break;
          case "notify":
            AddLog("Remote file changed.");
            break;
          default:
            AddLog(message);
            break;
        }
      }
    }

    private void SubscribeToSubprocessEvents()
    {
      if (m_subprocess != null)
      {
        m_subprocess.OnExited += SubprocessExited;
        m_subprocess.OnStarted += SubprocessStarted;
        m_subprocess.OnInternalError += SubprocessInternalError;
        m_subprocess.OnErrorDataReceived += SubprocessErrorDataReceived;
        m_subprocess.OnOutputDataReceived += SubprocessOutputDataReceived;
      }
    }
    private void UnsubscribeFromSubprocessEvents()
    {
      if (m_subprocess != null)
      {
        m_subprocess.OnExited -= SubprocessExited;
        m_subprocess.OnStarted -= SubprocessStarted;
        m_subprocess.OnInternalError -= SubprocessInternalError;
        m_subprocess.OnErrorDataReceived -= SubprocessErrorDataReceived;
        m_subprocess.OnOutputDataReceived -= SubprocessOutputDataReceived;
      }
    }

    private void StartSubprocess()
    {
      try
      {
        SaveSettings();
        string executable = m_customConfig.GetString(CFG_EXECUTABLE);
        string args = $"{m_customConfig.GetString(CFG_ARGS)} \"{m_host.Database.IOConnectionInfo.Path}\"";
        string working_directory = m_customConfig.GetString(CFG_WORKING_DIRECTORY);
        if (executable != "")
        {
          m_subprocess = new Subprocess(executable, args, working_directory);
          SubscribeToSubprocessEvents();
          m_subprocess.Start();
        }
      }
      catch (Exception ex)
      {
        MessageService.ShowInfo("Error: StartSubprocess:", ex);
      }
    }
    private void StopSubprocess()
    {
      m_subprocess?.Stop();
    }
    private void RestartSubprocess()
    {
      if (m_subprocess != null)
      {
        m_subprocess.OnExited += (Subprocess sender) =>
        {
          StartSubprocess();
        };
        m_subprocess?.Stop();
      }
      else
      {
        StartSubprocess();
      }
    }
  }
}
