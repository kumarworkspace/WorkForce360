-- Test the bulk attendance SP
DECLARE @TestData NVARCHAR(MAX) = '[{"StaffId":1,"IsPresent":true,"Remarks":null}]';
EXEC usp_BulkMarkCourseAttendanceDateWise
    @CoursePlanId = 1,
    @AttendanceDate = '2026-02-05',
    @TenantId = 1,
    @CreatedBy = 1,
    @AttendanceData = @TestData;
