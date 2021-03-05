﻿// <auto-generated />
using System;
using Kaguya.Database.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Kaguya.Migrations
{
    [DbContext(typeof(KaguyaDbContext))]
    partial class KaguyaDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 64)
                .HasAnnotation("ProductVersion", "5.0.1");

            modelBuilder.Entity("Kaguya.Database.Model.AdminAction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Action")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<ulong>("ActionedUserId")
                        .HasColumnType("bigint unsigned");

                    b.Property<DateTimeOffset?>("Expiration")
                        .HasColumnType("datetime(6)");

                    b.Property<bool?>("HasTriggered")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("IsHidden")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("IsSystemAction")
                        .HasColumnType("tinyint(1)");

                    b.Property<ulong>("ModeratorId")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("Reason")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<ulong>("ServerId")
                        .HasColumnType("bigint unsigned");

                    b.Property<DateTimeOffset>("Timestamp")
                        .HasColumnType("datetime(6)");

                    b.HasKey("Id");

                    b.HasIndex("ServerId");

                    b.HasIndex("ServerId", "Action");

                    b.HasIndex("ActionedUserId", "ServerId", "Expiration");

                    b.HasIndex("ActionedUserId", "ServerId", "Action", "Expiration");

                    b.ToTable("AdminActions");
                });

            modelBuilder.Entity("Kaguya.Database.Model.AntiRaidConfig", b =>
                {
                    b.Property<ulong>("ServerId")
                        .HasColumnType("bigint unsigned");

                    b.Property<int>("Action")
                        .HasColumnType("int");

                    b.Property<string>("AntiraidPunishmentDirectMessage")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<bool>("ConfigEnabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("PunishmentDmEnabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<TimeSpan?>("PunishmentLength")
                        .HasColumnType("time(6)");

                    b.Property<uint>("Seconds")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("UserThreshold")
                        .HasColumnType("int unsigned");

                    b.HasKey("ServerId");

                    b.ToTable("AntiRaidConfigs");
                });

            modelBuilder.Entity("Kaguya.Database.Model.AutoAssignedRole", b =>
                {
                    b.Property<ulong>("ServerId")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("RoleId")
                        .HasColumnType("bigint unsigned");

                    b.Property<TimeSpan?>("Delay")
                        .HasColumnType("time(6)");

                    b.HasKey("ServerId", "RoleId");

                    b.ToTable("AutoAssignedRoles");
                });

            modelBuilder.Entity("Kaguya.Database.Model.BlacklistedEntity", b =>
                {
                    b.Property<ulong>("EntityId")
                        .HasColumnType("bigint unsigned");

                    b.Property<int>("EntityType")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset?>("ExpirationTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Reason")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.HasKey("EntityId");

                    b.ToTable("BlacklistedEntities");
                });

            modelBuilder.Entity("Kaguya.Database.Model.CommandHistory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("CommandName")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<string>("ErrorMessage")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<bool>("ExecutedSuccessfully")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTimeOffset>("ExecutionTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Message")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<ulong>("ServerId")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("UserId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.HasIndex("UserId", "ServerId", "CommandName");

                    b.ToTable("CommandHistories");
                });

            modelBuilder.Entity("Kaguya.Database.Model.Eightball", b =>
                {
                    b.Property<string>("Phrase")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<int>("Outlook")
                        .HasColumnType("int");

                    b.HasKey("Phrase");

                    b.ToTable("Eightballs");
                });

            modelBuilder.Entity("Kaguya.Database.Model.FavoriteTrack", b =>
                {
                    b.Property<ulong>("UserId")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("SongId")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<DateTimeOffset>("DateAdded")
                        .HasColumnType("datetime(6)");

                    b.Property<ulong>("ServerId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("UserId", "SongId");

                    b.HasIndex("UserId");

                    b.ToTable("FavoriteTracks");
                });

            modelBuilder.Entity("Kaguya.Database.Model.FilteredWord", b =>
                {
                    b.Property<ulong>("ServerId")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("Word")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<int>("FilterReaction")
                        .HasColumnType("int");

                    b.HasKey("ServerId", "Word");

                    b.HasIndex("ServerId");

                    b.ToTable("FilteredWords");
                });

            modelBuilder.Entity("Kaguya.Database.Model.Fish", b =>
                {
                    b.Property<long>("FishId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<int>("BaseCost")
                        .HasColumnType("int");

                    b.Property<ulong>("ChannelId")
                        .HasColumnType("bigint unsigned");

                    b.Property<int>("CoinValue")
                        .HasColumnType("int");

                    b.Property<int>("CostOfPlay")
                        .HasColumnType("int");

                    b.Property<int>("ExpValue")
                        .HasColumnType("int");

                    b.Property<int>("FishType")
                        .HasColumnType("int");

                    b.Property<int>("Rarity")
                        .HasColumnType("int");

                    b.Property<ulong>("ServerId")
                        .HasColumnType("bigint unsigned");

                    b.Property<DateTimeOffset>("TimeCaught")
                        .HasColumnType("datetime(6)");

                    b.Property<ulong>("UserId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("FishId");

                    b.HasIndex("ServerId");

                    b.HasIndex("UserId");

                    b.HasIndex("UserId", "FishType");

                    b.HasIndex("UserId", "Rarity");

                    b.ToTable("Fish");
                });

            modelBuilder.Entity("Kaguya.Database.Model.GambleHistory", b =>
                {
                    b.Property<uint>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int unsigned");

                    b.Property<int>("Action")
                        .HasColumnType("int");

                    b.Property<int>("AmountBet")
                        .HasColumnType("int");

                    b.Property<int>("AmountRewarded")
                        .HasColumnType("int");

                    b.Property<bool>("IsWinner")
                        .HasColumnType("tinyint(1)");

                    b.Property<ulong>("ServerId")
                        .HasColumnType("bigint unsigned");

                    b.Property<DateTimeOffset>("Timestamp")
                        .HasColumnType("datetime(6)");

                    b.Property<ulong>("UserId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.HasIndex("ServerId");

                    b.HasIndex("UserId");

                    b.HasIndex("UserId", "IsWinner");

                    b.ToTable("GambleHistories");
                });

            modelBuilder.Entity("Kaguya.Database.Model.Giveaway", b =>
                {
                    b.Property<ulong>("MessageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned");

                    b.Property<int>("Amount")
                        .HasColumnType("int");

                    b.Property<ulong>("ChannelId")
                        .HasColumnType("bigint unsigned");

                    b.Property<int?>("Coins")
                        .HasColumnType("int");

                    b.Property<int?>("Exp")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset>("Expiration")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Item")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<ulong>("ServerId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("MessageId");

                    b.HasIndex("Expiration");

                    b.HasIndex("ServerId", "Expiration");

                    b.ToTable("Giveaways");
                });

            modelBuilder.Entity("Kaguya.Database.Model.KaguyaServer", b =>
                {
                    b.Property<ulong>("ServerId")
                        .HasColumnType("bigint unsigned");

                    b.Property<bool>("AutomaticOsuLinkParsingEnabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("CommandPrefix")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("CustomGreeting")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<bool>("CustomGreetingIsEnabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<ulong?>("CustomGreetingTextChannelId")
                        .HasColumnType("bigint unsigned");

                    b.Property<DateTimeOffset>("DateFirstTracked")
                        .HasColumnType("datetime(6)");

                    b.Property<bool>("IsNsfwAllowed")
                        .HasColumnType("tinyint(1)");

                    b.Property<ulong?>("LevelAnnouncementsChannelId")
                        .HasColumnType("bigint unsigned");

                    b.Property<int>("LevelNotifications")
                        .HasColumnType("int");

                    b.Property<ulong?>("MuteRoleId")
                        .HasColumnType("bigint unsigned");

                    b.Property<DateTimeOffset?>("NsfwAllowanceTime")
                        .HasColumnType("datetime(6)");

                    b.Property<ulong?>("NsfwAllowedId")
                        .HasColumnType("bigint unsigned");

                    b.Property<int>("PraiseCooldown")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset?>("PremiumExpiration")
                        .HasColumnType("datetime(6)");

                    b.Property<ulong?>("ShadowbanRoleId")
                        .HasColumnType("bigint unsigned");

                    b.Property<int>("TotalAdminActions")
                        .HasColumnType("int");

                    b.Property<int>("TotalCommandCount")
                        .HasColumnType("int");

                    b.HasKey("ServerId");

                    b.ToTable("KaguyaServers");
                });

            modelBuilder.Entity("Kaguya.Database.Model.KaguyaStatistics", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<long>("Coins")
                        .HasColumnType("bigint");

                    b.Property<int>("CommandsExecuted")
                        .HasColumnType("int");

                    b.Property<int>("CommandsExecutedTwentyFourHours")
                        .HasColumnType("int");

                    b.Property<int>("ConnectedServers")
                        .HasColumnType("int");

                    b.Property<int>("Fish")
                        .HasColumnType("int");

                    b.Property<int>("Gambles")
                        .HasColumnType("int");

                    b.Property<int>("LatencyMilliseconds")
                        .HasColumnType("int");

                    b.Property<double>("RamUsageMegabytes")
                        .HasColumnType("double");

                    b.Property<int>("Servers")
                        .HasColumnType("int");

                    b.Property<int>("Shards")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset?>("Timestamp")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("Users")
                        .HasColumnType("int");

                    b.Property<string>("Version")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.HasKey("Id");

                    b.ToTable("KaguyaStatistics");
                });

            modelBuilder.Entity("Kaguya.Database.Model.KaguyaUser", b =>
                {
                    b.Property<ulong>("UserId")
                        .HasColumnType("bigint unsigned");

                    b.Property<int>("ActiveRateLimit")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset?>("BlacklistExpiration")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("Coins")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset>("DateFirstTracked")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("FishExp")
                        .HasColumnType("int");

                    b.Property<int>("GlobalExp")
                        .HasColumnType("int");

                    b.Property<int>("GrossGambleCoinLosses")
                        .HasColumnType("int");

                    b.Property<int>("GrossGambleCoinWinnings")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset?>("LastBlacklisted")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTimeOffset?>("LastDailyBonus")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTimeOffset?>("LastFished")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTimeOffset?>("LastGivenExp")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTimeOffset?>("LastGivenRep")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTimeOffset?>("LastRatelimited")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTimeOffset?>("LastUpvotedDiscordBoats")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTimeOffset?>("LastUpvotedTopGg")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTimeOffset?>("LastWeeklyBonus")
                        .HasColumnType("datetime(6)");

                    b.Property<int?>("OsuGameMode")
                        .HasColumnType("int");

                    b.Property<long?>("OsuId")
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset?>("PremiumExpiration")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("RateLimitWarnings")
                        .HasColumnType("int");

                    b.Property<int>("TotalCoinsGambled")
                        .HasColumnType("int");

                    b.Property<int>("TotalCommandUses")
                        .HasColumnType("int");

                    b.Property<int>("TotalDaysPremium")
                        .HasColumnType("int");

                    b.Property<int>("TotalGambleLosses")
                        .HasColumnType("int");

                    b.Property<int>("TotalGambleWins")
                        .HasColumnType("int");

                    b.Property<int>("TotalGambles")
                        .HasColumnType("int");

                    b.Property<int>("TotalPremiumRedemptions")
                        .HasColumnType("int");

                    b.Property<int>("TotalUpvotesDiscordBoats")
                        .HasColumnType("int");

                    b.Property<int>("TotalUpvotesTopGg")
                        .HasColumnType("int");

                    b.HasKey("UserId");

                    b.HasIndex("ActiveRateLimit");

                    b.ToTable("KaguyaUsers");
                });

            modelBuilder.Entity("Kaguya.Database.Model.LogConfiguration", b =>
                {
                    b.Property<ulong>("ServerId")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong?>("Antiraids")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong?>("Bans")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong?>("FilteredWord")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong?>("MessageDeleted")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong?>("MessageUpdated")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong?>("Shadowbans")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong?>("UnBans")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong?>("Unshadowbans")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong?>("Unwarns")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong?>("UserJoins")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong?>("UserLeaves")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong?>("VoiceUpdates")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong?>("Warns")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("ServerId");

                    b.ToTable("LogConfigurations");
                });

            modelBuilder.Entity("Kaguya.Database.Model.PremiumKey", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTimeOffset?>("Expiration")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Key")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<ulong>("KeyCreatorId")
                        .HasColumnType("bigint unsigned");

                    b.Property<int>("LengthInSeconds")
                        .HasColumnType("int");

                    b.Property<ulong?>("ServerId")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong?>("UserId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.HasIndex("Key");

                    b.ToTable("PremiumKeys");
                });

            modelBuilder.Entity("Kaguya.Database.Model.Quote", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<ulong>("ServerId")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("Text")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<DateTimeOffset>("Timestamp")
                        .HasColumnType("datetime(6)");

                    b.Property<ulong>("UserId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.HasIndex("ServerId");

                    b.ToTable("Quotes");
                });

            modelBuilder.Entity("Kaguya.Database.Model.ReactionRole", b =>
                {
                    b.Property<ulong>("MessageId")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("RoleId")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("Emote")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<bool>("IsStandardEmoji")
                        .HasColumnType("tinyint(1)");

                    b.Property<ulong>("ServerId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("MessageId", "RoleId");

                    b.ToTable("ReactionRoles");
                });

            modelBuilder.Entity("Kaguya.Database.Model.Reminder", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset>("Expiration")
                        .HasColumnType("datetime(6)");

                    b.Property<bool>("HasTriggered")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Text")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<ulong>("UserId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.HasIndex("UserId", "Expiration");

                    b.ToTable("Reminders");
                });

            modelBuilder.Entity("Kaguya.Database.Model.Rep", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<ulong>("GivenBy")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("Reason")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<DateTimeOffset>("TimeGiven")
                        .HasColumnType("datetime(6)");

                    b.Property<ulong>("UserId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Rep");
                });

            modelBuilder.Entity("Kaguya.Database.Model.RoleReward", b =>
                {
                    b.Property<ulong>("RoleId")
                        .HasColumnType("bigint unsigned");

                    b.Property<int>("Level")
                        .HasColumnType("int");

                    b.Property<ulong>("ServerId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("RoleId");

                    b.HasIndex("ServerId");

                    b.ToTable("RoleRewards");
                });

            modelBuilder.Entity("Kaguya.Database.Model.ServerExperience", b =>
                {
                    b.Property<ulong>("ServerId")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("UserId")
                        .HasColumnType("bigint unsigned");

                    b.Property<int>("Exp")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset?>("LastGivenExp")
                        .HasColumnType("datetime(6)");

                    b.HasKey("ServerId", "UserId");

                    b.ToTable("ServerExperience");
                });

            modelBuilder.Entity("Kaguya.Database.Model.Upvote", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<ulong>("BotId")
                        .HasColumnType("bigint unsigned");

                    b.Property<int>("CoinsAwarded")
                        .HasColumnType("int");

                    b.Property<int>("ExpAwarded")
                        .HasColumnType("int");

                    b.Property<bool>("IsWeekend")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("QueryParams")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<bool>("ReminderSent")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTimeOffset>("Timestamp")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Type")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<ulong>("UserId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Upvotes");
                });

            modelBuilder.Entity("Kaguya.Database.Model.WarnConfiguration", b =>
                {
                    b.Property<ulong>("ServerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned");

                    b.Property<int>("BanNum")
                        .HasColumnType("int");

                    b.Property<int>("KickNum")
                        .HasColumnType("int");

                    b.Property<int>("MuteNum")
                        .HasColumnType("int");

                    b.Property<int>("ShadowbanNum")
                        .HasColumnType("int");

                    b.HasKey("ServerId");

                    b.ToTable("WarnConfigurations");
                });

            modelBuilder.Entity("Kaguya.Database.Views.KaguyaUserExperienceRank", b =>
                {
                    b.Property<int>("Rank")
                        .HasColumnType("int");

                    b.Property<ulong>("UserId")
                        .HasColumnType("bigint unsigned");

                    b.ToView("KaguyaUserRanks");
                });
#pragma warning restore 612, 618
        }
    }
}
