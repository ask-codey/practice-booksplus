using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;

namespace BooksPlusWeb.Tests;

// These tests start your whole web app in memory and request its pages, the
// way a browser would. The app reads your own BooksPlus database, so SQL Server
// must be running and BooksPlus created. You don't need to change this file.
[TestClass]
public class SiteTests
{
    private static WebApplicationFactory<Program> factory = null!;
    private static HttpClient client = null!;

    // The BOOKSPLUS_CONNECTION environment variable, if set, points the tests
    // at another database; otherwise the app uses its own appsettings.json.
    private static string? OtherDatabase => Environment.GetEnvironmentVariable("BOOKSPLUS_CONNECTION");

    [ClassInitialize]
    public static void StartTheApp(TestContext context)
    {
        factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            if (OtherDatabase is not null)
            {
                builder.UseSetting("ConnectionStrings:BooksPlus", OtherDatabase);
            }
        });
        client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    [ClassCleanup]
    public static void StopTheApp()
    {
        client.Dispose();
        factory.Dispose();
    }

    private static async Task<string> GetPage(string path)
    {
        HttpResponseMessage response = await client.GetAsync(path);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, $"GET {path} should show a page.");
        return await response.Content.ReadAsStringAsync();
    }

    [TestMethod]
    public async Task HomePage_ShowsTheFeaturedBooks()
    {
        string html = await GetPage("/");

        Assert.Contains("Featured Books", html, "The home page has a Featured Books heading.");
        Assert.Contains("Pro SQL Server on Linux", html, "The newest book is featured.");
        Assert.Contains("Video Production Handbook", html, "The fifth-newest book is featured.");
    }

    [TestMethod]
    public async Task BooksPage_ListsEveryBook_WithLinksToTheirDetails()
    {
        string html = await GetPage("/Books");

        Assert.Contains("Fast Track to MDX", html);
        Assert.Contains("SQL Server 2012 Programming", html);
        Assert.Contains("/Books/Details/21", html, "Each title links to its details page.");
    }

    [TestMethod]
    public async Task DetailsPage_ShowsOneBook()
    {
        string html = await GetPage("/Books/Details/21");

        Assert.Contains("Fast Track to MDX", html);
        Assert.Contains("$36.58", html, "The details page shows the price as money.");
    }

    [TestMethod]
    public async Task DetailsPage_IsNotFound_ForABookThatDoesNotExist()
    {
        await GetPage("/Books/Details/21");
        HttpResponseMessage response = await client.GetAsync("/Books/Details/9999");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode, "There's no book 9999, so the page is Not Found (404).");
    }

    [TestMethod]
    public async Task SearchPage_ShowsOnlyMatchingBooks()
    {
        string html = await GetPage("/Books/Search?title=Program");

        Assert.Contains("SQL Server 2012 Programming", html, "Its title contains Program.");
        Assert.DoesNotContain("Fast Track to MDX", html, "Its title doesn't contain Program.");
    }

    [TestMethod]
    public async Task CreatePage_ShowsAFormForANewBook()
    {
        string html = await GetPage("/Books/Create");

        Assert.Contains("<form", html, "The page has a form.");
        Assert.Contains("name=\"Title\"", html, "The form has a Title field.");
        Assert.Contains("Database Administration", html, "The form lists the categories to choose from.");
    }

    [TestMethod]
    public async Task PostingTheForm_AddsTheBook_AndRedirectsToItsDetails()
    {
        // Read the form first, for the anti-forgery token it carries.
        string form = await GetPage("/Books/Create");
        Match token = Regex.Match(form, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"");
        Assert.IsTrue(token.Success, "The form includes an anti-forgery token (a form tag helper with method=\"post\" adds it).");

        HttpResponseMessage response = await client.PostAsync("/Books/Create", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token.Groups[1].Value,
            ["Title"] = "A Test Book",
            ["CategoryId"] = "1",
            ["PublishDate"] = "2026-01-15",
            ["Price"] = "12.50"
        }));

        try
        {
            Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode, "After saving, the app redirects (Post-Redirect-Get).");
            string location = response.Headers.Location!.ToString();
            StringAssert.StartsWith(location, "/Books/Details/", "It redirects to the new book's details page.");

            string details = await GetPage(location);
            Assert.Contains("A Test Book", details, "The new book can be seen on its details page.");
        }
        finally
        {
            DeleteTestBooks();
        }
    }

    // Removes the books the test added, so the database is as it was.
    private static void DeleteTestBooks()
    {
        string connectionString = OtherDatabase ?? "Server=localhost;Database=BooksPlus;Integrated Security=true;TrustServerCertificate=true;";
        using SqlConnection connection = new SqlConnection(connectionString);
        connection.Open();
        using SqlCommand command = new SqlCommand("DELETE FROM Book WHERE Title = 'A Test Book';", connection);
        command.ExecuteNonQuery();
    }
}
