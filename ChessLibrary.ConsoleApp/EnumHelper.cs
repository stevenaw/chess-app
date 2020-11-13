using ChessLibrary.Models;
using System;
using System.Collections.Generic;

namespace ChessLibrary.ConsoleApp
{
    public static class EnumHelper
    {
        private static readonly Dictionary<AttackState, string> AttackStrings = BuildEnumStrings<AttackState>();
        private static readonly Dictionary<ErrorCondition, string> ErrorStrings = BuildEnumStrings<ErrorCondition>();

        private static Dictionary<T, string> BuildEnumStrings<T>() where T : struct, Enum
        {
            var dict = new Dictionary<T, string>();

            foreach (var value in Enum.GetValues<T>())
                dict[value] = Enum.GetName(value)!;

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
