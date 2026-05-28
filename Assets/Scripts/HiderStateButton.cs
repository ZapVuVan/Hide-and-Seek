using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HiderStateButton : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] PlayerController controller;
    public void OnPointerDown(PointerEventData eventData)
    {
        controller.TransitionToState(controller.playerHiderState);
    }

}
