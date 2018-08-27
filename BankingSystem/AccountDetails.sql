CREATE TABLE [dbo].[AccountDetails](
	[AccountId] [int] NOT NULL,
	[AccountNumber] [bigint] IDENTITY(9000000000,1) NOT NULL,
	[FirstName] [nvarchar](50) NOT NULL,
	[LastName] [nvarchar](50) NOT NULL,
	[Address] [nvarchar](50) NOT NULL,
	[EmailAddress] [nvarchar](50) NOT NULL,
	[RowVersion] [timestamp] NOT NULL,
 CONSTRAINT [PK_AccountNumber] PRIMARY KEY CLUSTERED 
(
	[AccountNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO