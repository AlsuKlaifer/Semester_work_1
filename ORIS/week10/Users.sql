CREATE TABLE Users
(
	[Id]       INT           IDENTITY (1, 1) NOT NULL,
	[Name]	   VARCHAR (50) NOT NULL,
	[Number]   VARCHAR (15)	NOT NULL,
    [Login]    VARCHAR (100) NOT NULL,
    [Password] VARCHAR (100) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
)
