using System.Collections.Generic;
using UnityEngine;

public enum ObjectType
{
    Не_задано,
    Дерево,
    Куст,
    Грядка
}

public enum PlantFamily
{
    Не_задано,
    Яблоня,
    Груша,
    Слива,
    Абрикос,
    Вишня,
    Смородина_черная,
    Смородина_красная,
    Смородина_белая,
    Крыжовник,
    Жимолость,
    Ежевика,
    Малина
}

public enum RipeningPeriod
{
    Не_задано,
    Ранний,
    Средний,
    Поздний
}

[System.Serializable]
public class SerializationWrapper
{
    public string Date;
    public List<InteractiveObjectDto> Objects;
}

[System.Serializable]
public class InteractiveObjectDto
{
    public Vector3 Position;
    public Vector3 Scale;
    public Vector3 Rotation;
    public ObjectType Type;
    public PlantFamily Family;
    public int PlantingYear;
    public RipeningPeriod Ripening;
    public string Features;
}
