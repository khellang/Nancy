namespace Nancy.Tests.Responses
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Nancy.Helpers;
    using Nancy.Responses;
    using Xunit;

    public class MaterialisingResponseFixture
    {
        [Fact]
        public async Task Should_call_inner_response_on_preinit()
        {
            // Given
            var sourceResponse = new FakeResponse();
            var response = new MaterialisingResponse(sourceResponse);
            var context = this.GetContext();

            // When
            await response.PreExecute(context, CancellationToken.None);

            // Then
            sourceResponse.ContentsCalled.ShouldBeTrue();
        }

        [Fact]
        public async Task Should_not_call_inner_response_again_if_alread_inited()
        {
            // Given
            var sourceResponse = new FakeResponse();
            var response = new MaterialisingResponse(sourceResponse);
            var context = this.GetContext();
            await response.PreExecute(context, CancellationToken.None);
            sourceResponse.ContentsCalled = false;

            // When
            await response.Contents.Invoke(new MemoryStream(), CancellationToken.None);

            // Then
            sourceResponse.ContentsCalled.ShouldBeFalse();
        }

        [Fact]
        public async Task Should_call_inner_response_again_if_executed_and_not_already_inited()
        {
            // Given
            var sourceResponse = new FakeResponse();
            var response = new MaterialisingResponse(sourceResponse);

            // When
            await response.Contents.Invoke(new MemoryStream(), CancellationToken.None);

            // Then
            sourceResponse.ContentsCalled.ShouldBeTrue();
        }

        private NancyContext GetContext()
        {
            return new NancyContext();
        }
    }

    public class FakeResponse : Response
    {
        public bool ContentsCalled { get; set; }

        public Stream PassedStream { get; set; }

        public FakeResponse()
        {
            this.Contents = (stream, ct) =>
            {
                this.PassedStream = stream;
                this.ContentsCalled = true;

                return TaskHelpers.CompletedTask;
            };
        }
    }
}
