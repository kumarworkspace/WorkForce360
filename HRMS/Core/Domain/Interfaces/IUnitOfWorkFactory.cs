namespace HRMS.Core.Domain.Interfaces;

/// <summary>
/// Factory interface for creating UnitOfWork instances with their own DbContext.
/// Used for concurrent operations where a scoped DbContext might conflict.
/// </summary>
public interface IUnitOfWorkFactory : IDisposable
{
    /// <summary>
    /// Creates a new UnitOfWork instance with its own DbContext.
    /// The caller is responsible for disposing the returned UnitOfWork.
    /// </summary>
    IUnitOfWork Create();
}





