using BusinessLogic.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLogic.Crud
{
    public class Crud<T, TKey> : ICrud<T, TKey> where T : class
    {
        private readonly AppDbContext _dbContext;

        public Crud(IServiceProvider serviceProvider)
        {
            // Using DI to resolve DbContext and Logger
            _dbContext = serviceProvider.GetRequiredService<AppDbContext>();
        }

        public async Task<T> AddAsync(T entity)
        {
            try
            {
                if (entity == null) throw new ArgumentNullException(nameof(entity), "Entity cannot be null");

                await _dbContext.Set<T>().AddAsync(entity);
                await _dbContext.SaveChangesAsync();

                return entity;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while adding the entity.", ex);
            }
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            try
            {
                var entities = await _dbContext.Set<T>().ToListAsync();
                return entities;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while retrieving entities.", ex);
            }
        }

        public async Task<T> GetByIdAsync(TKey id)
        {
            try
            {
                if (id == null) throw new ArgumentNullException(nameof(id), "ID cannot be null");

                var entity = await _dbContext.Set<T>().FindAsync(id);

                return entity;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while retrieving the entity by ID.", ex);
            }
        }

        public async Task<bool> UpdateAsync(T entity)
        {
            try
            {
                if (entity == null) throw new ArgumentNullException(nameof(entity), "Entity cannot be null");

                _dbContext.Set<T>().Update(entity);
                var result = await _dbContext.SaveChangesAsync() > 0;

                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while updating the entity.", ex);
            }
        }

        public async Task<bool> DeleteAsync(TKey id)
        {
            try
            {
                if (id == null) throw new ArgumentNullException(nameof(id), "ID cannot be null");

                var entity = await _dbContext.Set<T>().FindAsync(id);
                if (entity == null)
                {
                    return false;
                }

                _dbContext.Set<T>().Remove(entity);
                var result = await _dbContext.SaveChangesAsync() > 0;
                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while deleting the entity.", ex);
            }
        }
    }
}
