using System.Diagnostics;
using OpenTelemetry;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// OpenTelemetry processor that filters out Blazor Server SignalR component traces
/// to reduce noise in the Aspire traces dashboard.
/// </summary>
public class BlazorComponentTraceFilter : BaseProcessor<Activity>
{
    private const string ComponentHubOperationName = "Microsoft.AspNetCore.Components.Server.ComponentHub/OnRenderCompleted";

    public override void OnStart(Activity activity)
    {
        // Filter out Blazor Server ComponentHub traces
        if (activity.DisplayName == ComponentHubOperationName)
        {
            // Setting IsAllDataRequested to false prevents the activity from being recorded
            activity.IsAllDataRequested = false;
        }
    }
}
