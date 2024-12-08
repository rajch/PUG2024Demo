using ET.Web.Data;
using ET.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using System.Linq;

namespace ET.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly ApplicationDbContext _context;

    [BindProperty]
    public int CurrentMonth { get; set; }

    [BindProperty]
    public int CurrentYear { get; set; }

    [BindProperty]
    public required Expense NewExpenseItem { get; set; }

    public IEnumerable<Expense>? CurrentMonthExpenses { get; private set; }

    public IndexModel(ILogger<IndexModel> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task OnGetAsync(int? year, int? month)
    {
        var userid = User?.Identity?.Name;
        if (userid == null)
        {
            // No data population for anonymous users
            return;
        }

        CurrentMonth = month ?? DateTime.Now.Month;
        CurrentYear = year ?? DateTime.Now.Year;

        await SetupPageModel(userid);
    }

    public async Task OnPostMonthYearAsync()
    {
        var userid = User?.Identity?.Name;
        if (userid == null)
        {
            _logger.Log(LogLevel.Error, "Could not identify user.");
            return;
        }

        _logger.Log(
            LogLevel.Trace,
            "Month and year changed to {Month} and {Year}",
            CurrentMonth,
            CurrentYear
        );

        await SetupPageModel(userid);
    }

    public async Task<IActionResult> OnPostNewExpenseAsync()
    {
        var userid = User?.Identity?.Name;
        if (userid == null)
        {
            _logger.Log(LogLevel.Error, "Could not identify user.");
            return Unauthorized();
        }

        NewExpenseItem.OwnerId = userid;
        NewExpenseItem.Date = NewExpenseItem.Date.ToUniversalTime();

        _logger.Log(
            LogLevel.Trace,
            "Saving this: {Expense}",
            NewExpenseItem.ToJson()
        );

        await _context.Expenses.AddAsync(NewExpenseItem);
        await _context.SaveChangesAsync();

        return RedirectToPage("/Index", new { month = CurrentMonth, year = CurrentYear });
    }

    private async Task SetupPageModel(string userid)
    {
        CurrentMonthExpenses = await _context.Expenses
                                .Where(e => e.OwnerId == userid)
                                .Where(e => e.Date.Year == CurrentYear && e.Date.Month == CurrentMonth)
                                .OrderBy(e => e.Date)
                                .ToListAsync();

        NewExpenseItem = new Expense { OwnerId = userid };
    }
}
