using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Chello.Core;

namespace Chello.ScrumExtensions
{
    public static class ChelloScrumExtensions
    {
        private static Regex _pointsRegex = new Regex("^\\((?<points>[0-9]+)\\)", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace);

        public static bool HasPoints(this Card card)
        {
            Match match = _pointsRegex.Match(card.Name);

            if (match.Success && match.Groups["points"] != null && match.Groups["points"].Success)
            {
                return true;
            }

            return false;
        }

        public static int Points(this Card card)
        {
            Match match = _pointsRegex.Match(card.Name);

            if (match.Success && match.Groups["points"] != null && match.Groups["points"].Success)
            {
                return int.Parse(match.Groups["points"].Value);
            }

            return -1;
        }
    }
}
