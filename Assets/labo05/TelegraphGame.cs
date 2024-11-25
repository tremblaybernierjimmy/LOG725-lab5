using System;
using UnityEngine;
using Unity.Netcode;
using System.Text;
using TMPro;
using UnityEngine.Rendering; // Nécessaire pour utiliser TextMeshPro

public class TelegraphGame : NetworkBehaviour
{
    [SerializeField] private TMP_InputField _hostTextField;
    [SerializeField] private TMP_Text _morsePreviewText;
    [SerializeField] private TMP_Text _clientTextLabel;
    [SerializeField] private TMP_Text _outputLabel;
    [SerializeField] private TMP_Text _clientOutputLabel;
    [SerializeField] private GameObject _hostUI;
    [SerializeField] private GameObject _mainMenuUI;
    [SerializeField] private GameObject _clientUI;
    [SerializeField] private AudioSource _dotAudioSource;
    [SerializeField] private AudioSource _dashAudioSource;
    
    private string _hostText = "";
    private string _hostMorse = "";
    private StringBuilder _clientAnswer = new StringBuilder();
    private bool _gameOver = true;
    private bool _canWriteMorseCode = false;

    private void Start()
    {
        Debug.developerConsoleVisible = true;
        _hostUI.SetActive(false);
        _clientUI.SetActive(false);
        _mainMenuUI.SetActive(true);
    }

    void Update()
    {
        if (IsHost || !_canWriteMorseCode) return;

        if (Input.GetKeyDown(KeyCode.Period))
        {
            PlayDotServerRpc();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayDashServerRpc();
        }
    }
    
    public void OnHostPressed()
    {
        NetworkManager.Singleton.StartHost();
        ShowHostUI();
    }

    public void OnJoinPressed()
    {
        NetworkManager.Singleton.StartClient();
        ShowClientUI();
    }

    public void OnSendPressed()
    {
        _clientAnswer.Clear();
        _gameOver = false;

        UpdateSendText();
        _outputLabel.text = "Waiting for client";

        ShowTextOnClientRpc(_hostText);
    }

    public void UpdateSendText()
    {
        _hostText = _hostTextField.text;
        _hostMorse = GetMorseFromString(_hostText);
        _morsePreviewText.text = _hostMorse;
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void PlayDotServerRpc()
    {
        if (_gameOver) return;
        _clientAnswer.Append(".");
        _outputLabel.text = _clientAnswer.ToString();
        _dotAudioSource.Play();
        CheckVictory();
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void PlayDashServerRpc()
    {
        if (_gameOver) return;
        _clientAnswer.Append("-");
        _outputLabel.text = _clientAnswer.ToString();
        _dashAudioSource.Play();
        CheckVictory();
    }
    
    private string GetMorseFromString(string text)
    {
        var morseCode = new System.Collections.Generic.Dictionary<char, string>
        {
            { 'A', ".-" }, { 'B', "-..." }, { 'C', "-.-." }, { 'D', "-.." }, { 'E', "." },
            { 'F', "..-." }, { 'G', "--." }, { 'H', "...." }, { 'I', ".." }, { 'J', ".---" },
            { 'K', "-.-" }, { 'L', ".-.." }, { 'M', "--" }, { 'N', "-." }, { 'O', "---" },
            { 'P', ".--." }, { 'Q', "--.-" }, { 'R', ".-." }, { 'S', "..." }, { 'T', "-" },
            { 'U', "..-" }, { 'V', "...-" }, { 'W', ".--" }, { 'X', "-..-" }, { 'Y', "-.--" },
            { 'Z', "--.." }, { '1', ".----" }, { '2', "..---" }, { '3', "...--" }, { '4', "....-" },
            { '5', "....." }, { '6', "-...." }, { '7', "--..." }, { '8', "---.." }, { '9', "----." },
            { '0', "-----" }, { ' ', "/" }
        };

        var morseText = new StringBuilder();
        foreach (char c in text.ToUpper())
        {
            if (morseCode.TryGetValue(c, out var morseChar))
            {
                morseText.Append(morseChar);
            }
        }
        return morseText.ToString();
    }
    
    private void CheckVictory()
    {
        if (_clientAnswer.Length != _hostMorse.Length)
        {
            return;
        }


        if(_clientAnswer.ToString().Equals(_hostMorse))
        {
            _outputLabel.text = "CONGRATULATION !!!";
            _gameOver = true;
            SendVictoryToClientRpc();
        }
        else
        {
            _outputLabel.text = "Wrong Answer, let's retry !";
            SendDefeatToClientRpc();
            _clientAnswer.Clear();
        }
    }
    
    [ClientRpc]
    private void ShowTextOnClientRpc(string text)
    {
        if (IsHost) return;
        _gameOver = false;
        _clientTextLabel.text = text;
        _clientOutputLabel.text = "New Message Received !";
        _canWriteMorseCode = true;
        _outputLabel.text = "";
    }

    [ClientRpc]
    private void SendVictoryToClientRpc()
    {
        if (IsHost) return;
        _gameOver = true;
        _canWriteMorseCode = false;
        _clientOutputLabel.text = "CONGRATULATION!!!";
    }

    [ClientRpc]
    private void SendDefeatToClientRpc()
    {
        if (IsHost) return;
        _clientOutputLabel.text = "Wrong Answer, let's retry !";
    }

    private void ShowHostUI()
    {
        _hostUI.SetActive(true);
        _mainMenuUI.SetActive(false);
    }
    
    private void ShowClientUI()
    {
        _clientUI.SetActive(true);
        _mainMenuUI.SetActive(false);
    }
}
