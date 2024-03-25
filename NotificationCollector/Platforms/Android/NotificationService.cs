using Android.App;
using Android.Content;
using Android.Service.Notification;

namespace NotificationCollector
{
  [Service(Name = "NotificationCollector.Platforms.Android.NotificationService", Label = "NotificationService", Enabled = true, Exported = true, Permission = "android.permission.BIND_NOTIFICATION_LISTENER_SERVICE")]
  [IntentFilter(["android.service.notification.NotificationListenerService"])]
  public class NotificationService : NotificationListenerService
  {
    public override void OnNotificationPosted(StatusBarNotification? sbn)
    {
      base.OnNotificationPosted(sbn);

      BroadcastNotificationMessageIfPossible(sbn);
    }

    public override void OnNotificationRemoved(StatusBarNotification? sbn)
    {
      base.OnNotificationRemoved(sbn);

      BroadcastNotificationMessageIfPossible(sbn);
    }

    private void BroadcastNotificationMessageIfPossible(StatusBarNotification? statusBarNotification)
    {
      if (statusBarNotification == null || statusBarNotification.Notification == null)
      {
        return;
      }

      string? title = statusBarNotification.Notification.Extras.GetCharSequence("android.title");
      string? text = statusBarNotification.Notification.Extras.GetCharSequence("android.text");
      string key = statusBarNotification.Key + "|" + statusBarNotification.PostTime;

      if (!string.IsNullOrEmpty(statusBarNotification.Tag))
      {
        key += $"|{statusBarNotification.Tag}";
      }

      if (!string.IsNullOrEmpty(title))
      {
        key += $"|{title}";
      }

      if (!string.IsNullOrEmpty(text))
      {
        key += $"|{text}";
      }

      Intent message = new(NotificationConsts.Action);
      message.PutExtra(NotificationConsts.Key, key);
      message.PutExtra(NotificationConsts.Title, title);
      message.PutExtra(NotificationConsts.Text, text);
      message.PutExtra(NotificationConsts.Sender, statusBarNotification.PackageName);
      message.PutExtra(NotificationConsts.PostTime, statusBarNotification.PostTime);

      ApplicationContext.SendBroadcast(message);
    }
  }
}