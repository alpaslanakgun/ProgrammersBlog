using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using ProgrammersBlog.Entities.Dtos;
using ProgrammersBlog.Mvc.Helpers.Abstract;
using ProgrammersBlog.Shared.Utilities.Results.Abstract;
using System.IO;
using System;
using System.Threading.Tasks;
using ProgrammersBlog.Entities.ComplexTypes;
using ProgrammersBlog.Services.Extension;
using ProgrammersBlog.Shared.Utilities.Results.Concrete;
using ProgrammersBlog.Shared.Utilities.Results.ComplexType;

namespace ProgrammersBlog.Mvc.Helpers.Concrete
{
    public class ImageHelper : IImageHelper
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string _wwwroot;
        private readonly string imgFolder= "img";
        private const string userImagesFolder = "userImages";
        private const string postImagesFolder = "postImages";

        public ImageHelper(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            _wwwroot = _webHostEnvironment.WebRootPath;
        }

      

        public async Task<IDataResult<ImageUploadedDto>> Upload(string name, IFormFile pictureFile,PictureType pictureType, string folderName=null)
        {
            folderName ??= pictureType == PictureType.User ? userImagesFolder : postImagesFolder;

            if (!Directory.Exists($"{_wwwroot}/{imgFolder}/{folderName}"))
            {
                Directory.CreateDirectory($"{_wwwroot}/{imgFolder}/{folderName} ");
            }

            string oldFileName = Path.GetFileNameWithoutExtension(pictureFile.FileName);
            //.png
            string fileExtensions = Path.GetExtension(pictureFile.FileName);
            var dateTime = DateTime.Now;
            string newFileName = $"{name}_{dateTime.FullDateAndTimeStringWithUnderscore()}{fileExtensions}";
            var path = Path.Combine($"{_wwwroot}/{imgFolder}/{folderName}", newFileName);
            await using (var stream = new FileStream(path, FileMode.Create))
            {
                await pictureFile.CopyToAsync(stream);
            }

            string message = pictureType == PictureType.User
                ? $"{name} adlı kullanıcının resimi başarıyla yüklenmiştir"
                : $"{name} adlı kullanıcının resimi başarıyla yüklenmiştir";

;            return new DataResult<ImageUploadedDto>(ResultStatus.Success, message, new ImageUploadedDto
            {
                FullName = $"{folderName}/{newFileName}",
                OldFullName = oldFileName,
                Extension = fileExtensions,
                Path = path,
                Size = pictureFile.Length
            });

          
        }
        public IDataResult<ImageDeletedDto> Delete(string pictureName)
        {
            var fileToDelete = Path.Combine($"{_wwwroot}/{imgFolder}/", pictureName);
            if (File.Exists(fileToDelete))
            {
                var fileInfo = new FileInfo(fileToDelete);
                var imageDeletedDto = new ImageDeletedDto
                {
                    FullName = pictureName,
                    Extension = fileInfo.Extension,
                    Path = fileInfo.FullName,
                    Size = fileInfo.Length
                };
               File.Delete(fileToDelete);
                return new DataResult<ImageDeletedDto>(ResultStatus.Success, imageDeletedDto);
            }
            else
            {
                return new DataResult<ImageDeletedDto>(ResultStatus.Error, $"Böyle bir resim bulunamadı.", null);
            }
        }
    }
}
