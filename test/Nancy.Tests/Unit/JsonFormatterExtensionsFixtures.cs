namespace Nancy.Tests.Unit
{
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Nancy.Configuration;
    using Nancy.Json;
    using Nancy.Responses;
    using Nancy.Tests.Fakes;

    using Xunit;

    public class JsonFormatterExtensionsFixtures
    {
        private readonly IResponseFormatter formatter;
        private readonly Person model;
        private readonly Response response;

        public JsonFormatterExtensionsFixtures()
        {
            var environment = GetTestingEnvironment();
            var serializerFactory =
               new DefaultSerializerFactory(new ISerializer[] { new DefaultJsonSerializer(environment) });

            this.formatter = A.Fake<IResponseFormatter>();
            A.CallTo(() => this.formatter.Environment).Returns(environment);
            A.CallTo(() => this.formatter.SerializerFactory).Returns(serializerFactory);
            this.model = new Person { FirstName = "Andy", LastName = "Pike" };
            this.response = this.formatter.AsJson(model);
        }

        [Fact]
        public void Should_return_a_response_with_the_standard_json_content_type()
        {
            this.response.ContentType.ShouldEqual("application/json; charset=utf-8");
        }

        [Fact]
        public void Should_return_a_response_with_status_code_200_OK()
        {
            this.response.StatusCode.ShouldEqual(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Should_return_a_valid_model_in_json_format()
        {
            using (var stream = new MemoryStream())
            {
                await this.response.Contents(stream, CancellationToken.None);

                Encoding.UTF8.GetString(stream.ToArray()).ShouldEqual("{\"firstName\":\"Andy\",\"lastName\":\"Pike\"}");
            }
        }

        [Fact]
        public async Task Should_return_null_in_json_format()
        {
            var nullResponse = this.formatter.AsJson<Person>(null);
            using (var stream = new MemoryStream())
            {
                await nullResponse.Contents(stream, CancellationToken.None);
                Encoding.UTF8.GetString(stream.ToArray()).ShouldHaveCount(0);
            }
        }

        [Fact]
        public async Task Json_formatter_can_deserialize_objects_of_type_Type()
        {
            var response = this.formatter.AsJson(new { type = typeof(string) });
            using (var stream = new MemoryStream())
            {
                await response.Contents(stream, CancellationToken.None);
                Encoding.UTF8.GetString(stream.ToArray()).ShouldEqual(@"{""type"":""System.String""}");
            }
        }

        [Fact]
        public void Can_set_status_on_json_response()
        {
            var response = this.formatter.AsJson(new { foo = "bar" }, HttpStatusCode.InternalServerError);
            Assert.Equal(response.StatusCode, HttpStatusCode.InternalServerError);
        }

        private static INancyEnvironment GetTestingEnvironment()
        {
            var envionment =
                new DefaultNancyEnvironment();

            envionment.AddValue(JsonConfiguration.Default);

            envionment.Tracing(
                enabled: true,
                displayErrorTraces: true);

            return envionment;
        }
    }
}
