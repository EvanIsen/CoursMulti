using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class ChatManager : MonoBehaviour
{
    public TMP_InputField messageInput;
    public TMP_Text chatDisplay, playerDisplay;
    public Button sendButton;
    private ClientWebSocket _webSocket;
    private const string  _serverUrl = "ws://192.168.104.189:8080";

    async void Start()
    {
        _webSocket = new ClientWebSocket();
        
        try
        {
            Debug.Log("Connecting to the server : " + _serverUrl);
            await _webSocket.ConnectAsync(new Uri(_serverUrl), CancellationToken.None);
            Debug.Log("Connected to server : " + _serverUrl);

            _ = ReceiveMessages(); // Start a listener
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            throw;
        }

        sendButton.onClick.AddListener((() => SendMessageToServer(messageInput.text)));
    }



    private async void OnApplicationQuit()
    {
        if (_webSocket != null && _webSocket.State == WebSocketState.Open)
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            Debug.Log("Closing connection : " + _webSocket.State);
        }
    }
    
    
    
    private async Task ReceiveMessages()
    {
        var buffer = new byte[1024];
        while (_webSocket.State == WebSocketState.Open)
        {
            try
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                
                Debug.Log(message);
                chatDisplay.text += "\n" + message;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                break;
            }
        }

    }


    private async void SendMessageToServer(string message)
    {
            try
            {

                await _webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)), WebSocketMessageType.Text, true, CancellationToken.None);
                Debug.Log("Message sent : " + message);
                chatDisplay.text += "\n" + message;
                messageInput.text = String.Empty;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                
            }
    }
    
    
}
