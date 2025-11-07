using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject Avatar1;
    public GameObject Avatar2;
    public GameObject Avatar3;
    // public GameObject Avatar4;

    public TMPro.TMP_Dropdown BBTDrop;
    public TMPro.TMP_Dropdown BBTDropAll;

    public GameObject level1Button;
    public GameObject button11;
    public GameObject button12;
    public GameObject level2Button;
    public GameObject button21;
    public GameObject button22;
    public GameObject level3Button;
    public GameObject button31;
    public GameObject button32;
    public GameObject level4Button;
    public GameObject button41;
    public GameObject button42;
    public GameObject level5Button;
    public GameObject button51;
    public GameObject button52;

    public void Update()
    {
        // ATTIVO L'AVATAR GIUSTO A SECONDA DEL LIVELLO A CUI SONO ARRIVATA (SE HO FATTO ALMENO UN PUNTO)
        if (PlayerPrefs.GetInt("number_avatar"+SaveID.saveID)!=1)
        {
            /*
            if (PlayerPrefs.GetInt("number_avatar" + SaveID.saveID) == 4)
            {
                Avatar1.SetActive(false);
                Avatar2.SetActive(false);
                Avatar3.SetActive(false);
                Avatar4.SetActive(true);
            }
            */
            if (PlayerPrefs.GetInt("number_avatar" + SaveID.saveID) ==3)
                {
                Avatar1.SetActive(false);
                Avatar2.SetActive(false);
                Avatar3.SetActive(true);
            }
            else if (PlayerPrefs.GetInt("number_avatar" + SaveID.saveID) ==2)
                {
                Avatar1.SetActive(false);
                Avatar2.SetActive(true);
            }
        }
    }

    public void Quit()
    {
        PlayerPrefs.SetInt("SlotSaved" + SaveID.saveID, 1);
        PlayerPrefs.SetInt("LoadSaved" + SaveID.saveID, 1);
        SceneManager.LoadScene("selectPlayer");         
    }

    // MI CONSIGLIA IL LIVELLO IN BASE AL PUNTEGGIO DELLA SCALA FM
    public void BBTSelector()
    {
        if (BBTDrop.value == 0)
        {
            level1Button.GetComponent<Image>().color = Color.white;
            button11.SetActive(true);
            button12.SetActive(true);
            level2Button.GetComponent<Image>().color = Color.gray;
            button21.SetActive(false);
            button22.SetActive(false);
            level3Button.GetComponent<Image>().color = Color.gray;
            button31.SetActive(false);
            button32.SetActive(false);
            level4Button.GetComponent<Image>().color = Color.gray;
            button41.SetActive(false);
            button42.SetActive(false);
            level5Button.GetComponent<Image>().color = Color.gray;
            button51.SetActive(false);
            button52.SetActive(false);
        }
        else if (BBTDrop.value == 1)
        {
            level1Button.GetComponent<Image>().color = Color.gray;
            button11.SetActive(false);
            button12.SetActive(false);
            level2Button.GetComponent<Image>().color = Color.white;
            button21.SetActive(true);
            button22.SetActive(true);
            level3Button.GetComponent<Image>().color = Color.gray;
            button31.SetActive(false);
            button32.SetActive(false);
            level4Button.GetComponent<Image>().color = Color.gray;
            button41.SetActive(false);
            button42.SetActive(false);
            level5Button.GetComponent<Image>().color = Color.gray;
            button51.SetActive(false);
            button52.SetActive(false);
        }
        else if (BBTDrop.value == 2)
        {
            level1Button.GetComponent<Image>().color = Color.gray;
            button11.SetActive(false);
            button12.SetActive(false);
            level2Button.GetComponent<Image>().color = Color.gray;
            button21.SetActive(false);
            button22.SetActive(false);
            level3Button.GetComponent<Image>().color = Color.white;
            button31.SetActive(true);
            button32.SetActive(true);
            level4Button.GetComponent<Image>().color = Color.gray;
            button41.SetActive(false);
            button42.SetActive(false);
            level5Button.GetComponent<Image>().color = Color.gray;
            button51.SetActive(false);
            button52.SetActive(false);
        }
        else if (BBTDrop.value == 3)
        {
            level1Button.GetComponent<Image>().color = Color.gray;
            button11.SetActive(false);
            button12.SetActive(false);
            level2Button.GetComponent<Image>().color = Color.gray;
            button21.SetActive(false);
            button22.SetActive(false);
            level3Button.GetComponent<Image>().color = Color.gray;
            button31.SetActive(false);
            button32.SetActive(false);
            level4Button.GetComponent<Image>().color = Color.white;
            button41.SetActive(true);
            button42.SetActive(true);
            level5Button.GetComponent<Image>().color = Color.gray;
            button51.SetActive(false);
            button52.SetActive(false);
        }
        else if (BBTDrop.value == 4)
        {
            level1Button.GetComponent<Image>().color = Color.gray;
            button11.SetActive(false);
            button12.SetActive(false);
            level2Button.GetComponent<Image>().color = Color.gray;
            button21.SetActive(false);
            button22.SetActive(false);
            level3Button.GetComponent<Image>().color = Color.gray;
            button31.SetActive(false);
            button32.SetActive(false);
            level4Button.GetComponent<Image>().color = Color.gray;
            button41.SetActive(false);
            button42.SetActive(false);
            level5Button.GetComponent<Image>().color = Color.white;
            button51.SetActive(true);
            button52.SetActive(true);
        }

    }

    //Per entrambe le articolazioni libere
    /*public void BBTSelectorAll()
    {
        if (BBTDropAll.value == 1|| BBTDropAll.value == 2)
        {
            level1Button.GetComponent<Image>().color = Color.white;
            button11.SetActive(true);
            button12.SetActive(true);
            level2Button.GetComponent<Image>().color = Color.gray;
            button21.SetActive(false);
            button22.SetActive(false);
            level3Button.GetComponent<Image>().color = Color.gray;
            button31.SetActive(false);
            button32.SetActive(false);
        }
        else if (BBTDropAll.value == 3||BBTDropAll.value == 4)
        {
            level1Button.GetComponent<Image>().color = Color.gray;
            button11.SetActive(false);
            button12.SetActive(false);
            level2Button.GetComponent<Image>().color = Color.white;
            button21.SetActive(true);
            button22.SetActive(true);
            level3Button.GetComponent<Image>().color = Color.gray;
            button31.SetActive(false);
            button32.SetActive(false);
        }
        else if (BBTDropAll.value == 5 || BBTDropAll.value == 6 || BBTDropAll.value == 7|| BBTDropAll.value == 8)
        {
            level1Button.GetComponent<Image>().color = Color.gray;
            button11.SetActive(false);
            button12.SetActive(false);
            level2Button.GetComponent<Image>().color = Color.gray;
            button21.SetActive(false);
            button22.SetActive(false);
            level3Button.GetComponent<Image>().color = Color.white;
            button31.SetActive(true);
            button32.SetActive(true);
        }

    }*/
}
