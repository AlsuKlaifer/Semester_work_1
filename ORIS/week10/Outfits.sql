CREATE TABLE [dbo].[Outfits] (
    [Id]     INT           IDENTITY (1, 1) NOT NULL,
    [Season] VARCHAR (100) NOT NULL,
    [Style]  VARCHAR (100) NOT NULL,
    [Image]  TEXT         NOT NULL,
    [Date]       DATETIME      DEFAULT (getdate()) NOT NULL,
    [Author]     INT           NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Outfit_author] FOREIGN KEY ([Author]) REFERENCES [dbo].[Users] ([Id])
);

