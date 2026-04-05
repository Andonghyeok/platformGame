using UnityEngine;

public class DroppedItem : MonoBehaviour
{
    public string itemKey; 
    public void OnCollect() 
    {
        ObjectPoolManager.Instance.ReleaseObject(itemKey, gameObject);
    }


    private void OnEnable()
    {
        Invoke(nameof(AutoRelease), 5f);
    }

    private void AutoRelease()
    {
        ObjectPoolManager.Instance.ReleaseObject(itemKey, gameObject);
    }

    private void OnDisable()
    {
        CancelInvoke(); 
    }
}