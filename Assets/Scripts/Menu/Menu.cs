using UnityEngine;

public class Menu : MonoBehaviour
{
    [SerializeField] public GameObject OptionmenuUI;
    [SerializeField] public PlayerController PlayerControllerScript;
    [SerializeField] public Attack AttackScript;
    [SerializeField] public InventoryUI InventoryScript;
    void Start()
    {

    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Escape))
        {
           DisableUi();
            if (OptionmenuUI.activeSelf)
                {
                    PauseGame();
                }
                else
                {
                    ResumeGame();
                }
        }
        if (InventoryScript.IsOpen)
        {
            AttackScript.enabled = false;
        }
        else if (!InventoryScript.IsOpen && !OptionmenuUI.activeSelf)
        {
            AttackScript.enabled = true;
        }

    }
    void PauseGame()
    {
        Time.timeScale = 0f;
        AttackScript.enabled = false;

        PlayerControllerScript.enabled = false; 
    }
    public void ResumeGame()
    {
        Time.timeScale = 1f; 

        PlayerControllerScript.enabled = true;
        AttackScript.enabled = true; 
    }
   public void DisableUi()
    {
        OptionmenuUI.SetActive(!OptionmenuUI.activeSelf);

    }


}
