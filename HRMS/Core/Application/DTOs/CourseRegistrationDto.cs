namespace HRMS.Core.Application.DTOs;

public class CreateCourseRequest
{
    public string Code { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string TrainingModule { get; set; } = string.Empty;
    public int CourseTypeId { get; set; }
    public int CourseCategoryId { get; set; }
    public int TrainerId { get; set; }
    public decimal Duration { get; set; }
    public int ValidityPeriod { get; set; }
    public string? UploadFilePath { get; set; }
}

public class UpdateCourseRequest : CreateCourseRequest
{
    public int CourseId { get; set; }
}

public class CourseListDto
{
    public int CourseId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string TrainingModule { get; set; } = string.Empty;
    public string CourseType { get; set; } = string.Empty;
    public string CourseCategory { get; set; } = string.Empty;
    public string TrainerName { get; set; } = string.Empty;
    public decimal Duration { get; set; }
    public string ValidityPeriod { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CourseDto
{
    public int CourseId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string TrainingModule { get; set; } = string.Empty;
    public int CourseTypeId { get; set; }
    public string? CourseTypeName { get; set; }
    public int CourseCategoryId { get; set; }
    public string? CourseCategoryName { get; set; }
    public int TrainerId { get; set; }
    public string? TrainerName { get; set; }
    public decimal Duration { get; set; }
    public int ValidityPeriod { get; set; }
    public string? ValidityPeriodName { get; set; }
    public string? UploadFilePath { get; set; }
    public bool IsActive { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
}

public class CourseStatisticsDto
{
    public int CreatedCoursesCount { get; set; }
    public int AssignedCoursesCount { get; set; }
    public int ActiveCoursesCount { get; set; }
    public decimal TotalTrainingHours { get; set; }
}

public class MonthlyStatisticsDto
{
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class YearlyStatisticsDto
{
    public int Year { get; set; }
    public int Count { get; set; }
}
