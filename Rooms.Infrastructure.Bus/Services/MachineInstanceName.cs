namespace Rooms.Infrastructure.Bus.Services;

/// <summary>
/// Реализация <see cref="IInstanceName"/>, которая берёт имя инстанса
/// из <see cref="Environment.MachineName"/>.
/// </summary>
public class MachineInstanceName : IInstanceName
{
    /// <summary>
    /// Имя инстанса, совпадающее с именем текущей машины/контейнера.
    /// </summary>
    public string Name => Environment.MachineName;
}