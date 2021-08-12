using System.Configuration;
using CJG.Web.External.Helpers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CJG.Testing.UnitTests.Helpers
{
	[TestClass]
	public class CanadaPostConfigurationTests
	{
		private const string TestKey = "1234-abcd-5678-efgh";

		[TestCleanup]
		public void ResetKey()
		{
			ConfigurationManager.AppSettings["CanadaPostAutoCompleteApiKey"] = null;
		}

		[TestMethod, TestCategory("Unit")]
		public void GetJSPath_WithoutKey_ReturnsPathButDoesNotExplode()
		{
			ConfigurationManager.AppSettings["CanadaPostAutoCompleteApiKey"] = null;

			var postConfig = new CanadaPostConfiguration();
			var path = postConfig.GetJsPath();
			path.Should().Be($"https://ws1.postescanada-canadapost.ca/js/addresscomplete-2.30.min.js?key=");
		}

		[TestMethod, TestCategory("Unit")]
		public void GetJSPath_WithKey_ReturnsPath()
		{
			ConfigurationManager.AppSettings["CanadaPostAutoCompleteApiKey"] = TestKey;

			var postConfig = new CanadaPostConfiguration();
			var path = postConfig.GetJsPath();
			path.Should().Be($"https://ws1.postescanada-canadapost.ca/js/addresscomplete-2.30.min.js?key={TestKey}");
		}

		[TestMethod, TestCategory("Unit")]
		public void GetCSSPath_WithKey_ReturnsPath()
		{
			ConfigurationManager.AppSettings["CanadaPostAutoCompleteApiKey"] = TestKey;

			var postConfig = new CanadaPostConfiguration();
			var path = postConfig.GetCssPath();
			path.Should().Be($"https://ws1.postescanada-canadapost.ca/css/addresscomplete-2.30.min.css?key={TestKey}");
		}

		[TestMethod, TestCategory("Unit")]
		public void GetKey_WithKeySetting_ReturnsKey()
		{
			// Can't mock out ConfigurationManager directly - no interface
			ConfigurationManager.AppSettings["CanadaPostAutoCompleteApiKey"] = TestKey;

			var postConfig = new CanadaPostConfiguration();
			var canadaPostKey = postConfig.Key;

			canadaPostKey.Should().Be(TestKey);
		}

		[TestMethod, TestCategory("Unit")]
		public void GetKey_WithoutKeySetting_ReturnsBlank()
		{
			ConfigurationManager.AppSettings["CanadaPostAutoCompleteApiKey"] = null;

			var postConfig = new CanadaPostConfiguration();
			var canadaPostKey = postConfig.Key;

			canadaPostKey.Should().Be(string.Empty);
		}
	}
}