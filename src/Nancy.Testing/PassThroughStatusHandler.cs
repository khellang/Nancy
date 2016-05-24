namespace Nancy.Testing
{
    using System;
    using System.Threading.Tasks;
    using Nancy.ErrorHandling;
    using Nancy.Extensions;

    public class PassThroughStatusCodeHandler : IStatusCodeHandler
    {
        public bool HandlesStatusCode(HttpStatusCode statusCode, NancyContext context)
        {
            Exception exception;
            if (!context.TryGetException(out exception) || exception == null)
            {
                return false;
            }

            return statusCode == HttpStatusCode.InternalServerError;
        }

        public Task Handle(HttpStatusCode statusCode, NancyContext context)
        {
            throw new Exception("ConfigurableBootstrapper Exception", context.GetException());
        }
    }
}
