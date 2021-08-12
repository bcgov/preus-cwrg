using Autofac;
using CJG.Application.Services;
using CJG.Application.Services.Web;
using CJG.Core.Interfaces.Service;
using CJG.Infrastructure.BCeID.WebService;
using CJG.Infrastructure.EF;
using CJG.Infrastructure.Entities;
using CJG.Infrastructure.ReportingService.Properties;
using NLog;
using System.Web;

namespace CJG.Infrastructure.ReportingService
{
	internal class AppFactory
	{
		public AppFactory()
		{
			_logger = LogManager.GetCurrentClassLogger();
			Container = CreateContainer();
		}

		private IContainer CreateContainer()
		{
			var builder = new ContainerBuilder();

			builder.RegisterInstance(_logger).As<ILogger>();
			builder.RegisterInstance(new InternalHttpContext(new HttpContextWrapper(new HttpContext(new HttpRequest("", Settings.Default.SiteUrl, null), new HttpResponse(null))))).As<HttpContextBase>();

			builder.RegisterType<SdsiReportJob>().As<ISdsiReportJob>();
			builder.RegisterType<SinReportJob>().As<ISinReportJob>();
			builder.RegisterType<DuplicateSinReportJob>().As<IDuplicateSinReportJob>();
			builder.RegisterType<ExitSurveyReportJob>().As<IExitSurveyReportJob>();
			builder.RegisterType<WithdrawalSurveyReportJob>().As<IWithdrawalSurveyReportJob>();

			builder.RegisterType<ParticipantService>().As<IParticipantService>();
			builder.RegisterType<SurveyService>().As<ISurveyService>();
			builder.RegisterType<GrantApplicationService>().As<IGrantApplicationService>();
			builder.RegisterType<EligibleCostService>().As<IEligibleCostService>();
			builder.RegisterType<SiteMinderService>().As<ISiteMinderService>();
			builder.RegisterType<OrganizationService>().As<IOrganizationService>();
			builder.RegisterType<FiscalYearService>().As<IFiscalYearService>();
			builder.RegisterType<UserService>().As<IUserService>();
			builder.RegisterType<BCeIDService>().As<IBCeIDService>();
			builder.RegisterType<DataContext>().As<IDataContext>().InstancePerLifetimeScope();

			return builder.Build();
		}

		private readonly ILogger _logger;

		public IContainer Container { get; }
		
		public ILogger GetLogger()
		{
			return _logger;
		}
		
		public ISdsiReportJob GetSection25ReportJob()
		{
			return Container.Resolve<ISdsiReportJob>();
		}

		public ISinReportJob GetTemporaryResidentReportJob()
		{
			return Container.Resolve<ISinReportJob>();
		}

		public IDuplicateSinReportJob GetDuplicateSinReportJob()
		{
			return Container.Resolve<IDuplicateSinReportJob>();
		}

		public IExitSurveyReportJob GetExitSurveyReportJob()
		{
			return Container.Resolve<IExitSurveyReportJob>();
		}

		public IWithdrawalSurveyReportJob GetWithdrawalSurveyReportJob()
		{
			return Container.Resolve<IWithdrawalSurveyReportJob>();
		}
	}
}