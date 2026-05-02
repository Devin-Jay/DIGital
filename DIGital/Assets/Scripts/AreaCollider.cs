using UnityEngine;

public class AreaCollider : MonoBehaviour
{
    public string areaName;
    public string areaSceneName;
    [SerializeField]
    private SpriteRenderer spriteRenderer;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            spriteRenderer.enabled = true;
            other.GetComponent<DroneController>().ShowInteractPrompt(areaName, areaSceneName);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            spriteRenderer.enabled = false;
            other.GetComponent<DroneController>().HideInteractPrompt();
        }
    }
}
