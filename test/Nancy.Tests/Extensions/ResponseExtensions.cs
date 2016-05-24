namespace Nancy.Tests.Extensions
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public static class ResponseExtensions
    {
        public static async Task<string> GetStringContentsFromResponse(this Response response, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var memory = new MemoryStream())
            {
                await response.Contents.Invoke(memory, cancellationToken);

                memory.Position = 0;

                using (var reader = new StreamReader(memory))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }
    }
}
