namespace Nancy.Testing
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Serialization;

    using Nancy.Json;

    /// <summary>
    /// Defines extensions for the <see cref="NancyContext"/> type.
    /// </summary>
    public static class NancyContextExtensions
    {
        private const string DOCUMENT_WRAPPER_KEY_NAME = "@@@@DOCUMENT_WRAPPER@@@@";
        private const string JSONRESPONSE_KEY_NAME = "@@@@JSONRESPONSE@@@@";
        private const string XMLRESPONSE_KEY_NAME = "@@@@XMLRESPONSE@@@@";

        private static async Task<T> Cache<T>(NancyContext context, string key, Func<Task<T>> getData)
        {
            // We only really want to generate this once, so we'll stick it in the context
            // This isn't ideal, but we don't want to hide the guts of the context from the
            // tests this will have to do.
            if (context.Items.ContainsKey(key))
            {
                return (T)context.Items[key];
            }

            T data = await getData.Invoke().ConfigureAwait(false);
            context.Items[key] = data;
            return data;
        }

        /// <summary>
        /// Returns the HTTP response body, of the specified <see cref="NancyContext"/>, wrapped in an <see cref="DocumentWrapper"/> instance.
        /// </summary>
        /// <param name="context">The <see cref="NancyContext"/> instance that the HTTP response body should be retrieved from.</param>
        /// <returns>A <see cref="DocumentWrapper"/> instance, wrapping the HTTP response body of the context.</returns>
        public static Task<DocumentWrapper> DocumentBody(this NancyContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Cache(context, DOCUMENT_WRAPPER_KEY_NAME, async () =>
            {
                using (var contentsStream = new MemoryStream())
                {
                    await context.Response.Contents.Invoke(contentsStream, cancellationToken).ConfigureAwait(false);
                    contentsStream.Position = 0;
                    return new DocumentWrapper(contentsStream.GetBuffer());
                }
            });
        }

        public static Task<TModel> JsonBody<TModel>(this NancyContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            return context.JsonBody<TModel>(new JavaScriptSerializer(), cancellationToken);
        }

        public static Task<TModel> JsonBody<TModel>(this NancyContext context, JavaScriptSerializer serializer, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Cache(context, JSONRESPONSE_KEY_NAME, async () =>
            {
                using (var contentsStream = new MemoryStream())
                {
                    await context.Response.Contents.Invoke(contentsStream, cancellationToken).ConfigureAwait(false);
                    contentsStream.Position = 0;
                    using (var contents = new StreamReader(contentsStream))
                    {
                        var model = serializer.Deserialize<TModel>(contents.ReadToEnd());
                        return model;
                    }
                }
            });
        }

        public static Task<TModel> XmlBody<TModel>(this NancyContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Cache(context, XMLRESPONSE_KEY_NAME, async () =>
            {
                using (var contentsStream = new MemoryStream())
                {
                    await context.Response.Contents.Invoke(contentsStream, cancellationToken).ConfigureAwait(false);
                    contentsStream.Position = 0;
                    var serializer = new XmlSerializer(typeof (TModel));
                    var model = serializer.Deserialize(contentsStream);
                    return (TModel) model;
                }
            });
        }
    }
}
