namespace Nancy.Tests.Unit
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Nancy.Tests.Extensions;
    using FakeItEasy;

    using Xunit;

    public class HeadResponseFixture
    {
        private readonly IDictionary<string, string> headers;
        private readonly Response response;

        public HeadResponseFixture()
        {
            // Given
            this.headers = new Dictionary<string, string> { { "Test", "Value " } };
            this.response = "This is the content";

            this.response.ContentType = "application/json";
            this.response.Headers = headers;
            this.response.StatusCode = HttpStatusCode.ResetContent;
        }

        private async Task<HeadResponse> CreateHeadResponse()
        {
            var head = new HeadResponse(this.response);
            await head.PreExecute(A.Dummy<NancyContext>(), CancellationToken.None);
            await head.Contents(new MemoryStream(), CancellationToken.None);
            return head;
        }

        [Fact]
        public async Task Should_set_status_property_to_that_of_decorated_response()
        {
            //When
            var head = await this.CreateHeadResponse();

            // Then
            head.StatusCode.ShouldEqual(this.response.StatusCode);
        }

        [Fact]
        public async Task Should_set_headers_property_to_that_of_decorated_response()
        {
            //When
            var head = await this.CreateHeadResponse();

            // Then
            head.Headers.ShouldBeSameAs(this.headers);
        }

        [Fact]
        public async Task Should_set_content_type_property_to_that_of_decorated_response()
        {
            //When
            var head = await this.CreateHeadResponse();

            // Then
            head.ContentType.ShouldEqual(this.response.ContentType);
        }

        [Fact]
        public async Task Should_set_empty_content()
        {
            //When
            var head = await this.CreateHeadResponse();

            var content = await head.GetStringContentsFromResponse();

            // Then
            content.ShouldBeEmpty();
        }

        [Fact]
        public async Task Should_set_content_length()
        {
            //When
            var head = await this.CreateHeadResponse();

            // Then
            head.Headers.ContainsKey("Content-Length").ShouldBeTrue();
            head.Headers["Content-Length"].ShouldNotEqual("0");
        }

        [Fact]
        public async Task Should_not_overwrite_content_length()
        {
            // Given, When
            this.response.Headers.Add("Content-Length", "foo");
            var head = await this.CreateHeadResponse();

            // Then
            head.Headers.ContainsKey("Content-Length").ShouldBeTrue();
            head.Headers["Content-Length"].ShouldEqual("foo");
        }

    }
}
