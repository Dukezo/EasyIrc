using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyIrc
{
    /// <summary>
    ///     Represents an IRC message.
    /// </summary>
    public class IrcMessage
    {
        /// <summary>
        ///     The unparsed raw message.
        /// </summary>
        public string RawMessage { get; protected set; }

        /// <summary>
        ///     The parsed prefix of the message.
        /// </summary>
        public string Prefix { get; protected set; }

        /// <summary>
        ///     The parsed command of the message.
        /// </summary>
        public string Command { get; protected set; }

        /// <summary>
        ///     The parsed parameters of the message.
        /// </summary>
        public string[] Parameters { get; protected set; }

        /// <summary>
        ///     The parsed tags of the message.
        /// </summary>
        public Dictionary<string, string> Tags { get; protected set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="IrcMessage"></see> class.
        /// </summary>
        /// <param name="rawMessage"></param>
        public IrcMessage(string rawMessage)
        {
            RawMessage = rawMessage;

            string[] segments = rawMessage.Split(' ');

            if(segments.Length >= 2)
            {
                int parametersStartingIdx = -1;
                int possiblePrefixIndex = 0;

                // Check if message contains tags.
                if(segments[0].StartsWith("@"))
                {
                    Tags = new Dictionary<string, string>();
                    string[] splittedTags = segments[0].Substring(1).Split(';');
                    
                    foreach(string splittedTag in splittedTags)
                    {
                        if (splittedTag.Contains("="))
                        {
                            string[] kvTag = splittedTag.Split('=');
                            Tags.Add(kvTag[0], kvTag[1]);
                        }
                        else
                            Debug.WriteLine("IrcMessage contains an invalid tag.");
                    }

                    possiblePrefixIndex = 1;
                }

                // Check if message contains a prefix.
                if(segments[possiblePrefixIndex].StartsWith(":"))
                {
                    Prefix = segments[possiblePrefixIndex].Substring(1);
                    Command = segments[possiblePrefixIndex + 1];
                    parametersStartingIdx = possiblePrefixIndex + 2;
                }
                else
                {
                    Command = segments[possiblePrefixIndex];
                    parametersStartingIdx = possiblePrefixIndex + 1;
                }

                List<string> listParameters = new List<string>();

                for(int i = parametersStartingIdx; i < segments.Length; i++ )
                {
                    if(segments[i].StartsWith(":"))
                    {
                        string[] listParameterSegments = new string[segments.Length - i];
                        Array.Copy(segments, i, listParameterSegments, 0, segments.Length - i);
                        listParameterSegments[0] = listParameterSegments[0].Substring(1); // Remove array colon from the first segment.

                        listParameters.Add(String.Join(" ", listParameterSegments));
                        break;
                    }
                    else
                    {
                        listParameters.Add(segments[i]);
                    }
                }

                Parameters = listParameters.ToArray();
            }
        }
    }
}
