using UnityEngine;

[CreateAssetMenu(fileName = "FirebaseEndpoints", menuName = "MaMoney/FirebaseEndpoints", order = 1)]
public class FirebaseEndpointsScriptableObject : ScriptableObject
{
    public string baseUrl;
    public string transactionsRoute;
}
