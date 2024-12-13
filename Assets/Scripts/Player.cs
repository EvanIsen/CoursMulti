using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

public class Player : MonoBehaviour
{
    private ClientWebSocket _webSocket = new ClientWebSocket();
    private static readonly Uri _serverUrl = new Uri("ws://192.168.104.189:8080");
    private Dictionary<string, GameObject> _players = new Dictionary<string, GameObject>();

    public Transform playerTransform;

    private async void Start()
    {
        await ConnectedToServer();
    }

    private async Task ConnectedToServer()
    {
        try
        {
            await _webSocket.ConnectAsync(_serverUrl, CancellationToken.None);
            Debug.Log("Connected to server");
            _ = ReceiveMessage();
            InvokeRepeating(nameof(SendPlayerPosition), 1f, 0.1f);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    
    private async Task ReceiveMessage()
    {
        var buffer = new byte[1024];
        while (_webSocket.State == WebSocketState.Open)
        {
            try
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                string json = JsonConvert.DeserializeObject<string>(message);
                
                // trier les actions 
                // objectif : récupérer la position du joueur et l'id associé
                // Diffuser à tout le monde
                
                
                Debug.Log(json);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                break;
            }
        }
    }
    
    private async void SendMessage()
    {

    }

    private async void SendPlayerPosition()
    {
        if (_webSocket.State == WebSocketState.Open)
        {
            var message = new
            {
                type = "Position",
                x = playerTransform.position.x,
                y = playerTransform.position.y,
                z = playerTransform.position.z
            };
            
            string messageToSend = JsonConvert.SerializeObject(message);
            var buffer = Encoding.UTF8.GetBytes(messageToSend);
            await _webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    private async void OnApplicationQuit()
    {
        if (_webSocket.State == WebSocketState.Open)
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
        }
    }
}
