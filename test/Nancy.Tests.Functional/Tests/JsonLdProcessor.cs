namespace Nancy.Tests.Functional.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Nancy.Responses.Negotiation;

    public class JsonLdProcessor : IResponseProcessor
    {
        private readonly ISerializer serializer;

        public JsonLdProcessor(IEnumerable<ISerializer> serializers)
        {
            this.serializer = serializers.FirstOrDefault(x => x.CanSerialize("application/json"));
        }

        public IEnumerable<Tuple<string, MediaRange>> ExtensionMappings
        {
            get { return new[] { new Tuple<string, MediaRange>("json", new MediaRange("application/json")) }; }
        }

        public ProcessorMatch CanProcess(MediaRange requestedMediaRange, dynamic model, NancyContext context)
        {
            return new ProcessorMatch
            {
                ModelResult = MatchResult.DontCare,
                RequestedContentTypeResult = MatchResult.DontCare
            };
        }

        public Task<Response> Process(MediaRange requestedMediaRange, dynamic model, NancyContext context)
        {
            return Task.FromResult(new Response
            {
                ContentType = "application/json",
                Contents = (stream, ct) => this.serializer.Serialize("application/json", model, stream, ct),
                StatusCode = HttpStatusCode.OK
            }.WithHeader("Link", "</context.jsonld>; rel=\"http://www.w3.org/ns/json-ld#context\"; type=\"application/ld+json\""));
        }
    }
}
