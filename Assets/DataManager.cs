using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    // Start is called before the first frame update
    public int resultatRuleta1 = 0;
    public int resultatRuleta2 = 0;
    public int multi = 0;
    public int playerAplicat = 0;
    public int playerGirat = 0;
    public int posicióJugador1; // Posició del jugador 1
    public int posicióJugador2; // Posició del jugador 2
    public int posicióJugador3; // Posició del jugador 3
    public int jugadorsRestants;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        switch (resultatRuleta1)
        {
            case 1:
                multi = 2;
                break;
            case 2:
                multi = 3;
                break;
            case 3:
                multi = 4;
                break;
        }
    }
}
