using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HafezTelegram.Data;
using HafezTelegram.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TdLib;

namespace HafezTelegram.DataSource
{
    public class DataReceiver : BackgroundService
    {
        private readonly string _connectionString =
            "Data Source=localhost;Database=hafez_db;User ID=dbadmin;Password=Dqz5u_24;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;";

        private const string DollarChannelName = "💰طوفان هریرود💰";
        private const long DollarChannelId = -1001421742383;

        //private const string GoldChannelName = "قیمت آنلاین آبشده";
        //private const long GoldChannelId = -1001472775596;

        private const string GoldChannelName = "🌹کانال‌‌ vip قیمت‌آنلاین‌آبشده🌹";
        private const long GoldChannelId = -1001478222392;

        private readonly ILogger<DataReceiver> _logger;
        private readonly Random _random = new Random();
        private TdClient _client;

        public DataReceiver(ILogger<DataReceiver> logger)
        {
            _logger = logger;
        }

        private async Task StartTelegram()
        {
            try
            {
                DataReceiveHelper.Information.AppendLine("Telegram Function Is Started");
                _client = await DataReceiveHelper.NewClientAsync();
                DataReceiveHelper.Information.AppendLine("New Client Created");
                //await client.SetLogStreamAsync(new TdApi.LogStream.LogStreamFile() { Path = "tdlib.log" });
                _client.UpdateReceived += Client_UpdateReceived;
                DataReceiveHelper.Information.AppendLine("Client_UpdateReceived Created");
                //await _client.GetChatsAsync(null, Int64.MaxValue, 0, 100);
                //DataReceiveHelper.Information.AppendLine("GetChats Completed");
            }
            catch (TdException e)
            {
                var error = e.Error;
                DataReceiveHelper.Information.AppendLine("StartTelegram-1 " + error.Message);
            }
            catch (Exception ex)
            {
                DataReceiveHelper.Information.AppendLine("StartTelegram-2 " + ex.Message);
            }
        }

        private void Client_UpdateReceived(object sender, TdApi.Update e)
        {
            switch (e)
            {
                //case TdApi.Update.UpdateAuthorizationState updateAuthorizationState:
                //break;
                case TdApi.Update.UpdateNewMessage updateNewMessage:
                    //DataReceiveHelper.Information.AppendLine("New Message Received");
                    switch (updateNewMessage.Message.ChatId)
                    {
                        case GoldChannelId:
                            CheckGoldChannelMessage(updateNewMessage.Message);
                            break;
                        case DollarChannelId:
                            CheckDollarChannelMessage(updateNewMessage.Message);
                            break;
                    }
                    break;
            }
        }

        private void CheckGoldChannelMessage(TdApi.Message message)
        {
            try
            {
                //DataReceiveHelper.Information.AppendLine("Gold Message Received");
                var messageText = "";
                if (message.Content is TdApi.MessageContent.MessageText text)
                    messageText = text.Text.Text;
                var price = FindGoldPrice(messageText);
                if (price <= 0) return;
                var tomorrowMeltedGold = new TomorrowMeltedGold
                {
                    MessageId = message.Id,
                    MessageText = messageText,
                    Date = UnixTimeStampToDateTime(message.Date),
                    Price = price
                };
                var goldOptionsBuilder = new DbContextOptionsBuilder<TomorrowMeltedGoldContext>();
                goldOptionsBuilder.UseSqlServer(_connectionString); 
                using var goldContext =
                    new TomorrowMeltedGoldContext(goldOptionsBuilder.Options);
                if (goldContext.TomorrowMeltedGold.Any(item =>
                    item.MessageId == tomorrowMeltedGold.MessageId)) return;
                goldContext.Add(tomorrowMeltedGold); 
                goldContext.SaveChanges();
                //DataReceiveHelper.Information.AppendLine("New Gold Message Added");
            }
            catch (TdException e)
            {
                var error = e.Error;
                DataReceiveHelper.Information.AppendLine("CheckGoldChannelMessage-1 " + error.Message);
            }
            catch (Exception ex)
            {
                DataReceiveHelper.Information.AppendLine("CheckGoldChannelMessage-2 " + ex.Message);
            }
        }

        private void CheckDollarChannelMessage(TdApi.Message message)
        {
            try
            {
                //DataReceiveHelper.Information.AppendLine("Dollar Message Received");
                var messageText = "";
                if (message.Content is TdApi.MessageContent.MessageText text)
                    messageText = text.Text.Text;
                var price = FindDollarPrice(messageText);
                if (price <= 0) return;
                var tomorrowHeratDollar = new TomorrowHeratDollar
                {
                    MessageId = message.Id,
                    MessageText = messageText,
                    Date = UnixTimeStampToDateTime(message.Date),
                    Price = price
                };

                var dollarOptionsBuilder = new DbContextOptionsBuilder<TomorrowHeratDollarContext>();
                dollarOptionsBuilder.UseSqlServer(_connectionString); 
                using var dollarContext = new TomorrowHeratDollarContext(dollarOptionsBuilder.Options);
                if (dollarContext.TomorrowHeratDollar.Any(item =>
                    item.MessageId == tomorrowHeratDollar.MessageId)) return;
                dollarContext.Add(tomorrowHeratDollar);
                dollarContext.SaveChanges();
                //DataReceiveHelper.Information.AppendLine("New Dollar Message Added");
            }
            catch (TdException e)
            {
                var error = e.Error;
                DataReceiveHelper.Information.AppendLine("CheckDollarChannelMessage-1 " + error.Message);
            }
            catch (Exception ex)
            {
                DataReceiveHelper.Information.AppendLine("CheckDollarChannelMessage-2 " + ex.Message);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //while (stoppingToken.IsCancellationRequested)
            //{
            //if (!DataReceiveHelper.IsAuthorised) break;
            //GetOldData();
            await StartTelegram();
            //await Task.Delay(1000, stoppingToken);
            //}
        }

        private async void GetOldData()
        {
            try
            {
                var goldOptionsBuilder = new DbContextOptionsBuilder<TomorrowMeltedGoldContext>();
                goldOptionsBuilder.UseSqlServer(_connectionString);
                var dollarOptionsBuilder = new DbContextOptionsBuilder<TomorrowHeratDollarContext>();
                dollarOptionsBuilder.UseSqlServer(_connectionString);
                TdApi.Chat goldChannel = null;
                TdApi.Chat dollarChannel = null;
                using var client = await DataReceiveHelper.NewClientAsync();
                await foreach (var chat in GetChannels(client))
                {
                    //DataReceiveHelper.Information.AppendLine($"{chat.Title} ID {chat.Id}");
                    if (chat.Title.Equals(GoldChannelName)) goldChannel = chat;
                    if (chat.Title.Equals(DollarChannelName)) dollarChannel = chat;
                }
                var goldMessageId = goldChannel?.LastMessage.Id ?? 0;
                var dollarMessageId = dollarChannel?.LastMessage.Id ?? 0;
                for (var j = 0; j < 10000; j++)
                {
                    try
                    {
                        if (goldChannel != null)
                        {
                            var messages = await client.GetChatHistoryAsync(goldChannel.Id, goldMessageId, 0, 10);
                            foreach (var message in messages.Messages_)
                            {
                                goldMessageId = message.Id;
                                var messageText = "";
                                if (message.Content is TdApi.MessageContent.MessageText text)
                                    messageText = text.Text.Text;
                                var price = FindGoldPrice(messageText);
                                if (price <= 0) continue;
                                var tomorrowMeltedGold = new TomorrowMeltedGold
                                {
                                    MessageId = message.Id,
                                    MessageText = messageText,
                                    Date = UnixTimeStampToDateTime(message.Date),
                                    Price = price
                                };
                                await using var goldContext = new TomorrowMeltedGoldContext(goldOptionsBuilder.Options);
                                if (goldContext.TomorrowMeltedGold.All(item =>
                                    item.MessageId != tomorrowMeltedGold.MessageId))
                                {
                                    goldContext.Add(tomorrowMeltedGold);
                                    await goldContext.SaveChangesAsync();
                                }
                            }
                        }

                        if (dollarChannel != null)
                        {
                            var messages =
                                await client.GetChatHistoryAsync(dollarChannel.Id, dollarMessageId, 0, 10);
                            foreach (var message in messages.Messages_)
                            {
                                dollarMessageId = message.Id;
                                var messageText = "";
                                if (message.Content is TdApi.MessageContent.MessageText text)
                                    messageText = text.Text.Text;
                                var price = FindDollarPrice(messageText);
                                if (price <= 0) continue;
                                var tomorrowHeratDollar = new TomorrowHeratDollar
                                {
                                    MessageId = message.Id,
                                    MessageText = messageText,
                                    Date = UnixTimeStampToDateTime(message.Date),
                                    Price = price
                                };
                                await using var dollarContext =
                                    new TomorrowHeratDollarContext(dollarOptionsBuilder.Options);
                                if (dollarContext.TomorrowHeratDollar.All(item =>
                                    item.MessageId != tomorrowHeratDollar.MessageId))
                                {
                                    dollarContext.Add(tomorrowHeratDollar);
                                    await dollarContext.SaveChangesAsync();
                                }
                            }
                        }
                    }
                    catch (TdException e)
                    {
                        var error = e.Error;
                        DataReceiveHelper.Information.AppendLine("GetOldData-1-1 " + error.Message);
                    }
                    catch (Exception ex)
                    {
                        DataReceiveHelper.Information.AppendLine("GetOldData-1-2 " + ex.Message);
                    }

                    _logger.LogInformation("DataReceiver Service running at: {time}", DateTimeOffset.Now);
                    DataReceiveHelper.LoopCount++;
                    await Task.Delay(TimeSpan.FromSeconds(_random.Next(3, 7)));
                }
            }
            catch (TdException e)
            {
                var error = e.Error;
                DataReceiveHelper.Information.AppendLine("GetOldData-2-1 " + error.Message);
            }
            catch (Exception ex)
            {
                DataReceiveHelper.Information.AppendLine("GetOldData-2-2 " + ex.Message);
            }
        }

        private async IAsyncEnumerable<TdApi.Chat> GetChannels(TdClient client, long offsetOrder = long.MaxValue,
            long offsetId = 0, int limit = 1000)
        {
            var chats = await client.ExecuteAsync(new TdApi.GetChats
            { OffsetOrder = offsetOrder, Limit = limit, OffsetChatId = offsetId });
            foreach (var chatId in chats.ChatIds)
            {
                var chat = await client.ExecuteAsync(new TdApi.GetChat { ChatId = chatId });
                yield return chat;
            }
        }

        private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        private int FindGoldPrice(string message)
        {
            if (message.IndexOf("معامله", StringComparison.Ordinal) < 0) return 0;
            if (message.IndexOf("آبشده‌فردایی‌پله‌آهنی", StringComparison.Ordinal) > 0)
            {
                var value = message.Substring(23, 9);
                //DataReceiveHelper.Information.AppendLine($"value:{value} | ");
                value = value.Replace(",", "");
                var result = int.TryParse(value, out var price);
                if (result)
                    return price;
                return 0;
            }

            //if (message.IndexOf("نقدی_شنبه", StringComparison.Ordinal) > 0)
            //{
            //    var value = message.Substring(11, 9);
            //    value = value.Replace(",", "");
            //    var result = int.TryParse(value, out var price);
            //    if (result)
            //        return price;
            //    return 0;
            //}

            return 0;
        }

        private int FindDollarPrice(string message)
        {
            if (message.IndexOf("معامله", StringComparison.Ordinal) < 0) return 0;
            if (message.IndexOf("فردایۍ", StringComparison.Ordinal) > 0)
            {
                var value = message.Substring(0, 6);
                value = value.Replace(",", "");
                var result = int.TryParse(value, out var price);
                if (result)
                    return price;
                return 0;
            }

            if (message.IndexOf("پسفردا", StringComparison.Ordinal) > 0)
            {
                var value = message.Substring(0, 6);
                value = value.Replace(",", "");
                var result = int.TryParse(value, out var price);
                if (result)
                    return price;
                return 0;
            }

            if (message.IndexOf("شنبه", StringComparison.Ordinal) > 0)
            {
                var value = message.Substring(0, 6);
                value = value.Replace(",", "");
                var result = int.TryParse(value, out var price);
                if (result)
                    return price;
                return 0;
            }

            return 0;
        }
    }
}