﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Z.BulkOperations;
using Domain.Base;
using Infrastructure;
using Infrastructure.Utilities;

namespace Repository.Command
{
    /// <summary>
    /// Please keep a note that DBContext is not thread safe and so be cautious while using EF DB Context in multi threaded scenarios
    /// as indicated here - http://mehdi.me/ambient-dbcontext-in-ef6/
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class EntityFrameworkCodeFirstCommand<TEntity> : DisposableClass, ICommand<TEntity> where TEntity : class, ICommandAggregateRoot
    {
        private readonly DbContext _dbContext;
        private readonly DbSet<TEntity> _dbSet;

        public EntityFrameworkCodeFirstCommand(DbContext dbContext)
        {
            ContractUtility.Requires<ArgumentNullException>(dbContext.IsNotNull(), "dbContext instance cannot be null");
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<TEntity>();
        }

        #region ICommand<T> Members

        /// <summary>
        /// One good stuff about Entity Framework is that the value of the primary key gets automatically updated
        /// within the supplied object(here the object is "item").One doesn't need to explicitly return some type here
        /// after calling _dbContext.SaveChanges(). 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public void Insert(TEntity item)
        {
            ContractUtility.Requires<ArgumentNullException>(item.IsNotNull(), "item instance cannot be null");
            var entry = _dbContext.Entry(item);
            if (entry.State != EntityState.Detached)
            {
                entry.State = EntityState.Added;
            }
            else
            {
                _dbSet.Add(item);
            }
            SaveChanges();
        }

        public void Update(TEntity item)
        {
            ContractUtility.Requires<ArgumentNullException>(item.IsNotNull(), "item instance cannot be null");
            var entry = _dbContext.Entry(item);
            if (entry.State == EntityState.Detached)
            {
                _dbSet.Attach(item);
            }
            entry.State = EntityState.Modified;
            SaveChanges();
        }

        public void Delete(TEntity item)
        {
            ContractUtility.Requires<ArgumentNullException>(item.IsNotNull(), "item instance cannot be null");
            var entry = _dbContext.Entry(item);
            if (entry.State != EntityState.Deleted)
            {
                entry.State = EntityState.Deleted;
            }
            else
            {
                _dbSet.Attach(item);
                _dbSet.Remove(item);
            }
            SaveChanges();
        }

        //Cascading effects to be taken care at Domain level.
        public void Insert(IList<TEntity> items)
        {
            ContractUtility.Requires<ArgumentNullException>(items.IsNotNull(), "items instance cannot be null");
            ContractUtility.Requires<ArgumentOutOfRangeException>(items.IsNotEmpty(), "items count should be greater than 0");
            items.ForEach(item =>
             {
                 var entry = _dbContext.Entry(item);
                 if (entry.State != EntityState.Detached)
                 {
                     entry.State = EntityState.Added;
                 }
                 else
                 {
                     _dbSet.Add(item);
                 }
             });
            SaveChanges();
        }

        //Cascading effects to be taken care at Domain level.
        public void Update(IList<TEntity> items)
        {
            ContractUtility.Requires<ArgumentNullException>(items.IsNotNull(), "items instance cannot be null");
            ContractUtility.Requires<ArgumentOutOfRangeException>(items.IsNotEmpty(), "items count should be greater than 0");
            items.ForEach(item =>
            {
                var entry = _dbContext.Entry(item);
                if (entry.State == EntityState.Detached)
                {
                    _dbSet.Attach(item);
                }
                entry.State = EntityState.Modified;
            });
            SaveChanges();
        }

        //Cascading effects to be taken care at Domain level.
        public void Delete(IList<TEntity> items)
        {
            ContractUtility.Requires<ArgumentNullException>(items.IsNotNull(), "items instance cannot be null");
            ContractUtility.Requires<ArgumentOutOfRangeException>(items.IsNotEmpty(), "items count should be greater than 0");
            items.ForEach(item =>
            {
                var entry = _dbContext.Entry(item);
                if (entry.State != EntityState.Deleted)
                {
                    entry.State = EntityState.Deleted;
                }
                else
                {
                    _dbSet.Attach(item);
                    _dbSet.Remove(item);
                }
            });
            SaveChanges();
        }

        public void BulkInsert(IList<TEntity> items)
        {
            ContractUtility.Requires<ArgumentNullException>(items.IsNotNull(), "items instance cannot be null");
            ContractUtility.Requires<ArgumentOutOfRangeException>(items.IsNotEmpty(), "items count should be greater than 0");
            var connection = _dbContext.Database.Connection;
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            var bulkOperation = new BulkOperation<TEntity>(connection);
            ApplyAuditInfoRules();
            bulkOperation.BulkInsert(items);
        }

        public void BulkUpdate(IList<TEntity> items)
        {
            ContractUtility.Requires<ArgumentNullException>(items.IsNotNull(), "items instance cannot be null");
            ContractUtility.Requires<ArgumentOutOfRangeException>(items.IsNotEmpty(), "items count should be greater than 0");
            var connection = _dbContext.Database.Connection;
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            var bulkOperation = new BulkOperation<TEntity>(connection);
            ApplyAuditInfoRules();
            bulkOperation.BulkUpdate(items);
        }

        public void BulkDelete(IList<TEntity> items)
        {
            ContractUtility.Requires<ArgumentNullException>(items.IsNotNull(), "items instance cannot be null");
            ContractUtility.Requires<ArgumentOutOfRangeException>(items.IsNotEmpty(), "items count should be greater than 0");
            var connection = _dbContext.Database.Connection;
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            var bulkOperation = new BulkOperation<TEntity>(connection);
            ApplyAuditInfoRules();
            bulkOperation.BulkDelete(items);
        }

        public async Task InsertAsync(TEntity item, CancellationToken token = default(CancellationToken))
        {
            ContractUtility.Requires<ArgumentNullException>(item.IsNotNull(), "item instance cannot be null");
            var entry = _dbContext.Entry(item);
            if (entry.State != EntityState.Detached)
            {
                entry.State = EntityState.Added;
            }
            else
            {
                _dbSet.Add(item);
            }
           await SaveChangesAsync(token);
        }

        public async Task UpdateAsync(TEntity item, CancellationToken token = default(CancellationToken))
        {
            ContractUtility.Requires<ArgumentNullException>(item.IsNotNull(), "item instance cannot be null");
            var entry = _dbContext.Entry(item);
            if (entry.State == EntityState.Detached)
            {
                _dbSet.Attach(item);
            }
            entry.State = EntityState.Modified;
            await SaveChangesAsync(token);
        }

        public async Task DeleteAsync(TEntity item, CancellationToken token = default(CancellationToken))
        {
            ContractUtility.Requires<ArgumentNullException>(item.IsNotNull(), "item instance cannot be null");
            var entry = _dbContext.Entry(item);
            if (entry.State != EntityState.Deleted)
            {
                entry.State = EntityState.Deleted;
            }
            else
            {
                _dbSet.Attach(item);
                _dbSet.Remove(item);
            }
            await SaveChangesAsync(token);
        }

        public async Task InsertAsync(IList<TEntity> items, CancellationToken token = default(CancellationToken))
        {
            ContractUtility.Requires<ArgumentNullException>(items.IsNotNull(), "items instance cannot be null");
            ContractUtility.Requires<ArgumentOutOfRangeException>(items.IsNotEmpty(), "items count should be greater than 0");
            items.ForEach(item =>
            {
                var entry = _dbContext.Entry(item);
                if (entry.State != EntityState.Detached)
                {
                    entry.State = EntityState.Added;
                }
                else
                {
                    _dbSet.Add(item);
                }
            });
            await SaveChangesAsync(token);
        }

        public async Task UpdateAsync(IList<TEntity> items, CancellationToken token = default(CancellationToken))
        {
            ContractUtility.Requires<ArgumentNullException>(items.IsNotNull(), "items instance cannot be null");
            ContractUtility.Requires<ArgumentOutOfRangeException>(items.IsNotEmpty(), "items count should be greater than 0");
            items.ForEach(item =>
            {
                var entry = _dbContext.Entry(item);
                if (entry.State == EntityState.Detached)
                {
                    _dbSet.Attach(item);
                }
                entry.State = EntityState.Modified;
            });
            await SaveChangesAsync(token);
        }

        public async Task DeleteAsync(IList<TEntity> items, CancellationToken token = default(CancellationToken))
        {
            ContractUtility.Requires<ArgumentNullException>(items.IsNotNull(), "items instance cannot be null");
            ContractUtility.Requires<ArgumentOutOfRangeException>(items.IsNotEmpty(), "items count should be greater than 0");
            items.ForEach(item =>
            {
                var entry = _dbContext.Entry(item);
                if (entry.State != EntityState.Deleted)
                {
                    entry.State = EntityState.Deleted;
                }
                else
                {
                    _dbSet.Attach(item);
                    _dbSet.Remove(item);
                }
            });
            await SaveChangesAsync(token);
        }

        public async Task BulkInsertAsync(IList<TEntity> items, CancellationToken token = default(CancellationToken))
        {
            ///TODO - Need to come up with a way to handle this.
            throw new NotImplementedException();
        }

        public async Task BulkUpdateAsync(IList<TEntity> items, CancellationToken token = default(CancellationToken))
        {
            ///TODO - Need to come up with a way to handle this.
            throw new NotImplementedException();
        }

        public async Task BulkDeleteAsync(IList<TEntity> items, CancellationToken token = default(CancellationToken))
        {
            ///TODO - Need to come up with a way to handle this.
            throw new NotImplementedException();
        }

        #endregion

        private void SaveChanges()
        {
            ApplyAuditInfoRules();
            _dbContext.SaveChanges();
        }

        private async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            ApplyAuditInfoRules();
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private void ApplyAuditInfoRules()
        {
            var changedAudits = _dbContext.ChangeTracker.Entries()
                                 .Where(e => e.Entity is BaseIdentityAndAuditableCommandAggregateRoot
                                 && ((e.State == EntityState.Added) 
                                 || (e.State == EntityState.Modified)));

            changedAudits.ForEach(entry =>
            {
                var entity = entry.Entity as BaseIdentityAndAuditableCommandAggregateRoot;

                if (entry.State == EntityState.Added)
                {
                    if (!entity.PreserveCreatedOn)
                    {
                        entity.CreatedOn = DateTime.Now;
                    }
                }
                else
                {
                    entity.LastUpdateOn = DateTime.Now;
                }
            });
        }

        #region Free Disposable Members

        protected override void FreeManagedResources()
        {
            base.FreeManagedResources();
            _dbContext.Dispose();
        }

        #endregion
    }
}
