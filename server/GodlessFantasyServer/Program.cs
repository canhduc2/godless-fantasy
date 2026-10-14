using System;
using System.Threading.Tasks;
using GodlessFantasyServer.Data;
using GodlessFantasyServer.Network;

namespace GodlessFantasyServer
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Title = "Godless Fantasy - Master World Server (.NET 8 Headless)";

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
   ______            ____               ______            __                  
  / ____/___  ____  / / /__  __________/ ____/___ _____  / /_____ ________  __
 / / __/ __ \/ __ \/ / / _ \/ ___/ ___/ /_  / __ `/ __ \/ __/ __ `/ ___/ / / /
/ /_/ / /_/ / /_/ / / /  __(__  |__  ) __/ / /_/ / / / / /_/ /_/ (__  ) /_/ / 
\____/\____/\____/_/_/\___/____/____/_/    \__,_/_/ /_/\__/\__,_/____/\__, /  
                                                                     /____/   
   MMORPG Dedicated World Server · Server-Authoritative · Multi-Universe
            ");
            Console.ResetColor();

            // 1. Khởi tạo Cơ sở Dữ liệu SQLite3 (Chuẩn WAL Mode không khóa)
            Console.WriteLine("[INIT] Initializing SQLite3 Database with WAL Mode...");
            await DatabaseManager.Instance.InitializeAsync();

            // 2. Khởi chạy WebSocket Gateway Server trên cổng 8088
            int port = 8088;
            if (args.Length > 0 && int.TryParse(args[0], out int customPort))
            {
                port = customPort;
            }

            var server = new WebSocketServer(port);
            var serverTask = server.StartAsync();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[READY] Server is fully operational on ws://localhost:{port}/ws/");
            Console.WriteLine("[READY] Supported Universes:");
            Console.WriteLine("        - Universe 1: Sơ Khai (Alpha Realm)");
            Console.WriteLine("        - Universe 2: Hỗn Độn (Chaos Realm)");
            Console.WriteLine("        - Universe 3: Thần Giới (Celestial Realm)");
            Console.WriteLine("[READY] Press Ctrl+C to safely shutdown.");
            Console.ResetColor();

            AppDomain.CurrentDomain.ProcessExit += (s, e) =>
            {
                Console.WriteLine("\n[SHUTDOWN] Stopping server...");
                server.Stop();
            };

            await serverTask;
        }
    }
}
