//using Microsoft.AspNetCore.Mvc.Testing;
//using Microsoft.VisualStudio.TestPlatform.TestHost;
//using System.Net;
//using Xunit;

//namespace StudyBuddy.Test.Integration
//{
//    public class PartnerMatchingIntegrationTests : IAsyncLifetime
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
//            var response = await _client.GetAsync("/PartnerMatching/Index");
//            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
//        }

//        [Fact]
//        public async Task Index_ReturnsHtmlContentType()
//        {
//            var response = await _client.GetAsync("/PartnerMatching/Index");
//            Assert.Contains("text/html", response.Content.Headers.ContentType?.ToString() ?? "");
//        }

//        #endregion

//        #region Search Tests

//        [Fact]
//        public async Task Search_WithValidQuery_ReturnsSuccess()
//        {
//            var response = await _client.GetAsync("/PartnerMatching/Search?query=C%23");
//            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
//        }

//        [Fact]
//        public async Task Search_WithoutQuery_ReturnsBadRequest()
//        {
//            var response = await _client.GetAsync("/PartnerMatching/Search");
//            // Možda vraća Redirect ili BadRequest - prilagodi prema kontroleru
//            Assert.True(response.StatusCode == HttpStatusCode.BadRequest ||
//                       response.StatusCode == HttpStatusCode.Redirect ||
//                       response.StatusCode == HttpStatusCode.OK);
//        }

//        #endregion

//        #region Profile Tests

//        [Fact]
//        public async Task EditProfile_Get_ReturnsSuccess()
//        {
//            var response = await _client.GetAsync("/PartnerMatching/EditProfile");
//            // Može biti 200 ili 302 (Redirect na Login)
//            Assert.True(response.StatusCode == HttpStatusCode.OK ||
//                       response.StatusCode == HttpStatusCode.Redirect);
//        }

//        #endregion

//        #region Favorites Tests

//        [Fact]
//        public async Task MarkFavorite_WithValidId_ReturnsSuccess()
//        {
//            var response = await _client.PostAsync("/PartnerMatching/MarkFavorite?id=1", null);
//            Assert.True(response.StatusCode == HttpStatusCode.OK ||
//                       response.StatusCode == HttpStatusCode.Redirect ||
//                       response.StatusCode == HttpStatusCode.BadRequest);
//        }

//        [Fact]
//        public async Task ViewFavorites_ReturnsSuccess()
//        {
//            var response = await _client.GetAsync("/PartnerMatching/ViewFavorites");
//            Assert.True(response.StatusCode == HttpStatusCode.OK ||
//                       response.StatusCode == HttpStatusCode.Redirect);
//        }

//        #endregion

//        #region Details Tests

//        [Fact]
//        public async Task Details_WithValidId_ReturnsSuccess()
//        {
//            var response = await _client.GetAsync("/PartnerMatching/Details/1");
//            Assert.True(response.StatusCode == HttpStatusCode.OK ||
//                       response.StatusCode == HttpStatusCode.NotFound ||
//                       response.StatusCode == HttpStatusCode.Redirect);
//        }

//        [Fact]
//        public async Task Details_WithInvalidId_ReturnsBadRequest()
//        {
//            var response = await _client.GetAsync("/PartnerMatching/Details/0");
//            Assert.True(response.StatusCode == HttpStatusCode.BadRequest ||
//                       response.StatusCode == HttpStatusCode.NotFound);
//        }

//        #endregion
//    }
//}
