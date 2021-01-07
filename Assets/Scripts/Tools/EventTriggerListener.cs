using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Tianbo.Wang
{

    public class EventTriggerListener : EventTrigger
    {
        public delegate void MouseAction(GameObject go);
        public MouseAction onEnter;
        public MouseAction onExit;
        public MouseAction onClick;
        public MouseAction onDown;
        public MouseAction onUp;
        public MouseAction onBeginDrag;
        public MouseAction onDrag;
        public MouseAction onEndDrag;
        public MouseAction onSelect;
        public MouseAction onDeSelect;


        public static EventTriggerListener Get(GameObject go)
        {
            EventTriggerListener eventTriggerListener = go.GetComponent<EventTriggerListener>();
            if (eventTriggerListener == null)
            {
                eventTriggerListener = go.AddComponent<EventTriggerListener>();
            }
            return eventTriggerListener;
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            onClick?.Invoke(gameObject);
        }
        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            onDown?.Invoke(gameObject);
        }
        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            onUp?.Invoke(gameObject);
        }
        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            onEnter?.Invoke(gameObject);
        }
        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            onExit?.Invoke(gameObject);
        }
        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);
            onBeginDrag?.Invoke(gameObject);
        }
        public override void OnDrag(PointerEventData eventData)
        {
            base.OnDrag(eventData);
            onDrag?.Invoke(gameObject);
        }
        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
            onEndDrag?.Invoke(gameObject);
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            onSelect?.Invoke(gameObject);
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
            onDeSelect?.Invoke(gameObject);
        }
    }
}