CREATE TABLE [dbo].[Songs]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY(1, 1), 
    [Id_author] BIGINT NOT NULL, 
    [Name] NCHAR(50) NOT NULL, 
    [Length] TIME NULL, 
    [Year] SMALLINT NULL, 
    [Path_location] NVARCHAR(MAX) NOT NULL, 
    [Path_image] NVARCHAR(150) NULL, 
    CONSTRAINT [FK_Songs_PK_Authors] FOREIGN KEY (Id_author) REFERENCES Authors(Id)
)
