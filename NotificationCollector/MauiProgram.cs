using Android.App;
using Android.Content;
using Android.OS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using NotificationCollector.Clients;
using NotificationCollector.Clients.Interface;
using NotificationCollector.Repository;
using NotificationCollector.Repository.Interface;
using NotificationCollector.Services;
using NotificationCollector.Services.Interface;
using RestSharp;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using System.Reflection;
using System.Security.Cryptography;

namespace NotificationCollector
{
  public static class MauiProgram
  {
    private static MauiApp _app;

    public static MauiApp CreateMauiApp(Context context)
    {
      MauiAppBuilder builder = MauiApp.CreateBuilder();

      builder
        .UseMauiApp<App>()
        .ConfigureFonts(fonts =>
        {
          fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
          fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        })
        .ConfigureLifecycleEvents(x =>
        {
          x.AddAndroid(y => y.OnPostCreate(OnPostCreate));
          x.AddAndroid(y => y.OnDestroy(OnDestroy));
        });

      Assembly assembly = Assembly.GetExecutingAssembly();

      using (Stream stream = assembly.GetManifestResourceStream("NotificationCollector.appsettings.json"))
      {
        IConfigurationRoot configurationRoot = new ConfigurationBuilder()
              .AddJsonStream(stream)
              .Build();

        builder.Configuration.AddConfiguration(configurationRoot);
      }

      string logsConnectionString = builder.Configuration.GetConnectionString("Logs");
      string machineName = $"{DeviceInfo.Current.Manufacturer} {DeviceInfo.Current.Model} {DeviceInfo.Current.Version}";
      Java.IO.File? documentsDirectory = context.GetExternalFilesDir(Android.OS.Environment.DirectoryDocuments);
      string logFilePath = Path.Combine(documentsDirectory.AbsolutePath, "logs.txt");

      Log.Logger = new LoggerConfiguration()
        .Enrich.WithProperty("ApplicationName", "NotificationCollector")
        .Enrich.WithProperty("MachineName", machineName)
        .WriteTo.MSSqlServer(connectionString: logsConnectionString, sinkOptions: new MSSqlServerSinkOptions { TableName = "LogEvents" })
        .WriteTo.File(logFilePath, flushToDiskInterval: TimeSpan.FromSeconds(5), rollingInterval: RollingInterval.Day, retainedFileCountLimit: 3)
        .CreateLogger();

      builder.Services.AddLogging(x => x.AddSerilog(Log.Logger));
      builder.Services.AddSingleton<HashAlgorithm>(MD5.Create());
      builder.Services.AddSingleton(CreateNotificationReceiverRestClient);
      builder.Services.AddSingleton<IDirectoryPathService>(new DirectoryPathService { DocumentsDirectoryPath = documentsDirectory.AbsolutePath });
      builder.Services.AddSingleton<INotificationRepository, NotificationRepository>();
      builder.Services.AddSingleton<NotificationsBroadcastReceiver, NotificationsBroadcastReceiver>();
      builder.Services.AddSingleton<INotificationReceiverClient, NotificationReceiverClient>();
      builder.Services.AddSingleton<INotificationDispatchingService, NotificationDispatchingService>();

      _app = builder.Build();

      return _app;
    }

    private static void OnPostCreate(Activity activity, Bundle? savedInstanceState)
    {
      ILogger<MainApplication> log = _app.Services.GetRequiredService<ILogger<MainApplication>>();

      log.LogInformation("NotificationCollector started");
    }

    private static void OnDestroy(Activity activity)
    {
      ILogger<MainApplication> log = _app.Services.GetRequiredService<ILogger<MainApplication>>();
      INotificationDispatchingService notificationDispatchingService = _app.Services.GetRequiredService<INotificationDispatchingService>();

      try
      {
        notificationDispatchingService.StopDispatchingNotifications();
      }
      catch (Exception)
      {
      }

      log.LogInformation("NotificationCollector stopped");
    }

    private static RestClient CreateNotificationReceiverRestClient(IServiceProvider serviceProvider)
    {
      IConfiguration configuration = serviceProvider.GetRequiredService<IConfiguration>();

      RestClient restClient = new(configuration["NotificationReceiverApiURL"]);

      return restClient;
    }
  }
}