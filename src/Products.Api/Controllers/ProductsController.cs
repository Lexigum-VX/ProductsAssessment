
using Microsoft.AspNetCore.Mvc;
using Products.Application.DTOs.Products;
using Products.Application.Interfaces;
using Products.Application.Results;

namespace Products.Api.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(
    IProductService productService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var products = await productService.GetAllAsync(cancellationToken);
        return Ok(products);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var product = await productService.GetByIdAsync(id, cancellationToken);

        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponse>> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await productService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = product.Id },
            product);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductResponse>> Update(
        int id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await productService.UpdateAsync(
            id, request, cancellationToken);

        return product is null ? NotFound() : Ok(product);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var deleted = await productService.DeleteAsync(
            id, cancellationToken);

        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{id:int}/decrement-stock/{quantity:int}")]
    public async Task<IActionResult> DecrementStock(
        int id,
        int quantity,
        CancellationToken cancellationToken)
    {
        var result = await productService.DecrementStockAsync(
            id, quantity, cancellationToken);

        return MapStockResult(result);
    }

    [HttpPost("{id:int}/add-to-stock/{quantity:int}")]
    public async Task<IActionResult> AddStock(
        int id,
        int quantity,
        CancellationToken cancellationToken)
    {
        var result = await productService.AddStockAsync(
            id, quantity, cancellationToken);

        return MapStockResult(result);
    }

    [HttpGet("search")]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> Search(
        [FromQuery] string name,
        CancellationToken cancellationToken)
    {
        var products = await productService.SearchByNameAsync(
            name, cancellationToken);

        return Ok(products);
    }

    [HttpGet("stock-level")]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> StockLevel(
        [FromQuery] int min,
        [FromQuery] int max,
        CancellationToken cancellationToken)
    {
        var products = await productService.GetByStockRangeAsync(
            min, max, cancellationToken);

        return Ok(products);
    }

    private IActionResult MapStockResult(StockOperationResult result) =>
        result switch
        {
            StockOperationResult.Success => Ok(),
            StockOperationResult.ProductNotFound => NotFound(),
            StockOperationResult.Conflict => Conflict(),
            _ => throw new ArgumentOutOfRangeException(nameof(result))
        };
}
