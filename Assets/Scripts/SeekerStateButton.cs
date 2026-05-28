using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SeekerStateButton : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] PlayerController controller;
    public void OnPointerDown(PointerEventData eventData)
    {
        if (controller != null)
        {
            controller.TransitionToState(controller.playerSeekerState);
        }

    }

}
