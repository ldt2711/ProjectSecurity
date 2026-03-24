using Microsoft.AspNetCore.Mvc;
using ProjectSecurity.Services;

public class ClassicalController : Controller
{
    private readonly SecurityService _securityService;

    public ClassicalController(SecurityService securityService)
    {
        _securityService = securityService;
    }

    public IActionResult Encrypt()
    {
        return View();
    }

    public IActionResult Decrypt()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Encrypt(string inputText, string key, string algorithm)
    {
        ViewBag.Input = inputText;
        ViewBag.Key = key;

        if (string.IsNullOrWhiteSpace(inputText))
        {
            ViewBag.Error = 0;
            return View();
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            ViewBag.Error = 1;
            return View();
        }

        try
        {
            string result = "";

            switch (algorithm)
            {
                case "caesar":

                    int caesarKey = int.Parse(key);

                    result = _securityService.CaesarEncrypt(inputText, caesarKey);

                    break;

                case "playfair":

                    result = _securityService.PlayfairEncrypt(inputText, key);

                    break;

                case "vigenererepeat":

                    result = _securityService.VigenereRepeatEncrypt(inputText, key);

                    break;

                case "vigenereauto":

                    result = _securityService.VigenereAutoEncrypt(inputText, key);

                    break;
            }

            ViewBag.Result = result;

            return View();
        }
        catch
        {
            ViewBag.Error = 3;
            return View();
        }
    }

    [HttpPost]
    public IActionResult Decrypt(string inputText, string key, string algorithm)
    {
        ViewBag.Input = inputText;
        ViewBag.Key = key;
        
        if (string.IsNullOrWhiteSpace(inputText))
        {
            ViewBag.Error = 0;
            return View();
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            ViewBag.Error = 1;
            return View();
        }

        try
        {
            string result = "";

            switch (algorithm)
            {
                case "caesar":

                    int caesarKey = int.Parse(key);

                    result = _securityService.CaesarDecrypt(inputText, caesarKey);

                    break;
                
                case "playfair":

                    result = _securityService.PlayfairDecrypt(inputText, key);

                    break;

                case "vigenererepeat":

                    result = _securityService.VigenereRepeatDecrypt(inputText, key);

                    break;

                case "vigenereauto":

                    result = _securityService.VigenereAutoDecrypt(inputText, key);

                    break;
            }

            ViewBag.Result = result;

            return View();
        }
        catch
        {
            ViewBag.Error = 3;
            return View();
        }
    }

    
}