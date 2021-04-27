using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xero.NetStandard.OAuth2.Model.PayrollAu;
using Xero.NetStandard.OAuth2.Token;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;

namespace XeroNetStandardApp.Controllers
{
  public class PayrollController : Controller
  {
    private readonly ILogger<AuthorizationController> _logger;
    private readonly IOptions<XeroConfiguration> XeroConfig;
    private readonly IHttpClientFactory httpClientFactory;

    public PayrollController(IOptions<XeroConfiguration> XeroConfig, IHttpClientFactory httpClientFactory, ILogger<AuthorizationController> logger)
    {
      _logger = logger;
      this.XeroConfig = XeroConfig;
      this.httpClientFactory = httpClientFactory;
    }

    // GET: /PayrollController/
    public async Task<ActionResult> Index()
    {
      var xeroToken = TokenUtilities.GetStoredToken();
      var utcTimeNow = DateTime.UtcNow;

      if (utcTimeNow > xeroToken.ExpiresAtUtc)
      {
        var client = new XeroClient(XeroConfig.Value, httpClientFactory);
        xeroToken = (XeroOAuth2Token)await client.RefreshAccessTokenAsync(xeroToken);
        TokenUtilities.StoreToken(xeroToken);
      }

      string accessToken = xeroToken.AccessToken;
      string xeroTenantId = xeroToken.Tenants[0].TenantId.ToString();

      var PayrollAUApi = new PayrollAuApi();
      var response = await PayrollAUApi.GetTimesheetsAsync(accessToken, xeroTenantId);

      var timesheet = response._Timesheets;

      return View(timesheet);
    }

    // GET: /Contacts#Create
    [HttpGet]
    public IActionResult Create()
    {
      return View();
    }

   }
}