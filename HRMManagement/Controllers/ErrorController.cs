﻿using Microsoft.AspNetCore.Mvc;

public class ErrorController : Controller
{
    [Route("404")]
    public IActionResult PageNotFound()
    {
        string originalPath = "unknown";
        if (HttpContext.Items.ContainsKey("originalPath"))
        {
            originalPath = HttpContext.Items["originalPath"] as string;
        }
        return View();
    }
}
