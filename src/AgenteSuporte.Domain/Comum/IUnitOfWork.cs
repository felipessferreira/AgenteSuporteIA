namespace AgenteSuporte.Domain.Comum;

/// <summary>
/// Confirma as alterações feitas nos agregados como uma única transação.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SalvarAlteracoesAsync(CancellationToken ct = default);
}
