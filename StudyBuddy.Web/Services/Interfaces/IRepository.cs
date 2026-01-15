using System.Linq.Expressions;

namespace StudyBuddy.Web.Services.Interfaces
{
    // (Generic Repository Pattern - DIP)
    /// <summary>
    /// SOLID - DIP: Abstraktni repozitorij za sve entitete.
    /// SOLID - Generic programiranje: Izbjegava duplikaciju koda.
    /// </summary>
    /// Repo
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<TEntity> GetByIdAsync(int id);
        Task<IReadOnlyList<TEntity>> GetAllAsync();
        Task<IReadOnlyList<TEntity>> GetWhereAsync(Expression<Func<TEntity, bool>> predicate);
        Task AddAsync(TEntity entity);
        Task UpdateAsync(TEntity entity);
        Task DeleteAsync(TEntity entity);
        Task SaveChangesAsync();
    }
}