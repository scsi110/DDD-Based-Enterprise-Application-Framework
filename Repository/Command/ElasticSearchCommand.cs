﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ElasticsearchCRUD;
using Domain.Base;
using DomainContextsAndMaps;
using Infrastructure;
using Infrastructure.Utilities;

namespace Repository.Command
{
    public class ElasticSearchCommand<TEntity> : DisposableClass, ICommand<TEntity> where TEntity : BaseIdentityAndAuditableCommandAggregateRoot
    {
        private readonly ElasticsearchContext _elasticsearchContext;

        public ElasticSearchCommand(ElasticSearchContext esContext)
        {
            ContractUtility.Requires<ArgumentNullException>(esContext != null, "esContext instance cannot be null");
            _elasticsearchContext = esContext.ElasticsearchContext;
        }

        public void Insert(TEntity item)
        {
            _elasticsearchContext.AddUpdateDocument(item, item.Id);
            SaveChanges();
        }

        public void Update(TEntity item)
        {
            _elasticsearchContext.AddUpdateDocument(item, item.Id);
            SaveChanges();
        }

        public void Delete(TEntity item)
        {
            _elasticsearchContext.DeleteDocument<TEntity>(item.Id);
            SaveChanges();
        }

        public void Insert(IList<TEntity> items)
        {
            items.ToList().ForEach(item =>
                {
                    _elasticsearchContext.AddUpdateDocument(item, item.Id);
                }
             );
            SaveChanges();
        }

        public void Update(IList<TEntity> items)
        {
            items.ToList().ForEach(item =>
                {
                    _elasticsearchContext.AddUpdateDocument(item, item.Id);
                }
            );
            SaveChanges();
        }

        public void Delete(IList<TEntity> items)
        {
            items.ToList().ForEach(item =>
                {
                    _elasticsearchContext.DeleteDocument<TEntity>(item.Id);
                }
            );
            SaveChanges();
        }
        
        public void BulkInsert(IList<TEntity> items)
        {
            this.Insert(items);
        }

        public void BulkUpdate(IList<TEntity> items)
        {
            this.Update(items);
        }

        public void BulkDelete(IList<TEntity> items)
        {
            this.Delete(items);
        }
        
        public async Task InsertAsync(TEntity item, CancellationToken token = default(CancellationToken))
        {
            _elasticsearchContext.AddUpdateDocument(item, item.Id);
            await SaveChangesAsync(token);
        }

        public async Task UpdateAsync(TEntity item, CancellationToken token = default(CancellationToken))
        {
            _elasticsearchContext.AddUpdateDocument(item, item.Id);
            await SaveChangesAsync(token);
        }

        public async Task DeleteAsync(TEntity item, CancellationToken token = default(CancellationToken))
        {
            _elasticsearchContext.DeleteDocument<TEntity>(item.Id);
            await SaveChangesAsync(token);
        }

        public async Task InsertAsync(IList<TEntity> items, CancellationToken token = default(CancellationToken))
        {
            items.ToList().ForEach(item =>
                {
                    _elasticsearchContext.AddUpdateDocument(item, item.Id);
                }
            );
            await SaveChangesAsync(token);
        }

        public async Task UpdateAsync(IList<TEntity> items, CancellationToken token = default(CancellationToken))
        {
            items.ToList().ForEach(item =>
            {
                _elasticsearchContext.AddUpdateDocument(item, item.Id);
            }
            );
            await SaveChangesAsync(token);
        }

        public async Task DeleteAsync(IList<TEntity> items, CancellationToken token = default(CancellationToken))
        {
            items.ToList().ForEach(item =>
            {
                _elasticsearchContext.DeleteDocument<TEntity>(item.Id);
            }
            );
            await SaveChangesAsync(token);
        }

        public async Task BulkInsertAsync(IList<TEntity> items, CancellationToken token = default(CancellationToken))
        {
            await this.InsertAsync(items, token);
        }

        public async Task BulkUpdateAsync(IList<TEntity> items, CancellationToken token = default(CancellationToken))
        {
            await this.UpdateAsync(items, token);
        }

        public async Task BulkDeleteAsync(IList<TEntity> items, CancellationToken token = default(CancellationToken))
        {
            await this.DeleteAsync(items, token);
        }

        #region Private Methods


        private void SaveChanges()
        {
            if (_elasticsearchContext.IndexTypeExists<TEntity>())
            {
                _elasticsearchContext.SaveChanges();
            }
            else
            {
                _elasticsearchContext.SaveChangesAndInitMappings();
            }
        }

        private async Task SaveChangesAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (_elasticsearchContext.IndexTypeExistsAsync<TEntity>().Result.PayloadResult)
            {
                await _elasticsearchContext.SaveChangesAsync();
            }
            else
            {
                _elasticsearchContext.SaveChangesAndInitMappings();
            }
        }

        #endregion


        #region Free Disposable Members

        protected override void FreeManagedResources()
        {
            base.FreeManagedResources();
            _elasticsearchContext.Dispose();
        }

        #endregion
    }
}
