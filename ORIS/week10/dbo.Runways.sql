CREATE TABLE dbo.Runways (
    [Id]         INT           IDENTITY (1, 1) NOT NULL,
    [ModelName]  VARCHAR (100) NOT NULL,
    [Country]    VARCHAR (100) NOT NULL,
    [Brand]      VARCHAR (100) NOT NULL,
    [Collection] VARCHAR (100) NOT NULL,
    [Image]      TEXT          NOT NULL,
    [Date]       DATETIME      DEFAULT (getdate()) NOT NULL,
    [Author]     INT		   NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Post_author] FOREIGN KEY ([Author]) REFERENCES [dbo].[Users] ([Id])
);
