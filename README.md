# StatusBot

StatusBot is a simple .NET Core discord bot that PM's a user if another bot goes offline. It comes with a very simple per-server configuration that determines what bot will be tracked for its status, and who will be PM'ed when the bot goes offline.

So far this bot is only used for testing and to assist on special cases only. It's not intended for public use as this bot is still experimental. But if you're interested in testing it, either visit it's development [discord server](https://discord.gg/HPpxujb) and message me, or PM me on discord for further info.

## Database
For anyone trying to run their own local instance or self-hosting StatusBot, it requires An SQLite database which should be named StatusBot.db in the Database folder. (For Linux, Windows and Pi, more preferred, portable)
Although SQLite is the best choice for StatusBot, you are not restricted to other database providers but they might restrict you from compatible hosting platforms. If you want to change do not forget to:
1. Add the respective database provider of your choice. [List of providers](https://docs.microsoft.com/en-us/ef/core/providers/)
1. Change the statement inside the OnConfiguring method on the [StatusBotContext.cs](https://github.com/StahlFerro/StatusBot/blob/master/StatusBot/Services/StatusBotContext.cs) statement to your chosen provider

## Hosting on Raspberry Pi (*new in 2.1.5!*)
Publishing StatusBot with dotnet publish is pretty straight forward:
1. Use the following command to publish the app for your Pi: `dotnet publish -c Release -r linux-arm`
2. Copy the entire `publish` folder inside the generated `linux-arm` folder into your Pi
3. Execute `chmod 755 StatusBot` and then run it! (`./StatusBot`)

Further questions regarding self-hosting StatusBot can be asked on it's development server
