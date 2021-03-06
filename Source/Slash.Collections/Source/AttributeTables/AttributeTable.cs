﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AttributeTable.cs" company="Slash Games">
//   Copyright (c) Slash Games. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Slash.Collections.AttributeTables
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    using Slash.Collections.Utils;
    using Slash.Reflection.Utils;
    using Slash.Serialization;
    using Slash.Serialization.Binary;
    using Slash.Serialization.Utils;
    using Slash.SystemExt.Utils;

    /// <summary>
    ///   Table that allows storing and looking up attributes and their
    ///   respective values.
    /// </summary>
    [Serializable]
    public class AttributeTable : IAttributeTable,
                                  IXmlSerializable,
                                  IBinarySerializable,
                                  IEnumerable<KeyValuePair<object, object>>
    {
        #region Constants

        private const string ItemElementName = "Attribute";

        private const string KeyElementName = "Key";

        private const string KeyTypeAttributeName = "keyType";

        private const string ValueElementName = "Value";

        private const string ValueTypeAttributeName = "valueType";

        #endregion

        #region Fields

        /// <summary>
        ///   Dictionary to store attributes.
        /// </summary>
        [SerializeMember]
        private readonly Dictionary<object, object> attributes;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///   Constructs a new, empty attribute table.
        /// </summary>
        public AttributeTable()
        {
            this.attributes = new Dictionary<object, object>();
        }

        /// <summary>
        ///   Constructs a new attribute table, copying all content of the passed
        ///   original one.
        /// </summary>
        /// <param name="original"> Attribute table to copy. </param>
        public AttributeTable(IAttributeTable original)
            : this()
        {
            this.AddRange(original);
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///   Number of attribute values stored in this table.
        /// </summary>
        public int Count
        {
            get
            {
                return this.attributes.Count;
            }
        }

        #endregion

        #region Public Indexers

        /// <summary>
        ///   Gets or sets the attribute with the specified key.
        ///   If no attribute is found when it should be returned, an exception is thrown.
        /// </summary>
        /// <param name="attributeKey"> Attribute key. </param>
        /// <returns> Value of attribute with the specified key. </returns>
        public object this[object attributeKey]
        {
            get
            {
                return this.GetValue(attributeKey);
            }

            set
            {
                this.SetValue(attributeKey, value);
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///   Maps the passed key to the specified value in this attribute table,
        ///   if it hasn't already been mapped before.
        /// </summary>
        /// <param name="key"> Key to map. </param>
        /// <param name="value"> Value to map the key to. </param>
        /// <exception cref="ArgumentException">An element with the same key already exists in the attribute table.</exception>
        public void Add(object key, object value)
        {
            this.AddValue(key, value);
        }

        /// <summary>
        ///   Adds the specified attribute pair (key/value) to the attribute table.
        /// </summary>
        /// <param name="attributePair">Attribute pair to add.</param>
        public void Add(KeyValuePair<object, object> attributePair)
        {
            this.AddValue(attributePair.Key, attributePair.Value);
        }

        /// <summary>
        ///   Adds the specified attribute pair (key/value) to the attribute table.
        /// </summary>
        /// <param name="attributePair">Attribute pair to add.</param>
        public void Add(object attributePair)
        {
            if (attributePair == null)
            {
                throw new ArgumentNullException("attributePair", "Attribute pair object is null.");
            }

            if (!(attributePair is KeyValuePair<object, object>))
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Attribute pair is not of type KeyValuePair<object, object>, but '{0}'.",
                        attributePair.GetType()));
            }

            this.Add((KeyValuePair<object, object>)attributePair);
        }

        /// <summary>
        ///   Adds all content of the passed attribute table to this one.
        /// </summary>
        /// <param name="attributeTable"> Table to add the content of. </param>
        public void AddRange(IAttributeTable attributeTable)
        {
            foreach (KeyValuePair<object, object> keyValuePair in attributeTable)
            {
                this.SetValue(keyValuePair.Key, keyValuePair.Value);
            }
        }

        /// <summary>
        ///   Maps the passed key to the specified value in this attribute table,
        ///   if it hasn't already been mapped before.
        /// </summary>
        /// <param name="key"> Key to map. </param>
        /// <param name="value"> Value to map the key to. </param>
        public virtual void AddValue(object key, object value)
        {
            this.attributes.Add(key, value);
        }

        /// <summary>
        ///   Clears the attribute table.
        /// </summary>
        public virtual void Clear()
        {
            this.attributes.Clear();
        }

        /// <summary>
        ///   Returns <c>true</c> if the passed key is mapped within this
        ///   attribute table, and <c>false</c> otherwise.
        /// </summary>
        /// <param name="key"> Key to check. </param>
        /// <returns> True if the passed key is mapped within this attribute table. </returns>
        public virtual bool Contains(object key)
        {
            return this.attributes.ContainsKey(key);
        }

        /// <summary>
        ///   Reads this object from its binary representation.
        /// </summary>
        /// <param name="deserializer">Deserializer to read the object with.</param>
        public void Deserialize(BinaryDeserializer deserializer)
        {
            Dictionary<object, object> attributeTable = deserializer.Deserialize<Dictionary<object, object>>();
            foreach (KeyValuePair<object, object> keyValuePair in attributeTable)
            {
                this.attributes[keyValuePair.Key] = keyValuePair.Value;
            }
        }

        /// <summary>
        ///   Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />.
        /// </summary>
        /// <returns>
        ///   true if the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />; otherwise, false.
        /// </returns>
        /// <param name="obj">
        ///   The <see cref="T:System.Object" /> to compare with the current <see cref="T:System.Object" />.
        /// </param>
        /// <filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return this.Equals((AttributeTable)obj);
        }

        /// <summary>
        ///   Gets an enumerator over all attributes of this table.
        /// </summary>
        /// <returns>All attributes of this table.</returns>
        public IEnumerator GetEnumerator()
        {
            return this.attributes.GetEnumerator();
        }

        /// <summary>
        ///   Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        ///   A hash code for the current <see cref="T:System.Object" />.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return this.attributes != null ? this.attributes.GetHashCode() : 0;
        }

        /// <summary>
        ///   This method is reserved and should not be used. When implementing the IXmlSerializable interface, you should return null (Nothing in Visual Basic) from this method, and instead, if specifying a custom schema is required, apply the
        ///   <see
        ///     cref="T:System.Xml.Serialization.XmlSchemaProviderAttribute" />
        ///   to the class.
        /// </summary>
        /// <returns>
        ///   An <see cref="T:System.Xml.Schema.XmlSchema" /> that describes the XML representation of the object that is produced by the
        ///   <see
        ///     cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)" />
        ///   method and consumed by the
        ///   <see
        ///     cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)" />
        ///   method.
        /// </returns>
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        ///   Returns the attribute with the specified key.
        ///   If no attribute is found, an exception is thrown.
        /// </summary>
        /// <param name="attributeKey"> Attribute key. </param>
        /// <returns> Value of attribute with the specified key. </returns>
        /// <exception cref="KeyNotFoundException">Specified key wasn't found.</exception>
        public virtual object GetValue(object attributeKey)
        {
            try
            {
                return this.attributes[attributeKey];
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException(string.Format("Attribute key not found: {0}", attributeKey));
            }
        }

        /// <summary>
        ///   Returns the attribute with the specified key casted to the specified type.
        ///   If no attribute with the specified key is found or the attribute can't be casted
        ///   to the specified type, an exception is thrown.
        /// </summary>
        /// <typeparam name="T"> Expected type of attribute. </typeparam>
        /// <param name="attributeKey"> Attribute key. </param>
        /// <returns> Value of attribute of the specified type with the specified key. </returns>
        /// <exception cref="KeyNotFoundException">Specified key wasn't found.</exception>
        /// <exception cref="InvalidCastException">Attribute was found but couldn't be casted to specified type.</exception>
        public T GetValue<T>(object attributeKey) where T : class
        {
            object attributeValue = this.GetValue(attributeKey);
            return (T)attributeValue;
        }

        /// <summary>
        ///   Generates an object from its XML representation.
        /// </summary>
        /// <param name="reader">
        ///   The <see cref="T:System.Xml.XmlReader" /> stream from which the object is deserialized.
        /// </param>
        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            bool isEmpty = reader.IsEmptyElement;
            reader.ReadStartElement();
            if (isEmpty)
            {
                return;
            }

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    bool isEmptyElement = reader.IsEmptyElement;
                    string elementName = reader.Name;
                    if (elementName == ItemElementName)
                    {
                        string keyTypeString = reader.GetAttribute(KeyTypeAttributeName);
                        Type keyType = ReflectionUtils.FindType(keyTypeString);

                        string valueTypeString = reader.GetAttribute(ValueTypeAttributeName);
                        Type valueType = ReflectionUtils.FindType(valueTypeString);

                        reader.ReadStartElement();
                        KeyValuePair<object, object> attributePair = this.ReadAttributeXml(reader, keyType, valueType);
                        this.attributes.Add(attributePair.Key, attributePair.Value);
                    }
                    else
                    {
                        reader.ReadStartElement();
                    }

                    if (!isEmptyElement)
                    {
                        reader.ReadEndElement();
                    }
                }
                else
                {
                    reader.Read();
                }
            }

            reader.ReadEndElement();
        }

        /// <summary>
        ///   Removes the passed key from this attribute table.
        /// </summary>
        /// <param name="key"> Key to remove. </param>
        /// <returns>
        ///   <c>true</c>, if the key has been removed, and <c>false</c> otherwise.
        /// </returns>
        public virtual bool RemoveValue(object key)
        {
            return this.attributes.Remove(key);
        }

        /// <summary>
        ///   Converts this object to its binary representation.
        /// </summary>
        /// <param name="serializer">Serializer to writer this object with.</param>
        public void Serialize(BinarySerializer serializer)
        {
            serializer.Serialize(this.attributes);
        }

        /// <summary>
        ///   Maps the passed key to the specified value in this attribute table,
        ///   if it has already been mapped before.
        /// </summary>
        /// <param name="key"> Key to map. </param>
        /// <param name="value"> Value to map the key to. </param>
        public virtual void SetValue(object key, object value)
        {
            this.attributes[key] = value;
        }

        /// <summary>
        ///   Tries to retrieve the value the passed key is mapped to within this
        ///   attribute table.
        /// </summary>
        /// <param name="key"> Key to retrieve the value of. </param>
        /// <param name="value"> Retrieved value. </param>
        /// <returns> true if a value was found, and false otherwise. </returns>
        public virtual bool TryGetValue(object key, out object value)
        {
            return this.attributes.TryGetValue(key, out value);
        }

        /// <summary>
        ///   Converts an object into its XML representation.
        /// </summary>
        /// <param name="writer">
        ///   The <see cref="T:System.Xml.XmlWriter" /> stream to which the object is serialized.
        /// </param>
        public void WriteXml(XmlWriter writer)
        {
            if (this.attributes == null || this.attributes.Count == 0)
            {
                return;
            }

            foreach (var attributePair in this.attributes)
            {
                this.WriteAttributeXml(writer, attributePair);
            }
        }

        #endregion

        #region Explicit Interface Methods

        IEnumerator<KeyValuePair<object, object>> IEnumerable<KeyValuePair<object, object>>.GetEnumerator()
        {
            return this.attributes.GetEnumerator();
        }

        #endregion

        #region Methods

        /// <summary>
        ///   Determines whether the specified <see cref="AttributeTable" /> is equal to the current <see cref="AttributeTable" />.
        /// </summary>
        /// <returns>
        ///   true if the specified <see cref="AttributeTable" /> is equal to the current <see cref="AttributeTable" />; otherwise, false.
        /// </returns>
        /// <param name="other">
        ///   The <see cref="AttributeTable" /> to compare with the current <see cref="AttributeTable" />.
        /// </param>
        protected bool Equals(AttributeTable other)
        {
            return CollectionUtils.DictionaryEqual(this.attributes, other.attributes);
        }

        private KeyValuePair<object, object> ReadAttributeXml(XmlReader reader, Type keyType, Type valueType)
        {
            object key = null;
            object value = null;

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    string elementName = reader.Name;

                    if (elementName == KeyElementName)
                    {
                        key = SerializationUtils.ReadXml(reader, keyType, KeyElementName);
                    }
                    else if (elementName == ValueElementName)
                    {
                        value = SerializationUtils.ReadXml(reader, valueType, ValueElementName);
                    }
                    else
                    {
                        reader.Read();
                    }
                }
                else
                {
                    reader.Read();
                }
            }

            return new KeyValuePair<object, object>(key, value);
        }

        private void WriteAttributeXml(XmlWriter writer, KeyValuePair<object, object> attributePair)
        {
            writer.WriteStartElement(ItemElementName);
            writer.WriteAttributeString(KeyTypeAttributeName, attributePair.Key.GetType().FullNameWithoutAssemblyInfo());
            if (attributePair.Value != null)
            {
                writer.WriteAttributeString(
                    ValueTypeAttributeName, attributePair.Value.GetType().FullNameWithoutAssemblyInfo());
            }

            SerializationUtils.WriteXml(writer, attributePair.Key, KeyElementName);
            if (attributePair.Value != null)
            {
                SerializationUtils.WriteXml(writer, attributePair.Value, ValueElementName);
            }

            writer.WriteEndElement();
        }

        #endregion
    }
}