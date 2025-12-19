// Copyright (C) 2017 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

using GameVanilla.Core;
using GameVanilla.Game.Common;
using GameVanilla.Game.Scenes;

namespace GameVanilla.Game.Popups
{
    /// <summary>
    /// This class contains the logic associated to the popup that offers the player to buy more moves or time
    /// after he loses a game.
    /// </summary>
    public class NoMovesOrTimePopup : Popup
    {
#pragma warning disable 649
        [SerializeField]
        private Text numCoinsText;

        [SerializeField]
        private GameObject movesGroup;

        [SerializeField]
        private GameObject timeGroup;

        [SerializeField]
        private Text title1Text;

        [SerializeField]
        private Text title2Text;

        [SerializeField]
        private Text numExtraMovesText;

        [SerializeField]
        private Text costText;

        [SerializeField]
        private ParticleSystem coinParticles;
        
        [SerializeField]
        private GameObject girl;
        
        [SerializeField]
        private GameObject boy;
#pragma warning restore 649

        private GameScene gameScene;



        //mudit changes start here-------------------
        [SerializeField]
        private GameObject extraMovesPanel;   // the video panel ref
        public GameObject current_panel_Panel;  

        public script_for_adding_moves sfam;

        //mudit changes ends here

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            Assert.IsNotNull(numCoinsText);
            Assert.IsNotNull(movesGroup);
            Assert.IsNotNull(timeGroup);
            Assert.IsNotNull(title1Text);
            Assert.IsNotNull(title2Text);
            Assert.IsNotNull(numExtraMovesText);
            Assert.IsNotNull(costText);
            Assert.IsNotNull(coinParticles);
            Assert.IsNotNull(girl);
            Assert.IsNotNull(boy);

            sfam = FindObjectOfType<script_for_adding_moves>();
        }

        /// <summary>
        /// Unity's Start method.
        /// </summary>
        protected override void Start()
        {
            base.Start();
            var coins = PlayerPrefs.GetInt("num_coins");
            numCoinsText.text = coins.ToString("n0");
            var avatarSelected = PlayerPrefs.GetInt("avatar_selected");
            if (avatarSelected == 0)
                boy.SetActive(false);
            else
                girl.SetActive(false);
        }

        /// <summary>
        /// Sets the game scene associated to this popup.
        /// </summary>
        /// <param name="scene">The associated game scene.</param>
        public void SetGameScene(GameScene scene)
        {
            gameScene = scene;
            var gameConfig = PuzzleMatchManager.instance.gameConfig;
            if (gameScene.level.limitType == LimitType.Moves)
            {
                timeGroup.SetActive(false);
                title1Text.text = "Out of moves!";
                title2Text.text = string.Format("Add +{0} extra moves to continue.", gameConfig.numExtraMoves);
                costText.text = gameConfig.extraMovesCost.ToString();
                numExtraMovesText.text = string.Format("+{0}", gameConfig.numExtraMoves);
            }
            else
            {
                movesGroup.SetActive(false);
                title1Text.text = "Out of time!";
                title2Text.text = string.Format("Add +{0} extra seconds to continue.",
                    PuzzleMatchManager.instance.gameConfig.numExtraTime);
                costText.text = gameConfig.extraTimeCost.ToString();
            }
        }

        /// <summary>
        /// Called when the play button is pressed.
        /// </summary>
        public void OnPlayButtonPressed()
        {
            //if (gameScene.level.limitType == LimitType.Moves)
            //{
            //    var numCoins = PlayerPrefs.GetInt("num_coins");
            //    if (numCoins >= PuzzleMatchManager.instance.gameConfig.extraMovesCost)
            //    {
            //        PuzzleMatchManager.instance.coinsSystem.SpendCoins(PuzzleMatchManager.instance.gameConfig.extraMovesCost);
            //        coinParticles.Play();
            //        SoundManager.instance.PlaySound("CoinsPopButton");
            //        Close();
            //        gameScene.Continue();
            //    }
            //    else
            //    {
            //        SoundManager.instance.PlaySound("Button");
            //        OpenCoinsPopup();
            //    }
            //}
            //else if (gameScene.level.limitType == LimitType.Time)
            //{
            //    var numCoins = PlayerPrefs.GetInt("num_coins");
            //    if (numCoins >= PuzzleMatchManager.instance.gameConfig.extraTimeCost)
            //    {
            //        PuzzleMatchManager.instance.coinsSystem.SpendCoins(PuzzleMatchManager.instance.gameConfig.extraTimeCost);
            //        coinParticles.Play();
            //        SoundManager.instance.PlaySound("CoinsPopButton");
            //        Close();
            //        gameScene.Continue();
            //    }
            //    else
            //    {
            //        SoundManager.instance.PlaySound("Button");
            //        OpenCoinsPopup();
            //    }
            //}

// mudit changes starts here----------------------------------------------------------------------------------
            // Instead of giving moves now ? show your confirmation panel
            extraMovesPanel.SetActive(true);
            current_panel_Panel.SetActive(false);

        }

        public void OnAddExtraMovesConfirmed()
        {
// this is the part where the lives is granted with the coin reduction -----------------------------------------------

            //int cost = PuzzleMatchManager.instance.gameConfig.extraMovesCost;

            //int numCoins = PlayerPrefs.GetInt("num_coins");

            //if (numCoins >= cost)
            //{
            //    // Spend coins
            //    PuzzleMatchManager.instance.coinsSystem.SpendCoins(cost);

            //    // Play effects
            //    coinParticles.Play();
            //    SoundManager.instance.PlaySound("CoinsPopButton");

            //    // Add +5 extra moves
            //    sfam.AddMoves(5);
            //    gameScene.gameUi.SetLimit(gameScene.gameBoard.currentLimit);

            //    // Close popup & resume game
            //    Close();
            //    gameScene.Continue();
            //}
            //else
            //{
            //    // Not enough coins ? open coins popup
            //    SoundManager.instance.PlaySound("Button");
            //    OpenCoinsPopup();
            //}


            // this is the part where the lives is granted without the coin reduction-----------------------------------------------------------

            // Play effects (optional – you can keep or remove)
            coinParticles.Play();
            SoundManager.instance.PlaySound("CoinsPopButton");

            // Add +5 extra moves (FREE)
            sfam.AddMoves(5);
            gameScene.gameUi.SetLimit(gameScene.gameBoard.currentLimit);

            // Close popup & resume game
            Close();
            gameScene.Continue();

        }

        //mudit changes endes here----------------------------------------------------------------------------------



        /// <summary>
        /// Called when the exit button is pressed.
        /// </summary>
        public void OnExitButtonPressed()
        {
            Close();
            gameScene.OpenLosePopup();
        }

        /// <summary>
        /// Opens the coins popup.
        /// </summary>
        private void OpenCoinsPopup()
        {
            var scene = parentScene as GameScene;
            if (scene != null)
            {
                scene.CloseCurrentPopup();
                scene.OpenPopup<BuyCoinsPopup>("Popups/BuyCoinsPopup",
                    popup =>
                    {
                        popup.onClose.AddListener(() =>
                        {
                            scene.OpenPopup<NoMovesOrTimePopup>("Popups/NoMovesOrTimePopup",
                                extraPopup =>
                                {
                                    extraPopup.SetGameScene(scene);
                                });
                        });
                    });
            }
        }
    }
}
