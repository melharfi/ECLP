using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using The_Morpher.Events;

namespace The_Morpher
{
    public class ECLP
    {
        #region Events
        public event EventHandler<AddedArgumentEventArgs> AddedArgs;
        public event EventHandler<AddedFlagsEventArgs> AddedFlags;
        public event EventHandler<AddedPropertyEventArgs> AddedProperties;
        public event EventHandler<AddedCollectionEventArgs> AddedCollections;
        public event EventHandler<AddedExCollectionEventArgs> AddedExCollections;
        #endregion

        /// <summary>
        /// Command as one signle string
        /// </summary>
        public string Raw { get; set; }

        /// <summary>
        /// String of arguments to ba parsed
        /// </summary>
        /// <param name="raw">string command</param>
        public ECLP(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                throw new NotImplementedException("Can't parse null or empty string");
            this.Raw = raw.Trim();
        }

        /// <summary>
        /// Array of arguments to be parsed
        /// </summary>
        /// <param name="args">Arguments as Array</param>
        public ECLP(string[] args)
        {
            this.Raw = string.Join(" ", args);
            this.Raw = this.Raw.Trim();
        }

        /// <summary>
        /// Parsing the command with one of two ways, with Regex or not.
        /// </summary>
        /// <param name="usingRegex">If true the parsing function will use regex patterns, if not it will use side by side words check algorithm, the way choise is given to user is because Regex sometime not used in some assemblies like wrapped or mono modified assemblies and if regex fail to work perfectly you still can use the side by side one.</param>
        /// <returns></returns>
        public CommandResult Parse(bool usingRegex = true)
        {
            if (usingRegex)
                return UsingRegex();
            else
                return WithoutRegex();
        }
        private CommandResult UsingRegex()
        {
            CommandResult data = new CommandResult();

            #region Flags verb ex pattern "--verbose --zomby --deadmatch"
            {
                #region Fetch for matches
                string pattern = @"--\w+";
                MatchCollection matches = Regex.Matches(Raw, pattern);
                data.Flags = matches.Cast<Match>().Select(match => match.Value).ToList();
                #endregion

                #region TriggerEvent
                foreach (string flag in data.Flags)
                {
                    AddedFlagsEventArgs addedFlagsEvents = new AddedFlagsEventArgs
                    {
                        Flag = flag
                    };
                    OnAddedFlags(addedFlagsEvents);
                }
                #endregion

                #region Cleaning Raw
                Raw = Regex.Replace(Raw, pattern, "");      // deleting pattern just fetched
                Raw = Regex.Replace(Raw, @" \s*", " ");     // deleting multispaces
                this.Raw = this.Raw.Trim();                 // deleting single space from start or end
                #endregion
            }
            #endregion

            #region Property verb ex pattern "-p driver=steave -p age=30"
            {
                #region Fetch for matches
                string pattern = @"-p (\w+)=(\w+)";
                MatchCollection matches = Regex.Matches(Raw, pattern);
                #endregion

                #region raitement + Parsing values + Trigger Events
                foreach (Match match in matches)
                {
                    var name = match.Groups[1].Value;
                    var value = match.Groups[2].Value;
                    object newValue = DetectType(value);
                    data.Properties.Add(name, newValue);

                    #region TriggerEvent
                    AddedPropertyEventArgs addedPropertiesEvents = new AddedPropertyEventArgs
                    {
                        Property = new KeyValuePair<string, object>(name, newValue)
                    };
                    OnAddedProperties(addedPropertiesEvents);
                    #endregion
                }
                #endregion

                #region Cleaning Raw
                Raw = Regex.Replace(Raw, pattern, "");      // deleting pattern just fetched
                Raw = Regex.Replace(Raw, @" \s*", " ");     // deleting multispaces
                this.Raw = this.Raw.Trim();                 // deleting single space from start or end
                #endregion
            }
            #endregion

            #region Collection verb ex pattern "-c players=steave|john|clark -c ages=21|15|30"
            {
                #region Fetch for matches
                string pattern = @"-c (\w\S+)=(\w\S+)\|?(\w\S+)?";
                MatchCollection matches = Regex.Matches(Raw, pattern);
                #endregion

                #region Traitement + Parsing values + Trigger Events
                foreach (Match match in matches)
                {
                    var name = match.Groups[1].Value;
                    object[] values = new object[match.Groups[2].Value.Split('|').Length];
                    string[] oldValues = match.Groups[2].Value.Split('|');
                    for (int v = 0; v < oldValues.Length; v++)
                    {
                        object newValue = DetectType(oldValues[v]);
                        values[v] = newValue;
                    }
                    data.Collections.Add(name, values);

                    #region TriggerEvent
                    AddedCollectionEventArgs addedCollectionsEvents = new AddedCollectionEventArgs
                    {
                        Collections = new KeyValuePair<string, object[]>(name, values)
                    };
                    OnAddedCollections(addedCollectionsEvents);
                    #endregion
                }
                #endregion

                #region Cleaning Raw
                Raw = Regex.Replace(Raw, pattern, "");      // deleting pattern just fetched
                Raw = Regex.Replace(Raw, @" \s*", " ");     // deleting multispaces
                this.Raw = this.Raw.Trim();                 // deleting single space from start or end
                #endregion
            }
            #endregion

            #region ExCollection verb ex pattern "-xc players=steave:21|john:15|clark:30 -xc adresses=Japan:Tokyo|USA:Washington"
            {
                #region Fetch for matches
                string pattern = @"-xc (\w\S+)=(\w\S+:\w\S+[\|\w\S+:\w\S])?";
                MatchCollection matches = Regex.Matches(Raw, pattern);
                #endregion

                #region Traitement + Parsing values + Trigger Events
                foreach (Match match in matches)
                {
                    List<KeyValuePair<string, object>> l = new List<KeyValuePair<string, object>>();
                    var name = match.Groups[1].Value;
                    string[] rawValues = match.Groups[2].Value.Split('|');

                    foreach (string values in rawValues)
                    {
                        string[] subValues = values.Split(':');
                        if (subValues.Length == 2)
                        {
                            string key = subValues[0];
                            object newValue = DetectType(subValues[1]);
                            KeyValuePair<string, object> value = new KeyValuePair<string, object>(key, newValue);
                            l.Add(value);
                        }
                    }

                    data.ExCollections.Add(name, l);

                    #region TriggerEvent
                    AddedExCollectionEventArgs addedExCollectionsEvents = new AddedExCollectionEventArgs();
                    addedExCollectionsEvents.ExCollections = new KeyValuePair<string, List<KeyValuePair<string, object>>>(name, l);
                    OnAddedExCollections(addedExCollectionsEvents);
                    #endregion
                }
                #endregion

                #region Cleaning Raw
                Raw = Regex.Replace(Raw, pattern, "");      // deleting pattern just fetched
                Raw = Regex.Replace(Raw, @" \s*", " ");     // deleting multispaces
                this.Raw = this.Raw.Trim();                 // deleting single space from start or end
                #endregion
            }
            #endregion

            // Args should be last thing to fetch
            #region Args verb ex "5 3,5 true james"
            {
                if (!string.IsNullOrEmpty(Raw))
                {
                    string[] args = Raw.Split(' ');
                    for (int i = 0; i < args.Length; i++)
                    {
                        // double check if it's not a verb just escape matching in the previouse code
                        // if no one of the known verbs is found then it's an Arg
                        if (args[i] != "--" && args[i] != "-p" && args[i] != "-c" && args[i] != "-xc")
                        {
                            // Args, ex "james 5 3,5 true"
                            object detectedType = DetectType(args[i]);
                            if (detectedType == null)
                                detectedType = args[i];
                            object newValue = Convert.ChangeType(args[i], detectedType.GetType(), CultureInfo.GetCultureInfo("en-US"));
                            data.Args.Add(newValue);

                            #region TriggerEvent
                            AddedArgumentEventArgs aELPArgsEvents = new AddedArgumentEventArgs
                            {
                                Arg = newValue
                            };
                            OnAddedArgs(aELPArgsEvents);
                            #endregion
                        }
                    }
                }
            }
            #endregion

            return data;
        }
        private CommandResult WithoutRegex()
        {
            CommandResult data = new CommandResult();
            data.Clear();
            string[] args = Raw.Split(' ');

            for (int i = 0; i < args.Length; i++)
            {
                // define actual verb
                // if no one of the known verbs is found then it's an Arg
                if (!args[i].StartsWith("--", StringComparison.OrdinalIgnoreCase) && args[i] != "-p" && args[i] != "-c" && args[i] != "-xc")
                {
                    // Args, ex "start 17 3,5 true T" as string, int, float, bool and char
                    object detectedType = DetectType(args[i]);
                    if (detectedType == null)
                        detectedType = args[i];
                    object newValue = Convert.ChangeType(args[i], detectedType.GetType(), CultureInfo.GetCultureInfo("en-US"));
                    data.Args.Add(newValue);

                    #region TriggerEvent
                    AddedArgumentEventArgs addedArgsEvent = new AddedArgumentEventArgs
                    {
                        Arg = newValue
                    };
                    OnAddedArgs(addedArgsEvent);
                    #endregion
                }
                else
                {
                    // check if next argument exist otherwize no need to associate a null value
                    if (args.Length == i + 1)
                        break;

                    if (args[i].StartsWith("--", StringComparison.OrdinalIgnoreCase))
                    {
                        // Flags, ex "--verbose"
                        string flag = args[i];
                        data.Flags.Add(flag);

                        #region TriggerEvent
                        AddedFlagsEventArgs addedFlagEvent = new AddedFlagsEventArgs
                        {
                            Flag = flag
                        };
                        OnAddedFlags(addedFlagEvent);
                        #endregion

                        #region Skip next argument
                        if (args.Length == i)
                            break;
                        #endregion
                    }
                    else if (args[i] == "-p")
                    {
                        // Properties, ex "-p driver=steave"
                        string[] prop = args[i + 1].Split('=');
                        if (prop.Length == 2)
                        {
                            string name = prop[0];
                            string value = prop[1];
                            object newValue = DetectType(value);
                            data.Properties.Add(name, newValue);

                            #region TriggerEvent
                            AddedPropertyEventArgs addedPropertyEvent = new AddedPropertyEventArgs();
                            addedPropertyEvent.Property = new KeyValuePair<string, object>(name, newValue);
                            OnAddedProperties(addedPropertyEvent);
                            #endregion
                        }

                        #region Skip next argument
                        i++;
                        if (args.Length == i + 1)
                            break;
                        #endregion
                    }
                    else if (args[i] == "-c")
                    {
                        // Properties, ex "-c players=steave|john|clark"
                        string[] col = args[i + 1].Split('=');
                        if (col.Length == 2)
                        {
                            string name = col[0];
                            object[] values = new object[col[1].Split('|').Length];

                            string[] oldValues = col[1].Split('|');
                            for (int v = 0; v < oldValues.Length; v++)
                            {
                                object newValue = DetectType(oldValues[v]);
                                values[v] = newValue;
                            }
                            data.Collections.Add(name, values);

                            #region TriggerEvent
                            AddedCollectionEventArgs addedCollectionEvent = new AddedCollectionEventArgs();
                            addedCollectionEvent.Collections = new KeyValuePair<string, object[]>(name, values);
                            OnAddedCollections(addedCollectionEvent);
                            #endregion
                        }

                        #region Skip next argument
                        i++;
                        if (args.Length == i + 1)
                            break;
                        #endregion
                    }
                    else if (args[i] == "-xc")
                    {
                        // Extanded Collection, ex "-xc players=steave:21|john:15|clark:30"
                        string[] col = args[i + 1].Split('=');
                        if (col.Length == 2)
                        {
                            List<KeyValuePair<string, object>> l = new List<KeyValuePair<string, object>>();
                            string name = col[0];
                            string[] rawValues = col[1].Split('|');

                            foreach (string values in rawValues)
                            {
                                string[] subValues = values.Split(':');
                                if (subValues.Length == 2)
                                {
                                    string key = subValues[0];
                                    object newValue = DetectType(subValues[1]);
                                    KeyValuePair<string, object> value = new KeyValuePair<string, object>(key, newValue);
                                    l.Add(value);
                                }
                            }

                            data.ExCollections.Add(name, l);

                            #region TriggerEvent
                            AddedExCollectionEventArgs addedExCollectionEvent = new AddedExCollectionEventArgs();
                            addedExCollectionEvent.ExCollections = new KeyValuePair<string, List<KeyValuePair<string, object>>>(name, l);
                            OnAddedExCollections(addedExCollectionEvent);
                            #endregion
                        }

                        #region Skip next argument
                        i++;
                        if (args.Length == i + 1)
                            break;
                        #endregion
                    }
                }
            }
            return data;
        }
        private object DetectType(string str)
        {
            if (int.TryParse(str, out int intResult))
                return intResult;
            else if (float.TryParse(str, out float floatResult))
                return floatResult;
            else if (bool.TryParse(str, out bool boolResult))
                return boolResult;
            else if (char.TryParse(str, out char charResult))
                return charResult;
            else
                return str;
        }
        #region Handlers
        protected virtual void OnAddedArgs(AddedArgumentEventArgs e)
        {
            AddedArgs?.Invoke(this, e);
        }
        protected virtual void OnAddedFlags(AddedFlagsEventArgs e)
        {
            AddedFlags?.Invoke(this, e);
        }
        protected virtual void OnAddedProperties(AddedPropertyEventArgs e)
        {
            AddedProperties?.Invoke(this, e);
        }
        protected virtual void OnAddedCollections(AddedCollectionEventArgs e)
        {
            AddedCollections?.Invoke(this, e);
        }
        protected virtual void OnAddedExCollections(AddedExCollectionEventArgs e)
        {
            AddedExCollections?.Invoke(this, e);
        }
        #endregion
    }
}
