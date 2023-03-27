using Newtonsoft.Json;
using System;
using UnityEngine;
public class FirebaseTransactionModel : MonoBehaviour
{
    public FirebaseEndpointsScriptableObject firebaseEndpoints;
    private string transactionUrl;
    public void Awake()
    {
        transactionUrl = firebaseEndpoints.baseUrl + firebaseEndpoints.transactionsRoute;
    }

    public void GETUsers()
    {
        FirebaseRequest.instance.FirebaseListRequestPetiton<TransactionDto>(transactionUrl, null, RequestUsersCallback, RequestType.GET);
    }

    public void SENDNewUser()
    {
        FirebaseRequest.instance.FirebaseListRequestPetiton<TransactionDto>(transactionUrl, new TransactionDto
        {
            name = "Hamburguesa",
            type = TransactionType.EXPENSE,
            account = Accounts.COLPATRIA,
            amount = 10000,
            date = DateTime.Now.Date.ToString(),
            tags = new string[] { "Necessary" },
        },
        (success, data) =>
         {
             if (success)
             {
                 Debug.Log(success);
             }
         }
         , RequestType.PATCH);
    }

    public void DELETEUser()
    {
        FirebaseRequest.instance.FirebaseListRequestPetiton<TransactionDto>(transactionUrl, 0, null, RequestType.DELETE);
    }

    public void RequestUsersCallback(bool success, FirebaseListDto<TransactionDto> data)
    {
        Debug.Log($"Success: {success}");

        if (success)
            Debug.Log(JsonConvert.SerializeObject(data));
    }

}
