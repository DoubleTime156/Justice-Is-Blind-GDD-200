using TMPro;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public GameObject keyImage;
    public GameObject bottleImage;
    public GameObject coinImage;
    public TextMeshProUGUI bottleText;
    public TextMeshProUGUI coinText;
    public PlayerData data;

    public void updateAmount() // Update inventory UI
    {
        bottleText.text = data.inventory[1].ToString();
        coinText.text = data.inventory[0].ToString();
        if (data.hasKey) { keyImage.SetActive(true); }
        else { keyImage.SetActive(false); }
        if (data.heldItem == 0)
        {
            coinImage.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
            bottleImage.gameObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }
        else if (data.heldItem == 1)
        {
            coinImage.gameObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            bottleImage.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
        }
    }
}
