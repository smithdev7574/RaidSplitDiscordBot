using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaidRobot.Infrastructure
{
    public partial class Commands
    {
        [Command("CreatePreSplit", RunMode = RunMode.Async)]
        public async Task CreatePreSplit(string name, string leader, string looter, string inviter)
        {
            try
            {
                var permissionResult = permissionChecker.CheckManagerPermissions(Context, "CreatePreSplit");
                if (!permissionResult.HasPremission)
                {
                    await ReplyAsync(permissionResult.Message);
                    return;
                }

                var result = preSplitOrchestrator.CreatePreSplit(name, leader, looter, inviter);
                await ReplyAsync(result);
            }
            catch (Exception ex)
            {
                await ReplyAsync($"Error: {ex.Message}");
            }
        }

        [Command("SetPreSplitRole", RunMode = RunMode.Async)]
        public async Task SetPreSplitRole(string name, string characterName, string role)
        {
            try
            {
                var permissionResult = permissionChecker.CheckManagerPermissions(Context, "SetPreSplitRole");
                if (!permissionResult.HasPremission)
                {
                    await ReplyAsync(permissionResult.Message);
                    return;
                }

                if (!Enum.TryParse<RaidResponsibilities>(role, out var responsibility))
                {
                    await ReplyAsync($"Role must be Leader, Looter, or Inviter.  Proper Format !rr SetPreSplitRole PreSplit1 LilMeech Leader");
                    return;
                }

                var result = preSplitOrchestrator.SetRole(name, characterName, responsibility);
                await ReplyAsync(result);
            }
            catch (Exception ex)
            {
                await ReplyAsync($"Error: {ex.Message}");
            }
        }

        [Command("RemovePreSplit", RunMode = RunMode.Async)]
        public async Task RemovePreSplit(string name)
        {
            try
            {
                var permissionResult = permissionChecker.CheckManagerPermissions(Context, "RemovePreSplit");
                if (!permissionResult.HasPremission)
                {
                    await ReplyAsync(permissionResult.Message);
                    return;
                }

                var result = preSplitOrchestrator.RemovePreSplit(name);
                await ReplyAsync(result);
            }
            catch (Exception ex)
            {
                await ReplyAsync($"Error: {ex.Message}");
            }
        }

        [Command("AddPreSplitCharacters", RunMode = RunMode.Async)]
        public async Task AddPreSplitCharacters(string name, [Remainder] string characterNames)
        {
            try
            {
                var permissionResult = permissionChecker.CheckManagerPermissions(Context, "AddPreSplitCharacters");
                if (!permissionResult.HasPremission)
                {
                    await ReplyAsync(permissionResult.Message);
                    return;
                }

                var result = preSplitOrchestrator.AddCharacters(name, characterNames);
                await ReplyAsync(result);
            }
            catch (Exception ex)
            {
                await ReplyAsync($"Error: {ex.Message}");
            }
        }

        [Command("RemovePreSplitCharacters", RunMode = RunMode.Async)]
        public async Task RemovePreSplitCharacters(string name, [Remainder] string characterNames)
        {
            try
            {
                var permissionResult = permissionChecker.CheckManagerPermissions(Context, "RemovePreSplitCharacters");
                if (!permissionResult.HasPremission)
                {
                    await ReplyAsync(permissionResult.Message);
                    return;
                }

                var result = preSplitOrchestrator.RemoveCharacters(name, characterNames);
                await ReplyAsync(result);

            }
            catch (Exception ex)
            {
                await ReplyAsync($"Error: {ex.Message}");
            }
        }

        [Command("ViewPreSplits", RunMode = RunMode.Async)]
        public async Task ViewPreSplits()
        {
            try
            {
                var permissionResult = permissionChecker.CheckManagerPermissions(Context, "ViewPreSplits");
                if (!permissionResult.HasPremission)
                {
                    await ReplyAsync(permissionResult.Message);
                    return;
                }

                var result = preSplitOrchestrator.ViewPreSplits();
                await ReplyAsync(result);
            }
            catch (Exception ex)
            {
                await ReplyAsync($"Error: {ex.Message}");
            }
        }
    }
}
