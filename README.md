# StatusBot

StatusBot is a simple .NET Core discord bot that PM's a user if another bot goes offline. It comes with a very simple per-server configuration that determines what bot will be tracked for its status, and who will be PM'ed when the bot goes offline.

So far this bot is only used for testing and to assist on special cases only. It's not intended for public use as this bot is still experimental. But if you're interested in testing it, either visit it's development [discord server](https://discord.gg/GRBeCAX) and message me, or PM me on discord for further info.

For anyone trying to run their own local instance or self-hosting StatusBot by cloning this repo, it requires either:
1. An MS SQL Server database which is not included, but can be generated using [EF Core's PMC Tools](https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/powershell) on Visual Studio.
2. An SQLite Server database which should be named StatusBot.db in the Services folder.

Each of these database choices require the statements inside the OnConfiguring method on the [StatusBotContext.cs](https://github.com/StahlFerro/StatusBot/blob/master/StatusBot/Services/StatusBotContext.cs) file changed to the database provider of your choice.

Further questions regarding self-hosting StatusBot can be asked on it's development server
