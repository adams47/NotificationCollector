namespace NotificationCollector.Services.Interface
{
  public interface INotificationDispatchingService : IDisposable
  {
    void StartDispatchingNotifications();

    void StopDispatchingNotifications();
  }
}