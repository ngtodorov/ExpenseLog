using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;

using System.Data.Entity.Infrastructure;
using System.Data;

namespace ExpenseLog.Utils
{
    public class ExceptionHandler
    {
        public virtual void HandleException(Exception exception)
        {
            if (exception is DbUpdateConcurrencyException concurrencyEx)
            {
                throw new DBConcurrencyException();
            }
            else if (exception is DbUpdateException dbUpdateEx)
            {
                if (dbUpdateEx.InnerException != null && dbUpdateEx.InnerException.InnerException != null)
                {
                    if (dbUpdateEx.InnerException.InnerException is SqlException sqlException)
                    {
                        switch (sqlException.Number)
                        {
                            case 2627:  throw new Exception("Uniqueue constrain!");
                            case 547:   throw new Exception("Constraint check violation!");
                            case 2601:  throw new Exception("Duplicated key row error!");
                            default:    throw new Exception(dbUpdateEx.Message, dbUpdateEx.InnerException);
                        }
                    }

                    throw new  Exception(dbUpdateEx.Message, dbUpdateEx.InnerException);
                }
            }

            throw exception;
        }

        public virtual string GetExceptionMessage(Exception exception)
        {
            if (exception is DbUpdateConcurrencyException concurrencyEx)
            {
                return $"DbUpdateConcurrencyException: {concurrencyEx.Message}";
            }
            else 
                if (exception is DbUpdateException dbUpdateEx)
                {
                    if (dbUpdateEx.InnerException != null && dbUpdateEx.InnerException.InnerException != null)
                    {
                        if (dbUpdateEx.InnerException.InnerException is SqlException sqlException)
                        {
                            switch (sqlException.Number)
                            {
                                case 2627: return "Uniqueue constrain!";
                                case 547:  return "Constraint check violation! Can't delete the record because of attached 'child' records.";
                                case 2601: return "Duplicated key row error!";
                                default: return $"{dbUpdateEx.Message} : {dbUpdateEx.InnerException}";
                            }
                        }
                        return $"{dbUpdateEx.Message} : {dbUpdateEx.InnerException}";
                    }
                }
            return exception.GetBaseException().Message;
        }
    }
}