using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Calibration : MonoBehaviour
{


    public GameObject panel1;
    public Text minAngleText1;
    public Text maxAngleText1;
    public GameObject skipButton;
    public GameObject calibrateButton;

    public GameObject panel2;
    public Text minAngleText2;
    public Text maxAngleText2;

    public Text countdownText;
    public Text angleText;
    public Text sliderCountdownText;
    public Slider slider;

    public float minAngle;
    public float maxAngle;
    public float currentAngle, currentAngle2;

    public bool calibrationOn;

    public int player;

    void Start()
    {
        player = SaveID.saveID; // numero giocatore selezionato

        minAngle = PlayerPrefs.GetFloat("Min Angle" + player);
        maxAngle = PlayerPrefs.GetFloat("Max Angle" + player);

        panel2.SetActive(false);
        Panel1();
        calibrationOn = false;
        Time.timeScale = 1;
    }

    IEnumerator Countdown()
    {
        sliderCountdownText.enabled = false;
        angleText.enabled = false;

        yield return new WaitForSeconds(3f);

        countdownText.text = "3";
        yield return new WaitForSeconds(1f);
        countdownText.text = "2";
        yield return new WaitForSeconds(1f);
        countdownText.text = "1";
        yield return new WaitForSeconds(1f);
        countdownText.text = "";

        StartCoroutine(SliderCountdown());
    }

    void bo()
    {
        calibrationOn = false;
        Panel2();
    }

    // attivo lo slider durante il Countdown
    IEnumerator SliderCountdown()
    {
        slider.enabled = true;
        sliderCountdownText.enabled = true;
        angleText.enabled = true;

        calibrationOn = true;

        for (int i = 10; i >= 0; i--)
        {
            sliderCountdownText.text = (i).ToString();
            slider.value = i;
            yield return new WaitForSeconds(1f);
        }

        calibrationOn = false;

        Panel2(); // finiti i 10 secondi di calibrazione apro il pannello 2
    }

    void Update()
    {

        if (calibrationOn == true)
        {


            SaveAngles();
        }


    }

    public void SaveAngles()
    {
        if (minAngle == 0f)
            minAngle = 90f;

        angleText.text = currentAngle.ToString("0.0") + "°";

        if (currentAngle < minAngle)
        {
            minAngle = currentAngle;
        }
        if (currentAngle > maxAngle)
        {
            maxAngle = currentAngle;
        }
    }

    public void Panel1()
    {
        panel1.SetActive(true);

        minAngleText1.text = "Min angle: " + minAngle.ToString("0.00") + "°";
        maxAngleText1.text = "Max angle: " + maxAngle.ToString("0.00") + "°";

        // se non sono ancora stati registrati valori, non si può skippare la calibrazione
        if (minAngle == 0f && maxAngle == 0f)
        {
            skipButton.SetActive(false);
        }

    }

    public void Panel2()
    {
        panel2.SetActive(true);

        minAngleText2.text = "Min angle: " + minAngle.ToString("0.00") + "°";
        maxAngleText2.text = "Max angle: " + maxAngle.ToString("0.00") + "°";
    }

    // Funzioni dei tasti: 
    public void Skip()
    {
        SceneManager.LoadScene("prova");
    }
    
    public void Calibrate()
    {
        panel1.SetActive(false);

        minAngle = 0f;
        maxAngle = 0f;

        StartCoroutine(Countdown());
    }

    public void Repeat()
    {
        panel2.SetActive(false);

        minAngle = 0f;
        maxAngle = 0f;

        StartCoroutine(Countdown());
    }
    
    public void LetsGo()
    {
        // finchè non si schiaccia Let's go, nel player prefs restano salvati gli angoli di prima
        PlayerPrefs.SetFloat("Min Angle" + player, minAngle);
        PlayerPrefs.SetFloat("Max Angle" + player, maxAngle);

        SceneManager.LoadScene("prova");
    }
}
