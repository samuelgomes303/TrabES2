using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;
using System.Threading;

namespace TrabalhoES2.Tests.UI
{
    [TestFixture]
    public class FundosUITests
    {
        private IWebDriver _driver;
        private WebDriverWait _wait;
        private const string BASE_URL = "http://localhost:5168";
        private const int WAIT_TIMEOUT = 10;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("--disable-web-security", "--headless", "--no-sandbox", "--disable-gpu", "--window-size=1920,1080");
            _driver = new ChromeDriver(chromeOptions);
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(WAIT_TIMEOUT));
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(WAIT_TIMEOUT);
        }

        [SetUp]
        public void SetUp()
        {
            _driver.Navigate().GoToUrl(BASE_URL);
            Thread.Sleep(2000); // garantir que servidor está ativo
        }

        [Test, Order(0)]
        public void Test_CriarFundoComoAdmin()
        {
            LoginAsAdmin();
            _driver.Navigate().GoToUrl($"{BASE_URL}/Carteira/CreateFundo");

            _wait.Until(d => d.FindElement(By.Name("Nome"))).SendKeys("Fundo UI Teste");
            _driver.FindElement(By.Name("BancoId")).Click();
            _driver.FindElements(By.Name("BancoId")).First().SendKeys(Keys.ArrowDown + Keys.Enter);

            _driver.FindElement(By.Id("montante")).SendKeys("1000");
            _driver.FindElement(By.Id("taxa")).SendKeys("3");

            _driver.FindElement(By.Name("ativo.Duracaomeses")).SendKeys("12");

            var submit = _driver.FindElements(By.CssSelector("button[type='submit']")).FirstOrDefault();
            Assert.IsNotNull(submit, "Botão de submit não encontrado.");
            submit.Click();

            Thread.Sleep(2000);
            Assert.IsTrue(_driver.Url.Contains("AtivosCatalogo") || _driver.Url.Contains("GestaoFundos"));
        }

        [Test, Order(1)]
        public void Test_AdminFiltraFundos()
        {
            LoginAsAdmin();
            _driver.Navigate().GoToUrl($"{BASE_URL}/Carteira/GestaoFundos");

            var input = _wait.Until(d => d.FindElement(By.Id("search-ativo")));
            input.SendKeys("fundo");
            Thread.Sleep(1500);

            var fundos = _driver.FindElements(By.CssSelector("#grid-ativos .col-md-4"));
            Assert.IsTrue(fundos.Count > 0, "Nenhum fundo visível após filtro.");
        }

        [Test, Order(2)]
        public void Test_AdminVeGestaoFundos()
        {
            LoginAsAdmin();
            _driver.Navigate().GoToUrl($"{BASE_URL}/Carteira/GestaoFundos");

            Assert.IsTrue(_driver.PageSource.Contains("Gestão de Ativos do Administrador"), "Texto de gestão não encontrado.");
        }

        [Test, Order(3)]
        public void Test_ClienteVeFundosDoAdmin()
        {
            LoginAsCliente();
            _driver.Navigate().GoToUrl($"{BASE_URL}/Carteira/AtivosCatalogo");

            Thread.Sleep(1500);

            bool encontrou = _driver.PageSource.Contains("Fundos do Administrador") || 
                             _driver.FindElements(By.XPath("//*[contains(text(),'Fundos do Administrador')]")).Count > 0;

            Assert.IsTrue(encontrou, "Seção 'Fundos do Administrador' não visível para o cliente.");
        }

        private void LoginAsAdmin()
        {
            _driver.Navigate().GoToUrl($"{BASE_URL}/Identity/Account/Login");
            _wait.Until(d => d.FindElement(By.Id("Input_Email"))).SendKeys("admin@example.com");
            _driver.FindElement(By.Id("Input_Password")).SendKeys("Admin@123");
            _driver.FindElement(By.CssSelector("button[type='submit']")).Click();
            Thread.Sleep(2000);
        }

        private void LoginAsCliente()
        {
            _driver.Navigate().GoToUrl($"{BASE_URL}/Identity/Account/Login");
            _wait.Until(d => d.FindElement(By.Id("Input_Email"))).SendKeys("client@gmail.com");
            _driver.FindElement(By.Id("Input_Password")).SendKeys("Admin@123");
            _driver.FindElement(By.CssSelector("button[type='submit']")).Click();
            Thread.Sleep(2000);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _driver.Quit();
            _driver.Dispose();
        }
    }
}
