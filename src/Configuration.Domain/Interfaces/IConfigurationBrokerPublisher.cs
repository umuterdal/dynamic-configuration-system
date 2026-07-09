namespace Configuration.Domain.Interfaces;

/// <summary>
/// Publishes configuration change events to a message broker.
/// Used to notify consumers of configuration updates for instant cache refresh.
/// Polling continues as the primary mechanism; broker improves latency.
/// </summary>
public interface IConfigurationBrokerPublisher
{
    /// <summary>
    /// Publishes a configuration change event.
    /// </summary>
    /// <param name="applicationName">The application affected by the change.</param>
    /// <param name="changeType">Type of change: Created, Updated, Deleted.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PublishAsync(string applicationName, string changeType, CancellationToken cancellationToken = default);
}
