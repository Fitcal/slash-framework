﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EditorGUIUtils.cs" company="Slash Games">
//   Copyright (c) Slash Games. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Slash.Unity.Editor.Common.Inspectors.Utils
{
    using System;
    using System.Collections;

    using Slash.ECS.Inspector.Attributes;

    using UnityEditor;

    using UnityEngine;

    using Object = UnityEngine.Object;

    /// <summary>
    ///   Additional EditorGUILayout kind methods.
    /// </summary>
    public static class EditorGUIUtils
    {
        #region Public Methods and Operators

        /// <summary>
        ///   Draws an inspector for modifying the specified array.
        /// </summary>
        /// <typeparam name="T">Type of the array to draw an inspector for.</typeparam>
        /// <param name="foldout">Whether to show all array entries, or not.</param>
        /// <param name="foldoutText">Text to show next to the array editor.</param>
        /// <param name="array">Array to draw the inspector for.</param>
        /// <returns>Whether to show all array entries, or not.</returns>
        public static bool ArrayField<T>(bool foldout, GUIContent foldoutText, ref T[] array) where T : Object
        {
            IList newArray;
            bool newFoldout = ListField(foldout, foldoutText, array, i => new T[i], typeof(T), out newArray);
            array = (T[])newArray;
            return newFoldout;
        }

        /// <summary>
        ///   Draws an inspector for modifying the specified list.
        /// </summary>
        /// <param name="foldout">Whether to show all list entries, or not.</param>
        /// <param name="foldoutText">Text to show next to the list editor.</param>
        /// <param name="list">List to draw the inspector for.</param>
        /// <param name="createList">Method for creating a new list if the size should be changed.</param>
        /// <param name="objectType">Unity object type of the list items.</param>
        /// <param name="newList">Modified list.</param>
        /// <returns>Whether to show all list entries, or not.</returns>
        public static bool ListField(
            bool foldout,
            GUIContent foldoutText,
            IList list,
            Func<int, IList> createList,
            Type objectType,
            out IList newList)
        {
            return ListField(
                foldout,
                foldoutText,
                list,
                createList,
                (obj, index) => EditorGUILayout.ObjectField("Element " + index, (Object)obj, objectType, false),
                out newList);
        }

        /// <summary>
        ///   Draws an inspector for modifying the specified list.
        /// </summary>
        /// <param name="foldout">Whether to show all list entries, or not.</param>
        /// <param name="foldoutText">Text to show next to the list editor.</param>
        /// <param name="list">List to draw the inspector for.</param>
        /// <param name="createList">Method for creating a new list if the size should be changed.</param>
        /// <param name="editItem">Method for changing a specific list item.</param>
        /// <param name="newList">Modified list.</param>
        /// <returns>Whether to show all list entries, or not.</returns>
        public static bool ListField(
            bool foldout,
            GUIContent foldoutText,
            IList list,
            Func<int, IList> createList,
            Func<object, int, object> editItem,
            out IList newList)
        {
            foldout = EditorGUILayout.Foldout(foldout, foldoutText);
            if (foldout)
            {
                EditorGUI.indentLevel++;

                int currentSize = list != null ? list.Count : 0;
                int newSize = EditorGUILayout.IntField("Size", currentSize);
                if (currentSize != newSize)
                {
                    newList = createList(newSize);
                    for (int x = 0; x < newSize; x++)
                    {
                        if (x < currentSize)
                        {
                            newList[x] = list != null ? list[x] : null;
                        }
                    }
                }
                else
                {
                    newList = list;
                    list = newList;
                }

                for (int x = 0; x < currentSize; x++)
                {
                    list[x] = editItem(list[x], x);
                }

                EditorGUI.indentLevel--;
            }
            else
            {
                newList = list;
            }

            return foldout;
        }

        /// <summary>
        ///   Draws an inspector for modifying the specified list.
        /// </summary>
        /// <typeparam name="T">Type of the list items.</typeparam>
        /// <param name="foldout">Whether to show all list entries, or not.</param>
        /// <param name="foldoutText">Text to show next to the list editor.</param>
        /// <param name="list">List to draw the inspector for.</param>
        /// <param name="createList">Method for creating a new list if the size should be changed.</param>
        /// <returns>Whether to show all list entries, or not.</returns>
        public static bool ListField<T>(
            bool foldout, GUIContent foldoutText, ref IList list, Func<int, IList> createList) where T : Object
        {
            IList newArray;
            bool newFoldout = ListField(foldout, foldoutText, list, createList, typeof(T), out newArray);
            list = newArray;
            return newFoldout;
        }

        /// <summary>
        ///   Draws an inspector for the specified logic property.
        /// </summary>
        /// <param name="inspectorProperty">Logic property to draw the inspector for.</param>
        /// <param name="currentValue">Current logic property value.</param>
        /// <param name="label">Text to show next to the property editor.</param>
        /// <returns>New logic property value.</returns>
        public static object LogicInspectorPropertyField(
            InspectorPropertyAttribute inspectorProperty, object currentValue, GUIContent label)
        {
            // Draw inspector control.
            if (inspectorProperty is InspectorBoolAttribute)
            {
                return EditorGUILayout.Toggle(label, Convert.ToBoolean(currentValue));
            }
            if (inspectorProperty is InspectorStringAttribute || inspectorProperty is InspectorBlueprintAttribute)
            {
                return EditorGUILayout.TextField(label, Convert.ToString(currentValue));
            }
            if (inspectorProperty is InspectorFloatAttribute)
            {
                return EditorGUILayout.FloatField(label, Convert.ToSingle(currentValue));
            }
            if (inspectorProperty is InspectorIntAttribute)
            {
                return EditorGUILayout.IntField(label, Convert.ToInt32(currentValue));
            }
            InspectorEnumAttribute enumInspectorProperty = inspectorProperty as InspectorEnumAttribute;
            if (enumInspectorProperty != null)
            {
                return EditorGUILayout.EnumPopup(
                    label, (Enum)Convert.ChangeType(currentValue, enumInspectorProperty.PropertyType));
            }

            EditorGUILayout.HelpBox(
                string.Format("No inspector found for property type '{0}'.", inspectorProperty.GetType().Name),
                MessageType.Warning);
            return currentValue;
        }

        /// <summary>
        ///   Draws an inspector for the specified logic property.
        /// </summary>
        /// <param name="inspectorProperty">Logic property to draw the inspector for.</param>
        /// <param name="currentValue">Current logic property value.</param>
        /// <returns>New logic property value.</returns>
        public static object LogicInspectorPropertyField(
            InspectorPropertyAttribute inspectorProperty, object currentValue)
        {
            return LogicInspectorPropertyField(
                inspectorProperty, currentValue, new GUIContent(inspectorProperty.Name, inspectorProperty.Description));
        }

        /// <summary>
        ///   Draws an inspector for the specified shader.
        /// </summary>
        /// <param name="shaderContext">Shader to draw the inspector for.</param>
        /// <param name="label">Text to show next to the shader editor.</param>
        /// <returns>New selected shader name.</returns>
        public static string ShaderField(ShaderContext shaderContext, string label)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            string selectedShaderName = shaderContext.SelectedShader;
            if (GUILayout.Button(selectedShaderName))
            {
                shaderContext.SelectedShader = selectedShaderName;
                shaderContext.DisplayShaderContext(GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.popup));
            }
            EditorGUILayout.EndHorizontal();

            return selectedShaderName;
        }

        #endregion
    }
}