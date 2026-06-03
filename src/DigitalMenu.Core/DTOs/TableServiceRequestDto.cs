namespace DigitalMenu.Core.DTOs;

public class TableServiceRequestDto
{
    public Guid TableId { get; set; }
    public string TableNumber { get; set; } = null!;
    public string RequestType { get; set; } = null!; // WAITER | BILL
    public DateTime RequestedAt { get; set; }
}
