namespace Nancy.ViewEngines.Markdown.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using FakeItEasy;

    using Nancy.Tests;
    using Nancy.ViewEngines.SuperSimpleViewEngine;

    using Xunit;

    public class MarkdownViewEngineFixture
    {
        private readonly MarkDownViewEngine viewEngine;
        private readonly IRenderContext renderContext;
        private readonly IRootPathProvider rootPathProvider;
        private readonly FileSystemViewLocationProvider fileSystemViewLocationProvider;

        public MarkdownViewEngineFixture()
        {
            this.renderContext = A.Fake<IRenderContext>();
            this.viewEngine = new MarkDownViewEngine(new SuperSimpleViewEngine(Enumerable.Empty<ISuperSimpleViewEngineMatcher>()));

            this.rootPathProvider = A.Fake<IRootPathProvider>();

            A.CallTo(() => this.rootPathProvider.GetRootPath()).Returns(Path.Combine(Environment.CurrentDirectory, "Markdown"));

            this.fileSystemViewLocationProvider = new FileSystemViewLocationProvider(this.rootPathProvider, new DefaultFileSystemReader());
        }

        [Fact]
        public async Task Should_return_Markdown()
        {
            // Given
            const string markdown = @"#Header1
##Header2
###Header3
Hi there!
> This is a blockquote.";

            var location = new ViewLocationResult(
                string.Empty,
                string.Empty,
                "md",
                () => new StringReader(markdown)
            );

            var html = await MarkDownViewEngine.ConvertMarkdown(location);

            var stream = new MemoryStream();

            A.CallTo(() => this.renderContext.ViewCache.GetOrAdd(location, A<Func<ViewLocationResult, Task<string>>>.Ignored))
             .Returns(html);

            // When
            var response = await this.viewEngine.RenderView(location, null, this.renderContext);
            await response.Contents.Invoke(stream, CancellationToken.None);

            // Then
            var result = await ReadAll(stream);
            result.ShouldEqual(html);
        }

        [Fact]
        public async Task Should_use_masterpage()
        {
            //Given, When
            var result = await SetupCallAndReadViewWithMasterPage();

            //Then
            result.ShouldContain("What does Samuel L Jackson think?");
        }

        [Fact]
        public async Task Should_render_model()
        {
            //Given, When
            var result = await SetupCallAndReadViewWithMasterPage(useModel: true);

            //Then
            result.ShouldContain("My name is Vincent Vega and I come from the model");
        }

        [Fact]
        public async Task Should_handle_script_tags_before_body_tag()
        {
            //Given, When
            const string expected = @"<script type='text/javascript' src='http://code.jquery.com/jquery-latest.min.js'></script>";

            var result = await SetupCallAndReadViewWithMasterPage();

            //Then
            result.ShouldContain(expected);
        }

        [Fact]
        public async Task Should_convert_markdown_in_master()
        {
            //Given, When
            var result = await SetupCallAndReadViewWithMasterPage();

            //Then
            result.ShouldContain("<h1>Markdown Engine Demo</h1>");
        }

        [Fact]
        public async Task Should_convert_partial_views_with_markdown_content()
        {
            //Given, When
            var result = await SetupCallAndReadViewWithMasterPage();

            //Then
            result.ShouldContain("<h4>This is from a partial</h4>");
        }

        [Fact]
        public async Task Should_convert_standalone()
        {
            var location = FindView("standalone");

            var result = await MarkDownViewEngine.ConvertMarkdown(location);

            Assert.True(result.StartsWith("<!DOCTYPE html>"));
        }

        [Fact]
        public async Task Should_convert_view()
        {
            var location = FindView("home");
            var result = await MarkDownViewEngine.ConvertMarkdown(location);

            result.ShouldStartWith("@Master['master']");
        }

        [Fact]
        public async Task Should_convert_standalone_view_with_no_master()
        {
            var location = FindView("standalone");

            var html = await MarkDownViewEngine.ConvertMarkdown(location);

            A.CallTo(() => this.renderContext.ViewCache.GetOrAdd(location, A<Func<ViewLocationResult, Task<string>>>.Ignored))
             .Returns(html);

            var stream = new MemoryStream();

            var response = await this.viewEngine.RenderView(location, null, this.renderContext);

            await response.Contents.Invoke(stream, CancellationToken.None);

            var result = await ReadAll(stream);

            Assert.True(result.StartsWith("<!DOCTYPE html>"));
        }

        [Fact]
        public async Task Should_be_able_to_use_HTML_MasterPage()
        {
            var location = FindView("viewwithhtmlmaster");

            var masterLocation = FindView("htmlmaster");

            var html = await MarkDownViewEngine.ConvertMarkdown(location);

            A.CallTo(() => this.renderContext.ViewCache.GetOrAdd(location, A<Func<ViewLocationResult, Task<string>>>.Ignored))
             .Returns(html);

            A.CallTo(() => this.renderContext.LocateView("htmlmaster", A<object>.Ignored)).Returns(masterLocation);

            var stream = new MemoryStream();


            var response = await this.viewEngine.RenderView(location, null, this.renderContext);

            await response.Contents.Invoke(stream, CancellationToken.None);

            var result = await ReadAll(stream);

            result.ShouldStartWith("<!DOCTYPE html>");
            result.ShouldContain("<p>Bacon ipsum dolor");
        }

        private async Task<string> SetupCallAndReadViewWithMasterPage(bool useModel = false)
        {
            var location = FindView("home");

            var masterLocation = FindView("master");

            var partialLocation = FindView("partial");

            var html = await MarkDownViewEngine.ConvertMarkdown(location);

            A.CallTo(() => this.renderContext.ViewCache.GetOrAdd(location, A<Func<ViewLocationResult, Task<string>>>.Ignored))
             .Returns(html);

            A.CallTo(() => this.renderContext.LocateView("master", A<object>.Ignored)).Returns(masterLocation);

            A.CallTo(() => this.renderContext.LocateView("partial", A<object>.Ignored)).Returns(partialLocation);

            var stream = new MemoryStream();

            var model = useModel ? new UserModel("Vincent", "Vega") : null;

            var response = await this.viewEngine.RenderView(location, model, this.renderContext);

            await response.Contents.Invoke(stream, CancellationToken.None).ConfigureAwait(false);

            return await ReadAll(stream);
        }

        private static async Task<string> ReadAll(Stream stream)
        {
            stream.Position = 0;

            using (var reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }

        private ViewLocationResult FindView(string viewName)
        {
            var location = this.fileSystemViewLocationProvider.GetLocatedViews(new[] { "md", "markdown", "html" }).First(r => r.Name == viewName);
            return location;
        }
    }
}
