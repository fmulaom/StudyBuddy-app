using StudyBuddy.Web.Services.Interfaces;
using StudyBuddy.Web.Services.Repositories;
using StudyBuddy.Web.Services.ServicesImplementation;

namespace StudyBuddy.Web.Extensions
{
    /// <summary>
    /// SOLID - S: Ova klasa ima samo jednu odgovornost - registracija servisa.
    /// SOLID - O: Svi servisi su na jednom mjestu, lako se dodaju novi bez mijenjanja Program.cs.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registrira sve servise za modul "Učenje i podudaranje".
        /// 
        /// SOLID - DIP: Metoda je apstrakcija - Program.cs ne trebam znati što se registrira.
        /// SOLID - ISP: Modul je izoiran - spreman za integraciju u bilo koju aplikaciju.
        /// </summary>
        public static IServiceCollection AddStudyPartnerServices(this IServiceCollection services)
        {
            // SOLID - S: Registracija repozitorija (generički - jedan za sve entitete)
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IPartnerMatchingService, PartnerMatchingService>();
            services.AddScoped<IStudyGroupService, StudyGroupService>();
            services.AddScoped<IStudyScheduleService, StudyScheduleService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IResourceService, ResourceService>();

            return services;
        }

        /// <summary>
        /// Dodatna metoda - ako trebam konfigurirati samo dio servisa.
        /// SOLID - ISP: Fleksibilnost - mogu koristiti samo što trebam.
        /// </summary>
        public static IServiceCollection AddStudyGroupServicesOnly(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IStudyGroupService, StudyGroupService>();
            return services;
        }

        /// <summary>
        /// Metoda za registraciju samo servisa za raspored i obavijesti.
        /// SOLID - ISP: Modularna registracija.
        /// </summary>
        public static IServiceCollection AddSchedulingServices(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IStudyScheduleService, StudyScheduleService>();
            services.AddScoped<INotificationService, NotificationService>();
            return services;
        }
    }
}
