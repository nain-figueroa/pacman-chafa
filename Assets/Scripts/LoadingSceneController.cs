using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingSceneController : MonoBehaviour
{
    private IEnumerator Start()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync("Game");
        
        operation.allowSceneActivation = false;

        while (operation.progress < 0.9f)
        {
            yield return null;
        }

        yield return new WaitForSeconds(1f);
        // yield return operation;

        operation.allowSceneActivation = true;
    }
}
