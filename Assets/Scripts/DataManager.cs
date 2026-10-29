using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class SerializationWrapper
{
    public List<InteractiveObjectDto> objects;
}

public interface IDataManager
{
    void Save(List<InteractiveObjectDto> data);
    List<InteractiveObjectDto> Load();
}


public class DataManager : IDataManager
{
    private readonly string _savePath;

    public DataManager()
    {
        _savePath = Path.Combine(Application.streamingAssetsPath, "savefile.json");
    }

    public void Save(List<InteractiveObjectDto> data)
    {
        if (data == null) return;

        SerializationWrapper wrapper = new SerializationWrapper { objects = data };
        string json = JsonUtility.ToJson(wrapper, true);

        string directory = Path.GetDirectoryName(_savePath);
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

        File.WriteAllText(_savePath, json);
        Debug.Log("[DataManager] Данные успешно сохранены.");
    }

    public List<InteractiveObjectDto> Load()
    {
        if (!File.Exists(_savePath))
        {
            Debug.LogWarning($"[DataManager] Файл сохранения не найден: {_savePath}");
            return null;
        }

        string json = File.ReadAllText(_savePath);
        SerializationWrapper wrapper = JsonUtility.FromJson<SerializationWrapper>(json);

        return wrapper?.objects;
    }
}



