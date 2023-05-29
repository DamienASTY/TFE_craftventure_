using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenusController : MonoBehaviour
{
   public void ChangeScene(string _sceneName)
   {
      SceneManager.LoadScene(_sceneName);
   }

   public void Quit()
   {
      Application.Quit();
   }

   public void Update()
   {
      if (Input.GetKeyDown(KeyCode.Escape))
         SceneManager.LoadScene("cubes");
   }
}
