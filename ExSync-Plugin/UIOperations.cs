using KeePassLib;
using KeePassLib.Interfaces;
using System;

namespace ExSyncPlugin
{
  // For Synchronizing Files
  internal class UIOperations : IUIOperations
  {
    private readonly PwDatabase m_database;
    public UIOperations(PwDatabase database)
    {
      m_database = database ?? throw new ArgumentException("Database must not be null.", nameof(database));
    }
    public bool UIFileSave(bool bForceSave)
    {
      try
      {
        m_database.Save(null);
        return true;
      }
      catch (Exception) { }
      return false;
    }
  }
}
