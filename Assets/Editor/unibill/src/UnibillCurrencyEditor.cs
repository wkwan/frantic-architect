using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Linq;
using UnityEngine;
using UnityEditor;
using Unibill.Impl;

public class UnibillCurrencyEditor : InventoryEditor.IPlatformEditor {

	private static List<EditableCurrency> toRemove = new List<EditableCurrency> ();
	private List<EditableCurrency> currencies = new List<EditableCurrency> ();
    private UnibillConfiguration config;

	public UnibillCurrencyEditor(UnibillConfiguration config) {
        this.config = config;
		foreach (var currency in config.currencies) {
			this.currencies.Add (new EditableCurrency (currency));
		}
	}

    public string DisplayName() {
        return "TODO";
    }

	public void onGUI () {
		EditorGUILayout.Space();
		EditorGUILayout.LabelField ("Currencies:");
		EditorGUILayout.Space();
		EditorGUILayout.BeginVertical (GUI.skin.box);

		foreach (var currency in currencies) {
			currency.onGUI ();
		}

		if (GUILayout.Button ("Add currency...")) {
			var c = new VirtualCurrency("Name me!", new Dictionary<string, decimal>());
			config.currencies.Add(c);
			currencies.Add (new EditableCurrency(c));
		}

		EditorGUILayout.EndVertical ();

		currencies.RemoveAll (x => toRemove.Contains (x));
        foreach (var currency in toRemove) {
            config.currencies.Remove(currency.currency);
        }
		toRemove.Clear ();
	}

	private class EditableCurrency : InventoryEditor.IPlatformEditor {

		private bool visible;
		private string id = "Name me!";
		private List<CurrencyMapping> mappings = new List<CurrencyMapping>();
		private List<CurrencyMapping> mappingsToRemove = new List<CurrencyMapping>();
        public VirtualCurrency currency;

        public string DisplayName() {
            return id;
        }

		public EditableCurrency(VirtualCurrency currency) {
            this.currency = currency;
            id = currency.currencyId;
			foreach (var mapping in currency.mappings) {
				mappings.Add(new CurrencyMapping(mapping.Key, (int)mapping.Value, this));
			}
		}

		public void onGUI () {
			var box = EditorGUILayout.BeginVertical (GUI.skin.box);

			Rect rect = new Rect (box.xMax - 20, box.yMin - 2, 20, 20);
			if (GUI.Button (rect, "x")) {
				toRemove.Add (this);
			}

			GUIStyle s = new GUIStyle (EditorStyles.foldout);
			this.visible = EditorGUILayout.Foldout (visible, id, s);
			if (visible) {
				id = EditorGUILayout.TextField ("Currency ID:", id);
				currency.currencyId = id;
				EditorGUILayout.LabelField ("Mappings:");
				EditorGUILayout.BeginVertical (GUI.skin.box);
				foreach (var mapping in mappings) {
					mapping.onGUI ();
				}

				if (GUILayout.Button (string.Format("Add mapping for {0}", id))) {
					mappings.Add (new CurrencyMapping(this));
				}

				EditorGUILayout.EndVertical ();
			}

			EditorGUILayout.EndVertical ();


			mappings.RemoveAll (x => this.mappingsToRemove.Contains (x));
            foreach (var mapping in mappingsToRemove) {
                currency.mappings.Remove(mapping.id);
            }
            mappingsToRemove.Clear();
		}

		private class CurrencyMapping : InventoryEditor.IPlatformEditor {

			private EditableCurrency parent;
			private int amount;
			public string id;
			private int selectedItemIndex;
            

			public CurrencyMapping(EditableCurrency parent) {
				this.parent = parent;
			}

            public string DisplayName() {
                return "TODO";
            }

			public CurrencyMapping(string id, int amount, EditableCurrency parent) {
                this.id = id;
                this.amount = amount;
				this.parent = parent;
				var items = InventoryEditor.consumableIds();
				for (int t = 0; t < items.Count(); t++) {
					if (items[t] == this.id) {
						selectedItemIndex = t;
						break;
					}
				}
			}

			public void onGUI () {
				var box = EditorGUILayout.BeginVertical (GUI.skin.box);

				EditorGUILayout.LabelField ("When the following consumable is purchased:");
				Rect rect = new Rect (box.xMax - 20, box.yMin - 2, 20, 20);
				if (GUI.Button (rect, "x")) {
					parent.mappingsToRemove.Add(this);
				}

				var ids = InventoryEditor.consumableIds ();
				selectedItemIndex = EditorGUILayout.Popup (selectedItemIndex, ids);
				if (0 <= selectedItemIndex && selectedItemIndex < ids.Length) {
					id = ids [selectedItemIndex];
				}

				EditorGUILayout.LabelField (string.Format("Give this much {0}:", parent.id));
				amount = EditorGUILayout.IntField (amount);
                parent.currency.mappings[id] = amount;
				EditorGUILayout.EndVertical ();
			}

			public XElement serialise () {
                throw new NotImplementedException();
			}
		}
	}
}
