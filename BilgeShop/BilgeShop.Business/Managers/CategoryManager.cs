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
    public class CategoryManager : ICategoryService
    {
        private readonly IRepository<CategoryEntity> _categoryRepository;
      
        public CategoryManager(IRepository<CategoryEntity> categoryRepository)
        {
            _categoryRepository = categoryRepository;
            
        }
        public bool AddCategory(AddCategoryDto addCategoryDto)
        {
            
             var hasCategory = _categoryRepository.GetAll(x => x.Name.ToLower() ==  addCategoryDto.Name.ToLower()).ToList();

           if(hasCategory.Any()) // hasCategory != 0 demek
            {
                return false;

            }

            var categoryEntity = new CategoryEntity()
            {
                Name = addCategoryDto.Name,
                Description = addCategoryDto.Description
            };

            _categoryRepository.Add(categoryEntity);

            return true;
        }

        public void DeleteCategory(int id)
        {
            _categoryRepository.Delete(id);

        
            // TODO : CategoryId ile eşleşen bütün ürünler de softDelete edilecek.
        }

        public void EditCategory(EditCategoryDto editCategoryDto)
        {
            var categoryEntity = _categoryRepository.GetById(editCategoryDto.Id);

            categoryEntity.Name = editCategoryDto.Name;
            categoryEntity.Description = editCategoryDto.Description;

            _categoryRepository.Update(categoryEntity);
        }

        public List<ListCategoryDto> GetCategories()
        {
            var categoryEntities = _categoryRepository.GetAll().OrderBy(x => x.Name);

            var categoryDtoList = categoryEntities.Select(x => new ListCategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description
            }).ToList();
            // Her bir entity için ( x ) 1 adet ListCategoryDto nesnesi new'leyip, verileri aktarıyorum.
            // Özetle bir tür listeden diğer tür listeye çeviriyorum.

            return categoryDtoList;

        }

        public EditCategoryDto GetCategoryById(int id)
        {
            var categoryEntity = _categoryRepository.GetById(id);

            var editCategoryDto = new EditCategoryDto()
            {
                Id = categoryEntity.Id,
                Name = categoryEntity.Name,
                Description = categoryEntity.Description
            };

            return editCategoryDto;
        }
    }
}

