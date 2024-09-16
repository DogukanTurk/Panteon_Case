using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DOT.Utilities
{
    /* ------------------------------------------ */

    [System.Serializable]
    public class EnumSelection
    {
        /* ------------------------------------------ */

        public int[] selectedValues = Array.Empty<int>();

        /* ------------------------------------------ */

        public bool Contains(int value)
        {
            for (int x = 0; x < selectedValues.Length; x++)
            {
                if (selectedValues[x].Equals(value))
                    return true;
            }

            return false;
        }

        public bool IsAnythingSelected()
        {
            if (selectedValues.Length > 0)
                return true;

            return false;
        }
        
        public int[] GetSelectedValues()
        {
            return Array.FindAll(selectedValues, x => x != 0 && x != 1);
        }

        /* ------------------------------------------ */
    }

    public class EnumSelectorAttribute : PropertyAttribute
    {
        public System.Type EnumType { get; private set; }

        public EnumSelectorAttribute(System.Type enumType)
        {
            if (!enumType.IsEnum)
            {
                throw new System.ArgumentException("Type must be an enum");
            }

            EnumType = enumType;
        }
    }
    
#if UNITY_EDITOR
    /* ------------------------------------------ */

    [CustomPropertyDrawer(typeof(EnumSelectorAttribute))]
    public class EnumSelectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty selectedValuesProp = property.FindPropertyRelative("selectedValues");

            EnumSelectorAttribute enumSelector = (EnumSelectorAttribute)attribute;
            Type enumType = enumSelector.EnumType;

            if (enumType == null || !enumType.IsEnum)
            {
                EditorGUI.LabelField(position, label.text, "PropertyDrawer is not valid for non-enum types.");
                return;
            }

            // Mevcut seçimleri al
            var selectedValues = GetSelectedValues(selectedValuesProp);

            // Enum değerlerini ve isimlerini eşle
            var enumValues = Enum.GetValues(enumType).Cast<int>().ToArray();
            var enumNames = Enum.GetNames(enumType);
            Dictionary<int, string> enumDict = enumValues.Zip(enumNames, (v, n) => new { Value = v, Name = n })
                .ToDictionary(x => x.Value, x => x.Name);

            position.height = EditorGUIUtility.singleLineHeight * 1 + 4;

            // Kutu içindeki alan
            Rect boxRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight * 2 + 4);
            EditorGUI.DrawRect(boxRect, Color.black); // Arka plan rengi

            // Kutu içine yerleştirilmiş buton ve etiketler
            Rect buttonRect = new Rect(position.x + (position.width / 2), position.y, position.width / 2,
                EditorGUIUtility.singleLineHeight);
            Rect labelRect = new Rect(position.x, position.y, position.width / 2,
                EditorGUIUtility.singleLineHeight);

            // Buton ve etiket
            GUI.Label(labelRect, $"{label.text}", EditorStyles.boldLabel);
            if (GUI.Button(buttonRect, "Select"))
            {
                GenericMenu menu = new GenericMenu();
                foreach (var kvp in enumDict)
                {
                    bool isSelected = selectedValues.Contains(kvp.Key);
                    menu.AddItem(new GUIContent(kvp.Value), isSelected,
                        () => ToggleSelection(kvp.Key, selectedValuesProp, enumDict));
                }

                menu.ShowAsContext();
            }

            // // Seçilen değerleri etiket olarak göster
            GUI.Label(
                new Rect(position.x,
                    position.y + EditorGUIUtility.singleLineHeight + 2,
                    position.width, EditorGUIUtility.singleLineHeight),
                "Selected: " + string.Join(", ", selectedValues.Select(v => enumDict[v])));
        }

        private void ToggleSelection(int value, SerializedProperty selectedValuesProp, Dictionary<int, string> enumDict)
        {
            var selectedValues = GetSelectedValues(selectedValuesProp).ToList();

            if (value == 0)
            {
                // None seçilmişse tüm listeyi temizle
                selectedValues.Clear();
            }
            else if (value == 1)
            {
                // Everything seçilmişse tüm diğer değerleri seç
                selectedValues.Clear();
                foreach (var kvp in enumDict)
                {
                    if (kvp.Key != 0) // None ve Everything hariç diğer tüm değerleri ekle
                        // if (kvp.Key != 0 && kvp.Key != 1) // None ve Everything hariç diğer tüm değerleri ekle
                    {
                        if (!selectedValues.Contains(kvp.Key))
                            selectedValues.Add(kvp.Key);
                    }
                }
            }
            else
            {
                // Diğer değerler seçiliyorsa veya seçimi kaldırıyorsak
                if (selectedValues.Contains(value))
                {
                    selectedValues.Remove(value);
                }
                else
                {
                    selectedValues.Add(value);
                }
            }

            // Güncellenmiş seçilmiş değerleri ayarla
            selectedValuesProp.ClearArray();
            for (int i = 0; i < selectedValues.Count; i++)
            {
                selectedValuesProp.InsertArrayElementAtIndex(i);
                selectedValuesProp.GetArrayElementAtIndex(i).intValue = selectedValues[i];
            }

            selectedValuesProp.serializedObject.ApplyModifiedProperties(); // Değişiklikleri uygula
        }

        private int[] GetSelectedValues(SerializedProperty selectedValuesProp)
        {
            var selectedValues = new int[selectedValuesProp.arraySize];
            for (int x = 0; x < selectedValuesProp.arraySize; x++)
                selectedValues[x] = selectedValuesProp.GetArrayElementAtIndex(x).intValue;

            return selectedValues;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2 + 4; // Buton ve etiketler için yükseklik
        }
    }

    /* ------------------------------------------ */
#endif
}
