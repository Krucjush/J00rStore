using J00rStore.Data;
using J00rStore.Models;
using J00rStore.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace J00rStore.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDBContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(AppDBContext appDbContext, IWebHostEnvironment iWebHostEnvironment)
        {
            _dbContext = appDbContext;
            _webHostEnvironment = iWebHostEnvironment;
        }
        public async Task<IActionResult> Index()
        {
            var products = await _dbContext.Products.Include("Brand").ToListAsync();
            return View(products);
        }
        public async Task<IActionResult> Create()
        {
            var productVM = new ProductViewModel();
            productVM.Product = new Product();
            var brands = await _dbContext.Brands.ToListAsync();
            var listItems = new List<SelectListItem>();
            foreach (var brand in brands)
            {
                var item = new SelectListItem()
                {
                    Text = brand.Name,
                    Value = brand.Id.ToString()
                };
                listItems.Add(item);
            }
            productVM.Brands = listItems;
            return View(productVM);
        }
        [HttpPost]
        public async Task<IActionResult> Create(ProductViewModel productViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(productViewModel);
            }

            var webRootPath = _webHostEnvironment.WebRootPath;
            if (productViewModel.Product.Image != null)
            {
                string fileName = Guid.NewGuid().ToString();
                var uploads = Path.Combine(webRootPath, @"img\products");
                var extension = Path.GetExtension(productViewModel.Product.Image.FileName);

                if (productViewModel.Product.ImageUrl != null)
                {
                    var path = webRootPath + productViewModel.Product.ImageUrl;
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }

                var fullPath = Path.Combine(uploads, fileName + extension);

                using (var fileStream = new FileStream(fullPath, FileMode.Create))
                {
                    productViewModel.Product.Image.CopyTo(fileStream);
                }

                productViewModel.Product.ImageUrl = @"\img\products" + fileName + extension;
            }

            await _dbContext.Products.AddAsync(productViewModel.Product);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
