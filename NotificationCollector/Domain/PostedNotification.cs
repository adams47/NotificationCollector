namespace NotificationCollector.Domain
{
  public class PostedNotification
  {
    public long Id1 { get; set; }

    public long Id2 { get; set; }

    public long PostTime { get; set; }

    public string Title { get; set; }

    public string Text { get; set; }

    public string Sender { get; set; }
  }
}