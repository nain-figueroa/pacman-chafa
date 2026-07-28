using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Animator tittleAnimator, startButtonAnimator;
    [SerializeField] private Button exitButton;
    [SerializeField] private TextMeshProUGUI versionText;
    public void StartGame()
    {
        StartCoroutine(StartSequence());
    }

    public void ExitGame()
    {
        #if UNITY_EDITOR
            EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private IEnumerator StartSequence()
    {
        tittleAnimator.SetBool("isStart", true);
        startButtonAnimator.SetBool("isStart", true);
        exitButton.gameObject.SetActive(false);
        versionText.gameObject.SetActive(false);
        
        yield return new WaitForSeconds(1.28f);
        
        SceneManager.LoadScene("LoadingScene");
    }
}
