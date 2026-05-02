using Mafi;
using Mafi.Base;
using Mafi.Base.Prototypes.Buildings.ThermalStorages;
using Mafi.Base.Prototypes.Trains;
using Mafi.Core;
using Mafi.Core.Buildings.Cargo.Modules;
using Mafi.Core.Buildings.Farms;
using Mafi.Core.Buildings.Mine;
using Mafi.Core.Buildings.Offices;
using Mafi.Core.Buildings.Settlements;
using Mafi.Core.Buildings.Storages;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Dynamic;
using Mafi.Core.Entities.Priorities;
using Mafi.Core.Entities.Static;
using Mafi.Core.Factory.ElectricPower;
using Mafi.Core.Factory.Machines;
using Mafi.Core.Factory.NuclearReactors;
using Mafi.Core.Factory.Recipes;
using Mafi.Core.Factory.Sorters;
using Mafi.Core.Factory.WellPumps;
using Mafi.Core.Maintenance;
using Mafi.Core.Mods;
using Mafi.Core.Population;
using Mafi.Core.Products;
using Mafi.Core.Trains;
using Mafi.Core.Vehicles;
using Mafi.Unity.InputControl;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using ProgramableNetwork.Data.Antene;
using ProgramableNetwork.Data.DisplayEntity;
using ProgramableNetwork.Data.DisplayEntity.Displays;
using ProgramableNetwork.Data.Modules;
using ProgramableNetwork.Data.Speaker;
using ProgramableNetwork.Data.Variables;
using ProgramableNetwork.Ui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mafi.Base.Prototypes.Machines.PowerGenerators;
using Mafi.Core.Factory.MechanicalPower;
using Mafi.Localization;
using Mafi.Unity.Ui.Library;
using static Mafi.Unity.Assets.Unity;
using static Mafi.Unity.Ui.Library.LogisticsZoneUIComponents;
using CargoDepot = Mafi.Core.Buildings.Cargo.CargoDepot;
using LayoutEntity = Mafi.Core.Entities.Static.Layout.LayoutEntity;
using Transport = Mafi.Core.Factory.Transports.Transport;
using Vehicle = Mafi.Core.Entities.Dynamic.Vehicle;

namespace ProgramableNetwork; 

// TODO splip-up the implementations
public class Modules : ModuleGroup, IModuleGroup {

	public override void RegisterData(ProtoRegistrator registrator) {

		Constants(registrator);
		Buttons(registrator);
		Variables(registrator);
		Arithmetic(registrator);
		Comparation(registrator);
		Connections(registrator);
		Stats(registrator);
		Forks(registrator);
		Booleans(registrator);
		Decisions(registrator);
		Display(registrator);
		RadioAM(registrator);
		RadioFM(registrator);
		// Plc lives in Plc.cs as its own ModuleGroup; auto-registered by
		// ModDefinition's RegisterDataWithInterface<IModuleGroup>().

		// SPECIAL
		registrator
			.ModuleBuilderStart("Game_Pause", "Pause game (DEBUG)", "GP")
			.SetDescription("Debug-only module: when input <b>pause</b> > 0, requests a global game pause via GameSpeedController and lights the info indicator.")
			.AddCategory(Category.Control)
			.AddInput("pause", "Pause")
			.Action(m => {
				m.Info = false;
				if (m.Input["pause", 0] > Fix32.Zero) {
					GlobalDependencyResolver.Get<GameSpeedController>().RequestPause();
					m.Info = true;
				} else {
					m.Info = false;
				}
			})
			.AddControllerDevice()
			.BuildAndAdd();
	}

	private void Constants(ProtoRegistrator registrator) {
		registrator
			.ModuleBuilderStart("Constant", "Constant (integer)", "#I")
			.SetDescription("Outputs the integer stored in the <b>number</b> field on output <b>value</b>. Used as a literal source in arithmetic chains.")
			.AddCategory(Category.Arithmetic)
			.AddCategory(Category.Constants)
			.AddOutput("value", "Value")
			.AddInt32Field("number", "Number")
			.Action(m => { m.Output["value"] = m.Field["number"]; })
			.AddDisplay("number", "Value", 1)
			.Display(m => {
				var s = m.Field.Integer["number"].ToString();
				m.Display["number"] = s.Length > 3 ? s.Substring(s.Length - 3) : s;
			})
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Constant_Hex", "Constant (hex)", "#H")
			.SetDescription("Outputs the hexadecimal integer stored in the <b>number</b> field on output <b>value</b>. Useful as a bitmask source for boolean/bit operations.")
			.AddCategory(Category.Arithmetic)
			.AddCategory(Category.Constants)
			.AddOutput("value", "Value")
			.AddHexInt32Field("number", "Number")
			.Action(m => { m.Output["value"] = m.Field["number"]; })
			.AddDisplay("number", "Value", 1)
			.Display(m => {
				var s = m.Field["number"].RawValue.ToString("X");
				m.Display["number"] = s.Length > 3 ? s.Substring(s.Length - 3) : s;
			})
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Constant_Product", "Constant (product)", "#P")
			.SetDescription("Outputs the product slim-id selected in the <b>product</b> field on output <b>value</b>. Pair with filter/sorter modules to identify a product type.")
			.AddCategory(Category.Arithmetic)
			.AddCategory(Category.Constants)
			.AddOutput("value", "Value")
			.AddProductField("product", "Product")
			.Action(m => { m.Output["value"] = m.Field["product"]; })
			.AddDisplay("product", "Product", 1, image: true)
			.Display(m => { m.Display["product"] = m.Field.Product("product")?.IconPath; })
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Constant_Crop", "Constant (crop)", "#C")
			.SetDescription("Outputs the crop product slim-id selected in the <b>crop</b> field on output <b>value</b>. Field is restricted to crop products only via FarmProductFilter.")
			.AddCategory(Category.Arithmetic)
			.AddCategory(Category.Constants)
			.AddOutput("value", "Value")
			.AddProductField("crop", "Crop", filter: FarmProductFilter)
			.Action(m => { m.Output["value"] = m.Field["crop"]; })
			.AddDisplay("crop", "Crop", 1, image: true)
			.Display(m => { m.Display["crop"] = m.Field.Product("crop")?.IconPath; })
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Constant_Machine", "Constant (machine)", "#M")
			.SetDescription("Outputs the MachineProto id selected in the <b>machine</b> field on output <b>value</b>. Used to identify a machine type for downstream connection modules.")
			.AddCategory(Category.Arithmetic)
			.AddCategory(Category.Constants)
			.AddOutput("value", "Value")
			.AddEntityTypeField<MachineProto>("machine", "Machine")
			.Action(m => { m.Output["value"] = m.Field["machine"]; })
			.AddDisplay("machine", "Machine", 1, image: true)
			.Display(m => { m.Display["machine"] = m.Field.EntityProtoIconified("machine")?.IconPath; })
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Constant_Vehicle", "Constant (vehicle)", "#V")
			.SetDescription("Outputs the DrivingEntityProto (vehicle) id selected in the <b>vehicle</b> field on output <b>value</b>. Used to identify a vehicle type for downstream connection modules.")
			.AddCategory(Category.Arithmetic)
			.AddCategory(Category.Constants)
			.AddOutput("value", "Value")
			.AddEntityTypeField<DrivingEntityProto>("vehicle", "vehicle")
			.Action(m => { m.Output["value"] = m.Field["vehicle"]; })
			.AddDisplay("vehicle", "Vehicle", 1, image: true)
			.Display(m => { m.Display["vehicle"] = m.Field.EntityProtoIconified("vehicle")?.IconPath; })
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Constant_Boolean", "Constant (boolean)", "#B")
			.SetDescription("Outputs 1 if the <b>boolean</b> field is checked, 0 otherwise. The output value is a Fix32 of 0 or 1.")
			.AddCategory(Category.Arithmetic)
			.AddCategory(Category.Constants)
			.AddOutput("value", "Value")
			.AddBooleanField("boolean", "Boolean")
			.Action(m => { m.Output["value"] = m.Field["boolean"]; })
			.AddDisplay("boolean", "On", 1, led: true)
			.Display(m => { m.Display["boolean"] = m.Field.Bool["boolean"] ? "1" : ""; })
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Constant_Float", "Constant (float)", "#F")
			.SetDescription("Outputs the fixed-point decimal stored in the <b>float</b> field on output <b>value</b>. Use when fractional constants are needed (e.g. ratios, multipliers).")
			.AddCategory(Category.Arithmetic)
			.AddCategory(Category.Constants)
			.AddOutput("value", "Value")
			.AddFix32Field("float", "Float")
			.Action(m => { m.Output["value"] = m.Field["float"]; })
			.AddDisplay("number", "Value", 1)
			.Display(m => {
				var s = m.Field["float"].ToStringRounded(1);
				m.Display["float"] = s.Length > 3 ? s.Substring(s.Length - 3) : s;
			})
			.AddControllerDevice()
			.BuildAndAdd();
	}

	private void Buttons(ProtoRegistrator registrator) {
		registrator
			.ModuleBuilderStart("Button_1", "Button (on/off)", "0/I")
			.SetDescription("Manual on/off toggle. Output <b>value</b> is 1 while the toggle display is active, 0 when inactive. Player clicks the display to flip it.")
			.AddCategory(Category.Control)
			.AddOutput("value", "On - 1, Off - 0")
			.AddDisplay("toggle", "Toggle", 1, toggle: new[] { "( | )" })
			.Action(m => m.Output["value"] = (m.Display["toggle", ""].Length > 0) ? Fix32.One : Fix32.Zero)
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Button_Pass", "Button (pass value)", "0/I")
			.SetDescription("Manual gate. When the toggle display is active, passes input <b>value</b> straight through to output <b>value</b>; when inactive, outputs 0.")
			.AddCategory(Category.Control)
			.AddInput("value", "Value")
			.AddOutput("value", "Value")
			.AddDisplay("toggle", "Toggle", 1, toggle: new string[] { "⬇" })
			.Action(m => m.Output["value"] = (m.Display["toggle", ""].Length > 0) ? m.Input["value"] : Fix32.Zero)
			.AddControllerDevice()
			.BuildAndAdd();
	}

	private void Arithmetic(ProtoRegistrator registrator) {
		registrator
			.ModuleBuilderStart("Sum", "C = A + B", "A+B")
			.SetDescription("Outputs <b>a</b> + <b>b</b> to <b>c</b>. If the <b>field_b</b> toggle is on, the constant <b>b</b> field is used instead of the input pin (see FieldOrInput).")
			.AddCategory(Category.Arithmetic)
			.AddInput("a", "A")
			.AddInput("b", "B")
			.AddFix32Field("b", "B", overrideInput: true)
			.AddOutput("c", "Sum")
			.Action(m => { m.Output["c"] = m.Input["a", 0] + m.FieldOrInput["b"]; })
			.AddControllerDevice()
			.BuildAndAdd();

		Action<Module> SumFor(int i) {
			return (m) => {
				Fix32 value = 0;
				for (int j = 0; j < i; j++) {
					value += m.Input[NAMES[j], 0];
				}
				m.Output["sum"] = value;
			};
		}
		foreach (int i in new int[] { 4, 8 }) {
			var sum = registrator
				.ModuleBuilderStart($"Sum_{i}", $"C = A + .. ({i - 1})", $"A+({i - 1})")
				.SetDescription($"Outputs the sum of {i} numeric inputs (<b>a</b> through <b>{NAMES[i - 1]}</b>) to <b>sum</b>. Unconnected inputs are treated as 0.")
				.AddCategory(Category.Arithmetic)
				.AddOutput("sum", "Sum")
				.Action(m => { m.Output["c"] = m.Input["a"] + m.Input["b", 0]; })
				.AddControllerDevice();

			for (int j = 0; j < i; j++) {
				sum.AddInput(NAMES[j], NAMES[j].ToUpper());
			}

			sum.Action(SumFor(i));
			sum.BuildAndAdd();
		}

		registrator
			.ModuleBuilderStart("Sub", "C = A - B", "A-B")
			.SetDescription("Outputs <b>a</b> - <b>b</b> to <b>c</b>. If the <b>field_b</b> toggle is on, the constant <b>b</b> field is subtracted instead of the input pin.")
			.AddCategory(Category.Arithmetic)
			.AddInput("a", "A")
			.AddInput("b", "B")
			.AddFix32Field("b", "B", overrideInput: true)
			.AddOutput("c", "C")
			.Action(m => { m.Output["c"] = m.Input["a"] - m.FieldOrInput["b", 0]; })
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Invert", "B = -A", "-A")
			.SetDescription("Outputs the arithmetic negation of <b>a</b> (i.e. <b>0 - a</b>) to <b>b</b>. Use to flip the sign of a numeric signal.")
			.AddCategory(Category.Arithmetic)
			.AddInput("a", "A")
			.AddOutput("b", "B")
			.Action(m => { m.Output["b"] = 0 - m.Input["a"]; })
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Multiply", "C = A multiply by B", "A*B")
			.SetDescription("Outputs <b>a</b> * <b>b</b> to <b>c</b>. If the <b>field_b</b> toggle is on, the constant <b>b</b> field is used as the multiplier instead of the input pin.")
			.AddCategory(Category.Arithmetic)
			.AddInput("a", "A")
			.AddInput("b", "B")
			.AddFix32Field("b", "B", overrideInput: true)
			.AddOutput("c", "C")
			.Action(m => {
				Fix32 a = m.Input["a"];
				Fix32 b = m.FieldOrInput["b"];
				Fix32 c = a * b;
				m.Output["c"] = c;
			})
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Divide", "C = A divide by B", "A/B")
			.SetDescription("Outputs <b>a</b> / <b>b</b> to <b>c</b>. If <b>b</b> is zero, sets output <b>error</b> to 1 and <b>c</b> to Fix32.MaxValue; otherwise <b>error</b> is 0. <b>field_b</b> switches <b>b</b> to the constant field.")
			.AddCategory(Category.Arithmetic)
			.AddInput("a", "A")
			.AddInput("b", "B")
			.AddFix32Field("b", "B", overrideInput: true)
			.AddOutput("c", "C")
			.AddOutput("error", "Error")
			.Action(m => {
				Fix32 a = m.Input["a"];
				Fix32 b = m.FieldOrInput["b"];
				if (b == 0) {
					m.Output["error"] = 1;
					m.Output["c"] = Fix32.MaxValue;
				} else {
					m.Output["error"] = 0;
					m.Output["c"] = a / b;
				}
			})
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Modulo", "C = A modulo B", "A%B")
			.SetDescription("Outputs <b>a</b> % <b>b</b> to <b>c</b>. If <b>b</b> is zero, sets output <b>error</b> to 1 and <b>c</b> to 0; otherwise <b>error</b> is 0. <b>field_b</b> switches <b>b</b> to the constant field.")
			.AddCategory(Category.Arithmetic)
			.AddInput("a", "A")
			.AddInput("b", "B")
			.AddFix32Field("b", "B", overrideInput: true)
			.AddOutput("c", "C")
			.AddOutput("error", "Error")
			.Action(m => {
				Fix32 a = m.Input["a", 0];
				Fix32 b = m.FieldOrInput["b", 0];
				if (b == 0) {
					m.Output["error"] = 1;
					m.Output["c"] = 0;
				} else {
					m.Output["error"] = 0;
					m.Output["c"] = a % b;
				}
			})
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Average", "Average", "~A")
			.SetDescription("Maintains a running average of <b>input</b> over up to <b>count</b> field samples (default 10). Outputs current sample count to <b>count</b> and the running mean to <b>average</b>. Errors if count < 1.")
			.AddCategory(Category.Arithmetic)
			.AddInput("input", "Input")
			.AddOutput("count", "Average")
			.AddOutput("average", "Average")
			.AddInt32Field("count", "Count", "Maximum number of values to be counted in average with default: 10", 10)
			.Action(m => {
				Fix32 desiredCount = m.Field["count", Fix32.One];
				if (desiredCount < Fix32.One) {
					return ModuleStatus.Error;
				}

				Fix32 input = m.Input["input", 0];
				Fix32 oldCount = (m.Output["count", 1] - 1);
				Fix32 average = (m.Output["average", 0] * oldCount) + input;

				m.Output["count"] = Min(desiredCount, m.Output["count", 0] + 1.ToFix32());
				m.Output["average"] = (average / m.Output["count"]);
				return ModuleStatus.Running;
			})
			.AddControllerDevice()
			.BuildAndAdd();
	}

	private Fix32 Min(Fix32 a, Fix32 b) {
		return a < b ? a : b;
	}

	private Fix32 Min(Fix32 a, int b) {
		return a < b.ToFix32() ? a : b.ToFix32();
	}

	private Fix32 Min(int a, Fix32 b) {
		return a.ToFix32() < b ? a.ToFix32() : b;
	}

	private void Stats(ProtoRegistrator registrator) {
		registrator
			.ModuleBuilderStart("Stats_Unity", "Connection: Office - Unity", "UNI")
			.SetDescription("Reads the captain's current Unity total via UpointsManager and outputs it on <b>v</b>. Errors if no CaptainOffice is linked in the <b>office</b> field.")
			.AddCategory(Category.Connection)
			.AddCategory(Category.ConnectionRead)
			.AddCategory(Category.Stats)
			.AddOutput("v", "Unity value")
			.AddEntityField<CaptainOffice>("office", "Captains office", "Must be placest next to Captains office")
			.Action(m => {
				if (m.Field.Entity<CaptainOffice>("office") is null) {
					return ModuleStatus.Error;
				}

				m.Output["v"] = Fix32.FromRaw(m.Context.UpointsManager.Quantity.Value);
				return ModuleStatus.Running;
			})
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Stats_Workers", "Connection: Office - Workers", "WRK")
			.SetDescription("Reads worker statistics from WorkersManager. Outputs used <b>u</b>, available <b>a</b>, missing <b>m</b> (deficit, positive when short), and total <b>t = u + a</b>. Errors if no CaptainOffice is linked.")
			.AddCategory(Category.Connection)
			.AddCategory(Category.ConnectionRead)
			.AddCategory(Category.Stats)
			.AddOutput("u", "Used workers")
			.AddOutput("a", "Available workers")
			.AddOutput("m", "Missing workers")
			.AddOutput("t", "Total workers")
			.AddEntityField<CaptainOffice>("office", "Captains office", "Must be placest next to Captains office")
			.Action(m => {
				if (m.Field.Entity<CaptainOffice>("office") is null) {
					return ModuleStatus.Error;
				}

				m.Output["a"] = Math.Max(0, m.Context.WorkersManager.AmountOfFreeWorkersOrMissing);
				m.Output["t"] = (int)(m.Context.WorkersManager as WorkersManager).TotalWorkersNeededStats.ThisYear + m.Output["a"];
				m.Output["m"] = 0 - Math.Min(0, m.Context.WorkersManager.AmountOfFreeWorkersOrMissing);
				m.Output["u"] = m.Output["t", 0] - m.Output["a", 0];
				return ModuleStatus.Running;
			})
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Stats_Electricity", "Statistic: Electricity", "PWR")
			.SetDescription("Reads global ElectricityManager metrics for the current tick. Outputs <b>consumption</b> (DemandedThisTick), <b>production</b> (GeneratedThisTick), <b>capacity</b> (GenerationCapacityThisTick) in kW, and <b>usage</b> as percentage 0-100 of consumption / capacity.")
			.AddCategory(Category.Stats)
			.AddOutput("consumption", "Consumption")
			.AddOutput("production", "Production")
			.AddOutput("capacity", "Power capacity")
			.AddOutput("usage", "Power usage (0-100)")
			.Width(4)
			.Action(m => {
				ElectricityManager electricity = m.Controller.ElectricityConsumer.Value.GetType()
					.GetField("m_electricityManager", BindingFlags.Instance | BindingFlags.NonPublic)
					.GetValue(m.Controller.ElectricityConsumer.Value) as ElectricityManager;

				Electricity consumption = electricity.DemandedThisTick;
				Electricity production = electricity.GeneratedThisTick;
				Electricity capacity = electricity.GenerationCapacityThisTick;

				m.Output["consumption"] = consumption.Value.ToFix32();
				m.Output["production"] = production.Value.ToFix32();
				m.Output["capacity"] = capacity.Value.ToFix32();
				m.Output["usage"] = 100.ToFix32() * (consumption.Value.ToFix32() / capacity.Value.ToFix32());
			})
			.AddDisplay("consumption", "Consumption", 1.2f.ToFix32())
			.AddDisplay("production", "Production", 1.8f.ToFix32())
			.AddDisplay("power", "Power", 1, image: true)
			.Display(m => {
				var stage = new[] { "kW", "MW", "GW", "TW" };
				var cons = m.Output["consumption"];
				var prod = m.Output["production"];
				var cap = m.Output["capacity"];
				var consUnit = 0;
				var state = "";
				if (prod == 0 || cons > prod) {
					state = "#E";
				} else if (cons < (prod * 0.75f.ToFix32())) {
					state = "#P";
				} else if (cons > (cap * 0.75f.ToFix32())) {
					state = "#W";
				} else {
					state = "";
				}

				while (cons > 100.ToFix32() || prod > 100.ToFix32()) {
					cons /= 1000;
					prod /= 1000;
					consUnit++;
				}

				int indexCons = cons > 19.ToFix32() ? 0 : 1;
				int indexProd = prod > 19.ToFix32() ? 0 : 1;

				m.Display["consumption"] = $"{state}{cons.ToStringRounded(indexCons)}";
				m.Display["production"] = $"{state}{prod.ToStringRounded(indexProd)} {stage[consUnit]}";
				m.Display["power"] = UserInterface.General.Electricity_svg;
			})
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Stats_Maintenance", "Connection: Maintenance", "MAINT")
			.SetDescription("Reads maintenance buffer of the linked MaintenanceDepot for the selected tier (<b>m</b> field). Outputs current <b>a</b> amount, <b>c</b> capacity, <b>p</b> percentage 0-100, and monthly <b>u</b> use (positive surplus / negative deficit).")
			.AddCategory(Category.Connection)
			.AddCategory(Category.ConnectionRead)
			.AddCategory(Category.Stats)
			.AddOutput("a", "Amount")
			.AddOutput("c", "Capacity")
			.AddOutput("p", "Percentage (0-100)")
			.AddOutput("u", "Use (monthly +surplus / -deficit)")
			.Width(4)
			.AddEntityField<MaintenanceDepot>("depot", "Maintenance depot")
			.AddProductField("m", "Maintenance tier", filter: (m, p) => p.Id.Value.StartsWith("Product_Virtual_MaintenanceT"))
			.Action(m => {
				MaintenanceDepot maintenanceDepot = m.Field.Entity<MaintenanceDepot>("depot");
				if (maintenanceDepot == null) {
					m.SetError("Invalid maintenance product");
					return ModuleStatus.Error;
				}

				ProductProto product = m.Field.Product("m");
				if (product == null) {
					product = m.Context.ProtosDb.Get<ProductProto>(Ids.Products.MaintenanceT1).Value;
					m.Field["m"] = Fix32.FromRaw(product.SlimId.Value);
				}

				ProductStats productStats = m.Context.ProductsManager.GetStatsFor(product);
				IMaintenanceBufferReadonly buffer = ((MaintenanceManager)maintenanceDepot.GetType()
					.GetField("m_maintenanceManager", BindingFlags.Instance | BindingFlags.NonPublic)
					.GetValue(maintenanceDepot))
					.MaintenanceBuffers.First(b => b.Product == product);

				//m.Output["a"] = productStats.GlobalQuantity.ToQuantity().Value.Value.ToFix32();
				m.Output["a"] = buffer.Quantity.Value.ToFix32();
				m.Output["c"] = buffer.Capacity.Value.ToFix32();
				m.Output["p"] = 100.ToFix32() * (buffer.Quantity.Value.ToFix32() / buffer.Capacity.Value.ToFix32());
				m.Output["u"] = (((productStats.CreatedByProduction.LastMonth - productStats.UsedTotalStats.LastMonth)
									.ToQuantity().Value.Value / 10) * 10).ToFix32();
				return ModuleStatus.Running;
			})
			.AddDisplay("product", "Product", 1, image: true)
			.AddDisplay("direction", "Surplus/Deficit", 1, image: true)
			.AddDisplay("value", "Value", 2)
			.Display(m => {
				bool surplus = m.Output["u"] > Fix32.Zero;
				bool deficit = m.Output["u"] < Fix32.Zero;

				string color = surplus ? "#C00FF00" : deficit ? "#CFF0000" : "";
				string direction = surplus ? UserInterface.General.MoveUp_svg : deficit ? UserInterface.General.MoveDown_svg : UserInterface.General.Minus128_png;
				Fix32 value = m.Output["p"];
				string state = value < 25 ? "#E" : value < 50 ? "#W" : value < 75 ? "" : "#E";

				m.Display["product"] = m.Field.Product("m")?.IconPath;
				m.Display["direction"] = $"{color}{direction}";
				m.Display["value"] = $"{state}{value.ToStringRounded(0)} %";
			})
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Stats_Vehicle", "Connection: Office - Vehicles", "VEH")
			.SetDescription("Reads vehicle stats via IVehiclesManager. With a vehicle type chosen via input or <b>vehicle</b> field: outputs <b>count</b> (owned) and <b>assignable</b>. Without selection: outputs total fleet count to <b>count</b> and 0 to <b>assignable</b>. Errors if no CaptainOffice is linked.")
			.AddCategory(Category.Connection)
			.AddCategory(Category.ConnectionRead)
			.AddCategory(Category.Stats)
			.AddEntityField<CaptainOffice>("office", "Captains office", "Must be placest next to Captains office")
			.AddInput("vehicle", "Amount of owned vehicle/ship (all when unselected)")
			.AddEntityTypeField<DynamicEntityProto>("vehicle", "Vehicle", "Vehicle count to read", overrideInput: true).AddInput("vehicle", "Vehicle type")
			.AddOutput("count", "Amount of owned vehicle/ship (all when unselected)")
			.AddOutput("assignable", "Amount of assignable vehicle/ship (all when unselected)")
			.Width(2)
			.Action(m => {
				if (m.Field.Entity<CaptainOffice>("office") is null) {
					return ModuleStatus.Error;
				}

				if (m.FieldOrInput.EntityProtoIconified("vehicle") is DynamicEntityProto drivingEntity) {
					var stats = GlobalDependencyResolver.Get<IVehiclesManager>().GetStats(drivingEntity, 0xFFFFFFFFFFFFFFFF);
					m.Output.Integer["count"] = stats.Owned;
					m.Output.Integer["assignable"] = stats.Assignable;
					return ModuleStatus.Running;
				}

				m.Output.Integer["count"] = GlobalDependencyResolver.Get<IVehiclesManager>().AllVehicles.Count;
				m.Output.Integer["assignable"] = 0;
				return ModuleStatus.Running;
			})
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Connection_Speaker", "Connection: Speaker - play", "SPK")
			.SetDescription("Drives a Speaker entity's playback state from the <b>play</b> input or <b>play</b> field (boolean, controlled by <b>field_play</b> toggle). Errors if no Speaker is linked in the <b>speaker</b> field.")
			.AddCategory(Category.Devices)
			.AddCategory(Category.DevicesSound)
			.AddEntityField<Speaker>("speaker", "Speaker", "Must be placest next to Speaker tower")
			.AddInput("play", "Activate sound")
			.AddBooleanField("play", "Activate sound", overrideInput: true)
			.Width(1)
			.Action(m => {
				if (m.Field.Entity<Speaker>("speaker") is Speaker speaker) {
					speaker.SetPlaying(m.FieldOrInput.Bool["play"]);
					return ModuleStatus.Running;
				}

				m.SetError("No connected speaker");
				return ModuleStatus.Error;
			})
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Connection_Display", "Connection: Display - active", "DIA")
			.SetDescription("Connects lights and display for activation")
			.AddCategory(Category.Devices)
			.AddCategory(Category.DevicesDisplay)
			.AddEntityField<DisplayEntity>("display", "Display", "Must be placest next to display")
			.AddInput("active", "Activate display")
			.AddBooleanField("active", "Activate display", overrideInput: true)
			.Width(1)
			.Action(m => {
				if (m.Field.Entity<DisplayEntity>("display") is DisplayEntity display) {
					display.SetActive(m.FieldOrInput.Bool["active"]);
					return ModuleStatus.Running;
				}

				m.SetError("No connected display");
				return ModuleStatus.Error;
			})
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Connection_Display_Color", "Connection: Display - color", "CLR")
			.SetDescription("Drives the LED color of the linked display from the <b>color</b> field (when <b>field_color</b> toggle is on) or <b>color</b> input. Sets both lit and dim color channels on the display entity.")
			.AddCategory(Category.Devices)
			.AddCategory(Category.DevicesDisplay)
			.AddEntityField<DisplayEntity>("display", "Display", "Must be placest next to display")
			.AddInput("color", "Activate display")
			.AddColorField("color", "Color", "", ColorRgba.Red, overrideInput: true)
			.AddDisplay("color", "Color", 1, led: true)
			.Width(1)
			.Action(m => {
				if (m.Field.Entity<DisplayEntity>("display") is { } display) {
					ColorRgba v = m.FieldOrInput["color"].AsColorRgba;
					display.SetProperty("colorOn.R", v.R);
					display.SetProperty("colorOn.G", v.G);
					display.SetProperty("colorOn.B", v.B);
					display.SetProperty("colorOff.R", v.R.Min(100));
					display.SetProperty("colorOff.G", v.G.Min(100));
					display.SetProperty("colorOff.B", v.B.Min(100));
					return ModuleStatus.Running;
				}
				m.SetError("No connected display");
				return ModuleStatus.Error;
			})
			.Display(m => {
				m.Display["color"] = $"#C{m.FieldOrInput["color"].AsColorRgba.AsHexString}";
			})
			.AddControllerDevice()
			.BuildAndAdd();

		var seven8 = registrator
			.ModuleBuilderStart("Connection_Display_7SEG_8", "Connection: Display - 7 segment (8-inputs)", "7-SEGMENT")
			.SetDescription("Light up 7-segment display and activate lines per signal")
			.AddCategory(Category.Devices)
			.AddCategory(Category.DevicesDisplay)
			.AddEntityField<DisplayEntity>("display", "Display", "Must be placest next in 40 metres",
				filter: (module, entity) => entity.Prototype.Id == NewIds.Controllers.Display7 || entity.Prototype.Id == NewIds.Controllers.Display16)
			.Width(8)
			.Action(m => {
				if (m.Field.Entity<DisplayEntity>("display") is not DisplayEntity display) {
					m.SetError("No connected display");
					return ModuleStatus.Error;
				}

				bool active = false;
				if (display.DisplayManager is SevenSegmentManager sevenSegmentManager) {
					foreach (var item in SevenSegmentManager.SEGMENTS) {
						Fix32 thisActive = m.FieldOrInput.Bool[item] ? 1 : 0;
						display.SetProperty(item, thisActive);
						active = active || thisActive > 0;
					}
				} else if (display.DisplayManager is SixteenSegmentManager sixteenSegmentManager) {
					string[] halves = ["A", "D", "G"];
					foreach (var item in SevenSegmentManager.SEGMENTS) {
						Fix32 thisActive = m.FieldOrInput.Bool[item] ? 1 : 0;
						if (halves.Contains(item)) {
							display.SetProperty(item + "1", thisActive);
							display.SetProperty(item + "2", thisActive);
						} else {
							display.SetProperty(item, thisActive);
						}
						active = active || thisActive > 0;
					}
				}

				display.SetActive(active);
				return ModuleStatus.Running;
			})
			.AddDisplayFiller(6)
			.AddDisplay("bits", "Bits", 2.ToFix32())
			.Display(m => {
				int bits = 0;
				for (int i = SevenSegmentManager.SEGMENTS.Length - 1; i >= 0; i--) {
					string item = SevenSegmentManager.SEGMENTS[i];
					bits = (bits << 1) | (m.FieldOrInput.Bool[item] ? 1 : 0);
				}
				m.Display["bits"] = $"{bits:D3}";
			})
			.AddControllerDevice();

		foreach (var item in SevenSegmentManager.SEGMENTS) {
			seven8
				.AddInput(item, item == "DP" ? "Dot" : item)
				.AddBooleanField(item, item == "DP" ? "Dot" : $"Signal: {item}", overrideInput: true);
		}

		seven8.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Connection_Display_7SEG_B", "Connection: Display - 7 segment (2-inputs)", "7-SEG")
			.SetDescription("Light up 7-segment display and activate lines by bits inside single number")
			.AddCategory(Category.Devices)
			.AddCategory(Category.DevicesDisplay)
			.AddEntityField<DisplayEntity>("display", "Display", "Must be placest next in 40 metres",
				filter: (module, entity) => entity.Prototype.Id == NewIds.Controllers.Display7 || entity.Prototype.Id == NewIds.Controllers.Display16)
			.AddInput("N", "Bits")
			.AddInt32Field("N", "Encoded number (Bits, 0-127)", overrideInput: true)
			.AddInput("DP", "Dot")
			.AddBooleanField("DP", "Dot", overrideInput: true)
			.Width(2)
			.Action(m => {
				if (m.Field.Entity<DisplayEntity>("display") is not DisplayEntity display) {
					m.SetError("No connected display");
					return ModuleStatus.Error;
				}

				bool active = false;
				int n = m.FieldOrInput.Integer["N"];
				if (display.DisplayManager is SevenSegmentManager sevenSegmentManager) {
					for (int i = 0; i < 7; i++) {
						bool thisActive = ((n >> i) & 0x1) == 0x1;
						display.SetProperty(SevenSegmentManager.SEGMENTS[i], thisActive ? 1 : 0);
						active = active || thisActive;
					}
				} else if (display.DisplayManager is SixteenSegmentManager sixteenSegmentManager) {
					int[] halves = [
						Array.IndexOf(SevenSegmentManager.SEGMENTS, "A"),
						Array.IndexOf(SevenSegmentManager.SEGMENTS, "D"),
						Array.IndexOf(SevenSegmentManager.SEGMENTS, "G")
					];
					for (int i = 0; i < 7; i++) {
						bool thisActive = ((n >> i) & 0x1) == 0x1;
						if (halves.Contains(i)) {
							display.SetProperty(SevenSegmentManager.SEGMENTS[i] + "1", thisActive ? 1 : 0);
							display.SetProperty(SevenSegmentManager.SEGMENTS[i] + "2", thisActive ? 1 : 0);
						} else {
							display.SetProperty(SevenSegmentManager.SEGMENTS[i], thisActive ? 1 : 0);
						}
						active = active || thisActive;
					}
				}

				bool dp = m.FieldOrInput.Bool["DP"];
				display.SetProperty("DP", dp ? 1 : 0);
				active = active || dp;

				display.SetActive(active);
				return ModuleStatus.Running;
			})
			.AddDisplay("bits", "Bits", 2.ToFix32())
			.Display(m => {
				m.Display["bits"] = $"{m.FieldOrInput.Integer["N"]:D3}";
			})
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Connection_Display_16SEG_B", "Connection: Display - 16 segment (2-inputs)", "16-SEG")
			.SetDescription("Light up 16-segment display and activate lines by bits inside single number")
			.AddCategory(Category.Devices)
			.AddCategory(Category.DevicesDisplay)
			.AddEntityField<DisplayEntity>("display", "Display", "Must be placest next in 40 metres",
				filter: (module, entity) => entity.Prototype.Id == NewIds.Controllers.Display7 || entity.Prototype.Id == NewIds.Controllers.Display16)
			.AddInput("N", "Bits")
			.AddInt32Field("N", "Encoded number (Bits, 0-65 535)", overrideInput: true)
			.AddInput("DP", "Dot")
			.AddBooleanField("DP", "Dot", overrideInput: true)
			.Width(2)
			.Action(m => {
				if (m.Field.Entity<DisplayEntity>("display") is not DisplayEntity display) {
					m.SetError("No connected display");
					return ModuleStatus.Error;
				}

				bool active = false;
				int n = m.FieldOrInput.Integer["N"];
				if (display.DisplayManager is SixteenSegmentManager sixteenSegmentManager) {
					for (int i = 0; i < 16; i++) {
						bool thisActive = ((n >> i) & 0x1) == 0x1;
						display.SetProperty(SixteenSegmentManager.SEGMENTS[i], thisActive ? 1 : 0);
						active = active || thisActive;
					}
				}
				// TODO 7-segment back

				bool dp = m.FieldOrInput.Bool["DP"];
				display.SetProperty("DP", dp ? 1 : 0);
				active = active || dp;

				display.SetActive(active);
				return ModuleStatus.Running;
			})
			.AddDisplay("bits", "Bits", 2.ToFix32())
			.Display(m => {
				m.Display["bits"] = $"{m.FieldOrInput.Integer["N"]:D3}";
			})
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Arithmetic_Display_7SEG_B", "Arithmetic: 7 segment", "7S-NB")
			.SetDescription("Light up 7-segment display and activate lines by bits inside single number")
			.AddCategory(Category.Arithmetic)
			.AddInput("V", "Number")
			.AddOutput("bits", "Bits")
			.AddOutput("rest", "Rest")
			.Width(2)
			.Action(m => {
				Fix32 v = m.Input["V"];
				if (v.IsNegative) {
					int val = 9 - (v.IntegerPart % 10);
					int rest = v.IntegerPart / 10;
					m.Output["bits"] = new int[]{
						0b00111111, // segment 0
						0b00000110, // segment 1
						0b01011011, // segment 2
						0b01001111, // segment 3
						0b01100110, // segment 4
						0b01101101, // segment 5
						0b01111101, // segment 6
						0b00000111, // segment 7
						0b01111111, // segment 8
						0b01101111  // segment 9
					}[val];
					m.Output["rest"] = rest.ToFix32();
				} else {
					int val = v.IntegerPart % 10;
					int rest = v.IntegerPart / 10;
					m.Output["bits"] = new int[]{
						0b00111111, // segment 0
						0b00000110, // segment 1
						0b01011011, // segment 2
						0b01001111, // segment 3
						0b01100110, // segment 4
						0b01101101, // segment 5
						0b01111101, // segment 6
						0b00000111, // segment 7
						0b01111111, // segment 8
						0b01101111  // segment 9
					}[val];
					m.Output["rest"] = rest.ToFix32();
				}
				return ModuleStatus.Running;
			})
			.AddDisplay("val", "Value", 0.75.ToFix32())
			.AddDisplay("bits", "Bits", 1.25.ToFix32())
			.Display(m => {
				m.Display["val"] = $"{m.Input["V"].IntegerPart.Abs() % 10}";
				m.Display["bits"] = $"{m.Output.Integer["bits"]:D3}";
			})
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Arithmetic_Display_16SEG_B", "Arithmetic: 16 segment", "16S-NB")
			.SetDescription("Light up 16-segment display and activate lines by bits inside single number")
			.AddCategory(Category.Arithmetic)
			.AddInput("V", "Number")
			.AddOutput("bits", "Bits")
			.AddOutput("rest", "Rest")
			.Width(2)
			.Action(m => {
				Fix32 v = m.Input["V"];
				if (v.IsNegative) {
					int val = 9 - (v.IntegerPart % 10);
					int rest = v.IntegerPart / 10;
					m.Output["bits"] = new int[]{
						0b001_100_011_0111111, // segment 0
						0b000_000_000_0000110, // segment 1
						0b000_000_111_1011011, // segment 2
						0b000_000_111_0001111, // segment 3
						0b000_000_100_1100110, // segment 4
						0b000_000_111_1101101, // segment 5
						0b000_000_111_1111101, // segment 6
						0b001_100_001_0000001, // segment 7
						0b000_000_111_1111111, // segment 8
						0b000_000_111_1101111  // segment 9
					}[val];
					m.Output["rest"] = rest.ToFix32();
				} else {
					int val = v.IntegerPart % 10;
					int rest = v.IntegerPart / 10;
					m.Output["bits"] = new int[]{
						0b001_100_011_0111111, // segment 0
						0b000_000_000_0000110, // segment 1
						0b000_000_111_1011011, // segment 2
						0b000_000_111_0001111, // segment 3
						0b000_000_100_1100110, // segment 4
						0b000_000_111_1101101, // segment 5
						0b000_000_111_1111101, // segment 6
						0b001_100_001_0000001, // segment 7
						0b000_000_111_1111111, // segment 8
						0b000_000_111_1101111  // segment 9
					}[val];
					m.Output["rest"] = rest.ToFix32();
				}
				return ModuleStatus.Running;
			})
			.AddDisplay("val", "Value", 0.75.ToFix32())
			.AddDisplay("bits", "Bits", 1.25.ToFix32())
			.Display(m => {
				m.Display["val"] = $"{m.Input["V"].IntegerPart.Abs() % 10}";
				m.Display["bits"] = $"{m.Output.Integer["bits"]:D3}";
			})
			.AddControllerDevice()
			.BuildAndAdd();
	}

	private void Forks(ProtoRegistrator registrator) {
		Action<Module>[] actions = new Action<Module>[] {
			m => {
				m.Output["a"] = m.Input["a"];
				m.Output["b"] = m.Input["a"];
			},
			m => {
				m.Output["a"] = m.Input["a"];
				m.Output["b"] = m.Input["a"];
				m.Output["c"] = m.Input["a"];
				m.Output["d"] = m.Input["a"];
			},
		};
		foreach (int i in new int[] { 2, 4 }) {
			var builder = registrator
				.ModuleBuilderStart($"Fork_{i}", $"Fork: 1 pin to {i}", $"F-{i}")
				.SetDescription("Is used for organizing of pin connection. Is not required to use, output may be connected to multiple inputs")
				.AddInput("a", "A")
				.AddControllerDevice()
				// dynamic
				.Action(actions[(i / 2) - 1]);

			for (int j = 0; j < i; j++) {
				builder.AddOutput(NAMES[j], NAMES[j].ToUpper());
			}

			builder.BuildAndAdd();
		}
	}

	private void Booleans(ProtoRegistrator registrator) {
		Action<Module>[] ands = new Action<Module>[] {
			(m) =>
			{
				m.Output.Bool["a"] =
					m.Input["a", 0] > 0 &&
					m.Input["b", 0] > 0;
				m.Output["b"] = m.Output["a"] > 0 ? 0 : 1;
			},
			(m) =>
			{
				m.Output.Bool["a"] =
					m.Input["a", 0] > 0 &&
					m.Input["b", 0] > 0 &&
					m.Input["c", 0] > 0 &&
					m.Input["d", 0] > 0;
				m.Output.Bool["b"] = !m.Output.Bool["a"];
			}
		};
		foreach (int i in new int[] { 2, 4 }) {
			var builder = registrator
				.ModuleBuilderStart($"Boolean_And_{i}", $"Boolean: AND ({i} pins)", $"AND-{i}")
				.SetDescription($"Outputs <b>a</b> = 1 on <b>c</b> if all {i} inputs (<b>a</b>..<b>{NAMES[i - 1]}</b>) are > 0, else 0; <b>b</b> (<b>not_c</b>) is the inverse.")
				.AddCategory(Category.Boolean)
				.AddOutput("b", "not C")
				.AddOutput("a", "C")
				.Display(m => {
					m.Display["c"] = m.Output.Bool["a"] ? "1" : "";
					m.Display["not_c"] = !m.Output.Bool["a"] ? "1" : "";
				})
				.AddDisplayFiller(i - 2)
				.AddDisplay("not_c", "not C", 1, led: true)
				.AddDisplay("c", "C", 1, led: true)
				.AddControllerDevice()
				// dynamic
				.Action(ands[(i / 2) - 1]);

			for (int j = 0; j < i; j++) {
				builder.AddInput(NAMES[j], NAMES[j].ToUpper());
			}

			builder.BuildAndAdd();
		}
		Action<Module>[] ors = new Action<Module>[] {
			(m) =>
			{
				m.Output["a"] = (
					m.Input["a"] > 0 ||
					m.Input["b"] > 0
				) ? 1 : 0;
				m.Output["b"] = m.Output["a"] > 0 ? 0 : 1;
			},
			(m) =>
			{
				m.Output["a"] = (
					m.Input["a", 0] > 0 ||
					m.Input["b", 0] > 0 ||
					m.Input["c", 0] > 0 ||
					m.Input["d", 0] > 0
				) ? 1 : 0;
				m.Output["b"] = m.Output["a"] > 0 ? 0 : 1;
			}
		};
		foreach (int i in new int[] { 2, 4 }) {
			var builder = registrator
				.ModuleBuilderStart($"Boolean_Or_{i}", $"Boolean: OR ({i} pins)", $"OR-{i}")
				.SetDescription($"Outputs <b>a</b> = 1 on <b>c</b> if any of the {i} inputs (<b>a</b>..<b>{NAMES[i - 1]}</b>) is > 0, else 0; <b>b</b> (<b>not_c</b>) is the inverse.")
				.AddCategory(Category.Boolean)
				.AddOutput("b", "not C")
				.AddOutput("a", "C")
				.Display(m => {
					m.Display["c"] = m.Output.Bool["a"] ? "1" : "";
					m.Display["not_c"] = !m.Output.Bool["a"] ? "1" : "";
				})
				.AddDisplayFiller(i - 2)
				.AddDisplay("not_c", "not C", 1, led: true)
				.AddDisplay("c", "C", 1, led: true)
				.AddControllerDevice()
				// dynamic
				.Action(ors[(i / 2) - 1]);

			for (int j = 0; j < i; j++) {
				builder.AddInput(NAMES[j], NAMES[j].ToUpper());
			}

			builder.BuildAndAdd();
		}
		registrator
			.ModuleBuilderStart($"Boolean_Xor", $"Boolean: XOR", $"XOR")
			.SetDescription("Outputs <b>a</b> = 1 on <b>c</b> if exactly one of <b>a</b>, <b>b</b> is > 0 (logical XOR); otherwise 0. <b>b</b> (<b>not_c</b>) is the inverse.")
			.AddCategory(Category.Boolean)
			.AddInput("a", "A")
			.AddInput("b", "B")
			.AddOutput("b", "not C")
			.AddOutput("a", "C")
			.AddControllerDevice()
			// dynamic
			.Action(m => {
				m.Output["a"] = (
					m.Input["a"] > 0 !=
					m.Input["b"] > 0
				) ? 1 : 0;
				m.Output["b"] = m.Output["a"] > 0 ? 0 : 1;
			})
			.Display(m => {
				m.Display["c"] = m.Output.Bool["a"] ? "1" : "";
				m.Display["not_c"] = !m.Output.Bool["a"] ? "1" : "";
			})
			.AddDisplay("not_c", "not C", 1, led: true)
			.AddDisplay("c", "C", 1, led: true)
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart($"Boolean_Not", $"Boolean: NOT", $"nA")
			.SetDescription("Outputs <b>a</b> = 1 if input <b>a</b> is 0 or negative, else 0. Logical inversion of a single input.")
			.AddCategory(Category.Boolean)
			.AddInput("a", "A")
			.AddOutput("a", "not A")
			.Display(m => m.Display["a"] = !m.Input.Bool["a"] ? "1" : "")
			.AddDisplay("a", "not A", 1, led: true)
			.AddControllerDevice()
			// dynamic
			.Action(m => m.Output["a"] = m.Input["a"] > 0 ? 0 : 1)
			.BuildAndAdd();
	}

	private void Decisions(ProtoRegistrator registrator) {
		Action<Module> Select(int count) {
			return m => {
				Fix32 index = m.Input["index", Fix32.MaxValue];
				int digits = count * 2;
				string text = index.IntegerPart.ToString($"D{digits}");
				m.Display["index"] = text.Length > digits ? text.Substring(text.Length - digits) : text;

				for (int i = 0; i < count; i++) {
					string name = NAMES[i];
					Fix32 value = m.Field[name];
					if (index <= value) {
						m.Output["selected"] = m.Input[name, 0];
						return;
					}
				}
				m.Output["selected"] = m.Input["else", 0];
			};
		}
		foreach (int i in new int[] { 4, 8 }) {
			var builder = registrator
				.ModuleBuilderStart($"Decision_Select_{i}", $"Select ({i - 1} pins, integer)", $"SEL-{i - 1}")
				.SetDescription($"Routes the first input whose threshold field (<b>a</b>..<b>{NAMES[i - 3]}</b>) is >= <b>index</b> to <b>selected</b>; falls back to <b>else</b> if none match.")
				.AddCategory(Category.Decision)
				.AddCategory(Category.Control)
				.AddInput("index", "Index")
				.AddOutput("selected", "Selected")
				.AddDisplay("index", "Index", i)
				.AddControllerDevice()
				// dynamic
				.Action(Select(i - 2));

			for (int j = 0; j < i - 2; j++) {
				builder.AddInput(NAMES[j], NAMES[j].ToUpper());
			}

			for (int j = 0; j < i - 2; j++) {
				builder.AddInt32Field(NAMES[j], NAMES[j].ToUpper() + ": Index ≤", defaultValue: j);
			}

			builder.AddInput("else", "Else");
			builder.BuildAndAdd();
		}
	}

	// Configurable multi-tick delay was moved to Python — see Runtime_Delay_1
	// in src/ProgramableNetwork.Modules/Custom/delay.py.  It now serves as the
	// reference example for the Module.Array API; the C#-side helpers it relies
	// on (ArrayAccess.Resize(size, fillNew) / ArrayAccess.ShiftLeftWith) are
	// what make the loop-free Python action() possible.

	private void Connections(ProtoRegistrator registrator) {
		registrator
			.ModuleBuilderStart("Connection_Controller_Input", "Connection: Controller (4 pin, input)", "C-IN")
			.SetDescription("Reads 4 pins from a paired <b>Connection_Controller_Output</b> module on a remote Controller, matched by the <b>name</b> field. Outputs the remote inputs <b>a</b>, <b>b</b>, <b>c</b>, <b>d</b>; outputs 0 when no matching module is found.")
			.AddCategory(Category.Connection)
			.AddCategory(Category.ConnectionRead)
			.AddOutput("a", "A")
			.AddOutput("b", "B")
			.AddOutput("c", "C")
			.AddOutput("d", "D")
			.AddEntityField<Controller>("controller", "Connection device", "Name of output module, which must exist in target Controller")
			.AddStringField("name", "Output Name", defaultValue: "C")
			.Display(m => m.Display["name"] = m.Field["name", "C"])
			.AddDisplay("name", "Connection name", 4, defaultText: "C")
			.Action(m => {
				//Mafi.Log.Info("Update of input");
				Controller controller = m.Field.Entity<Controller>("controller");
				string moduleType = "Connection_Controller_Output".ModuleId();
				string noduleName = m.Field["name", "C"];
				if (noduleName.Length > 0 && controller != null) {
					//Mafi.Log.Info("Target entity found");
					Module targetModule = controller.Modules.AsEnumerable()
						.FirstOrDefault(mod => mod.Prototype.Id.Value == moduleType
											&& mod.Field["name", ""] == noduleName);
					if (targetModule != null) {
						m.Output["a"] = targetModule.Input["a", 0];
						m.Output["b"] = targetModule.Input["b", 0];
						m.Output["c"] = targetModule.Input["c", 0];
						m.Output["d"] = targetModule.Input["d", 0];
						return;
					}
				}
				m.Output["a"] = 0;
				m.Output["b"] = 0;
				m.Output["c"] = 0;
				m.Output["d"] = 0;
			})
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Connection_Controller_Output", "Connection: Controller (4 pin, output)", "C-OUT")
			.SetDescription("Exposes 4 inputs (<b>a</b>, <b>b</b>, <b>c</b>, <b>d</b>) under the name set in the <b>name</b> field so a remote <b>Connection_Controller_Input</b> module can read them. Acts as a passive endpoint - it does not drive any entity.")
			.AddCategory(Category.Connection)
			.AddCategory(Category.ConnectionWrite)
			.AddInput("a", "A")
			.AddInput("b", "B")
			.AddInput("c", "C")
			.AddInput("d", "D")
			.AddDisplay("name", "Connection name", 4, defaultText: "C")
			.Display(m => m.Display["name"] = m.Field["name", "C"])
			.AddStringField("name", "Name", "Name of output module, which must be selected in target Controller", defaultValue: "C")
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Connection_SwitchOff", "Connection: Switch Off", "S")
			.SetDescription("Pauses or unpauses the linked <b>entity</b> based on <b>pause</b> input (>0 pauses, otherwise resumes). Supports any pausable building and CargoDepots (forwards to the moored CargoShip). Errors if the <b>entity</b> is unset or unsupported.")
			.AddCategory(Category.Connection)
			.AddCategory(Category.ConnectionWrite)
			.AddInput("pause", "Pause")
			.AddEntityField<StaticEntity>("entity", "Connection device", "Any pausable building connectable by cable 40m from controller", filter: (m, e) => e.CanBePaused || e is CargoDepot)
			.Action(m => {
				StaticEntity entity = m.Field.Entity<StaticEntity>("entity");
				Fix32 input = m.Input["pause", 0];
				if (entity?.CanBePaused ?? false) {
					entity.SetPaused(input > 0);
					return ModuleStatus.Running;
				}
				if (entity is CargoDepot cargo) {
					m.Warning = !cargo.CargoShip.HasValue;
					if (cargo.CargoShip.HasValue) {
						cargo.CargoShip.Value.SetPaused(input > 0);
					}
					return ModuleStatus.Running;
				}
				return ModuleStatus.Error;
			})
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Connection_Storage", "Connection: Storage", "STOCK")
			.SetDescription("Reads the linked storage <b>entity</b> (storages, in/out buffers, virtual miners, FlyWheels, ThermalStorage). Outputs <b>quantity</b>, <b>capacity</b>, <b>fullness</b> (%), and stored <b>product</b> slim-id. With <b>field_product</b> set, filters buffers by the chosen <b>product</b>.")
			.AddCategory(Category.Connection)
			.AddCategory(Category.ConnectionRead)
			.AddInput("product", "Product")
			.AddOutput("quantity", "Quantity")
			.AddOutput("capacity", "Capacity")
			.AddOutput("fullness", "Fullness in %")
			.AddOutput("product", "Product in #")
			.AddEntityField<LayoutEntity>("entity", "Connected storage", "Storage connectable by cable 40m from"
					+ " controller\nCan read everything with buffer information including Thermal Storage and"
					+ " shaft of generators in single row.",
				filter: (m, e) => e
					is IEntityWithStoredProductForUi
					or IEntityWithInputBuffersForUi
					or IEntityWithOutputBuffersForUi
					or IVirtualResourceMiningEntity
					or FlyWheelEntity
					or ThermalStorage
				)
			.AddProductField("product", "Product", "Select filter for product", overrideInput: true)
			.Width(4)
			.Action(m => {
				LayoutEntity entity = m.Field.Entity<LayoutEntity>("entity");

				if (entity is IEntityWithStoredProductForUi storage)
				// entity is SettlementWasteModule
				{
					m.Output["quantity"] = storage.CurrentQuantity.Value;
					m.Output["capacity"] = storage.Capacity.Value;
					m.Output["fullness"] = (100f * storage.CurrentQuantity.Value / storage.Capacity.Value).ToFix32();
					if (storage.StoredProduct.HasValue) {
						m.Output["product"] = Fix32.FromRaw((int)(uint)storage.StoredProduct.Value.SlimId.Value);
					} else {
						m.Output["product"] = Fix32.Zero;
					}
					return ModuleStatus.Running;
				}

				if (entity is IEntityWithInputBuffersForUi and IEntityWithOutputBuffersForUi) {
					return GetValueFromBuffers(m, m.FieldOrInput.Product("product"),
						(entity as IEntityWithInputBuffersForUi).InputBuffers.AsEnumerable()
						.Concat((entity as IEntityWithOutputBuffersForUi).OutputBuffers.AsEnumerable()
						));
				}

				if (entity is IEntityWithInputBuffersForUi inputEntity) {
					return GetValueFromBuffers(m, m.FieldOrInput.Product("product"),
						inputEntity.InputBuffers.AsEnumerable());
				}

				if (entity is IEntityWithOutputBuffersForUi outputEntity) {
					return GetValueFromBuffers(m, m.FieldOrInput.Product("product"),
						outputEntity.OutputBuffers.AsEnumerable());
				}

				if (entity is IVirtualResourceMiningEntity miner) {
					m.Output["quantity"] = miner.QuantityLeftToMine.Value;
					m.Output["capacity"] = miner.CapacityOfMine.Value;
					m.Output["fullness"] = (int)(100f * miner.QuantityLeftToMine.Value / miner.CapacityOfMine.Value);
					m.Output["product"] = Fix32.FromRaw((int)(uint)miner.ProductToMine.SlimId.Value);
					return ModuleStatus.Running;
				}

				if (entity is FlyWheelEntity flyWheel) {
					IShaftManager shafts = (IShaftManager)flyWheel.GetType()
						.GetField("m_shaftManager", BindingFlags.Instance | BindingFlags.NonPublic)!
						.GetValue(flyWheel);
					IShaft shaft = shafts.GetCurrentShaftFor(flyWheel);
					IProductBufferReadOnly buffer = shaft.InertiaBuffer;
					m.Output["quantity"] = ((buffer?.Quantity.Value ?? 0) / 100000f).ToFix32();
					m.Output["capacity"] = ((buffer?.Capacity.Value ?? 0) / 100000f).ToFix32();
					m.Output["fullness"] = (100f * (float)buffer.Quantity.Value / buffer.Capacity.Value).ToFix32();
					m.Output["product"] = Fix32.FromRaw(buffer.Product?.SlimId.Value ?? 0);
					return ModuleStatus.Running;
				}

				if (entity is ThermalStorage thermal) {
					if (thermal.AssignedProduct.HasValue == false) {
						m.Output["quantity"] = 0;
						m.Output["capacity"] = 0;
						m.Output["fullness"] = 100;
						m.Output["product"] = -1;
						return ModuleStatus.Running;
					}

					m.Output["quantity"] = thermal.HeatStored;
					m.Output["capacity"] = thermal.HeatCapacity;
					m.Output["fullness"] = (int)(100f * thermal.HeatStored / thermal.HeatCapacity);
					m.Output["product"] = Fix32.FromRaw((int)(uint)thermal.AssignedProduct.Value.Product.SlimId.Value);
					return ModuleStatus.Running;
				}

				m.Output["quantity"] = 0;
				m.Output["capacity"] = 0;
				m.Output["fullness"] = 100;
				m.Output["product"] = -1;
				return ModuleStatus.Error;
			})
			.AddDisplay("quantity", "Quantity", 1.5f.ToFix32())
			.AddDisplay("fullness", "Fullness", 1.5f.ToFix32())
			.AddDisplay("product", "Product", 1, image: true)
			.Display((m) => {
				m.Display["product"] = m.Output.Product("product")?.IconPath;
				m.Display["quantity"] = Thousands(m.Output.Integer["quantity"]);
				m.Display["fullness"] = $"{m.Output.Integer["fullness"]}%";
			})
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Connection_Transport", "Connection: Transport", "TRANS")
			.SetDescription("Transport connectable by cable 40m from controller")
			.AddCategory(Category.Connection)
			.AddCategory(Category.ConnectionRead)
			.AddOutput("quantity", "Quantity")
			.AddOutput("capacity", "Capacity")
			.AddOutput("fullness", "Fullness in %")
			.AddOutput("moving", "Is moving")
			.AddInput("direction", "Flip direction:\n1 - from close port to far port\n2 - from far port to close port")
			.AddInput("product", "Product filter")
			.AddEntityField<Transport>("entity", "Connection device")
			.AddBooleanField("fullstack", "Cap fullness to 100%", "Bigger tiers of transport may display value over 100%. It's caused by maximum stack size. Activating this option will be the value normalized to 100%.")
			.AddProductField("product", "Product", "Select filter for product", overrideInput: true)
			.Width(4)
			.Action(m => {
				Transport entity = m.Field.Entity<Transport>("entity");
				Fix32 buffer = m.Input["buffer", 0];
				ProductProto filterId = m.FieldOrInput.Product("product");
				bool fullstack = m.Field.Bool["fullstack"];

				if (entity != null) {
					m.Output["quantity"] = entity.TransportedProducts
										.Where(p => filterId is null || p.SlimId == filterId.SlimId)
										.Select(p => p.Quantity.Value).Sum();
					m.Output["capacity"] = entity.Trajectory.MaxProducts
										 * (fullstack ? entity.Prototype.MaxQuantityPerTransportedProduct.Value : 1);
					m.Output["fullness"] = (100.ToFix32() * m.Output["quantity"]) / m.Output["capacity"];
					m.Output["moving"] = entity.GetStatus() == Mafi.Core.Factory.Transports.Transport.Status.Moving ? 1 : 0;

					int dirSet = m.Input.Integer["direction"];
					if (dirSet > 0 && entity.StartInputPort.Type != IoPortType.Any) { // flipper
						bool startHere = entity.StartPosition.DistanceSqrTo(m.Controller.Position3f.Tile3i) <
										 entity.EndPosition.DistanceSqrTo(m.Controller.Position3f.Tile3i);
						bool fromHere = startHere && entity.StartInputPort.Type == IoPortType.Input;

						if (startHere && fromHere && dirSet == 2) {
							if (entity.TryReverse(out string error)) {
								m.SetError(error);
								return ModuleStatus.Error;
							}
							return ModuleStatus.Running;
						}

						if (!startHere && !fromHere && dirSet == 1) {
							if (entity.TryReverse(out string error)) {
								m.SetError(error);
								return ModuleStatus.Error;
							}
							return ModuleStatus.Running;
						}
					}

					return ModuleStatus.Running;
				}

				m.Output["quantity"] = 0;
				m.Output["capacity"] = 0;
				m.Output["fullness"] = 100;
				m.Output["moving"] = 0;
				m.SetError("Entity can not be read");
				return ModuleStatus.Error;
			})
			.AddDisplay("quantity", "Quantity", 1.5f.ToFix32())
			.AddDisplay("fullness", "Fullness", 1.5f.ToFix32())
			.AddDisplay("moving", "Is moving", 1, led: true)
			.Display((m) => {
				m.Display["moving"] = m.Output.Bool["moving"] ? "1" : "";
				m.Display["quantity"] = Thousands(m.Output.Integer["quantity"]);
				m.Display["fullness"] = $"{m.Output.Integer["fullness"]}%";
			})
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Connection_Settlement", "Connection: Settlement (population)", "SETTLE")
			.SetDescription("Reads the linked SettlementHousingModule <b>entity</b>. Outputs <b>pop_this</b> (population in this housing module) and <b>pop_nearby</b> (population of the parent settlement, falling back to this module). Outputs 0 when no housing is linked.")
			.AddCategory(Category.Connection)
			.AddCategory(Category.ConnectionRead)
			.AddCategory(Category.Stats)
			.AddOutput("pop_this", "Population (settlement module)")
			.AddOutput("pop_nearby", "Population (settlement total)")
			.AddInput("product", "First Product in #")
			.AddEntityField<SettlementHousingModule>("entity", "Connection device")
			// TODO add filter input field
			.Action(m => {
				SettlementHousingModule entity = m.Field.Entity<SettlementHousingModule>("entity");

				if (entity != null) {
					m.Output["pop_this"] = entity.Population;
					m.Output["pop_nearby"] = entity.Settlement.ValueOrNull?.Population ?? entity.Population;
					return;
				}

				m.Output["pop_this"] = 0;
				m.Output["pop_nearby"] = 0;
			})
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Connection_NuclearReactor", "Connection: Nuclear Reactor", "NR")
			.SetDescription("Reads and drives the linked NuclearReactor: outputs <b>heat</b>, <b>meltdown</b> flag, and current <b>power</b>; sets target power level from <b>target</b> input (clamped to reactor max). When <b>breed_control</b> input is on, applies <b>breed_step</b> to the enrichment cycle. Errors if no reactor is linked.")
			.AddCategory(Category.Connection)
			.AddCategory(Category.ConnectionRead)
			.AddCategory(Category.ConnectionWrite)
			.UnlockedBy(Ids.Research.NuclearReactor)
			.UnlockedBy(Ids.Research.BasicComputing)
			.AddInput("breed_control", "Control breeding")
			.AddInput("breed_step", "Breeding step")
			.AddInput("target", "Target power level")
			.AddOutput("heat", "Stored Heat")
			.AddOutput("meltdown", "Is in melt down")
			.AddOutput("power", "Actual power")
			.Width(4)
			.UseComputation(PartialQuantity.One * 2)
			.AddEntityField<NuclearReactor>("reactor", "Connection reactor", "Must be placed next to reactor (2 metres)")
			.AddBooleanField("field_breeding", "Control breeding")
			.Action(m => {
				var reactor = m.Field.Entity<NuclearReactor>("reactor");

				m.Output["heat"] = reactor?.HeatAmount.ToFix32() ?? Fix32.Zero;
				m.Output["meltdown"] = (reactor?.IsInMeltdown ?? false) ? 1.ToFix32() : 2.ToFix32();
				m.Output["power"] = reactor?.CurrentPowerLevel.ToFix32() ?? Fix32.Zero;
				m.Output.Integer["breeding"] = reactor?.EnrichmentStep ?? 0;

				if (reactor == null) {
					m.SetError("Reactor is not connected");
					return ModuleStatus.Error;
				}

				Percent target = m.Input["target", Fix32.Zero].ToPercent()
					.Clamp(Percent.Zero, reactor.MaxPowerLevelPercent);
				if (!reactor.IsInMeltdown && reactor.TargetPowerLevel != target) {
					reactor.SetTargetPowerLevel(target);
				}

				if (reactor.Prototype.Enrichment.HasValue && m.Input.Bool["breed_control"]) {
					int breeding = m.Input.Integer["breed_step"];
					if (breeding == 0 && reactor.EnrichmentStep > 0) {
						reactor.SetEnrichmentStep(0);
					} else if (breeding != 0 && breeding != reactor.EnrichmentStep) {
						reactor.SetEnrichmentStep(breeding);
					}
				}
				return ModuleStatus.Running;
			})
			.AddDisplay("power", "Power", 2, 100.ToFix32().ToStringRounded(1) + "%")
			.AddDisplay("meltdown", "Meltdown", 1, led: true)
			.AddDisplay("breading", "Breading", 1, image: true)
			.Display((m) => {
				m.Display["power"] = (m.Output["power"] * 100).ToStringRounded(1) + "%";
				m.Display["meltdown"] = m.Output["meltdown"] > 0 ? "" : "1";

				if (m.Field.Entity<NuclearReactor>("reactor") is { } reactor) {
					m.Display["breading"] = reactor.Prototype.Enrichment.HasValue && m.Output.Integer["breeding"] > 0
						? reactor.Prototype.Enrichment.Value.OutputProduct.IconPath
						: reactor.Prototype.SteamOutPerPowerLevel.Product.IconPath;
				} else {
					m.Display["breading"] = Mafi.Unity.Assets.Unity.UserInterface.General.Empty128_png;
				}
			})
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Connection_Farm", "Connection: Farm", "FARM")
			.SetDescription("Reads and drives the linked <b>farm</b>: outputs current <b>crop</b> slim-id, <b>water</b> (imported + soil), <b>fertility</b> (%), and <b>fertilizer</b> level (%). Writes <b>fertility</b> target and queues the next slot from <b>crop</b> input (slim-id), GreenManure when <b>fertilize</b> > 0, or NoCrop. Errors if no farm is linked or the crop is invalid.")
			.AddCategory(Category.Connection)
			.AddCategory(Category.ConnectionWrite)
			.AddCategory(Category.ConnectionRead)
			.AddInput("crop", "Next growing crop")
			.AddInput("fertility", "Target fertility (default: 100)")
			.AddInput("fertilize", "Assign nature fertilizing (0/1)")
			.AddOutput("crop", "Actual growing crop")
			.AddOutput("water", "Actual water level (including tank)")
			.AddOutput("fertility", "Actual fertility")
			.AddOutput("fertilizer", "Actual fertilizer level")
			.Width(4)
			.AddEntityField<Farm>("farm", "Managed farm", "Farm placed next to controller (2 metres)")
			.Action(m => {
				var farm = m.Field.Entity<Farm>("farm");
				if (farm is null) {
					m.SetError("Farm is not set");
					return ModuleStatus.Error;
				}

				if (farm.CurrentCrop.ValueOrNull is null || farm.CurrentCrop.Value.ProductProduced.IsEmpty) {
					m.Output["crop"] = Fix32.Zero;
				} else {
					m.Output["crop"] = Fix32.FromRaw(farm.CurrentCrop.Value.Prototype.ProductProduced.Product.SlimId.Value);
				}
				m.Output["water"] = farm.ImportedWaterBuffer.Quantity.Value.ToFix32() + farm.SoilWaterBuffer.Quantity.Value.ToFix32();
				m.Output["fertility"] = farm.Fertility.ToFix32() * 100;
				if (farm.StoredFertilizerCapacity.Value > 0) {
					m.Output["fertilizer"] = (100f * farm.StoredFertilizerCount.Value / farm.StoredFertilizerCapacity.Value).ToFix32();
				} else {
					m.Output["fertilizer"] = Fix32.Zero;
				}

				farm.SetFertilityTarget((m.Input["fertility", Fix32.One * 100] / 100).ToPercent());

				int nextSlot = (farm.ActiveScheduleIndex + 1) % 4;
				if (m.Input["crop", Fix32.Zero] > Fix32.Zero) { // try get crop type by output product
					int slimId = m.Input["crop", Fix32.Zero].RawValue;
					CropProto foundCrop = m.Context.ProtosDb
						.Filter<CropProto>(crop => crop.ProductProduced.Product.SlimId.Value == slimId)
						.FirstOrDefault();

					if (foundCrop != null) {
						m.SetError("");
						farm.AssignCropToSlot(foundCrop.CreateOption(), nextSlot);
						return ModuleStatus.Running;
					} else {
						m.SetError("Invalid crop");
						farm.AssignCropToSlot(m.Context.ProtosDb.Get<CropProto>(Ids.Crops.NoCrop), nextSlot);
						return ModuleStatus.Error;
					}
				} else if (m.Input["crop", Fix32.Zero] == 0 && m.Input["fertilize", Fix32.Zero] > Fix32.Zero) {
					farm.AssignCropToSlot(m.Context.ProtosDb.Get<CropProto>(Ids.Crops.GreenManure), nextSlot);
					return ModuleStatus.Running;
				} else {
					farm.AssignCropToSlot(m.Context.ProtosDb.Get<CropProto>(Ids.Crops.NoCrop), nextSlot);
					return ModuleStatus.Running;
				}
			})
			.AddDisplay("crop_3", "Crop 4", 1, image: true)
			.AddDisplay("crop_2", "Crop 3", 1, image: true)
			.AddDisplay("crop_1", "Crop 2", 1, image: true)
			.AddDisplay("crop_0", "Crop 1", 1, image: true)
			.Display((m) => {
				var farm = m.Field.Entity<Farm>("farm");
				if (farm is null) {
					return;
				}

				string emptyCrop = m.Context.ProtosDb.Get<CropProto>(Ids.Crops.NoCrop).Value.IconPath;
				DisplayCrop(0);
				DisplayCrop(1);
				DisplayCrop(2);
				DisplayCrop(3);

				void DisplayCrop(int i) {
					Option<CropProto> crop = farm.Schedule[i];
					if (crop.ValueOrNull is null) {
						m.Display["crop_" + i] = emptyCrop;
					} else {
						m.Display["crop_" + i] = crop.Value.IconPath ??
							crop.Value.ProductProduced.Product.IconPath;
					}
				}
			})
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Connection_Import_Set", "Connection: Import (set)", "IMS")
			.SetDescription("Sets the import logistics mode of the linked <b>logistic</b> building from <b>mode</b> input (0 = auto, 1 = on, 2 = off). For buildings with simple logistics, only on/off is meaningful (mode 2 disables input). Errors if no compatible building is linked.")
			.AddCategory(Category.Connection)
			.AddCategory(Category.ConnectionWrite)
			.AddInput("mode", "Import mode:\n  0 (auto),\n  1 (on),\n  2 (off)")
			.AddEntityField<IStaticEntity>("logistic", "Logistic building", "Any logistic building (40 metres)",
				filter: (m, e) => e is IEntityWithLogisticsControl or IEntityWithSimpleLogisticsControl)
			.Action(m => {
				IEntity entity = m.Field.Entity<IEntity>("logistic");
				if (entity is IEntityWithLogisticsControl logistic) {
					logistic.SetLogisticsInputMode((EntityLogisticsMode)m.Input["mode", 0].IntegerPart);
					return ModuleStatus.Running;
				} else if (entity is IEntityWithSimpleLogisticsControl simple) {
					simple.SetLogisticsInputDisabled(m.Input["mode", 0].IntegerPart == 2);
					return ModuleStatus.Running;
				} else {
					m.SetError("Building is not connected");
					return ModuleStatus.Error;
				}
			})
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Connection_Import_Get", "Connection: Import (get)", "IMG")
			.SetDescription("Reads the import logistics mode of the linked <b>logistic</b> building to output <b>mode</b> (0 = auto, 1 = on, 2 = off). For buildings with simple logistics, outputs 1 (enabled) or 2 (disabled). Errors if no compatible building is linked.")
			.AddCategory(Category.Connection)
			.AddCategory(Category.ConnectionRead)
			.AddOutput("mode", "Import mode:\n  0 (auto),\n  1 (on),\n  2 (off)")
			.AddEntityField<IStaticEntity>("logistic", "Logistic building", "Any logistic building (40 metres)",
				filter: (m, e) => e is IEntityWithLogisticsControl || e is IEntityWithSimpleLogisticsControl)
			.Action(m => {
				IEntity entity = m.Field.Entity<IEntity>("logistic");
				if (entity is IEntityWithLogisticsControl logistic) {
					m.Output["mode"] = ((int)logistic.LogisticsInputMode).ToFix32();
					return ModuleStatus.Running;
				} else if (entity is IEntityWithSimpleLogisticsControl simple) {
					m.Output["mode"] = (simple.IsLogisticsInputDisabled ? 2 : 1).ToFix32();
					return ModuleStatus.Running;
				} else {
					m.SetError("Building is not connected");
					return ModuleStatus.Error;
				}
			})
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Connection_Export_Set", "Connection: Export (set)", "EXS")
			.SetDescription("Sets the export logistics mode of the linked <b>logistic</b> building from <b>mode</b> input (0 = auto, 1 = on, 2 = off). For buildings with simple logistics, only on/off is meaningful (mode 2 disables output). Errors if no compatible building is linked.")
			.AddCategory(Category.Connection)
			.AddCategory(Category.ConnectionWrite)
			.AddInput("mode", "Export mode:\n  0 (auto),\n  1 (on),\n  2 (off)")
			.AddEntityField<IStaticEntity>("logistic", "Logistic building", "Any logistic building (40 metres)",
				filter: (m, e) => e is IEntityWithLogisticsControl or IEntityWithSimpleLogisticsControl)
			.Action(m => {
				IEntity entity = m.Field.Entity<IEntity>("logistic");
				if (entity is IEntityWithLogisticsControl logistic) {
					logistic.SetLogisticsOutputMode((EntityLogisticsMode)m.Input["mode", 0].IntegerPart);
					return ModuleStatus.Running;
				} else if (entity is IEntityWithSimpleLogisticsControl simple) {
					simple.SetLogisticsOutputDisabled(m.Input["mode", 0].IntegerPart == 2);
					return ModuleStatus.Running;
				} else {
					m.SetError("Building is not connected");
					return ModuleStatus.Error;
				}
			})
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Connection_Export_Get", "Connection: Export (get)", "EXG")
			.SetDescription("Reads the export logistics mode of the linked <b>logistic</b> building to output <b>mode</b> (0 = auto, 1 = on, 2 = off). For buildings with simple logistics, outputs 1 (enabled) or 2 (disabled). Errors if no compatible building is linked.")
			.AddCategory(Category.Connection)
			.AddCategory(Category.ConnectionRead)
			.AddOutput("mode", "Export mode:\n  0 (auto),\n  1 (on),\n  2 (off)")
			.AddEntityField<IStaticEntity>("logistic", "Logistic building", "Any logistic building (40 metres)",
				filter: (m, e) => e is IEntityWithLogisticsControl || e is IEntityWithSimpleLogisticsControl)
			.Action(m => {
				IEntity entity = m.Field.Entity<IEntity>("logistic");
				if (entity is IEntityWithLogisticsControl logistic) {
					m.Output["mode"] = ((int)logistic.LogisticsOutputMode).ToFix32();
					return ModuleStatus.Running;
				} else if (entity is IEntityWithSimpleLogisticsControl simple) {
					m.Output["mode"] = (simple.IsLogisticsOutputDisabled ? 2 : 1).ToFix32();
					return ModuleStatus.Running;
				} else {
					m.SetError("Building is not connected");
					return ModuleStatus.Error;
				}
			})
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Connection_Priority_Set", "Connection: Priority (set)", "P-S")
			.SetDescription("Sets the general priority (1-15) of the linked <b>logistic</b> building from the <b>priority</b> input (defaults to 8 when unconnected). Errors if no priority-capable building is linked.")
			.AddCategory(Category.Connection)
			.AddCategory(Category.ConnectionWrite)
			.AddInput("priority", "Priority: 1 - 15")
			.AddEntityField<IEntityWithGeneralPriority>("logistic", "Logistic building", "Any logistic building (40 metres)")
			.Action(m => {
				var logistic = m.Field.Entity<IEntityWithGeneralPriority>("logistic");
				if (logistic is null) {
					m.SetError("Building is not connected");
					return ModuleStatus.Error;
				}
				logistic.SetGeneralPriority(m.Input["priority", 8.ToFix32()].IntegerPart);
				return ModuleStatus.Running;
			})
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Connection_Priority_Get", "Connection: Priority (get)", "P-G")
			.SetDescription("Reads the general priority (1-15) of the linked <b>building</b> and outputs it on <b>priority</b>. Errors if no priority-capable building is linked.")
			.AddCategory(Category.Connection)
			.AddCategory(Category.ConnectionRead)
			.AddOutput("priority", "Priority: 1 - 15")
			.AddEntityField<IEntityWithGeneralPriority>("building", "Building", "Any building with configurable priority (40 metres)")
			.Action(m => {
				var logistic = m.Field.Entity<IEntityWithGeneralPriority>("building");
				if (logistic is null) {
					m.SetError("Building is not connected");
					return ModuleStatus.Error;
				}
				m.Output["priority"] = logistic.GeneralPriority;
				return ModuleStatus.Running;
			})
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Connection_Vehicle_Get", "Connection: Vehicle count (get)", "V-G")
			.SetDescription("Reads the vehicle count assigned to the linked <b>building</b>. With a vehicle type chosen via the <b>vehicle</b> input or <b>vehicle</b> field (toggled by <b>field_vehicle</b>), outputs the count of that type to <b>count</b>; otherwise outputs the total count of all assigned vehicles. Errors if no building is linked or the type is invalid.")
			.AddCategory(Category.Connection)
			.AddCategory(Category.ConnectionRead)
			.Width(4)
			.AddInput("vehicle", "Vehicle type")
			.AddOutput("count", "Vehicle count")
			.AddEntityField<IEntityAssignedWithVehicles>("building", "Building", "Any building with configurable priority (40 metres)")
			.AddEntityTypeField<DrivingEntityProto>("vehicle", "Vehicle", "Any suppoted vehicle type",
				filter: (m, p) => m.Field.Entity<Entity>("building") is IEntityAssignedWithVehicles w && w.CanVehicleBeAssigned(p),
				overrideInput: true) // TODO filter by building
			.Action(m => {
				var logistic = m.Field.Entity<IEntityAssignedWithVehicles>("building");
				if (logistic is null) {
					m.SetError("Building is not connected");
					return ModuleStatus.Error;
				}
				DrivingEntityProto drivingEntity = m.FieldOrInput.EntityProtoIconified("vehicle") as DrivingEntityProto;
				if (drivingEntity is null) {
					m.Output["count"] = logistic.AllVehicles.Count;
					return ModuleStatus.Running;
				}

				if (!logistic.CanVehicleBeAssigned(drivingEntity)) {
					m.SetError("Invalid vehicle type");
					return ModuleStatus.Error;
				}

				m.Output["count"] = logistic.AllVehicles.Where(v => v.Prototype == drivingEntity).Count();
				return ModuleStatus.Running;
			})
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Connection_Vehicle_Set", "Connection: Vehicle count (set)", "V-S")
			.AddCategory(Category.Connection)
			.AddCategory(Category.ConnectionRead)
			.SetDescription("Sets count of vehicles assigned to the building, by default it takes vehicles from all zones.")
			.Width(4)
			.AddInput("count", "Vehicle count")
			.AddInput("vehicle", "Vehicle type")
			.AddEntityField<IEntityAssignedWithVehicles>("building", "Building", "Any building with configurable priority (40 metres)")
			.AddEntityTypeField<DrivingEntityProto>("vehicle", "Vehicle", "Any suppoted vehicle type",
				filter: (m, p) => m.Field.Entity<Entity>("building") is IEntityAssignedWithVehicles w && w.CanVehicleBeAssigned(p),
				overrideInput: true) // TODO filter by building
			.AddInt32Field("count", "Vehicle count", defaultValue: 0, overrideInput: true)
			.AddCustomField("zone", "Vehicle zone",
				(ControllerInspector inspector, UiComponent container, Module module, Action refresh, Reference reference) => {
					Dropdown<LogisticsZone> zonesDropdown = new Dropdown<LogisticsZone>(
						(LogisticsZone option, int index, bool isInDropdown) => new ZoneUi().Value(option));
					LogisticsZone zones = reference.StringValue is { } zone
						? module.Controller.Context.LogisticsZonesManager.GetFirstZoneForMask(ulong.Parse(zone)).Value
						: module.Controller.Context.LogisticsZonesManager.DefaultZone;
					zonesDropdown.SetValue(zones);
					zonesDropdown.OnValueChanged((z, _) => reference.StringValue = z.Mask.ToString());
					container.Add(zonesDropdown);
				})
			.Action(m => {
				var logistic = m.Field.Entity<IEntityAssignedWithVehicles>("building");
				if (logistic is null) {
					m.SetError("Building is not connected");
					return ModuleStatus.Error;
				}

				DrivingEntityProto drivingEntity = m.FieldOrInput.EntityProtoIconified("vehicle") as DrivingEntityProto;

				if (drivingEntity is null || !logistic.CanVehicleBeAssigned(drivingEntity)) {
					m.SetError("Invalid vehicle type");
					return ModuleStatus.Error;
				}

				if (!drivingEntity.IsAvailable) {
					m.SetError("Vehicle type not unlocked yet");
					return ModuleStatus.Error;
				}

				int count = m.FieldOrInput["count", Fix32.Zero].IntegerPart;

				int actualCount = logistic.AllVehicles.Where(v => v.Prototype == drivingEntity).Count();
				if (actualCount < count) {
					ulong zones = m.Field["zone", null] is { } zone
						? ulong.Parse(zone)
						: m.Controller.Context.LogisticsZonesManager.DefaultZone.Mask;
					Option<Vehicle> v = GlobalDependencyResolver.Get<IVehiclesManager>()
						.GetFreeVehicle<Vehicle>(drivingEntity, logistic.Position2f, zones);
					if (v.HasValue) {
						logistic.AssignVehicle(v.Value, doNotCancelJobs: true);
						m.Warning = false;
					} else {
						// No spare vehicle of this type in the chosen zone — surface it instead of
						// silently no-oping so the user knows they need to build/free one up.
						m.Warning = true;
					}
				} else if (actualCount > count) {
					Vehicle veh = logistic.AllVehicles.Where(v => v.Prototype == drivingEntity).FirstOrDefault();
					logistic.UnassignVehicle(veh, cancelJobs: false);
					m.Warning = false;
				} else {
					m.Warning = false;
				}
				return ModuleStatus.Running;
			})
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Connection_Filter_Get", "Connection: Filter (get)", "F-G")
			.SetDescription("Reads the assigned product slim-id from the linked <b>entity</b> and outputs it on <b>product</b>. Supports storages, train stations, settlement food/service, hospitals, virtual miners, and sorters; the <b>index</b> input/field selects the compartment/slot. Errors if the entity type is unsupported.")
			.AddCategory(Category.Connection)
			.AddCategory(Category.ConnectionRead)
			.Width(2)
			.AddInput("index", "Storage compartment")
			.AddOutput("product", "Product type")
			.AddEntityField<LayoutEntity>("entity", "Connection device", "Storage connectable by cable 40m from controller",
				filter: (m, e) => e is StorageBase || // e is SettlementWasteModule
								  e is TrainStationModule ||
								  e is SettlementFoodModule ||
								  e is Hospital ||
								  e is SettlementServiceModule ||
								  e is IVirtualResourceMiningEntity ||
								  e is Sorter // ||
											  // e is OreSortingPlant
				)
			.AddInt32Field("index", "Storage compartment", overrideInput: true)
			.AddDisplayFiller(1)
			.AddDisplay("product", "Product", 1, image: true)
			.Display(m => m.Display["product"] = m.Output.Product("product")?.IconPath)
			.Action(m => {
				LayoutEntity entity = m.Field.Entity<LayoutEntity>("entity");

				if (entity is StorageBase storage)
				// entity is SettlementWasteModule
				{
					if (storage.StoredProduct.HasValue) {
						m.Output["product"] = Fix32.FromRaw((int)(uint)storage.StoredProduct.Value.SlimId.Value);
					} else {
						m.Output["product"] = Fix32.Zero;
					}
					return ModuleStatus.Running;
				}

				if (entity is TrainStationModule station) {
					if (station.StoredProduct.HasValue) {
						m.Output["product"] = Fix32.FromRaw((int)(uint)station.StoredProduct.Value.SlimId.Value);
					} else {
						m.Output["product"] = Fix32.Zero;
					}
					return ModuleStatus.Running;
				}

				if (entity is SettlementFoodModule foodModule) {
					ProductProto product = m.Input.Product("product");
					var buffers = new Func<IProductBuffer>[] {
						() => foodModule.GetBuffer(0).ValueOrNull,
						() => foodModule.GetBuffer(1).ValueOrNull
					};
					return GetTypeFromBuffer(m, buffers);
				}

				if (entity is Hospital hospital) {
					var buffers = new Func<IProductBuffer>[] {
						() => hospital.GetBuffer(0).ValueOrNull,
						() => hospital.GetBuffer(1).ValueOrNull
					};
					return GetTypeFromBuffer(m, buffers);
				}

				if (entity is SettlementServiceModule module) {
					var buffers = new Func<IProductBuffer>[] {
						() => (IProductBuffer)module.GetType().GetField("m_inputBuffer", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(module),
						() => ((Option<IProductBuffer>)module.GetType().GetField("m_inputBuffer", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(module)).ValueOrNull
					};
					return GetTypeFromBuffer(m, buffers);
				}

				if (entity is IVirtualResourceMiningEntity miner) {
					m.Output["product"] = Fix32.FromRaw((int)(uint)miner.ProductToMine.SlimId.Value);
					return ModuleStatus.Running;
				}

				if (entity is Sorter sorter) {
					int index = m.FieldOrInput["index", Fix32.Zero].IntegerPart;
					m.Output["product"] = Fix32.FromRaw((int)((uint?)sorter.FilteredProducts.Skip(index).FirstOrDefault()?.SlimId.Value ?? 0));
					return ModuleStatus.Running;
				}

				//if (entity is OreSortingPlant oreSorter)
				//{
				//    int index = m.FieldOrInput["index", Fix32.Zero].IntegerPart;
				//    char port = (char)('A' + index);
				//    var first = oreSorter.ProductsData.Where(d => d.Value.OutputPort == port).FirstOrDefault();
				//
				//    m.Output["product"] = first.Value is null ? Fix32.Zero :
				//        Fix32.FromRaw((int)(uint)first.Value.Buffer.Product.SlimId.Value);
				//    return ModuleStatus.Running;
				//}

				m.Output["product"] = -1;
				return ModuleStatus.Error;
			})
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Connection_Filter_Set", "Connection: Filter (set)", "F-S")
			.SetDescription("Assigns a product filter to the linked <b>entity</b> from the <b>product</b> input or <b>product</b> field (toggled by <b>field_product</b>); the <b>index</b> input/field picks the compartment for multi-slot buildings. Supports storages, cargo depots, train stations, settlement food, hospitals, mine towers, and sorters. Errors if the entity is unsupported or storage is non-empty when re-assigning.")
			.AddCategory(Category.Connection)
			.AddCategory(Category.ConnectionWrite)
			.Width(2)
			.AddInput("index", "Storage compartment")
			.AddInput("product", "Product type")
			.AddEntityField<LayoutEntity>("entity", "Building with filter", "Connectable by cable 40m from controller",
				filter: (m, e) => e is Storage ||
								  e is TrainStationModule ||
								  e is CargoDepotModule ||
								  e is SettlementFoodModule ||
								  e is Hospital ||
								  e is MineTower ||
								  e is Sorter
				)
			.AddInt32Field("index", "Storage compartment", overrideInput: true)
			.AddProductField("product", "Filtered product", filter: (m, p) => true, overrideInput: true)
			.AddDisplay("index", "Index", 1)
			.AddDisplay("product", "Product", 1, image: true)
			.Display(m => {
				m.Display["index"] = m.FieldOrInput.Integer["index"].ToString();
				m.Display["product"] = m.FieldOrInput.Product("product")?.IconPath;
			})
			.Action(m => {
				LayoutEntity entity = m.Field.Entity<LayoutEntity>("entity");

				if (entity is Storage storage) {
					ProductProto product = m.FieldOrInput.Product("product");
					if (product is null && !(storage.CurrentQuantity > Quantity.Zero)) {
						storage.ToggleClearProduct();
					} else if (storage.StoredProduct.ValueOrNull != product && !(storage.CurrentQuantity > Quantity.Zero)) {
						storage.AssignProduct(product);
					}
					return ModuleStatus.Running;
				}

				if (entity is CargoDepotModule module) {
					ProductProto product = m.FieldOrInput.Product("product");
					if (product is null && !(module.CurrentQuantity > Quantity.Zero)) {
						module.ToggleClearProduct();
					} else if (module.StoredProduct.ValueOrNull != product && !(module.CurrentQuantity > Quantity.Zero)) {
						module.AssignProduct(product);
					}
					return ModuleStatus.Running;
				}

				if (entity is TrainStationModule station) {
					ProductProto product = m.FieldOrInput.Product("product");
					if (product is null && !(station.StoredProductQuantity?.Quantity > Quantity.Zero)) {
						station.ClearAssignedProduct();
					} else if (station.StoredProduct.ValueOrNull != product
								&& !(station.StoredProductQuantity?.Quantity > Quantity.Zero)
								&& !station.TryAssignProduct(product)) {
						if (station.StoredProduct.ValueOrNull != product
						&& !(station.StoredProduct.ValueOrNull is null)) {
							station.ClearAssignedProduct();
						}
					}
					return ModuleStatus.Running;
				}

				if (entity is SettlementFoodModule foodModule) {
					int index = m.FieldOrInput["index", Fix32.Zero].IntegerPart;
					if (index >= 2 || index < 0) {
						m.SetError("Invalid compartment index");
						return ModuleStatus.Error;
					}

					Option<ProductProto> product = m.FieldOrInput.Product("product").CreateOption();
					Option<ProductProto> actual = foodModule.GetBuffer(index).AsOption<IProductBuffer, ProductProto>(b => b.Product);
					if (actual.ValueOrNull != product.ValueOrNull && !(foodModule.GetBuffer(index).ValueOrNull?.Quantity > Quantity.Zero)) {
						foodModule.SetProduct(product, index, false);
					}

					if (product.HasValue || foodModule.GetBuffer(index).HasValue) {
						m.Warning = !(product.Value?.SlimId == foodModule.GetBuffer(index).Value?.Product.SlimId);
						if (actual.HasValue && foodModule.GetBuffer(index).Value.Quantity > Quantity.Zero) {
							m.SetError("Storage is not empty");
						}
						return ModuleStatus.Running;
					} else {
						return ModuleStatus.Running;
					}
				}

				if (entity is Hospital hospital) {
					int index = m.FieldOrInput["index", Fix32.Zero].IntegerPart;
					if (index >= 2 || index < 0) {
						m.SetError("Invalid compartment index");
						return ModuleStatus.Error;
					}

					Option<ProductProto> product = m.FieldOrInput.Product("product").CreateOption();
					hospital.SetProduct(product, index, false);
					if (product.HasValue || hospital.GetBuffer(index).HasValue) {
						m.Warning = !(product.Value?.SlimId == hospital.GetBuffer(index).Value?.Product.SlimId);
						return ModuleStatus.Running;
					} else {
						return ModuleStatus.Running;
					}
				}

				if (entity is MineTower tower) {
					Option<ProductProto> product = m.FieldOrInput.Product("product").CreateOption();
					if (product.HasValue && product.Value is LooseProductProto loose) {
						foreach (var ecavator in tower.AllAssignedExcavators) {
							ecavator.SetPrioritizeProduct(loose);
						}
					} else {
						foreach (var ecavator in tower.AllAssignedExcavators) {
							ecavator.SetPrioritizeProduct(Option.None);
						}
					}
					return ModuleStatus.Running;
				}

				if (entity is Sorter sorter) {
					ProductProto newProduct = m.FieldOrInput.Product("product");
					if (m.NumberData.TryGetValue("old__product", out int product)) {
						if (product != 0 && (newProduct is null || (int)(uint)newProduct.SlimId.Value != product)) { // remove old from set
							ProductProto oldProduct = sorter.FilteredProducts
									.FirstOrDefault(p => (int)(uint)p.SlimId.Value == product);
							if (oldProduct != null) {
								sorter.ToggleFilteredProduct(oldProduct);
							}
						}
					}
					if (!(newProduct is null) && sorter.FilteredProducts
							  .FirstOrDefault(p => p.SlimId == newProduct.SlimId)
							  is null) { // add to set
						sorter.ToggleFilteredProduct(newProduct);
						m.NumberData["old__product"] = (int)(uint)newProduct.SlimId.Value;
					}
					return ModuleStatus.Running;
				}

				m.Output["product"] = 0;
				return ModuleStatus.Error;
			})
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Connection_Recipe_Set", "Connection: Recipe (set)", "REC-S")
			.SetDescription("Assigns the recipe selected in the <b>recipe</b> field to the linked <b>entity</b> Machine when <b>on</b> is true (input or <b>on</b> field, toggled by <b>field_on</b>). Clears existing assignments and re-assigns; does nothing while <b>on</b> is false. Errors if no machine is linked, no recipe is selected, or the recipe could not be assigned.")
			.AddCategory(Category.Connection)
			.AddCategory(Category.ConnectionWrite)
			.Width(2)
			.AddInput("on", "Active recipe")
			.AddEntityField<Machine>("entity", "Building with filter", "Connectable by cable 40m from controller",
				filter: (m, e) => true /* Get info about is able to set recipe */)
			.AddBooleanField("on", "Active recipe", overrideInput: true)
			.AddCustomField("recipe", "Recipe", (inspector, settings, module, refresh, reference) => settings.Add(new Ui.RecipeSelector(inspector, module, refresh, reference)))
			.Action(m => {
				Machine entity = m.Field.Entity<Machine>("entity");

				if (entity is null) {
					m.SetError("Disconnected machine");
					return ModuleStatus.Error;
				}

				if (!m.FieldOrInput.Bool["on"]) {
					return ModuleStatus.Running;
				}

				string recipeId = m.Field["recipe", (string)null];
				if (recipeId.IsNullOrEmpty() ||
					!(m.Controller.Context.ProtosDb.Get(new Mafi.Core.Prototypes.Proto.ID(recipeId)).ValueOrNull is RecipeProto recipe)) {
					m.SetError("No recipe selected");
					return ModuleStatus.Error;
				}

				if (entity.RecipesAssigned.Count > 0 && entity.RecipesAssigned[0] != recipe) {
					entity.ClearAssignedRecipes();
					entity.AssignRecipe(recipe);
				}

				// in case it was not set again
				if (entity.RecipesAssigned.Count == 0 || entity.RecipesAssigned[0] != recipe) {
					m.SetError("Recipe can not be assigned");
					return ModuleStatus.Error;
				}

				return ModuleStatus.Running;
			})
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Connection_Station_TrainInfo", "Connection: Station", "ST-TR")
			.SetDescription("Reads and toggles the linked <b>station</b> (TrainStationBase). Outputs <b>instation</b> (1 when a train of the station group is present) and, for station modules, <b>cargo</b> (1 = loading, 2 = unloading). The <b>direction</b> input switches the module between loading (1) and unloading (2). Errors if no station is linked.")
			.AddCategory(Category.Connection)
			.AddCategory(Category.ConnectionRead)
			.Width(4)
			.AddInput("direction", $"{Tr.ToggleDirection}:\n1 - {Tr.TrainStation_Loading.TranslatedString}\n2 - {Tr.TrainStation_Unloading.TranslatedString}")
			.AddOutput("instation", Tr.TrainStatus_AtStation.TranslatedString)
			.AddOutput("cargo", $"{Tr.EntityStatus__Working}:\n1 - {Tr.TrainStation_Loading.TranslatedString}\n2 - {Tr.TrainStation_Unloading.TranslatedString}")
			.AddDisplayFiller(2)
			.AddDisplay("instation", Tr.TrainStatus_AtStation.TranslatedString, 1, led: true)
			.AddDisplay("cargo", $"{Tr.TrainStation_Loading.TranslatedString} / {Tr.TrainStation_Unloading.TranslatedString}", 1, image: true)
			.Display(m => {
				m.Display["instation"] = m.Output.Bool["instation"] ? "1" : "";

				if (m.Output.Bool["unloading"]) {
					m.Display["cargo"] = Mafi.Unity.Assets.Unity.UserInterface.General.MoveUp_svg;
				} else if (m.Output.Bool["loading"]) {
					m.Display["cargo"] = Mafi.Unity.Assets.Unity.UserInterface.General.MoveDown_svg;
				} else {
					m.Display["cargo"] = null;
				}
			})
			.AddEntityField<TrainStationBase>("station",
				registrator.PrototypesDb.Get<TrainStationBaseProto>(Ids.TrainTracks.TrainStationRoot).ValueOrNull?.Strings.Name.TranslatedString ?? "Train station")
			.Action(m => {
				TrainStationBase stationBase = m.Field.Entity<TrainStationBase>("station");
				if (stationBase is null) {
					m.SetError("Station or module is not connected");
					return ModuleStatus.Error;
				}

				TrainsManager trainsManager = GlobalDependencyResolver.Get<TrainsManager>();

				var isGroup = trainsManager.TrainStationManager
					.TrainStationEntities.TryGetValue(stationBase, out var group);

				var train = trainsManager
					.Trains.FirstOrDefault(t => isGroup && group.StationEntities.Any(g => t.CurrentStation == g.Station));
				m.Output.Bool["instation"] = train != null;

				if (stationBase is TrainStationModule module) {
					int dir = m.FieldOrInput.Integer["direction"];
					if (dir == 1) {
						module.SetLoadingUnloading(true);
					} else if (dir == 2) {
						module.SetLoadingUnloading(false);
					}
					// else unchanged

					m.Output.Bool["loading"] = module.IsLoading;
					m.Output.Bool["unloading"] = module.IsUnloading;

					return ModuleStatus.Running;
				}

				return ModuleStatus.Running;
			})
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Connection_IsActive", "Connection: Status", "STAT")
			.SetDescription("Reads operational status of the linked <b>entity</b>. Outputs <b>power</b> (1 when not power-starved or not power-consuming), <b>workers</b> (1 when staffed or no workers needed), <b>constructed</b> (1 when build is finished), and <b>pause</b> (1 when the entity is paused). Errors when no entity is linked.")
			.AddCategory(Category.Connection)
			.AddCategory(Category.ConnectionRead)
			.Width(4)
			.AddOutput("power", "Has enough power")
			.AddOutput("workers", "Has enough workers")
			.AddOutput("constructed", "Is build")
			.AddOutput("pause", "Is Paused")
			.AddDisplay("power", "Electricity", 1, image: true)
			.AddDisplay("workers", "Workers", 1, image: true)
			.AddDisplay("constructed", "Constructed", 1, led: true)
			.AddDisplay("pause", "Paused", 1, image: true)
			.AddEntityField<StaticEntity>("entity", "Connection device",
				"Any pausable building connectable by cable 40m from controller")
			.Action(m => {
				StaticEntity e = m.Field.Entity<StaticEntity>("entity");
				if (e is not null) {
					if (e is IElectricityConsumingEntity electric && electric.ElectricityConsumer.HasValue) {
						bool hasPower = electric.ElectricityConsumer.Value.NotEnoughPower == false;
						m.Output.Bool["power"] = hasPower || (e.CanBePaused && e.IsPaused);
						string boxC = hasPower ? "#P" : "#E";
						string imgC = hasPower ? "#C00DD00" : "#CDD0000";
						m.Display["power"] = $"{boxC}{imgC}{UserInterface.EntityIcons.Electricity_png}";
					} else {
						m.Output.Bool["power"] = true;
						m.Display["power"] = $"#I{UserInterface.EntityIcons.Electricity_png}";
					}
					if (e is IEntityWithWorkers workersEntity && workersEntity.WorkersNeeded > 0) {
						bool hasWorkers = workersEntity.HasWorkersCached;
						m.Output.Bool["workers"] = hasWorkers || (e.CanBePaused && e.IsPaused);
						string boxC = hasWorkers ? "#P" : "#E";
						string imgC = hasWorkers ? "#C00DD00" : "#CDD0000";
						m.Display["workers"] = $"{boxC}{imgC}{UserInterface.EntityIcons.Worker_png}";
					} else {
						m.Output.Bool["workers"] = false;
						m.Display["workers"] = $"#I{UserInterface.EntityIcons.Worker_png}";
					}
					m.Output["constructed"] = e.IsConstructed ? 1 : 0;
					m.Display["constructed"] = e.IsConstructed ? "1" : "";
					m.Output["pause"] = (e.CanBePaused && e.IsPaused) ? 1 : 0;
					m.Display["pause"] = (e.CanBePaused && e.IsPaused)
						? $"#CFFDD00{UserInterface.Toolbar.Pause128_png}"
						: $"#C00FF00{UserInterface.EntityIcons.Gears_png}";
					return ModuleStatus.Running;
				} else {
					m.Output["power"] = 0;
					m.Output["workers"] = 0;
					m.Output["constructed"] = 1;
					m.Output["pause"] = 1;
					m.Display["power"] = $"#I{UserInterface.EntityIcons.Electricity_png}";
					m.Display["workers"] = $"#I{UserInterface.EntityIcons.Worker_png}";
					m.Display["constructed"] = "";
					m.Display["pause"] = $"#C00FF00{UserInterface.EntityIcons.Gears_png}";
					return ModuleStatus.Error;
				}
			})
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Connection_Boost_Set", "Connection: Unity Boost", "BST")
			.SetDescription("Drives the Unity boost state of the linked <b>entity</b> from the <b>boost</b> input or <b>boost</b> field (toggled by <b>field_boost</b>) and reflects the current request on the <b>boost</b> output. Errors when no boost-capable entity is linked or when the building has no BoostCost.")
			.AddCategory(Category.Connection)
			.AddCategory(Category.ConnectionWrite)
			.AddCategory(Category.ConnectionRead)
			.Width(1)
			.AddInput("boost", "Unity boost active")
			.AddOutput("boost", "Unity boost active")
			.AddEntityField<IEntityWithBoost>("entity", "Connection device",
				"Any building connectable by cable 40m from controller")
			.AddBooleanField("boost", "Set boost by settings", defaultValue: false, overrideInput: true)
			.AddDisplay("boost", "Boost", 1, image: true)
			.Action(m => {
				IEntityWithBoost e = m.Field.Entity<IEntityWithBoost>("entity");
				if (e is not null && (e is not LayoutEntity ent || ent.Prototype.BoostCost.HasValue)) {
					bool boost = m.FieldOrInput.Bool["boost"];
					e.SetBoosted(boost);
					m.Output.Bool["boost"] = e.IsBoostRequested;
					return ModuleStatus.Running;
				} else {
					m.Output.Bool["boost"] = false;
					return ModuleStatus.Error;
				}
			})
			.Display(m => {
				m.Display["boost"] = m.Output.Bool["boost"]
					? $"#CA000E0{UserInterface.EntityIcons.Boost_png}"
					: $"#C606060{UserInterface.EntityIcons.Boost_png}";
			})
			.BuildAndAdd();
	}

	private string Thousands(int v) {
		if (v > 1100000) {
			return (v / 1000000).ToString();
		}
		if (v > 900000) {
			return $"{(v.ToFix32() / 100000).ToStringRounded(1)}M";
		}
		if (v > 1100) {
			return (v / 1000000).ToString();
		}
		if (v > 900) {
			return $"{(v.ToFix32() / 100).ToStringRounded(1)}k";
		}
		return v.ToString();
	}

	private ModuleStatus GetValueFromBuffers(Module m, ProductProto product, IEnumerable<IProductBufferReadOnly> buffers) {
		ProductProto productProto = product;
		Quantity quantity = Quantity.Zero;
		Quantity capacity = Quantity.Zero;

		foreach (var buffer in buffers) {
			if (buffer is null) {
				continue;
			}
			if (productProto != null && buffer.Product.Id != productProto.Id) {
				continue;
			}

			productProto = buffer.Product;
			quantity += buffer.Quantity;
			capacity += buffer.Capacity;
		}

		if (capacity == Quantity.Zero) {
			m.SetError(product != null ? "Invalid filter product" : "No product set in storage");

			m.Output["quantity"] = 0;
			m.Output["capacity"] = 0;
			m.Output["fullness"] = 100f.ToFix32();
			m.Output["product"] = Fix32.FromRaw(0);
			return ModuleStatus.Error;
		}

		m.Output["quantity"] = quantity.Value;
		m.Output["capacity"] = capacity.Value;
		m.Output["fullness"] = (100f * quantity.Value / capacity.Value).ToFix32();
		m.Output["product"] = Fix32.FromRaw(productProto.SlimId.Value);
		return ModuleStatus.Running;
	}

	private ModuleStatus GetTypeFromBuffer(Module m, Func<IProductBuffer>[] buffers) {
		if (buffers.Length == 1) {
			m.Output["product"] = Fix32.FromRaw(buffers[0].Invoke().Product.SlimId.Value);
			return ModuleStatus.Running;
		}

		int index = m.FieldOrInput["index", Fix32.Zero].IntegerPart;
		if (index >= buffers.Length || index < 0) {
			m.SetError("Invalid compartment index");
			return ModuleStatus.Error;
		}

		m.Output["product"] = Fix32.FromRaw(buffers[index].Invoke()?.Product.SlimId.Value ?? 0);
		return ModuleStatus.Running;
	}

	private ModuleStatus GetValueFromBuffer(Module m, IProductBufferReadOnly buffer) {
		m.Output["quantity"] = buffer?.Quantity.Value ?? 0;
		m.Output["capacity"] = buffer?.Capacity.Value ?? 0;
		m.Output["fullness"] = (buffer is null ? 100f : 100f * buffer.Quantity.Value / buffer.Capacity.Value).ToFix32();
		m.Output["product"] = Fix32.FromRaw(buffer.Product?.SlimId.Value ?? 0);
		return ModuleStatus.Running;
	}

	private bool FarmProductFilter(Module m, ProductProto product) {
		return m.Context.ProtosDb.First<CropProto>(crop => !crop.IsEmptyCrop && crop.ProductProduced.Product.SlimId == product.SlimId).HasValue;
	}

	private void Comparation(ProtoRegistrator registrator) {
		registrator
			.ModuleBuilderStart("Compare_Int_Equal", "Compare: A = B", "A=B")
			.SetDescription("Outputs <b>c</b> = 1 if <b>a</b> equals <b>b</b> (input or <b>field_b</b> constant when enabled), else 0. <b>not_c</b> is the inverse.")
			.AddCategory(Category.Boolean)
			.AddCategory(Category.Arithmetic)
			.AddInput("a", "A")
			.AddInput("b", "B")
			.AddFix32Field("b", "B", overrideInput: true)
			.AddOutput("not_c", "not C")
			.AddOutput("c", "C")
			.Action(m => {
				m.Output.Bool["c"] = m.Input["a", 0] == m.FieldOrInput["b", 0];
				m.Output.Bool["not_c"] = !m.Output.Bool["c"];
			})
			.Display(m => {
				m.Display["c"] = m.Output.Bool["c"] ? "1" : "";
				m.Display["not_c"] = !m.Output.Bool["c"] ? "1" : "";
			})
			.AddDisplay("not_c", "not C", 1, led: true)
			.AddDisplay("c", "C", 1, led: true)
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Compare_Int_Greater", "Compare: A > B", "A>B")
			.SetDescription("Outputs <b>c</b> = 1 if <b>a</b> is strictly greater than <b>b</b> (input or <b>field_b</b> constant when enabled), else 0. <b>not_c</b> is the inverse.")
			.AddCategory(Category.Boolean)
			.AddCategory(Category.Arithmetic)
			.AddInput("a", "A")
			.AddInput("b", "B")
			.AddFix32Field("b", "B", overrideInput: true)
			.AddOutput("not_c", "not C")
			.AddOutput("c", "C")
			.Action(m => {
				m.Output.Bool["c"] = m.Input["a", 0] > m.FieldOrInput["b", 0];
				m.Output.Bool["not_c"] = !m.Output.Bool["c"];
			})
			.Display(m => {
				m.Display["c"] = m.Output.Bool["c"] ? "1" : "";
				m.Display["not_c"] = !m.Output.Bool["c"] ? "1" : "";
			})
			.AddDisplay("not_c", "not C", 1, led: true)
			.AddDisplay("c", "C", 1, led: true)
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Compare_Int_Lower", "Compare: A < B", "A<B")
			.SetDescription("Outputs <b>c</b> = 1 if <b>a</b> is strictly less than <b>b</b> (input or <b>field_b</b> constant when enabled), else 0. <b>not_c</b> is the inverse.")
			.AddCategory(Category.Boolean)
			.AddCategory(Category.Arithmetic)
			.AddInput("a", "A")
			.AddInput("b", "B")
			.AddFix32Field("b", "B", overrideInput: true)
			.AddOutput("not_c", "not C")
			.AddOutput("c", "C")
			.Action(m => {
				m.Output.Bool["c"] = m.Input["a", 0] < m.FieldOrInput["b", 0];
				m.Output.Bool["not_c"] = !m.Output.Bool["c"];
			})
			.Display(m => {
				m.Display["c"] = m.Output.Bool["c"] ? "1" : "";
				m.Display["not_c"] = !m.Output.Bool["c"] ? "1" : "";
			})
			.AddDisplay("not_c", "not C", 1, led: true)
			.AddDisplay("c", "C", 1, led: true)
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Compare_Int_GreaterOrEqual", "Compare: A ≥ B", "A≥B")
			.SetDescription("Outputs <b>c</b> = 1 if <b>a</b> is greater than or equal to <b>b</b> (input or <b>field_b</b> constant when enabled), else 0. <b>not_c</b> is the inverse.")
			.AddCategory(Category.Boolean)
			.AddCategory(Category.Arithmetic)
			.AddInput("a", "A")
			.AddInput("b", "B")
			.AddFix32Field("b", "B", overrideInput: true)
			.AddOutput("not_c", "not C")
			.AddOutput("c", "C")
			.Action(m => {
				m.Output.Bool["c"] = m.Input["a", 0] >= m.FieldOrInput["b", 0];
				m.Output.Bool["not_c"] = !m.Output.Bool["c"];
			})
			.Display(m => {
				m.Display["c"] = m.Output.Bool["c"] ? "1" : "";
				m.Display["not_c"] = !m.Output.Bool["c"] ? "1" : "";
			})
			.AddDisplay("not_c", "not C", 1, led: true)
			.AddDisplay("c", "C", 1, led: true)
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Compare_Int_LowerOrEqual", "Compare: A ≤ B", "A≤B")
			.SetDescription("Outputs <b>c</b> = 1 if <b>a</b> is less than or equal to <b>b</b> (input or <b>field_b</b> constant when enabled), else 0. <b>not_c</b> is the inverse.")
			.AddCategory(Category.Boolean)
			.AddCategory(Category.Arithmetic)
			.AddInput("a", "A")
			.AddInput("b", "B")
			.AddFix32Field("b", "B", overrideInput: true)
			.AddOutput("not_c", "not C")
			.AddOutput("c", "C")
			.Action(m => {
				m.Output.Bool["c"] = m.Input["a", 0] <= m.FieldOrInput["b", 0];
				m.Output.Bool["not_c"] = !m.Output.Bool["c"];
			})
			.Display(m => {
				m.Display["c"] = m.Output.Bool["c"] ? "1" : "";
				m.Display["not_c"] = !m.Output.Bool["c"] ? "1" : "";
			})
			.AddDisplay("not_c", "not C", 1, led: true)
			.AddDisplay("c", "C", 1, led: true)
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Compare_Int_Max", "Maximum: A or B", "MAX")
			.SetDescription("Routes the larger of <b>a</b> and <b>b</b> (input or <b>field_b</b> constant when enabled) to output <b>a</b> (High) and the smaller to output <b>b</b> (Low).")
			.AddCategory(Category.Boolean)
			.AddCategory(Category.Arithmetic)
			.AddCategory(Category.Decision)
			.AddCategory(Category.Control)
			.AddInput("a", "A")
			.AddInput("b", "B")
			.AddFix32Field("b", "B", overrideInput: true)
			.AddOutput("b", "Low")
			.AddOutput("a", "High")
			.Action(m => {
				Fix32 a = m.Input["a", 0];
				Fix32 b = m.FieldOrInput["b", 0];
				if (a > b) {
					m.Output["a"] = a;
					m.Output["b"] = b;
				} else {
					m.Output["a"] = b;
					m.Output["b"] = a;
				}

			})
			.AddControllerDevice()
			.BuildAndAdd();
	}

	private void Display(ProtoRegistrator registrator) {
		// TODO add display
		// display float values
		Action<Module> ModuleFunction(int digits) {
			return (Module m) => {
				Fix32 value = m.Input["a"];
				int floating = Math.Min(m.Field.Integer["float"], digits);
				int inting = Math.Max(Math.Min(digits - floating, digits), 0);

				string full = inting > 0 ? value.IntegerPart.ToString($"D{inting}") : "";
				string fract = floating > 0 ? (value.FractionalPartNonNegative * Math.Pow(10, floating).ToFix32())
										.IntegerPart.ToString($"D{floating}") : "";

				m.Display["a"] = $"{full},{fract}";
			};
		}
		foreach (int i in new int[] { 2, 4, 8, 16 }) {
			registrator
				.ModuleBuilderStart($"Display_Int_{i}", $"Display: {i * 2} digits", $"F-{i}")
				.SetDescription($"Formats input <b>a</b> as a {i * 2}-digit decimal display, splitting integer and fractional parts based on the <b>float</b> field (clamped to {i * 2}).")
				.AddCategory(Category.Display)
				.AddInput("a", "A")
				.AddDisplay("a", "A", i)
				.AddInt32Field("float", "Floating numbers", "Ammount of numbers displayed from fractional part", defaultValue: 0)
				.AddControllerDevice()
				// dynamic
				.Display(ModuleFunction(i * 2))
				.BuildAndAdd();
		}

		registrator
			.ModuleBuilderStart($"Display_Product", $"Display: product", $"F-P")
			.SetDescription("Renders the icon of the product whose slim-id arrives on input <b>a</b> to display <b>a</b>. Used to visually identify the current product in a chain.")
			.AddCategory(Category.Display)
			.AddInput("a", "Product")
			.AddDisplay("a", "Product", 1, image: true)
			.AddControllerDevice()
			.Display(m => m.Display["a"] = m.Input.Product("a")?.IconPath)
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart($"Display_Entity", $"Display: entity", $"F-E")
			.SetDescription("Renders the icon of the entity prototype referenced by input <b>a</b> (machine, vehicle, etc.) to display <b>a</b>. Useful for identifying entity types visually.")
			.AddCategory(Category.Display)
			.AddInput("a", "Entity")
			.AddDisplay("a", "Entity", 1, image: true)
			.AddControllerDevice()
			.Display(m => m.Display["a"] = m.Input.EntityProtoIconified("a")?.IconPath)
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart($"Display_Bool", $"Display: LED", $"F-B")
			.SetDescription("Lights an LED on display <b>a</b> when input <b>a</b> is greater than 0; otherwise the LED is off. Treats any positive numeric value as truthy.")
			.AddCategory(Category.Display)
			.AddInput("a", "Product")
			.AddDisplay("a", "Product", 1, led: true)
			.AddControllerDevice()
			.Display(m => m.Display["a"] = m.Input["a", 0] > 0 ? "1" : "")
			.BuildAndAdd();
	}

	private void RadioFM(ProtoRegistrator registrator) {
		Action<Module> DisplayReceiveSignals(int digits) {
			return (Module m) => {
				int signal = (m.Output["signal"] * 5).ToIntRounded().Min(5).Max(0);
				m.Display["signal"] = new string('|', signal) + new string('.', 5 - signal);

				int value = m.Field.Integer["fm"];
				Fix32 displayValue = (171 + value).ToFix32() * 0.5f.ToFix32();
				m.Display["fm"] = displayValue.ToStringRounded(1) + (digits > 4 ? " MHz" : "");
			};
		}

		Action<Module> ReadSignals(int digits) {
			return (Module m) => {
				FMManager fmManager = GlobalDependencyResolver.Get<FMManager>();

				bool logging = m.Field.Bool["logging"];
				if (logging) {
					m.Field.Bool["logging"] = false;
				}

				(Fix32 strenght, FMDataBandChannel signals) = fmManager.Signal(m.Controller.Position3f.Tile3i, m.Field.Integer["fm"], logging);
				if (strenght == 0 || signals?.Value == null)
				// TODO generate noise or read data
				{
					m.Output["signal"] = 0;
					for (int i = 0; i < digits; i++) {
						m.Output[NAMES[i]] = 0;
					}
				} else {
					m.Output["signal"] = strenght;
					m.Display["id3"] = signals.Id3 ?? "N/A";
					int minCount = Math.Min(signals.Count, digits);
					for (int i = 0; i < minCount; i++) {
						m.Output[NAMES[i]] = signals.Value[i];
					}
					for (int i = minCount; i < digits; i++) {
						m.Output[NAMES[i]] = 0;
					}
				}
			};
		}
		foreach (int i in new int[] { 2, 4, 8, 16 }) {
			var module = registrator
				.ModuleBuilderStart($"Radio_In_FM_{i}", $"FM receiver ({i} signals)", $"FM-R")
				.SetDescription($"Listens on the FM channel from the <b>fm</b> field and emits up to {i} signals on outputs <b>a</b>..<b>{NAMES[i - 1]}</b>, plus reception strength on <b>signal</b> and ID3 channel name on display <b>id3</b>. Outputs zero when out of range.")
				.AddCategory(Category.Antene)
				.AddCategory(Category.AnteneFM)
				.AddCustomField("fm", "FM", "Listening frequency",
					(inspector, settings, refresh, reference) => settings.Add(new Ui.DataBand.FMDataBandChannelView(inspector, refresh, reference))
				)
				.AddCustomField("id3", "ID3 (name)", "ID3 metadata for the channel",
					(inspector, settings, module, refresh, reference)
						=> settings.AddAndReturn(new Display())
							.Fill()
							.TextLeftMiddle()
							.ObserveValue(() => module.Display["id3", "N/A"].AsLoc()))
				.AddControllerDevice()
				// dynamic
				.Width(i)
				.Action(ReadSignals(i))
				.Display(DisplayReceiveSignals(i));

			if (i == 2) {
				module.AddDisplay("signal", "Signal", 0.5.ToFix32());
				module.AddDisplay("fm", "Frequency", i - 0.5.ToFix32());
			} else {
				module.AddDisplay("signal", "Signal", 1);
				module.AddDisplay("fm", "Frequency", i - 1);
			}

			for (int j = 0; j < i; j++) {
				module.AddOutput(NAMES[j], NAMES[j].ToUpper());
			}

			module.AddBooleanField("logging", "Enable debug log of antene");
			module.BuildAndAdd();
		}

		Action<Module> DisplayBroadcastSignals(int digits) {
			return (Module m) => {
				Antena entity = m.Field.Entity<Antena>("antena");
				if (entity?.DataBand is FMDataBand fm) {
					if (!entity.IsEnabled) {
						m.Display["fm"] = "OFF";
						return;
					}

					int value = m.Field.Integer["fm"];
					Fix32 displayValue = (171 + value).ToFix32() * 0.5f.ToFix32();
					m.Display["fm"] = displayValue.ToStringRounded(1) + (digits > 4 ? " MHz" : "");
				} else {
					m.Display["fm"] = "NOA";
				}
			};
		}

		Action<Module> WriteSignals(int digits) {
			return (Module m) => {
				Antena entity = m.Field.Entity<Antena>("antena");
				if (entity.DataBand is FMDataBand fm) {
					if (!entity.IsEnabled) {
						return;
					}

					if (!entity.IsPaused) {
						bool logging = m.Field.Bool["logging"];
						if (logging) {
							m.Field.Bool["logging"] = false;
						}

						Fix32[] scratch = SignalBufferPool.Rent();
						try {
							for (int i = 0; i < digits; i++) {
								scratch[i] = m.Input[NAMES[i], 0];
							}
							int channel = m.Field.Integer["fm"];
							fm.Update(channel, scratch, digits, logging);
							fm.Id3(channel, m.Field["id3", string.Empty]);
						} finally {
							SignalBufferPool.Return(scratch);
						}
					}
				} else {
					m.SetError("No antena connected");
				}
			};
		}
		foreach (int i in new int[] { 2, 4, 8, 16 }) {
			var module = registrator
				.ModuleBuilderStart($"Radio_Out_FM_{i}", $"FM broadcaster ({i} signals)", $"FM-B")
				.SetDescription($"Broadcasts up to {i} input signals (<b>a</b>..<b>{NAMES[i - 1]}</b>) on the FM channel from the <b>fm</b> field via the linked <b>antena</b>, tagging the stream with the <b>id3</b> field. Errors if no antena is connected.")
				.AddCategory(Category.Antene)
				.AddCategory(Category.AnteneFM)
				.AddCustomField("fm", "FM", "Broadcasting frequency",
					(inspector, settings, refresh, reference) => settings.Add(new Ui.DataBand.FMDataBandChannelView(inspector, refresh, reference))
				)
				.AddStringField("id3", "ID3 (name)", defaultValue: null)
				.AddEntityField<Antena>("antena", "Antena")
				.AddDisplay("fm", "Frequency", i)
				.AddControllerDevice()
				// dynamic
				.Width(i)
				.Action(WriteSignals(i))
				.Display(DisplayBroadcastSignals(i));

			for (int j = 0; j < i; j++) {
				module.AddInput(NAMES[j], NAMES[j].ToUpper());
			}

			module.AddBooleanField("logging", "Enable debug log of antene");
			module.BuildAndAdd();
		}
	}

	private void RadioAM(ProtoRegistrator registrator) {
		registrator
			.ModuleBuilderStart($"Radio_In_AM", $"AM receiver", $"AM-R")
			.SetDescription("Reads the AM channel selected in the <b>am</b> field via the linked <b>antena</b> and outputs the received value on <b>am</b>. Errors and outputs 0 if no antena is connected.")
			.AddCategory(Category.Antene)
			.AddCategory(Category.AnteneAM)
			.AddCustomField("am", "AM", "Listening frequency", (inspector, settings, refresh, reference) => settings.Add(new Ui.DataBand.AMDataBandChannelView(inspector, refresh, reference)))
			.AddEntityField<Antena>("antena", "Antena")
			.AddDisplay("am", "Frequency", 2)
			.AddOutput("am", "AM signal")
			.AddControllerDevice()
			.Width(2)
			// dynamic
			.Action((Module m) => {
				Antena entity = m.Field.Entity<Antena>("antena");
				if ((entity?.DataBand is AMDataBand am))
				// TODO generate noise or read data
				{
					m.Output["am"] = am.Read(m.Field["am", Fix32.Zero].IntegerPart, Fix32.Zero);
				} else {
					m.SetError("No antena connected");
					m.Output["am"] = Fix32.Zero;
				}
			})
			.Display((Module m) => {
				Antena entity = m.Field.Entity<Antena>("antena");
				if ((entity?.DataBand is AMDataBand am))
				// TODO generate noise or read data
				{
					// signal value
					int value = m.Field.Integer["am"];
					Fix32 displayValue = (53 + value).ToFix32() * 10f.ToFix32();
					m.Display["am"] = displayValue.ToStringRounded(0);
				} else {
					m.Display["am"] = "NOA";
				}
			})
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart($"Radio_Out_AM", $"AM broadcaster", $"AM-B")
			.SetDescription("Broadcasts input <b>am</b> on the AM channel selected in the <b>am</b> field via the linked <b>antena</b>. Skips while the antena is disabled or paused; errors if no antena is connected.")
			.AddCategory(Category.Antene)
			.AddCategory(Category.AnteneAM)
			.AddCustomField("am", "AM", "Listening frequency", (inspector, settings, refresh, reference) => settings.Add(new Ui.DataBand.AMDataBandChannelView(inspector, refresh, reference)))
			.AddEntityField<Antena>("antena", "Antena")
			.AddDisplay("am", "Frequency", 2)
			.AddInput("am", "AM signal")
			.AddControllerDevice()
			.Width(2)
			// dynamic
			.Action((Module m) => {
				Antena entity = m.Field.Entity<Antena>("antena");
				if (entity?.DataBand is AMDataBand am) {
					if (!entity.IsEnabled) {
						return;
					}

					if (!entity.IsPaused) {
						am.Update(m.Field.Integer["am"], m.Input["am", Fix32.Zero]);
					}
				} else {
					m.SetError("No antena connected");
					m.Output["am"] = Fix32.Zero;
				}
			})
			.Display((Module m) => {
				Antena entity = m.Field.Entity<Antena>("antena");
				if (entity?.DataBand is AMDataBand am) {
					if (!entity.IsEnabled) {
						m.Display["am"] = "OFF";
						return;
					}

					int value = m.Field.Integer["am"];
					Fix32 displayValue = (53 + value).ToFix32() * 10.ToFix32();
					m.Display["am"] = displayValue.ToStringRounded(0);
				} else {
					m.Display["am"] = "NOA";
				}
			})
			.BuildAndAdd();
	}

	private void Variables(ProtoRegistrator registrator) {
		registrator
			.ModuleBuilderStart("VariableNetwork_Read", "Network Variable (read)", "*N")
			.SetDescription("Reads the global network variable identified by the <b>name</b> field via VariableManager and outputs its current value on <b>value</b>. Errors if <b>name</b> is empty.")
			.AddCategory(Category.Control)
			.AddCategory(Category.Arithmetic)
			.UnlockedBy(Ids.Research.Datacenter)
			.UseComputation(0.1.Quantity())
			.AddOutput("value", "Value")
			.AddDisplay("name", "Variable name (should be longer)", 1)
			.AddStringField("name", "Variable name", defaultValue: "")
			.AddCustomField("variables", "Variables", (inspector, container, refresh, reference) => {
				container.Add(
					new ButtonText("Variables".ToDoLoc())
					.OnClick(() => inspector.VariableWindowController.ActivateSelf())
				);
			})
			.Width(1)
			.Action(m => {
				string name = m.Field["name", ""];
				m.Display["name"] = name.Substring(0, Math.Min(name.Length, 2));

				if (name.IsNullOrEmpty()) {
					m.SetError("Invalid variable name");
					return ModuleStatus.Error;
				}

				m.Output["value"] = GlobalDependencyResolver
					.Get<VariableManager>()
					.GetVariable(name);
				return ModuleStatus.Running;
			})
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("VariableNetwork_Write", "Network Variable (write)", "*N")
			.SetDescription("Writes input <b>value</b> (or the constant <b>value</b> field when <b>field_value</b> is on) into the global network variable identified by the <b>name</b> field via VariableManager. Errors if <b>name</b> is empty.")
			.AddCategory(Category.Control)
			.AddCategory(Category.Arithmetic)
			.UseComputation(0.1.Quantity())
			.UnlockedBy(Ids.Research.Datacenter)
			.AddDisplay("name", "Variable name (should be longer)", 1)
			.AddStringField("name", "Variable name", defaultValue: "")
			.AddInput("value", "Value")
			.AddFix32Field("value", "Value", overrideInput: true)
			.AddCustomField("variables", "Variables", (inspector, container, refresh, reference) => {
				container.Add(
					new ButtonText("Variables".ToDoLoc())
						.OnClick(() => inspector.VariableWindowController.ActivateSelf())
				);
			})
			.Width(1)
			.Action(m => {
				string name = m.Field["name", ""];
				m.Display["name"] = name.Substring(0, Math.Min(name.Length, 2));

				if (name.IsNullOrEmpty()) {
					m.SetError("Invalid variable name");
					return ModuleStatus.Error;
				}

				GlobalDependencyResolver
					.Get<VariableManager>()
					.SetVariable(name, m.FieldOrInput["value", Fix32.Zero]);
				return ModuleStatus.Running;
			})
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Variable_Read", "Variable (read)", "*C")
			.SetDescription("Reads a controller-local variable: searches the same Controller for a <b>Variable_Write</b> module with a matching <b>name</b> field and outputs its <b>value</b> on <b>value</b>. Outputs 0 when none is found; errors if <b>name</b> is empty.")
			.AddCategory(Category.Control)
			.AddCategory(Category.Arithmetic)
			.AddOutput("value", "Value")
			.AddDisplay("name", "Variable name (should be longer)", 1)
			.AddStringField("name", "Variable name", defaultValue: "")
			.Width(1)
			.Action(m => {
				string name = m.Field["name", ""];
				m.Display["name"] = name.Substring(0, Math.Min(name.Length, 2));

				if (name.IsNullOrEmpty()) {
					m.SetError("Invalid variable name");
					return ModuleStatus.Error;
				}

				Controller controller = m.Controller;
				string moduleType = "Variable_Write".ModuleId();

				Module targetModule = controller.Modules.AsEnumerable()
					.FirstOrDefault(mod => mod.Prototype.Id.Value == moduleType
						&& mod.Field["name", ""] == name);
				if (targetModule != null) {
					m.Output["value"] = targetModule.FieldOrInput["value"];
					return ModuleStatus.Running;
				}

				m.Output["value"] = 0;
				return ModuleStatus.Running;
			})
			.AddControllerDevice()
			.BuildAndAdd();

		registrator
			.ModuleBuilderStart("Variable_Write", "Variable (write)", "*C")
			.SetDescription("Holds a controller-local variable named by the <b>name</b> field. Stores input <b>value</b> (or the constant <b>value</b> field when <b>field_value</b> is on) so a paired <b>Variable_Read</b> on the same controller can fetch it. Errors if <b>name</b> is empty.")
			.AddCategory(Category.Control)
			.AddCategory(Category.Arithmetic)
			.AddDisplay("name", "Variable name (should be longer)", 1)
			.AddStringField("name", "Variable name", defaultValue: "")
			.AddInput("value", "Value")
			.AddFix32Field("value", "Value", overrideInput: true)
			.Width(1)
			.Action(m => {
				string name = m.Field["name", ""];
				m.Display["name"] = name.Substring(0, Math.Min(name.Length, 2));

				if (name.IsNullOrEmpty()) {
					m.SetError("Invalid variable name");
					return ModuleStatus.Error;
				}
				return ModuleStatus.Running;
			})
			.AddControllerDevice()
			.BuildAndAdd();
	}
}
