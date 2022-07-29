CREATE TABLE [dbo].[WoodDeals] (
    [id]               INT           NOT NULL IDENTITY,
    [sellerName]       VARCHAR (255) NULL,
    [sellerInn]        VARCHAR (50)  NULL,
    [buyerName]        VARCHAR (50)  NULL,
    [buyerInn]         VARCHAR (50)  NULL,
    [woodVolumeBuyer]  FLOAT (53)    NULL,
    [woodVolumeSeller] FLOAT (53)    NULL,
    [dealDate]         DATE          NULL,
    [dealNumber]       VARCHAR (50)  NULL,
    PRIMARY KEY CLUSTERED ([id] ASC)
);


