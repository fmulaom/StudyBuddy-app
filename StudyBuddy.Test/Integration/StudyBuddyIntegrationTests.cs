//using System.Net;
//using Microsoft.AspNetCore.Hosting;
//using FluentAssertions;
//using Microsoft.AspNetCore.Mvc.Testing;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.VisualStudio.TestPlatform.TestHost;
//using Xunit;

//namespace StudyBuddy.Web.Tests.Integration
//{
//    public class CustomWebApplicationFactory
//        : WebApplicationFactory<Program>
//    {
//        protected override void ConfigureWebHost(IWebHostBuilder builder)
//        {
//            builder.ConfigureServices(services =>
//            {
//                // Ovdje kasnije možeš:
//                // - zamijeniti bazu s InMemory
//                // - mockati servise
//                // Trenutno ostavljamo default radi render testova
//            });
//        }
//    }

//    public class StudyBuddyIntegrationTests
//        : IClassFixture<CustomWebApplicationFactory>
//    {
//        private readonly HttpClient _client;

//        public StudyBuddyIntegrationTests(CustomWebApplicationFactory factory)
//        {
//            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
//            {
//                AllowAutoRedirect = true
//            });
//        }

//        // -------------------------
//        // STUDY TASKS
//        // -------------------------

//        [Fact]
//        public async Task StudyTasks_Index_ReturnsOk()
//        {
//            var response = await _client.GetAsync("/StudyTasks");

//            response.StatusCode.Should().Be(HttpStatusCode.OK);

//            var html = await response.Content.ReadAsStringAsync();
//            html.Should().Contain("Study Tasks");
//        }

//        // -------------------------
//        // STUDY GROUPS
//        // -------------------------

//        [Fact]
//        public async Task StudyGroups_Index_ReturnsOk()
//        {
//            var response = await _client.GetAsync("/StudyGroups");

//            response.StatusCode.Should().Be(HttpStatusCode.OK);

//            var html = await response.Content.ReadAsStringAsync();
//            html.Should().Contain("Study Groups");
//        }

//        [Fact]
//        public async Task StudyGroups_Details_ReturnsOk()
//        {
//            // koristi ID koji sigurno postoji
//            var response = await _client.GetAsync("/StudyGroups/Details/1");

//            response.StatusCode.Should().Be(HttpStatusCode.OK);

//            var html = await response.Content.ReadAsStringAsync();
//            html.Should().Contain("Group");
//        }

//        [Fact]
//        public async Task StudyGroups_Members_ViewRenders()
//        {
//            var response = await _client.GetAsync("/StudyGroups/Members/1");

//            response.StatusCode.Should().Be(HttpStatusCode.OK);

//            var html = await response.Content.ReadAsStringAsync();
//            html.Should().Contain("Members");
//        }

//        // -------------------------
//        // SCHEDULE
//        // -------------------------

//        [Fact]
//        public async Task Schedule_Index_ReturnsOk()
//        {
//            var response = await _client.GetAsync("/Schedule");

//            response.StatusCode.Should().Be(HttpStatusCode.OK);

//            var html = await response.Content.ReadAsStringAsync();
//            html.Should().Contain("Schedule");
//        }

//        // -------------------------
//        // PARTNER MATCHING
//        // -------------------------

//        [Fact]
//        public async Task PartnerMatching_Index_ReturnsOk()
//        {
//            var response = await _client.GetAsync("/PartnerMatching");

//            response.StatusCode.Should().Be(HttpStatusCode.OK);

//            var html = await response.Content.ReadAsStringAsync();
//            html.Should().Contain("Partner");
//        }

//        [Fact]
//        public async Task PartnerMatching_Search_ReturnsOk()
//        {
//            var response = await _client.GetAsync("/PartnerMatching/Search");

//            response.StatusCode.Should().Be(HttpStatusCode.OK);

//            var html = await response.Content.ReadAsStringAsync();
//            html.Should().Contain("Search");
//        }

//        // -------------------------
//        // LEARNING GOALS
//        // -------------------------

//        [Fact]
//        public async Task LearningGoals_Index_ReturnsOk()
//        {
//            var response = await _client.GetAsync("/LearningGoals");

//            response.StatusCode.Should().Be(HttpStatusCode.OK);

//            var html = await response.Content.ReadAsStringAsync();
//            html.Should().Contain("Learning Goals");
//        }

//        // -------------------------
//        // SHARED / LAYOUT
//        // -------------------------

//        [Fact]
//        public async Task Layout_IsRendered_ForAnyPage()
//        {
//            var response = await _client.GetAsync("/StudyTasks");

//            var html = await response.Content.ReadAsStringAsync();

//            // provjera layout elemenata
//            html.Should().Contain("<header");
//            html.Should().Contain("<footer");
//        }

//        // -------------------------
//        // LOGIN PARTIAL
//        // -------------------------

//        [Fact]
//        public async Task LoginPartial_IsRendered()
//        {
//            var response = await _client.GetAsync("/");

//            var html = await response.Content.ReadAsStringAsync();

//            html.Should().Contain("Login");
//        }
//    }
//}
