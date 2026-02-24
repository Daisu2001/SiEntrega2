using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class HttpTest : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private string apiBaseUrl = "https://my-json-server.typicode.com/Daisu2001/SiEntrega2/users/"; 
    private string rickAndMortyUrl = "https://rickandmortyapi.com/api/character/";

    [Header("UI General")]
    public TMP_Dropdown userDropdown; 
    public TMP_Text usernameDisplay; // Texto en la esquina
    public TMP_Text statusText;
    public GameObject UserSwap;
    
    [Header("Paneles de Pantalla")]
    public GameObject buscadorPanel; // El panel con el dropdown y botón buscar
    public GameObject deckContainer; // El panel con las 5 cartas y botón volver

    [Header("Colección de Cartas")]
    public CharacterCard[] cards; 

    void Start()
    {
        // Al empezar, solo mostramos el buscador
        RegresarAlBuscador();
    }

    public void OnSearchUser()
    {
        int selectedId = userDropdown.value; 
        StartCoroutine(GetUserData(selectedId));
    }

    // Función para el botón "Volver"
    public void RegresarAlBuscador()
    {
        buscadorPanel.SetActive(true);
        deckContainer.SetActive(false);
        UserSwap.gameObject.SetActive(false);
        usernameDisplay.gameObject.SetActive(false); // Ocultamos el nombre de la esquina
        statusText.text = "";
    }

    IEnumerator GetUserData(int id)
    {
        statusText.text = "Consultando API...";
        
        using (UnityWebRequest www = UnityWebRequest.Get(apiBaseUrl + id))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                statusText.text = "Error de conexión";
                yield break;
            }

            User user = JsonUtility.FromJson<User>(www.downloadHandler.text);

            if (user != null)
            {
                // Cambiamos de "pantalla"
                buscadorPanel.SetActive(false);
                deckContainer.SetActive(true);
                
                // Mostramos nombre en la esquina
                usernameDisplay.gameObject.SetActive(true);
                usernameDisplay.text = "Usuario: " + user.username;
                UserSwap.gameObject.SetActive(true);
                statusText.text = "";

                foreach(var card in cards) card.Clear();

                for (int i = 0; i < user.deck.Count && i < cards.Length; i++)
                {
                    StartCoroutine(GetRickAndMortyData(user.deck[i], cards[i]));
                }
            }
        }
    }

    IEnumerator GetRickAndMortyData(int charId, CharacterCard cardUI)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(rickAndMortyUrl + charId))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                CharacterData data = JsonUtility.FromJson<CharacterData>(www.downloadHandler.text);
                UnityWebRequest imgWww = UnityWebRequestTexture.GetTexture(data.image);
                yield return imgWww.SendWebRequest();

                if (imgWww.result == UnityWebRequest.Result.Success)
                {
                    Texture2D tex = DownloadHandlerTexture.GetContent(imgWww);
                    cardUI.SetData(data.name, data.species, tex);
                }
            }
        }
    }
}


[Serializable]
public class User
{
    public int id;
    public string username;
    public List<int> deck;
}

[Serializable]
public class CharacterData
{
    public string name;
    public string species;
    public string image;
}