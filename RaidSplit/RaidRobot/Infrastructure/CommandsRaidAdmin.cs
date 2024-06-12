using Discord;
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
        [Command("CreateEvent", RunMode = RunMode.Async)]
        public async Task CreateEvent(string eventName, string raidType, [Remainder] string eventDate)
        {
            try
            {
                var permissionResult = permissionChecker.CheckManagerPermissions(Context, "Create Event");
                if (!permissionResult.HasPremission)
                {
                    await ReplyAsync(permissionResult.Message);
                    return;
                }

                if (!DateTime.TryParse(eventDate, out var eventTime))
                {
                    await ReplyAsync("Event Date must be a valid date mm/dd/yy hh:mm...");
                    return;
                }
                await eventCreator.CreateEvent(Context, Context.User as IGuildUser, eventName, eventTime, raidType);
            }
            catch(Exception ex)
            {
                logger.DebugLog($"Error Creating Event {ex.Message}");
                await ReplyAsync($"Error: {ex.Message}");
            }
        }

        [Command("PreviewEvent", RunMode = RunMode.Async)]
        public async Task PreviewEvent(string eventName, string numberOfSplits)
        {
            try
            {
                var permissionResult = permissionChecker.CheckManagerPermissions(Context, "Preview Event");
                if (!permissionResult.HasPremission)
                {
                    await ReplyAsync(permissionResult.Message);
                    return;
                }


                if (!int.TryParse(numberOfSplits, out var splitCount))
                {
                    await ReplyAsync("Number of Splits must be numeric. !rr Split EventName NumberOfSplits");
                    return;
                }

                await eventOrchestrator.PrepareSplits(Context.Guild.Id, eventName, splitCount);
                
            }
            catch (Exception ex)
            {
                logger.DebugLog($"Error Previewing Event {ex.Message}");
                await ReplyAsync($"Error: {ex.Message}");

            }
        }

        [Command("AddASplit", RunMode = RunMode.Async)]
        public async Task AddASplit(string eventName)
        {
            try
            {
                var permissionResult = permissionChecker.CheckManagerPermissions(Context, "AddASplit");
                if (!permissionResult.HasPremission)
                {
                    await ReplyAsync(permissionResult.Message);
                    return;
                }

                await eventOrchestrator.AddASplit(Context.Guild.Id, eventName);
            }
            catch (Exception ex)
            {
                logger.DebugLog($"Error Adding Split {ex.Message}");
                await ReplyAsync($"Error: {ex.Message}");
            }

        }

        [Command("Undo", RunMode = RunMode.Async)]
        public async Task Undo(string eventName)
        {
            try
            {
                var permissionResult = permissionChecker.CheckManagerPermissions(Context, "Undo");
                if (!permissionResult.HasPremission)
                {
                    await ReplyAsync(permissionResult.Message);
                    return;
                }

                await eventOrchestrator.Undo(Context.Guild.Id, eventName);
            }
            catch (Exception ex)
            {
                logger.DebugLog($"Error Undoing Event {ex.Message}");
                await ReplyAsync($"Error: {ex.Message}");
            }

        }

        [Command("MoveTo", RunMode = RunMode.Async)]
        public async Task MoveTo(string eventName, string characterName, string splitNumber)
        {
            try
            {
                var permissionResult = permissionChecker.CheckManagerPermissions(Context, "MoveTo");
                if (!permissionResult.HasPremission)
                {
                    await ReplyAsync(permissionResult.Message);
                    return;
                }

                if (!int.TryParse(splitNumber, out var splitCount))
                {
                    await ReplyAsync("SplitNumber must be numeric. !rr Split EventName NumberOfSplits");
                    return;
                }

                await eventOrchestrator.MoveTo(Context.Guild.Id, eventName, characterName, splitCount);
            }
            catch (Exception ex)
            {
                await ReplyAsync($"Error: {ex.Message}");

            }
        }

        [Command("FinalizeEvent", RunMode = RunMode.Async)]
        public async Task FinalizeEvent(string eventName)
        {
            try
            {
                var permissionResult = permissionChecker.CheckManagerPermissions(Context, "Create Event");
                if (!permissionResult.HasPremission)
                {
                    await ReplyAsync(permissionResult.Message);
                    return;
                }

                await eventOrchestrator.FinalizeSplits(Context.Guild.Id, eventName);
            }
            catch (Exception ex)
            {
                await ReplyAsync($"Error: {ex.Message}");

            }
        }

        [Command("Swap", RunMode = RunMode.Async)]
        public async Task Swap(string eventName, string characterName1, string characterName2)
        {
            try
            {
                var permissionResult = permissionChecker.CheckManagerPermissions(Context, "Swap");
                if (!permissionResult.HasPremission)
                {
                    await ReplyAsync(permissionResult.Message);
                    return;
                }
                await eventOrchestrator.Swap(Context.Guild.Id, eventName, characterName1, characterName2);
            }
            catch (Exception ex)
            {
                await ReplyAsync($"Error: {ex.Message}");
            }
        }

        [Command("OpenEvent", RunMode = RunMode.Async)]
        public async Task OpenRaid(string eventName)
        {
            try
            {
                var permissionResult = permissionChecker.CheckManagerPermissions(Context, "Open Event");
                if (!permissionResult.HasPremission)
                {
                    await ReplyAsync(permissionResult.Message);
                    return;
                }

                await eventOrchestrator.OpenEvent(Context.Guild.Id, eventName);
            }
            catch (Exception ex)
            {
                await ReplyAsync($"Error: {ex.Message}");
            }
        }

        [Command("NeedsItem", RunMode = RunMode.Async)]
        public async Task NeedsItem(string eventName, string itemName, [Remainder] string characters)
        {
            try
            {
                var permissionResult = permissionChecker.CheckManagerPermissions(Context, "NeedsItem");
                if (!permissionResult.HasPremission)
                {
                    await ReplyAsync(permissionResult.Message);
                    return;
                }

                await eventOrchestrator.NeedsItem(Context.Guild.Id, eventName, itemName, characters);
            }
            catch (Exception ex)
            {
                await ReplyAsync($"Error: {ex.Message}");
            }
        }

        [Command("CancelEvent", RunMode = RunMode.Async)]
        public async Task CancelEvent([Remainder] string eventName)
        {
            try
            {
                var permissionResult = permissionChecker.CheckManagerPermissions(Context, "CancelEvent");
                if (!permissionResult.HasPremission)
                {
                    await ReplyAsync(permissionResult.Message);
                    return;
                }

                await eventOrchestrator.CancelEvent(Context.Guild.Id, eventName);
            }
            catch (Exception ex)
            {
                await ReplyAsync($"Error: {ex.Message}");

            }
        }

        [Command("Remove", RunMode = RunMode.Async)]
        public async Task Remove(string eventName, string characterName)
        {
            try
            {
                var permissionResult = permissionChecker.CheckManagerPermissions(Context, "Remove");
                if (!permissionResult.HasPremission)
                {
                    await ReplyAsync(permissionResult.Message);
                    return;
                }

                await eventOrchestrator.RemoveCharacter(Context.Guild.Id, eventName, characterName);
            }
            catch (Exception ex)
            {
                await ReplyAsync($"Error: {ex.Message}");
            }
        }

        [Command("SplitASplit", RunMode = RunMode.Async)]
        public async Task SplitASplit(string eventName, string splitNumberStr, string numberOfSplitsStr)
        {

            try
            {
                var permissionResult = permissionChecker.CheckManagerPermissions(Context, "SplitASplit");
                if (!permissionResult.HasPremission)
                {
                    await ReplyAsync(permissionResult.Message);
                    return;
                }

                if (!int.TryParse(splitNumberStr, out var splitNumber))
                {
                    await ReplyAsync($"Second Param Split Number must be number you entered {splitNumberStr}");
                    return;
                }

                if (!int.TryParse(numberOfSplitsStr, out var numberOfSplits))
                {
                    await ReplyAsync($"Third Param Number Of Splits must be number you entered {numberOfSplitsStr}");
                    return;
                }


                var user = Context.User as IGuildUser;
                if (user == null) return;

                var result = await eventOrchestrator.SplitASplit(Context.Guild.Id, Context.User.Id, user?.Nickname ?? user?.Username, eventName, splitNumber, numberOfSplits);
                await ReplyAsync(result);
            }
            catch (Exception ex)
            {
                await ReplyAsync($"Error: {ex.Message}");
            }
        }

        [Command("DebugEvent", RunMode = RunMode.Async)]
        public async Task DebugEvent(string eventName)
        {
            var result = await eventOrchestrator.DebugEvent(eventName);
            await ReplyAsync(result);
        }

    }
}
