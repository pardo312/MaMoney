using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class FirebaseRequest : MonoBehaviour
{
    #region Instance
    public static FirebaseRequest instance;

    [RuntimeInitializeOnLoadMethod]
    public static void Init()
    {
        GameObject go = new GameObject();
        go.name = "FirebasRequestHandler";
        FirebaseRequest fbRequest = (FirebaseRequest)go.AddComponent(typeof(FirebaseRequest));
        instance = fbRequest;
        DontDestroyOnLoad(fbRequest);
    }
    #endregion Instance

    #region  Methods
    #region SingleObject Request 
    public void FirebaseObjectRequestPetiton<T>(string _url, object _payload = null, Action<bool, T> _callback = null, RequestType _type = RequestType.GET)
    {
        string data = null;
        if (_payload != null)
        {
            data = JsonConvert.SerializeObject(_payload);

            if (_type == RequestType.PATCH)
            {
                _url = _url.Remove(_url.LastIndexOf("/") + 1, _url.Length - (_url.LastIndexOf("/") + 1));
            }

            _url += ".json";
            if (_type == RequestType.GET)
                _url += SetGetParameters(data);
        }
        else
        {
            _url += ".json";
        }

        StartCoroutine(WebRequestObject(_url, _type, data, _callback));
    }

    IEnumerator WebRequestObject<T>(string _url, RequestType _type, string data, Action<bool, T> _callback)
    {
        UnityWebRequest request;
        switch (_type)
        {
            case RequestType.GET:
                request = UnityWebRequest.Get(_url);
                break;
            case RequestType.POST:
                request = UnityWebRequest.Post(_url, data);
                break;
            case RequestType.PUT:
                request = UnityWebRequest.Put(_url, data);
                break;
            case RequestType.PATCH:
                request = UnityWebRequest.Put(_url, data);
                request.method = "PATCH";
                break;
            case RequestType.DELETE:
                request = UnityWebRequest.Delete(_url);
                break;
            default:
                request = UnityWebRequest.Get(_url);
                break;
        }

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            string result = "";
            if (request.downloadHandler != null)
            {
                result = request.downloadHandler.text;
                result = result.Replace("\\", string.Empty);
            }

            T responseR = default(T);
            if (!String.IsNullOrEmpty(result) && result != "null")
                responseR = JsonConvert.DeserializeObject<T>(result);

            if (_callback != null)
            {
                if (IsAnyNotNullOrEmpty(responseR))
                    _callback(responseR != null, responseR);
                else
                    _callback(false, default(T));
            }
        }
    }
    #endregion SingleObject Request 

    #region List Request Petiton
    public void FirebaseListRequestPetiton<T>(string _url, object _payload = null, Action<bool, FirebaseListDto<T>> _callback = null, RequestType _type = RequestType.GET, bool patchWithFinalPayload = false)
    {
        string json = null;
        if (_payload != null)
        {
            json = "";
            if (_payload.GetType() == typeof(string))
                json = (string)_payload;
            else
                json = JsonConvert.SerializeObject(_payload);


            if (_type == RequestType.PATCH)
            {
                if (!patchWithFinalPayload)
                {
                    SetPayloadPatchRequest(_url, json, (finalJson) =>
                      {
                          FirebaseListRequestPetiton<T>(_url, finalJson, _callback, _type, true);
                      });
                    return;
                }
                else
                {
                    if (_payload.GetType() != typeof(ChildCountPayload))
                        _url += "/List";
                }
            }
            else if (_type == RequestType.DELETE)
            {
                LowerChildCount(_url, (int)_payload, (key) =>
                {
                    _url += $"/List/{(int)key}.json";
                    StartCoroutine(WebRequestObject(_url, _type, json, _callback));
                });
                return;
            }


            _url += ".json";
            if (_type == RequestType.GET)
                _url += SetGetParameters(json);
        }
        else
        {
            _url += ".json";
        }
        StartCoroutine(WebRequestObject(_url, _type, json, _callback));
    }

    IEnumerator WebRequestList<T>(string _url, RequestType _type, string data, Action<bool, FirebaseListDto<T>> _callback, Type _payloadType, bool patchWithFinalPayload)
    {
        UnityWebRequest request;
        switch (_type)
        {
            case RequestType.GET:
                request = UnityWebRequest.Get(_url);
                break;
            case RequestType.POST:
                request = UnityWebRequest.Post(_url, data);
                break;
            case RequestType.PUT:
                request = UnityWebRequest.Put(_url, data);
                break;
            case RequestType.PATCH:
                request = UnityWebRequest.Put(_url, data);
                request.method = "PATCH";
                break;
            case RequestType.DELETE:
                request = UnityWebRequest.Delete(_url);
                break;
            default:
                request = UnityWebRequest.Get(_url);
                break;
        }

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            string result = request.downloadHandler.text;
            result = result.Replace("\\", string.Empty);

            FirebaseListDto<T> responseR = default(FirebaseListDto<T>);
            if (!String.IsNullOrEmpty(result) && result != "null")
            {
                responseR = JsonConvert.DeserializeObject<FirebaseListDto<T>>(result);
                if (responseR != null && responseR.List != null)
                    responseR.List.RemoveAll(item => item == null);
            }

            if (_callback != null)
            {
                if (_type == RequestType.PATCH && patchWithFinalPayload && _payloadType != typeof(ChildCountPayload))
                {
                    if (!String.IsNullOrEmpty(result) && result != "null")
                        _callback(true, null);
                    else
                        _callback(false, null);
                }
                else
                {
                    if (IsAnyNotNullOrEmpty(responseR))
                        _callback(responseR != null, responseR);
                    else
                        _callback(false, default(FirebaseListDto<T>));
                }
            }
        }
    }
    #region Patch Payload Set
    private void SetPayloadPatchRequest(string _url, string _originalJson, Action<string> _finalPayloadCallback = null)
    {
        string dq = ('"' + "");
        GetChildIndexList(_url, (listOfIndexes) =>
         {
             SetChildIndexList(_url, listOfIndexes);

             string finalJson = "{" + dq + listOfIndexes[listOfIndexes.Count - 1] + dq + ":" + _originalJson + "}";
             _finalPayloadCallback?.Invoke(finalJson);
         });
    }

    public void GetChildIndexList(string _url, Action<List<int>> _response)
    {
        FirebaseObjectRequestPetiton<List<int>>(_url + "/ChildKeys", null, (success, data) =>
          {
              if (success)
                  _response?.Invoke(data);
              else
              {
                  SetChildIndexList(_url, new List<int>());
                  _response.Invoke(new List<int>());
              }
          }, RequestType.GET);
    }

    private void SetChildIndexList(string _url, List<int> childKeys, bool replace = false)
    {
        if (!replace)
        {
            int childIndexToAdd = childKeys.Count > 0 ? ((childKeys[childKeys.Count - 1]) + 1) : 0;
            childKeys.Add(childIndexToAdd);
        }

        FirebaseListRequestPetiton<ChildCountPayload>(
            _url,
            new ChildCountPayload() { ChildKeys = childKeys }, null,
            RequestType.PATCH, true
        );
    }
    #endregion Patch Payload Set

    #region Delete Set
    public void LowerChildCount(string _url, int deletedChildIndex, Action<int> deletedChildKey)
    {
        GetChildIndexList(_url, (listIndex) =>
         {
             deletedChildKey?.Invoke(listIndex[deletedChildIndex]);

             listIndex.RemoveAt(deletedChildIndex);
             SetChildIndexList(_url, listIndex, replace: true);
         });
    }
    #endregion Delete Set
    #endregion List Request Petiton

    #region Helpers
    private string SetGetParameters(string json)
    {
        string paramsUrl = "";
        Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        foreach (KeyValuePair<string, object> entry in dictionary)
        {
            if (paramsUrl.Length > 0)
                paramsUrl += "&";
            paramsUrl += $"{entry.Key}={entry.Value}";
        }
        return $"?{paramsUrl}";
    }

    private bool IsAnyNotNullOrEmpty(object myObject)
    {
        if (myObject == null)
            return true;
        if (myObject.GetType() == typeof(List<int>) && ((List<int>)myObject) != null)
            return true;
        bool anyParamNotNull = true;
        foreach (FieldInfo pi in myObject.GetType().GetRuntimeFields())
        {
            if (pi.GetValue(myObject) == null)
            {
                anyParamNotNull = false;
            }
        }
        return anyParamNotNull;
    }
    #endregion Helpers
    #endregion  Methods
}
public class FirebaseListDto<T>
{
    public int ChildCount;
    public List<T> List;
}
