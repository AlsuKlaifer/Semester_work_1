CREATE TABLE [dbo].[Designers] (
    [Id]      INT           IDENTITY (1, 1) NOT NULL,
    [Name]    VARCHAR (100) NOT NULL,
    [Country] VARCHAR (100) NOT NULL,
    [Brand]   DATETIME      NOT NULL,
    [Image]   TEXT         NOT NULL,
    [Date]       DATETIME      DEFAULT (getdate()) NOT NULL,
    [Author]     INT           NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Designer_author] FOREIGN KEY ([Author]) REFERENCES [dbo].[Users] ([Id])
);

