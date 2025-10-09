using TMPro;
using UnityEngine;
using UnityEngine.UI;


public enum Difficult
{
    easy, normal, hard
}

public enum GameMode
{
    Survival, Creative
}

public class NewWorldCofiguration : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputWorldName;
    [SerializeField] private TMP_InputField inputWorldSeed;
    [SerializeField] private GameMode gamemode = GameMode.Survival;
    [SerializeField] private Difficult Difficulty = Difficult.easy;

    private void OnEnable()
    {
        GameManager.OnCreateNewWorld += CreateNewWorld;
    }

    private void OnDisable()
    {
        GameManager.OnCreateNewWorld -= CreateNewWorld;
    }
    private void CreateNewWorld()
    {
        if(inputWorldName.text == string.Empty)
        {
            GameManager.instance.CreateNewWorld(inputWorldSeed.text,"New World" ,gamemode, Difficulty);
        }
        else
        {
            GameManager.instance.CreateNewWorld(inputWorldSeed.text, inputWorldName.text, gamemode, Difficulty);
        }
        
    }



}
