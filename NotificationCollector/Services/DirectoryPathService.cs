using NotificationCollector.Services.Interface;

namespace NotificationCollector.Services
{
  public class DirectoryPathService : IDirectoryPathService
  {
    public string DocumentsDirectoryPath { get; set; }
  }
}