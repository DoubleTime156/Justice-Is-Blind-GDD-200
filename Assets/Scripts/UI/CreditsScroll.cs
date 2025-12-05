using UnityEngine;

public class CreditsScroll : MonoBehaviour
{

    public float scrollSpeed = 40f;
    public GameManager gameManager;
    private RectTransform rectTransform;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        rectTransform.anchoredPosition += new Vector2(0, scrollSpeed*Time.deltaTime);
        //Debug.Log(rectTransform.position.y);
        if(rectTransform.position.y> 2600) { gameManager.mainMenu(); }
    }
}
