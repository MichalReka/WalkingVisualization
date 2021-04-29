using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AnimalListItem : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public static string selectedAnimalIndex;
    public static string selectedAnimalPrefabName;
    private void Start()
    {
        gameObject.transform.GetComponent<Animator>().keepAnimatorControllerStateOnDisable = true;
        gameObject.transform.GetComponent<Animator>().Play("Normal", -1);
    }
    public void OnSelect(BaseEventData eventData)
    {
        selectedAnimalIndex = gameObject.transform.Find("ID").GetComponent<Text>().text;
        selectedAnimalPrefabName = gameObject.transform.Find("PrefabName").GetComponent<Text>().text;
        DatabasePanel.ActivateButton();
    }
    public void OnDeselect(BaseEventData eventData)
    {
        //https://stackoverflow.com/questions/39150165/how-do-i-find-which-object-is-eventsystem-current-ispointerovergameobject-detect/39150616
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = Input.mousePosition;
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, raycastResults);
        if (raycastResults.Count > 0)
        {
            foreach (var go in raycastResults)
            {
                if(go.gameObject.name==DatabasePanel.showIndButton.name)
                {
                    return;
                }
            }
        }
        DatabasePanel.DeactivateButton();
    }
}
