using System;
using System.Timers;

namespace ExSyncPlugin
{
  internal class ConnectionChecker
  {
    public bool _alive = true;
    public Timer _timer;
    public ConnectionChecker(int interval, Action<string> SendMessage, Action Exit)
    {
      Interval = interval;
      this.SendMessage = SendMessage;
      this.Exit = Exit;
      _ResetTimer();
    }
    public int Interval { get; set; }
    public Action<string> SendMessage { get; set; }
    public Action Exit { get; set; }
    public void CleanUp()
    {
      if (_timer != null)
      {
        _timer.Stop();
        _timer.Dispose();
      }
      _alive = false;
      _timer = null;
      SendMessage = (string _ignore) => { };
      Exit = () => { };
    }
    public void ProcessPing()
    {
      _alive = true;
      SendMessage("pong");
      _ResetTimer();
    }
    public void ProcessPong()
    {
      _alive = true;
    }
    public void _CheckConnection()
    {
      if (_alive == true)
      {
        _alive = false;
        SendMessage("ping");
        _ResetTimer();
      }
      else
      {
        Exit();
        CleanUp();
      }
    }
    public void _ResetTimer()
    {
      if (_timer != null)
      {
        _timer.Stop();
        _timer.Dispose();
      }
      _timer = new Timer(Interval);
      _timer.Elapsed += (sender, e) =>
      {
        _CheckConnection();
      };
      _timer.AutoReset = false;
      _timer.Start();
    }
  }
}
