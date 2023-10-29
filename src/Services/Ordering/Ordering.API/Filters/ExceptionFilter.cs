using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Ordering.Application.Exceptions;

namespace Ordering.API.Filters
{
    public class ExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            var exception = context.Exception;
            var exceptionType = exception.GetType();
            if (exceptionType == typeof(NotFoundException))
            {
                context.Result = new NotFoundObjectResult(exception.Message);
            }
            else if(exceptionType == typeof(ValidationException))
            {
                var validationException = (ValidationException) exception;
                context.Result = new BadRequestObjectResult(new { validationException.Message, validationException.Errors});
            }
            // Handle other exception types...
        }
    }
}
