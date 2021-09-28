using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace QuoteBot
{
	class CommandHandler
	{
		readonly Emoji thumbs_up = new Emoji("👍");
		public static void Main(string[] args)
		 => new CommandHandler().MainAsync().GetAwaiter().GetResult();

		private DiscordSocketClient _client;
		public async Task MainAsync()
		{
			Console.BackgroundColor = ConsoleColor.Black;
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("Running QuoteBot.exe");
			Console.WriteLine();
			Console.WriteLine($@"{DateTime.Now.ToString().Substring(11)} Discord     Initializing");
			Console.WriteLine();
			Console.ForegroundColor = ConsoleColor.Green;
			_client = new DiscordSocketClient();
			_client.MessageReceived += CommandHandlerFunc;
			_client.Log += LoginPrint;
			string token = File.ReadAllText("TOKEN.txt");
			await _client.LoginAsync(TokenType.Bot, token);
			await _client.StartAsync();
			await _client.SetActivityAsync(new Game(File.ReadAllText("PREFIX.txt")[0] + "help",
				ActivityType.Listening, ActivityProperties.None));
			await Task.Delay(-1);
		}

		private Task LoginPrint(LogMessage msg)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(msg.ToString());
			Console.WriteLine();
			Console.ForegroundColor = ConsoleColor.Green;
			return Task.CompletedTask;
		}

		private Task LogCommands(SocketMessage message, string command)
		{
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine($@"NEW MESSAGE (WITH {_client.CurrentUser.Username.ToString()})"
				+ $@" FROM {message.Author} USING {command.ToUpper()}"
				+ $@" AT {message.Timestamp.AddHours(3).ToString().Substring(0, 19)}");
			Console.WriteLine();
			Console.ForegroundColor = ConsoleColor.Green;
			message.AddReactionAsync(thumbs_up);
			return Task.CompletedTask;
		}

		private Task CommandHandlerFunc(SocketMessage message)
		{
			if (message.Author.IsBot) return Task.CompletedTask;
			char prefix = File.ReadAllText("PREFIX.txt")[0];
			if (!message.Content.StartsWith(prefix)) return Task.CompletedTask;
			int lengthOfCommand = message.Content.Length;
			if (message.Content.Contains(' ')) lengthOfCommand = message.Content.IndexOf(' ');
			string command = message.Content.Substring(1, lengthOfCommand - 1).ToLower();

			switch (command)
			{
				case "hello":
					LogCommands(message, command);
					message.Channel.SendMessageAsync($@"Hello {message.Author.Mention}");
					break;

				case "help":
					LogCommands(message, command);
					message.Channel.SendMessageAsync
				($@"THIS IS THE HELP SCREEN. CONSIDER YOURSELF HELPED

				List of **public** commands (last updated 01-08-2021):

				**{prefix}hello** is a friendly greeting
				**{prefix}help** gets you this screen
				**{prefix}quote** or **{prefix}q** sends a random quote
				**{prefix}ping** or **{prefix}latency** sends the current latency for the bot

				Source code found here: **https://github.com/Stormageddon37/QuotesBot**
				Current latency (ping): **{_client.Latency.ToString()} ms**.
				");
					break;

				case "quote":
				case "q":
					LogCommands(message, command);
					String URL = "https://api.quotable.io/random";
					StreamReader sr = new StreamReader(HttpWebRequest.Create(URL).GetResponse().GetResponseStream());
					string QUOTE_JSON = sr.ReadToEnd();
					Quote quoteObject = JsonConvert.DeserializeObject<Quote>(QUOTE_JSON);
					message.Channel.SendMessageAsync(quoteObject.Content.ToString() +
						" - " + quoteObject.Author.ToString());

					break;

				case "ping":
				case "latency":
					LogCommands(message, command);
					message.Channel.SendMessageAsync("ping is " + _client.Latency.ToString() + " ms");
					break;

				case "restart":
				case "reboot":
				case "r":
					LogCommands(message, command);
					string ADMIN_ID = File.ReadAllText("ADMIN.txt");
					if (!message.Author.Id.ToString().Equals(ADMIN_ID))
					{
						message.Channel.SendMessageAsync("You do not have permission to use this command");
						return Task.CompletedTask;
					}
					message.Channel.SendMessageAsync("Restarting Quote Bot...");
					Process.Start("QuoteBot.bat");
					Thread.Sleep(500);
					_client.StopAsync();
					break;
			}

			return Task.CompletedTask;
		}

		public partial class Quote
		{
			[JsonProperty("content")]
			public string Content { get; set; }

			[JsonProperty("author")]
			public string Author { get; set; }
		}
	}
}
