using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Inedo.BuildMasterExtensions.Oracle
{
    internal static class ScriptSplitter
    {
        private static readonly Regex Begin = new Regex(@"\G\s*BEGIN\b", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex Create = new Regex(@"\G\s*CREATE\b", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex Package = new Regex(@"\G\s*PACKAGE\b", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex As = new Regex(@"\G\s*AS\b", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex Case = new Regex(@"\G\s*CASE\b", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex End = new Regex(@"\G\s*END\b", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex EndIF = new Regex(@"\G\s*END\s*IF\b", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex EndLoop = new Regex(@"\G\s*END\s*LOOP\b", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex Semicolon = new Regex(@"\G(\s*;)", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture);
        private static readonly Regex StatementDelimiter = new Regex(@"\G(\s*;)|(\s*/\s*\n)", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture);

        public static IEnumerable<string> Process(string scriptText)
        {
            var buffer = new StringBuilder();
            var tokens = Tokenizer.GetTokens(scriptText.Replace("\r", string.Empty));
            int nestingLevel = 0;
            bool lastTokenWasEnd = false;
            bool createDef = false;
            bool packageDef = false;

            foreach (var token in tokens)
            {
                if (Begin.IsMatch(token))
                {
                    nestingLevel++;
                    buffer.Append(token);
                    lastTokenWasEnd = false;
                }
                else if (Case.IsMatch(token)) // case statements have explicit 'end'
                {
                    nestingLevel++;
                    buffer.Append(token);
                    lastTokenWasEnd = false;
                }
                else if (EndIF.IsMatch(token) || EndLoop.IsMatch(token)) // end if and end loop (in packages) should not change the nesting since we are not incrementing on IF or LOOP
                {
                    buffer.Append(token);
                    lastTokenWasEnd = false;
                }
                else if (End.IsMatch(token))
                {
                    nestingLevel--;
                    buffer.Append(token);
                    lastTokenWasEnd = true;
                }
                else if (Create.IsMatch(token))
                {
                    createDef = true;
                    buffer.Append(token);
                    lastTokenWasEnd = false;
                }
                else if (Package.IsMatch(token))
                {
                    packageDef = createDef;
                    buffer.Append(token);
                    lastTokenWasEnd = false;
                }
                else if (As.IsMatch(token))
                {
                    if (packageDef)
                        nestingLevel++;

                    buffer.Append(token);
                    lastTokenWasEnd = false;
                }
                else if (nestingLevel <= 0 && StatementDelimiter.IsMatch(token) && ((!lastTokenWasEnd) || (lastTokenWasEnd && (!Semicolon.IsMatch(token) || (packageDef)))))
                {
                    var script = buffer.ToString().Trim();
                    if (script != string.Empty)
                        yield return script + (packageDef ? ";" : ""); // package definitions do not run without the ending semi

                    buffer.Length = 0;
                    lastTokenWasEnd = false;
                    packageDef = false;
                    createDef = false;
                }
                else
                {
                    buffer.Append(token);
                    lastTokenWasEnd = false;
                }
            }

            var lastScript = buffer.ToString().Trim();
            if (lastScript != string.Empty)
                yield return lastScript;
        }
    }
}
