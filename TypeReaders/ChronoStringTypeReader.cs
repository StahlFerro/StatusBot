using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using StatusBot.Utility;

namespace StatusBot.TypeReaders
{
    public class ChronoStringTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            if (string.IsNullOrWhiteSpace(input))
                return Task.FromResult(TypeReaderResult.FromError(CommandError.BadArgCount, "Input is empty"));
            try
            {
                var chronostring = input.ParseChronoString();
                return Task.FromResult(TypeReaderResult.FromSuccess(chronostring));
            }
            catch (Exception ex)
            {
                return Task.FromResult(TypeReaderResult.FromError(CommandError.Exception,
                    $"[ChronoStringTypeReader ReadAsync Exception]\n{ex.Message}\n{ex.StackTrace}"));
            }
        }
    }
}
