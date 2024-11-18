using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Crud
{
    public interface ICrud<T, TKey> where T : class 
    {
        // Create
        Task<T> AddAsync(T entity);

        // Read
        Task<T> GetByIdAsync(TKey id);
        Task<IEnumerable<T>> GetAllAsync();

        // Update
        Task<bool> UpdateAsync(T entity);

        // Delete
        Task<bool> DeleteAsync(TKey id);
    }
}
