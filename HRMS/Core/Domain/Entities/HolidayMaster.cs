namespace HRMS.Core.Domain.Entities;

public class HolidayMaster : BaseEntity
{
    public int HolidayId { get; set; }
    public DateTime HolidayDate { get; set; }
    public string HolidayName { get; set; } = string.Empty;
}





