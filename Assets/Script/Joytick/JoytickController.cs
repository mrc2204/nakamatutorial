using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JoytickController : MonoBehaviour, IEndDragHandler, IDragHandler, IBeginDragHandler
{
    public Vector3 direction;

    [SerializeField]
    Transform BGJoyTick;
    [SerializeField]
    Transform JoyTick;

    public Vector3 OriginalPos = Vector3.zero;

    Image imJoytick;
    Image imBGJoytick;
    public virtual void Start()
    {
        OriginalPos = BGJoyTick.transform.position;
        imJoytick = JoyTick.GetComponent<Image>();
        imBGJoytick = BGJoyTick.GetComponent<Image>();
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        BGJoyTick.position = eventData.position;
        direction = Vector3.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Moveing(eventData.position);
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        JoyTick.transform.localPosition = Vector3.zero;
        BGJoyTick.position = OriginalPos;
    }
    private void Moveing(Vector3 touchPos)
    {
        Vector2 offset = touchPos - BGJoyTick.position;
        Vector3 realdirection = Vector3.ClampMagnitude(offset, 20.0f);
        direction = realdirection.normalized;
        JoyTick.position = new Vector2(BGJoyTick.position.x + realdirection.x, BGJoyTick.position.y + realdirection.y);
    }
}
