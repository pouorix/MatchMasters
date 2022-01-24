using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Medrick.Match3CoreSystem.Game;
using Nakama;
using Nakama.TinyJson;
using Sample;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Script.CoreGame
{
    [System.Serializable]
    public class sendMessageInfo
    {
        public string opcode;
        public string sourcePosition;
        public string targetPosition;

        public static sendMessageInfo CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<sendMessageInfo>(jsonString);
        }
    }

    public class socketLogic : spawnGems
    {
        private static ISocket mySocket;
        private static String myGameMatchicket;

        private async void Start()
        {
            mySocket = MatchMakingLogic.socket;
            myGameMatchicket = MatchMakingLogic.gameMatchicket;

            // Use whatever decoder for your message contents.
            var enc = System.Text.Encoding.UTF8;
            mySocket.ReceivedMatchState += newState =>
            {
                // var content = newState.State.ToString();
                string content = enc.GetString(newState.State).ToString();
                var jsonContent = sendMessageInfo.CreateFromJSON(content);

                Vector2Int sourcePosiiton = new Vector2Int(
                    Int32.Parse(jsonContent.sourcePosition.Split(',')[0].Remove(0, 1)),
                    Int32.Parse(jsonContent.sourcePosition.Split(',')[1].Remove(0, 1).Substring(0, 1)));

                Vector2Int targetPosition = new Vector2Int(
                    Int32.Parse(jsonContent.targetPosition.Split(',')[0].Remove(0, 1)),
                    Int32.Parse(jsonContent.targetPosition.Split(',')[1].Remove(0, 1).Substring(0, 1)));

                //opCodes : 
                //1 : swap
                //2 : Hammer perk
                //3 : Shuffle perk
                //4 : Booster
                //5 : Exit
                switch (jsonContent.opcode)
                {
                    case "1":
                        gameplayController.FrameBasedBlackBoard.GetComponent<SwapBlackBoard>().requestedSwaps
                            .Add(new SwapBlackBoard.SwapData(
                                sourcePosiiton, targetPosition,
                                true));
                        break;
                }
            };
        }

        public async void OnBackBtnClick()
        {
            var matchId = myGameMatchicket;

            await mySocket.LeaveMatchAsync(matchId);
            Debug.Log("Quited");

            // SceneManager.LoadScene("MainApp");
        }

        public async void OnReconnectBtnClick()
        {
            var matchId = myGameMatchicket;

            await mySocket.JoinMatchAsync(matchId);
            Debug.Log("Rejoined");

            // SceneManager.LoadScene("MainApp");
        }

        public static void sendChat(string opCode, string sourcePosition, string targetPosition)
        {
            var newState = new Dictionary<string, string>
                {{"opcode", opCode}, {"sourcePosition", sourcePosition}, {"targetPosition", targetPosition}}.ToJson();
            mySocket.SendMatchStateAsync(myGameMatchicket, 1, newState);
        }
    }
}