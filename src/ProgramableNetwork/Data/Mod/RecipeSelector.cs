using Mafi;
using Mafi.Collections;
using Mafi.Core;
using Mafi.Core.Factory.Machines;
using Mafi.Core.Factory.Recipes;
using Mafi.Core.Products;
using Mafi.Core.Syncers;
using Mafi.Unity.Ui.Library;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;

namespace ProgramableNetwork.Ui
{
    public class RecipeSelector : Panel
    {
        private RecipePicker picker;

        public RecipeSelector(ControllerInspector inspector, Module module, Action refresh, Reference reference)
        {
            Action onClick = () =>
            {
                picker = new RecipePicker(inspector.Context,
                    assignedRecipesFn: () => module.Field.Entity<Machine>("entity")?.RecipesAssigned.AsEnumerable() ?? new Lyst<RecipeProto>(),
                    allRecipesFn: () => module.Field.Entity<Machine>("entity")?.Prototype.Recipes.AsEnumerable() ?? new Lyst<RecipeProto>(),
                    onRecipeAdded: (r) => {
                        module.Field["recipe", false] = r.Id.Value;
                        picker.Close();
                    },
                    onRecipeRemoved: (r) => {
                        module.Field["recipe", false] = null;
                        picker.Close();
                    }
                );
                picker.OpenIn(this.TryGetClosestParent(c => c is IWindowHost, out UiComponent host) ? host as IWindowHost : inspector);
            };

            Action<Machine, string> refreshRecipe = (machine, recipeId) =>
            {
                Body.Clear();

                if (recipeId.IsNullOrEmpty())
                {
                    Body.AddAndReturn(new ButtonText(Tr.Recipes).OnClick(onClick));
                    return;
                }

                if (machine is null)
                {
                    Body.AddAndReturn(new ButtonText(Tr.EntityStatus__MissingInput));
                    return;
                }

                RecipeProto recipeProto = module.Context.ProtosDb
                    .Get<RecipeProto>(new Mafi.Core.Prototypes.Proto.ID(recipeId))
                    .ValueOrNull;

                if (recipeProto == null)
                {
                    module.Field["recipe", false] = null;
                    return;
                }

                Body.AddAndReturn(new MachineRecipeUi(() => new NoExecutor(), recipeProto).OnClick(onClick));
            };

            refreshRecipe(module.Field.Entity<Machine>("entity"), module.Field["recipe", false]);

            Body.Add(new RecipeUi().OnClick(onClick));
            this.Observe(() => module.Field.Entity<Machine>("entity"))
                .Observe(() => module.Field["recipe", (string)null])
                .Do(refreshRecipe);
        }

        private class NoExecutor : IRecipeExecutorForUi
        {
            public bool IsBoosted => false;

            public bool WorkedThisTick => false;

            public Percent DurationMultiplier => Percent.One;

            public Quantity GetInputCapacityFor(ProductProto product)
            {
                return Quantity.Zero;
            }

            public Quantity GetInputQuantityFor(ProductProto product)
            {
                return Quantity.Zero;
            }

            public Quantity GetOutputCapacityFor(ProductProto product)
            {
                return Quantity.Zero;
            }

            public Quantity GetOutputQuantityFor(ProductProto product)
            {
                return Quantity.Zero;
            }

            public Duration GetTargetDurationFor(IRecipeForUi recipe)
            {
                return recipe.Duration;
            }

            public void GetUnusedBuffersToClear(Lyst<ProductQuantity> result)
            {
                // nothing to do
            }

            public Percent ProgressOnRecipe(IRecipeForUi recipe)
            {
                return Percent.Zero;
            }
        }
    }
}