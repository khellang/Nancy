namespace Nancy.ErrorHandling
{
    using System.Threading.Tasks;

    /// <summary>
    /// Provides informative responses for particular HTTP status codes
    /// </summary>
    public interface IStatusCodeHandler
    {
        /// <summary>
        /// Check if the error handler can handle errors of the provided status code.
        /// </summary>
        /// <param name="statusCode">Status code</param>
        /// <param name="context">The <see cref="NancyContext"/> instance of the current request.</param>
        /// <returns>True if handled, false otherwise</returns>
        bool HandlesStatusCode(HttpStatusCode statusCode, NancyContext context);

        /// <summary>
        /// Handle the error code
        /// </summary>
        /// <param name="statusCode">Status code</param>
        /// <param name="context">Current context</param>
        Task Handle(HttpStatusCode statusCode, NancyContext context);
    }
}
