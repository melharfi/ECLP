using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Dynamic;
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
        public event EventHandler<AddedArgsEvents> AddedArgs;
        public event EventHandler<AddedFlagsEvents> AddedFlags;
        public event EventHandler<AddedPropertiesEvents> AddedProperties;
        public event EventHandler<AddedCollectionsEvents> AddedCollections;
        public event EventHandler<AddedExCollectionsEvents> AddedExCollections;
        #endregion

        /// <summary>
        /// Command as one signle string
        /// </summary>
        public string Raw;

        /// <summary>
        /// String of arguments to ba parsed
        /// </summary>
        /// <param name="raw">string command</param>
        public ECLP(string raw)
        {
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
        public CommandResult Parse()
        {
            CommandResult data = new CommandResult();

            #region Flags verb ex pattern "--verbose --zomby --deadmatch"
            {
                #region Fetch for matches
                string pattern = @"--\w\S+";
                MatchCollection matches = Regex.Matches(Raw, pattern);
                data.Flags = matches.Cast<Match>().Select(match => match.Value).ToList();
                #endregion

                #region TriggerEvent
                foreach (string flag in data.Flags)
                {
                    AddedFlagsEvents addedFlagsEvents = new AddedFlagsEvents
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
                string pattern = @"-p (\w\S+)=(\w\S+)";
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
                    AddedPropertiesEvents addedPropertiesEvents = new AddedPropertiesEvents
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
                    object[] values = new object[match.Groups[2].Value.Split('|').Count()];
                    string[] oldValues = match.Groups[2].Value.Split('|');
                    for (int v = 0; v < oldValues.Length; v++)
                    {
                        object newValue = DetectType(oldValues[v]);
                        values[v] = newValue;
                    }
                    data.Collections.Add(name, values);

                    #region TriggerEvent
                    AddedCollectionsEvents addedCollectionsEvents = new AddedCollectionsEvents
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
                        if (subValues.Count() == 2)
                        {
                            string key = subValues[0];
                            object newValue = DetectType(subValues[1]);
                            KeyValuePair<string, object> value = new KeyValuePair<string, object>(key, newValue);
                            l.Add(value);
                        }
                    }

                    data.ExCollections.Add(name, l);

                    #region TriggerEvent
                    AddedExCollectionsEvents addedExCollectionsEvents = new AddedExCollectionsEvents();
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
                string[] args = Raw.Split(' ');
                for (int i = 0; i < args.Count(); i++)
                {
                    // double check if it's not a verb just escape matching in the previouse code
                    // if no one of the known verbs is found then it's an Arg
                    if (args[i] != "--" && args[i] != "-p" && args[i] != "-c" && args[i] != "-xc")
                    {
                        // Args, ex "james 5 3,5 true"
                        object detectedType = DetectType(args[i]);
                        if (detectedType == null)
                            detectedType = args[i];
                        object newValue = Convert.ChangeType(args[i], detectedType.GetType());
                        data.Args.Add(newValue);

                        #region TriggerEvent
                        AddedArgsEvents aELPArgsEvents = new AddedArgsEvents
                        {
                            Arg = newValue
                        };
                        OnAddedArgs(aELPArgsEvents);
                        #endregion
                    }
                }
            }
            #endregion

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
        protected virtual void OnAddedArgs(AddedArgsEvents e)
        {
            AddedArgs?.Invoke(this, e);
        }
        protected virtual void OnAddedFlags(AddedFlagsEvents e)
        {
            AddedFlags?.Invoke(this, e);
        }
        protected virtual void OnAddedProperties(AddedPropertiesEvents e)
        {
            AddedProperties?.Invoke(this, e);
        }
        protected virtual void OnAddedCollections(AddedCollectionsEvents e)
        {
            AddedCollections?.Invoke(this, e);
        }
        protected virtual void OnAddedExCollections(AddedExCollectionsEvents e)
        {
            AddedExCollections?.Invoke(this, e);
        }
        #endregion
    }
}
