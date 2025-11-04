namespace Rooms.Infrastructure.Bus.Services;

/// <summary>
/// Реализация <see cref="IInstanceName"/>, которая берёт имя инстанса
/// из <see cref="Environment.MachineName"/>.
/// </summary>
public class KubernetesInstanceName : IInstanceName
{
    /// <summary>
    /// Имя инстанса, совпадающее с именем текущей машины/контейнера.
    /// </summary>
    public string Name => Environment.GetEnvironmentVariable("INSTANCE_NAME") ?? throw new Exception("Kubernetes instance name not set");
}