using UnityEngine;
using System.Collections;

public class MoveForward : MonoBehaviour
{
    public VerticalRoulette verticalRoulette;
    public float speed = 1f; // Velocitat de l'objecte
    private float originalSpeed; // Per guardar la velocitat original
    public string accio;

    private Vector3 startPosition; // Per guardar la posició inicial

    void Start()
    {
        // Guarda la posició inicial i la velocitat original
        startPosition = transform.position;
        originalSpeed = speed;

        // Reinicia la posició al començar
        ResetObject();
    }

    void Update()
    {
        // Mou l'objecte cap amunt (eix Y) a la velocitat definida
        transform.Translate(Vector3.up * speed * Time.deltaTime);
    }

    public void ApplyBoost(float boostAmount, float duration)
    {
        // Atura qualsevol corutina prèvia
        StopAllCoroutines();
        // Comença la corutina per aplicar el boost
        StartCoroutine(TemporaryBoost(boostAmount, duration));
    }

    public void ApplyFreeze(float duration)
    {
        // Atura qualsevol corutina prèvia
        StopAllCoroutines();
        // Comença la corutina per aplicar el freeze
        StartCoroutine(TemporaryFreeze(duration));
    }

    IEnumerator TemporaryBoost(float boostAmount, float duration)
    {
        // Augmenta la velocitat
        speed += boostAmount;
        yield return new WaitForSeconds(duration);
        // Torna a la velocitat original
        speed = originalSpeed;
    }

    IEnumerator TemporaryFreeze(float duration)
    {
        // Estableix la velocitat a 0
        speed = 0f;
        yield return new WaitForSeconds(duration);
        // Torna a la velocitat original
        speed = originalSpeed;
    }
    /*
    public void RealitzarAccio()
    {
        GetAccio();
        switch (accio)
        {
            case "Imatge1w":
                ApplyBoost(1, 1);
                GirarRuletaGeneral();
                break;
            case "Imatge2w":
                ApplyBoost(1, 1);
                GirarRuletaGeneral();
                break;
            case "Imatge3w":
                ApplyBoost(1, 1);
                GirarRuletaGeneral();
                break;
        }
    }
    */
    public void GetAccio()
    {
        accio = verticalRoulette.accio;
    }

    public void GirarRuletaGeneral()
    {
        verticalRoulette.IniciarRuleta();
        verticalRoulette.accio = "";
    }

    public void ResetObject()
    {
        // Reinicia la posició, velocitat i altres valors
        transform.position = startPosition;
        speed = originalSpeed;
        StopAllCoroutines(); // Atura qualsevol acció pendent
    }
}
