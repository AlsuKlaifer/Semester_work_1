CREATE TABLE Makeups (
    [Id]          INT           IDENTITY (1, 1) NOT NULL,
    [Category]    VARCHAR (100) NOT NULL,
    [Description] TEXT          NOT NULL,
    [Image]      TEXT          NOT NULL,
    [Date]       DATETIME      DEFAULT (getdate()) NOT NULL,
    [Author]     INT           NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Makeup_author] FOREIGN KEY ([Author]) REFERENCES [dbo].[Users] ([Id])
);

