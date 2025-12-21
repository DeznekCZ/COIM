using System.Collections.Generic;
using System.Threading.Tasks;

namespace CustomAssets.Python
{
    internal class SingleItem : SyncExpression, IRange
    {
        private IExpression index;

        public SingleItem(IExpression index)
        {
            this.index = index;
        }

        public override string Path => $"[{ index.Path }]";

        public override Reference<object> GetReference(IDictionary<string, object> context)
        {
            throw new System.NotImplementedException();
        }

		public override object GetValue(IDictionary<string, object> context)
        {
            return index.GetValue(context);
        }
	}

	public abstract class SyncExpression : IExpression {
		public abstract string Path { get; }

		public abstract Reference<object> GetReference(IDictionary<string, object> context);

		public Task<Reference<object>> GetReferenceAsync(IDictionary<string, object> context) {
			throw new System.NotImplementedException();
		}

		public abstract object GetValue(IDictionary<string, object> context);

		public Task<object> GetValueAsync(IDictionary<string, object> context) {
			return Task.FromResult(GetValue(context));
		}
	}
}