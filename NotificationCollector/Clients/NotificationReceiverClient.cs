using Microsoft.Extensions.Configuration;
using NotificationCollector.Clients.Interface;
using NotificationCollector.Domain;
using RestSharp;
using System.Net;

namespace NotificationCollector.Clients
{
  public class NotificationReceiverClient(RestClient restClient, IConfiguration configuration) : INotificationReceiverClient
  {
    private const string TokenAuthorizationScheme = "Token";

    private const string AuthorizationHeaderName = "Authorization";

    private readonly RestClient _restClient = restClient;

    private readonly IConfiguration _configuration = configuration;

    public async Task Send(PostedNotification postedNotification)
    {
      string resource = _configuration["NotificationReceiverApiResource"];
      string token = _configuration["NotificationReceiverAuthorizationToken"];

      RestRequest restRequest = new(resource, Method.Post);

      restRequest.AddHeader(AuthorizationHeaderName, $"{TokenAuthorizationScheme} {token}");
      restRequest.AddJsonBody(postedNotification);

      RestResponse response = await _restClient.ExecuteAsync(restRequest);

      if (response.StatusCode != HttpStatusCode.OK)
      {
        if (response.ErrorException != null)
        {
          throw response.ErrorException;
        }

        throw new Exception($"Status code: {response.StatusCode}, Content: {response.Content}");
      }
    }
  }
}