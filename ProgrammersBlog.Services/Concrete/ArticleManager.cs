using AutoMapper;
using ProgrammersBlog.Data.Abstract;
using ProgrammersBlog.Entities.Concrete;
using ProgrammersBlog.Entities.Dtos;
using ProgrammersBlog.Services.Abstract;
using ProgrammersBlog.Services.Utilities;
using ProgrammersBlog.Shared.Utilities.Results.Abstract;
using ProgrammersBlog.Shared.Utilities.Results.ComplexType;
using ProgrammersBlog.Shared.Utilities.Results.Concrete;
using System;
using System.Threading.Tasks;
using static ProgrammersBlog.Services.Utilities.Messages;

namespace ProgrammersBlog.Services.Concrete
{
    public class ArticleManager : IArticleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ArticleManager(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IResult> AddAsync(ArticleAddDto articleAddDto, string createdByName)
        {

            var article = _mapper.Map<Article>(articleAddDto);
            article.CreatedByName = createdByName;
            article.ModifiedByName = createdByName;
            article.UserId = 1;
            await _unitOfWork.Articles.AddAsync(article);
            await _unitOfWork.SaveAsync();
            return new Result(ResultStatus.Success, Messages.ArticleMessage.Add(articleAddDto.Title));

        
        }

        public async Task<IResult> DeleteAsync(int articleId, string modifiedByName)
        {
            var result = await _unitOfWork.Articles.AnyAsync(a => a.Id == articleId);
            if (result)
            {
                var article = await _unitOfWork.Articles.GetAsync(a => a.Id == articleId);
                article.IsDeleted = true;
                article.ModifiedByName= modifiedByName;
                article.ModifiedDate = DateTime.Now;
                await _unitOfWork.Articles.UpdateAsync(article);
                await _unitOfWork.SaveAsync();
                return new Result(ResultStatus.Success, Messages.ArticleMessage.Delete(article.Title));

            }
            return new Result(ResultStatus.Error, Messages.ArticleMessage.NotFound(isPlural: false));


        }

        public async Task<IDataResult<ArticleDto>> GetAsync(int articleId)
        {
            var article = await _unitOfWork.Articles.GetAsync(a => a.Id == articleId, a => a.User, a => a.Category);
            if (article != null)
            {
                return new DataResult<ArticleDto>(ResultStatus.Success, new ArticleDto
                {
                    Article = article,
                    ResultStatus = ResultStatus.Success
                });
            }
            return new DataResult<ArticleDto>(ResultStatus.Error,Messages.ArticleMessage.NotFound(isPlural:false), null);
        }

        public async Task<IDataResult<ArticleListDto>> GetAllAsync()
        {
            var articles = await _unitOfWork.Articles.GetAllAsync(null, a => a.User, a => a.Category);
            if (articles.Count > -1)
            {
                return new DataResult<ArticleListDto>(ResultStatus.Success, new ArticleListDto
                {
                    Articles = articles,
                    ResultStatus = ResultStatus.Success

                }); 
            }

            return new DataResult<ArticleListDto>(ResultStatus.Error, Messages.ArticleMessage.NotFound(isPlural:true), null);
        }

        public async Task<IDataResult<ArticleListDto>> GetAllByCategoryAsync(int categoryId)
        {
            var result = await _unitOfWork.Categories.AnyAsync(c => c.Id == categoryId);
            if (result)
            {
                var articles = await _unitOfWork.Articles.GetAllAsync(a => a.CategoryId == categoryId && !a.IsDeleted && a.IsActive, a => a.User, a => a.Category);
                if (articles.Count > -1)
                {
                    return new DataResult<ArticleListDto>(ResultStatus.Success, new ArticleListDto
                    {
                        Articles = articles,
                        ResultStatus = ResultStatus.Success

                    });
                }
                return new DataResult<ArticleListDto>(ResultStatus.Error,ArticleMessage.NotFound(isPlural:false), null);
            }
            return new DataResult<ArticleListDto>(ResultStatus.Error, Messages.CategoryMessage.NotFound(isPlural: false), null);


        }

        public async Task<IDataResult<ArticleListDto>> GetAllByNonDeletedAsync()
        {
            var articles = await _unitOfWork.Articles.GetAllAsync(a=>!a.IsDeleted,a=> a.User, a => a.Category);
            if (articles.Count > -1)
            {
                return new DataResult<ArticleListDto>(ResultStatus.Success, new ArticleListDto
                {
                    Articles = articles,
                    ResultStatus = ResultStatus.Success

                });
            }

            return new DataResult<ArticleListDto>(ResultStatus.Error, Messages.ArticleMessage.NotFound(isPlural:true), null);
        }

        public async Task<IDataResult<ArticleListDto>> GetAllByNonDeletedAndActiveAsync()
        {
            var articles= await _unitOfWork.Articles.GetAllAsync(a=>!a.IsDeleted&&a.IsActive,a=> a.User,a=> a.Category);
            if (articles.Count > -1)
            {
                return new DataResult<ArticleListDto>(ResultStatus.Success, new ArticleListDto
                {
                    Articles = articles,
                    ResultStatus = ResultStatus.Success

                });
            }

            return new DataResult<ArticleListDto>(ResultStatus.Error, Messages.ArticleMessage.NotFound(isPlural:true), null);
        }
        public async Task<IResult> HardDeleteAsync(int articleId)
        {
            var result = await _unitOfWork.Articles.AnyAsync(a => a.Id == articleId);
            if (result)
            {
                var article = await _unitOfWork.Articles.GetAsync(a => a.Id == articleId);

                await _unitOfWork.Articles.DeleteAsync(article);
                await _unitOfWork.SaveAsync();
                return new Result(ResultStatus.Success,Messages.ArticleMessage.Delete(article.Title));

            }
            return new Result(ResultStatus.Error, Messages.ArticleMessage.NotFound(isPlural:false));
        }

        public async Task<IResult> UpdateAsync(ArticleUpdateDto articleUpdateDto, string modifiedByName)
        {

            var article = _mapper.Map<Article>(articleUpdateDto);
            article.ModifiedByName = modifiedByName;
            await _unitOfWork.Articles.UpdateAsync(article);
            await _unitOfWork.SaveAsync();

            return new Result(ResultStatus.Success, Messages.ArticleMessage.Update(articleUpdateDto.Title));

        }

        public async Task<IDataResult<int>> CountAsync()
        {
            var articleCount = await _unitOfWork.Articles.CountAsync();
            if (articleCount > -1)
            {
                return new DataResult<int>(ResultStatus.Success, articleCount);
            }
            else
            {
                return new DataResult<int>(ResultStatus.Error, "Beklenmeyen bir hata ile karşılaşıldı", -1);
            }
        }

        public async Task<IDataResult<int>> CountByNonDeletedAsync()
        {
            var articlesCount = await _unitOfWork.Articles.CountAsync(c => !c.IsDeleted);
            if (articlesCount > -1)
            {
                return new DataResult<int>(ResultStatus.Success, articlesCount);
            }
            else
            {
                return new DataResult<int>(ResultStatus.Error, "Beklenmeyen bir hata ile karşılaşıldı", -1);
            }
        }
    }
}
