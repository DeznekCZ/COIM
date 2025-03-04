using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private const string data = @"(?<str>""[^""]*""|'[^']*')|(?<number>\d+(?:.\d+)?)|(?<name>[a-zA-Z_]\w*)";
        //lang=regex
        private const string keywordsList = @"^(?:(?<none>None)|(?<btrue>True)|(?<bfalse>False)|(?<and>and)|(?<or>or)|(?<ink>in)|(?<from>from)|(?<import>import)|(?<def>def)|(?<classp>class)|(?<ifp>if)|(?<elif>elif)|(?<elsep>else)|(?<returnp>return)|(?<pass>pass))$";
        //lang=regex
        private const string indentation = @"(?<block>:)|(?<space>[\t ]+)|(?<comment>#[^\n]+)|(?<rest>.)";

        private static readonly Regex combined = new Regex(
            string.Join("|", paren, comp, oper, data, indentation)
            , RegexOptions.Compiled);

        private static readonly Regex keywords = new Regex(keywordsList, RegexOptions.Compiled);
        private static readonly Regex indent = new Regex(@"^(?<block>[\t ]*)(?<rest>.*)$");

        public static Token[] Parse(string file)
        {
            List<Token> tokens = new List<Token>();
            string[] lines = File.ReadAllLines(file);
            FileInfo fileInfo = new FileInfo(file);

            Stack<string> indentaion = new Stack<string>();
            for (int i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                    continue; // skip empty lines

                Match indentMatch = Tokenizer.indent.Match(lines[i]);
                string indent = indentMatch.Groups["block"].Value;
                string rest = indentMatch.Groups["rest"].Value;
                while (indentaion.Count > 0 && indentaion.Peek().Length > indent.Length)
                {
                    tokens.Add(new Token(fileInfo, i + 1, 0, indent.Length, PythonTokens.dedent, indent));
                    indentaion.Pop();
                }
                if (indent.Length > 0 && (indentaion.Count == 0 || indentaion.Peek().Length < indent.Length))
                {
                    tokens.Add(new Token(fileInfo, i + 1, 0, indent.Length, PythonTokens.indent, indent));
                    indentaion.Push(indent);
                }

                MatchCollection matchCollection = combined.Matches(lines[i]);
                int end = indent.Length;
                foreach (Match match in matchCollection)
                {
                    bool found = false;
                    foreach (Group token in match.Groups)
                    {
                        if (token.Name == "0") continue;
                        if (token.Name == "rest") continue;
                        if (token.Name == "space")
                        { // skip whitespaces
                            found = true;
                            end = token.Index + token.Length;
                            break;
                        }
                        if (token.Name == "comment")
                        { // skip whitespaces
                            found = true;
                            tokens.Add(new Token(fileInfo, i, end, 1, PythonTokens.newline, "\n"));
                            end = token.Index + token.Length;
                            break;
                        }
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

            while (indentaion.Count > 0)
            {
                tokens.Add(new Token(fileInfo, lines.Length, lines.Last().Length, 0, PythonTokens.dedent, ""));
                indentaion.Pop();
            }
            tokens.Add(new Token(fileInfo, lines.Length, lines.Last().Length, 0, PythonTokens.eof, ""));

            return tokens.ToArray();
        }
    }
}