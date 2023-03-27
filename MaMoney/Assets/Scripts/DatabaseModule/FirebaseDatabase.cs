using Newtonsoft.Json;
using System;
using System.Linq;
using UnityEngine;

public class FirebaseDatabase : MonoBehaviour, IDatabase
{
    public FirebaseEndpointsScriptableObject firebaseEndpoints;
    private string transactionUrl;
    public void Awake()
    {
        transactionUrl = firebaseEndpoints.baseUrl + firebaseEndpoints.transactionsRoute;
    }

    [ContextMenu("Hello")]
    public void TestGet()
    {
        GetEntries(account: Accounts.NEQUI);
    }

    public void GetEntries(TransactionType? typeFilter = null, Accounts? account = null, DateTime? dateFilter = null)
    {
        FirebaseRequest.instance.FirebaseListRequestPetiton<TransactionDto>(transactionUrl, null, (success, transactions) =>
        {
            var list = transactions.List.Where(transaction =>
            {
                bool validated = true;
                if (typeFilter.HasValue)
                    validated = transaction.type == typeFilter.Value;
                if (account.HasValue)
                    validated = transaction.account == account.Value;
                if (dateFilter.HasValue)
                    validated = DateTime.Parse(transaction.date).Date == dateFilter.Value;

                return validated;
            });

            Debug.Log(JsonConvert.SerializeObject(list));
        },
        RequestType.GET);
    }



    [ContextMenu("Create")]
    private void TestCreate()
    {
        CreateTransaction(new TransactionDto()
        {
            name = "Taxi",
            type = TransactionType.EXPENSE,
            account = Accounts.NEQUI,
            amount = 10000,
            date = DateTime.Now.Date.ToString(),
            tags = new string[] { "Innecesary" },
        });
    }

    public void CreateTransaction(TransactionDto transactionDto)
    {
        FirebaseRequest.instance.FirebaseListRequestPetiton<TransactionDto>(transactionUrl, transactionDto,
        (success, data) =>
         {
             if (success)
                 Debug.Log(success);
         }
         , RequestType.PATCH);
    }


    [ContextMenu("Delete")]
    private void TestDel()
    {
        DeleteEntrie(0);
    }

    public void DeleteEntrie(ulong transactionId)
    {
        FirebaseRequest.instance.FirebaseListRequestPetiton<TransactionDto>(transactionUrl, 0, null, RequestType.DELETE);
    }
}
