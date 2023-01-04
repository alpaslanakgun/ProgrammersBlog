using Microsoft.AspNetCore.Http;
using ProgrammersBlog.Entities.Dtos;
using ProgrammersBlog.Shared.Utilities.Results.Abstract;
using System.Globalization;
using System.Threading.Tasks;
using ProgrammersBlog.Entities.ComplexTypes;

namespace ProgrammersBlog.Mvc.Helpers.Abstract
{
    public interface IImageHelper
    {
        Task<IDataResult<ImageUploadedDto>> Upload(string name, IFormFile pictureFile, PictureType pictureType, string folderName=null);
        IDataResult<ImageDeletedDto> Delete(string pictureName);

    }
}
