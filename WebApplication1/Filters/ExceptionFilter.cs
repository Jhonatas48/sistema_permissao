using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using WebApplication1.Models;

namespace WebApplication1.Filters
{
    public class ExceptionFilter
     : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var statusCode = context.Exception switch
            {
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                ArgumentException => (int)HttpStatusCode.BadRequest,
                _ => (int)HttpStatusCode.InternalServerError
            };

            var errorResponse = new ErrorResponse(statusCode, context.Exception.Message);

            context.Result = new JsonResult(errorResponse)
            {
                StatusCode = statusCode
            };
            context.ExceptionHandled = true;
        }
    }
}
