﻿using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Base.Aggregates;
using FluentRepository.FluentInterfaces;
using Infrastructure.Utilities;
using Repository.Command;

namespace FluentRepository.FluentImplementations
{
    internal class FluentCommandRepository : FluentCommands, IFluentCommandRepository
    {
        public FluentCommandRepository(UnitOfWorkData unitOfWorkData, IList<dynamic> commandRepositories, IList<dynamic> repositoriesList, Queue<OperationData> operationsQueue) : base(unitOfWorkData, commandRepositories, repositoriesList, operationsQueue)
        {
        }

        public IFluentCommands SetUpCommandPersistance<TEntity>(ICommand<TEntity> command)
            where TEntity : class, ICommandAggregateRoot
        {
            ContractUtility.Requires<ArgumentNullException>(command.IsNotNull(), "command instance cannot be null");
            var commandRepository = _commandRepositories.SingleOrDefault(x => x != null && x.GetType().GenericTypeArguments[0] == typeof(TEntity));
            ContractUtility.Requires<ArgumentNullException>(commandRepository != null, string.Format("Last Command Repository has been not set up for {0}.", typeof(TEntity).Name));
            commandRepository.SetCommand(command);
            return new FluentCommands(_unitOfWorkData, _commandRepositories, _repositoriesList, _operationsQueue);
        }

        public IFluentCommands SetUpCommandPersistance(params dynamic[] commands)
        {
            return SetUpCommandRepository(commands.ToList());
        }

        public IFluentCommands SetUpCommandPersistance(IList<dynamic> commands)
        {
            throw new NotImplementedException();
        }
    }
}
