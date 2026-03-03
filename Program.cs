using GuildMaster.Core.Entities;
using GuildMaster.Core.ValueObjects;
using GuildMaster.Systems.TasksSystem;
using GuildMaster.Systems.Resolution;
using GuildMaster.Systems.Recruitment;
using GuildMaster.Systems.SaveManager;
using GuildMaster.Systems.AdventurerService;
using GuildMaster.Systems.Insurance;

// Initialize save manager
var saveManager = new SaveManagerService();

// Try to load existing game
var guild = saveManager.LoadGame();

// If no save exists, create new game
if (guild == null)
{
    guild = new Guild(initialCoins: 500, maxAdventurers: 3);
}

var generator = new TaskGenerator();
var recruitmentPool = new RecruitmentPool();
List<RecruitmentCandidate>? currentRecruitmentOffers = null;
int lastLevelOffered = guild.GuildLevel - 1;

var continueLoop = true;

while (continueLoop)
{
    try { Console.Clear(); } catch { }
    Console.WriteLine("╔════════════════════════════════════════════════════════╗");
    Console.WriteLine("║         GUILD MASTER - ADVENTURER TASK SYSTEM          ║");
    Console.WriteLine("╚════════════════════════════════════════════════════════╝");
    Console.WriteLine();

    // Display Guild Status
    Console.WriteLine("=== GUILD STATUS ===");
    Console.WriteLine($"💰 Coins: {guild.Coins}");
    Console.WriteLine($"⭐ Reputation: {guild.Reputation}");
    Console.WriteLine($"🏆 Guild Level: {guild.GuildLevel}");
    Console.WriteLine($"👥 Adventurers: {guild.Adventurers.Count}/{guild.MaxAdventurers} slots");
    
    int injuredCount = guild.GetInjuredAdventurerCount();
    int totalAdventurers = guild.Adventurers.Count;
    int injuredPercentage = totalAdventurers > 0 ? (injuredCount * 100) / totalAdventurers : 0;
    Console.WriteLine($"✓ Available: {guild.GetAvailableAdventurerCount()} | 🤕 Injured: {injuredCount} ({injuredPercentage}%)");
    Console.WriteLine();

    // Check if new recruitment offers available
    if (guild.GuildLevel > lastLevelOffered)
    {
        currentRecruitmentOffers = recruitmentPool.GenerateCandidates(guild.GuildLevel, 3);
        lastLevelOffered = guild.GuildLevel;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("🎁 NEW RECRUITMENT OFFERS AVAILABLE!");
        Console.ResetColor();
        Console.WriteLine();
    }

    // Display Main Menu
    Console.WriteLine("=== MAIN MENU ===");
    if (guild.Adventurers.Count == 0)
    {
        Console.WriteLine("⚠️ You have no adventurers! You must recruit first.");
        Console.WriteLine("1. Recruit Adventurers");
        Console.WriteLine("5. Save Game");
        Console.WriteLine("6. Exit Game");
    }
    else
    {
        Console.WriteLine("1. Recruit Adventurers");
        Console.WriteLine("2. View Adventurers");
        Console.WriteLine("3. Start Task");
        Console.WriteLine("4. Manage Insurance");
        Console.WriteLine("5. Save Game");
        Console.WriteLine("6. Exit Game");
    }
    Console.WriteLine();

    Console.Write("Choose option: ");
    string? menuChoice = Console.ReadLine();

    if (guild.Adventurers.Count == 0 && menuChoice != "1" && menuChoice != "5" && menuChoice != "6")
    {
        Console.WriteLine("You must recruit adventurers first!");
        System.Threading.Thread.Sleep(2000);
        continue;
    }

    switch (menuChoice)
    {
        case "1": // Recruitment
            try { Console.Clear(); } catch { }
            Console.WriteLine("╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                 RECRUITMENT CENTER                     ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            if (currentRecruitmentOffers == null || currentRecruitmentOffers.Count == 0)
            {
                Console.WriteLine("No recruitment offers available. Level up your guild to unlock new candidates!");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                break;
            }

            if (!guild.CanRecruit())
            {
                Console.WriteLine("❌ Guild is full! All 3 adventurer slots are occupied.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                break;
            }

            Console.WriteLine($"Available Coins: {guild.Coins}");
            Console.WriteLine($"Available Slots: {guild.MaxAdventurers - guild.Adventurers.Count}");
            Console.WriteLine();

            // Display candidates
            for (int i = 0; i < currentRecruitmentOffers.Count; i++)
            {
                var candidate = currentRecruitmentOffers[i];
                Console.WriteLine($"[{i + 1}] {candidate.Adventurer.Name}");
                Console.WriteLine($"    Skill Level: {candidate.SkillLevel}");
                Console.WriteLine($"    Specialty: {candidate.Adventurer.Specialty}");
                Console.WriteLine($"    Cost: {candidate.RecruitmentCost} coins");
                Console.WriteLine($"    Stats: Combat:{candidate.Adventurer.GetStat(StatType.Combat)} " +
                                  $"Defense:{candidate.Adventurer.GetStat(StatType.Defense)} " +
                                  $"Intel:{candidate.Adventurer.GetStat(StatType.Intelligence)} " +
                                  $"Agility:{candidate.Adventurer.GetStat(StatType.Agility)} " +
                                  $"Charisma:{candidate.Adventurer.GetStat(StatType.Charisma)}");
                Console.WriteLine();
            }

            Console.WriteLine("0. Cancel");
            Console.Write("Select adventurer to recruit: ");
            
            if (int.TryParse(Console.ReadLine(), out int recruitChoice) && recruitChoice > 0 && recruitChoice <= currentRecruitmentOffers.Count)
            {
                var selectedCandidate = currentRecruitmentOffers[recruitChoice - 1];
                
                if (guild.TryRecruitAdventurer(selectedCandidate.Adventurer, selectedCandidate.RecruitmentCost))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\n✓ Successfully recruited {selectedCandidate.Adventurer.Name}!");
                    Console.ResetColor();
                    currentRecruitmentOffers.RemoveAt(recruitChoice - 1);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n✗ Not enough coins! Need {selectedCandidate.RecruitmentCost} but have {guild.Coins}.");
                    Console.ResetColor();
                }
                
                System.Threading.Thread.Sleep(2000);
            }
            break;

        case "2": // View Adventurers
            if (guild.Adventurers.Count == 0)
            {
                continueLoop = false;
                break;
            }

            try { Console.Clear(); } catch { }
            Console.WriteLine("╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                   YOUR ADVENTURERS                     ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            for (int i = 0; i < guild.Adventurers.Count; i++)
            {
                var adv = guild.Adventurers[i];
                int xpRequired = ExperienceService.GetXpRequiredForNextLevel(adv.Level);
                
                Console.WriteLine($"{i + 1}. {adv.Name} (Lv.{adv.Level})");
                Console.WriteLine($"   XP: {adv.XP}/{xpRequired}");
                Console.WriteLine($"   Specialty: {adv.Specialty} | Status: {adv.Status}");
                
                // Show insurance status with cost
                if (adv.InsuranceType != InsuranceType.None)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    int insuranceCost = InsuranceService.GetInsuranceCost(adv.InsuranceType);
                    Console.WriteLine($"   🛡️ Insurance: {adv.InsuranceType} ({insuranceCost} coins)");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"   🛡️ Insurance: None");
                    Console.ResetColor();
                }
                
                if (adv.IsInjured)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"   🤕 INJURED - Recovery Time: {adv.RecoveryTime} days");
                    Console.ResetColor();
                }
                
                Console.WriteLine($"   Stats: Combat:{adv.GetStat(StatType.Combat)} " +
                                  $"Defense:{adv.GetStat(StatType.Defense)} " +
                                  $"Intel:{adv.GetStat(StatType.Intelligence)} " +
                                  $"Agility:{adv.GetStat(StatType.Agility)} " +
                                  $"Charisma:{adv.GetStat(StatType.Charisma)}");
                Console.WriteLine();
            }

            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
            break;

        case "3": // Start Task
            try { Console.Clear(); } catch { }
            
            var availableAdventurers = guild.Adventurers.Where(a => a.Status == GuildMaster.Core.Enums.AdventurerStatus.Available && !a.IsInjured)
                .ToList();

            if (availableAdventurers.Count == 0)
            {
                Console.WriteLine("❌ No available adventurers! All are injured or unavailable.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                break;
            }
            
            int maxDifficulty = 3 + guild.GuildLevel;
            
            var availableTasks = new List<GuildMaster.Core.Entities.Task>();
            while (availableTasks.Count < 3)
            {
                var task = generator.GenerateRandomTask();
                if (task.Difficulty <= maxDifficulty)
                {
                    availableTasks.Add(task);
                }
            }

            // Display Available Tasks
            Console.WriteLine("╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║             AVAILABLE TASKS - SELECT ONE               ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝");
            for (int i = 0; i < availableTasks.Count; i++)
            {
                Console.WriteLine($"\n[Task {i + 1}] {availableTasks[i].Name}");
                Console.WriteLine($"  Difficulty: {availableTasks[i].Difficulty}");
                Console.WriteLine($"  Reward: {availableTasks[i].BaseReward} coins");
                Console.WriteLine($"  XP: {availableTasks[i].BaseXP}");
                Console.WriteLine($"  Required Avg Stats: {availableTasks[i].GetRequiredAverage():F1}");
                Console.Write($"  Required Stats: ");
                Console.WriteLine(string.Join(", ", availableTasks[i].RequiredStats.Select(s => $"{s.Key}:{s.Value}")));
            }
            Console.WriteLine();

            // Task Selection
            Console.Write("Enter task number (1-3): ");
            int taskChoice = 0;
            while (!int.TryParse(Console.ReadLine(), out taskChoice) || taskChoice < 1 || taskChoice > 3)
            {
                Console.Write("Invalid input. Enter task number (1-3): ");
            }
            var selectedTask = availableTasks[taskChoice - 1];

            Console.WriteLine();
            Console.WriteLine("╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║         SELECT ADVENTURER(S) FOR THIS TASK             ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝");
            for (int i = 0; i < availableAdventurers.Count; i++)
            {
                var adv = availableAdventurers[i];
                Console.WriteLine($"{i + 1}. {adv.Name} (Cost Per Mission: {adv.CostPerMission} coins)");
                Console.WriteLine($"   Combat: {adv.GetStat(StatType.Combat)}, Defense: {adv.GetStat(StatType.Defense)}, " +
                                  $"Intel: {adv.GetStat(StatType.Intelligence)}, Agility: {adv.GetStat(StatType.Agility)}, " +
                                  $"Charisma: {adv.GetStat(StatType.Charisma)}");
            }
            Console.WriteLine();

            // Adventurer Selection
            var selectedAdventurers = new List<Adventurer>();
            Console.Write("Enter adventurer numbers separated by comma (e.g., 1,2 or 1): ");
            string? adventurerInput = Console.ReadLine();
            if (!string.IsNullOrEmpty(adventurerInput))
            {
                var choices = adventurerInput.Split(',');
                foreach (var choice in choices)
                {
                    if (int.TryParse(choice.Trim(), out int advChoice) && advChoice > 0 && advChoice <= availableAdventurers.Count)
                    {
                        selectedAdventurers.Add(availableAdventurers[advChoice - 1]);
                    }
                }
            }

            if (selectedAdventurers.Count == 0)
            {
                Console.WriteLine("No valid adventurers selected. Returning to menu.");
                System.Threading.Thread.Sleep(2000);
                break;
            }

            // Calculate total cost for adventurers to attend this task
            int totalTaskCost = selectedAdventurers.Sum(a => a.CostPerMission);

            // Check if guild has enough coins
            if (guild.Coins < totalTaskCost)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ Not enough coins! Need {totalTaskCost} coins to send {selectedAdventurers.Count} adventurer(s), but only have {guild.Coins}.");
                Console.ResetColor();
                Console.WriteLine("Press Enter to return to menu...");
                Console.ReadLine();
                break;
            }

            Console.WriteLine();
            Console.WriteLine("╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║               RUNNING TASK CALCULATION...              ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝");
            Console.WriteLine($"Task: {selectedTask.Name} (Difficulty: {selectedTask.Difficulty})");
            Console.WriteLine($"Team: {string.Join(", ", selectedAdventurers.Select(a => a.Name))}");
            Console.WriteLine($"Mission Cost: {totalTaskCost} coins");
            Console.WriteLine();

            // Deduct cost from guild coins
            guild.AddCoins(-totalTaskCost);

            // Simulate background calculation
            System.Threading.Thread.Sleep(1500);

            // Resolve Task
            ITaskResolver resolver = new TaskResolver();
            var result = resolver.ResolveTask(selectedTask, selectedAdventurers);

            // Update Guild
            guild.AddCoins(result.CoinsEarned);
            guild.AddReputation(result.ReputationChange);

            // Apply XP gains to adventurers and track level-ups
            var levelUpInfo = new List<(Adventurer adv, int oldLevel, Dictionary<StatType, int> oldStats)>();
            
            foreach (var xpGain in result.XPGains)
            {
                var adventurer = xpGain.Key;
                int oldLevel = adventurer.Level;
                
                // Store old stats before adding XP
                var oldStats = new Dictionary<StatType, int>();
                foreach (var stat in adventurer.Stats)
                {
                    oldStats[stat.Key] = stat.Value;
                }
                
                // Apply XP (this may trigger level up)
                ExperienceService.AddXP(adventurer, xpGain.Value);
                
                // Check if leveled up
                if (adventurer.Level > oldLevel)
                {
                    levelUpInfo.Add((adventurer, oldLevel, oldStats));
                }
            }

            Console.WriteLine("╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    TASK RESULTS                        ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            string resultStatus = result.Success ? "✓ SUCCESS" : "✗ FAILURE";
            Console.ForegroundColor = result.Success ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine($"Result: {resultStatus}");
            Console.ResetColor();

            Console.WriteLine($"Success Chance: {result.SuccessChance:F2}%");
            Console.WriteLine($"Mission Cost: -{totalTaskCost} coins");
            Console.WriteLine($"Coins Earned: +{result.CoinsEarned} coins");
            Console.WriteLine($"Net Coins: {result.CoinsEarned - totalTaskCost} coins");
            Console.WriteLine($"Reputation Change: {(result.ReputationChange > 0 ? "+" : "")}{result.ReputationChange}");
            Console.WriteLine();

            if (result.XPGains.Count > 0)
            {
                Console.WriteLine("Experience Gained:");
                foreach (var xpGain in result.XPGains)
                {
                    int xpRequired = ExperienceService.GetXpRequiredForNextLevel(xpGain.Key.Level);
                    Console.WriteLine($"  {xpGain.Key.Name}: +{xpGain.Value} XP (Total: {xpGain.Key.XP}/{xpRequired})");
                }
            }

            // Display level-up information with stat changes
            if (levelUpInfo.Count > 0)
            {
                Console.WriteLine();
                foreach (var info in levelUpInfo)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"╔════════════════════════════════════════════════════════╗");
                    Console.WriteLine($"║              ⭐ LEVEL UP! ⭐                            ║");
                    Console.WriteLine($"╚════════════════════════════════════════════════════════╝");
                    Console.ResetColor();
                    
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"🎖️  {info.adv.Name} reached level {info.adv.Level}!");
                    Console.ResetColor();
                    
                    Console.WriteLine($"\n📊 Stat Changes:");
                    foreach (var stat in info.adv.Stats)
                    {
                        int oldValue = info.oldStats[stat.Key];
                        int newValue = stat.Value;
                        
                        if (oldValue != newValue)
                        {
                            int increase = newValue - oldValue;
                            Console.Write($"   • {stat.Key}: {oldValue} ");
                            
                            // Highlight the increase in green
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write($"+{increase}");
                            Console.ResetColor();
                            
                            // Show arrow and new value in cyan
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($" → {newValue}");
                            Console.ResetColor();
                        }
                    }
                    
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\n   ✓ Next level requires: {ExperienceService.GetXpRequiredForNextLevel(info.adv.Level)} XP");
                    Console.ResetColor();
                    Console.WriteLine();
                }
            }

            // Display injured adventurers
            var injuredAdventurers = selectedAdventurers.Where(a => a.IsInjured).ToList();
            if (injuredAdventurers.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n🤕 Injured Adventurers:");
                foreach (var injured in injuredAdventurers)
                {
                    Console.WriteLine($"  ⚠️ {injured.Name} - Recovery Time: {injured.RecoveryTime} days");
                }
                Console.ResetColor();
            }

            if (result.DeadAdventurers.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("\n💀 Fallen Adventurers:");
                foreach (var fallen in result.DeadAdventurers)
                {
                    Console.WriteLine($"  ✗ {fallen.Name} has died in battle...");
                    guild.Adventurers.Remove(fallen);
                }
                Console.ResetColor();
            }

            Console.WriteLine();
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
            break;

        case "4": // Manage Insurance
            try { Console.Clear(); } catch { }
            Console.WriteLine("╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                 INSURANCE MANAGEMENT                   ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            
            Console.WriteLine($"Guild Coins: {guild.Coins}");
            Console.WriteLine();
            Console.WriteLine("Available Insurance Plans:");
            Console.WriteLine($"1. Basic Insurance - {InsuranceService.GetInsuranceCost(InsuranceType.Basic)} coins");
            Console.WriteLine($"   {InsuranceService.GetInsuranceBenefits(InsuranceType.Basic)}");
            Console.WriteLine();
            Console.WriteLine($"2. Premium Insurance - {InsuranceService.GetInsuranceCost(InsuranceType.Premium)} coins");
            Console.WriteLine($"   {InsuranceService.GetInsuranceBenefits(InsuranceType.Premium)}");
            Console.WriteLine();
            
            Console.WriteLine("Your Adventurers:");
            for (int i = 0; i < guild.Adventurers.Count; i++)
            {
                var adv = guild.Adventurers[i];
                string insuranceStatus;
                
                if (adv.InsuranceType == InsuranceType.None)
                {
                    insuranceStatus = "No Insurance";
                }
                else
                {
                    int cost = InsuranceService.GetInsuranceCost(adv.InsuranceType);
                    insuranceStatus = $"{adv.InsuranceType} Insurance 🛡️ ({cost} coins)";
                }
                    
                Console.WriteLine($"{i + 1}. {adv.Name} (Lv.{adv.Level}) - {insuranceStatus}");
            }
            Console.WriteLine();
            Console.WriteLine("0. Back to Menu");
            Console.WriteLine();
            
            Console.Write("Select adventurer to manage insurance: ");
            if (int.TryParse(Console.ReadLine(), out int insuranceChoice) && insuranceChoice > 0 && insuranceChoice <= guild.Adventurers.Count)
            {
                var selectedAdv = guild.Adventurers[insuranceChoice - 1];
                
                try { Console.Clear(); } catch { }
                Console.WriteLine($"Managing Insurance for: {selectedAdv.Name}");
                Console.WriteLine($"Current Insurance: {(selectedAdv.InsuranceType == InsuranceType.None ? "None" : selectedAdv.InsuranceType.ToString())}");
                Console.WriteLine();
                Console.WriteLine("1. Purchase Basic Insurance (50 coins)");
                Console.WriteLine("2. Purchase Premium Insurance (150 coins)");
                Console.WriteLine("3. Cancel Insurance (Free)");
                Console.WriteLine("0. Back");
                Console.WriteLine();
                
                Console.Write("Choose option: ");
                string? insuranceAction = Console.ReadLine();
                
                switch (insuranceAction)
                {
                    case "1":
                        if (InsuranceService.PurchaseInsurance(selectedAdv, guild, InsuranceType.Basic))
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"\n✓ {selectedAdv.Name} now has Basic Insurance!");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"\n✗ Not enough coins! Need {InsuranceService.GetInsuranceCost(InsuranceType.Basic)} coins.");
                            Console.ResetColor();
                        }
                        System.Threading.Thread.Sleep(2000);
                        break;
                        
                    case "2":
                        if (InsuranceService.PurchaseInsurance(selectedAdv, guild, InsuranceType.Premium))
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"\n✓ {selectedAdv.Name} now has Premium Insurance!");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"\n✗ Not enough coins! Need {InsuranceService.GetInsuranceCost(InsuranceType.Premium)} coins.");
                            Console.ResetColor();
                        }
                        System.Threading.Thread.Sleep(2000);
                        break;
                        
                    case "3":
                        InsuranceService.CancelInsurance(selectedAdv);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"\n✓ {selectedAdv.Name}'s insurance has been cancelled.");
                        Console.ResetColor();
                        System.Threading.Thread.Sleep(2000);
                        break;
                }
            }
            break;

        case "5": // Save Game
            try { Console.Clear(); } catch { }
            saveManager.SaveGame(guild);
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
            break;

        case "6": // Exit Game
            saveManager.SaveGame(guild);
            continueLoop = false;
            break;

        default:
            Console.WriteLine("Invalid option. Press Enter to continue...");
            Console.ReadLine();
            break;
    }
}

Console.WriteLine();
Console.WriteLine("Thank you for playing Guild Master!");
Console.WriteLine("Press Enter to exit...");
Console.ReadLine();

