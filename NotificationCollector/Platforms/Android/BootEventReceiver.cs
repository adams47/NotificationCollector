using Android.App;
using Android.Content;

namespace NotificationCollector
{
  [BroadcastReceiver(Name = "NotificationCollector.BootEventReceiver", Label = "BootEventReceiver", Enabled = true, Exported = true, DirectBootAware = true, Permission = "android.permission.RECEIVE_BOOT_COMPLETED")]
  [IntentFilter([Intent.ActionBootCompleted], Categories = [Intent.CategoryDefault])]
  public class BootEventReceiver : BroadcastReceiver
  {
    public override void OnReceive(Context? context, Intent? intent)
    {
      Intent startMainActivityIntent = new(context, typeof(MainActivity));
      startMainActivityIntent.AddFlags(ActivityFlags.ClearTask | ActivityFlags.NewTask);

      context.StartActivity(startMainActivityIntent);
    }
  }
}