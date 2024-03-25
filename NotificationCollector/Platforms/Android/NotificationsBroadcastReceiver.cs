using Android.Content;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NotificationCollector.Repository.Interface;
using System.Security.Cryptography;
using System.Text;

namespace NotificationCollector
{
  public class NotificationsBroadcastReceiver : BroadcastReceiver
  {
    private static readonly char[] _separators = [',', ';', '|'];

    private readonly ILogger<NotificationsBroadcastReceiver> _log;

    private readonly INotificationRepository _notificationRepository;

    private readonly HashAlgorithm _hashAlgorithm;

    private readonly HashSet<string> _acceptedSenders;

    public NotificationsBroadcastReceiver(HashAlgorithm hashAlgorithm, INotificationRepository notificationRepository, IConfiguration configuration, ILogger<NotificationsBroadcastReceiver> log)
    {
      _log = log;
      _notificationRepository = notificationRepository;
      _hashAlgorithm = hashAlgorithm;

      IEnumerable<string> acceptedSendersCollection = [];

      if (!string.IsNullOrEmpty(configuration["AcceptedSenders"]))
      {
        acceptedSendersCollection = configuration["AcceptedSenders"].Split(_separators, StringSplitOptions.RemoveEmptyEntries).Select(x => x.ToLower());
      }

      _acceptedSenders = new HashSet<string>(acceptedSendersCollection);
    }

    public override void OnReceive(Context? context, Intent? intent)
    {
      try
      {
        if (intent == null)
        {
          return;
        }

        string sender = intent.GetStringExtra(NotificationConsts.Sender).ToLower();

        if (_acceptedSenders.Count > 0 && !_acceptedSenders.Contains(sender))
        {
          return;
        }

        string key = intent.GetStringExtra(NotificationConsts.Key);
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] hashBytes = _hashAlgorithm.ComputeHash(keyBytes);

        Domain.PostedNotification postedNotification = new();
        postedNotification.Id1 = BitConverter.ToInt64(hashBytes, 0);
        postedNotification.Id2 = BitConverter.ToInt64(hashBytes, 8);
        postedNotification.PostTime = intent.GetLongExtra(NotificationConsts.PostTime, 0);
        postedNotification.Title = intent.GetStringExtra(NotificationConsts.Title);
        postedNotification.Text = intent.GetStringExtra(NotificationConsts.Text);
        postedNotification.Sender = sender;

        _notificationRepository.AddIfNotExists(postedNotification);
      }
      catch (Exception e)
      {
        _log.LogError(e, $"An error occured while saving notification to database");
      }
    }
  }
}