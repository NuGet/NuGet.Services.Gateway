using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Web.Administration;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace NuGet.Services.Gateway
{
    public class WebRole : RoleEntryPoint
    {
        public override bool OnStart()
        {
            try
            {
                Trace.Listeners.Add(new DiagnosticMonitorTraceListener());
                Trace.TraceInformation("Starting Gateway");

                // Enable IIS Reverse Proxy
                using (ServerManager server = new ServerManager())
                {
                    var proxySection = server.GetApplicationHostConfiguration().GetSection("system.webServer/proxy");
                    proxySection.SetAttributeValue("enabled", true);
                    server.CommitChanges();
                    Trace.TraceInformation("Enabled Reverse Proxy");
                }

                return base.OnStart();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error starting gateway: {0}", ex.ToString());
                throw;
            }
        }
    }
}