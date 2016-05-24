namespace Nancy.Demo.Hosting.Aspnet
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Yarrrr;

    public class HereBeAResponseYouScurvyDog : Response
    {
        public HereBeAResponseYouScurvyDog(Response response)
        {
            this.ContentType = response.ContentType;
            this.Headers = response.Headers;
            this.StatusCode = response.StatusCode;

            this.Contents = GetContents(response);
        }

        protected static Func<Stream, CancellationToken, Task> GetContents(Response response)
        {
            return async (stream, ct) =>
            {
                using (var memoryStream = new MemoryStream())
                {
                    await response.Contents.Invoke(memoryStream, ct).ConfigureAwait(false);

                    var output = Encoding.ASCII.GetString(memoryStream.GetBuffer());

                    var writer = new StreamWriter(stream)
                    {
                        AutoFlush = true
                    };

                    await writer.WriteAsync(output.Piratize()).ConfigureAwait(false);
                }
            };
        }
    }
}
