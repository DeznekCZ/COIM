using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CustomAssets.ModuleParser.Registrator;

namespace CustomAssets.Python
{
	public class Constructor : IExpression
    {
        private FunctionCall value;
        private AsyncFunctionCall asyncValue;

		public static Constructor AsynConstructor(string[] argumentNames, AsyncFunctionCall asyncCall) {
			return new Constructor(argumentNames, asyncCall);
		}

        public Constructor(string[] argumentNames, FunctionCall value)
        {
            this.value = value;
            Arguments = argumentNames ?? [];
        }

        private Constructor(string[] argumentNames, AsyncFunctionCall asyncCall)
        {
			this.asyncValue = asyncCall;
            Arguments = argumentNames ?? [];
        }

        public Constructor(FunctionCall value)
        {
            this.value = value;
            Arguments = [];
        }

		public class CallArguments(NamedValue[] args) {
            
			public AnyArgument<T> GetArgument<T>(string argumentName) {
				IArgumentValue arg = args.FirstOrDefault(a => a.Name == argumentName);
				if (arg is null) {
					return new AnyArgument<T>(argumentName, null, empty: true);
				} else {
					return new AnyArgument<T>(argumentName, arg.Value);
				}
			}

            public NamedValue this[int index] => args[index];

            public NamedValue this[string argumentName] => args.FirstOrDefault(a => a.Name == argumentName);

			public object[] Values() {
				return [.. args.Select(a => a.Value)];
			}

			public AnyArgument<T> GetNumberArgument<T>(string order) {
                return this.GetArgument<T>(order)
					.When<object>(any => (T)Convert.ChangeType(any, typeof(T)));
			}
		}

		public delegate object FunctionCall(CallArguments args);
		public delegate Task<object> AsyncFunctionCall(CallArguments args);

        public string[] Arguments { get; private set; }

        public string Path => throw new NotImplementedException();

        public Reference<object> GetReference(IDictionary<string, object> context)
        {
            throw new InvalidCastException("Cannot be referenced");
        }

        public Task<Reference<object>> GetReferenceAsync(IDictionary<string, object> context)
        {
            throw new InvalidCastException("Cannot be referenced");
        }

        public object GetValue(IDictionary<string, object> context)
        {
            throw new NotImplementedException();
        }

        public Task<object> GetValueAsync(IDictionary<string, object> context)
        {
            throw new NotImplementedException();
        }

        public object Invoke(NamedValue[] args)
        {
            try
            {
				if (value is null) {
					throw new InvalidAsynchronousStateException("Cannot call asynchronous script in synchronous execution");
				}

                return value.Invoke(new CallArguments(args));
            }
            catch (ReturnException r)
            {
                return r.Value;
            }
        }

        public async Task<object> InvokeAsync(NamedValue[] args)
        {
            try
            {
				if (value is not null) {
					return value.Invoke(new CallArguments(args));
				}

                return await asyncValue.Invoke(new CallArguments(args));
            }
            catch (ReturnException r)
            {
                return r.Value;
            }
        }
    }
}