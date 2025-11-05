using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System;

public class PaddleScript : MonoBehaviour
{
    public float speedPaddle;
    public float upScreenEdge = 1.75f;
    public float downScreenEdge = -1.75f;
    public GameManager gm;
    public int collision_number=0;   //..........................................

    public TMPro.TMP_Dropdown paddleDimDrop;

    public float alpha,beta,gamma;
    public float anglemin, betamin;
    public float anglemax,betamax;
    public float rapporto, rapporto2;
    public BallScript Ball;

    Vector3 oldscale;
    float oldupScreenEdge;
    float olddownScreenEdge;

    SerialPort sp;

    void Start()
    {
        anglemin = PlayerPrefs.GetFloat("Min Angle" + SaveID.saveID) * (80f / 100f);
        anglemax = PlayerPrefs.GetFloat("Max Angle" + SaveID.saveID);

        // collaborative rob - motor input from ARDUINO
        sp = new SerialPort("COM12", 9600);
        sp.ReadTimeout = 10;
        sp.Open();

    }

   
    void Update()
    {
        if (!gm.inGame)
            return;
        // PER GIOCARE CON I TASTI 
        // da qui
        float vertical = Input.GetAxis("Vertical");
        transform.Translate(Vector2.right * vertical * Time.deltaTime * speedPaddle);

        if (transform.position.y < downScreenEdge)      //limite inferiore in cui puo andare il paddle
        {
            transform.position = new Vector2(transform.position.x, downScreenEdge);
        }

        if (transform.position.y > upScreenEdge)        //limite superiore
        {
            transform.position = new Vector2(transform.position.x, upScreenEdge);
        }
        // a qui

        //seno = Mathf.Sin(alpha - anglemin) / Mathf.Sin(anglemax - anglemin);
        //float posY = downScreenEdge + seno * upScreenEdge*2;

        // il paddle si alza proporzionalmente a quanto alzo il braccio 
        if (alpha<=anglemin)
        {
            alpha = anglemin;
        }else if (alpha>=anglemax)
        {
            alpha = anglemax;
        }
        rapporto = (alpha - anglemin) / (anglemax - anglemin);
        //float posY = downScreenEdge + (1f - rapporto) * upScreenEdge * 2;
        //transform.position = new Vector3(7.6f, posY, 0f);


        // collaborative rob - motor input from ARDUINO
        if (sp.IsOpen)
        {
            try
            {
                // Leggi il valore inviato da Arduino (angolo in gradi)
                int value = sp.ReadByte();
                alpha = (float)value;       // assegna direttamente l'angolo

                // --- Limita l'angolo ai valori di riferimento ---
                if (alpha <= anglemin)
                {
                    alpha = anglemin;
                }
                else if (alpha >= anglemax)
                {
                    alpha = anglemax;
                }

                // --- Calcolo posizione paddle ---
                rapporto = (alpha - anglemin) / (anglemax - anglemin);
                float posY = downScreenEdge + (1f - rapporto) * upScreenEdge * 2;

                transform.position = new Vector3(7.6f, posY, 0f);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Errore nella lettura seriale: " + e.Message);
            }
        }



    //    if (sp.IsOpen)
    //    {
    //        try
    //        {
    //            int value = sp.ReadByte();
    //            float positionUnity = (10 - ((float)value / 10));
    //            transform.position = new Vector3(positionUnity, transform.position.y, transform.position.z);
    //        }
    //        catch (System.Exception e)
    //        {

    //        }
    //    }

    }

    public void ResetPaddle()
    {
        transform.position = new Vector2(transform.position.x, downScreenEdge);
        collision_number = 0;  
    }

    public void PaddleDimSelector() // regolo la dimensione del paddle
    {
        if (paddleDimDrop.value == 0)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
            upScreenEdge = 4f;
            downScreenEdge = -4f;
        }
        else if (paddleDimDrop.value == 1)
        {
            transform.localScale = new Vector3(0.7f, 1f, 1f);
            upScreenEdge = 3.5f;
            downScreenEdge = -3.5f;
        }
        else if (paddleDimDrop.value == 2)
        {
            transform.localScale = new Vector3(1.3f, 1f, 1f);
            upScreenEdge = 4.5f;
            downScreenEdge = -4.5f;
        }
    }

    public void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Ball")
        {
            collision_number += 1;
            gm.audioSource[0].Play();
            Ball.cont = 0;
        }
    }
    public void AumentaPaddle()
    {
        oldscale = new Vector3(transform.localScale.x, 1f, 1f);
        oldupScreenEdge = upScreenEdge;
        olddownScreenEdge = downScreenEdge;

        transform.localScale = new Vector3(oldscale.x + 0.3f, 1f, 1f);
        upScreenEdge = oldupScreenEdge - 0.75f;
        downScreenEdge = olddownScreenEdge + 0.75f;

        StartCoroutine(RiduciPaddle());
    }

    IEnumerator RiduciPaddle()
    {
        yield return new WaitForSeconds(10f);

        transform.localScale = new Vector3(oldscale.x, 1f, 1f);
        upScreenEdge = oldupScreenEdge;
        downScreenEdge = olddownScreenEdge;
    }

}
