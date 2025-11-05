using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class SelectPlayer : MonoBehaviour
{
    public GameObject[] newgameButtons = new GameObject[6];
    public GameObject[] loadgameButtons = new GameObject[6];
    public int[] saveIDs = new int[6]; // vettore dei giocatori

    // ogni avatar ha tre immagini disponibili, corrispondenti ai tre livelli 
    public GameObject[] Avatar1 = new GameObject[6];
    public GameObject[] Avatar2 = new GameObject[6];
    public GameObject[] Avatar3 = new GameObject[6];
    // public GameObject[] Avatar4 = new GameObject[3];

    public string path;

    public void Start()
    {
        path = Application.dataPath;
    }

    public void Update()
    {
        //save slot 1: giocatore 1
        if (PlayerPrefs.GetInt("SlotSaved" + saveIDs[0]) == 1) // 1: nello slot è già presente il giocatore  
        { loadgameButtons[0].SetActive(true); // pulsante numero giocatore attivo
            newgameButtons[0].SetActive(false); // pulsante new game disattivato
        }
        else // 0: giocatore non ancora inserito
        { loadgameButtons[0].SetActive(false);
            newgameButtons[0].SetActive(true);
        }

        // imposta l'avatar in base al livello del giocatore:
        if (PlayerPrefs.GetInt("number_avatar" + saveIDs[0]) != 1) // avatar del livello 1 impostato di default
        {
            /* NO
            if (PlayerPrefs.GetInt("number_avatar" + saveIDs[0]) == 4) // giocatore al livello 4
            {
                Avatar1[0].SetActive(false);
                Avatar2[0].SetActive(false);
                Avatar3[0].SetActive(false);
                Avatar4[0].SetActive(true);
            }
            */
            if (PlayerPrefs.GetInt("number_avatar" + saveIDs[0]) == 5) // giocatore al livello 5
            {
                Avatar1[0].SetActive(false);
                Avatar2[0].SetActive(false);
                Avatar3[0].SetActive(true);
            }
            else if (PlayerPrefs.GetInt("number_avatar" + saveIDs[0]) == 3) // giocatore al livello 3
            {
                Avatar1[0].SetActive(false);
                Avatar2[0].SetActive(true);
            }
        }

        //save slot 2: giocatore 2
        if (PlayerPrefs.GetInt("SlotSaved" + saveIDs[1]) == 1)
        { loadgameButtons[1].SetActive(true);
            newgameButtons[1].SetActive(false);
        }
        else
        { loadgameButtons[1].SetActive(false);
            newgameButtons[1].SetActive(true);
        }

        if (PlayerPrefs.GetInt("number_avatar" + saveIDs[1]) != 1)
        {
            /*
            if (PlayerPrefs.GetInt("number_avatar" + saveIDs[1]) == 4)
            {
                Avatar1[1].SetActive(false);
                Avatar2[1].SetActive(false);
                Avatar3[1].SetActive(false);
                Avatar4[1].SetActive(true);
            }
            */
            if (PlayerPrefs.GetInt("number_avatar" + saveIDs[1]) == 5)
            {
                Avatar1[1].SetActive(false);
                Avatar2[1].SetActive(false);
                Avatar3[1].SetActive(true);
            }
            else if (PlayerPrefs.GetInt("number_avatar" + saveIDs[1]) == 3)
            {
                Avatar1[1].SetActive(false);
                Avatar2[1].SetActive(true);
            }
        }

            //save slot 3: giocatore 3
            if (PlayerPrefs.GetInt("SlotSaved" + saveIDs[2]) == 1)
            { loadgameButtons[2].SetActive(true);
                newgameButtons[2].SetActive(false);
            }
            else
            { loadgameButtons[2].SetActive(false);
                newgameButtons[2].SetActive(true);
            }

            if (PlayerPrefs.GetInt("number_avatar" + saveIDs[2]) != 1)
            {
            /*
                if (PlayerPrefs.GetInt("number_avatar" + saveIDs[2]) == 4)
                {
                    Avatar1[2].SetActive(false);
                    Avatar2[2].SetActive(false);
                    Avatar3[2].SetActive(false);
                    Avatar4[2].SetActive(true);
                }
            */
                if (PlayerPrefs.GetInt("number_avatar" + saveIDs[2]) == 5)
                {
                    Avatar1[2].SetActive(false);
                    Avatar2[2].SetActive(false);
                    Avatar3[2].SetActive(true);
                }
                else if (PlayerPrefs.GetInt("number_avatar" + saveIDs[2]) == 3)
                {
                    Avatar1[2].SetActive(false);
                    Avatar2[2].SetActive(true);
                }
            }
        //save slot 4: giocatore 4
        if (PlayerPrefs.GetInt("SlotSaved" + saveIDs[3]) == 1)
        {
            loadgameButtons[3].SetActive(true);
            newgameButtons[3].SetActive(false);
        }
        else
        {
            loadgameButtons[3].SetActive(false);
            newgameButtons[3].SetActive(true);
        }

        if (PlayerPrefs.GetInt("number_avatar" + saveIDs[3]) != 1)
        {
            
            if (PlayerPrefs.GetInt("number_avatar" + saveIDs[3]) == 5)
            {
                Avatar1[3].SetActive(false);
                Avatar2[3].SetActive(false);
                Avatar3[3].SetActive(true);
            }
            else if (PlayerPrefs.GetInt("number_avatar" + saveIDs[3]) == 3)
            {
                Avatar1[3].SetActive(false);
                Avatar2[3].SetActive(true);
            }
        }

        //save slot 5: giocatore 5
        if (PlayerPrefs.GetInt("SlotSaved" + saveIDs[4]) == 1)
        {
            loadgameButtons[4].SetActive(true);
            newgameButtons[4].SetActive(false);
        }
        else
        {
            loadgameButtons[4].SetActive(false);
            newgameButtons[4].SetActive(true);
        }

        if (PlayerPrefs.GetInt("number_avatar" + saveIDs[4]) != 1)
        {

            if (PlayerPrefs.GetInt("number_avatar" + saveIDs[4]) == 5)
            {
                Avatar1[4].SetActive(false);
                Avatar2[4].SetActive(false);
                Avatar3[4].SetActive(true);
            }
            else if (PlayerPrefs.GetInt("number_avatar" + saveIDs[4]) == 3)
            {
                Avatar1[4].SetActive(false);
                Avatar2[4].SetActive(true);
            }
        }
        //save slot 6: giocatore 6
        if (PlayerPrefs.GetInt("SlotSaved" + saveIDs[5]) == 1)
        {
            loadgameButtons[5].SetActive(true);
            newgameButtons[5].SetActive(false);
        }
        else
        {
            loadgameButtons[5].SetActive(false);
            newgameButtons[5].SetActive(true);
        }

        if (PlayerPrefs.GetInt("number_avatar" + saveIDs[5]) != 1)
        {

            if (PlayerPrefs.GetInt("number_avatar" + saveIDs[5]) == 5)
            {
                Avatar1[5].SetActive(false);
                Avatar2[5].SetActive(false);
                Avatar3[5].SetActive(true);
            }
            else if (PlayerPrefs.GetInt("number_avatar" + saveIDs[5]) == 3)
            {
                Avatar1[5].SetActive(false);
                Avatar2[5].SetActive(true);
            }
        }
    }



            public void NewGame()
            {
                SceneManager.LoadScene("Calibration");
            }

            public void LoadGame()
            {
                if (PlayerPrefs.GetInt("LoadSaved" + SaveID.saveID) == 1)
                {
                    SceneManager.LoadScene("Calibration");
                }
                else
                {
                    return;
                }
            }

            public void SetSaveID(int saveId)
            {
                SaveID.saveID = saveId;
            }

    // se schiaccio Reset cancello tutti i dati del giocatore
            public void ClearSave(int saveId)
            {
                PlayerPrefs.DeleteKey("SlotSaved" + saveId);
                PlayerPrefs.SetInt("number_avatar" + saveId, 1);

                Avatar1[saveId-1].SetActive(true);
                Avatar2[saveId - 1].SetActive(false);
                Avatar3[saveId - 1].SetActive(false);
                // Avatar4[saveId - 1].SetActive(false);

                 for (int i = 1; i <= 3; i++)   //resetto il punteggio massimo per ogni livello
                {
                    PlayerPrefs.SetInt("HighScore" + saveId + i, 0);
                }
                PlayerPrefs.SetInt("HighScore" + saveId + 12, 0);
                PlayerPrefs.SetInt("HighScore" + saveId + 22, 0);
                PlayerPrefs.SetInt("HighScore" + saveId + 32, 0);

                PlayerPrefs.SetFloat("Min Angle" + saveId, 0f);
                PlayerPrefs.SetFloat("Max Angle" + saveId, 0f);

             }

    // cancello i file con i dati salvati se schiaccio reset
    public void ResetFile(int saveid) 
    {

        for (int i = 0; i <= 3; i++)
        {
            if (File.Exists(path + "datiAcc" + saveid.ToString() + "_" + i.ToString() + ".txt"))
            {
                File.Delete(path + "datiAcc" + saveid.ToString() + "_" + i.ToString() + ".txt");
                UnityEditor.AssetDatabase.Refresh();
            }

            if (File.Exists(path + "datiAng" + saveid.ToString() + "_" + i.ToString() + ".txt"))
            {
                File.Delete(path + "datiAng" + saveid.ToString() + "_" + i.ToString() + ".txt");
                UnityEditor.AssetDatabase.Refresh();
            }
            if (File.Exists(path + "datiAcc" + saveid.ToString() + "_" + i.ToString() + "2" + ".txt"))
            {
                File.Delete(path + "datiAcc" + saveid.ToString() + "_" + i.ToString() + "2" + ".txt");
                UnityEditor.AssetDatabase.Refresh();
            }

            if (File.Exists(path + "datiAng" + saveid.ToString() + "_" + i.ToString() + "2" + ".txt"))
            {
                File.Delete(path + "datiAng" + saveid.ToString() + "_" + i.ToString() + "2" + ".txt");
                UnityEditor.AssetDatabase.Refresh();
            }

        }
    }
}