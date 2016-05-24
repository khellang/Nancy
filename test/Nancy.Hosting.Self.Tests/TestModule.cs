namespace Nancy.Hosting.Self.Tests
{
    using System;
    using System.IO;

    public class TestModule : NancyModule
    {
        public TestModule()
        {
        	Get("/", args => "This is the site home");

            Get("/rel", args => "This is the site route");

            Get("/rel/header", args =>
            {
                var response = new Response();
                response.Headers["X-Some-Header"] = "Some value";

                return response;
            });

            Post("/rel", args => new StreamReader(this.Request.Body).ReadToEnd());

        	Get("/exception", args =>
        	{
        	    return new Response
        	    {
        	        Contents = async (s, ct) =>
        	        {
        	            using (var writer = new StreamWriter(s))
                        {
                            await writer.WriteAsync("Content");
                            await writer.FlushAsync();

                            throw new Exception("An error occured during content rendering");
                        }
        	        }
        	    };
        	});
        }
    }
}
