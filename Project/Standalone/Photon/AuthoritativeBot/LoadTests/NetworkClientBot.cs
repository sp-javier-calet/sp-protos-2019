using SocialPoint.Network;
using System;
using System.Collections.Generic;
using SocialPoint.IO;

namespace SocialPoint.Multiplayer
{
    public class NetworkClientBot : INetworkMessageReceiver
    {
        bool _isConnected = false;
        string _gameName = null;

        Random _random = null;
        NetworkClientSceneController _clientController = null;
        GameMultiplayerClientBotLobbyBehaviour _clientLobby = null;

        public enum MatchTypeEnum
        {
            OneVsOne,
            ThreeVsThree
        }

        MatchTypeEnum _matchType;

        public NetworkClientBot(MatchTypeEnum matchType, NetworkClientSceneController clientController, string gameName)
        {
            _gameName = gameName;
            _matchType = matchType;

            _random = new Random();

            _clientController = clientController;
            _clientController.Restart(_clientController.Client);
            _clientController.RegisterReceiver(this);

            _clientLobby = new GameMultiplayerClientBotLobbyBehaviour(_clientController.Client, _clientController);
            _clientLobby.Restart(_clientController.Client);
            _clientLobby.LoadLevelCallback = LoadLevelCallback;
        }

        void LoadLevelCallback()
        {
            _clientLobby.SendLevelLoadedEvent();
            _clientController.ReceiveUpdateSceneEvents = false;
        }

        MultiplayerBattlePlayerData GeneratePlayerData()
        {
            var clientId = _clientController.Client.ClientId;
            var playerId = "player_" + clientId;
            var champions = GenerateChampions();
            var roomName = _gameName;
            var prefixRoom = MatchTypeEnum.OneVsOne == _matchType ? GameMultiplayerServerBehaviour.MatchIdPvPPrefixLoadTests : GameMultiplayerServerBehaviour.MatchId3v3PrefixLoadTests;
            var matchId = prefixRoom + roomName;

            // MultiplayerBattleUtils.CreateBattlePlayerData
            {
                var playerData = new MultiplayerBattlePlayerData();

                playerData.ClientId = clientId;
                playerData.Name = playerId;

                for (int heroIdx = 0; heroIdx < champions.Count; ++heroIdx)
                {
                    playerData.AddChampionData(champions[heroIdx]);
                }

                playerData.MatchId = matchId;
                playerData.PlayerToken = playerId;

                return playerData;
            }
        }

        struct ChampionInfo
        {
            public string id;
            public string type;
            public ChampionClass championClass;
            public string comboSkill;
            public string[] activeSkills;
            public string passiveSkill;
        }

        ChampionInfo[] championsInfo =
        {
            new ChampionInfo{ id="Ken", type="knight_nature", championClass=ChampionClass.WARRIOR, comboSkill="ComboSkill", activeSkills=new[]{"KenSkill_1", "KenSkill_2", "KenUltimate"}, passiveSkill="KenPassive" },
            new ChampionInfo{ id="Bestia", type="beast_fire", championClass=ChampionClass.WARRIOR, comboSkill="ComboSkill", activeSkills=new[]{"BeastSkill_1","BeastSkill_2","BeastUltimate"}, passiveSkill="BeastPassive" },
            new ChampionInfo{ id="Rondha", type="mage_water", championClass=ChampionClass.WARRIOR, comboSkill="ComboSkill", activeSkills=new[]{"RondhaSkill_1", "RondhaSkill_2", "RondhaSkill_ultimate"}, passiveSkill="RondhaPassive" },
            new ChampionInfo{ id="Gemma", type="rogue_fire", championClass=ChampionClass.WARRIOR, comboSkill="ComboSkill", activeSkills=new[]{"GemmaSkill_1", "GemmaSkill_2", "GemmaUltimate"}, passiveSkill="GemmaDeadlyShot" },
            new ChampionInfo{ id="Pris", type="rogue_nature", championClass=ChampionClass.WARRIOR, comboSkill="ComboSkill", activeSkills=new[]{"PrisSkill_1", "PrisSkill_2", "PrisSkill_ultimate"}, passiveSkill="PrisPassive" },
            new ChampionInfo{ id="Alaxtar", type="pet_wind", championClass=ChampionClass.WARRIOR, comboSkill="ComboSkill", activeSkills=new[]{"AlaxtarSkill_1", "AlaxtarSkill_2", "AlaxtarUltimate"}, passiveSkill="AlaxtarPassive" },
            new ChampionInfo{ id="Lindsey", type="assassin_fire", championClass=ChampionClass.WARRIOR, comboSkill="ComboSkill", activeSkills=new[]{"LindseySkill_1", "LindseySkill_2", "LindseyUltimate"}, passiveSkill="LindseyPassive" },
            new ChampionInfo{ id="Tenseed", type="golem_nature", championClass=ChampionClass.WARRIOR, comboSkill="ComboSkill", activeSkills=new[]{"TenseedSkill_1", "TenseedSkill_2", "TenseedSkill_ultimate"}, passiveSkill="TenseedPassive" },
        };

        List<MultiplayerBattleChampionData> GenerateChampions()
        {
            var champions = new List<MultiplayerBattleChampionData>();
            var indicesGenerated = new List<int>();
            int championIndex = 0;
            int nChampions = 3;

            for (int i = 0; i < nChampions; ++i)
            {
                do
                {
                    championIndex = _random.Next(championsInfo.Length);
                }
                while (indicesGenerated.Contains(championIndex));

                indicesGenerated.Add(championIndex);
                champions.Add(GenerateChampion(championIndex, i));
            }

            return champions;
        }
        
        MultiplayerBattleChampionData GenerateChampion(int championIndex, int championIndexSlot)
        {
            ChampionInfo champion = championsInfo[championIndex];

            var multiplayerBattleChampionData = new MultiplayerBattleChampionData();
            multiplayerBattleChampionData.ChampionId = champion.id;
            multiplayerBattleChampionData.ChampionType = champion.type;
            multiplayerBattleChampionData.ChampionClass = champion.championClass;
            multiplayerBattleChampionData.ChampionIndex = championIndexSlot;
            multiplayerBattleChampionData.PowerIndex = 1;

            multiplayerBattleChampionData.ComboSkill = new MultiplayerBattleSkillData(); ;

            // Always using combo skill.
            multiplayerBattleChampionData.ComboSkill.Id = champion.comboSkill;
            multiplayerBattleChampionData.ComboSkill.Level = 1;
            multiplayerBattleChampionData.ComboSkill.Slot = 0;

            int nextSkillSlot = 1;
            if (champion.activeSkills != null)
            {
                for (int skillIndex = 0; skillIndex < champion.activeSkills.Length; ++skillIndex)
                {
                    bool useActiveSkill = _random.Next(100) >= 50;
                    if (useActiveSkill)
                    {
                        var activeSkill = new MultiplayerBattleSkillData();
                        activeSkill.Id = champion.activeSkills[skillIndex];
                        activeSkill.Level = 1;
                        activeSkill.Slot = nextSkillSlot++;

                        multiplayerBattleChampionData.ActiveSkills.Add(activeSkill);
                    }
                }
            }

            if (!string.IsNullOrEmpty(champion.passiveSkill))
            {
                bool usePassiveSkill = _random.Next(100) >= 50;
                if (usePassiveSkill)
                {
                    var passiveSkill = new MultiplayerBattleSkillData();
                    passiveSkill.Id = champion.passiveSkill;
                    passiveSkill.Level = 1;
                    passiveSkill.Slot = nextSkillSlot++;

                    multiplayerBattleChampionData.PassiveSkills.Add(passiveSkill);
                }
            }

            return multiplayerBattleChampionData;
        }

        #region INetworkMessageReceiver

        public void OnMessageReceived(NetworkMessageData data, IReader reader)
        {
            switch (data.MessageType)
            {
                case SceneMsgType.ConnectEvent:
                    if(!_isConnected)
                    {
                        var playerData = GeneratePlayerData();
                        _clientLobby.SendJoinRequest(playerData);
                        _isConnected = true;
                    }
                    break;

                case GameMsgType.PlayerJoinAnswerEvent:
                    var eventData = new PlayerJoinAnswerEvent();
                    eventData.Deserialize(reader);
                    _clientLobby.OnPlayerJoinAnswer(eventData);
                    break;
            }
        }

        #endregion
    }
}
