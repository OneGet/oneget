namespace Microsoft.OneGet.Providers {
    using System;
    using Utility.Plugin;

    public interface IProvider {
        #region declare Provider-interface
        /* Synced/Generated code =================================================== */

        /// <summary>
        ///     Allows the Provider to do one-time initialization.
        ///     This is called after the Provider is instantiated .
        /// </summary>
        /// <param name="dynamicInterface">A reference to the DynamicInterface class -- used to implement late-binding</param>
        /// <param name="requestImpl">Object implementing some or all IRequest methods</param>
        [Required]
        void InitializeProvider(object dynamicInterface, Object requestImpl);

        /// <summary>
        ///     Gets the features advertized from the provider
        /// </summary>
        /// <param name="requestImpl"></param>
        void GetFeatures(Object requestImpl);

        /// <summary>
        ///     Gets dynamically defined options from the provider
        /// </summary>
        /// <param name="category"></param>
        /// <param name="requestImpl"></param>
        void GetDynamicOptions(string category, Object requestImpl);

        /// <summary>
        ///     Allows runtime examination of the implementing class to check if a given method is implemented.
        /// </summary>
        /// <param name="methodName"></param>
        /// <returns></returns>
        bool IsMethodImplemented(string methodName);


        /// <summary>
        /// Returns the version of the provider.
        /// 
        /// This is expected to be in multipart numeric format. 
        /// </summary>
        /// <returns>The version of the provider</returns>
        string GetProviderVersion();

        #endregion
    }
}