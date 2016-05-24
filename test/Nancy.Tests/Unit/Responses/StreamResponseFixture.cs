namespace Nancy.Tests.Unit.Responses
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using FakeItEasy;

    using Nancy.Responses;

    using Xunit;

    public class StreamResponseFixture
    {
        [Fact]
        public async Task Should_copy_stream_to_output_when_body_invoked()
        {
            // Given
            var streamContent =
                new byte[] { 1, 2, 3, 4, 5 };

            var inputStream =
                new MemoryStream(streamContent);

            var response =
                new StreamResponse(() => inputStream, "test");

            var outputStream = new MemoryStream();

            // When
            await response.Contents.Invoke(outputStream, CancellationToken.None);

            // Then
            outputStream.ToArray().ShouldEqualSequence(streamContent);
        }

        [Fact]
        public async Task Should_return_content_of_stream_from_current_location_of_stream()
        {
            // Given
            var streamContent =
                new byte[] { 1, 2, 3, 4, 5 };

            var inputStream =
                new MemoryStream(streamContent) { Position = 2 };

            var response =
                new StreamResponse(() => inputStream, "test");

            var outputStream = new MemoryStream();

            var expectedContent =
                new byte[] { 3, 4, 5 };

            // When
            await response.Contents.Invoke(outputStream, CancellationToken.None);

            // Then
            outputStream.ToArray().ShouldEqualSequence(expectedContent);
        }

        [Fact]
        public async Task Should_throw_exception_when_stream_is_non_readable()
        {
            // Given
            var inputStream =
                A.Fake<Stream>();

            A.CallTo(() => inputStream.CanRead).Returns(false);

            var response =
                new StreamResponse(() => inputStream, "test");

            var outputStream = new MemoryStream();

            // When
            var exception = await Assert.ThrowsAsync<Exception>(async () =>
                await response.Contents.Invoke(outputStream, CancellationToken.None));

            // Then
            exception.ShouldNotBeNull();
        }
    }
}
