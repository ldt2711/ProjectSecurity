using Microsoft.AspNetCore.Mvc;
using ProjectSecurity.Services;

public class DESController : Controller
{

    public IActionResult Encrypt()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Encrypt(string inputText, string key, string mode)
    {
        

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
        
        string binaryPlaintext;
        string binaryKey;

        if (mode == "text")
        {
            binaryPlaintext = SecurityService.TextToBinary(inputText, 64);
            binaryKey = SecurityService.TextToBinary(key, 64);
        }
        else if (mode == "hex")
        {
            if (inputText.Length != 16 || !System.Text.RegularExpressions.Regex.IsMatch(inputText, @"^[0-9A-Fa-f]{16}$"))
            {
                ViewBag.Error = 4; // New error code for invalid hex
                return View();
            }
            binaryPlaintext = SecurityService.HexToBinary(inputText.ToUpper());
            
            if (key.Length != 16 || !System.Text.RegularExpressions.Regex.IsMatch(key, @"^[0-9A-Fa-f]{16}$"))
            {
                ViewBag.Error = 5; // New error code for invalid hex key
                return View();
            }
            binaryKey = SecurityService.HexToBinary(key.ToUpper());
        }
        else // binary mode
        {
            if (inputText.Length != 64 || !inputText.All(c => c == '0' || c == '1'))
            {
                ViewBag.Error = 2;
                return View();
            }
            binaryPlaintext = inputText;
            
            if (key.Length != 64 || !key.All(c => c == '0' || c == '1'))
            {
                ViewBag.Error = 3;
                return View();
            }
            binaryKey = key;
        }

        string cipherText = SecurityService.DES(binaryPlaintext, binaryKey);
        ViewBag.Input = inputText;
        ViewBag.BinaryPlaintext = binaryPlaintext;
        ViewBag.Key = key;
        ViewBag.ResultBinary = cipherText;

        // Set result for all output forms
        ViewBag.ResultHex = SecurityService.BinaryToHex(cipherText);

        if (mode == "hex")
        {
            ViewBag.Result = ViewBag.ResultHex;
        }
        else
        {
            ViewBag.Result = SecurityService.BinaryToText(cipherText);
        }

        return View();
    }
}