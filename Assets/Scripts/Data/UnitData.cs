using UnityEngine;

[CreateAssetMenu(fileName = "NewUnit", menuName = "Data/UnitData")]
public class UnitData : ScriptableObject
{
    public string unitName;
    public int maxHP;
    public int minSpeed;
    public int maxSpeed;
}