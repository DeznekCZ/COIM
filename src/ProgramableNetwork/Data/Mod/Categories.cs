﻿using Mafi.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;

namespace ProgramableNetwork
{
    public class Category
    {
        public static List<Category> Categories(ProtosDb protos, Controller controller)
        {
            return protos.All<ModuleProto>()
                .Where(m => m.AllowedDevices.Contains(controller.Prototype.Id))
                .SelectMany(m => m.Categories)
                .Distinct()
                .OrderBy(m => m.Name)
                .ToList();
        }

        public Category(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Id { get; }
        public string Name { get; }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is Category cat && cat.Id == Id;
        }

        public override string ToString()
        {
            return $"Category(Id:{Id}, Name:{Name})";
        }

        public static Dictionary<string, Category> Categories()
        {
            Dictionary<string, Category> categories = new Dictionary<string, Category>();

            System.Reflection.PropertyInfo[] properties = typeof(Category).GetProperties();
            foreach (System.Reflection.PropertyInfo item in properties)
            {
                if (item.PropertyType == typeof(Category))
                {
                    categories.Add(item.Name, (Category)item.GetValue(null));
                }
            }

            return categories;
        }

        public static bool operator ==(Category a, Category b) => a?.Id == b?.Id;
        public static bool operator !=(Category a, Category b) => a?.Id != b?.Id;

        //// known types
        public static Category Display { get; } = new Category(id: "display", name: "Display modules");
        public static Category Connection { get; } = new Category(id: "connection", name: "Connection modules");
        public static Category ConnectionRead { get; } = new Category(id: "connection", name: "Connection modules (read)");
        public static Category ConnectionWrite { get; } = new Category(id: "connection", name: "Connection modules (write)");
        public static Category Arithmetic { get; } = new Category(id: "arithmetic", name: "Arithmetic modules");
        public static Category Boolean { get; } = new Category(id: "boolean", name: "Boolean modules");
        public static Category Decision { get; } = new Category(id: "decision", name: "Decision modules");
        public static Category Control { get; } = new Category(id: "control", name: "Control modules");
        public static Category Stats { get; } = new Category(id: "stats", name: "Stats modules");
        public static Category Antene { get; } = new Category(id: "antene", name: "Antena modules");
        public static Category AnteneFM { get; } = new Category(id: "antene_fm", name: "FM modules");
        public static Category AnteneAM { get; } = new Category(id: "antene_am", name: "AM modules");
    }
}