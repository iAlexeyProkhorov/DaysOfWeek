using System.Collections.Generic;
using System.Linq;

namespace Nop.Plugin.DiscountRules.DaysOfWeek.Extentions
{
    public static class SettingsExtentions
    {
        /// <summary>
        /// Parse separated string row to list of integer values
        /// </summary>
        /// <param name="separated">Separated row</param>
        /// <param name="separator">Separator</param>
        /// <returns>List of id numbers</returns>
        public static IList<int> ParseSeparatedNumbers(this string separated, char separator = ',')
        {
            if (string.IsNullOrEmpty(separated))
                return new List<int>();

            return separated.Split(separator).Select(x =>
            {
                int result = 0;
                int.TryParse(x, out result);
                return result;
            }).ToList();
        }
    }
}
