using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PieChartController : MonoBehaviour
{
    public List<Image> imagesPieChart;
    public List<float> values;

    public void Start()
    {
        SetValue(values);
    }

    public void SetValue(List<float> valuesToSet)
    {
        float totalValues = 0;
        for (int i = 0; i < valuesToSet.Count; i++)
        {
            totalValues += FindPercentage(valuesToSet, i);
            Debug.Log(totalValues);
            imagesPieChart[i].fillAmount = totalValues;
        }
    }

    private float FindPercentage(List<float> valuesToSet, int index)
    {
        float totalAmount = 0;
        for (int i = 0; i < valuesToSet.Count; i++)
            totalAmount += valuesToSet[i];

        return valuesToSet[index] / totalAmount;
    }
}
