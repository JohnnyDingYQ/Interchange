using System.Collections.Generic;
using UnityEngine;

public class CarDriver: MonoBehaviour
{
    public void Awake()
    {
        DemandsSatisfer.Drive += Drive;
    }

    public void OnDestroy()
    {
        DemandsSatisfer.Drive -= Drive;
    }

    public void Drive(IEnumerable<Path> paths)
    {
        Debug.Log("I drive!");
    }
}