using J00rStore.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using J00rStore.Data;
using Microsoft.EntityFrameworkCore;

namespace J00rStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDBContext _dbContext;

		public HomeController(ILogger<HomeController> logger, AppDBContext appDbContext)
        {
            _logger = logger;
            _dbContext = appDbContext;
        }

        public async Task<IActionResult> Index()
        {
			var products = await _dbContext.Products.ToListAsync();
			return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> Details(int id)
        {
	        var product = await _dbContext.Products.FirstOrDefaultAsync(q => q.Id == id);
            
	        return View(product);
        }

    }
}