using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class JoytickMoveController : JoytickController
{
    public static JoytickMoveController Intansce;
    private void Awake()
    {
        Intansce = this;
    }
    // Start is called before the first frame update

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        direction = Vector3.zero;
    }
}
