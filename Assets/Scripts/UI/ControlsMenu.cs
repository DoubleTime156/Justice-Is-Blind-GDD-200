using UnityEngine;

public class ControlsMenu : MonoBehaviour
{
    public GameObject controlsMenu;

    public void showMenu() { controlsMenu.gameObject.SetActive(true); }

    public void hideMenu() { controlsMenu.gameObject.SetActive(false);}
}
