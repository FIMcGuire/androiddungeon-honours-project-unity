using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class DragOrderObject : MonoBehaviour, IPointerEnterHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    DragOrderContainer container = null;

    void Start()
    {
        container = GetComponentInParent<DragOrderContainer>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        container.objectBeingDragged = this.gameObject;
    }

    public void OnDrag(PointerEventData eventData)
    {
        //Could add code here to make the dragged object follow mouse/touch
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (container.objectBeingDragged == this.gameObject) container.objectBeingDragged = null;
        /*List<GameObject> newList = new List<GameObject>();
        foreach (Transform obj in container.transform)
        {
            newList.Add(obj.gameObject);
        }
        GameObject.Find("Host").GetComponent<DNDHost>().initiativeObjects = newList;
        foreach (var obj in GameObject.Find("Host").GetComponent<DNDHost>().initiativeObjects)
        {
            Debug.Log("New List: " + obj.name);
        }*/
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        GameObject objectBeingDragged = container.objectBeingDragged;
        if (objectBeingDragged != null && objectBeingDragged != this.gameObject)
        {
            objectBeingDragged.transform.SetSiblingIndex(this.transform.GetSiblingIndex());
        }
    }
}
