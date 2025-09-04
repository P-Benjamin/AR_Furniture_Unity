using UnityEngine;

[CreateAssetMenu(menuName = "AR/Item Info", fileName = "ItemInfo")]
public class ObjectInfo : ScriptableObject
{
    public string title;
    [TextArea(2, 6)] public string description;          
    public string dimensions;      
    public string price;           

}