namespace ET.Web.Models;

public class Expense
{
    public int Id { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public string Description { get; set; } = "";
    // public string Category {get; set;} = "Uncategorized";
    public double Amount {get;set;}
    public required string OwnerId {get;set;}
}