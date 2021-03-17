using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class PlayerMenu : MonoBehaviour
{
    public Slider mainSlider;
    public TMP_Text fieldSizeText;
    
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex+1);
    }

    private void Update()
    {
        fieldSizeText.text = mainSlider.value.ToString();
    }
}
