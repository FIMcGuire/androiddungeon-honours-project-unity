using UnityEngine;
using System.Collections;
using System;

public class Initiative : IComparable<Initiative>
{
    public GameObject obj;
    public int initiative;

    public Initiative(GameObject obj, int initiative)
    {
        this.obj = obj;
        this.initiative = initiative;
    }

    public int CompareTo(Initiative other)
    {
        if (other == null)
        {
            return 1;
        }

        return initiative - other.initiative;
    }
}
