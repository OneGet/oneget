namespace Microsoft.OneGet.Providers {
    using System;
    using Utility.Plugin;
    using IRequestObject = System.Object;

    public interface IProvider {
        #region declare Provider-interface
        /* Synced/Generated code =================================================== */

        /// <summary>
        ///     Allows the Provider to do one-time initialization.
        ///     This is called after the Provider is instantiated .
        /// </summary>
        /// <param name="requestObject">Object implementing some or all IRequest methods</param>
        [Required]
        void InitializeProvider(IRequestObject requestObject);

        /// <summary>
        ///     Gets the features advertized from the provider
        /// </summary>
        /// <param name="requestObject"></param>
        void GetFeatures(IRequestObject requestObject);

        /// <summary>
        ///     Gets dynamically defined options from the provider
        /// </summary>
        /// <param name="category"></param>
        /// <param name="requestObject"></param>
        void GetDynamicOptions(string category, IRequestObject requestObject);

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