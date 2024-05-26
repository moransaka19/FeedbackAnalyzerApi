using System.Text.Json;
using Amazon.Comprehend;
using Amazon.Comprehend.Model;
using FeedbackAnalyze.Data;
using FeedbackAnalyze.Data.Entities;
using FeedbackAnalyze.Helpers;
using FeedbackAnalyze.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FeedbackAnalyze.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductController : ControllerBase
{
    private readonly AppDataContext _context;

    public ProductController(AppDataContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _context.Products.ToListAsync();
        return Ok(products);
    }
    
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] string productName)
    {
        var newProduct = new Product
        {
            Name = productName
        };
        await _context.Products.AddAsync(newProduct);
        await _context.SaveChangesAsync();
        
        return Ok();
    }
}