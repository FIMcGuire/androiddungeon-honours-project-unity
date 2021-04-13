using UnityEngine;

public static class LongClick
{
    private static float timer;

    public static bool IsLongClick(int _mouseButton)
    {
        if(Input.GetMouseButton(_mouseButton))
        {
            timer += Time.deltaTime;
            if (timer >= 1f)
            {
                timer = 0;
                return true;
            }
        }
        if (Input.GetMouseButtonUp(_mouseButton))
        {
            timer = 0;
            return false;
        }

        return false;
    }
}
