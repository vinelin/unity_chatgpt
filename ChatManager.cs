using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Collections;

public class ChatManager : MonoBehaviour
{
    public Text chatText;
    public InputField inputField;
    public Button sendButton;

    private List<string> messages = new List<string>();
    private string API_KEY = "";
    private string API_URL = "https://api.openai.com/v1/chat/completions";

    [System.Serializable]
    private class ChatCompletionRequest
    {
        public string model;
        public List<Message> messages;
    }

    [System.Serializable]
    private class Message
    {
        public string role;
        public string content;
    }

    [System.Serializable]
    private class ChatCompletionResponse
    {
        public string id;
        public string @object;
        public long created;
        public List<Choice> choices;
        public Usage usage;
    }

    [System.Serializable]
    private class Choice
    {
        public int index;
        public Message message;
        public string finish_reason;
    }

    [System.Serializable]
    private class Usage
    {
        public int prompt_tokens;
        public int completion_tokens;
        public int total_tokens;
    }

    void Start()
    {
        sendButton.onClick.AddListener(SendMessage);
    }

    void SendMessage()
    {
        string message = inputField.text;
        if (string.IsNullOrEmpty(message))
        {
            return;
        }
        inputField.text = "";
        var temp = "<color=blue><b>You:</b></color>" + message;
        AddMessage(temp);
        // Call the chat completion API
        StartCoroutine(CallChatCompletionAPI(message));
    }

    IEnumerator CallChatCompletionAPI(string prompt)
    {
        string postData = "{\"model\": \"gpt-3.5-turbo\", \"messages\": [{\"role\": \"user\", \"content\": \"" + prompt + "\"}]}";

        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(API_URL);
        request.Headers.Add("Authorization", "Bearer " + API_KEY);
        request.Method = "POST";
        request.ContentType = "application/json";
        byte[] bytes = Encoding.UTF8.GetBytes(postData);
        request.ContentLength = bytes.Length;

        using (Stream requestStream = request.GetRequestStream())
        {
            requestStream.Write(bytes, 0, bytes.Length);
        }

        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        string result = "";
        using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
        {
            result = streamReader.ReadToEnd();
        }

        // Parse the response and add the AI message to the chat
        string aiMessage = ParseChatCompletionResponse(result);
        aiMessage = aiMessage.Trim();
        aiMessage = "<color=green><b>Bot:</b></color>" + aiMessage;
        AddMessage(aiMessage);
        yield return null;
    }

    string ParseChatCompletionResponse(string response)
    {
        ChatCompletionResponse responseObj = JsonUtility.FromJson<ChatCompletionResponse>(response);
        string aiMessage = "";
        foreach (Choice choice in responseObj.choices)
        {
            aiMessage += choice.message.content;
        }
        return aiMessage;
    }

    void AddMessage(string message)
    {
        messages.Insert(0, message);
        StringBuilder sb = new StringBuilder();
        foreach (string msg in messages)
        {
                sb.AppendLine(msg);
        }
        chatText.text = sb.ToString();
    }
}
