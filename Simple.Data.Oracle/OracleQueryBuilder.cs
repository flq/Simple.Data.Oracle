using System;
using System.Collections.Generic;
using System.Linq;
using Simple.Data.Ado;

namespace Simple.Data.Oracle
{
    class OracleQueryBuilder : QueryBuilderBase
    {
        private List<SimpleQueryClauseBase> _unhandledClauses;

        public OracleQueryBuilder(AdoAdapter adapter, int bulkIndex, IFunctionNameConverter functionNameConverter)
            : base(adapter, bulkIndex, functionNameConverter)
        {
        }

        public override ICommandBuilder Build(SimpleQuery query, out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            // check that we don't have an invalid combination of clauses
            if ((query.Clauses.OfType<ForUpdateClause>().FirstOrDefault() != null) &&
                (query.Clauses.OfType<SkipClause>().FirstOrDefault() != null))
            {
                throw new InvalidOperationException("ForUpdate is not allowed with Skip");
            }

            _unhandledClauses = new List<SimpleQueryClauseBase>();
            SetQueryContext(query);

            HandleJoins();
            HandleQueryCriteria();
            HandleGrouping();
            HandleHavingCriteria();
            HandleOrderBy();
            HandleForUpdate();

            unhandledClauses = _unhandledClauses;
            return _commandBuilder;
        }

        private void HandleForUpdate()
        {
            var forUpdateClause = _query.Clauses.OfType<ForUpdateClause>().FirstOrDefault();
            if (forUpdateClause != null)
            {
                var forUpdate = " FOR UPDATE" + (forUpdateClause.SkipLockedRows ? " SKIP LOCKED" : " NOWAIT");
                _commandBuilder.Append(forUpdate);
            }
        }

        protected override void HandleQueryCriteria()
        {
            var takeClause = _query.Clauses.OfType<TakeClause>().FirstOrDefault();
            bool applylimit = (takeClause != null) && (_query.Clauses.OfType<SkipClause>().FirstOrDefault() == null);
            base.HandleQueryCriteria();
            if (applylimit)
                _commandBuilder.Append((_whereCriteria == SimpleExpression.Empty ? " WHERE " : " AND ") + "ROWNUM < " + (takeClause.Count + 1));
        }
    }
}
