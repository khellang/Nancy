namespace Nancy.Responses
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Response that returns the contents of a stream of a given content-type.
    /// </summary>
    public class StreamResponse : Response
    {
        private Stream source;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamResponse"/> class with the
        /// provided stream provider and content-type.
        /// </summary>
        /// <param name="source">The value producer for the response.</param>
        /// <param name="contentType">The content-type of the stream contents.</param>
        public StreamResponse(Func<Stream> source, string contentType)
        {
            this.Contents = this.GetResponseBodyDelegate(source);
            this.ContentType = contentType;
            this.StatusCode = HttpStatusCode.OK;
        }

        private Func<Stream, CancellationToken, Task> GetResponseBodyDelegate(Func<Stream> sourceDelegate)
        {
            return async (stream, ct) =>
            {
                using (this.source = sourceDelegate.Invoke())
                {
                    await this.source.CopyToAsync(stream);
                }
            };
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            if (this.source != null)
            {
                this.source.Dispose();
            }
        }
    }
}
