using System.Collections.Generic;
using System.Linq;
using Board;
using DataStorage;
using Player;

namespace Assets.Scripts.DataStorage.Managers
{
    public class LongestPathManager
    {
        //Destiny: Minimum values to get reward
        public const int RewardedLongestPathLength = 5;

        /// <summary>
        /// Updates points for longest path
        /// </summary>
        /// <param name="playersWithLongestPathToCompare">Dictionary with player (who has the longest path) id as key and path length as value. 
        /// You can use it to compare if point state before deleting object is the same as after this action. Default value is null.</param>
        public void CheckLongestPath(Dictionary<int, int> playersWithLongestPathToCompare = null)
        {
            //Destiny: Get all players with the longest path
            Dictionary<int, int> longestPathPlayerIds = FindPlayerIdsWithLongestPath();
            int playerIdWithAwardedLongestPath = GetPlayerIdWithAwardedLongestPath();

            //Destiny: if checking longest path is after removing element and if current player wasn't awarded at the begging of his move
            if ((playersWithLongestPathToCompare != null) && (GameManager.State.CurrentPlayerId != GameManager.State.PlayerIdWithAwardedPathAtBegining))
            {
                //Destiny: if current player after deleting element has the longest path
                // and before removing element he had the longest path too
                // and now the player that had reward at the begging of move of current player has the longest path
                if (longestPathPlayerIds.ContainsKey(GameManager.State.CurrentPlayerId) &&
                    playersWithLongestPathToCompare.ContainsKey(GameManager.State.CurrentPlayerId) &&
                    longestPathPlayerIds.ContainsKey(GameManager.State.PlayerIdWithAwardedPathAtBegining))
                {
                    //Destiny: current player gives additional points back to the right player
                    GameManager.State.Players[GameManager.State.CurrentPlayerId].score.RemovePoints(Score.PointType.LongestPath);
                    GameManager.Logs.Add($"Người chơi {GameManager.State.Players[GameManager.State.CurrentPlayerId].name} đã hoàn tác nước đi " +
                            ", vì vậy mất 2 điểm thưởng từ đường đi dài nhất");

                    if (GameManager.State.PlayerIdWithAwardedPathAtBegining >= 0 &&
                        GameManager.State.PlayerIdWithAwardedPathAtBegining < GameManager.State.Players.Length)
                    {
                        GameManager.State.Players[GameManager.State.PlayerIdWithAwardedPathAtBegining].score.AddPoints(Score.PointType.LongestPath);
                        GameManager.Logs.Add($"Người chơi {GameManager.State.Players[GameManager.State.PlayerIdWithAwardedPathAtBegining].name} " +
                                "nhận lại 2 điểm thưởng từ đường đi dài nhất");
                    }

                    return;
                }
                //Destiny: if before removing element current player had the longest path
                // and now he has not the longest path
                else if (playersWithLongestPathToCompare.ContainsKey(GameManager.State.CurrentPlayerId) &&
                    !longestPathPlayerIds.ContainsKey(GameManager.State.CurrentPlayerId))
                {
                    //Destiny: give back points to proper player
                    GameManager.State.Players[GameManager.State.CurrentPlayerId].score.RemovePoints(Score.PointType.LongestPath);
                    GameManager.Logs.Add($"Người chơi {GameManager.State.Players[GameManager.State.CurrentPlayerId].name} đã hoàn tác nước đi " +
                            ", vì vậy mất 2 điểm thưởng từ đường đi dài nhất");

                    if (GameManager.State.PlayerIdWithAwardedPathAtBegining >=0 && 
                        GameManager.State.PlayerIdWithAwardedPathAtBegining < GameManager.State.Players.Length)
                    {
                        GameManager.State.Players[GameManager.State.PlayerIdWithAwardedPathAtBegining].score.AddPoints(Score.PointType.LongestPath);
                        GameManager.Logs.Add($"Người chơi {GameManager.State.Players[GameManager.State.PlayerIdWithAwardedPathAtBegining].name} " +
                                "nhận lại 2 điểm thưởng từ đường đi dài nhất");
                    }

                    return;
                }
            }

            //Destiny: if actual longest path should be rewarded - length is at least 5
            if (longestPathPlayerIds.Values.Any() && longestPathPlayerIds.Values.First() >= RewardedLongestPathLength)
            {
                //Destiny: if one player already has reward
                if (playerIdWithAwardedLongestPath < GameManager.State.Players.Length)
                {
                    //Destiny: if player with awarded longest path still has longest path then end the function
                    if (longestPathPlayerIds.Keys.Contains(playerIdWithAwardedLongestPath))
                    {
                        return;
                    }

                    //Destiny: else clear his points
                    GameManager.State.Players[playerIdWithAwardedLongestPath].score.RemovePoints(Score.PointType.LongestPath);
                    
                    //Destiny: give points to proper player if he's the only player who has the longest path
                    if (longestPathPlayerIds.Count() == 1)
                    {
                        GameManager.State.Players[longestPathPlayerIds.Keys.First()].score.AddPoints(Score.PointType.LongestPath);
                        GameManager.Logs.Add($"{GameManager.State.Players[longestPathPlayerIds.Keys.First()].name} " +
                            $"đã xây đường đi dài nhất và lấy 2 điểm từ " +
                            $"{GameManager.State.Players[playerIdWithAwardedLongestPath].name}");
                    }
                    else
                    {
                        GameManager.Logs.Add($"{GameManager.State.Players[playerIdWithAwardedLongestPath].name} " +
                            "mất 2 điểm vì đường đi của họ không còn dài nhất");
                    }
                }
                //Destiny: if no one has reward and now is one player with the longest path
                else if (longestPathPlayerIds.Count() == 1)
                {
                    GameManager.State.Players[longestPathPlayerIds.Keys.First()].score.AddPoints(Score.PointType.LongestPath);
                    GameManager.Logs.Add($"{GameManager.State.Players[longestPathPlayerIds.Keys.First()].name} " +
                        "giành được 2 điểm nhờ xây đường đi dài nhất");
                }
            }
            //Destiny: if actual longest path shouldn't be rewarded - length is below 5 than remove points from the proper player
            else if (playerIdWithAwardedLongestPath < GameManager.State.Players.Length)
            {
                GameManager.State.Players[playerIdWithAwardedLongestPath].score.RemovePoints(Score.PointType.LongestPath);
                GameManager.Logs.Add($"Đường đi dài nhất của {GameManager.State.Players[playerIdWithAwardedLongestPath].name} " +
                        "đã bị phá vỡ, vì vậy họ mất 2 điểm");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Id of a player who now has points for the longest path</returns>
        public int GetPlayerIdWithAwardedLongestPath()
        {
            foreach (var player in GameManager.State.Players)
            {
                if (player.score.GetPoints(Score.PointType.LongestPath) != 0)
                {
                    return player.index;
                }
            }

            return GameManager.State.Players.Length;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Dictionary of players ids who have the longest path and the values of their longest path</returns>
        public Dictionary<int, int> FindPlayerIdsWithLongestPath()
        {
            Dictionary<int, int> playersLongestPath = new();
            List<int> longestPathIds;
            List<int> junctionIds; ;
            int longestPathLength;
            int tempLongestPathLength;

            //Destiny: for each player count the length of the longest path
            foreach (var player in GameManager.State.Players)
            {
                longestPathLength = 0;
                tempLongestPathLength = 0;

                foreach (int edgePathId in player.properties.paths)
                {
                    longestPathIds = new();
                    junctionIds = new();
                    longestPathIds.Add(edgePathId);

                    tempLongestPathLength = FindLongestPath(player.index, longestPathIds, junctionIds);
                    if (longestPathLength == 0 || longestPathLength < tempLongestPathLength)
                    {
                        longestPathLength = tempLongestPathLength;
                    }
                }

                playersLongestPath[player.index] = longestPathLength;
            }

            //Destiny: Find player with the longest path
            Dictionary<int, int> result = new();

            //Destiny: Guard against empty dictionary (no players have any paths)
            if (playersLongestPath.Count == 0)
                return result;

            longestPathLength = playersLongestPath.Values.Max();
            foreach (var playerLongestPath in playersLongestPath)
            {
                if (playerLongestPath.Value == longestPathLength)
                {
                    result.Add(playerLongestPath.Key, playerLongestPath.Value);
                }
            }
            return result;
        }

        /// <summary>
        /// Recursive function that finds the longest path and returns its length
        /// </summary>
        /// <param name="playerId">Id of the player for whom the function searches for his longest path</param>
        /// <param name="pathIds">The way so far consisting of path ids</param>
        /// <param name="junctionIds">The way so far consisting of junction ids</param>
        /// <returns>Length of the longest path belonging to given player</returns>
        private int FindLongestPath(int playerId, List<int> pathIds, List<int> junctionIds)
        {
            int longestPathLength = pathIds.Count();

            //Destiny: for each adjacent path to the last path in the longest path so far
            foreach (int adjacentPath in BoardManager.Paths[pathIds.Last()].pathsID)
            {
                int commonJunctionId = BoardManager.Paths[pathIds.Last()].FindCommonJunction(adjacentPath);

                //Destiny: if we haven't yet get through this junction and the junction is neutral or
                //belongs to given player then keep counting
                if (!junctionIds.Contains(commonJunctionId) &&
                    (BoardManager.Junctions[commonJunctionId].GetOwnerId() == GameManager.State.Players.Count() ||
                    BoardManager.Junctions[commonJunctionId].GetOwnerId() == playerId))
                {
                    junctionIds.Add(commonJunctionId);

                    //Destiny: if adjacent path belongs to the player and it is not in current path already then add it
                    if (BoardManager.Paths[adjacentPath].GetOwnerId() == playerId && !pathIds.Contains(adjacentPath))
                    {
                        pathIds.Add(adjacentPath);

                        int tempLongestPathLength = FindLongestPath(playerId, pathIds, junctionIds);
                        if (longestPathLength == 0 || longestPathLength < tempLongestPathLength)
                        {
                            longestPathLength = tempLongestPathLength;
                        }

                        pathIds.Remove(pathIds.Last());
                        junctionIds.Remove(junctionIds.Last());
                    }
                    else
                    {
                        junctionIds.Remove(junctionIds.Last());
                    }
                }
            }

            return longestPathLength;
        }
    }
}
