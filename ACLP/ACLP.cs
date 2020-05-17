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
    public class ACLP
    {
        public event EventHandler<AddedArgsEvents> AddedArgs;
        public event EventHandler<AddedFlagsEvents> AddedFlags;
        public event EventHandler<AddedPropertiesEvents> AddedProperties;
        public event EventHandler<AddedCollectionsEvents> AddedCollections;
        public event EventHandler<AddedExCollectionsEvents> AddedExCollections;

        /// <summary>
        /// Command as one signle string
        /// </summary>
        public readonly string Raw;

        /// <summary>
        /// String of arguments to ba parsed
        /// </summary>
        /// <param name="raw">string command</param>
        public ACLP(string raw)
        {
            this.Raw = raw.Trim();
        }

        /// <summary>
        /// Array of arguments to be parsed
        /// </summary>
        /// <param name="args">Arguments as Array</param>
        public ACLP(string[] args)
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
                // define type of data
                // if no of the known attributes is found then it's a simple arg
                if (args[i] != "-f" && args[i] != "-f-m" && args[i] != "-p" && args[i] != "-p-m" && args[i] != "-c" && args[i] != "-c-m" && args[i] != "-xc")
                {
                    // Args, ex "verbose 5 3,5 true --v"
                    object detectedType = DetectType(args[i]);
                    if (detectedType == null)
                        detectedType = args[i];
                    object newValue = Convert.ChangeType(args[i], detectedType.GetType());
                    data.Args.Add(newValue);

                    #region TriggerEvent
                    AddedArgsEvents aCLPArgsEvents = new AddedArgsEvents
                    {
                        Arg = newValue
                    };
                    OnAddedArgs(aCLPArgsEvents);
                    #endregion
                }
                else
                {
                    // check if next argument exist otherwize no need to associate a null value
                    if (args.Count() == i + 1)
                        break;

                    if (args[i] == "-f")
                    {
                        // Flags, ex "-f Driver"
                        string flag = args[i + 1];
                        data.Flags.Add(flag);

                        #region TriggerEvent
                        AddedFlagsEvents aCLPFlagsEvents = new AddedFlagsEvents
                        {
                            Flag = flag
                        };
                        OnAddedFlags(aCLPFlagsEvents);
                        #endregion

                        #region Skip next argument
                        i++;
                        if (args.Count() == i + 1)
                            break;
                        #endregion
                    }
                    else if(args[i] == "-f-m")
                    {
                        // Multiple Flags, ex "-f-m driver,car,california"
                        string[] flags = args[i + 1].Split('|');
                        foreach (string flag in flags)
                        {
                            data.Flags.Add(flag);
                            #region TriggerEvent
                            AddedFlagsEvents aCLPFlagsEvents = new AddedFlagsEvents
                            {
                                Flag = flag
                            };
                            OnAddedFlags(aCLPFlagsEvents);
                            #endregion
                        }

                        #region Skip next argument
                        i++;
                        if (args.Count() == i + 1)
                            break;
                        #endregion
                    }
                    else if (args[i] == "-p")
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
                            AddedPropertiesEvents aCLPPropertiesEvents = new AddedPropertiesEvents();
                            aCLPPropertiesEvents.Property = new KeyValuePair<string, object>(name, newValue);
                            OnAddedProperties(aCLPPropertiesEvents);
                            #endregion
                        }

                        #region Skip next argument
                        i++;
                        if (args.Count() == i + 1)
                            break;
                        #endregion
                    }
                    else if (args[i] == "-p-m")
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
                                AddedPropertiesEvents aCLPPropertiesEvents = new AddedPropertiesEvents();
                                aCLPPropertiesEvents.Property = new KeyValuePair<string, object>(name, newValue);
                                OnAddedProperties(aCLPPropertiesEvents);
                                #endregion
                            }
                        }

                        #region Skip next argument
                        i++;
                        if (args.Count() == i + 1)
                            break;
                        #endregion
                    }
                    else if (args[i] == "-c")
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
                            AddedCollectionsEvents aCLPCollectionsEvents = new AddedCollectionsEvents();
                            aCLPCollectionsEvents.Collections = new KeyValuePair<string, object[]>(name, values);
                            OnAddedCollections(aCLPCollectionsEvents);
                            #endregion
                        }

                        #region Skip next argument
                        i++;
                        if (args.Count() == i + 1)
                            break;
                        #endregion
                    }
                    else if (args[i] == "-c-m")
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
                                AddedCollectionsEvents aCLPCollectionsEvents = new AddedCollectionsEvents();
                                aCLPCollectionsEvents.Collections = new KeyValuePair<string, object[]>(name, values);
                                OnAddedCollections(aCLPCollectionsEvents);
                                #endregion
                            }
                        }

                        #region Skip next argument
                        i++;
                        if (args.Count() == i + 1)
                            break;
                        #endregion
                    }
                    else if (args[i] == "-xc")
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
                            AddedExCollectionsEvents aCLPExCollectionsEvents = new AddedExCollectionsEvents();
                            aCLPExCollectionsEvents.ExCollections = new KeyValuePair<string, List<KeyValuePair<string, object>>>(name, l);
                            OnAddedExCollections(aCLPExCollectionsEvents);
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
            else if (decimal.TryParse(str, out decimal decimalResult))
                return decimalResult;
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
            EventHandler<AddedArgsEvents> handler = AddedArgs;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        protected virtual void OnAddedFlags(AddedFlagsEvents e)
        {
            EventHandler<AddedFlagsEvents> handler = AddedFlags;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        protected virtual void OnAddedProperties(AddedPropertiesEvents e)
        {
            EventHandler<AddedPropertiesEvents> handler = AddedProperties;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        protected virtual void OnAddedCollections(AddedCollectionsEvents e)
        {
            EventHandler<AddedCollectionsEvents> handler = AddedCollections;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        protected virtual void OnAddedExCollections(AddedExCollectionsEvents e)
        {
            EventHandler<AddedExCollectionsEvents> handler = AddedExCollections;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion
    }
}
