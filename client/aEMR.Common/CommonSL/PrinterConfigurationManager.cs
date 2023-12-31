﻿
using System;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using aEMR.Common.Printing;

using System.Text;



public sealed class IsolatedStorageSettings : IDictionary<string, object>, IDictionary,
ICollection<KeyValuePair<string, object>>, ICollection,
IEnumerable<KeyValuePair<string, object>>, IEnumerable
{

    static private IsolatedStorageSettings application_settings;
    static private IsolatedStorageSettings site_settings;

    private IsolatedStorageFile container;
    private Dictionary<string, object> settings;

    // SL2 use a "well known" name and it's readable (and delete-able) directly by isolated storage
    private const string LocalSettings = "__LocalSettings";

    internal IsolatedStorageSettings(IsolatedStorageFile isf)
    {
        container = isf;

        if (!isf.FileExists(LocalSettings))
        {
            settings = new Dictionary<string, object>();
            return;
        }

        using (IsolatedStorageFileStream fs = isf.OpenFile(LocalSettings, FileMode.Open))
        {
            using (StreamReader sr = new StreamReader(fs))
            {
                DataContractSerializer reader = new DataContractSerializer(typeof(Dictionary<string, object>));
                try
                {
                    settings = (Dictionary<string, object>)reader.ReadObject(fs);
                }
                catch (XmlException)
                {
                    settings = new Dictionary<string, object>();
                }
            }
        }
    }

    ~IsolatedStorageSettings()
    {
        // settings are automatically saved if the application close normally
        //Save();
    }

    // static properties

    // per application, per-computer, per-user
    public static IsolatedStorageSettings ApplicationSettings
    {
        get
        {
            if (application_settings == null)
            {
                try
                {
                    application_settings = new IsolatedStorageSettings(IsolatedStorageFile.GetUserStoreForApplication());
                }
                catch
                {
                    application_settings = new IsolatedStorageSettings(IsolatedStorageFile.GetUserStoreForAssembly());
                }
            }
            return application_settings;
        }
    }

    // per domain, per-computer, per-user
    public static IsolatedStorageSettings SiteSettings
    {
        get
        {
            if (site_settings == null)
            {
                site_settings = new IsolatedStorageSettings(
                IsolatedStorageFile.GetUserStoreForSite());
            }
            return site_settings;
        }
    }

    // properties

    public int Count
    {
        get { return settings.Count; }
    }

    public ICollection Keys
    {
        get { return settings.Keys; }
    }

    public ICollection Values
    {
        get { return settings.Values; }
    }

    public object this[string key]
    {
        get
        {
            return settings[key];
        }
        set
        {
            settings[key] = value;
        }
    }

    // methods

    public void Add(string key, object value)
    {
        settings.Add(key, value);
    }

    // This method is emitted as virtual due to: https://bugzilla.novell.com/show_bug.cgi?id=446507
    public void Clear()
    {
        settings.Clear();
    }

    public bool Contains(string key)
    {
        if (key == null)
            throw new ArgumentNullException("key");
        return settings.ContainsKey(key);
    }

    public bool Remove(string key)
    {
        return settings.Remove(key);
    }

    public void Save()
    {
        using (IsolatedStorageFileStream fs = container.CreateFile(LocalSettings))
        {
            DataContractSerializer ser = new DataContractSerializer(settings.GetType());
            ser.WriteObject(fs, settings);
        }
    }

    public bool TryGetValue<T>(string key, out T value)
    {
        object v;
        if (!settings.TryGetValue(key, out v))
        {
            value = default(T);
            return false;
        }
        value = (T)v;
        return true;
    }

    // explicit interface implementations

    int ICollection<KeyValuePair<string, object>>.Count
    {
        get { return settings.Count; }
    }

    bool ICollection<KeyValuePair<string, object>>.IsReadOnly
    {
        get { return false; }
    }

    void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
    {
        settings.Add(item.Key, item.Value);
    }

    void ICollection<KeyValuePair<string, object>>.Clear()
    {
        settings.Clear();
    }

    bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
    {
        return settings.ContainsKey(item.Key);
    }

    void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
    {
        (settings as ICollection<KeyValuePair<string, object>>).CopyTo(array, arrayIndex);
    }

    bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
    {
        return settings.Remove(item.Key);
    }


    ICollection<string> IDictionary<string, object>.Keys
    {
        get { return settings.Keys; }
    }

    ICollection<object> IDictionary<string, object>.Values
    {
        get { return settings.Values; }
    }

    bool IDictionary<string, object>.ContainsKey(string key)
    {
        return settings.ContainsKey(key);
    }

    bool IDictionary<string, object>.TryGetValue(string key, out object value)
    {
        return settings.TryGetValue(key, out value);
    }


    private string ExtractKey(object key)
    {
        if (key == null)
            throw new ArgumentNullException("key");
        return (key as string);
    }

    void IDictionary.Add(object key, object value)
    {
        string s = ExtractKey(key);
        if (s == null)
            throw new ArgumentException("key");

        settings.Add(s, value);
    }

    void IDictionary.Clear()
    {
        settings.Clear();
    }

    bool IDictionary.Contains(object key)
    {
        string skey = ExtractKey(key);
        if (skey == null)
            return false;
        return settings.ContainsKey(skey);
    }

    object IDictionary.this[object key]
    {
        get
        {
            string s = ExtractKey(key);
            return (s == null) ? null : settings[s];
        }
        set
        {
            string s = ExtractKey(key);
            if (s == null)
                throw new ArgumentException("key");
            settings[s] = value;
        }
    }

    bool IDictionary.IsFixedSize
    {
        get { return false; }
    }

    bool IDictionary.IsReadOnly
    {
        get { return false; }
    }

    void IDictionary.Remove(object key)
    {
        string s = ExtractKey(key);
        if (s != null)
            settings.Remove(s);
    }


    void ICollection.CopyTo(Array array, int index)
    {
        (settings as ICollection).CopyTo(array, index);
    }

    bool ICollection.IsSynchronized
    {
        get { return (settings as ICollection).IsSynchronized; }
    }

    object ICollection.SyncRoot
    {
        get { return (settings as ICollection).SyncRoot; }
    }


    IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
    {
        return settings.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return settings.GetEnumerator();
    }


    IDictionaryEnumerator IDictionary.GetEnumerator()
    {
        return settings.GetEnumerator();
    }
}


namespace aEMR.Common.ConfigurationManager.Printer
{

    public class PrinterConfigurationManager
    {
        private static object _lockObj =  new object();
        private readonly IsolatedStorageSettings _appSettings = IsolatedStorageSettings.ApplicationSettings;

        private readonly string _printerTypePrefix = "printertype_";
        private readonly Regex _printerTypeRegEx = new Regex(@"^(printertype_)(\d+)$");
        /// <summary>
        /// Lay thong tin cac may in duoc gan cho tung loai.
        /// </summary>
        /// <returns></returns>
        public Dictionary<PrinterType,string> GetAllAssignedPrinterType()
        {
            var allItems = new Dictionary<PrinterType, string>();

            foreach (var key in _appSettings.Keys)
            {
                var match = _printerTypeRegEx.Match(key.ToString());
                string temp;
                int type;
                if(match.Success)
                {
                    temp = match.Groups[2].Value;
                    if(int.TryParse(temp, out type))
                    {
                        if (Enum.IsDefined(typeof(PrinterType), type) && _appSettings[key.ToString()] != null)
                        {
                            allItems.Add((PrinterType)type, _appSettings[key.ToString()].ToString());
                        }
                    }
                }
                
            }
            return allItems;
        }

        public void SavePrinterTypeAssignment(Dictionary<PrinterType, string> allItems)
        {
            string key = "";
            foreach (var item in allItems)
            {
                //Neu chua co thi add vo.
                //Co roi thi update.
                key = _printerTypePrefix + ((int)item.Key).ToString();
                if(_appSettings.Contains(key))
                {
                    _appSettings[key] = item.Value;
                }
                else
                {
                    _appSettings.Add(key,item.Value);
                }
            }
            _appSettings.Save();
        }
    }
}
