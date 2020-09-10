using ChessLibrary.Models;
using System;
using System.Collections.Generic;

namespace ChessLibrary.ConsoleApp
{
    public static class EnumHelper
    {
        private static Dictionary<AttackState, string> AttackStrings = BuildEnumStrings<AttackState>();
        private static Dictionary<ErrorCondition, string> ErrorStrings = BuildEnumStrings<ErrorCondition>();

        private static Dictionary<T, string> BuildEnumStrings<T>() where T : Enum
        {
            var dict = new Dictionary<T, string>();

            foreach (var value in (T[])Enum.GetValues(typeof(T)))
                dict[value] = value.ToString();

            return dict;
        }

        public static string GetAttackString(AttackState @enum)
        {
            return AttackStrings[@enum];
        }

        public static string GetErrorString(ErrorCondition @enum)
        {
            return ErrorStrings[@enum];
        }
    }
}
