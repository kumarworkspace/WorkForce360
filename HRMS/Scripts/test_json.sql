DECLARE @json NVARCHAR(MAX) = '[{"StaffId":6,"IsPresent":true,"Remarks":null}]';
SELECT
    JSON_VALUE(value, '$.StaffId') AS StaffId,
    JSON_VALUE(value, '$.IsPresent') AS IsPresentRaw,
    ISNULL(JSON_VALUE(value, '$.IsPresent'), 1) AS IsPresentDefault
FROM OPENJSON(@json);
