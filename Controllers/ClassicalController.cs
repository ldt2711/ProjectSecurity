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

            case "vigenereauto":

                result = _securityService.VigenereEncrypt(inputText, key);

                break;
        }

        ViewBag.Input = inputText;
        ViewBag.Key = key;
        ViewBag.Result = result;

        return View();
    }

    [HttpPost]
    public IActionResult Decrypt(string inputText, string key, string algorithm)
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

            case "vigenereauto":

                result = _securityService.VigenereDecrypt(inputText, key);

                break;
        }

        ViewBag.Input = inputText;
        ViewBag.Key = key;
        ViewBag.Result = result;

        return View();
    }
}