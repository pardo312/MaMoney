using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class ExampleMaxScoreModel : MonoBehaviour
{
    private const string baseUrl = "https://test-firebasedll-default-rtdb.firebaseio.com/MaxScore";
    public void GetMaxScore()
    {
        FirebaseRequest.instance.FirebaseObjectRequestPetiton<int>(baseUrl,
            null,
            (success, maxScore) =>
            {
                Debug.Log(maxScore);
            },
            RequestType.GET);
    }

    public void SetMaxScore()
    {
        FirebaseRequest.instance.FirebaseObjectRequestPetiton<MaxScorePayload>(baseUrl,
            new MaxScorePayload(){MaxScore = 3 },
            (success, maxScore) =>
            {
                Debug.Log(maxScore.MaxScore);
            },
            RequestType.PATCH);
    }

    public void DeleteMaxScore()
    {
        FirebaseRequest.instance.FirebaseObjectRequestPetiton<int>(baseUrl,
            null,
            (success, maxScore) =>
            {
                Debug.Log(maxScore);
            },
            RequestType.DELETE);
    }
}
public class MaxScorePayload
{
    public int MaxScore;
}
