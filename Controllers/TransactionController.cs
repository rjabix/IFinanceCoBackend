using IFinanceCoBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.DateTime;

namespace IFinanceCoBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController(
    ILogger<TransactionsController> logger,
    IFinanceDbContext context
    ) : ControllerBase
{
    [HttpPost("add-transaction")]
    public async Task<ActionResult> AddTransaction(AddTransactionRequest transaction)
    {
        var user = await context.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
        if (user == null)
            return NotFound("User not found");

        var date = DateOnly.FromDateTime(!TryParse(transaction.Date, out var dateTime) ? UtcNow : dateTime);

        var addedTransaction = context.Transactions.Add(
            new Transaction
            {
                UserId = user.Id,
                Date = date,
                Value = transaction.Value,
                Description = transaction.Description,
                IsRecurring = transaction.IsRecurring ?? false,
                IsIncome = transaction.IsIncome ?? false,
                Type = Enum.Parse<TransactionType>(transaction.Type)
            });
        await context.SaveChangesAsync();
        logger.LogInformation("Transaction added: {0}", addedTransaction.Entity.Id);
        return CreatedAtAction(nameof(AddTransaction), null, addedTransaction.Entity.Id);
    }
    
    public class AddTransactionRequest
    {
        public string? Date { get; set; }
        public float Value { get; set; }
        public string? Description { get; set; }
        public bool? IsRecurring { get; set; }
        public bool? IsIncome { get; set; }
        public string Type { get; set; }
    }
    
    [HttpPost("get-transactions")]
    public async Task<ActionResult> GetTransactions()
    {
        var user = await context.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
        if (user == null)
            return NotFound("User not found");

        var transactions = await context.Transactions
            .Where(t => t.UserId == user.Id)
            .ToListAsync();
        return Ok(transactions);
    }

    [HttpDelete("delete-transaction")]
    public async Task<ActionResult> DeleteTransaction([FromQuery] Guid id)
    {
        var user = await context.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

        if (user == null)
            return NotFound("User not found");

        var transaction = await context.Transactions.AsNoTracking()
            .SingleOrDefaultAsync(t => t.Id == id);

        if (transaction == null)
            return NotFound("Transaction not found");
        if (transaction.UserId != user.Id)
            return Unauthorized("Transaction does not belong to user");

        await context.Transactions.Where(t => t.Id == id).ExecuteDeleteAsync();
        return Ok("Transaction deleted");
    }

    [HttpPut("update-transaction")] // options -> time, value, description, isRecurring, type
    public async Task<ActionResult> UpdateTransaction(UpdateRequest request)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
        if (user == null)
            return NotFound("User not found");

        var transactionToUpdate = await context.Transactions.AsNoTracking()
            .SingleOrDefaultAsync(t => t.Id == request.Id);

        if (transactionToUpdate == null)
            return NotFound("Transaction not found");
        if (transactionToUpdate.UserId != user.Id)
            return Unauthorized("Transaction does not belong to user");

        try
        {
            switch (request.Option)
            {
                // Here I will be using .net 9 execute bulk methods, which don't need SaveChangesAsync() and tracking:
                // Similar: transactionToUpdate.Date = DateOnly.FromDateTime(DateTime.Parse(value)); SaveChangesAsync();
                case "date":
                    if(DateTime.Parse(request.Value) > DateTime.Now.AddHours(12)) 
                        return BadRequest("Invalid date");
                    await context.Transactions.Where(t => t.Id == request.Id)
                        .ExecuteUpdateAsync(t =>
                            t.SetProperty(p => p.Date, DateOnly.FromDateTime(Parse(request.Value))));
                    break;
                case "value":
                    await context.Transactions.Where(t => t.Id == request.Id)
                        .ExecuteUpdateAsync(t =>
                            t.SetProperty(p => p.Value, float.Parse(request.Value)));
                    break;
                case "description":
                    await context.Transactions.Where(t => t.Id == request.Id)
                        .ExecuteUpdateAsync(t =>
                            t.SetProperty(p => p.Description, request.Value));
                    break;
                case "isRecurring":
                    await context.Transactions.Where(t => t.Id == request.Id)
                        .ExecuteUpdateAsync(t =>
                            t.SetProperty(p => p.IsRecurring, bool.Parse(request.Value)));
                    break;
                case "type":
                    await context.Transactions.Where(t => t.Id == request.Id)
                        .ExecuteUpdateAsync(t =>
                            t.SetProperty(p => p.Type, Enum.Parse<TransactionType>(request.Value)));
                    break;
                default:
                    return BadRequest("The option is not valid");
            }
            return Ok("Successfully updated transaction");
        } 
        catch (Exception e)
        {
            return BadRequest("Bad request: " + e.Message);
        }
    }
    
    public class UpdateRequest
    {
        public string Option { get; set; }
        public Guid Id { get; set; }
        public string Value { get; set; }
    }

}