using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Mafi;
using Mafi.Collections;
using Mafi.Core.SaveGame.CustomSerializer;
using Mafi.Serialization;

namespace ProgramableNetwork.Utils;

// TODO replace with original game API when resolved
public class JsonConvert {

	private delegate void PropertyJsonConverter(
		PropertyInfo prop, object target, Dict<string, object> json, string name);

	private static readonly Dict<Type, PropertyJsonConverter> s_convertFrom = new() {
		[typeof(int)] = (prop, target, json, name) => {
			if (json.TryGetDouble(name, out double v)) {
				prop.SetValue(target, (int)v);
			}
		},
		[typeof(float)] = (prop, target, json, name) => {
			if (json.TryGetDouble(name, out double v)) {
				prop.SetValue(target, (float)v);
			}
		},
		[typeof(double)] = (prop, target, json, name) => {
			if (json.TryGetDouble(name, out double v)) {
				prop.SetValue(target, (double)v);
			}
		},
		[typeof(Fix32)] = (prop, target, json, name) => {
			if (json.TryGetDouble(name, out double v)) {
				prop.SetValue(target, (Fix32)v);
			}
		},
		[typeof(Fix64)] = (prop, target, json, name) => {
			if (json.TryGetDouble(name, out double v)) {
				prop.SetValue(target, (Fix64)v);
			}
		},
		[typeof(string)] = (prop, target, json, name) => {
			if (json.TryGetString(name, out string v)) {
				prop.SetValue(target, v);
			}
		},
		[typeof(bool)] = (prop, target, json, name) => {
			// boolean může přijít jako true/false nebo jako "true"/"false" nebo jako 0/1
			if (json.TryGetString(name, out string sv)) {
				if (bool.TryParse(sv, out bool bv)) {
					prop.SetValue(target, bv);
				}
			} else if (json.TryGetDouble(name, out double dv)) {
				prop.SetValue(target, dv != 0);
			}
		}
	};

	private static readonly Dict<Type, PropertyJsonConverter> s_convertTo = new() {
		[typeof(int)] = (prop, target, json, name) => json[name] = (double)(int) target,
		[typeof(float)] = (prop, target, json, name) => json[name] = (double)(float) target,
		[typeof(double)] = (prop, target, json, name) => json[name] = (double) target,
		[typeof(Fix32)] = (prop, target, json, name) => json[name] = ((Fix32) target).ToDouble(),
		[typeof(Fix64)] = (prop, target, json, name) => json[name] = ((Fix64) target).ToDouble(),
		[typeof(string)] = (prop, target, json, name) => json[name] = (string) target,
	};

	public static T DeserializeObject<T>(string jsonText) where T : new() {
		Dict<string, object> jsonObject = (Dict<string, object>)
			new JsonParser().Parse(new StringReader(jsonText));

		Type type = typeof(T);
		T result = new T();

		foreach (PropertyInfo prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
			if (prop.GetCustomAttribute<JsonIgnoreAttribute>() is not null) {
				continue;
			}
			string name = prop.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? prop.Name;

			Type memberType = prop.PropertyType;

			if (s_convertFrom.TryGetValue(memberType, out var converter)) {
				converter(prop, result, jsonObject, name);
			} else {
				throw new NotImplementedException($"Serialization of type: {memberType} is not implemented");
			}
		}

		return result;
	}

	public static string SerializeObject<T>(T entityInfo) {
		Type type = typeof(T);
		Dict<string, object> jsonObject = [];

		foreach (PropertyInfo prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
			if (prop.GetCustomAttribute<JsonIgnoreAttribute>() is not null) {
				continue;
			}
			string name = prop.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? prop.Name;

			Type memberType = prop.PropertyType;

			if (s_convertTo.TryGetValue(memberType, out var converter)) {
				converter(prop, prop.GetValue(entityInfo), jsonObject, name);
			} else {
				throw new NotImplementedException($"Serialization of type: {memberType} is not implemented");
			}
		}

		JsonWriter writer = new JsonWriter(1024);
		writer.AppendStartObject();
		foreach (KeyValuePair<string, object> kvp in jsonObject) {
			switch (kvp.Value) {
			case null: break;
			case double i: writer.AppendNumberField(kvp.Key, i); break;
			case string s: writer.AppendStringField(kvp.Key, s); break;
			case bool b: writer.AppendBoolField(kvp.Key, b); break;
			default: throw new NotImplementedException($"Not implemented serialization for: {kvp.Value.GetType()}");
			}
		}
		writer.AppendEndObject();
		return writer.GetJsonAndClear();
	}
}