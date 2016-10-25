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
        private static readonly Regex Function = new Regex(@"\G\s*FUNCTION\b", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex Procedure = new Regex(@"\G\s*PROCEDURE\b", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        
        private static readonly Regex Type = new Regex(@"\G\s+TYPE\b", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex IsNull = new Regex(@"\G\s*IS\s+NULL\b", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex As = new Regex(@"\G\s*AS\b", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex Is = new Regex(@"\G\s*IS\b", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex Alter = new Regex(@"\G\s*ALTER\b", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex Case = new Regex(@"\G\s*CASE\b", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex End = new Regex(@"\G\s*END\b", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex EndIF = new Regex(@"\G\s*END\s*IF\b", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex CursorDef = new Regex(@"\G\s*CURSOR\b", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
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
            bool packageOpen = false;
            bool openAsOrIsStatement = false;
            bool functionOrProcDef = false;


            foreach (var token in tokens)
            {
                if (Begin.IsMatch(token))
                {
                    nestingLevel++;
                    buffer.Append(token);
                    lastTokenWasEnd = false;
                    if (openAsOrIsStatement)
                    {
                        openAsOrIsStatement = false;
                    }
                }
                else if (Case.IsMatch(token)) // case statements have explicit 'end'
                {
                    nestingLevel++;
                    buffer.Append(token);
                    lastTokenWasEnd = false;
                }
                else if (CursorDef.IsMatch(token))
                {
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
                    packageOpen = true;
                    buffer.Append(token);
                    lastTokenWasEnd = false;
                }
                else if (Alter.IsMatch(token))
                {
                    //alterStatement = true;
                    buffer.Append(token);
                    lastTokenWasEnd = false;
                }
                else if (IsNull.IsMatch(token)) // do not increase nesting.
                {
                    buffer.Append(token);
                    lastTokenWasEnd = false;
                }
                else if (Function.IsMatch(token) || Procedure.IsMatch(token))
                {
                    functionOrProcDef = true;
                    buffer.Append(token);
                    lastTokenWasEnd = false;
                }
                else if (As.IsMatch(token) || Is.IsMatch(token))
                {
                    if (packageOpen)
                    { // no need for conditional?
                        nestingLevel++;
                        packageOpen = false;
                    } else {
                        // function or procedure definition, ignore statement delimiters until next BEGIN...
                        // but type definitions, comments should not
                        if (functionOrProcDef)
                        {
                            openAsOrIsStatement = true;
                        }
                    }

                    buffer.Append(token);
                    lastTokenWasEnd = false;
                }
                else if (nestingLevel <= 0 && (!openAsOrIsStatement) && StatementDelimiter.IsMatch(token) && ((!lastTokenWasEnd) || (lastTokenWasEnd && (!Semicolon.IsMatch(token) || (packageDef)))))
                {
                    
                    var script = buffer.ToString().Trim();
                    
                    if (script != string.Empty)
                        yield return script + ((packageDef||functionOrProcDef) ? ";" : ""); // package definitions do not run without the ending semi

                    functionOrProcDef = false;
                    openAsOrIsStatement = false;
                    buffer.Length = 0;
                    lastTokenWasEnd = false;
                    packageDef = false;
                    createDef = false;
                    packageOpen = false;
                }
                else
                {
                    buffer.Append(token);
                    lastTokenWasEnd = false;
                }
                System.Console.WriteLine(nestingLevel + " token:"+ token);
            }

            var lastScript = buffer.ToString().Trim();
            if (lastScript != string.Empty && !lastScript.Equals("/") && !lastScript.Equals(";")) 
                yield return lastScript;
        }
    }
}
