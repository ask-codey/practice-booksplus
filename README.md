# Practice: BooksPlus

Practice code for the lesson **Practice: BooksPlus** in the C# Full-Stack Bootcamp on [ask-codey.com](https://www.ask-codey.com).

You'll turn a new ASP.NET Core MVC project into a small bookstore website for the **BooksPlus** database: a home page with featured books, a list of every book, a details page, a search page and a form that adds a book.

## Before you start

The tests start your website and read your own BooksPlus database, so:

1. SQL Server must be running on your computer.
2. The BooksPlus database must exist. If you haven't created it yet, run `BooksPlus.sql` from the course's sample databases in SSMS.

The connection string is in `BooksPlusWeb/appsettings.json`. It uses Windows Authentication, so there's no password in it.

## What's here

- `BooksPlusWeb`: the project `dotnet new mvc` makes, with Bootstrap loaded from a CDN. The `HomeController` and its views are still the template's.
- `BooksPlusWeb/Models`: the `Book` and `Category` models, ready to use.
- `BooksPlusWeb/Data/BookDao.cs`: the data access, ready to use. Read the comment above each method to see what it returns.
- `BooksPlusWeb.Tests`: tests that request your pages the way a browser would. You don't need to change these.

## What to do

1. **Home page.** Change `HomeController.Index` to pass the five newest books (`GetFeatured(5)`) to its view. Under a **Featured Books** heading, show them in a Bootstrap carousel: each slide shows the title, the price and a link to the book's details page.
2. **Books.** Add a `BooksController`. `/Books` shows every book in a table, each title a link to `/Books/Details/{id}`.
3. **Details.** `/Books/Details/{id}` shows one book: its title, its publish date, its price as money (`$36.58`) and its summary. A book that doesn't exist is **Not Found** (`return NotFound();`).
4. **Search.** `/Books/Search?title=SQL` shows a search box (a form with `method="get"`) and, under it, the books whose titles contain the text.
5. **Add a book.** `/Books/Create` shows a form (`method="post"`) for a new book's title, category (a drop-down of every category), publish date, price and summary. Posting it adds the book and redirects to the new book's details page.
6. **Menu.** Add Books, Search and Add a Book links to the menu in `_Layout.cshtml`.

Run the website with **Ctrl+F5** and try every page in the browser. Run the tests with **Test > Run All Tests** (or `dotnet test`): each page you finish turns its tests green.

## Tips

- Give each controller the connection string the way `appsettings.json` names it: take `IConfiguration` in the constructor, then `new BookDao(configuration.GetConnectionString("BooksPlus")!)`.
- Pass a list to a view with `return View(books);` and start the view with `@model List<Book>`.
- Use tag helpers for links (`asp-action="Details" asp-route-id="@book.BookId"`) instead of typing addresses.
- Write the form with `asp-for` on each input: it fills in the name, the type and the value for you. A form tag helper with `method="post"` adds the anti-forgery token; put `[ValidateAntiForgeryToken]` on the POST action.
- Mark the two `Create` actions `[HttpGet]` and `[HttpPost]`. The POST one takes a `Book`, and model binding fills it from the form.
- The test that adds a book deletes it again afterward, so your database stays as it was.

## Hand it in

When it works, push your solution to a public repo of your own on GitHub, then paste that repo's address into **Submit your exercise** on the lesson page at [ask-codey.com](https://www.ask-codey.com). That's what completes the lesson. The lesson page shows the Git commands, step by step.
