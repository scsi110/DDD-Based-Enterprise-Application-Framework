﻿using System;
using System.Collections.Generic;
using Domain.Base;
using Repository.Base;
using Infrastructure.SemanticLogging.CrossCuttingEventSources;
using Infrastructure.Utilities; 

namespace ApplicationServices.Base.CommandApplicationServices
{
    public class CommandApplicationService<TEntity> : ICommandApplicationService<TEntity> where TEntity : ICommandAggregateRoot
    {
        protected readonly ICommandRepository<TEntity> _repository;

        public CommandApplicationService(ICommandRepository<TEntity> repository)
        {
            ContractUtility.Requires<ArgumentNullException>(repository != null, "repository instance cannot be null");
            _repository = repository;
        }
        
        public virtual bool Insert(TEntity item, Action operationToExecuteBeforeNextOperation = null)
        {
            return InvokeAfterWrappingWithinExceptionHandling(() => _repository.Insert(item, operationToExecuteBeforeNextOperation));
        }

        public virtual bool Update(TEntity item, Action operationToExecuteBeforeNextOperation = null)
        {
            return InvokeAfterWrappingWithinExceptionHandling(() => _repository.Update(item, operationToExecuteBeforeNextOperation));
        }

        public virtual bool Delete(TEntity item, Action operationToExecuteBeforeNextOperation = null)
        {
            return InvokeAfterWrappingWithinExceptionHandling(() => _repository.Delete(item, operationToExecuteBeforeNextOperation));
        }

        public virtual bool Insert(IList<TEntity> items, Action operationToExecuteBeforeNextOperation = null)
        {
            return InvokeAfterWrappingWithinExceptionHandling(() => _repository.Insert(items, operationToExecuteBeforeNextOperation));
        }

        public virtual bool Update(IList<TEntity> items, Action operationToExecuteBeforeNextOperation = null)
        {
            return InvokeAfterWrappingWithinExceptionHandling(() => _repository.Update(items, operationToExecuteBeforeNextOperation));
        }

        public virtual bool Delete(IList<TEntity> items, Action operationToExecuteBeforeNextOperation = null)
        {
            return InvokeAfterWrappingWithinExceptionHandling(() => _repository.Delete(items, operationToExecuteBeforeNextOperation));
        }

        public virtual bool BulkInsert(IList<TEntity> items, Action operationToExecuteBeforeNextOperation = null)
        {
            return InvokeAfterWrappingWithinExceptionHandling(() => _repository.BulkInsert(items, operationToExecuteBeforeNextOperation));
        }

        public virtual bool BulkUpdate(IList<TEntity> items, Action operationToExecuteBeforeNextOperation = null)
        {
            return InvokeAfterWrappingWithinExceptionHandling(() => _repository.BulkUpdate(items, operationToExecuteBeforeNextOperation));
        }

        public virtual bool BulkDelete(IList<TEntity> items, Action operationToExecuteBeforeNextOperation = null)
        {
            return InvokeAfterWrappingWithinExceptionHandling(() => _repository.BulkDelete(items, operationToExecuteBeforeNextOperation));
        }

        protected bool InvokeAfterWrappingWithinExceptionHandling(Action repositoryAction)
        {
            try
            {
                repositoryAction();
                return true;
            }
            catch (Exception ex)
            {
                ExceptionLogEvents.Log.LogException(ex.ToString());
                return false;
            }
        }
    }
}
