CREATE TABLE [dbo].[Wallet](
	[AccountNumber] [bigint] NOT NULL,
	[Balance] [decimal](18, 2) NOT NULL,
	[RowVersion] [timestamp] NOT NULL
) ON [PRIMARY]
GO