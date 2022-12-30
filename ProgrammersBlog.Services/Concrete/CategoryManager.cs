using AutoMapper;
using ProgrammersBlog.Data.Abstract;
using ProgrammersBlog.Data.Concrete;
using ProgrammersBlog.Entities.Concrete;
using ProgrammersBlog.Entities.Dtos;
using ProgrammersBlog.Services.Abstract;
using ProgrammersBlog.Services.Utilities;
using ProgrammersBlog.Shared.Utilities.Results.Abstract;
using ProgrammersBlog.Shared.Utilities.Results.ComplexType;
using ProgrammersBlog.Shared.Utilities.Results.Concrete;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            return new DataResult<CategoryDto>(ResultStatus.Success,Messages.CategoryMessage.Add(categoryAddDto.Name),new CategoryDto
            {
                Category= categoryAdded,
                ResultStatus=ResultStatus.Success,
                Message = Messages.CategoryMessage.Add(categoryAddDto.Name)
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
                return new DataResult<CategoryDto>(ResultStatus.Success, Messages.CategoryMessage.Delete(category.Name), new CategoryDto
                {
                    Category = deletedCategory,
                    ResultStatus = ResultStatus.Success,
                    Message = Messages.CategoryMessage.Delete(category.Name)
                });
            }
            return new DataResult<CategoryDto>(ResultStatus.Error, Messages.CategoryMessage.NotFound(isPlural: false), new CategoryDto
            {
                Category = null,
                ResultStatus = ResultStatus.Error,
                Message = Messages.CategoryMessage.NotFound(isPlural: false)
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

                return new DataResult<CategoryDto>(ResultStatus.Error, Messages.CategoryMessage.NotFound(false), new CategoryDto
                {
                    Category=null,
                    ResultStatus=ResultStatus.Error,
                    Message= Messages.CategoryMessage.NotFound(false)
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

                return new DataResult<CategoryListDto>(ResultStatus.Error, Messages.CategoryMessage.NotFound(isPlural:true), new CategoryListDto
                {
                    Categories = null,
                    ResultStatus = ResultStatus.Error,
                    Message = Messages.CategoryMessage.NotFound(isPlural: true)
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

                return new DataResult<CategoryListDto>(ResultStatus.Error, Messages.CategoryMessage.NotFound(isPlural: true), new CategoryListDto
                {
                    Categories = null,
                    ResultStatus = ResultStatus.Error,
                    Message = Messages.CategoryMessage.NotFound(isPlural: true)
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
                return new DataResult<CategoryListDto>(ResultStatus.Error, Messages.CategoryMessage.NotFound(isPlural: true), null);

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

                return new Result(ResultStatus.Success, Messages.CategoryMessage.HardDelete(category.Name));
            }
            return new Result(ResultStatus.Error, Messages.CategoryMessage.NotFound(isPlural: false));

        }

        public async Task<IDataResult<CategoryDto>> UpdateAsync(CategoryUpdateDto categoryUpdateDto, string modifiedByName)
        {
            var oldCategory = await _unitOfWork.Categories.GetAsync(c => c.Id == categoryUpdateDto.Id);
            var category = _mapper.Map<CategoryUpdateDto,Category>(categoryUpdateDto,oldCategory);
            category.ModifiedByName = modifiedByName;
            var updatedCategory= await _unitOfWork.Categories.UpdateAsync(category);
            await _unitOfWork.SaveAsync();
            return new  DataResult<CategoryDto>(ResultStatus.Success,Messages.CategoryMessage.Update(categoryUpdateDto.Name), new CategoryDto
            {
                Category=updatedCategory,
                ResultStatus=ResultStatus.Success,
                Message= Messages.CategoryMessage.Update(categoryUpdateDto.Name)
            });

        }

        public async Task<IDataResult<CategoryUpdateDto>> GetCategoryUpdateDto(int categoryId)
        {
            var result = await _unitOfWork.Categories.AnyAsync(c => c.Id == categoryId);
            if (result)
            {
                var category = await _unitOfWork.Categories.GetAsync(c => c.Id == categoryId);
                var categoryUpdateDto = _mapper.Map<CategoryUpdateDto>(category);
                return new DataResult<CategoryUpdateDto>(ResultStatus.Success, categoryUpdateDto);
            }
            else
            {
                return new DataResult<CategoryUpdateDto>(ResultStatus.Error, Messages.CategoryMessage.NotFound(isPlural:false), null);
            }
        }
    } 
}
