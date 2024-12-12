using Microsoft.AspNetCore.Mvc;
using BaseLibrary.Entities;

namespace WebClient.Controllers
{
    public class BooksController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl = "https://localhost:7249/api/Books";

        public BooksController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // GET: Books
        public async Task<IActionResult> Index()
        {
            var books = await _httpClient.GetFromJsonAsync<IEnumerable<Book>>(_apiUrl);
            return View(books);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Author,PublishedDate,Genre")] Book book)
        {
            if (ModelState.IsValid)
            {
                var response = await _httpClient.PostAsJsonAsync(_apiUrl, book);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, "Unable to create book.");
            }
            return View(book);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var book = await _httpClient.GetFromJsonAsync<Book>($"{_apiUrl}/{id}");
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Author,PublishedDate,Genre")] Book book)
        {
            if (id != book.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                var response = await _httpClient.PutAsJsonAsync($"{_apiUrl}/{id}", book);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, "Unable to update book.");
            }
            return View(book);
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var book = await _httpClient.GetFromJsonAsync<Book>($"{_apiUrl}/{id}");
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_apiUrl}/{id}");
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            return View();
        }
    }
}
