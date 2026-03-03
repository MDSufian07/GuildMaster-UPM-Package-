using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using GuildMaster.Core.Entities;
using GuildMaster.Core.ValueObjects;

namespace GuildMaster.Systems.SaveManager
{

    public class SaveManagerService
    {
        private readonly string _savePath = "Saves";
        private readonly string _guildsFile = "guild_data.csv";
        private readonly string _adventurersFile = "adventurers_data.csv";
        private const char Sep = '|';

        public SaveManagerService()
        {
            // Create Saves directory if it doesn't exist
            if (!Directory.Exists(_savePath))
            {
                Directory.CreateDirectory(_savePath);
            }
        }

        /// <summary>
        /// Save guild and adventurer data to CSV files
        /// </summary>
        public void SaveGame(Guild guild)
        {
            try
            {
                // Save guild data
                SaveGuildData(guild);

                // Save adventurer data
                SaveAdventurerData(guild);

                Debug.Log("✓ Game saved successfully!");
            }
            catch (Exception ex)
            {
                Debug.LogError($"✗ Error saving game: {ex.Message}");
            }
        }

        /// <summary>
        /// Load guild and adventurer data from CSV files
        /// </summary>
        public Guild? LoadGame()
        {
            try
            {
                var guildFilePath = Path.Combine(_savePath, _guildsFile);

                if (!File.Exists(guildFilePath))
                {
                    Debug.Log("No save file found. Starting new game.");
                    return null;
                }

                var fileInfo = new FileInfo(guildFilePath);
                if (fileInfo.Length < 10)
                {
                    Debug.LogWarning("Save file is empty or corrupted. Starting new game.");

                    // Delete corrupted files
                    if (File.Exists(guildFilePath))
                        File.Delete(guildFilePath);
                    if (File.Exists(Path.Combine(_savePath, _adventurersFile)))
                        File.Delete(Path.Combine(_savePath, _adventurersFile));

                    return null;
                }

                // Load guild data
                var guildData = LoadGuildData();
                if (guildData == null) return null;

                // Create guild from loaded data
                var guild = new Guild(guildData.GuildCoins, 3);

                // Manually set loaded values using reflection
                guild.GetType().GetProperty("Coins")?.SetValue(guild, guildData.GuildCoins);
                guild.GetType().GetProperty("Reputation")?.SetValue(guild, guildData.GuildReputation);
                guild.GetType().GetProperty("GuildLevel")?.SetValue(guild, guildData.GuildLevel);

                // Load adventurers
                var adventurersData = LoadAdventurerData();
                foreach (var advData in adventurersData)
                {
                    var stats = new Dictionary<StatType, int>
                    {
                        { StatType.Combat, advData.Combat },
                        { StatType.Defense, advData.Defense },
                        { StatType.Intelligence, advData.Intelligence },
                        { StatType.Agility, advData.Agility },
                        { StatType.Charisma, advData.Charisma }
                    };

                    if (!string.IsNullOrEmpty(advData.Specialty))
                    {
                        if (Enum.TryParse<SpecialtyType>(advData.Specialty, out var specialty))
                        {
                            if (!string.IsNullOrEmpty(advData.Name))
                            {
                                var adventurer = new Adventurer(advData.Name, stats, specialty);

                                // Set additional properties
                                adventurer.GetType().GetProperty("Level")?.SetValue(adventurer, advData.Level);
                                adventurer.GetType().GetProperty("XP")?.SetValue(adventurer, advData.XP);
                                adventurer.GetType().GetProperty("CostPerMission")
                                    ?.SetValue(adventurer, advData.CostPerMission);
                                adventurer.GetType().GetProperty("IsInjured")?.SetValue(adventurer, advData.IsInjured);
                                adventurer.GetType().GetProperty("RecoveryTime")
                                    ?.SetValue(adventurer, advData.RecoveryTime);

                                if (!string.IsNullOrEmpty(advData.Status))
                                {
                                    if (Enum.TryParse<Core.Enums.AdventurerStatus>(advData.Status, out var status))
                                    {
                                        adventurer.GetType().GetProperty("Status")?.SetValue(adventurer, status);
                                    }
                                }

                                guild.Adventurers.Add(adventurer);
                            }
                        }
                    }
                }

                Debug.Log($"✓ Game loaded successfully! Guild Level: {guild.GuildLevel}, Adventurers: {guild.Adventurers.Count}");

                return guild;
            }
            catch (Exception ex)
            {
                Debug.LogError($"✗ Error loading game: {ex.Message}");
                Debug.Log("Starting new game instead.");

                // Clean up corrupted save files
                try
                {
                    if (File.Exists(Path.Combine(_savePath, _guildsFile)))
                        File.Delete(Path.Combine(_savePath, _guildsFile));
                    if (File.Exists(Path.Combine(_savePath, _adventurersFile)))
                        File.Delete(Path.Combine(_savePath, _adventurersFile));
                }
                catch
                {
                }

                return null;
            }
        }

        private void SaveGuildData(Guild guild)
        {
            var guildPath = Path.Combine(_savePath, _guildsFile);

            var sb = new StringBuilder();
            // header
            sb.AppendLine(string.Join(Sep, new[] { "Coins", "Reputation", "GuildLevel", "Adventurers", "Timestamp" }));
            // record
            sb.AppendLine(string.Join(Sep, new[] { guild.Coins.ToString(), guild.Reputation.ToString(), guild.GuildLevel.ToString(), guild.Adventurers.Count.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }));

            File.WriteAllText(guildPath, sb.ToString());
        }

        private void SaveAdventurerData(Guild guild)
        {
            var adventurersPath = Path.Combine(_savePath, _adventurersFile);
            var sb = new StringBuilder();

            var headerFields = new[] { "Name", "Level", "XP", "CostPerMission", "Specialty", "IsInjured", "RecoveryTime", "Status", "Combat", "Defense", "Intelligence", "Agility", "Charisma" };
            sb.AppendLine(string.Join(Sep, headerFields));

            foreach (var adv in guild.Adventurers)
            {
                var fields = new[] {
                    Escape(adv.Name),
                    adv.Level.ToString(),
                    adv.XP.ToString(),
                    adv.CostPerMission.ToString(),
                    Escape(adv.Specialty.ToString()),
                    adv.IsInjured.ToString(),
                    adv.RecoveryTime.ToString(),
                    Escape(adv.Status.ToString()),
                    adv.GetStat(StatType.Combat).ToString(),
                    adv.GetStat(StatType.Defense).ToString(),
                    adv.GetStat(StatType.Intelligence).ToString(),
                    adv.GetStat(StatType.Agility).ToString(),
                    adv.GetStat(StatType.Charisma).ToString()
                };

                sb.AppendLine(string.Join(Sep, fields));
            }

            File.WriteAllText(adventurersPath, sb.ToString());
        }

        private SaveData? LoadGuildData()
        {
            var guildPath = Path.Combine(_savePath, _guildsFile);

            var lines = File.ReadAllLines(guildPath);
            if (lines.Length < 2) return null;

            var header = lines[0].Split(Sep);
            var values = lines[1].Split(Sep);

            int ParseInt(string? s) => int.TryParse(s, out var v) ? v : 0;

            return new SaveData
            {
                GuildCoins = ParseInt(values.ElementAtOrDefault(0)),
                GuildReputation = ParseInt(values.ElementAtOrDefault(1)),
                GuildLevel = ParseInt(values.ElementAtOrDefault(2))
            };
        }

        private List<AdventurerData> LoadAdventurerData()
        {
            var adventurersPath = Path.Combine(_savePath, _adventurersFile);

            var result = new List<AdventurerData>();
            if (!File.Exists(adventurersPath)) return result;

            var lines = File.ReadAllLines(adventurersPath);
            if (lines.Length < 2) return result;

            var header = lines[0].Split(Sep);
            for (int i = 1; i < lines.Length; i++)
            {
                var cols = lines[i].Split(Sep);
                var adv = new AdventurerData();
                adv.Name = cols.ElementAtOrDefault(0) ?? string.Empty;
                adv.Level = int.TryParse(cols.ElementAtOrDefault(1) ?? string.Empty, out var lvl) ? lvl : 1;
                adv.XP = int.TryParse(cols.ElementAtOrDefault(2) ?? string.Empty, out var xp) ? xp : 0;
                adv.CostPerMission = int.TryParse(cols.ElementAtOrDefault(3) ?? string.Empty, out var cpm) ? cpm : 0;
                adv.Specialty = cols.ElementAtOrDefault(4) ?? string.Empty;
                adv.IsInjured = bool.TryParse(cols.ElementAtOrDefault(5) ?? string.Empty, out var inj) ? inj : false;
                adv.RecoveryTime = int.TryParse(cols.ElementAtOrDefault(6) ?? string.Empty, out var rt) ? rt : 0;
                adv.Status = cols.ElementAtOrDefault(7) ?? string.Empty;
                adv.Combat = int.TryParse(cols.ElementAtOrDefault(8) ?? string.Empty, out var c1) ? c1 : 0;
                adv.Defense = int.TryParse(cols.ElementAtOrDefault(9) ?? string.Empty, out var c2) ? c2 : 0;
                adv.Intelligence = int.TryParse(cols.ElementAtOrDefault(10) ?? string.Empty, out var c3) ? c3 : 0;
                adv.Agility = int.TryParse(cols.ElementAtOrDefault(11) ?? string.Empty, out var c4) ? c4 : 0;
                adv.Charisma = int.TryParse(cols.ElementAtOrDefault(12) ?? string.Empty, out var c5) ? c5 : 0;

                result.Add(adv);
            }

            return result;
        }

        /// <summary>
        /// Delete all save files
        /// </summary>
        public void DeleteSave()
        {
            try
            {
                var guildPath = Path.Combine(_savePath, _guildsFile);
                var adventurersPath = Path.Combine(_savePath, _adventurersFile);

                if (File.Exists(guildPath)) File.Delete(guildPath);
                if (File.Exists(adventurersPath)) File.Delete(adventurersPath);

                Debug.Log("✓ Save files deleted.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"✗ Error deleting save: {ex.Message}");
            }
        }

        private static string Escape(string? s) => (s ?? string.Empty).Replace(Sep, ' ').Replace("\n", " ").Replace("\r", " ");
    }
}
