﻿using BilgeShop.Business.Dtos;
using BilgeShop.Business.Services;
using BilgeShop.Data.Entities;
using BilgeShop.Data.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilgeShop.Business.Managers
{
    public class ProductManager : IProductService
    {
        // private -> yalnızca bu class içerisinde tanınır.
        // readonly -> ataması yalnızca constructor'da yapılabilir.
        private readonly IRepository<ProductEntity> _productRepository;
        private readonly IRepository<CategoryEntity> _categoryRepository;
        public ProductManager(IRepository<ProductEntity> productRepository, IRepository<CategoryEntity> categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }
        public bool AddProduct(AddProductDto addProductDto)
        {
            var hasProduct = _productRepository.GetAll(x => x.Name.ToLower() == addProductDto.Name.ToLower()).ToList();

            if (hasProduct.Any())
            {
                return false;
            }

            var productEntity = new ProductEntity()
            {
                Name = addProductDto.Name,
                Description = addProductDto.Description,
                UnitInStock = addProductDto.UnitInStock,
                UnitPrice = addProductDto.UnitPrice,
                CategoryId = addProductDto.CategoryId,
                ImagePath = addProductDto.ImagePath
            };

            _productRepository.Add(productEntity);
            return true;


        }

        public void DeleteProduct(int id)
        {
            _productRepository.Delete(id);
        }

        public void EditProduct(EditProductDto editProductDto)
        {
            var productEntity = _productRepository.GetById(editProductDto.Id);
            // id ile eşleşen nesnenin tamamını yakaladım.

            productEntity.Name = editProductDto.Name;
            productEntity.Description = editProductDto.Description;
            productEntity.UnitPrice = editProductDto.UnitPrice;
            productEntity.UnitInStock = editProductDto.UnitInStock;
            productEntity.CategoryId = editProductDto.CategoryId;

            if(editProductDto.ImagePath is not null)
            {
                productEntity.ImagePath = editProductDto.ImagePath;
            }
            // Bu If'i yazmazsam, editProductDto aracılığı ile gelen null olan ImagePath bilgisi, veritabanındaki görsel bilgisi üzerine atanır; Böylelikle elimde olan görseli silmiş olurum --> BUNU ISTEMIYORUM!

            _productRepository.Update(productEntity);

        }

        public EditProductDto GetProductById(int id)
        {
            var productEntity = _productRepository.GetById(id);

            var editProductDto = new EditProductDto()
            {
                Id = productEntity.Id,
                Name = productEntity.Name,
                Description = productEntity.Description,
                UnitInStock = productEntity.UnitInStock,
                UnitPrice = productEntity.UnitPrice,
                CategoryId = productEntity.CategoryId,
                ImagePath = productEntity.ImagePath
            };

            return editProductDto;

        }

        public ProductDetailDto GetProductDetailById(int id)
        {
            var productEntity = _productRepository.GetById(id);

            var productDetailDto = new ProductDetailDto()
            {
                ProductId = productEntity.Id,
                ProductName = productEntity.Name,
                Description = productEntity.Description,
                UnitInStock = productEntity.UnitInStock,
                UnitPrice = productEntity.UnitPrice,
                ImagePath = productEntity.ImagePath,
                CategoryId = productEntity.CategoryId,
                CategoryName = _categoryRepository.GetById(productEntity.CategoryId).Name
            };

            // Select ile işlem yaparken tablolar arası sıçrama yapabilirsin. Tek çekim yaparken ilgili servisi çağırıp veriyi çek.


            return productDetailDto;


        }

        public List<ListProductDto> GetProducts()
        {
            var productEntites = _productRepository.GetAll().OrderBy(x => x.Category.Name).ThenBy(x => x.Name);
            // Önce kategori adına göre, ardından ürün adına göre sıralıyor.

            var productDtoList = productEntites.Select(x => new ListProductDto
            {
                Id = x.Id,
                Name = x.Name,
                UnitPrice = x.UnitPrice,
                UnitInStock = x.UnitInStock,
                CategoryId = x.CategoryId,
                CategoryName = x.Category.Name,
                ImagePath = x.ImagePath
            }).ToList();

            return productDtoList;
        }

        public List<ListProductDto> GetProductsByCategoryId(int? categoryId)
        {
           
            if(categoryId.HasValue) // is not null
            {
                var productEntites = _productRepository.GetAll(x => x.CategoryId == categoryId).OrderBy(x => x.Name);

                var productDtos = productEntites.Select(x => new ListProductDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    UnitInStock = x.UnitInStock,
                    UnitPrice = x.UnitPrice,
                    CategoryId = x.CategoryId,
                    CategoryName = x.Category.Name,
                    ImagePath = x.ImagePath
                }).ToList();

                return productDtos;
            }


            return GetProducts();
            // GetProducts metodunu çalıştır. O metot zaten categoryId'den bağımsız bir şekilde bütün ürün bilgilierini geriye dönüyor.
        }
    }
}
