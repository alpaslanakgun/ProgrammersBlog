using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProgrammersBlog.Entities.Concrete;
using ProgrammersBlog.Entities.Dtos;
using ProgrammersBlog.Mvc.Areas.Admin.Models;
using ProgrammersBlog.Services.Abstract;
using ProgrammersBlog.Services.Extension;
using ProgrammersBlog.Shared.Utilities.Extensions;
using ProgrammersBlog.Shared.Utilities.Results.ComplexType;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace ProgrammersBlog.Mvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMapper _mapper;
        public UserController(UserManager<User> userManager, IWebHostEnvironment webHostEnvironment, IMapper mapper = null, SignInManager<User> signInManager = null)
        {
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _mapper = mapper;
            _signInManager = signInManager;
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            ImageDelete("test");
            return View(new UserListDto
            {
                Users = users,
                ResultStatus = ResultStatus.Success
            });
        }

        [HttpGet]
        public  IActionResult Login()
        {
        
            return View("UserLogin");
        }

        [HttpPost]
        public async Task<IActionResult> Login(UserLoginDto userLoginDto)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(userLoginDto.Email);
                if (user!=null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, userLoginDto.Password, userLoginDto.RememberMe, false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Eposta Adresiniz veya Şifreniz Yanlıştır");
                        return View("UserLogin");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Eposta Adresiniz veya Şifreniz Yanlıştır");
                    return View("UserLogin");
                }
            }
            else
            {
                return View("UserLogin");
            }
        
        }



        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var userListDto = JsonSerializer.Serialize(new UserListDto
            {
                Users = users,
                ResultStatus = ResultStatus.Success
            }, new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve
            });
            return Json(userListDto);
        }
        [HttpGet]
        [Authorize]
        public IActionResult Add()
        {
            return PartialView("_UserAddPartial");
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Add(UserAddDto userAddDto)
        {
            if (ModelState.IsValid)
            {
                userAddDto.Picture = await ImageUpload(userAddDto.UserName,userAddDto.PictureFile);
                var user = _mapper.Map<User>(userAddDto);
                var result = await _userManager.CreateAsync(user, userAddDto.Password);
                if (result.Succeeded)
                {
                    var userAddAjaxModel = JsonSerializer.Serialize(new UserAddAjaxViewModel
                    {
                        UserDto = new UserDto
                        {
                            ResultStatus = ResultStatus.Success,
                            Message = $"{user.UserName} adlı kullanıcı başarıyla eklenmiştir",
                            User = user
                        },
                        UserAddPartial=await this.RenderViewToStringAsync("_UserAddPartial",userAddDto)
                    });
                    return Json(userAddAjaxModel);
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    var userAddAjaxErrorModel = JsonSerializer.Serialize(new UserAddAjaxViewModel
                    {
                        UserAddDto = userAddDto,
                        UserAddPartial = await this.RenderViewToStringAsync("_UserAddPartial", userAddDto)
                    });
                    return Json(userAddAjaxErrorModel);
                }
            }
            var userAddAjaxErrorModelState = JsonSerializer.Serialize(new UserAddAjaxViewModel
            {
                UserAddDto = userAddDto,
                UserAddPartial = await this.RenderViewToStringAsync("_UserAddPartial", userAddDto)
            });
            return Json(userAddAjaxErrorModelState);
        }

        [HttpPost]
        [Authorize]
        public async Task<JsonResult>Delete(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                var deletedUser = JsonSerializer.Serialize(new UserDto
                {
                    ResultStatus=ResultStatus.Success,
                    Message=$"{user.UserName} adlı kullanıcı başarıyla silinmiştir.",
                    User=user
                });
                return Json(deletedUser);
            }
            else
            {
                string errorMessages = string.Empty;
                foreach (var error in result.Errors)
                {
                    errorMessages = $"{error.Description}";
                }
                var deletedUserErrorModel = JsonSerializer.Serialize(new UserDto
                {
                    ResultStatus = ResultStatus.Error,
                    Message = $"{user.UserName} adl kullanıcı silinirken bir hata oluştu \n{errorMessages}",
                    User=user
                });
                return Json(deletedUserErrorModel);
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<PartialViewResult>Update(int userId)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == userId);
            var userUpdateDto = _mapper.Map<UserUpdateDto>(user);
            return PartialView("_UserUpdatePartial", userUpdateDto);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Update(UserUpdateDto userUpdateDto)
        {
            if (ModelState.IsValid)
            {
                bool isNewPictureUploaded = default;
                var oldUser = await _userManager.FindByIdAsync(userUpdateDto.Id.ToString());
                var oldUserPicture = oldUser.Picture;
                     if (userUpdateDto.PictureFile!=null)
                {
                    userUpdateDto.Picture = await ImageUpload(userUpdateDto.UserName, userUpdateDto.PictureFile);
                    isNewPictureUploaded = true;
                }
                var updatedUser=_mapper.Map<UserUpdateDto,User>(userUpdateDto,oldUser);
                var result = await _userManager.UpdateAsync(updatedUser);
                if (result.Succeeded)
                {
                    if (isNewPictureUploaded)
                    {
                        ImageDelete(oldUserPicture);
                    }
                    var userUpdateViewModel = JsonSerializer.Serialize(new UserUpdateAjaxViewModel
                    {
                        UserDto = new UserDto
                        {
                            ResultStatus = ResultStatus.Success,
                            Message = $"{updatedUser.UserName} adlı kullanıcı başarıyla güncellenmiştir",
                            User = updatedUser
                        },
                        UserUpdatePartial = await this.RenderViewToStringAsync("_UserUpdatePartial",userUpdateDto)

                    });
                    return Json(userUpdateViewModel);
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    var userUpdateErrorViewModel = JsonSerializer.Serialize(new UserUpdateAjaxViewModel
                    {
                        UserUpdateDto=userUpdateDto,
                        UserUpdatePartial = await this.RenderViewToStringAsync("_UserUpdatePartial", userUpdateDto)

                    });
                    return Json(userUpdateErrorViewModel);
                }
            }
            else
            {
                var userUpdateErrorModelStateViewModel = JsonSerializer.Serialize(new UserUpdateAjaxViewModel
                {
                    UserUpdateDto = userUpdateDto,
                    UserUpdatePartial = await this.RenderViewToStringAsync("_UserUpdatePartial", userUpdateDto)

                });
                return Json(userUpdateErrorModelStateViewModel);
            }

        }

        [Authorize]
        public async Task<string> ImageUpload(string username,IFormFile pictureFile)
        {
            // ~/img/user.Picture
            string wwwroot = _webHostEnvironment.WebRootPath;
            //alpaslanakgun  .png
            //string fileName = Path.GetFileNameWithoutExtension(pictureFile.FileName);
            //.png
            string fileExtensions = Path.GetExtension(pictureFile.FileName);
            DateTime dateTime = DateTime.Now;
            string fileName = $"{username}_{dateTime.FullDateAndTimeStringWithUnderscore()}{fileExtensions}";
            var path = Path.Combine($"{wwwroot}/img", fileName);
            await using (var stream = new FileStream(path, FileMode.Create))
            {
                await pictureFile.CopyToAsync(stream);
            }
            return fileName;


        }
        [Authorize]
        public bool ImageDelete(string pictureName)
        {
            string wwwroot = _webHostEnvironment.WebRootPath;
            var fileToDelete = Path.Combine($"{wwwroot}/img", pictureName);
            if (System.IO.File.Exists(fileToDelete))
            {
                System.IO.File.Delete(fileToDelete);
                return true;
            }
            else
            {
                return false;
            }
        }







    }
}
