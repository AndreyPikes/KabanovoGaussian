using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

[System.Serializable]
public class SerializationWrapper
{
    public List<InteractiveObject> objects;
}

public class DataManager : MonoBehaviour
{
    [SerializeField] private Transform objectsContainer;
    [SerializeField] private Button saveButton;
    private string savePath;

    private List<InteractiveObject> currentObjectsList;

    private void Start()
    {
        saveButton.onClick.AddListener(OnSaveClick);

        savePath = Path.Combine(Application.persistentDataPath, "savefile.json");
    }

    private void OnSaveClick()
    {
        var containerChilds = objectsContainer.GetComponentsInChildren<SelectableObject>();
        currentObjectsList  = new();
        foreach (var obj in containerChilds)
        {
            currentObjectsList.Add(obj.UpdateAndGetInteractiveObject());
        }

        foreach (var obj in currentObjectsList)
        {
            Debug.LogWarning(obj.Description);
        }

        // Создаем объект обертки и передаем туда ваш список
        SerializationWrapper wrapper = new SerializationWrapper { objects = currentObjectsList };

        // Сериализуем обертку вместо самого списка
        string json = JsonUtility.ToJson(wrapper, true);

        // 5. Записываем строку в файл на диск
        File.WriteAllText(savePath, json);
        Debug.Log(currentObjectsList.Count);
        Debug.Log(savePath);
        Debug.Log(json);

#if UNITY_EDITOR_OSX
    // Откроет Finder прямо с папкой сохранения при запуске игры в редакторе
    System.Diagnostics.Process.Start("open", Application.persistentDataPath);
#endif
    }

    
}
