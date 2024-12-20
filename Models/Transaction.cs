using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IFinanceCoBackend.Models;

public class Transaction
{
    
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    public string UserId { get; set; }
    
    public DateOnly Date { get; set; }
    
    public float Value { get; set; }
    
    [MaxLength(100)]
    public string? Description { get; set; }
    
    public bool IsRecurring { get; set; }
    
    public bool IsIncome { get; set; }
    
    public TransactionType Type { get; set; }
}

public enum TransactionType
{
    // Income:
    Salary,
    Gift,
    
    // Expense:
    Rent,
    Food,
    Transportation,
    Entertainment,
    Health,
    Education,
    Alcohol,
    
    Other
}