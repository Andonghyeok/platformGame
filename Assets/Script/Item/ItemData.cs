using UnityEngine;

[CreateAssetMenu(menuName = "Items/Potion")]
public class ItemData : ScriptableObject
{
    public string ItemName;
    public Sprite Icon;
    public int itemID;

    public void Use(GameObject user)
    {
        var handler = user.GetComponent<EffectHandler>();
        if (handler != null)
        {
            handler.ApplyEffect(itemID);
        }
    }
}