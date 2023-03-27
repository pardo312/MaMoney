using Newtonsoft.Json;
using System;
using System.Collections.Generic;
public class TransactionDto
{
    public ulong transactionId;
    public string name;
    public TransactionType type;
    public Accounts account;
    public Accounts targetAccount;
    public int amount;
    public string[] tags;
    public string date;
}

public enum Accounts
{
    COLPATRIA,
    NEQUI,
    EFECTIVO
}

public enum TransactionType
{
    INCOME,
    EXPENSE,
    TRANSFER
}
