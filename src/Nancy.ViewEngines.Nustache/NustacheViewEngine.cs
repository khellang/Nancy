namespace Nancy.ViewEngines.Nustache
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using global::Nustache.Core;
    using Nancy.Helpers;
    using Nancy.Responses;

    /// <summary>
    /// View engine for rendering nustache views.
    /// </summary>
    public class NustacheViewEngine : IViewEngine
    {
        /// <summary>
        /// Gets the extensions file extensions that are supported by the view engine.
        /// </summary>
        /// <value>An <see cref="IEnumerable{T}"/> instance containing the extensions.</value>
        /// <remarks>The extensions should not have a leading dot in the name.</remarks>
        public IEnumerable<string> Extensions
        {
            get { return new[] { "nustache" }; }
        }

        /// <summary>
        /// Initialise the view engine (if necessary)
        /// </summary>
        /// <param name="viewEngineStartupContext">Startup context</param>
        public void Initialize(ViewEngineStartupContext viewEngineStartupContext)
        {
        }

        private async Task<Template> GetOrCompileTemplate(ViewLocationResult viewLocationResult, IRenderContext renderContext)
        {
            var viewFactory = await renderContext.ViewCache.GetOrAdd(
                viewLocationResult,
                x =>
                {
                    using (var reader = x.Contents.Invoke())
                    {
                        return Task.FromResult(this.GetCompiledTemplate<dynamic>(reader));
                    }
                });

            var view = viewFactory.Invoke();

            return view;
        }

        private Func<Template> GetCompiledTemplate<TModel>(TextReader reader)
        {
            var template = new Template();
            template.Load(reader);

            return () => template;
        }

        /// <summary>
        /// Renders the view.
        /// </summary>
        /// <param name="viewLocationResult">A <see cref="ViewLocationResult"/> instance, containing information on how to get the view template.</param>
        /// <param name="model">The model that should be passed into the view</param>
        /// <param name="renderContext"></param>
        /// <returns>A response</returns>
        public Task<Response> RenderView(ViewLocationResult viewLocationResult, object model, IRenderContext renderContext)
        {
            return Task.FromResult<Response>(new HtmlResponse
            {
                Contents = async (stream, ct) =>
                {
                    var template =
                        await this.GetOrCompileTemplate(viewLocationResult, renderContext);

                    var writer =
                        new StreamWriter(stream);

                    template.Render(model, writer,
                        new TemplateLocator(name => this.GetPartial(renderContext, name, model)));
                }
            });
        }

        private Template GetPartial(IRenderContext renderContext, string name, dynamic model)
        {
            var view = renderContext.LocateView(name, model);
            return this.GetOrCompileTemplate(view, renderContext);
        }
    }
}
