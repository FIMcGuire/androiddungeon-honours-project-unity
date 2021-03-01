using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class OnlyLocalPlayer : NetworkBehaviour
{
    public GameObject PlayerCanvasObject;

    // Start is called before the first frame update
    void Start()
    {
        if (hasAuthority)
        {
            PlayerCanvasObject.SetActive(true);
        }
    }
}
