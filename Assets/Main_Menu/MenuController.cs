using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.VisualScripting;

public class MenuController : MonoBehaviour
{
    [Header("Volume Setting")]
    [SerializeField] private TMP_Text volumeTextValue = null;
    [SerializeField] private Slider volumeSlider = null;
    [SerializeField] private float defaultVolume = 1.0f;

    [Header("Confirmation")]
    [SerializeField] private GameObject comfirmationPrompt = null;

    [Header("Levels To Load")]
    public string EndlessScene;
    public string StoryScene;
    private string levelToLoad;
    [SerializeField] private GameObject noSavedGameDialog = null;

    [Header("Main Menu UI")]
    [SerializeField] private GameObject mainMenuUI = null;

    private bool isPaused = false;

    public void NewGameDialogYesEndless()
    {
        GameManager.Instance.currentGameMode = GameManager.GameMode.Endless;
        Time.timeScale = 1f;
        GameManager.Instance.SetLevel(1);
        SceneManager.LoadScene(EndlessScene);
    }
    
    public void NewGameDialogYesStory()
    {
        GameManager.Instance.currentGameMode = GameManager.GameMode.StoryMode;
        Time.timeScale = 1f;
        GameManager.Instance.SetLevel(0);
        SceneManager.LoadScene(StoryScene);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && mainMenuUI != null && Time.timeScale == 1f)
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        isPaused = !isPaused;
        mainMenuUI.SetActive(isPaused);

        if (isPaused)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void ResumeButton()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        mainMenuUI.SetActive(false);
    }
    
    public void MainMenuButton()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        mainMenuUI.SetActive(false);
        SceneManager.LoadScene("main_menu");
    }

    public void LoadGameDialogYes()
    {
        if (PlayerPrefs.HasKey("SavedLevel"))
        {
            levelToLoad = PlayerPrefs.GetString("SavedLevel");
            SceneManager.LoadScene(levelToLoad);
        }
        else
        {
            noSavedGameDialog.SetActive(true);
        }
    }

    public void ExitButton()
    {
        Application.Quit();
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume; 
        volumeTextValue.text = volume.ToString("0.0");
    }

    public void VolumeApply()
    {
        PlayerPrefs.SetFloat("masterVolume", AudioListener.volume);
        StartCoroutine(ConfirmationBox());
    }

    public void ResetButton(string MenuType)
    {
        if(MenuType == "Audio") 
        {
            AudioListener.volume = defaultVolume;
            volumeSlider.value = defaultVolume;
            volumeTextValue.text = defaultVolume.ToString("0.0");
            VolumeApply();
        }
        
    }

    public IEnumerator ConfirmationBox()
    {
        comfirmationPrompt.SetActive(true);
        yield return new WaitForSeconds(2);
        comfirmationPrompt.SetActive(false);
    }
   
}