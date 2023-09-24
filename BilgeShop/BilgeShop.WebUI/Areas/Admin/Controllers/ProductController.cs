using BilgeShop.Business.Dtos;
using BilgeShop.Business.Services;
using BilgeShop.WebUI.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace BilgeShop.WebUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly IWebHostEnvironment _environment;
        
        public ProductController(ICategoryService categoryService, IProductService productService, IWebHostEnvironment environment)
        {
            _categoryService = categoryService;
            _productService = productService;
            _environment = environment;
        }
        public IActionResult List()
        {
            var productDtoList = _productService.GetProducts();

            // Select ile bir tür listeden diğer tür listeye çeviriyorum.
            // Her bir elemanı için yeni bir ListProductViewModel açılıp veriler aktarılıyor.
            var viewModel = productDtoList.Select(x => new ListProductViewModel
            {
                Id = x.Id,
                Name = x.Name,
                CategoryId = x.CategoryId,
                CategoryName = x.CategoryName,
                UnitInStock = x.UnitInStock,
                UnitPrice = x.UnitPrice,
                ImagePath = x.ImagePath
            }).ToList();

            return View(viewModel);
        }

        public IActionResult New()
        {
        
          
            ViewBag.Categories = _categoryService.GetCategories();
            return View("Form" , new ProductFormViewModel());
            
        }

        public IActionResult Edit(int id)
        {
            var editProductDto = _productService.GetProductById(id);

            var viewModel = new ProductFormViewModel()
            {
                Id = editProductDto.Id,
                Name = editProductDto.Name,
                Description = editProductDto.Description,
                UnitInStock = editProductDto.UnitInStock,
                UnitPrice = editProductDto.UnitPrice,
                CategoryId = editProductDto.CategoryId
            };

            // eski görseli ekranda görmek istiyorum. O yüzden viewbag ile ilgili viewe gönderiyorum.
            ViewBag.ImagePath = editProductDto.ImagePath;
            ViewBag.Categories = _categoryService.GetCategories();
            return View("form" , viewModel);
            
        }


        [HttpPost]
        public IActionResult Save(ProductFormViewModel formData)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _categoryService.GetCategories();
                return View("Form", formData);
            }

            var newFileName = "";

            if(formData.File is not null) // bir görsel gönderildiyse
            {

                var allowedFileTypes = new string[] { "image/jpeg", "image/jpg", "image/png", "image/jfif" };
                // izin vereceğim dosya türleri.

                var allowedFileExtensions = new string[] { ".jpg", ".jpeg", ".png", ".jfif" };
                // izin vereceğim dosya uzantıları.

                var fileContentType = formData.File.ContentType; //dosyanın içerik tipi.

                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(formData.File.FileName); // uzantısız dosya ismi.

                var fileExtension = Path.GetExtension(formData.File.FileName); // uzantı.

                if(!allowedFileTypes.Contains(fileContentType) ||
                    !allowedFileExtensions.Contains(fileExtension))
                {
                    ViewBag.FileError = "Dosya formatı veya içeriği hatalı";


                    ViewBag.Categories = _categoryService.GetCategories();
                    return View("Form", formData);

                }

                newFileName = fileNameWithoutExtension + "-" + Guid.NewGuid() + fileExtension;
                // Aynı isimde iki dosya yüklenildiğinde hata vermesin, birbiriyle asla eşleşmeyecek şekilde her dosya adına unique(eşsiz) bir metin ilavesi yapıyorum.

                var folderPath = Path.Combine("images", "products");
                // images/products

                var wwwrootFolderPath = Path.Combine(_environment.WebRootPath, folderPath);
                // ...wwwroot/images/products

                var wwwrootFilePath = Path.Combine(wwwrootFolderPath, newFileName);
                // ...wwwroot/images/products/urunGorseli-12312312.jpg

                Directory.CreateDirectory(wwwrootFolderPath); // Eğer images/products yoksa, oluştur.

                using (var fileStream = new FileStream(
                    wwwrootFilePath, FileMode.Create))
                {
                    formData.File.CopyTo(fileStream);
                }
                // asıl dosyayı kopyaladığım kısım.

                // using içerisinde new'lenen filestream nesnesi scope boyunca yaşar, scope bitimi silinir.
            }

            if (formData.Id == 0) // ekleme
            {
                var addProductDto = new AddProductDto()
                {
                    Name = formData.Name.Trim(),
                    Description = formData.Description,
                    UnitPrice = formData.UnitPrice,
                    UnitInStock = formData.UnitInStock,
                    CategoryId = formData.CategoryId,
                    ImagePath = newFileName                
                };

                _productService.AddProduct(addProductDto);
            }
            else // Güncelleme
            {
                var editProductDto = new EditProductDto()
                {
                    Id = formData.Id,
                    Name = formData.Name,
                    Description = formData.Description,
                    UnitInStock = formData.UnitInStock,
                    UnitPrice = formData.UnitPrice,
                    CategoryId = formData.CategoryId
                };

                if(formData.File is not null)
                {
                    editProductDto.ImagePath = newFileName;
                }
                // Bu kontrolü hem controller hem business katmanında yapacağım. Yeni bir dosya seçilmediyse yani null gönderildiyse Db'de olan görselin üzerine yazılmasını istemiyorum.

                _productService.EditProduct(editProductDto);

            }


            return RedirectToAction("List");
        }


        public IActionResult Delete(int id)
        {
            _productService.DeleteProduct(id);

            return RedirectToAction("List");
        }
    }
}


