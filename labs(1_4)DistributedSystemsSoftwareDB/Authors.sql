CREATE TABLE [dbo].[Authors]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY(1, 1), 
    [Name] NCHAR(70) NOT NULL, 
    [Country] NCHAR(25) NULL, 
    [Description] TEXT NULL
)
