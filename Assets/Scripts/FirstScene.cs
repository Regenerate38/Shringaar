using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class FirstScene : MonoBehaviour
{
    public void Scene1()
    {
        SceneManager.LoadScene("SampleScene");
    }
    public void Scene2()
    {
        SceneManager.LoadScene("MeasureScene");
    }
/*
    public void Scene3()
    {
        SceneManager.LoadScene("Scene3");
    }
  */
}