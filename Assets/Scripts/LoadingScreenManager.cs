using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;


public class LoadingScreenManager : MonoBehaviour
{ 
    public static LoadingScreenManager Instance;
    public GameObject m_LoadingScreenObject;
    public Slider ProgressBar;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }
    public void SwitchToScene(string sceneName)
    {
        m_LoadingScreenObject.SetActive(true);
        ProgressBar.value = 0;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
