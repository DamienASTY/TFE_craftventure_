using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;

public class PlayMenuController : MonoBehaviour
{
    public InputField seedField;

    public void GetInputField()
    {
        string inputValue = seedField.text;
        Debug.Log("Contenu de la box : " + inputValue);
    }

    public void ChangeScene(string _scene)
    {
        SceneManager.LoadScene(_scene);
    }
    
    //Génère une seed random de 16 char
    public void RandomSeed()
    {
        const ushort max = 5;
        string seed = "";
        for (int i = 0; i < max; i++)
        {
            int randomNumber = Random.Range(1, 9);
            seed += $"{randomNumber}";
        }
        Debug.Log(seed);
        int seedInt = int.Parse(seed);
    }
}
