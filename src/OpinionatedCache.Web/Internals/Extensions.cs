using System;
using System.Data.SqlClient;
using System.Transactions;

namespace OpinionatedCache.SQL
{
    public static class Utility
    {
        public static bool IsWorthRetry(this Exception ex)
        {
            if (ex is TransactionException)
            {
                if (ex.Message.Contains(" aborted."))
                    return true;

                if (ex.Message.Contains(" in doubt"))
                    return true;

                if (ex.Message.Contains(" is not valid for the state of the transaction")
                    && (ex.InnerException == null || !ex.InnerException.Message.Contains(" timeout")))
                    return false;

                return false;
            }
            else if (ex is SqlException)
            {
                if (ex.Message.Contains(" deadlocked ")
                    || ex.Message.Contains(" timeout "))
                    return true;
            }
            else if (ex is InvalidOperationException)
            {
                if (ex.Message.Contains(" pool "))
                    return true;
            }

            return false;
        }
    }
}
