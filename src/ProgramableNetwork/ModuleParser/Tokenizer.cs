using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace ProgramableNetwork.Python
{
    public class Tokenizer
    {
        //lang=regex
        private const string paren = @"(?<lparen>\()|(?<rparen>\))|(?<llist>\[)|(?<rlist>\])|(?<ldict>{)|(?<rdict>})";
        //lang=regex
        private const string comp = @"(?<eq>==)|(?<neq>!=)|(?<lre><=)|(?<gre><=)|(?<lr><)|(?<gr>>)|(?<isp>is)|(?<not>not)";
        //lang=regex
        private const string oper = @"(?<plus>\+)|(?<minus>-)|(?<power>\*\*)|(?<mul>\*)|(?<divint>\\\\)|(?<div>\\)|(?<mod>%)|(?<dot>\.)|(?<next>,)|(?<set>=)|(?<and>and)|(?<or>or)";
        //lang=regex
        private const string data = @"(?<str>""[^""]+""|'[^']+')|(?<number>\d+(?:.\d+)?)|(?<none>None)|(?<btrue>True)|(?<bfalse>False)";
        //lang=regex
        private const string keywords = @"(?<from>from)|(?<import>import)|(?<def>def)|(?<classp>class)|(?<ifp>if)|(?<elif>elif)|(?<elsep>else)|(?<name>[a-zA-Z_]\w*)";
        //lang=regex
        private const string indentation = @"(?<block>:)|(?<space>\t| )|(?<rest>.)";

        private static readonly Regex combined = new Regex(
            string.Join("|", paren, comp, oper, data, keywords, indentation)
            , RegexOptions.Compiled);

        public static Token[] Parse(string file)
        {
            List<Token> tokens = new List<Token>();
            string[] lines = File.ReadAllLines(file);
            FileInfo fileInfo = new FileInfo(file);

            for (int i = 0; i < lines.Length; i++)
            {
                MatchCollection matchCollection = combined.Matches(lines[i]);
                int end = 0;
                foreach (Match match in matchCollection)
                {
                    bool found = false;
                    foreach (Group token in match.Groups)
                    {
                        if (token.Name == "0") continue;
                        if (token.Name == "rest") continue;
                        if (found = token.Success)
                        {
                            tokens.Add(new Token(fileInfo, i + 1, token.Index+1, token.Length, (PythonTokens)Enum.Parse(typeof(PythonTokens), token.Name), token.Value));
                            end = token.Index + token.Length;
                            break;
                        }
                    }
                    if (!found)
                    {
                        throw new NotImplementedException($"Missing type of token: {match.Value}");
                    }
                }
                if ((i + 1) < lines.Length)
                    tokens.Add(new Token(fileInfo, i, end, 1, PythonTokens.newline, "\n"));
            }

            return tokens.ToArray();
        }
    }
}