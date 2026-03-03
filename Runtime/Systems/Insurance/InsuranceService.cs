using GuildMaster.Core.Entities;
using GuildMaster.Core.Enums;

namespace GuildMaster.Systems.Insurance
{
    public abstract class InsuranceService
    {
        // Insurance costs and benefits
        private const int BasicInsuranceCost = 50;
        private const int PremiumInsuranceCost = 150;
        
        // Basic: Reduces recovery time by 50%
        // Premium: Prevents death + reduces recovery time by 75%
        
        public static bool PurchaseInsurance(Adventurer adventurer, Guild guild, InsuranceType type)
        {
            int cost = type == InsuranceType.Basic ? BasicInsuranceCost : PremiumInsuranceCost;
            
            if (guild.Coins < cost)
            {
                return false;
            }
            
            guild.AddCoins(-cost);
            
            // Set insurance using reflection
            adventurer.GetType().GetProperty("InsuranceType")?.SetValue(adventurer, type);
            
            return true;
        }
        
        public static void CancelInsurance(Adventurer adventurer)
        {
            adventurer.GetType().GetProperty("InsuranceType")?.SetValue(adventurer, InsuranceType.None);
        }
        
        public static int GetInsuranceCost(InsuranceType type)
        {
            return type switch
            {
                InsuranceType.Basic => BasicInsuranceCost,
                InsuranceType.Premium => PremiumInsuranceCost,
                _ => 0
            };
        }
        
        public static string GetInsuranceBenefits(InsuranceType type)
        {
            return type switch
            {
                InsuranceType.Basic => "Reduces injury recovery time by 50%",
                InsuranceType.Premium => "Prevents death & reduces recovery by 75%",
                _ => "No insurance coverage"
            };
        }
    }
}
