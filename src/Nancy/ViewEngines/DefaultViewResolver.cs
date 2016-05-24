﻿namespace Nancy.ViewEngines
{
    using System;
    using System.Threading.Tasks;
    using Nancy.Conventions;

    /// <summary>
    /// Default implementation on how views are resolved by Nancy.
    /// </summary>
    public class DefaultViewResolver : IViewResolver
    {
        private readonly ViewLocationConventions conventions;
        private readonly IViewLocator viewLocator;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultViewResolver"/> class.
        /// </summary>
        /// <param name="viewLocator">The view locator that should be used to locate views.</param>
        /// <param name="conventions">The conventions that the view resolver should use to figure out where to look for views.</param>
        public DefaultViewResolver(IViewLocator viewLocator, ViewLocationConventions conventions)
        {
            if (viewLocator == null)
            {
                throw new InvalidOperationException("Cannot create an instance of DefaultViewResolver with view locator parameter having null value.");
            }

            if (conventions == null)
            {
                throw new InvalidOperationException("Cannot create an instance of DefaultViewResolver with conventions parameter having null value.");
            }

            this.viewLocator = viewLocator;
            this.conventions = conventions;
        }

        /// <summary>
        /// Locates a view based on the provided information.
        /// </summary>
        /// <param name="viewName">The name of the view to locate.</param>
        /// <param name="model">The model that will be used with the view.</param>
        /// <param name="viewLocationContext">A <see cref="ViewLocationContext"/> instance, containing information about the context for which the view is being located.</param>
        /// <returns>A <see cref="ViewLocationResult"/> instance if the view could be found, otherwise <see langword="null"/>.</returns>
        public Task<ViewLocationResult> GetViewLocation(string viewName, object model, ViewLocationContext viewLocationContext)
        {
            var nullResult = Task.FromResult<ViewLocationResult>(null);

            if (string.IsNullOrEmpty(viewName))
            {
                return nullResult;
            }

            if (viewLocationContext == null)
            {
                return nullResult;
            }

            viewLocationContext.Context.Trace.TraceLog.WriteLog(x => x.AppendLine(string.Concat("[DefaultViewResolver] Resolving view for '", viewName , "', using view location conventions.")));

            foreach (var convention in conventions)
            {
                var conventionBasedViewName =
                    SafeInvokeConvention(convention, viewName, model, viewLocationContext);

                if (string.IsNullOrEmpty(conventionBasedViewName))
                {
                    continue;
                }

                viewLocationContext.Context.Trace.TraceLog.WriteLog(x => x.AppendLine(string.Concat("[DefaultViewResolver] Attempting to locate view using convention '", conventionBasedViewName, "'")));

                var locatedView =
                    this.viewLocator.LocateView(conventionBasedViewName, viewLocationContext.Context);

                if (locatedView != null)
                {
                    viewLocationContext.Context.Trace.TraceLog.WriteLog(x => x.AppendLine(string.Concat("[DefaultViewResolver] View resolved at '", conventionBasedViewName, "'")));
                    return Task.FromResult(locatedView);
                }
            }

            viewLocationContext.Context.Trace.TraceLog.WriteLog(x => x.AppendLine("[DefaultViewResolver] No view could be resolved using the available view location conventions."));

            return nullResult;
        }

        private static string SafeInvokeConvention(Func<string, object, ViewLocationContext, string> convention, string viewName, dynamic model, ViewLocationContext viewLocationContext)
        {
            try
            {
                return convention.Invoke(viewName, model, viewLocationContext);
            }
            catch
            {
                return null;
            }
        }
    }
}
