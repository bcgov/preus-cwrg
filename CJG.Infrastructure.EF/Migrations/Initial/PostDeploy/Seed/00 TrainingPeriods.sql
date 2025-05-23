PRINT 'Inserting [TrainingPeriods]'

DECLARE @Incrementor INT
DECLARE @IntValue INT

SET @Incrementor = 1
SET @IntValue = DATEPART(YEAR, GETDATE()) - 1

WHILE @Incrementor <= 20
BEGIN
	INSERT [dbo].[TrainingPeriods]
		([FiscalYearId],	[Caption],			[StartDate],																[EndDate],																	[DefaultPublishDate],													[DefaultOpeningDate]) VALUES
		 (@Incrementor,		N'Intake Period 1', CAST(CAST(@IntValue AS VARCHAR) + N'-04-01T07:00:00.000' AS DateTime),		CAST(CAST(@IntValue AS VARCHAR) + N'-08-31T07:00:00.000' AS DateTime),		CAST(CAST(@IntValue AS VARCHAR) + N'-01-01T08:00:00.000' AS DateTime),	CAST(CAST(@IntValue AS VARCHAR) + N'-02-01T08:00:00.000' AS DateTime))
		,(@Incrementor,		N'Intake Period 2', CAST(CAST(@IntValue AS VARCHAR) + N'-09-01T07:00:00.000' AS DateTime),		CAST(CAST(@IntValue AS VARCHAR) + N'-12-31T08:00:00.000' AS DateTime),		CAST(CAST(@IntValue AS VARCHAR) + N'-03-01T08:00:00.000' AS DateTime),	CAST(CAST(@IntValue AS VARCHAR) + N'-04-01T07:00:00.000' AS DateTime))
		,(@Incrementor,		N'Intake Period 3', CAST(CAST(@IntValue + 1 AS VARCHAR) + N'-01-01T08:00:00.000' AS DateTime),	CAST(CAST(@IntValue + 1 AS VARCHAR) + N'-03-31T07:00:00.000' AS DateTime),	CAST(CAST(@IntValue AS VARCHAR) + N'-09-01T07:00:00.000' AS DateTime),	CAST(CAST(@IntValue AS VARCHAR) + N'-10-01T07:00:00.000' AS DateTime))

	SET @Incrementor = @Incrementor + 1
	SET @IntValue = @IntValue + 1
END