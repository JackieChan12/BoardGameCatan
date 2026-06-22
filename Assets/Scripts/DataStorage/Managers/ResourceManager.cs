using System.Collections.Generic;
using Assets.Scripts.Board.States;
using Board;
using DataStorage;
using System.Linq;
using static Assets.Scripts.Board.States.JunctionState;
using static Player.Resources;
using static Assets.Scripts.Board.States.FieldState;

namespace Assets.Scripts.DataStorage.Managers
{
    public struct IncomingResourceRequest
    {
        public IncomingResourceRequest(FieldType t, int n, int pId = -1)
        {
            type = t;
            number = n;
            playerId = pId;
        }

        public FieldType type { get; }

        public int number { get; }

        public int playerId { get; }
    }

    public class ResourceManager
    {
        //Destiny: Maximum values of player's elements
        public const int MaxResourcesNumber = 19;
        public const int MaxResourceNumberWhenTheft = 7;
        
        public List<IncomingResourceRequest> IncomingResourcesRequests = new();

        /// <summary>
        /// Updates resources for each player who has the junction adjacent to the field with thrown number
        /// </summary>
        public void UpdatePlayersResources()
        {
            Dictionary<int, Dictionary<ResourceType, int>> resourcesToAddToPlayer = new Dictionary<int, Dictionary<ResourceType, int>>();
            foreach (var player in GameManager.State.Players)
            {
                resourcesToAddToPlayer.Add(player.index, GetEmptyResourceDictionary());
            }

            //Destiny: for each field with thrown number
            foreach (FieldElement field in 
                BoardManager.Fields.Where(f => ((FieldState)f.State).number == GameManager.State.CurrentDiceThrownNumber))
            {
                //Destiny: for each junction adjacent to this field
                field.junctionsID.ForEach(delegate (int fieldJunctionId) 
                {
                    //Destiny: If thief is not over this field
                    if (!((FieldState)field.State).isThief)
                    {
                        field.ParticleAnimation();

                        //Destiny: for each player
                        foreach (var player in GameManager.State.Players)
                        {
                            //Destiny: if player owns adjacent junction then add proper number of resources
                            if (player.OwnsBuilding(fieldJunctionId))
                            {
                                int resourceNumber = ((JunctionState)BoardManager.Junctions[fieldJunctionId].State).type 
                                    == JunctionType.Village ? 1 : 2;

                                if (CheckIfResourceExists(field.GetResourceType(), resourceNumber))
                                {
                                    resourcesToAddToPlayer[player.index][field.GetResourceType()] += resourceNumber;
                                    string resourceString = resourceNumber == 1 ? "1 đơn vị" : "2 đơn vị";
                                    string resourceType = GetResourceName(field.GetResourceType());
                                    GameManager.Logs.Add(
                                        $"{player.name} nhận được {resourceString} tài nguyên loại {resourceType}");
                                }
                                else if (CheckIfResourceExists(field.GetResourceType()))
                                {
                                    resourcesToAddToPlayer[player.index][field.GetResourceType()] += 1;
                                    string resourceType = GetResourceName(field.GetResourceType());
                                    GameManager.Logs.Add(
                                        $"{player.name} nhận được 1 đơn vị tài nguyên loại {resourceType}");
                                }
                            }
                        }
                    }
                });
            }

            foreach (var player in GameManager.State.Players)
            {
                player.resources.AddResources(resourcesToAddToPlayer[player.index]);
            }
        }

        /// <summary>
        /// Adds resources from fields around given junction
        /// </summary>
        /// <param name="junctionId"></param>
        /// <param name="playerId"></param>
        public void AddResourcesInitialDistribution(int junctionId, int playerId)
        {
            var resources = GetEmptyResourceDictionary();

            BoardManager.Junctions[junctionId].fieldsID.ForEach(delegate (int fieldId)
            {
                var resourceType = BoardManager.Fields[fieldId].GetResourceType();
                if (resourceType != ResourceType.None)
                {
                    resources[resourceType] += 1;
                }
            });

            GameManager.State.Players[playerId].resources.AddResources(resources);
        }

        /// <summary>
        /// Subtracts resources from fields around given junction
        /// </summary>
        /// <param name="junctionId"></param>
        /// <param name="playerId"></param>
        public void SubtractResourcesInitialDistribution(int junctionId, int playerId)
        {
            var resources = GetEmptyResourceDictionary();

            BoardManager.Junctions[junctionId].fieldsID.ForEach(delegate (int fieldId)
            {
                var resourceType = BoardManager.Fields[fieldId].GetResourceType();
                if (resourceType != ResourceType.None)
                {
                    resources[resourceType] += 1;
                }
            });

            GameManager.State.Players[playerId].resources.SubtractResources(resources);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resource">type of resource</param>
        /// <param name="neddedValue">number of resources needed of given type</param>
        /// <returns>true if resource exists in bank (players have less than 19 cards in total)</returns>
        public bool CheckIfResourceExists(ResourceType resource, int neddedValue = 1)
        {
            return CountPlayersResources(resource) + neddedValue <= MaxResourcesNumber;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceType"></param>
        /// <returns>The number of all resources of given type belonging to all players</returns>
        public static int CountPlayersResources(ResourceType resourceType)
        {
            int result = 0;

            foreach (var player in GameManager.State.Players)
            {
                result += player.resources.GetResourceNumber(resourceType);
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceType">type of resource</param>
        /// <returns>name of resource as string</returns>
        public string GetResourceName(ResourceType resourceType)
        {
            return resourceType switch
            {
                ResourceType.Wood => "gỗ",
                ResourceType.Clay => "gạch",
                ResourceType.Wool => "lông cừu",
                ResourceType.Iron => "quặng sắt",
                ResourceType.Wheat => "lúa mì",
                _ => ""
            };
        }

        public FieldType GetFieldType(ResourceType resourceType)
        {
            return resourceType switch
            {
                ResourceType.Wood => FieldType.Forest,
                ResourceType.Clay => FieldType.Hills,
                ResourceType.Wool => FieldType.Pasture,
                ResourceType.Iron => FieldType.Mountains,
                ResourceType.Wheat => FieldType.Field,
                _ => FieldType.Desert
            };
        }

        private Dictionary<ResourceType, int> GetEmptyResourceDictionary()
        {
            Dictionary<ResourceType, int> dict = new Dictionary<ResourceType, int>();

            dict.Add(ResourceType.Wood, 0);
            dict.Add(ResourceType.Clay, 0);
            dict.Add(ResourceType.Iron, 0);
            dict.Add(ResourceType.Wool, 0);
            dict.Add(ResourceType.Wheat, 0);

            return dict;
        }
    }
}
