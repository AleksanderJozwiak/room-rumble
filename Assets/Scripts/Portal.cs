using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalInteraction : MonoBehaviour
{
    public string sceneToLoad;
    private bool isPlayerNear = false;

    void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.F))
        {
            LoadNextScene();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            Debug.Log("Press 'F' to enter the portal.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
        }
    }

    void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            GameManager.Instance.SetLevel(GameManager.Instance.GetLevel() + 1);
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogError("Scene name not set in PortalInteraction script.");
        }
    }
}
