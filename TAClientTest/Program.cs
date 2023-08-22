﻿using TAClientTest.Interop;
using TournamentAssistantShared;

namespace TAClientTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TryConnectToTA();
            Console.ReadLine();
        }

        public static async Task TryConnectToTA()
        {
            var client = new TAClient("server.tournamentassistant.net", 2052);
            client.SetAuthToken(TAAuthLibraryWrapper.GetToken("test", "349857"));

            var result = await client.Connect();

            Console.WriteLine(result.ToString());
        }
    }
}