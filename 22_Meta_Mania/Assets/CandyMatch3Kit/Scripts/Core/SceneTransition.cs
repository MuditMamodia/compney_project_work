// Copyright (C) 2017 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections.Generic;
namespace GameVanilla.Core
{
    // This class is responsible for loading the next scene in a transition.
    public class SceneTransition : MonoBehaviour
    {
        public string scene = "<Insert scene name>";
        public float duration = 1.0f;
        public Color color = Color.black;

        //changes by mudit start herer
        [Header("panel refrence")]
        public List<GameObject> logo_screen_panel;
        public GameObject instruction_video_panel;
        [Header("Video Reference")]
        public VideoPlayer videoPlayer;

        [Header("Button reference")]
        public Button skipButton;

        //changes by mudit ends here

        /// <summary>
        /// Performs the transition to the next scene.
        /// </summary>
        public void PerformTransition()
        {
            Transition.LoadLevel(scene, duration, color);
        }

        // changes by mudit start here

        public void start_video_then_game()
        {
            if (logo_screen_panel != null)
            {
                foreach (var logo in logo_screen_panel)
                {
                    logo.gameObject.SetActive(false);
                }
            }
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
            //SceneManager.LoadScene(scene);

            Transition.LoadLevel(scene, duration, color);
        }

    }
}

