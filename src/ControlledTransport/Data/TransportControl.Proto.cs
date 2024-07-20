using Mafi;
using Mafi.Base;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Mods;
using Mafi.Core.Ports;
using Mafi.Core.Ports.Io;
using Mafi.Core.Prototypes;
using Mafi.Unity;
using Mafi.Unity.InputControl.Inspectors;
using Mafi.Unity.UiFramework.Components;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Mafi.Unity.UiFramework;
using Mafi.Serialization;
using Mafi.Core.Buildings.Storages;
using Mafi.Core.Factory.Transports;
using Mafi.Core.Products;

namespace ControlledTransport
{

    public class TransportControlProto : LayoutEntityProto, ILayoutEntityProto, IProtoWithPropertiesUpdate
    {
        public override Type EntityType { get; } = typeof(TransportControl);
        public Func<ProductProto, bool> Filter { get; }

        public TransportControlProto(ID id, Str strings, EntityLayout layout, EntityCosts costs, Gfx graphics, ProductType filter, Duration? constructionDurationPerProduct = null, Upoints? boostCost = null, bool cannotBeBuiltByPlayer = false, bool isUnique = false, bool cannotBeReflected = false, bool autoBuildMiniZippers = false, bool doNotStartConstructionAutomatically = false, IEnumerable<Tag> tags = null)
            : base(id, strings, layout, costs, graphics, constructionDurationPerProduct, boostCost, cannotBeBuiltByPlayer, isUnique, cannotBeReflected, autoBuildMiniZippers, doNotStartConstructionAutomatically, tags)
        {
            this.Filter = proto => proto.IsAvailable && proto.Type == filter;
        }
    }
}
