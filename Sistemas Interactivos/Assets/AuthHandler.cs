using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Linq;

public class AuthHandler : MonoBehaviour
{
    private string Token = "";
    private string Username = ""; // Usaremos esta variable para la URL dinámica
    private string apiiUrl = "https://sid-restapi.onrender.com";

    [Header("UI References")]
    [SerializeField] private TMP_InputField usernameInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private TMP_InputField scoreInputField;
    [SerializeField] private TMP_Text usernameLabel;
    [SerializeField] private TMP_Text leaderboardText;
    [SerializeField] private GameObject panelLogin;
    [SerializeField] private GameObject panelDashboard;

    private void Start()
    {
        // Limpieza de seguridad al iniciar
        Token = "";
        Username = "";
        SetUIState(false);
    }

    // --- BOTONES ---

    public void RegisterButtonHandler() => StartCoroutine(RegisterRoutine());
    public void LoginButtonHandler() => StartCoroutine(LoginRoutine());
    public void LogoutButtonHandler()
    {
        Token = ""; Username = "";
        SetUIState(false);
    }
    public void UpdateScoreButtonHandler()
    {
        if (int.TryParse(scoreInputField.text, out int score))
            StartCoroutine(UpdateScoreRoutine(score));
    }

    // --- LÓGICA DE RED ---

    IEnumerator RegisterRoutine()
    {
        AuthData authData = new AuthData { username = usernameInputField.text, password = passwordInputField.text };
        string jsonData = JsonUtility.ToJson(authData);

        using (UnityWebRequest www = UnityWebRequest.Post(apiiUrl + "/api/usuarios", jsonData, "application/json"))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("<color=green>Registro exitoso.</color> Procede al Login.");
                passwordInputField.text = ""; 
            }
            else Debug.LogError("Error en registro: " + www.downloadHandler.text);
        }
    }

    IEnumerator LoginRoutine()
    {
        AuthData authData = new AuthData { username = usernameInputField.text, password = passwordInputField.text };
        string jsonData = JsonUtility.ToJson(authData);

        using (UnityWebRequest www = UnityWebRequest.Post(apiiUrl + "/api/auth/login", jsonData, "application/json"))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                AuthResponse response = JsonUtility.FromJson<AuthResponse>(www.downloadHandler.text);
                Token = response.token; // Guardamos el token real
                Username = response.usuario.username; // Guardamos el nombre dinámico

                SetUIState(true);
                StartCoroutine(GetUsersRoutine());
            }
            else Debug.LogError("Login fallido. Revisa tus credenciales.");
        }
    }

IEnumerator UpdateScoreRoutine(int score)
{
    if (string.IsNullOrEmpty(Token) || string.IsNullOrEmpty(Username)) 
    {
        Debug.LogError("Error: Sesión no válida.");
        yield break;
    }

    // AJUSTE CRÍTICO: Incluimos el "username" dentro del JSON como pide el error 400
    // Estructura: { "username": "tu_usuario", "data": { "score": 100 } }
    string json = "{\"username\":\"" + Username + "\",\"data\":{\"score\":" + score + "}}";
    
    // Usamos la ruta base que funcionó en tu Postman
    string url = "https://sid-restapi.onrender.com/api/usuarios"; 

    Debug.Log("Enviando a: " + url + " con JSON: " + json);

    UnityWebRequest www = new UnityWebRequest(url, "PATCH");
    byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
    www.uploadHandler = new UploadHandlerRaw(bodyRaw);
    www.downloadHandler = new DownloadHandlerBuffer();

    www.SetRequestHeader("Content-Type", "application/json");
    www.SetRequestHeader("x-token", Token);

    yield return www.SendWebRequest();

    if (www.result == UnityWebRequest.Result.Success)
    {
        Debug.Log("<color=green>¡Puntaje actualizado!</color>");
        StartCoroutine(GetUsersRoutine());
        scoreInputField.text = "";
    }
    else
    {
        // Esto te mostrará si falta algún otro campo
        Debug.LogError($"Error {www.responseCode}: {www.downloadHandler.text}");
    }
}

    IEnumerator GetUsersRoutine()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(apiiUrl + "/api/usuarios?sort=true"))
        {
            www.SetRequestHeader("x-token", Token);
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                userlist res = JsonUtility.FromJson<userlist>(www.downloadHandler.text);
                leaderboardText.text = "<b>LEADERBOARD</b>\n";
                var sorted = res.usuarios.OrderByDescending(u => u.data.score).ToList();
                foreach (var u in sorted) leaderboardText.text += $"{u.username}: {u.data.score}\n";
            }
        }
    }

    private void SetUIState(bool loggedIn)
    {
        panelLogin.SetActive(!loggedIn);
        panelDashboard.SetActive(loggedIn);
        if (loggedIn) usernameLabel.text = "Usuario: " + Username;
    }
}

// Estructuras de datos
[System.Serializable] public class AuthData { public string username; public string password; }
[System.Serializable] public class userData { public int score = 0; }
[System.Serializable] public class User { public string username; public userData data; }
[System.Serializable] public class AuthResponse { public User usuario; public string token; }
[System.Serializable] public class userlist { public User[] usuarios; }