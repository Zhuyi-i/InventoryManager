using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] public GameObject OptionmenuUI;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale == 1)
            {
                Time.timeScale = 0; // Pause the game
            }
            else
            {
                Time.timeScale = 1; // Resume the game
            }
        }
    }
    public void resumeGame()
    {
        Time.timeScale = 1; // Resume the game
        OptionmenuUI.SetActive(!OptionmenuUI.activeSelf);
    
    }
}
