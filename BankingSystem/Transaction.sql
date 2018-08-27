CREATE TABLE [dbo].[Transaction](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AccountNumber] [bigint] NOT NULL,
	[TransactionType] [int] NOT NULL,
	[Credit] [decimal](18, 2) NULL,
	[Debit] [decimal](18, 2) NULL,
	[TransactionReference] [int] NULL,
	[Balance] [decimal](18, 2) NOT NULL,
	[TransactionDate] [datetime] NOT NULL,
 CONSTRAINT [PK_Transaction] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO