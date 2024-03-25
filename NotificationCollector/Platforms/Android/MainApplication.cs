using Android.App;
using Android.Content;
using Android.Runtime;
using Microsoft.Extensions.Logging;
using NotificationCollector.Repository.Interface;
using NotificationCollector.Services.Interface;

namespace NotificationCollector
{
  [Application]
  public class MainApplication : MauiApplication
  {
    private static readonly object _applicationInitiaizationMutex = new object();

    private static MauiApp _application;

    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
      : base(handle, ownership)
    {
    }

    protected override MauiApp CreateMauiApp()
    {
      lock (_applicationInitiaizationMutex)
      {
        ILogger<MainApplication> log;

        if (_application != null)
        {
          log = _application.Services.GetRequiredService<ILogger<MainApplication>>();

          log.LogInformation("NotificationCollector has previously been initialized.");

          return _application;
        }

        _application = MauiProgram.CreateMauiApp(ApplicationContext);
        log = _application.Services.GetRequiredService<ILogger<MainApplication>>();

        try
        {
          log.LogInformation("NotificationCollector initialization started");

          INotificationRepository notificationRepository = _application.Services.GetRequiredService<INotificationRepository>();

          notificationRepository.InitializeDatabase();

          NotificationsBroadcastReceiver notificationsBroadcastReceiver = _application.Services.GetService<NotificationsBroadcastReceiver>();

          ApplicationContext.RegisterReceiver(notificationsBroadcastReceiver, new IntentFilter(NotificationConsts.Action));

          INotificationDispatchingService notificationDispatchingService = _application.Services.GetRequiredService<INotificationDispatchingService>();

          notificationDispatchingService.StartDispatchingNotifications();

          log.LogInformation("NotificationCollector initialized");

          return _application;
        }
        catch (Exception e)
        {
          log.LogError(e, "An error occured while initializing NotificationCollector");

          throw;
        }
      }
    }
  }
}