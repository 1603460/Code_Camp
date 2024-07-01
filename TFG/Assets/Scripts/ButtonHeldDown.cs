using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonHeldDown : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Image image;
    public Sprite newSprite;
    public Sprite oldSprite;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        image.sprite = newSprite;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        image.sprite = oldSprite;
    }
}
