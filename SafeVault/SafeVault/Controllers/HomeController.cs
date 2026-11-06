using Microsoft.AspNetCore.Mvc;
using SafeVault.Data;
using SafeVault.Models;

namespace SafeVault.Controllers
{
    /// <summary>
    /// Handles requests for the SafeVault user-information form.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly SafeVaultDatabase _database;

        /// <summary>
        /// Initializes the controller with the database service supplied
        /// through dependency injection.
        /// </summary>
        public HomeController(SafeVaultDatabase database)
        {
            _database = database;
        }

        /// <summary>
        /// Displays an empty user-information form.
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return View(new UserInputModel());
        }

        /// <summary>
        /// Validates submitted user information and stores valid input.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(UserInputModel model)
        {
            // Invalid input must not reach the data-access layer.
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _database.AddUser(model.Username, model.Email);

            return View("Success", model);
        }
    }
}