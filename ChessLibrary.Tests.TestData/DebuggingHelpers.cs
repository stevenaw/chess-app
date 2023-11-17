﻿using System.Text;

namespace ChessLibrary.Tests.TestHelpers
{
    public static class DebuggingHelpers
    {
        public static string RenderBitboard(ulong mask)
        {
            var sb = new StringBuilder(64 + (Environment.NewLine.Length * 8));
            for(var i = 7; i >= 0; i--)
            {
                for (var j = 0; j < 8; j++)
                {
                    var bit = i * 8 + j;
                    var masked = (1UL << bit) & mask;
                    sb.Append(masked == 0 ? '0' : '1');
                }

                sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }
    }
}
