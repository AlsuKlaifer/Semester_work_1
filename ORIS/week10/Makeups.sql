﻿CREATE TABLE Makeups
(
	[Id] INT IDENTITY (1, 1) NOT NULL,
    [Category] VARCHAR (100) NOT NULL,
    [Description] TEXT NOT NULL,
    [Image] IMAGE NOT NULL,
	[Date] DATETIME NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
)
