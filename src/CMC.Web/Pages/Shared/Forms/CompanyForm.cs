namespace CMC.Web.Pages.Shared.Forms;

public sealed class UpdateCompanyForm
{
  public Guid Id { get; set; }
  public string Name { get; set; } = "";
  public int EmployeeCount { get; set; }
  public decimal RevenuePerYear { get; set; }
}
