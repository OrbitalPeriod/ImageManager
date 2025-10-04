using ImageManager.Data;
using ImageManager.Data.Models;
using ImageManager.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ImageManager.Controllers;

[ApiController]
public class TestController(IPixivService pixivService, ITaggerService taggerService, IPixivImageImportManager importManager, ApplicationDbContext dbContext, UserManager<User> userManager, IDatabaseService databaseService) : Controller
{
    
}