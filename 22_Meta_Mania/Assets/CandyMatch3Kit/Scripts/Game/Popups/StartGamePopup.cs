// Copyright (C) 2017 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

using FullSerializer;

using GameVanilla.Core;
using GameVanilla.Game.Common;
using GameVanilla.Game.UI;

namespace GameVanilla.Game.Popups
{
    /// <summary>
    /// This class contains the logic associated to the popup that is shown before starting a game.
    /// </summary>
    public class StartGamePopup : Popup
    {
#pragma warning disable 649
        [SerializeField]
        private Text levelText;

        [SerializeField]
        private Sprite enabledStarSprite;

        [SerializeField]
        private Image star1Image;

        [SerializeField]
        private Image star2Image;

        [SerializeField]
        private Image star3Image;

        [SerializeField]
        private GameObject goalPrefab;

        [SerializeField]
        private GameObject goalGroup;

        [SerializeField]
        private GameObject scoreGoalGroup;

        [SerializeField]
        private Text scoreGoalAmountText;

        [SerializeField]
        private GameObject scoreGoalOnlyItem;

        [SerializeField]
        private Text scoreGoalOnlyItemText;

        [SerializeField]
        private GameObject playButton;

        [SerializeField]
        private Transform playButtonPositionScore;

        [SerializeField]
        private Transform playButtonPositionNoScore;
#pragma warning restore 649

        private int numLevel;

        // Mudit's changes start's here -----------------------------------

        [Header("panel reference")]
        [SerializeField] private GameObject grabhic_panel_to_earn_boost;
        [SerializeField] private Button correct_button;

        // Mudit's changes ends here --------------------------------------

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            Assert.IsNotNull(levelText);
            Assert.IsNotNull(enabledStarSprite);
            Assert.IsNotNull(star1Image);
            Assert.IsNotNull(star2Image);
            Assert.IsNotNull(star3Image);
            Assert.IsNotNull(goalPrefab);
            Assert.IsNotNull(goalGroup);
            Assert.IsNotNull(scoreGoalGroup);
            Assert.IsNotNull(scoreGoalAmountText);
            Assert.IsNotNull(scoreGoalOnlyItem);
            Assert.IsNotNull(scoreGoalOnlyItemText);
            Assert.IsNotNull(playButton);
            Assert.IsNotNull(playButtonPositionScore);
            Assert.IsNotNull(playButtonPositionNoScore);
        }

        private void Start()
        {
            correct_button.onClick.AddListener(()=> boost_granted());
            grabhic_panel_to_earn_boost.SetActive(false);
        }

        /// <summary>
        /// Loads the level data corresponding to the specified level number.
        /// </summary>
        /// <param name="levelNum">The number of the level to load.</param>
        public void LoadLevelData(int levelNum)
        {
            numLevel = levelNum;

            var serializer = new fsSerializer();
            var level = FileUtils.LoadJsonFile<Level>(serializer, "Levels/" + numLevel);
            levelText.text = "Level " + numLevel;
            var stars = PlayerPrefs.GetInt("level_stars_" + numLevel);
            if (stars == 1)
            {
                star1Image.sprite = enabledStarSprite;
            }
            else if (stars == 2)
            {
                star1Image.sprite = enabledStarSprite;
                star2Image.sprite = enabledStarSprite;
            }
            else if (stars == 3)
            {
                star1Image.sprite = enabledStarSprite;
                star2Image.sprite = enabledStarSprite;
                star3Image.sprite = enabledStarSprite;
            }

            foreach (var goal in level.goals)
            {
                if (!(goal is ReachScoreGoal))
                {
                    var goalObject = Instantiate(goalPrefab);
                    goalObject.transform.SetParent(goalGroup.transform, false);
                    goalObject.GetComponent<GoalUiElement>().Fill(goal);
                }
            }
            var reachScoreGoal = level.goals.Find(x => x is ReachScoreGoal);
            if (reachScoreGoal != null)
            {
                if (level.goals.Count == 1)
                {
                    scoreGoalGroup.SetActive(false);
                    scoreGoalOnlyItem.SetActive(true);
                    scoreGoalOnlyItemText.text = ((ReachScoreGoal)reachScoreGoal).score.ToString();
                    playButton.transform.position = playButtonPositionNoScore.position;
                }
                else
                {
                    scoreGoalGroup.SetActive(true);
                    scoreGoalOnlyItem.SetActive(false);
                    scoreGoalAmountText.text = ((ReachScoreGoal)reachScoreGoal).score.ToString();
                    playButton.transform.position = playButtonPositionScore.position;
                }
            }
            else
            {
                scoreGoalGroup.SetActive(false);
                scoreGoalOnlyItem.SetActive(false);
                playButton.transform.position = playButtonPositionNoScore.position;
            }
        }

        /// <summary>
        /// Called when the play button is pressed.
        /// </summary>
        public void OnPlayButtonPressed()
        {
            // Mudit's changes start's here -----------------------------------
            //Booster_manager_singleton_m.Instance.giveRandomBooster = false;
            //Booster_manager_singleton_m.Instance.boostersToGive = 0;
            // Mudit's changes ends here --------------------------------------
            Debug.Log("bc function ye call huaa OnPlayButtonPressed");
            PuzzleMatchManager.instance.lastSelectedLevel = numLevel;
            GetComponent<SceneTransition>().PerformTransition();
        }

        /// <summary>
        /// Called when the close button is pressed.
        /// </summary>
        public void OnCloseButtonPressed()
        {
            Close();
        }

        // Mudit's changes start's here -----------------------------------

        public void onGet_early_boost_starts_m()
        {
            grabhic_panel_to_earn_boost.SetActive(true);
            Debug.Log("bc function ye call huaa onGet_early_boost_starts_m");
        }

        public void boost_granted()
        {
            Booster_manager_singleton_m.Instance.giveRandomBooster = true;
            Booster_manager_singleton_m.Instance.boostersToGive = 1;


            PuzzleMatchManager.instance.lastSelectedLevel = numLevel;
            GetComponent<SceneTransition>().PerformTransition();

            Debug.Log("bc function ye call huaa boost_granted");
        }

        // Mudit's changes ends here --------------------------------------

    }
}
