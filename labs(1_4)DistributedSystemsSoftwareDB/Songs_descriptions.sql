CREATE TABLE [dbo].[Songs_descriptions]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY(1, 1), 
    [Id_song] BIGINT NOT NULL UNIQUE, 
    [Description] NVARCHAR(MAX) NOT NULL, 
    CONSTRAINT [FK_Songs_descriptions_PK_Songs] FOREIGN KEY (Id_song) REFERENCES Songs(Id)
)
