using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalInteraction : MonoBehaviour
{
    public string sceneToLoad;
    private bool isPlayerNear = false;
    private GameObject PortalInteractionText;

    void Awake()
    {
        PortalInteractionText = GameObject.FindGameObjectWithTag("PortalInteractionText");

        if (PortalInteractionText == null)
        {
            Debug.LogError("PortalInteractionText GameObject not found. Please ensure it's tagged correctly.");
        }
        else
        {
            PortalInteractionText.SetActive(false);
        }
    }


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
            PortalInteractionText.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            PortalInteractionText.SetActive(false);
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
