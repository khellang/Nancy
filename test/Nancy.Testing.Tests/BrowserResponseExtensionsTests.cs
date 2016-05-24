namespace Nancy.Testing.Tests
{
    using System.Threading.Tasks;
    using System.Xml;
    using FakeItEasy;
    using Xunit;

    public class BrowserResponseExtensionsTests
	{
		private BrowserResponse sut;

		[Fact]
		public async Task Should_create_xdocument_from_xml_body()
		{
            // Given
			var context = new NancyContext() { Response = "<tag />" };
            sut = new BrowserResponse(context, A.Fake<Browser>(), A.Dummy<BrowserContext>());

            // When
            var bodyAsXml = await sut.BodyAsXml();

            // Then
			Assert.NotNull(bodyAsXml.Element("tag"));
		}

		[Fact]
		public async Task Should_fail_to_create_xdocument_from_non_xml_body()
		{
            // Given
			var context = new NancyContext() { Response = "hello" };

            // When
		    sut = new BrowserResponse(context, A.Fake<Browser>(), A.Dummy<BrowserContext>());

            // Then
			await Assert.ThrowsAsync<XmlException>(() => sut.BodyAsXml());
		}
	}
}
