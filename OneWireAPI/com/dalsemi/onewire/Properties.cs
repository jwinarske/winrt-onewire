﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Storage;

namespace com.dalsemi.onewire
{
    public class Properties
    {
        /// <summary>
        /// Enable/disable debug messages </summary>
        private static bool doDebugMessages = false;

        /// <summary>
        /// property table
        /// </summary>
        private Dictionary<string, string> props = null;

        /// <summary>
        /// Default constructor
        /// </summary>
        public Properties()
        {
            props = new Dictionary<string, string>();
        }

        /// <summary>
        /// Routine to populate the hash table
        /// </summary>
        /// <param name="table"></param>
        /// <param name="reader"></param>
        private void loadTable(Dictionary<string, string> table, StreamReader reader)
        {
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] st = line.Split(new char[] { '=' });

                if (st.Length < 2)
                    continue;
                else if (st.Length == 2)
                {
                    if (st[0].StartsWith("#"))
                    {
                        if (doDebugMessages)
                        {
                            Debug.WriteLine("Commented out property >> " + st[0] + "=" + st[1]);
                        }
                        continue;
                    }

                    table.Add(st[0].Trim(), st[1].Trim());
                }
                else if (st.Length > 2)
                {
                    if (doDebugMessages)
                    {
                        Debug.WriteLine("Property ignored as it has more than one '='!");
                    }
                    continue;
                }
            };
        }

        /// <summary>
        /// Called to load property file from local folder on filesystem
        /// </summary>
        /// <param name="file"></param>
        public bool loadLocalFile(string file)
        {
            bool result = false;
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            if (doDebugMessages)
            {
                Debug.WriteLine("Loading " + localFolder.Path + "\\" + file);
            }

            if (File.Exists(localFolder.Path + "\\" + file))
            {
                StorageFile localFile = null;
                Stream stream = null;
                var t = Task.Run(async () =>
                {
                    localFile = await localFolder.GetFileAsync(file);
                    stream = await localFile.OpenStreamForReadAsync();
                });
                t.Wait();
                using (var reader = new StreamReader(stream))
                {
                    if (doDebugMessages)
                    {
                        Debug.WriteLine("Loading " + localFolder.Path + "\\" + file);
                    }
                    loadTable(props, reader);
                    result = true;
                }
            }
            else
            {
                if (doDebugMessages)
                {
                    Debug.WriteLine("Did not find " + localFolder.Path + "\\" + file);
                }
            }
            return result;
        }

        /// <summary>
        /// Load property file from the OneWireAPI assembly
        /// </summary>
        /// <param name="resource_file"></param>
        public bool loadResourceFile(Assembly asm, string file)
        {
            bool result = false;
            Stream stream = null;
            StreamReader reader = null;
            try
            {
                stream = asm.GetManifestResourceStream(file);
                reader = new StreamReader(stream);
                if (doDebugMessages)
                {
                    Debug.WriteLine("Loading resource: " + file);
                }
                loadTable(props, reader);
                result = true;
            }
            catch (Exception)
            {
                if (doDebugMessages)
                {
                    Debug.WriteLine("Can't find resource: " + file);
                }
            }

            if (stream != null)
            {
                stream.Dispose();
                stream = null;
            }
            return result;
        }

        /// <summary>
        /// Look up key in hashtable for value
        /// </summary>
        /// <param name="propName"></param>
        /// <returns>null if no entry, otherwise string value</returns>
        public string getProperty(string propName)
        {
            string ret_str = null;

            if (props != null)
            {
                props.TryGetValue(propName, out ret_str);
            }
            return ret_str;
        }

        public string getProperty(string propName, string defValue)
        {
            string ret = getProperty(propName);
            return (string.ReferenceEquals(ret, null)) ? defValue : ret;
        }

        public bool getPropertyBoolean(string propName, bool defValue)
        {
            string strValue = getProperty(propName);
            if (!string.ReferenceEquals(strValue, null))
            {
                defValue = System.Convert.ToBoolean(strValue);
            }
            return defValue;
        }

        public byte[] getPropertyBytes(string propName, byte[] defValue)
        {
            string strValue = getProperty(propName);
            if (!string.ReferenceEquals(strValue, null))
            {
                //only supports up to 128 bytes of data
                byte[] tmp = new byte[128];

                //split the string on commas and spaces
                string[] strtok = strValue.Split(new Char[] { ',', ' ' });

                //how many bytes we got
                int i = 0;
                foreach (string multiByteStr in strtok)
                {
                    //this string could have more than one byte in it
                    int strLen = multiByteStr.Length;

                    for (int j = 0; j < strLen; j += 2)
                    {
                        //get just two nibbles at a time
                        string byteStr = multiByteStr.Substring(j, Math.Min(2, strLen));

                        long lng = 0;
                        try
                        {
                            //parse the two nibbles into a byte
                            lng = long.Parse(byteStr); //16
                        }
                        catch (FormatException nfe)
                        {
                            Debug.WriteLine(nfe.ToString());
                            Debug.Write(nfe.StackTrace);

                            //no mercy!
                            return defValue;
                        }

                        //store the byte and increment the counter
                        if (i < tmp.Length)
                        {
                            tmp[i++] = (byte)(lng & 0x0FF);
                        }
                    }
                }

                if (i > 0)
                {
                    byte[] retVal = new byte[i];
                    Array.Copy(tmp, 0, retVal, 0, i);
                    return retVal;
                }
            }
            return defValue;
        }

        /// <summary>
        /// Returns true if hashtable is empty, false otherwise
        /// </summary>
        public bool Empty
        {
            get
            {
                if (props.Count == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Returns Enumerator of hash table
        /// </summary>
        /// <returns></returns>
        internal IEnumerator keys()
        {
            return props.GetEnumerator();
        }

        /// <summary>
        /// Put with default value
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        internal void put(string key, string value)
        {
            props.Add(key, value);
        }

        /// <summary>
        /// Returns hashtable entry
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal string get(string key)
        {
            string val;

            if (!props.TryGetValue(key, out val))
                return null;

            return val;
        }

        /// <summary>
        /// Removes hashtable entry
        /// </summary>
        /// <param name="key"></param>
        internal void remove(string key)
        {
            if (props.ContainsKey(key))
                props.Remove(key);
        }
    }
}