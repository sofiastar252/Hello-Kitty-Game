using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace FlappyBird.Web.Models
{
    public class GameManager
    {
        private int _gravity = 2;
        private int _score = 0;
        private PipeModel _lastScoredPipe;
        private int _highScore = 0; 

        private readonly IJSRuntime _jsRuntime;

        public event EventHandler MainLoopCompleted;
        public event EventHandler GameOverOccurred;

        public KittyModel Kitty { get; private set; }
        public List<PipeModel> Pipes { get; private set; }
        public bool IsRunning { get; private set; } = false;
        public int Score => _score;
        public int HighScore => _highScore;

        public GameManager(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            Kitty = new KittyModel();
            Pipes = new List<PipeModel>();
        }

        public async void MainLoop()
        {
            IsRunning = true;
            while (IsRunning)
            {
                MoveObjects();
                CheckForCollisions();
                ManagePipes();
                UpdateScore();

                MainLoopCompleted?.Invoke(this, EventArgs.Empty);
                await Task.Delay(20);
            }
        }

        public void StartGame()
        {
            if (!IsRunning)
            {
                Kitty = new KittyModel();
                Pipes = new List<PipeModel>();
                _score = 0;
                _lastScoredPipe = null;

                // Stop start screen music and play game music
                _ = _jsRuntime.InvokeVoidAsync("stopStartMusic");
                PlayBackgroundMusic();

                MainLoop();
            }
        }

        public void Jump()
        {
            if (IsRunning)
                Kitty.Jump();
        }

        void CheckForCollisions()
        {
            if (Kitty.IsOnGround())
            {
                GameOver();
            }

            var centeredPipe = Pipes.FirstOrDefault(p => p.IsCentered());

            if (centeredPipe != null)
            {
                bool hasCollidedWithBottom = Kitty.DistanceFromGround < centeredPipe.GapBottom - 150;
                bool hasCollidedWithTop = Kitty.DistanceFromGround + 45 > centeredPipe.GapTop - 150;

                if (hasCollidedWithBottom || hasCollidedWithTop)
                {
                    GameOver();
                }
            }
        }

        void ManagePipes()
        {
            if (!Pipes.Any() || Pipes.Last().DistanceFromLeft <= 250)
                Pipes.Add(new PipeModel());

            if (Pipes.First().IsOffScreen())
                Pipes.Remove(Pipes.First());
        }

        void MoveObjects()
        {
            Kitty.Fall(_gravity);
            foreach (var pipe in Pipes)
            {
                pipe.Move();
            }
        }

        void UpdateScore()
        {
            var nextPipe = Pipes.FirstOrDefault(p => p.IsCentered());
            if (nextPipe != null && _lastScoredPipe != nextPipe)
            {
                _score++;
                _lastScoredPipe = nextPipe;
                PlayCatchSound(); // Play catch sound when player scores
            }
        }

        public async void GameOver()
        {
            IsRunning = false;
            StopBackgroundMusic();

            // Update the high score
            if (_score > _highScore)
            {
                _highScore = _score;
            }

            await _jsRuntime.InvokeVoidAsync("playGameOverSound");
            GameOverOccurred?.Invoke(this, EventArgs.Empty);
        }

        public async void RestartGame()
        {
            await _jsRuntime.InvokeVoidAsync("stopGameOverSound");
            StartGame();
        }

        async void PlayCatchSound()
        {
            await _jsRuntime.InvokeVoidAsync("playCatchSound");
        }

        async void PlayErrorSound()
        {
            await _jsRuntime.InvokeVoidAsync("playErrorSound");
            await _jsRuntime.InvokeVoidAsync("playGameOverSound");
        }

        async void PlayBackgroundMusic()
        {
            await _jsRuntime.InvokeVoidAsync("playBackgroundMusic");
        }

        async void StopBackgroundMusic()
        {
            await _jsRuntime.InvokeVoidAsync("stopBackgroundMusic");
        }
    }
}
