using AutoMapper;
using ProgrammersBlog.Data.Abstract;
using ProgrammersBlog.Entities.Concrete;
using ProgrammersBlog.Entities.Dtos;
using ProgrammersBlog.Services.Abstract;
using ProgrammersBlog.Shared.Utilities.Results.Abstract;
using ProgrammersBlog.Shared.Utilities.Results.ComplexType;
using ProgrammersBlog.Shared.Utilities.Results.Concrete;
using System;
using System.Collections.Generic;

using System.Threading.Tasks;

namespace ProgrammersBlog.Services.Concrete
{
    public class CategoryManager : ICategoryService

    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryManager(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IDataResult<CategoryDto>> AddAsync(CategoryAddDto categoryAddDto, string createdByName)
        {
            var category = _mapper.Map<Category>(categoryAddDto);
            category.CreatedByName = createdByName;
            category.ModifiedByName = createdByName;
            var categoryAdded=  await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.SaveAsync();
            return new DataResult<CategoryDto>(ResultStatus.Success,$"{categoryAddDto.Name} adlı kategori başarı ile güncellenmiştir.",new CategoryDto
            {
                Category= categoryAdded,
                ResultStatus=ResultStatus.Success,
                Message = $"{categoryAddDto.Name} adlı kategori başarı ile eklenmiştir."
            });
        }
        /// <summary>
        /// Soft Delete
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="modifiedByName"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<IDataResult<CategoryDto>> DeleteAsync(int categoryId, string modifiedByName)
        {
            var category = await _unitOfWork.Categories.GetAsync(c => c.Id == categoryId);
            if (category != null)
            {
                category.IsDeleted = true;
                category.ModifiedByName = modifiedByName;
                category.ModifiedDate = DateTime.Now;
                var deletedCategory = await _unitOfWork.Categories.UpdateAsync(category);
                await _unitOfWork.SaveAsync();
                return new DataResult<CategoryDto>(ResultStatus.Success, $"{deletedCategory.Name} adlı kategori başarı ile silinmiştir.", new CategoryDto
                {
                    Category = deletedCategory,
                    ResultStatus = ResultStatus.Success,
                    Message = $"{deletedCategory.Name} adlı kategori başarı ile silinmiştir."
                });
            }
            return new DataResult<CategoryDto>(ResultStatus.Error, "Böyle bir kategori bulunamamıştır.", new CategoryDto
            {
                Category = null,
                ResultStatus = ResultStatus.Error,
                Message =  "Böyle bir kategori bulunamamıştır.."
            });
        }
        public async Task<IDataResult<CategoryDto>> GetAsync(int categoryId)
        {
            var category = await _unitOfWork.Categories.GetAsync(x => x.Id == categoryId, c => c.Articles);
            if (category != null)
            {

                return new DataResult<CategoryDto>(ResultStatus.Success, new CategoryDto
                {
                    Category = category,
                    ResultStatus = ResultStatus.Success
                });
            }
            else
            {

                return new DataResult<CategoryDto>(ResultStatus.Error, "Kategori Bulunamadı", new CategoryDto
                {
                    Category=null,
                    ResultStatus=ResultStatus.Error,
                    Message= "Kategori Bulunamadı"
                });
            }
        }

        public async Task<IDataResult<CategoryListDto>> GetAllAsync()
        {
            var categories = await _unitOfWork.Categories.GetAllAsync(null, c => c.Articles);
            if (categories.Count > -1)
            {

                return new DataResult<CategoryListDto>(ResultStatus.Success, new CategoryListDto
                {
                    Categories = categories,
                    ResultStatus = ResultStatus.Success
                });
            }
            else
            {

                return new DataResult<CategoryListDto>(ResultStatus.Error, "Kategori Bulunamadı", new CategoryListDto
                {
                    Categories = null,
                    ResultStatus = ResultStatus.Error,
                    Message = "Kategori Bulunamadı"
                });
            }


        }

        public async Task<IDataResult<CategoryListDto>> GetAllByNonDeletedAsync()
        {
            var categories = await _unitOfWork.Categories.GetAllAsync(c => !c.IsDeleted, c => c.Articles);
            if (categories.Count > -1)
            {

                return new DataResult<CategoryListDto>(ResultStatus.Success, new CategoryListDto
                {
                    Categories = categories,
                    ResultStatus = ResultStatus.Success
                });
            }
            else
            {

                return new DataResult<CategoryListDto>(ResultStatus.Error, "Kategori Bulunamadı", new CategoryListDto
                {
                    Categories = null,
                    ResultStatus = ResultStatus.Error,
                    Message = "Kategori Bulunamadı"
                });
            }
        }
        public async Task<IDataResult<CategoryListDto>> GetAllByNonDeletedAndActiveAsync()
        {


            var categories = await _unitOfWork.Categories.GetAllAsync(c => !c.IsDeleted && c.IsActive, c => c.Articles);
            if (categories.Count > -1)
            {

                return new DataResult<CategoryListDto>(ResultStatus.Success, new CategoryListDto
                {
                    Categories = categories
                });
            }
            else
            {
                return new DataResult<CategoryListDto>(ResultStatus.Error, "Kategori Bulunamadı", null);

            }


        }
        public async Task<IResult> HardDeleteAsync(int categoryId)
        {
            var category = await _unitOfWork.Categories.GetAsync(c => c.Id == categoryId);
            if (category != null)
            {
                category.IsDeleted = true;

                await _unitOfWork.Categories.DeleteAsync(category);
                await _unitOfWork.SaveAsync();

                return new Result(ResultStatus.Success, $"{category.Name} adlı kategori başarı ile silinmiştir.");
            }
            return new Result(ResultStatus.Error, "Kategori Bulunamadı");

        }

        public async Task<IDataResult<CategoryDto>> UpdateAsync(CategoryUpdateDto categoryUpdateDto, string modifiedByName)
        {
            var category = _mapper.Map<Category>(categoryUpdateDto);
            category.ModifiedByName = modifiedByName;
            var updatedCategory= await _unitOfWork.Categories.UpdateAsync(category);
            await _unitOfWork.SaveAsync();
            return new  DataResult<CategoryDto>(ResultStatus.Success, $"{categoryUpdateDto.Name} adlı kategori başarı ile güncellenmiştir.", new CategoryDto
            {
                Category=updatedCategory,
                ResultStatus=ResultStatus.Success,
                Message= $"{categoryUpdateDto.Name} adlı kategori başarı ile güncellenmiştir."
            });
            //return new DataResult<CategoryDto>(ResultStatus.Error, "Kategori Bulunamadı", null);

        }


    }
}
