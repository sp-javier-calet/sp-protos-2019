namespace SocialPoint.Utils
{
    public interface IGenericFactory<M, C>
    {
        bool SupportsModel(M model);

        C Create(M model);
    }

    public static class GenericFactoryExtensions
    {
        public static C[] CreateMultiple<M, C>(this IGenericFactory<M, C> factory, M[] models)
        {
            var results = new C[models.Length];
            for(int i = 0; i < models.Length; ++i)
            {
                results[i] = factory.Create(models[i]);
            }
            return results;
        }
    }
}