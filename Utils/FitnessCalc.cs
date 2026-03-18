namespace GymAppFresh.Utils
{
    public class FitnessCalc
    {

        public static double? Bmi(decimal? height, decimal? weight)
        {
            if (weight.HasValue && height.HasValue)
            {
                var kgd = (double)weight.Value;
                var md = (double)height.Value / 100.0;
                return kgd / (md * md);
            }
            return null;
        }

        public static int? Age(DateTime? birthDate)
        {
            if (birthDate.HasValue)
            {
                return DateTime.Now.Year - birthDate.Value.Year;
            }
            return null;
        }


    }
}