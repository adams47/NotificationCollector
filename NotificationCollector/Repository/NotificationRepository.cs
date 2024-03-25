using NotificationCollector.Domain;
using NotificationCollector.Repository.Interface;
using NotificationCollector.Services.Interface;
using SQLite;

namespace NotificationCollector.Repository
{
  public class NotificationRepository(IDirectoryPathService directoryPathService) : INotificationRepository
  {
    private readonly string _connectionString = Path.Combine(directoryPathService.DocumentsDirectoryPath, "Notifications.db3");

    public void InitializeDatabase()
    {
      using SQLiteConnection connection = new(_connectionString);

      connection.Execute("CREATE TABLE IF NOT EXISTS Notification ([id1] INTEGER NOT NULL, [id2] INTEGER NOT NULL, [postTime] INTEGER NOT NULL, [title] TEXT NULL, [text] TEXT NULL, [sender] TEXT NOT NULL, [dispatchDate] INTEGER NOT NULL, PRIMARY KEY (id1, id2)) WITHOUT ROWID;");
      connection.Execute("CREATE INDEX IF NOT EXISTS IDX_Notification_dispatchDate ON Notification(dispatchDate)");
    }

    public void AddIfNotExists(PostedNotification postedNotification)
    {
      using SQLiteConnection connection = new(_connectionString);

      connection.BeginTransaction();

      int? existResult = connection.ExecuteScalar<int?>("SELECT 1 FROM Notification WHERE [id1] = @id1 AND [id2] = @id2", postedNotification.Id1, postedNotification.Id2);

      if (existResult.HasValue)
      {
        return;
      }

      connection.Execute("INSERT INTO Notification ([id1], [id2], [postTime], [title], [text], [sender], [dispatchDate]) VALUES (@id1, @id2, @postTime, @title, @text, @sender, @dispatchDate)", postedNotification.Id1, postedNotification.Id2, postedNotification.PostTime, postedNotification.Title, postedNotification.Text, postedNotification.Sender, 0);

      connection.Commit();
    }

    public List<PostedNotification> GetPostedNotificationsToDispatch()
    {
      using SQLiteConnection connection = new(_connectionString);

      return connection.Query<PostedNotification>("SELECT [id1], [id2], [postTime], [title], [text], [sender] FROM Notification WHERE [dispatchDate] = 0");
    }

    public void SetAsDispatched(long id1, long id2, long date)
    {
      using SQLiteConnection connection = new(_connectionString);

      connection.Execute("UPDATE Notification SET dispatchDate=@date WHERE id1=@id1 AND id2=@id2", date, id1, id2);
    }

    public void DeleteOlderThan(long date)
    {
      using SQLiteConnection connection = new(_connectionString);

      connection.Execute("DELETE FROM Notification WHERE dispatchDate>0 AND dispatchDate<@date", date);
    }
  }
}