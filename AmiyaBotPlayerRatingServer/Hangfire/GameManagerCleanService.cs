﻿using AmiyaBotPlayerRatingServer.Data;
using AmiyaBotPlayerRatingServer.GameLogic;
using Microsoft.EntityFrameworkCore;

namespace AmiyaBotPlayerRatingServer.Hangfire
{
    public class GameManagerCleanService
    {
        private readonly GameManager _gameManager;
        private readonly PlayerRatingDatabaseContext _dbContext;
        
        public GameManagerCleanService(GameManager gameManager,PlayerRatingDatabaseContext dbContext)
        {
            _gameManager = gameManager;
            _dbContext = dbContext;
        }

        public async Task Clean()
        {
            var gameInfos = await _dbContext.GameInfos.Where(g => g.IsClosed == false).ToListAsync();
            foreach (var info in gameInfos)
            {
                var game =await _gameManager.GetGameAsync(info.Id);

                if (game == null)
                {
                    continue;
                }

                if (game.IsClosed)
                {
                    continue;
                }

                if ((game.IsCompleted && (DateTime.Now - game.CompleteTime > new TimeSpan(0, 1, 0, 0)))
                    || (game.IsStarted && (DateTime.Now - game.StartTime > new TimeSpan(1, 0, 0, 0))))
                {
                    await using var depGame = await _gameManager.GetGameAsync(info.Id,false);
                    if (depGame != null)
                    {
                        depGame.IsClosed = true;
                        depGame.CloseTime = DateTime.Now;
                        await _gameManager.SaveGameAsync(depGame);
                    }
                }
            }
        }
    }
}