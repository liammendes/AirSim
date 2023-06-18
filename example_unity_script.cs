using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class SelfDrivingCar : MonoBehaviour
{
    private const string OpenAIEndpoint = "https://api.openai.com/v1/engines/davinci-codex/completions";

    [SerializeField] private string apiKey = "<YOUR_API_KEY>";

    private Coroutine currentRequestCoroutine;

    private void Start()
    {
        // Start the self-driving behavior
        StartSelfDriving();
    }

    private void StartSelfDriving()
    {
        // Continuously request actions from OpenAI
        currentRequestCoroutine = StartCoroutine(RequestOpenAIAction());
    }

    private void StopSelfDriving()
    {
        // Stop requesting actions from OpenAI
        if (currentRequestCoroutine != null)
            StopCoroutine(currentRequestCoroutine);
    }

    private IEnumerator RequestOpenAIAction()
    {
        while (true)
        {
            // Prepare the request data
            string prompt = "The car is currently at position (x, y) = (" + transform.position.x + ", " + transform.position.y + ").";
            string requestData = "{ \"prompt\": \"" + prompt + "\", \"max_tokens\": 1 }";

            // Create and send the HTTP request to OpenAI
            UnityWebRequest request = UnityWebRequest.Post(OpenAIEndpoint, requestData);
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // Process the response from OpenAI
                string responseJson = request.downloadHandler.text;
                OpenAIResponse response = JsonUtility.FromJson<OpenAIResponse>(responseJson);

                if (response != null && response.choices != null && response.choices.Length > 0)
                {
                    string action = response.choices[0].text.Trim();
                    Debug.Log("Action received from OpenAI: " + action);

                    // Perform the action (e.g., steer the car)
                    // Implement your own logic here based on the received action
                }
                else
                {
                    Debug.LogWarning("Invalid or empty response from OpenAI.");
                }
            }
            else
            {
                Debug.LogWarning("Error requesting action from OpenAI: " + request.error);
            }

            // Delay before sending the next request
            yield return new WaitForSeconds(1f);
        }
    }

    private void OnDestroy()
    {
        // Stop self-driving when the object is destroyed
        StopSelfDriving();
    }

    [System.Serializable]
    private class OpenAIResponse
    {
        public OpenAIChoice[] choices;
    }

    [System.Serializable]
    private class OpenAIChoice
    {
        public string text;
    }
}
