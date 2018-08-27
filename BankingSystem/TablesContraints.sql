ALTER TABLE [dbo].[Account] ADD  CONSTRAINT [DF_Account_CreatedDate]  DEFAULT (getutcdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[Transaction] ADD  CONSTRAINT [DF_Transaction_TransactionDate]  DEFAULT (getutcdate()) FOR [TransactionDate]
GO
ALTER TABLE [dbo].[Wallet] ADD  CONSTRAINT [DF_Wallet_Balance]  DEFAULT ((0)) FOR [Balance]
GO
ALTER TABLE [dbo].[AccountDetails]  WITH CHECK ADD  CONSTRAINT [FK_AccountDetails_Account] FOREIGN KEY([AccountId])
REFERENCES [dbo].[Account] ([Id])
GO
ALTER TABLE [dbo].[AccountDetails] CHECK CONSTRAINT [FK_AccountDetails_Account]
GO
ALTER TABLE [dbo].[Transaction]  WITH CHECK ADD  CONSTRAINT [FK_Transaction_AccountDetails] FOREIGN KEY([AccountNumber])
REFERENCES [dbo].[AccountDetails] ([AccountNumber])
GO
ALTER TABLE [dbo].[Transaction] CHECK CONSTRAINT [FK_Transaction_AccountDetails]
GO
ALTER TABLE [dbo].[Wallet]  WITH CHECK ADD  CONSTRAINT [FK_Wallet_AccountDetails] FOREIGN KEY([AccountNumber])
REFERENCES [dbo].[AccountDetails] ([AccountNumber])
GO
ALTER TABLE [dbo].[Wallet] CHECK CONSTRAINT [FK_Wallet_AccountDetails]
GO