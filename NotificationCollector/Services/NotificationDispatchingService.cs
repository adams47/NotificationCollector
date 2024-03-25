using Microsoft.Extensions.Logging;
using NotificationCollector.Clients.Interface;
using NotificationCollector.Domain;
using NotificationCollector.Repository.Interface;
using NotificationCollector.Services.Interface;

namespace NotificationCollector.Services
{
  public class NotificationDispatchingService(INotificationRepository notificationRepository, INotificationReceiverClient notificationReceiverClient, ILogger<NotificationDispatchingService> log) : INotificationDispatchingService
  {
    private const int NotificationRemovalThresholdInDays = 30;

    private readonly DateTime _epochStart = new(1970, 1, 1);

    private readonly ILogger<NotificationDispatchingService> _log = log;

    private readonly INotificationRepository _notificationRepository = notificationRepository;

    private readonly INotificationReceiverClient _notificationReceiverClient = notificationReceiverClient;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public void StartDispatchingNotifications()
    {
      Task.Run(StartDispatchingNotificationsTask, _cancellationTokenSource.Token);
    }

    public void StopDispatchingNotifications()
    {
      _cancellationTokenSource.Cancel();
    }

    public void Dispose()
    {
      StopDispatchingNotifications();
    }

    private async Task StartDispatchingNotificationsTask()
    {
      while (!_cancellationTokenSource.IsCancellationRequested)
      {
        try
        {
          List<PostedNotification> postedNotificationsToDispatch = _notificationRepository.GetPostedNotificationsToDispatch();

          foreach (PostedNotification postedNotification in postedNotificationsToDispatch)
          {
            try
            {
              await _notificationReceiverClient.Send(postedNotification);

              TimeSpan timeFromEpochStart = DateTime.Now - _epochStart;

              _notificationRepository.SetAsDispatched(postedNotification.Id1, postedNotification.Id2, (long)timeFromEpochStart.TotalMilliseconds);
            }
            catch (Exception e)
            {
              _log.LogError(e, $"An error occured while dispatching notification. Id1: {postedNotification.Id1}, Id2: {postedNotification.Id2}");
            }
          }

          DateTime thresholdDate = DateTime.Now.AddDays(-NotificationRemovalThresholdInDays);
          TimeSpan timeFromEpochStartTillThresholdDate = thresholdDate - _epochStart;

          _notificationRepository.DeleteOlderThan((long)timeFromEpochStartTillThresholdDate.TotalMilliseconds);
        }
        catch (Exception e)
        {
          _log.LogError(e, $"An error occured while retrieving notifications to dispatch");
        }
        finally
        {
          await Task.Delay(5000);
        }
      }
    }
  }
}