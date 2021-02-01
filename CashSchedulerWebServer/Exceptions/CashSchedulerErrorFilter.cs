﻿using HotChocolate;

namespace CashSchedulerWebServer.Exceptions
{
    public class CashSchedulerErrorFilter : IErrorFilter
    {
        public IError OnError(IError error)
        {
            IError finalError;
            
            if (error.Exception is CashSchedulerException exception)
            {
                finalError = error.WithExtensions(exception.Fields).WithCode(exception.Code);
            }
            else
            {
                finalError = error.WithCode("500");
            }

            return finalError.WithMessage(error.Exception?.Message ?? error.Message);
        }
    }
}
