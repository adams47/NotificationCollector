using NotificationCollector.Domain;

namespace NotificationCollector.Clients.Interface
{
  public interface INotificationReceiverClient
  {
    Task Send(PostedNotification postedNotification);
  }
}