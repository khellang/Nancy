namespace Nancy.Responses
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using Nancy.Helpers;

    /// <summary>
    /// Takes an existing response and materialises the body.
    /// Can be used as a wrapper to force execution of the deferred body for
    /// error checking etc.
    /// Copies the existing response into memory, so use with caution.
    /// </summary>
    public class MaterialisingResponse : Response
    {
        private readonly Response sourceResponse;
        private byte[] oldResponseOutput;

        public override async Task PreExecute(NancyContext context, CancellationToken cancellationToken)
        {
            using (var memoryStream = new MemoryStream())
            {
                await this.sourceResponse.Contents.Invoke(memoryStream, cancellationToken);
                this.oldResponseOutput = memoryStream.ToArray();
            }

            await base.PreExecute(context, cancellationToken);
        }

        public MaterialisingResponse(Response sourceResponse)
        {
            this.sourceResponse = sourceResponse;
            this.ContentType = sourceResponse.ContentType;
            this.Headers = sourceResponse.Headers;
            this.StatusCode = sourceResponse.StatusCode;
            this.ReasonPhrase = sourceResponse.ReasonPhrase;

            this.Contents = this.WriteContents;
        }

        private async Task WriteContents(Stream stream, CancellationToken cancellationToken)
        {
            if (this.oldResponseOutput == null)
            {
                await this.sourceResponse.Contents.Invoke(stream, cancellationToken);
            }
            else
            {
                await stream.WriteAsync(this.oldResponseOutput, 0, this.oldResponseOutput.Length, cancellationToken);
            }
        }
    }
}
