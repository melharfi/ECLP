using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using The_Morpher.Events;

namespace The_Morpher
{
    public class ECLP
    {
        #region define Verbs
        public string FlagsVerb = "-f";
        public string MultuFlagsVerb = "-f-m";
        public string PropertiesVerb = "-p";
        public string MutiPropertiesVerb = "-p-m";
        public string CollectionsVerb = "-c";
        public string MultiCollectionsVerb = "-c-m";
        public string ExCollectionsVerb = "-xc";
        #endregion

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
        public readonly string Raw;

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
        }
        public ArgsModel Parse()
        {
            ArgsModel data = new ArgsModel();
            data.Clear();
            string[] args = Raw.Split(' ');

            for (int i = 0; i < args.Count(); i++)
            {
                // define actual verb
                // if no one of the known verbs is found then it's an Arg
                if (args[i] != FlagsVerb && args[i] != MultuFlagsVerb && args[i] != PropertiesVerb && args[i] != MutiPropertiesVerb && args[i] != CollectionsVerb && args[i] != MultiCollectionsVerb && args[i] != ExCollectionsVerb)
                {
                    // Args, ex "verbose 5 3,5 true --v"
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
                else
                {
                    // check if next argument exist otherwize no need to associate a null value
                    if (args.Count() == i + 1)
                        break;

                    if (args[i] == FlagsVerb)
                    {
                        // Flags, ex "-f Driver"
                        string flag = args[i + 1];
                        data.Flags.Add(flag);

                        #region TriggerEvent
                        AddedFlagsEvents aELPFlagsEvents = new AddedFlagsEvents
                        {
                            Flag = flag
                        };
                        OnAddedFlags(aELPFlagsEvents);
                        #endregion

                        #region Skip next argument
                        i++;
                        if (args.Count() == i + 1)
                            break;
                        #endregion
                    }
                    else if(args[i] == MultuFlagsVerb)
                    {
                        // Multiple Flags, ex "-f-m driver,car,california"
                        string[] flags = args[i + 1].Split('|');
                        foreach (string flag in flags)
                        {
                            data.Flags.Add(flag);
                            #region TriggerEvent
                            AddedFlagsEvents aELPFlagsEvents = new AddedFlagsEvents
                            {
                                Flag = flag
                            };
                            OnAddedFlags(aELPFlagsEvents);
                            #endregion
                        }

                        #region Skip next argument
                        i++;
                        if (args.Count() == i + 1)
                            break;
                        #endregion
                    }
                    else if (args[i] == PropertiesVerb)
                    {
                        // Properties, ex "-p driver=steave"
                        string[] prop = args[i + 1].Split('=');
                        if(prop.Count() == 2)
                        {
                            string name = prop[0];
                            string value = prop[1];
                            object newValue = DetectType(value);
                            data.Properties.Add(name, newValue);

                            #region TriggerEvent
                            AddedPropertiesEvents aELPPropertiesEvents = new AddedPropertiesEvents();
                            aELPPropertiesEvents.Property = new KeyValuePair<string, object>(name, newValue);
                            OnAddedProperties(aELPPropertiesEvents);
                            #endregion
                        }

                        #region Skip next argument
                        i++;
                        if (args.Count() == i + 1)
                            break;
                        #endregion
                    }
                    else if (args[i] == MutiPropertiesVerb)
                    {
                        // Properties, ex "-p-m driver=steave|age=30"
                        string[] raws = args[i + 1].Split('|');
                        foreach (string raw in raws)
                        {
                            string[] prop = raw.Split('=');
                            if (prop.Count() == 2)
                            {
                                string name = prop[0];
                                string value = prop[1];
                                object newValue = DetectType(value);
                                data.Properties.Add(name, newValue);

                                #region TriggerEvent
                                AddedPropertiesEvents aELPPropertiesEvents = new AddedPropertiesEvents();
                                aELPPropertiesEvents.Property = new KeyValuePair<string, object>(name, newValue);
                                OnAddedProperties(aELPPropertiesEvents);
                                #endregion
                            }
                        }

                        #region Skip next argument
                        i++;
                        if (args.Count() == i + 1)
                            break;
                        #endregion
                    }
                    else if (args[i] == CollectionsVerb)
                    {
                        // Properties, ex "-c players=steave|john|clark"
                        string[] col = args[i + 1].Split('=');
                        if (col.Count() == 2)
                        {
                            string name = col[0];
                            object[] values = new object[col[1].Split('|').Count()];

                            string[] oldValues = col[1].Split('|');
                            for (int v = 0; v < oldValues.Length; v++)
                            {
                                object newValue = DetectType(oldValues[v]);
                                values[v] = newValue;
                            }
                            data.Collections.Add(name, values);

                            #region TriggerEvent
                            AddedCollectionsEvents aELPCollectionsEvents = new AddedCollectionsEvents();
                            aELPCollectionsEvents.Collections = new KeyValuePair<string, object[]>(name, values);
                            OnAddedCollections(aELPCollectionsEvents);
                            #endregion
                        }

                        #region Skip next argument
                        i++;
                        if (args.Count() == i + 1)
                            break;
                        #endregion
                    }
                    else if (args[i] == MultiCollectionsVerb)
                    {
                        // Properties, ex "-c-m players=steave|john|clark/ages=21|15|30"
                        string[] raws = args[i + 1].Split('/');
                        foreach(string raw in raws)
                        {
                            string[] col = raw.Split('=');
                            if (col.Count() == 2)
                            {
                                string name = col[0];
                                object[] values = new object[col[1].Split('|').Count()];

                                string[] oldValues = col[1].Split('|');
                                for (int v = 0; v < oldValues.Length; v++)
                                {
                                    object newValue = DetectType(oldValues[v]);
                                    values[v] = newValue;
                                }
                                data.Collections.Add(name, values);

                                #region TriggerEvent
                                AddedCollectionsEvents aELPCollectionsEvents = new AddedCollectionsEvents();
                                aELPCollectionsEvents.Collections = new KeyValuePair<string, object[]>(name, values);
                                OnAddedCollections(aELPCollectionsEvents);
                                #endregion
                            }
                        }

                        #region Skip next argument
                        i++;
                        if (args.Count() == i + 1)
                            break;
                        #endregion
                    }
                    else if (args[i] == ExCollectionsVerb)
                    {
                        // Extanded Collection, ex "-xc players=steave:21|john:15|clark:30"
                        string[] col = args[i + 1].Split('=');
                        if (col.Count() == 2)
                        {
                            List<KeyValuePair<string, object>> l = new List<KeyValuePair<string, object>>();
                            string name = col[0];
                            string[] rawValues = col[1].Split('|');

                            foreach (string values in rawValues)
                            {
                                string[] subValues = values.Split(':');
                                if(subValues.Count() == 2)
                                {
                                    string key = subValues[0];
                                    object newValue = DetectType(subValues[1]);
                                    KeyValuePair<string, object> value = new KeyValuePair<string, object>(key, newValue);
                                    l.Add(value);
                                }
                            }

                            data.ExCollections.Add(name, l);

                            #region TriggerEvent
                            AddedExCollectionsEvents aELPExCollectionsEvents = new AddedExCollectionsEvents();
                            aELPExCollectionsEvents.ExCollections = new KeyValuePair<string, List<KeyValuePair<string, object>>>(name, l);
                            OnAddedExCollections(aELPExCollectionsEvents);
                            #endregion
                        }

                        #region Skip next argument
                        i++;
                        if (args.Count() == i + 1)
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
