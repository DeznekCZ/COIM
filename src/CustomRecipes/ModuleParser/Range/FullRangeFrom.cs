using System.Collections.Generic;
using System.Threading.Tasks;

namespace CustomAssets.Python
{
    internal class FullRangeFrom : IRange
    {
        private IExpression start;
        private IExpression step;

        public FullRangeFrom(IExpression start, IExpression step = null)
        {
            this.start = start;
            this.step = step ?? new NumberConstant(1);
        }

        public string Path => throw new System.NotImplementedException();

        public Reference<object> GetReference(IDictionary<string, object> context)
        {
            throw new System.NotImplementedException();
        }
		public Task<Reference<object>> GetReferenceAsync(IDictionary<string, object> context) {
			throw new System.NotImplementedException();
		}

		public object GetValue(IDictionary<string, object> context)
        {
            throw new System.NotImplementedException();
        }
		public Task<object> GetValueAsync(IDictionary<string, object> context) {
			throw new System.NotImplementedException();
		}
	}
}