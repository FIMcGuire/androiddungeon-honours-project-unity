using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Utils
{
    public static void DrawLine(Vector3 start, Vector3 end)
    {
        Debug.DrawLine(start, end);
    }

    public static Vector3 GetMouseWorldPosition()
    {
        Vector3 vec = GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
        vec.z = 0f;
        return vec;
    }

    public static Vector3 GetMouseWorldPositionWithZ()
    {
        return GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
    }

    public static Vector3 GetMouseWorldPositionWithZ(Camera worldCamera)
    {
        return GetMouseWorldPositionWithZ(Input.mousePosition, worldCamera);
    }

    public static Vector3 GetMouseWorldPositionWithZ(Vector3 screenPosition, Camera worldCamera)
    {
        Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
        return worldPosition;
    }

    public static Vector3 GetVectorFromAngle(float angle)
    {
        // angle = 0 -> 360
        float angleRad = angle * (Mathf.PI / 180f);
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }

    public static string getModifier(int stat)
    {
        switch (stat)
        {
            case 1:
                return "-5";
            case 2:
            case 3:
                return "-4";
            case 4:
            case 5:
                return "-3";
            case 6:
            case 7:
                return "-2";
            case 8:
            case 9:
                return "-1";
            case 10:
            case 11:
                return "+0";
            case 12:
            case 13:
                return "+1";
            case 14:
            case 15:
                return "+2";
            case 16:
            case 17:
                return "+3";
            case 18:
            case 19:
                return "+4";
            case 20:
                return "+5";
            default:
                return "+0";
        }
    }
}
