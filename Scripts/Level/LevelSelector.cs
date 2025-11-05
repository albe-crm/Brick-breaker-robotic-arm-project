using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelector : MonoBehaviour
{
    public int level;

    public void OpenScene() // nel menu quando schiaccio il tasto del livello, mi carica la scena giusta
    {
        SceneManager.LoadScene("Level " + level.ToString());
    }

}
