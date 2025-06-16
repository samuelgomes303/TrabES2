using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;
using System.Threading;

namespace TrabalhoES2.UITests
{
    [TestFixture]
    public class CarteiraFilterTests
    {
        private IWebDriver _driver;
        private string _baseUrl = "http://localhost:5168";

        private string AddTestMode(string url)
        {
            if (url.Contains("?")) return url + "&testmode=1";
            return url + "?testmode=1";
        }

        [SetUp]
        public void SetUp()
        {
            var opts = new ChromeOptions();
            opts.AddArgument("--headless");
            _driver = new ChromeDriver(opts);

            // Always use the seeded Selenium test user for login
            _driver.Navigate().GoToUrl(AddTestMode($"{_baseUrl}/Identity/Account/Login"));
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

            try
            {
                // Remove overlays and force login button visible in test mode
                ((IJavaScriptExecutor)_driver).ExecuteScript(@"
                    document.querySelectorAll('[style*=""z-index""], .modal, .overlay, .backdrop, .sticky-top, .fixed-top, .fixed-bottom').forEach(e => e.style.display = 'none');
                    var btn = document.querySelector('button[type=submit]');
                    if (btn) { btn.style.display = 'block'; btn.disabled = false; btn.removeAttribute('disabled'); }
                ");

                var email = wait.Until(d => d.FindElement(By.Name("Input.Email")));
                var pass = wait.Until(d => d.FindElement(By.Name("Input.Password")));
                email.Clear();
                pass.Clear();
                email.SendKeys("tf@ipvc.pt");
                pass.SendKeys("Piruças123#");
                // Force set password value via JS in case SendKeys is blocked
                ((IJavaScriptExecutor)_driver).ExecuteScript("document.getElementsByName('Input.Password')[0].value = 'Piruças123#';");

                // Dump field values again after JS set
                var fieldValues = ((IJavaScriptExecutor)_driver).ExecuteScript(@"
                    return {
                        email: document.getElementsByName('Input.Email')[0]?.value,
                        password: document.getElementsByName('Input.Password')[0]?.value
                    };
                ");
                Console.WriteLine("[DIAG] Field values before submit: " + System.Text.Json.JsonSerializer.Serialize(fieldValues));

                // Screenshot and DOM dump before submit
                var tsPre = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                var screenshotPre = ((ITakesScreenshot)_driver).GetScreenshot();
                var screenshotPathPre = $"screenshot_before_submit_{tsPre}.png";
                screenshotPre.SaveAsFile(screenshotPathPre);
                var domPre = _driver.PageSource;
                System.IO.File.WriteAllText($"dom_before_submit_{tsPre}.html", domPre);
                Console.WriteLine($"[DIAG] Screenshot before submit: {screenshotPathPre}, DOM: dom_before_submit_{tsPre}.html");

                var btn = wait.Until(d => d.FindElement(By.CssSelector("button[type=submit]")));

                // Try normal click first
                TryClickWithDiagnostics(btn, "login");
                Thread.Sleep(1000);

                // If still on login page, try form submission via JS
                if (_driver.Url.Contains("/Identity/Account/Login"))
                {
                    Console.WriteLine("[DIAG] Still on login page after click, trying JS form submission");
                    var form = _driver.FindElement(By.TagName("form"));
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].submit();", form);
                    Thread.Sleep(2000);
                }

                Console.WriteLine($"[DIAG] Current URL after form submission: {_driver.Url}");

                // Dump page source for debugging
                var pageSource = _driver.PageSource;
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                System.IO.File.WriteAllText($"page_after_login_{timestamp}.html", pageSource);
                Console.WriteLine($"[DIAG] Page source dumped to: page_after_login_{timestamp}.html");

                // If we're not on login page anymore, login likely succeeded
                if (!_driver.Url.Contains("/Identity/Account/Login"))
                {
                    Console.WriteLine("[DIAG] Redirected away from login page - login may have succeeded");
                    return;
                }

                // Wait for either debug block (authenticated), error summary, or password error span
                bool loginOutcome = false;
                string loginDebug = "";
                try
                {
                    loginOutcome = wait.Until(d =>
                    {
                        var debugBlock = d.FindElements(By.Id("testmode-login-debug-block"));
                        if (debugBlock.Count > 0 && debugBlock[0].Displayed && debugBlock[0].Text.Contains("Authenticated: True"))
                        {
                            loginDebug += "[Debug block: " + debugBlock[0].Text + "] ";
                            return true;
                        }
                        var summary = d.FindElements(By.CssSelector("div.text-danger[role='alert']"));
                        if (summary.Count > 0 && summary[0].Displayed && !string.IsNullOrWhiteSpace(summary[0].Text))
                        {
                            loginDebug += "[Summary: " + summary[0].Text + "] ";
                            return true;
                        }
                        var passError = d.FindElements(By.CssSelector("span[data-valmsg-for='Input.Password']"));
                        if (passError.Count > 0 && passError[0].Displayed && !string.IsNullOrWhiteSpace(passError[0].Text))
                        {
                            loginDebug += "[Password error span: " + passError[0].Text + "] ";
                            return true;
                        }
                        return false;
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DIAG] No debug block or error summary found: {ex.Message}");
                }
                if (!loginOutcome)
                {
                    var dom = _driver.PageSource;
                    System.IO.File.WriteAllText("login_debug_fail.html", dom);
                    Console.WriteLine("[DIAG] Login failed. " + loginDebug);
                    Assert.Fail("Login did not succeed. " + loginDebug);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Login setup failed: {ex.Message}");
                Assert.Fail($"Login setup failed: {ex.Message}");
            }

            Thread.Sleep(500); // Give time for page to settle
        }

        private void TryClickWithDiagnostics(IWebElement element, string label)
        {
            try
            {
                element.Click();
            }
            catch (ElementClickInterceptedException)
            {
                var ts = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                var screenshot = ((ITakesScreenshot)_driver).GetScreenshot();
                var screenshotPath = $"screenshot_{label}_{ts}.png";
                screenshot.SaveAsFile(screenshotPath);
                var dom = _driver.PageSource;
                System.IO.File.WriteAllText($"dom_{label}_{ts}.html", dom);
                var loc = element.Location;
                var size = element.Size;
                Console.WriteLine($"[DIAG] Click intercepted on '{label}' at {loc.X},{loc.Y} size {size.Width}x{size.Height}");
                Console.WriteLine($"[DIAG] Screenshot: {screenshotPath}, DOM: dom_{label}_{ts}.html");
                // Try JS click as fallback
                try
                {
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", element);
                }
                catch (Exception jsEx)
                {
                    Console.WriteLine($"[DIAG] JS click fallback failed for '{label}': {jsEx.Message}");
                    var dom2 = _driver.PageSource;
                    System.IO.File.WriteAllText($"dom_{label}_jsfail_{ts}.html", dom2);
                }
            }
        }

        [TearDown]
        public void TearDown()
        {
            _driver?.Quit();
            _driver?.Dispose();
        }

        [Test]
        public void FiltrarPorDesignacao_ClientSide_HidesNonMatchingRows()
        {
            _driver.Navigate().GoToUrl(AddTestMode(_baseUrl + "/Carteira/Index"));
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            
            var input = wait.Until(d => d.FindElement(By.Id("designacaoInput")));
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", input);
            Thread.Sleep(200);
            input.Clear();
            input.SendKeys("ABC");
            
            wait.Until(d => d.FindElements(By.CssSelector("#ativosTableBody tr")).All(r => !r.Displayed || r.Text.Contains("ABC")));
            var rows = wait.Until(d => d.FindElements(By.CssSelector("#ativosTableBody tr")));
            
            Assert.That(rows.Any(r => r.Displayed && !r.Text.Contains("ABC")), Is.False,
                        "Todas as linhas visíveis devem conter 'ABC'");
        }

        [Test]
        [Ignore("Server-side type filtering does a full postback; consider a dedicated controller test instead.")]
        public void FiltrarPorTipo_ServerSide_FullPageReload()
        {
            Assert.Inconclusive("Teste de filtro por tipo movido para testes de controller.");
        }

        [Test]
        public void Login_WithValidCredentials_RedirectsToHome()
        {
            // Already logged in by SetUp, just check for logout button or home page
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            bool foundLogout = false;
            try
            {
                foundLogout = wait.Until(d => d.FindElement(By.CssSelector("form[action*='/Identity/Account/Logout']"))).Displayed;
            }
            catch { }
            if (!foundLogout)
            {
                try
                {
                    foundLogout = wait.Until(d => d.FindElement(By.LinkText("Logout"))).Displayed;
                }
                catch { }
            }
            Assert.That(foundLogout, Is.True, "Logout button or link should be present after login");
        }

        private void Login()
        {
            // No-op: login is handled in SetUp. This method is kept for compatibility.
        }

        [Test]
        public void Logout_AfterLogin_ReturnsToLoginPage()
        {
            Login();
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            
            // Try to find logout button/form
            IWebElement? logoutForm = null;
            try 
            { 
                logoutForm = wait.Until(d => d.FindElement(By.CssSelector("form[action*='/Identity/Account/Logout']"))); 
            } 
            catch { }
            
            if (logoutForm == null)
            {
                try 
                { 
                    logoutForm = wait.Until(d => d.FindElement(By.LinkText("Logout"))); 
                } 
                catch { }
            }
            
            if (logoutForm != null)
            {
                var logoutBtn = logoutForm.FindElements(By.CssSelector("button[type=submit]"))?.FirstOrDefault();
                if (logoutBtn != null)
                {
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", logoutBtn);
                    Thread.Sleep(200);
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", logoutBtn);
                }
                else
                {
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", logoutForm);
                    Thread.Sleep(200);
                    logoutForm.Click();
                }
            }
            
            wait.Until(d => d.Url.Contains("/"));
            _driver.Navigate().GoToUrl(_baseUrl + "/Carteira/Index");
            wait.Until(d => d.Url.Contains("/Identity/Account/Login"));
            Assert.That(_driver.Url, Does.Contain("/Identity/Account/Login"));
        }

        [Test]
        public void Register_WithValidData_SucceedsAndAllowsLogin()
        {
            // Do NOT use SetUp login for this test: create a new driver instance
            var opts = new ChromeOptions();
            opts.AddArgument("--headless");
            using (var driver = new ChromeDriver(opts))
            {
                string baseUrl = _baseUrl;
                string AddTestMode(string url) => url.Contains("?") ? url + "&testmode=1" : url + "?testmode=1";
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                // Ensure TestResults directory exists in the output directory
                var testResultsDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestResults");
                System.IO.Directory.CreateDirectory(testResultsDir);

                var uniqueEmail = $"ui{DateTime.Now.Ticks}@example.com";

                driver.Navigate().GoToUrl(AddTestMode($"{baseUrl}/Identity/Account/Register"));
                var nomeInput = wait.Until(d => d.FindElement(By.Name("Input.Nome")));
                var emailInput = wait.Until(d => d.FindElement(By.Name("Input.Email")));
                var passInput = wait.Until(d => d.FindElement(By.Name("Input.Password")));
                var confirmInput = wait.Until(d => d.FindElement(By.Name("Input.ConfirmPassword")));

                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", nomeInput);
                Thread.Sleep(200);
                nomeInput.SendKeys("Teste UI");

                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", emailInput);
                Thread.Sleep(200);
                emailInput.SendKeys(uniqueEmail);

                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", passInput);
                Thread.Sleep(200);
                passInput.SendKeys("Password123!");

                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", confirmInput);
                Thread.Sleep(200);
                confirmInput.SendKeys("Password123!");

                var registerBtn = wait.Until(d => d.FindElement(By.CssSelector("button[type=submit]")));
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", registerBtn);
                Thread.Sleep(200);
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", registerBtn);

                // Wait for registration to complete (redirect or debug block)
                wait.Until(d =>
                {
                    if (!d.Url.Contains("Register")) return true;
                    if (d.Url.Contains("/Identity/Account/Login")) return true;
                    var debugBlock = d.FindElements(By.Id("selenium-debug-authenticated"));
                    if (debugBlock.Count > 0 && debugBlock[0].Displayed) return true;
                    return false;
                });

                // Clear cookies after registration to avoid token/cookie mismatch
                driver.Manage().Cookies.DeleteAllCookies();
                Thread.Sleep(200);

                // Always try to login after registration
                driver.Navigate().GoToUrl(AddTestMode($"{baseUrl}/Identity/Account/Login"));
                var emailInput2 = wait.Until(d => d.FindElement(By.Name("Input.Email")));
                var passInput2 = wait.Until(d => d.FindElement(By.Name("Input.Password")));

                // Dump anti-forgery token for diagnostics
                var antiForgeryToken = ((IJavaScriptExecutor)driver).ExecuteScript(@"
                    var input = document.querySelector('input[name=__RequestVerificationToken]');
                    return input ? input.value : null;
                ");
                Console.WriteLine($"[DIAG] [RegisterTest] Anti-forgery token on login page: {antiForgeryToken}");

                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", emailInput2);
                Thread.Sleep(200);
                emailInput2.Clear();
                emailInput2.SendKeys(uniqueEmail); // Always use the newly registered email

                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", passInput2);
                Thread.Sleep(200);
                passInput2.Clear();
                passInput2.SendKeys("Password123!");
                // Force set password value via JS in case SendKeys is blocked
                ((IJavaScriptExecutor)driver).ExecuteScript("document.getElementsByName('Input.Password')[0].value = 'Password123!';");

                // Dump field values before submit for diagnostics
                var fieldValues = ((IJavaScriptExecutor)driver).ExecuteScript(@"
                    return {
                        email: document.getElementsByName('Input.Email')[0]?.value,
                        password: document.getElementsByName('Input.Password')[0]?.value
                    };
                ");
                Console.WriteLine("[DIAG] [RegisterTest] Field values before submit: " + System.Text.Json.JsonSerializer.Serialize(fieldValues));
                var tsPre = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                var screenshotPre = ((ITakesScreenshot)driver).GetScreenshot();
                var screenshotPathPre = System.IO.Path.Combine(testResultsDir, $"register_login_before_submit_{tsPre}.png");
                screenshotPre.SaveAsFile(screenshotPathPre);
                var domPre = driver.PageSource;
                System.IO.File.WriteAllText(System.IO.Path.Combine(testResultsDir, $"register_login_before_submit_{tsPre}.html"), domPre);
                Console.WriteLine($"[DIAG] [RegisterTest] Screenshot before submit: {screenshotPathPre}, DOM: register_login_before_submit_{tsPre}.html");

                var loginBtn2 = wait.Until(d => d.FindElement(By.CssSelector("button[type=submit]")));
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", loginBtn2);
                Thread.Sleep(200);
                loginBtn2.Click();

                // Wait for either redirect away from login page (success) OR debug block (if still on login page)
                try
                {
                    wait.Until(d =>
                    {
                        if (!d.Url.Contains("/Identity/Account/Login"))
                            return true; // Success: redirected
                        // Always re-find the debug block to avoid stale element
                        var debugBlock = d.FindElements(By.Id("selenium-debug-authenticated"));
                        if (debugBlock.Count > 0 && debugBlock[0].Displayed)
                            return true; // Success: debug block shows authenticated
                        return false;
                    });
                    // If redirected away from login page, navigate to home with testmode=1 and check for debug block
                    if (!driver.Url.Contains("/Identity/Account/Login"))
                    {
                        driver.Navigate().GoToUrl(AddTestMode($"{baseUrl}/"));
                        try
                        {
                            var debugBlock = new WebDriverWait(driver, TimeSpan.FromSeconds(15)).Until(d =>
                            {
                                var elems = d.FindElements(By.Id("selenium-debug-authenticated"));
                                return elems.Count > 0 && elems[0].Displayed ? elems[0] : null;
                            });
                            if (debugBlock == null || !debugBlock.Displayed)
                            {
                                var dom = driver.PageSource;
                                System.IO.File.WriteAllText(System.IO.Path.Combine(testResultsDir, "register_login_debug_block_not_found.html"), dom);
                                throw new Exception("Debug block not found after login on home page");
                            }
                        }
                        catch (Exception ex2)
                        {
                            var dom = driver.PageSource;
                            System.IO.File.WriteAllText(System.IO.Path.Combine(testResultsDir, "register_login_debug_block_wait_fail.html"), dom);
                            throw new Exception($"Debug block not found after login on home page (waited 15s): {ex2.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    var dom = driver.PageSource;
                    System.IO.File.WriteAllText(System.IO.Path.Combine(testResultsDir, "register_login_debug_fail.html"), dom);
                    Assert.Fail($"Login did not succeed after registration. Exception: {ex.Message}");
                }
            }
        }

        [Test]
        public void FiltrarPorTipo_ServerSide_RetornaApenasDepositoPrazo()
        {
            _driver.Navigate().GoToUrl(AddTestMode(_baseUrl + "/Carteira/Index"));
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

            var selectElem = wait.Until(d => d.FindElement(By.Name("tipo")));
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", selectElem);
            Thread.Sleep(200);
            var select = new SelectElement(selectElem);
            select.SelectByValue("DepositoPrazo");

            var filtrarBtn = wait.Until(d => d.FindElement(By.Id("filtrarBtn")));
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", filtrarBtn);
            Thread.Sleep(200);
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", filtrarBtn);

            wait.Until(d => d.Url.Contains("tipo=DepositoPrazo"));
            var rows = wait.Until(d => d.FindElements(By.CssSelector("#ativosTableBody tr")));

            Assert.That(rows.Count, Is.GreaterThan(0), "Deveria haver pelo menos uma linha após o filtro por tipo.");
            Assert.That(rows.All(r => r.FindElement(By.CssSelector(".badge-type")).Text.Trim().Equals("PRAZO", StringComparison.OrdinalIgnoreCase)),
                Is.True, "Todas as linhas devem ter tipo PRAZO após o filtro.");
        }

        [Test]
        public void FiltrarPorMontante_ServerSide()
        {
            _driver.Navigate().GoToUrl(AddTestMode(_baseUrl + "/Carteira/Index"));
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

            var montanteInput = wait.Until(d => d.FindElement(By.Name("montanteAplicado")));
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", montanteInput);
            Thread.Sleep(200);
            montanteInput.Clear();
            montanteInput.SendKeys("150");

            var filtrarBtn = wait.Until(d => d.FindElement(By.Id("filtrarBtn")));
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", filtrarBtn);
            Thread.Sleep(200);
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", filtrarBtn);

            wait.Until(d => d.Url.Contains("montanteAplicado=150"));
            var rows = wait.Until(d => d.FindElements(By.CssSelector("#ativosTableBody tr")));

            Assert.That(rows.Count, Is.GreaterThan(0), "Deveria haver pelo menos uma linha após filtrar por montante.");

            foreach (var row in rows)
            {
                var cols = row.FindElements(By.TagName("td"));
                if (cols.Count < 4) continue; // ajusta índice conforme estrutura
                var textoMontante = cols[3].Text.Replace("€", "").Trim();
                if (decimal.TryParse(textoMontante, out var montante))
                {
                    Assert.That(montante, Is.GreaterThanOrEqualTo(150),
                        $"Linha apresenta montante abaixo do filtro: {montante}");
                }
            }
        }
    }
}
