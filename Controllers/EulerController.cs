using System.Numerics;
using Microsoft.AspNetCore.Mvc;
using ProjectSecurity.Services;

public class EulerController : Controller
{
    private readonly SecurityService _securityService;

    public EulerController(SecurityService securityService)
    {
        _securityService = securityService;
    }

    public IActionResult EulerCalc()
    {
        return View();
    }

    [HttpPost]
    public IActionResult EulerCalc(string inputText, string func)
    {
        ViewBag.Input = inputText;

        if (string.IsNullOrWhiteSpace(inputText))
        {
            ViewBag.Error = "Hãy nhập số!";
            return View();
        }


        try
        {
            BigInteger result = -1;
            if (func == "1")
            {
                BigInteger input = BigInteger.Parse(inputText);
                result = _securityService.Euler(input);
                ViewBag.Result = result;
            }
            return View();
        }
        catch
        {
            ViewBag.Error = "Hãy nhập số!";
            return View();
        }
    }
}