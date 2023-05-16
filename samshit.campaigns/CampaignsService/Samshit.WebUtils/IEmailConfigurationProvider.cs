namespace Samshit.WebUtils
{
    /// <summary>
    /// Provides a way to get Email Service configuration
    /// </summary>
    public interface IEmailConfigurationProvider
    {
        /// <summary>
        /// Retrieves the Email Service Configutaion based on external parameters came from outside the app
        /// </summary>
        /// <returns></returns>
        public EmailServiceConfiguration GetConfiguration();
    }
}
