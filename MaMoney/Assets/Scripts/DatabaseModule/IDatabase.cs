using System;

public interface IDatabase
{
    public void CreateTransaction(TransactionDto transactionDto);
    public void GetEntries(TransactionType? typeFilter = null, Accounts? account = null, DateTime? dateFilter = null);
    public void DeleteEntrie(ulong transactionId);
}
