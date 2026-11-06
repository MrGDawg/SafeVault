using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SafeVault.Controllers
{
    /// <summary>
    /// Provides administrative functionality that is restricted
    /// to authenticated users assigned to the admin role.
    /// </summary>
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        /// <summary>
        /// Displays the SafeVault administrative dashboard.
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}