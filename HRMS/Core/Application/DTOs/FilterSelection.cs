namespace HRMS.Core.Application.DTOs
{
    public class FilterSelections
    {
        public List<string> DocumentTypes { get; set; } = new();
        public List<string> VersionStatuses { get; set; } = new();
        public List<string> Authors { get; set; } = new();
        public List<string> Approvers { get; set; } = new();
        public List<string> Checks { get; set; } = new();
        public List<string> Controls { get; set; } = new();
        public List<string> Frameworks { get; set; } = new();
    }

}
