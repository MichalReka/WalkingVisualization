using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DropdownHandler : MonoBehaviour
{
    Dropdown animalPrefabDropdown;
    GameObject animalPreview;

    public void RemoveComponents<T>(GameObject gameObject)
    {
        Component[] components = gameObject.GetComponentsInChildren(typeof(T), true);
        foreach (var c in components)
        {

            DestroyImmediate(c);
        }
    }
    public void AddComponentToChildren<T>(GameObject gameObject)
    {
        MeshRenderer[] components = gameObject.GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < components.Length; i++)
        {
            components[i].gameObject.AddComponent(typeof(T));
        }
    }
    public void FreezeObject(GameObject gameObject)
    {
        Rigidbody[] components = gameObject.GetComponentsInChildren<Rigidbody>();
        foreach (var c in components)
        {
            c.isKinematic = true;
        }
    }
    public void IncreaseChildrenScale(GameObject gameObject)
    {
        //każdy obiekt ma komponent MeshRenderer[], dzięki temu otrzymamy listę wszystkich potomków
        MeshRenderer[] components = gameObject.GetComponentsInChildren<MeshRenderer>(); 
        for (int i = 0; i < components.Length; i++)
        {
            //zmiana skali obiektu wiąże się ze zmianą jego pozycji
            //inaczej pokazany by był niewłaściwy wygląd całego modelu osobnika
            components[i].gameObject.transform.localScale = components[i].gameObject.transform.localScale * 1.1f;
            components[i].gameObject.transform.localPosition = components[i].gameObject.transform.localPosition * 1.1f;
        }
    }
    public void CentralizeChildrenY(GameObject gameObject)
    {
        float averageY = 0;
        float averageZ = 0;
        float averageX = 0;
        MeshRenderer[] components = gameObject.GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < components.Length; i++)
        {
            averageY += components[i].gameObject.transform.localPosition.y;
            averageZ += components[i].gameObject.transform.localScale.x;
            averageX += components[i].gameObject.transform.localScale.z;
        }
        averageY = averageY / components.Length;
        averageZ = averageZ / components.Length;
        averageX = averageX / components.Length;
        for (int i = 0; i < components.Length; i++)
        {
            components[i].gameObject.transform.localPosition -= new Vector3(averageX, averageY, averageZ);
        }
    }
    void Start()
    {
        animalPrefabDropdown = transform.GetComponent<Dropdown>();
        animalPrefabDropdown.options.Add(new Dropdown.OptionData() { text = "tetrapod1" });
        animalPrefabDropdown.options.Add(new Dropdown.OptionData() { text = "tetrapod2" });
        animalPrefabDropdown.options.Add(new Dropdown.OptionData() { text = "tetrapod3" });
        animalPrefabDropdown.options.Add(new Dropdown.OptionData() { text = "owad" });
        animalPrefabDropdown.onValueChanged.AddListener(delegate
        {
            if(gameObject.activeInHierarchy)
            {
                DropdownValueChanged(animalPrefabDropdown.options[animalPrefabDropdown.value].text);
            }
        });
        DropdownValueChanged(animalPrefabDropdown.options[0].text);
    }
    private void OnEnable()
    {
        if (animalPrefabDropdown)
        {
            animalPrefabDropdown.onValueChanged.AddListener(delegate
                    {
                        DropdownValueChanged(animalPrefabDropdown.options[animalPrefabDropdown.value].text);
                    });
            DropdownValueChanged(animalPrefabDropdown.options[0].text);
        }

    }
    IEnumerator ResizeAnimalPreview(AnimalPrefabCollision animalPrefabCollision)
    {

        while (animalPrefabCollision.ifScaled == false)
        {
            yield return new WaitForFixedUpdate();
            IncreaseChildrenScale(animalPreview);
        }
    }
    void DropdownValueChanged(string animalName)
    {
        Destroy(animalPreview);
        animalPreview = Instantiate(Resources.Load("Prefabs/" + animalName) as GameObject);
        var animalPrefabCollision = animalPreview.AddComponent<AnimalPrefabCollision>();
        animalPreview.transform.SetParent(GameObject.Find("Borders").transform);
        float x = GameObject.Find("rightBorder").transform.localPosition.x + GameObject.Find("leftBorder").transform.localPosition.x;
        float y = GameObject.Find("upBorder").transform.localPosition.y + GameObject.Find("downBorder").transform.localPosition.y;
        animalPreview.transform.localPosition = new Vector3(x / 2, y / 2, -20);
        RemoveComponents<JointHandler>(animalPreview);
        RemoveComponents<AnimalBodyPart>(animalPreview);
        FreezeObject(animalPreview);
        AddComponentToChildren<AnimalPrefabChildrenCollision>(animalPreview);
        CentralizeChildrenY(animalPreview);
        animalPrefabCollision.ifScaled = false;
        StartCoroutine("ResizeAnimalPreview", animalPrefabCollision);
    }
    private void OnDisable()
    {
        animalPrefabDropdown.onValueChanged.RemoveAllListeners();
        StopCoroutine("ResizeAnimalPreviewd");
        animalPrefabDropdown.value = 0;
        Destroy(animalPreview);
    }

}

