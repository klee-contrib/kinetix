﻿using System;
using System.Collections.Generic;
using System.Transactions;
using Kinetix.Services;

namespace Kinetix.Data.SqlClient
{
    internal class SqlTransactionContext : ITransactionContext
    {
        private readonly TransactionScope _scope = new(TransactionScopeOption.RequiresNew, new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted, Timeout = TimeSpan.Zero });

        public bool IsDatabaseContext => true;

        internal List<SqlServerConnection> Connections = new();

        public void Complete()
        {
            _scope.Complete();
        }

        public void Dispose()
        {
            foreach (var connection in Connections)
            {
                connection.Dispose();
            }

            _scope.Dispose();
        }
    }
}
