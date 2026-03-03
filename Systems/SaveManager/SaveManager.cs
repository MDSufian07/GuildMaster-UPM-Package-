using GuildMaster.Core.Entities;
using GuildMaster.Core.ValueObjects;
using System.Globalization;
using CsvHelper;

namespace GuildMaster.Systems.SaveManager
{

    public class SaveManagerService
    {
        private readonly string _savePath = "Saves";
        private readonly string _guildsFile = "guild_data.csv";
        private readonly string _adventurersFile = "adventurers_data.csv";

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

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ Game saved successfully!");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Error saving game: {ex.Message}");
                Console.ResetColor();
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
                    Console.WriteLine("No save file found. Starting new game.");
                    return null;
                }

                // Check if file is empty or too small (less than 10 bytes means no valid data)
                var fileInfo = new FileInfo(guildFilePath);
                if (fileInfo.Length < 10)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Save file is empty or corrupted. Starting new game.");
                    Console.ResetColor();

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
                var guild = new Guild(guildData.GuildCoins, 3)
                {
                };

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

                    if (advData.Specialty != null)
                    {
                        var specialty = Enum.Parse<SpecialtyType>(advData.Specialty);
                        if (advData.Name != null)
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

                            if (advData.Status != null)
                            {
                                var status = Enum.Parse<Core.Enums.AdventurerStatus>(advData.Status);
                                adventurer.GetType().GetProperty("Status")?.SetValue(adventurer, status);
                            }

                            guild.Adventurers.Add(adventurer);
                        }
                    }
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(
                    $"✓ Game loaded successfully! Guild Level: {guild.GuildLevel}, Adventurers: {guild.Adventurers.Count}");
                Console.ResetColor();

                return guild;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Error loading game: {ex.Message}");
                Console.WriteLine("Starting new game instead.");
                Console.ResetColor();

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
            var guildData = new[]
            {
                new
                {
                    Coins = guild.Coins,
                    Reputation = guild.Reputation,
                    GuildLevel = guild.GuildLevel,
                    Adventurers = guild.Adventurers.Count,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                }
            };

            using (var writer = new StreamWriter(guildPath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(guildData);
            }
        }

        private void SaveAdventurerData(Guild guild)
        {
            var adventurersPath = Path.Combine(_savePath, _adventurersFile);
            var adventurersData = new List<AdventurerData>();

            foreach (var adv in guild.Adventurers)
            {
                adventurersData.Add(new AdventurerData
                {
                    Name = adv.Name,
                    Level = adv.Level,
                    XP = adv.XP,
                    CostPerMission = adv.CostPerMission,
                    Specialty = adv.Specialty.ToString(),
                    IsInjured = adv.IsInjured,
                    RecoveryTime = adv.RecoveryTime,
                    Status = adv.Status.ToString(),
                    Combat = adv.GetStat(StatType.Combat),
                    Defense = adv.GetStat(StatType.Defense),
                    Intelligence = adv.GetStat(StatType.Intelligence),
                    Agility = adv.GetStat(StatType.Agility),
                    Charisma = adv.GetStat(StatType.Charisma)
                });
            }

            using (var writer = new StreamWriter(adventurersPath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(adventurersData);
            }
        }

        private SaveData? LoadGuildData()
        {
            var guildPath = Path.Combine(_savePath, _guildsFile);

            using (var reader = new StreamReader(guildPath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Read();
                csv.ReadHeader();

                if (!csv.Read()) return null;

                return new SaveData
                {
                    GuildCoins = csv.GetField<int>("Coins"),
                    GuildReputation = csv.GetField<int>("Reputation"),
                    GuildLevel = csv.GetField<int>("GuildLevel")
                };
            }
        }

        private List<AdventurerData> LoadAdventurerData()
        {
            var adventurersPath = Path.Combine(_savePath, _adventurersFile);

            if (!File.Exists(adventurersPath))
                return new List<AdventurerData>();

            using (var reader = new StreamReader(adventurersPath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                return csv.GetRecords<AdventurerData>().ToList();
            }
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

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("✓ Save files deleted.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Error deleting save: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}
