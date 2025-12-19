using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AnsweredQuestion // Keep this if quiz part is still used
{
    public string _question, _answer;
    public bool _answered_correctly;
}

public class SaveData
{
    public List<AnsweredQuestion> _Answered_questions = new();
    public float _Time_played;
    public long _Score;
    public int _Lifetime_questions_faced_count,
                _Lifetime_correct_answers_count,
                _Session_questions_faced_count,
                _Session_correct_answers_count,
                _Playthrough_questions_faced_count,
                _Playthrough_correct_answers_count,
                _Level_1_stars_count,
                _Level_2_stars_count,
                _Level_3_stars_count,
                _Level_1_correct_answer_count,
                _Level_2_correct_answer_count,
                _Level_3_correct_answer_count,
                _Bonus_round_stars_count;
    public bool _Did_they_choose_to_move_on_to_the_bonus_round; // Did they choose to move on to the bonus round
    public string _Last_session_id;

    // Question pool management - ensures all questions are seen before repeating
    public List<int> _Shuffled_question_indices = new(); // Shuffled order of question indices
    public int _Current_question_pool_index; // Current position in the shuffled pool
}

public class Survivor : MonoBehaviour
{
    public static Survivor _S;

    [Header("Video Page")]
    public List<Button> _Video_page_buttons;
    internal bool _is_entry = true;
    public VideoController _Video_controller;

    [Header("First Instruction Page")]
    public GameObject _First_instruction_page_obj;

    [Header("QnA")]
    public QuizManager _Quiz_Manager;
    internal int _question_index;
   

    public TMP_Text _Version_text;
    public List<AudioSource> _Audio_sources;
    public GameObject _Loading_obj;
    public List<AudioClip> _Jump_sound;

    [Header("Sound Button Settings")]
    [Tooltip("List of sound button GameObjects (parent buttons)")]
    public List<GameObject> _Sound_buttons;
    [Tooltip("0 for off 1 for on")]
    public List<Sprite> _Sound_sprites;

    [Header("Game Manager Reference")]
    //public GameManagerSpace.GameManager gameManager; // Reference to game manager for audio control

    internal SaveData _save_data = new();

    List<float> _default_audio_volumes = new();
    bool _is_muted, _started;

    void Awake()
    {
        if (_S == null)
        {
            _S = this;

            _Audio_sources = GetComponentsInChildren<AudioSource>().ToList();
            _Version_text.text = Application.version;

            _Audio_sources.ForEach(x => _default_audio_volumes.Add(x.volume)); // Store default audio volumes

            // Find GameManager if not assigned
            /*if (gameManager == null)
            {
                gameManager = FindFirstObjectByType<GameManagerSpace.GameManager>();
            }*/

            // Load mute state from PlayerPrefs
            _is_muted = PlayerPrefs.GetInt("GameMuted", 0) == 1;

            if (_is_muted)
            {
                _is_muted = false;
                Button_Toggle_Sound(); // Apply mute state to audio sources and buttons
            }

            UpdateSoundButtonSprites();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if (_is_entry)
        {
            _Loading_obj.SetActive(true);

        }

        _Video_page_buttons.ForEach(btn =>
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                _Video_controller.Stop_video();
                _Audio_sources[11].Play();

                if (_is_entry)
                {
                    _is_entry = false;
                    _First_instruction_page_obj.SetActive(true);
                }
                else
                {
                    //_Leaderboard_page_obj.SetActive(true);
                }

                _Video_page_buttons.ForEach(x => x.gameObject.SetActive(false));     //  Hide "Continue" button but the "Skip" button MUST stay ON
                _Video_controller._Video_player.transform.parent.gameObject.SetActive(false); // Deactivate the video page

            });
        });

        // Initialize the question pool system - ensures all questions are seen before repeating
    }

    void Update()
    {
        if (_started)
        {
            _save_data._Time_played += Time.deltaTime;
        }
    }

    public void Button_Toggle_Sound()
    {
        // Toggle mute state
        _is_muted = !_is_muted;

        // Save mute state
        PlayerPrefs.SetInt("GameMuted", _is_muted ? 1 : 0);
        PlayerPrefs.Save();

        // Apply to local audio sources
        if (_is_muted)
        {
            _Audio_sources.ForEach(x => x.volume = 0);
        }
        else
        {
            for (int j = 0; j < _Audio_sources.Count; j++)
            {
                if (j < _default_audio_volumes.Count)
                {
                    _Audio_sources[j].volume = _default_audio_volumes[j];
                }
            }
        }

        // Update button sprites
        UpdateSoundButtonSprites();
    }



    private void UpdateSoundButtonSprites()
    {
        if (_Sound_sprites == null || _Sound_sprites.Count < 2)
        {
            Debug.LogError("Sound sprites not set! Please assign 2 sprites (0=off, 1=on) in Inspector");
            return;
        }

        // Update all sound button child images
        for (int i = 0; i < _Sound_buttons.Count; i++)
        {
            if (_Sound_buttons[i] == null)
            {
                Debug.LogWarning($"Sound button {i} is NULL!");
                continue;
            }

            // Get ALL Image components in this button and its children
            Image[] allImages = _Sound_buttons[i].GetComponentsInChildren<Image>();

            if (allImages.Length == 0)
            {
                Debug.LogError($"No Image components found in button {i} or its children!");
                continue;
            }

            // Try to find the child image (not the button itself)
            Image targetImage = null;

            // If there's more than one Image, assume the first child is the icon
            if (allImages.Length > 1)
            {
                targetImage = allImages[1]; // Skip first (parent button), use second (child icon)
            }
            else
            {
                targetImage = allImages[0]; // Only one image, use it
            }

            if (targetImage != null)
            {
                Sprite newSprite = _is_muted ? _Sound_sprites[0] : _Sound_sprites[1];
                targetImage.sprite = newSprite;
            }
        }
    }

    internal IEnumerator Cor_load_scene_asynchronously(int p_index)
    {
        _Loading_obj.SetActive(true);
        AsyncOperation t_progress = SceneManager.LoadSceneAsync(p_index);

        while (!t_progress.isDone)
        {
            yield return null;
        }

        _Loading_obj.SetActive(false);
    }

    IEnumerator DuckBGMForAudio(AudioSource sfx, float duckPercentage = 0.3f)
    {
        // Get the BGM audio source (index 9)
        AudioSource bgm = _Audio_sources[9];

        // Use default volume as reference (not current volume which could be 0 if muted)
        float defaultBGMVolume = _default_audio_volumes[9];

        // Check if audio is currently muted (BGM volume is 0)
        bool isMutedAtStart = (bgm.volume == 0);

        if (!isMutedAtStart)
        {
            // Duck the BGM to specified percentage of default volume
            bgm.volume = defaultBGMVolume * duckPercentage;
        }

        // Play the sound effect
        sfx.Play();

        // Wait for the sound effect to finish playing
        yield return new WaitWhile(() => sfx.isPlaying);

        // Check if audio is currently muted (user may have toggled sound during ducking)
        // We check audio source 0 since Button_Toggle_Sound() sets all sources to 0 when muting
        bool isCurrentlyMuted = (_Audio_sources[0].volume == 0);

        // Restore BGM based on current mute state (not the saved state from start)
        bgm.volume = isCurrentlyMuted ? 0 : defaultBGMVolume;
    }

    public void Button_Start()
    {
        _started = true;
    }

    public void Button_Restart()
    {
        _Loading_obj.SetActive(true);
        StartCoroutine(Cor_load_scene_asynchronously(0));
    }

    // ==================== QUESTION POOL MANAGEMENT ====================
    // Ensures players see all questions before any repeats

    /// <summary>
    /// Initializes the question pool by creating and shuffling indices if needed.
    /// Call this at game start or when loading save data from server.
    /// </summary>
    public void InitializeQuestionPool()
    {
        int totalQuestions = _Quiz_Manager.Questions.Count;

        // If pool is empty or invalid, create a new shuffled pool
        if (_save_data._Shuffled_question_indices == null ||
            _save_data._Shuffled_question_indices.Count != totalQuestions)
        {
            ShuffleQuestionPool();

        }
    }

    /// <summary>
    /// Creates a new shuffled pool of all question indices.
    /// This ensures every question is included exactly once before reshuffling.
    /// </summary>
    void ShuffleQuestionPool()
    {
        int totalQuestions = _Quiz_Manager.Questions.Count;

        // Create list of all indices (0 to totalQuestions-1)
        _save_data._Shuffled_question_indices = new List<int>();
        for (int i = 0; i < totalQuestions; i++)
        {
            _save_data._Shuffled_question_indices.Add(i);
        }

        // Shuffle using Fisher-Yates algorithm for better randomization
        System.Random rng = new System.Random();
        int n = _save_data._Shuffled_question_indices.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            int temp = _save_data._Shuffled_question_indices[k];
            _save_data._Shuffled_question_indices[k] = _save_data._Shuffled_question_indices[n];
            _save_data._Shuffled_question_indices[n] = temp;
        }

        // Reset position to start
        _save_data._Current_question_pool_index = 0;
    }

    /// <summary>
    /// Gets the next question index from the shuffled pool.
    /// Automatically reshuffles when all questions have been used.
    /// </summary>
    /// <returns>The index of the next question to show</returns>
    internal int GetNextQuestionIndex()
    {
        // Safety check: initialize if needed
        if (_save_data._Shuffled_question_indices == null ||
            _save_data._Shuffled_question_indices.Count == 0)
        {
            InitializeQuestionPool();
        }

        // Check if we've exhausted the pool
        if (_save_data._Current_question_pool_index >= _save_data._Shuffled_question_indices.Count)
        {
            ShuffleQuestionPool();
        }

        // Get current question index and advance position
        int questionIndex = _save_data._Shuffled_question_indices[_save_data._Current_question_pool_index];
        _save_data._Current_question_pool_index++;

        return questionIndex;
    }

    /// <summary>
    /// Resets the question pool to start from the beginning.
    /// Call this when starting a new game session.
    /// </summary>
    internal void ResetQuestionPoolPosition()
    {
        _save_data._Current_question_pool_index = 0;
    }

    internal void Register_answer(bool p_correct)
    {
        _save_data._Lifetime_questions_faced_count++;
        _save_data._Playthrough_questions_faced_count++;
        _save_data._Session_questions_faced_count++;

        if (p_correct)
        {
            _save_data._Lifetime_correct_answers_count++;
            _save_data._Session_correct_answers_count++;
            _save_data._Playthrough_correct_answers_count++;

            if (_save_data._Current_question_pool_index <= 5)
            {
                _save_data._Level_1_correct_answer_count++;
            }
            else if (_save_data._Current_question_pool_index <= 10)
            {
                _save_data._Level_2_correct_answer_count++;
            }
            else
            {
                _save_data._Level_3_correct_answer_count++;
            }
        }
    }
}
