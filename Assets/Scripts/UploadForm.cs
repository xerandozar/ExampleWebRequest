using System;
using System.Collections;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class UploadForm : MonoBehaviour
{
    public Texture2D UploadTexture;
    public UIDocument UIDocument;

    private VisualElement m_UploadForm;
    private TextField m_ApiUrl;
    private TextField m_Username;
    private TextField m_Password;
    private Button m_UploadImage;
    private Label m_Response;
    
    private int m_Processing;

    private void Start()
    {
        var root = UIDocument.rootVisualElement;
        
        m_UploadForm = root.Q<VisualElement>("UploadForm");
        
        var apiUrlContainer = m_UploadForm.Q<VisualElement>("ApiUrl");
        m_ApiUrl = apiUrlContainer.Q<TextField>("Input");
        
        var usernameContainer = m_UploadForm.Q<VisualElement>("Username");
        m_Username = usernameContainer.Q<TextField>("Input");
        
        var passwordContainer = m_UploadForm.Q<VisualElement>("Password");
        m_Password = passwordContainer.Q<TextField>("Input");
        
        m_UploadImage = m_UploadForm.Q<Button>("UploadImage");
        m_UploadImage.clicked += OnUploadImageClick;
        
        var responseForm = root.Q<VisualElement>("ResponseForm");
        m_Response = responseForm.Q<Label>("Text");
        
        Interlocked.Exchange(ref m_Processing, 0);
    }

    private void OnUploadImageClick()
    {
        if (Interlocked.Exchange(ref m_Processing, 1) == 1)
            return;
        
        string url = m_ApiUrl.value;
        string username = m_Username.value;
        string password = m_Password.value;
        byte[] imageData = UploadTexture.EncodeToPNG();
        m_UploadForm.SetEnabled(false);
        
        Debug.Log($"PostRequest: {{ Url: {url}, Username: {username}, Password: {password} }}");
        
        byte[] authData = Encoding.ASCII.GetBytes($"{username}:{password}");
        string authBase64 = Convert.ToBase64String(authData);
        string authHeader = $"Basic {authBase64}";
        
        var form = new WWWForm();
        form.AddBinaryData("image", imageData, "image.png", "image/png"); 
        
        StartCoroutine(PostRequest(url, form, authHeader));
    }

    IEnumerator PostRequest(string url, WWWForm form, string authHeader)
    {
        using var request = UnityWebRequest.Post(url, form);
        request.SetRequestHeader("Authorization", authHeader);
        request.timeout = 10;
        
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            string message = $"Response: {request.downloadHandler.text}";
            m_Response.text = message;
            Debug.Log(message);
        }
        else
        {
            string errorMessage = $"Error[{request.responseCode}]: {request.error}";
            m_Response.text = errorMessage;
            Debug.LogError(errorMessage);
        }
        
        m_UploadForm.SetEnabled(true);
        Interlocked.Exchange(ref m_Processing, 0);
    }
}
