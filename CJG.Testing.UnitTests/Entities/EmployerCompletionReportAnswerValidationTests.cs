using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using CJG.Testing.Core;
using CJG.Core.Entities;
using CJG.Application.Services;

namespace CJG.Testing.UnitTests.Entities
{
    [TestClass]
    public class EmployerCompletionReportAnswerValidationTests
    {
        [TestMethod, TestCategory("EmployerCompletionReportAnswer"), TestCategory("Validate")]
        public void Validate_When_EmployerCompletionReportAnswer_Missing_Answer()
        {
            var user = EntityHelper.CreateExternalUser();
            var helper = new ServiceHelper(typeof(EligibleExpenseBreakdownService), user);
            helper.MockContext();

            var service = helper.Create<EligibleExpenseBreakdownService>();

			var employerCompletionReportAnswer = new EmployerCompletionReportAnswer();
            helper.MockDbSet(new[] { employerCompletionReportAnswer });

            var validationResults = service.Validate(employerCompletionReportAnswer).ToArray();
            Assert.IsTrue(validationResults.Any(x => x.ErrorMessage == "An answer must be entered."));
        }
    }
}