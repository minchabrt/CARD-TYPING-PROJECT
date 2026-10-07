using UnityEngine;
using System.Collections;
using TMPro;

public class WaveTimer : MonoBehaviour
{

    public float timer = 5f;
    public bool waveActive = false;

    public TMP_Text timerText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timerText.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.T) && !waveActive){
            
            StartCoroutine(StartWave());
            
        }
    }

    IEnumerator StartWave(){

        waveActive = true;


        while(timer > 0f){
            timer -= Time.deltaTime;
            timerText.text = timer.ToString("F1");
            yield return null;
        }



        // Funkcija za spawnovanje neprijatelja
        Debug.Log("Proslo je vreme!!!!");

        timerText.text = "";
        waveActive = false;
    }


}
