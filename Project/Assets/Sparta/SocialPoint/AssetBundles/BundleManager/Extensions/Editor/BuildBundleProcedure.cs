namespace BM.Extensions
{
    /// <summary>
    /// Base build bundle procedure class. Should not be derived from as it will cause the procedure to be called at every defined step.
    /// </summary>
    public interface BuildBundleProcedure
    {
        void run();
    }

    /// <summary>
    /// Procedures derived from this class will be called after all bundles have been built.
    /// </summary>
    public interface BBPPostBuild : BuildBundleProcedure
    {   
    }

    /// <summary>
    /// Procedures derived from this class will be called when a scene has been opened prior to the scene serialization(and prior the the bundle build step).
    /// Won't be executed if the project doesn't have serialization enabled.
    /// </summary>
    public interface BBPPreSceneSerialization : BuildBundleProcedure
    {   
    }
}
