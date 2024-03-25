using NotificationCollector.Domain;

namespace NotificationCollector.Repository.Interface
{
  public interface INotificationRepository
  {
    void InitializeDatabase();

    void AddIfNotExists(PostedNotification postedNotification);

    List<PostedNotification> GetPostedNotificationsToDispatch();

    void SetAsDispatched(long id1, long id2, long date);

    void DeleteOlderThan(long date);
  }
}