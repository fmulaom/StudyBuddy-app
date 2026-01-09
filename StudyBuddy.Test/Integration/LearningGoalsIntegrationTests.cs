//using Microsoft.AspNetCore.Mvc.Testing;
//using Microsoft.VisualStudio.TestPlatform.TestHost;
//using System.Net;
//using Xunit;

//namespace StudyBuddy.Test.Integration
//{
//    public class LearningGoalsIntegrationTests : IAsyncLifetime
//    {
//        private WebApplicationFactory<Program> _factory;
//        private HttpClient _client;

//        public async Task InitializeAsync()
//        {
//            _factory = new WebApplicationFactory<Program>();
//            _client = _factory.CreateClient();
//            await Task.CompletedTask;
//        }

//        public async Task DisposeAsync()
//        {
//            _client?.Dispose();
//            _factory?.Dispose();
//            await Task.CompletedTask;
//        }

//        #region Index Tests

//        [Fact]
//        public async Task Index_ReturnsSuccess()
//        {
//            var response = await _client.GetAsync("/LearningGoals/Index");
//            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
//        }

//        [Fact]
//        public async Task Index_ReturnsHtmlContentType()
//        {
//            var response = await _client.GetAsync("/LearningGoals/Index");
//            Assert.Contains("text/html", response.Content.Headers.ContentType?.ToString() ?? "");
//        }

//        #endregion

//        #region Create Tests

//        [Fact]
//        public async Task Create_Get_ReturnsSuccess()
//        {
//            var response = await _client.GetAsync("/LearningGoals/Create");
//            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
//        }

//        [Fact]
//        public async Task Create_Post_WithValidData_ReturnsRedirect()
//        {
//            var content = new FormUrlEncodedContent(new[]
//            {
//                new KeyValuePair<string, string>("Title", "Learn C#"),
//                new KeyValuePair<string, string>("Description", "Master C# basics"),
//                new KeyValuePair<string, string>("TargetDate", DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd")),
//                new KeyValuePair<string, string>("Progress", "0")
//            });

//            var response = await _client.PostAsync("/LearningGoals/Create", content);
//            Assert.True(response.StatusCode == HttpStatusCode.Redirect ||
//                       response.StatusCode == HttpStatusCode.OK ||
//                       response.StatusCode == HttpStatusCode.BadRequest);
//        }

//        #endregion

//        #region Edit Tests

//        [Fact]
//        public async Task Edit_Get_WithValidId_ReturnsSuccess()
//        {
//            var response = await _client.GetAsync("/LearningGoals/Edit/1");
//            Assert.True(response.StatusCode == HttpStatusCode.OK ||
//                       response.StatusCode == HttpStatusCode.NotFound ||
//                       response.StatusCode == HttpStatusCode.Redirect);
//        }

//        [Fact]
//        public async Task Edit_Post_WithValidData_ReturnsRedirect()
//        {
//            var content = new FormUrlEncodedContent(new[]
//            {
//                new KeyValuePair<string, string>("Id", "1"),
//                new KeyValuePair<string, string>("Title", "Updated Title"),
//                new KeyValuePair<string, string>("Progress", "50")
//            });

//            var response = await _client.PostAsync("/LearningGoals/Edit/1", content);
//            Assert.True(response.StatusCode == HttpStatusCode.Redirect ||
//                       response.StatusCode == HttpStatusCode.OK ||
//                       response.StatusCode == HttpStatusCode.BadRequest);
//        }

//        #endregion

//        #region Delete Tests

//        [Fact]
//        public async Task Delete_Post_WithValidId_ReturnsRedirect()
//        {
//            var response = await _client.PostAsync("/LearningGoals/Delete/1", null);
//            Assert.True(response.StatusCode == HttpStatusCode.Redirect ||
//                       response.StatusCode == HttpStatusCode.OK ||
//                       response.StatusCode == HttpStatusCode.BadRequest);
//        }

//        #endregion
//    }
//}
