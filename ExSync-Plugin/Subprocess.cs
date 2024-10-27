using KeePassLib.Utility;
using System;
using System.Diagnostics;

namespace ExSyncPlugin
{
  internal class Subprocess
  {
    private Process m_process = null;
    private ConnectionChecker m_connection = null;

    public Subprocess(string executable, string args, string working_directory)
    {
      try
      {
        m_process = new Process
        {
          EnableRaisingEvents = true,
          StartInfo = new ProcessStartInfo(executable, args)
          {
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            WindowStyle = ProcessWindowStyle.Hidden,
            WorkingDirectory = working_directory,
          }
        };
        m_process.OutputDataReceived += HandleOutputDataReceived;
        m_process.ErrorDataReceived += HandleErrorDataReceived;
        m_process.Exited += HandleExited;
      }
      catch (Exception)
      {
        m_process = null;
      }
    }

    public delegate void EmptyEventHandler(Subprocess sender);
    public delegate void ExceptionHandler(Subprocess sender, Exception ex, string cause);
    public delegate void StringHandler(Subprocess sender, string message);

    public event EmptyEventHandler OnExited;
    public event EmptyEventHandler OnStarted;
    public event ExceptionHandler OnInternalError;
    public event StringHandler OnErrorDataReceived;
    public event StringHandler OnOutputDataReceived;

    public void HandleExited(object sender, EventArgs e)
    {
      OnExited.Invoke(this);
      m_connection?.CleanUp();
      m_connection = null;
      m_process = null;
    }

    public void HandleErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
      try
      {
        if (string.IsNullOrEmpty(e.Data))
        {
          return;
        }
        OnErrorDataReceived?.Invoke(this, e.Data);
      }
      catch (Exception ex)
      {
        OnInternalError?.Invoke(this, ex, "ErrorDataReceived");
      }
    }

    public void HandleOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
      try
      {
        if (string.IsNullOrEmpty(e.Data))
        {
          return;
        }
        switch (e.Data.Trim())
        {
          case "ping":
            m_connection.ProcessPing();
            break;
          case "pong":
            m_connection.ProcessPong();
            break;
          default:
            OnOutputDataReceived?.Invoke(this, e.Data);
            break;
        }
      }
      catch (Exception ex)
      {
        OnInternalError?.Invoke(this, ex, "OutputDataReceived");
      }
    }

    public void SendMessage(string message)
    {
      try
      {
        m_process?.StandardInput.WriteLine(message);
      }
      catch (Exception) { }
    }

    public void Start()
    {
      m_process.Start();
      m_process.BeginOutputReadLine();
      m_process.BeginErrorReadLine();
      m_connection = new ConnectionChecker(60000, SendMessage, () =>
      {
        OnErrorDataReceived?.Invoke(this, "No communication since last timeout.");
        Stop();
      });
      OnStarted.Invoke(this);
    }

    public void Stop()
    {
      SendMessage("exit");
      try
      {
        m_connection?.CleanUp();
        m_connection = null;
        m_process?.WaitForExit(5000);
        m_process?.Kill();
      }
      catch (Exception) { }
    }
  }
}
