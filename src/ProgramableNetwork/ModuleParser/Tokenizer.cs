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
        private const string comp = @"(?<eq>==)|(?<neq>!=)|(?<lre><=)|(?<gre>>=)|(?<shiftl><<)|(?<shiftr>>>)|(?<lr><)|(?<gr>>)|(?<isp>is)|(?<not>not)|(?<bitor>\|)|(?<bitxor>\^)|(?<bitand>&)";
        //lang=regex
        private const string oper = @"(?<plus>\+)|(?<minus>-)|(?<power>\*\*)|(?<mul>\*)|(?<divint>//)|(?<div>/)|(?<mod>%)|(?<dot>\.)|(?<next>,)|(?<set>=)|(?<invert>~)|(?<semicolon>;)";
        //lang=regex
        private const string data = @"(?<str>""[^""]+""|'[^']+')|(?<number>\d+(?:.\d+)?)|(?<name>[a-zA-Z_]\w*)";
        //lang=regex
        private const string keywordsList = @"^(?:(?<none>None)|(?<btrue>True)|(?<bfalse>False)|(?<and>and)|(?<or>or)|(?<ink>in)|(?<from>from)|(?<import>import)|(?<def>def)|(?<classp>class)|(?<ifp>if)|(?<elif>elif)|(?<elsep>else)|(?<returnp>return))$";
        //lang=regex
        private const string indentation = @"(?<block>:)|(?<space>[\t ]+)|(?<comment>#[^\n]+)|(?<rest>.)";

        private static readonly Regex combined = new Regex(
            string.Join("|", paren, comp, oper, data, indentation)
            , RegexOptions.Compiled);

        private static readonly Regex keywords = new Regex(keywordsList, RegexOptions.Compiled);

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
                        if (token.Name == "name" && token.Success)
                        { // search for keyword
                            found = true;
                            Match kw = keywords.Match(token.Value);
                            if (kw.Success)
                            {
                                foreach (Group kwg in kw.Groups)
                                {
                                    if (kwg.Name == "0") continue;
                                    if (!kwg.Success) continue;

                                    tokens.Add(new Token(fileInfo, i + 1, token.Index + 1, token.Length, (PythonTokens)Enum.Parse(typeof(PythonTokens), kwg.Name), token.Value));
                                    end = token.Index + token.Length;
                                    break;
                                }
                            }
                            else
                            {
                                tokens.Add(new Token(fileInfo, i + 1, token.Index + 1, token.Length, PythonTokens.name, token.Value));
                                end = token.Index + token.Length;
                            }
                            break;
                        }
                        else if (token.Success && token.Length == match.Length)
                        {
                            found = true;
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