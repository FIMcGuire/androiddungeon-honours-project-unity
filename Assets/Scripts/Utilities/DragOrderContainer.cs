using UnityEngine;

public class DragOrderContainer : MonoBehaviour
{
    public GameObject objectBeingDragged { get; set; }

    private void Awake()
    {
        objectBeingDragged = null;
    }
}
