using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class ScaleButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler 
{
	RectTransform buttonRect;
	float scaleDuration = 0.15f;
	
	void Start()
	{
		buttonRect = GetComponent<RectTransform>();
	}
	
	public void OnPointerDown(PointerEventData eventData)
	{
		buttonRect.DOScale(new Vector3(1.1f, 1.1f, 1.1f), scaleDuration);
	}
	
	
	public void OnPointerUp(PointerEventData eventData)
	{
		buttonRect.DOScale(new Vector3(1f, 1f, 1f), scaleDuration);
		
	}

}

