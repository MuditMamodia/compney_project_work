using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;


public class main_menu_start_game_video : MonoBehaviour
{
    [Header("panel refrence")]
    public List< GameObject> logo_screen_panel;
    public GameObject video_panel;
    [Header("Video Reference")]
    public VideoPlayer videoPlayer;

    [Header("Button reference")]
    public Button skipButton;
    private void Awake()
    {
        if (logo_screen_panel != null)
        {
            foreach (var logo in logo_screen_panel)
            {
                logo.gameObject.SetActive(false);
            }
        }
        video_panel.SetActive(true);
        playingvideo_interoo();

        // Skip button
        skipButton.onClick.AddListener(skip_button);
    }
    

    void playingvideo_interoo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += onvideoends;
        }
    }

    void onvideoends(VideoPlayer vp)
    {
        if (logo_screen_panel != null)
        {
            foreach (var logo in logo_screen_panel)
            {
                logo.gameObject.SetActive(true);
            }
        }
        video_panel.SetActive(false);
    }
    void skip_button()
    {
        if (logo_screen_panel != null)
        {
            foreach (var logo in logo_screen_panel)
            {
                logo.gameObject.SetActive(true);
            }
        }
        video_panel.SetActive(false);
    }
}
