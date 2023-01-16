using DataAnnotationsExtensions.ClientValidation;
using FCIWeb.App_Start;

[assembly: WebActivator.PreApplicationStartMethod(typeof(RegisterClientValidationExtensions), "Start")]
 
namespace FCIWeb.App_Start {
    public static class RegisterClientValidationExtensions {
        public static void Start() {
            DataAnnotationsModelValidatorProviderExtensions.RegisterValidationExtensions();            
        }
    }
}