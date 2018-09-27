using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadNextScene : MonoBehaviour {
    public float startDelay = 3f;
    public int sceneId = 1;
    private bool isStarted = false;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if(!isStarted)
        {
            StartCoroutine(LoadScene());
            isStarted = true;
        }
	}

    IEnumerator LoadScene()
    {
        yield return new WaitForSeconds(startDelay);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneId);
        while(!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
