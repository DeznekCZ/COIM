using Mafi;

namespace ProgramableNetwork
{
    [GlobalDependency(RegistrationMode.AsSelf, false, false)]
    internal class GlobalDependencyResolver
    {
        private static GlobalDependencyResolver Instance { get; set; }

        private DependencyResolver m_dependencyResolver;

        public GlobalDependencyResolver(DependencyResolver dependencyResolver)
        {
            Instance = this;
            m_dependencyResolver = dependencyResolver;
        }

        public static T Get<T>() {
            return Instance.m_dependencyResolver.Resolve<T>();
        }

        public static T Instantiate<T>() {
            return Instance.m_dependencyResolver.Instantiate<T>();
        }
    }
}
