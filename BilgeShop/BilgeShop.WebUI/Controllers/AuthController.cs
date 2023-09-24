using BilgeShop.Business.Dtos;
using BilgeShop.Business.Services;
using BilgeShop.WebUI.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace BilgeShop.WebUI.Controllers
{
    // Authentication - Authorization
    // (Kimlik Doğrulama - Yetkilendirme)
    public class AuthController : Controller
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel formData)
        {

            if (!ModelState.IsValid)
            {
                // model istediğim şartlara uygun hazırlanmadıysa, forma geri döneceğim.
                return View(formData); // yeniden açılınca form girilmiş olan veriler kaybolmasın.

            }

            // Eğer her şey yolundaysa, beni anasayfaya geri göndersin. (kayıt işlemleri etc)

            var addUserDto = new AddUserDto()
            {
                FirstName = formData.FirstName.Trim(),
                LastName = formData.LastName.Trim(),
                Email = formData.Email.Trim().ToLower(),
                Password = formData.Password
            };

            var result = _userService.AddUser(addUserDto);

            if (result.IsSucceed)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.ErrorMessage = result.Message;
                return View(formData);
            }
        }

        [HttpPost]
        public  async Task<IActionResult> Login(LoginViewModel formData)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index", "Home");
                // Veriler istediğim formatta/şekilde girilmediyse, ana sayfaya geri gönder.
            }

            var loginUserDto = new LoginUserDto()
            {
                Email = formData.Email,
                Password = formData.Password
            };

            var userInfo = _userService.LoginUser(loginUserDto);

            if(userInfo is null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Buraya kadar kodlar geldiyse demek ki email ve şifre eşleşmiş. Gerekli bilgiler (id,email,fname,lname,usertype) veritabanından çekilip bu aşamaya userInfo içerisinde gelmiş.

            // Oturumda tutacağım her veri -> Claim
            // Claimlerin listesi -> Claims

            var claims = new List<Claim>();

            claims.Add(new Claim("id", userInfo.Id.ToString()));
            claims.Add(new Claim("email", userInfo.Email));
            claims.Add(new Claim("firstName", userInfo.FirstName));
            claims.Add(new Claim("lastName", userInfo.LastName));
            claims.Add(new Claim("userType", userInfo.UserType.ToString())); // benim kullanacağım

            // Yetkilendirme işlemleri için özel olarak bir claim açmam gerekiyor.

            claims.Add(new Claim(ClaimTypes.Role, userInfo.UserType.ToString())); // .net metotlarının kullanacağı

            var claimIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            // Claims içerisindeki verilerle bir oturum açılacağını söylüyorum.

            var autProperties = new AuthenticationProperties
            {
                AllowRefresh = true, // yenilenebilir enerji kaynakları
                ExpiresUtc = new DateTimeOffset(DateTime.Now.AddHours(48)) // oturum 48 saat geçerli
            };
            // oturumun özelliklerini belirliyorum


            // Asenkronize (async) bir metot kullanılıyorsa, await keywordü ile kullanılır.
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimIdentity), autProperties);

            return RedirectToAction("Index", "Home");
        }


        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(); // oturumu kapat.

            return RedirectToAction("Index", "Home");
        }



    }
}
