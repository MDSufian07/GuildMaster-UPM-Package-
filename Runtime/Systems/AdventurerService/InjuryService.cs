using GuildMaster.Core.Entities;
using GuildMaster.Core.Enums;
using GuildMaster.Systems.Insurance;

namespace GuildMaster.Systems.AdventurerService
{
    public static class InjuryService
    {
        public static void ApplyInjury(Adventurer adv, int recoveryDays)
        {
            // Apply insurance benefits
            float reductionMultiplier = 1.0f;
            
            if (adv.InsuranceType == InsuranceType.Basic)
            {
                reductionMultiplier = 0.5f; // 50% reduction
            }
            else if (adv.InsuranceType == InsuranceType.Premium)
            {
                reductionMultiplier = 0.25f; // 75% reduction
            }
            
            int adjustedRecoveryDays = (int)Math.Ceiling(recoveryDays * reductionMultiplier);
            
            adv.GetType().GetProperty("IsInjured")
                ?.SetValue(adv, true);

            adv.GetType().GetProperty("RecoveryTime")
                ?.SetValue(adv, adjustedRecoveryDays);

            adv.GetType().GetProperty("Status")
                ?.SetValue(adv, AdventurerStatus.Recovering);
                
            // Show insurance benefit message
            if (adv.InsuranceType != InsuranceType.None && recoveryDays != adjustedRecoveryDays)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"   🛡️ {adv.Name}'s {adv.InsuranceType} Insurance reduced recovery: {recoveryDays} → {adjustedRecoveryDays} days");
                Console.ResetColor();
            }
        }

        public static void HealInstantly(Adventurer adv)
        {
            adv.GetType().GetProperty("IsInjured")
                ?.SetValue(adv, false);

            adv.GetType().GetProperty("RecoveryTime")
                ?.SetValue(adv, 0);

            adv.GetType().GetProperty("Status")
                ?.SetValue(adv, AdventurerStatus.Available);
        }
    }
}






