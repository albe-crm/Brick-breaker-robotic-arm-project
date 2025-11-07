using System.Collections;
using NgimuApi.SearchForConnections;

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public int lives;
    public int score;
    public int LevelNumber;

    public Text scoreText;
    public Text HighscoreText;
    public Text yourHighscoreText;
    public Slider sliderHighscore;

    public bool inGame;
    public GameObject level1Panel;
    public GameObject GameOverPanel;
    public GameObject WinnerPanel;
    public GameObject PauseMenu;

    public int numberOfBricks;
    public int number_unbreakable = 0;
    private int Total_numberBricks;
    private int totalPoints;

    public BrickScript brick;
    public BallScript ball;
    public PaddleScript paddle;
    public BrickScript[] bricks;

    // vite a forma di cuore:
    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;

    public feedback fb;
    public int number_collision=0;
    public int vero_numero_collisioni;
    private int flag;

    public Text countDownText;

    public AudioSource[] audioSource;

    public GameObject MultiBall;
    public GameObject Life;
    public GameObject BiggerPaddle;
    public GameObject PausePower;
    public GameObject Balltwo;
    private float InstantiationTimer = 7f;

    //public NGIMUConnection ;


    void Start()
    {
        brick = FindObjectOfType<BrickScript>();
        ball = FindObjectOfType<BallScript>();
        paddle = FindObjectOfType<PaddleScript>();
        bricks = FindObjectsOfType<BrickScript>();

        scoreText.text =score.ToString();
        HighscoreText.text = "Best Score: " + PlayerPrefs.GetInt("HighScore"+SaveID.saveID+ LevelNumber).ToString();

        numberOfBricks = GameObject.FindGameObjectsWithTag("Brick").Length;
        
        for (int i=0;i<numberOfBricks;i++)      
        {
            if (bricks[i].unbreakable==true) //conto il numero di blocchi unbreakable
            {
                number_unbreakable += 1;
            }
            else // conto le vite dei mattoncini non unbrakable
            {
                totalPoints += bricks[i].states.Length;
            }
        }
        numberOfBricks = numberOfBricks - number_unbreakable;
        Total_numberBricks = numberOfBricks;
        totalPoints = totalPoints * brick.points;

        HighscorePanel();

        UpdateScore(0);

        audioSource = GetComponents<AudioSource>();
        Balltwo.SetActive(false);
    }

    
    void Update()
    {
        vero_numero_collisioni = paddle.collision_number;
        if (Input.GetKey(KeyCode.Space))       
        {
            Pause();
            Time.timeScale = 0f;
            
        }

        // GESTISCO LE VITE
        foreach (Image img in hearts)    //imposto tutti i cuori vuoti
        {
            img.sprite = emptyHeart;
        }

        for (int i=0; i<lives; i++)     //metto i cuori pieni in base alle vite che ho
        {
            hearts[i].sprite = fullHeart;
        }

        SuperPowers();

        //prendiamo l'angolo beta , aggiungi scritta a video quando b sopra soglia

        if (paddle.beta >= 10f /*|| paddle.beta <= 10f*/)
        {
            fb.Compensation();
        }

    }



    public void UpdateLives(int changeInLives)
    {
        lives += changeInLives;
        number_collision = -1;

        if (lives <= 0)
        {
            lives = 0;
            GameOver();
        }

        if (lives==1 && fb != null)
        {
            fb.OneLife();
        }

    }

    public void UpdateScore(int points)
    {
        score += points;
        scoreText.text = score.ToString();
        if (score > PlayerPrefs.GetInt("HighScore"+SaveID.saveID+LevelNumber)) // se ottengo un punteggio maggiore aggiorno il bestscore
        {
            PlayerPrefs.SetInt("HighScore" + SaveID.saveID + LevelNumber, score);
            HighscoreText.text = "Best Score: "+score.ToString();
        }

        if (PlayerPrefs.GetInt(("HighScore") + SaveID.saveID + LevelNumber) != 0) // se il punteggio è diverso da zero, aggiorno l'avatar
        {
            if (PlayerPrefs.GetInt(("HighScore") + SaveID.saveID + 4) != 0)
            {
                PlayerPrefs.SetInt(("number_avatar" +SaveID.saveID), 4);
            }
            else if ((PlayerPrefs.GetInt(("HighScore") + SaveID.saveID + 3) != 0) || (PlayerPrefs.GetInt(("HighScore") + SaveID.saveID + 32) != 0))
            {
                PlayerPrefs.SetInt(("number_avatar"+ SaveID.saveID),3);
            }
            else if ((PlayerPrefs.GetInt(("HighScore") + SaveID.saveID + 2 )!= 0)|| (PlayerPrefs.GetInt(("HighScore") + SaveID.saveID + 22) != 0))
            {
                PlayerPrefs.SetInt(("number_avatar" +SaveID.saveID), 2);
            }
            else
            {
                PlayerPrefs.SetInt(("number_avatar" + SaveID.saveID), 1);
            }
        }


    }

    public void UpdateNumberOfBricks()
    {
        numberOfBricks--;
        if (numberOfBricks==Total_numberBricks/2)
        {
            fb.HalfLevel();
        }

        if (numberOfBricks == 1)
        {
            fb.LastOne();
        }

        if (numberOfBricks <= 0)
        {
            Winner();
        }
    }

    public void NumberCollision()
    {
        if (paddle.collision_number == number_collision && flag == 0)     //se il numero delle collisioni è uguale a quelle del blocco prima
        {                                                               //ed è la prima volta che succede, non voglio che avvenga anche con i tris
            fb.Combo();
            flag = 1;
        }
        else
        {
            flag = 0;
        }
        number_collision = paddle.collision_number;
        
    }

   
   void GameOver()
        {
            inGame = false;
            GameOverPanel.SetActive(true);
        if (Balltwo.activeSelf == true)
        {
            Balltwo.SetActive(false);
        }
    }


    void Winner()
        {
            inGame = false;
            WinnerPanel.SetActive(true);
        if (Balltwo.activeSelf == true)
        {
            Balltwo.SetActive(false);
        }
    }
    
    public void Pause()
    {
        Time.timeScale = 0f;
        inGame = false;
        PauseMenu.SetActive(true);                   
    }

    public void resume()
    {
        PauseMenu.SetActive(false);
        inGame = true;
        Time.timeScale = 1f;
    }

    public void Play()
    {
        Time.timeScale = 1f;               

        PauseMenu.SetActive(false);                     

        StartCoroutine(WaitCountDownPlay());
    }

    public void PlayAgain()
    {
        GameOverPanel.SetActive(false);
        WinnerPanel.SetActive(false);
        level1Panel.SetActive(true);

        HighscorePanel();
    }

    public void Back()
    {
        SceneManager.LoadScene("Menu");
    }

    public void Next1()
    {
        PauseMenu.SetActive(false);
        SceneManager.LoadScene("Level 12");

    }

    public void Next2()
    {
        PauseMenu.SetActive(false);
        SceneManager.LoadScene("Level 2");

    }

    public void Next3()
    {
        PauseMenu.SetActive(false);
        SceneManager.LoadScene("Level 22");

    }

    public void Next4()
    {
        PauseMenu.SetActive(false);
        SceneManager.LoadScene("Level 3");

    }

    public void Next5()
    {
        PauseMenu.SetActive(false);
        SceneManager.LoadScene("Level 32");

    }

        public void Next6()
    {
        PauseMenu.SetActive(false);
        SceneManager.LoadScene("Level 4");

    }

    public void Next7()
    {
        PauseMenu.SetActive(false);
        SceneManager.LoadScene("Level 42");

    }

    public void Next8()
    {
        PauseMenu.SetActive(false);
        SceneManager.LoadScene("Level 5");

    }

    public void Next9()
    {
        //Disconnect();
        PauseMenu.SetActive(false);
        SceneManager.LoadScene("Level 52");
    }

    public void Back1()
    {
        PauseMenu.SetActive(false);
        SceneManager.LoadScene("Level 1");
    }

    public void Calibrazione()
    {
        PauseMenu.SetActive(false);
        SceneManager.LoadScene("Calibration");
    }
    

    public void ResetGame()
    {
        ball.ResetBall();
        paddle.ResetPaddle();
        Destroy(GameObject.Find("Life(Clone)"));
        Destroy(GameObject.Find("Multiball(Clone)"));
        Destroy(GameObject.Find("Pause(Clone)"));
        Balltwo.SetActive(false);

        for (int i = 0; i < bricks.Length; i++)
        {
            bricks[i].ResetBricks();
        }
        numberOfBricks = GameObject.FindGameObjectsWithTag("Brick").Length-number_unbreakable;

        score = 0;
        lives = 3;
        
        scoreText.text = score.ToString();
        number_collision = -1;   

    }


    public void ResetHighscore()  
    {
        PlayerPrefs.SetInt("HighScore" + SaveID.saveID + LevelNumber, 0);
        HighscoreText.text = "Best Score: " + PlayerPrefs.GetInt("HighScore" + SaveID.saveID + LevelNumber).ToString();

        if (PlayerPrefs.GetInt("number_avatar" + SaveID.saveID) != 1)
        {
            PlayerPrefs.SetInt("number_avatar" + SaveID.saveID, 2); //sono al livello 3 quindi lo riporto al 2
        }
        HighscorePanel();
    }

    public void HighscorePanel()
    {
        yourHighscoreText.text = "Your highscore: " + PlayerPrefs.GetInt("HighScore" + SaveID.saveID + LevelNumber).ToString() + "/" + totalPoints;

        sliderHighscore.value = PlayerPrefs.GetInt("HighScore" + SaveID.saveID + LevelNumber);
        sliderHighscore.maxValue = totalPoints;
    }


    //Per quittare: Application.Quit(); //Doesnt work in the editor

    IEnumerator WaitCountDownPlay()
    {
        WinnerPanel.SetActive(false);
        GameOverPanel.SetActive(false);
        level1Panel.SetActive(false);

        ResetGame();
        ball.gameObject.SetActive(false);
        countDownText.text = "3";
        yield return new WaitForSeconds(1f);
        countDownText.text = "2";
        yield return new WaitForSeconds(1f);
        countDownText.text = "1";
        yield return new WaitForSeconds(1f);
        countDownText.text = "";

        ball.gameObject.SetActive(true);
        inGame = true;
        ball.StartBall();
    }


   public void SuperPowers()
    {
        if (inGame == true && ball.inPlay==true)
        {
           
            InstantiationTimer -= Time.deltaTime;
            if (InstantiationTimer <= 0)
            {
                int s = Random.Range(0, 3);
                if (s == 0 && Balltwo.activeSelf == false)
                {
                    Instantiate(MultiBall, new Vector3(-9, +4, 0), Quaternion.identity);
                }
                if (s == 1)
                {
                    Instantiate(Life, new Vector3(-9, +4, 0), Quaternion.identity);
                }
                //if (s == 2)
                //{
                //    Instantiate(PausePower, new Vector3(-9, +4, 0), Quaternion.identity);
                // }
                if (s == 2)
                {
                    Instantiate(BiggerPaddle, new Vector3(-9, +4, 0), Quaternion.identity);
                }

                InstantiationTimer = 15f;
            }
        }
    }

}

