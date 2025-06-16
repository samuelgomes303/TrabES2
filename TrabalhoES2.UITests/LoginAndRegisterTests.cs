using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;

namespace TrabalhoES2.UITests
{
    [TestFixture]
    public class LoginAndRegisterTests
    {
        private IWebDriver _driver;
        // Ajusta aqui para a porta onde a tua app está a correr:
        private string _baseUrl = "http://localhost:5168";

        private string AddTestMode(string url)
        {
            if (url.Contains("?")) return url + "&testmode=1";
            return url + "?testmode=1";
        }

        [SetUp]
        public void Setup()
        {
            var options = new ChromeOptions();
            // REMOVE headless for diagnosis
            // options.AddArgument("--headless");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            _driver = new ChromeDriver(options);
        }

        [TearDown]
        public void Teardown()
        {
            if (_driver != null)
            {
                _driver.Quit();
                _driver.Dispose();
            }
        }

        [Test]
        public void Login_WithInvalidCredentials_ShowsError()
        {
            _driver.Navigate().GoToUrl(AddTestMode($"{_baseUrl}/Identity/Account/Login"));
            var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            var emailInput = wait.Until(d => d.FindElement(By.Name("Input.Email")));
            var passInput = wait.Until(d => d.FindElement(By.Name("Input.Password")));
            emailInput.SendKeys("nao@existe.com");
            passInput.SendKeys("senhaErrada");
            // Instead of clicking, submit the form directly via JS
            ((IJavaScriptExecutor)_driver).ExecuteScript(@"
                var form = document.getElementById('account');
                if(form) form.submit();
            ");

            // Wait for either the testmode error block, the summary, or a field error span to appear and be visible with text
            bool errorAppeared = false;
            string debugInfo = "";
            try
            {
                var errorElement = wait.Until(driver =>
                {
                    var testmodeError = driver.FindElements(By.Id("testmode-login-error-debug"));
                    if (testmodeError.Count > 0 && testmodeError[0].Displayed && !string.IsNullOrWhiteSpace(testmodeError[0].Text))
                    {
                        debugInfo += "[Testmode error block: " + testmodeError[0].Text + "] ";
                        return testmodeError[0];
                    }
                    var summary = driver.FindElements(By.CssSelector("div.text-danger[role='alert']"));
                    if (summary.Count > 0 && summary[0].Displayed && !string.IsNullOrWhiteSpace(summary[0].Text))
                    {
                        debugInfo += "[Summary: " + summary[0].Text + "] ";
                        return summary[0];
                    }
                    var passError = driver.FindElements(By.CssSelector("span[data-valmsg-for='Input.Password']"));
                    if (passError.Count > 0 && passError[0].Displayed && !string.IsNullOrWhiteSpace(passError[0].Text))
                    {
                        debugInfo += "[Password error span: " + passError[0].Text + "] ";
                        return passError[0];
                    }
                    return null;
                });
                errorAppeared = errorElement != null;
            }
            catch (Exception ex)
            {
                debugInfo += $"[Current URL: {_driver.Url}] ";
                debugInfo += "[Page snippet: " + _driver.PageSource.Substring(0, Math.Min(500, _driver.PageSource.Length)).Replace('\n',' ').Replace('\r',' ') + "] ";
                debugInfo += $"[Exception: {ex.Message}] ";
            }
            Assert.That(errorAppeared, Is.True, $"A mensagem de erro de login deveria estar visível. {debugInfo}");
        }

        [Test]
        public void Register_WithMismatchedPasswords_ShowsValidationError()
        {
            _driver.Navigate().GoToUrl(AddTestMode($"{_baseUrl}/Identity/Account/Register"));
            var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            var nomeInput = wait.Until(d => d.FindElement(By.Name("Input.Nome")));
            var emailInput = wait.Until(d => d.FindElement(By.Name("Input.Email")));
            var passInput = wait.Until(d => d.FindElement(By.Name("Input.Password")));
            var confirmInput = wait.Until(d => d.FindElement(By.Name("Input.ConfirmPassword")));
            nomeInput.SendKeys("Test User");
            var uniqueEmail = $"test{DateTime.Now.Ticks}@example.com";
            emailInput.SendKeys(uniqueEmail);
            passInput.SendKeys("Password123!");
            confirmInput.SendKeys("Different123!");
            var registerBtn = wait.Until(d => d.FindElement(By.CssSelector("button[type=submit]")));
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", registerBtn);
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", registerBtn);
            // Wait for error span to appear
            var errorSpan = wait.Until(d => d.FindElement(By.CssSelector("span[data-valmsg-for='Input.ConfirmPassword']")));
            Assert.That(errorSpan.Displayed, Is.True, "Deveria aparecer erro junto ao ConfirmPassword.");
            Assert.That(errorSpan.Text, Does.Contain("do not match"),
                        "Mensagem de erro inesperada (esperava algo como 'do not match').");
        }
    }
}
