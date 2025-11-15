using System.Diagnostics.Metrics;

namespace Common.Infrastructure.Repositories.Metrics;

/// <summary>
/// Метрики для репозиториев и операций UnitOfWork
/// </summary>
public static class RepositoryMetrics
{
    /// <summary>
    /// Имя метрики
    /// </summary>
    public const string MeterName = "Common.Repositories";
    
    /// <summary>
    /// Meter для группировки метрик
    /// </summary>
    private static readonly Meter Meter = new(MeterName);

    /// <summary>
    /// Счётчик успешных транзакций
    /// </summary>
    internal static readonly Counter<long> TransactionsCommitted =
        Meter.CreateCounter<long>(
            "repository_transactions_committed",
            unit: "count",
            description: "Total number of successfully committed transactions");

    /// <summary>
    /// Гистограмма времени выполнения транзакций
    /// </summary>
    internal static readonly Histogram<double> TransactionDuration =
        Meter.CreateHistogram<double>(
            "repository_transactions_duration",
            unit: "ms",
            description: "Duration of transactions in milliseconds");

    /// <summary>
    /// Гистограмма времени выполнения BeforeCommitSessionAsync
    /// </summary>
    internal static readonly Histogram<double> BeforeCommitDuration =
        Meter.CreateHistogram<double>(
            "repository_before_commit_duration_ms",
            unit: "ms",
            description: "Execution time of BeforeCommitSessionAsync in milliseconds");

    /// <summary>
    /// Гистограмма времени выполнения AfterCommitSessionAsync
    /// </summary>
    internal static readonly Histogram<double> AfterCommitDuration =
        Meter.CreateHistogram<double>(
            "repository_after_commit_duration_ms",
            unit: "ms",
            description: "Execution time of AfterCommitSessionAsync in milliseconds");
}
