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

                    // Patch web.config rewrite rules
                    string serviceStem = RoleEnvironment.GetConfigurationSettingValue("Gateway.ServiceStem");
                    if (!String.Equals(serviceStem, "nuget.org", StringComparison.OrdinalIgnoreCase))
                    {
                        RewriteStem(server, serviceStem);
                    }
                    server.CommitChanges();
                }

                return base.OnStart();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error starting gateway: {0}", ex.ToString());
                throw;
            }
        }

        private void RewriteStem(ServerManager server, string serviceStem)
        {
            // Get the site config
            const string siteBaseName = "Web"; // From the CSDEF file
            string siteName =
                RoleEnvironment.CurrentRoleInstance.Id + "_" + siteBaseName;
            var config = server.Sites[siteName].GetWebConfiguration();

            var rewriteSection = config.GetSection("system.webServer/rewrite");
            if (rewriteSection == null)
            {
                var rewriteRules = rewriteSection.GetChildElement("rules");
                if (rewriteRules != null)
                {
                    foreach (var rewriteRule in rewriteRules.ChildElements)
                    {
                        var action = rewriteRules.GetChildElement("action");
                        if (action != null)
                        {
                            string url = action.GetAttributeValue("url");
                            string rewritten = url.Replace("nuget.org", serviceStem);
                            if (!String.Equals(url, rewritten, StringComparison.Ordinal))
                            {
                                action.SetAttributeValue("url", rewritten);
                            }
                        }
                    }
                }
            }
        }
    }
}