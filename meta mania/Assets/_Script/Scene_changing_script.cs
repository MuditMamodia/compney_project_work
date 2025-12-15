using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class Scene_changing_script : MonoBehaviour
{
    [Header("Scene To Load")]
    public int scene_number;

    [Header("Video Reference")]
    public VideoPlayer videoPlayer;

    [Header("Button reference")]
    public Button skipButton;

    [Header("panel refrence")]
    public GameObject logo_screen_panel;
    public GameObject instruction_video_panel;

    void Start()
    {
        
    }

    void start_video_then_game()
    {
        logo_screen_panel.SetActive(false);
        instruction_video_panel.SetActive(true);

        if (videoPlayer != null)
        {
            // Call automatically when video finishes
            videoPlayer.loopPointReached += OnVideoEnd;
        }
        if (skipButton != null)
        {
            // Skip button
            skipButton.onClick.AddListener(SkipVideo);
        }
    }

    // Called when video finishes
    void OnVideoEnd(VideoPlayer vp)
    {
        LoadNextScene();
    }

    // Skip button function
    void SkipVideo()
    {
        LoadNextScene();
    }

    // Loads the next scene
    void LoadNextScene()
    {
        SceneManager.LoadScene(scene_number);
    }

}
