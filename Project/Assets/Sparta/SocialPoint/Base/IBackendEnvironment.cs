using System;

namespace SocialPoint.Base
{
    public enum EnvironmentType
    {
        Production,
        PreProduction,
        QA,
        Localization,
        Development
    }

    [Serializable]
    public struct Environment
    {
        public string Name;
        public string Url;
        public EnvironmentType Type;
    }

    public interface IBackendEnvironmentStorage
    {
        /// <summary>
        /// Gets the default environment for the current configuration
        /// </summary>
        /// <value>The name of the default environment</value>
        string Default { get; }

        /// <summary>
        /// Gets or sets the selected environment name.
        /// </summary>
        /// <value>The selected environment name. Null if there is no selected environment.</value>
        string Selected { get; set; }
    }

    public interface IBackendEnvironment
    {
        /// <summary>
        /// List of available environments
        /// </summary>
        /// <value>Available environments.</value>
        Environment[] Environments { get; }

        /// <summary>
        /// Backend Enviroment Storage
        /// </summary>
        /// <value>Environment storage.</value>
        IBackendEnvironmentStorage Storage { get; }

        /// <summary>
        /// Gets the Current Environment URL.
        /// </summary>
        /// <returns>The URL.</returns>
        string GetUrl();

        /// <summary>
        /// Gets the URL of a defined Environment
        /// </summary>
        /// <returns>The URL. Null if the environment does not exist</returns>
        /// <param name="name">Environment Name</param>
        string GetUrl(string name);

        /// <summary>
        /// Gets the current environment data.
        /// </summary>
        /// <returns>The environment.</returns>
        Environment GetEnvironment();

        /// <summary>
        /// Gets an environment.
        /// </summary>
        /// <returns>The environment. An empty optional value if the environment does not exist.</returns>
        /// <param name="name">Environment Name</param>
        Environment? GetEnvironment(string name);
    }
}
