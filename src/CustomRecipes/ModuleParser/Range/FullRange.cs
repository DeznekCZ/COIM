using System.Collections.Generic;
using System.Threading.Tasks;

namespace CustomAssets.Python
{
    public class FullRange : IRange
    {
        private IExpression step;

        public FullRange(IExpression step = null)
        {
            this.step = step ?? new NumberConstant(0);
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