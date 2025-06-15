using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Threading;

namespace TrabalhoES2.Tests.UI
{
    [TestFixture]
    public class FundosUITests
    {
        private IWebDriver driver;
        private const string BaseUrl = "http://localhost:5168";

        private void PreencherComDelay(By by, string valor, int delay = 1000)
        {
            var el = driver.FindElement(by);
            el.Clear();
            el.SendKeys(valor);
            Thread.Sleep(delay);
        }

        [SetUp]
        public void Setup()
        {
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl($"{BaseUrl}/Identity/Account/Login");

            Thread.Sleep(1000);
            driver.FindElement(By.Id("Input_Email")).SendKeys("admin@example.com");
            Thread.Sleep(800);
            driver.FindElement(By.Id("Input_Password")).SendKeys("Admin@123");
            Thread.Sleep(800);
            driver.FindElement(By.CssSelector("button[type='submit']")).Click();
            Thread.Sleep(2000);
        }

        [Test]
        public void CriarFundoEVerificarNaCarteira()
        {
            driver.Navigate().GoToUrl($"{BaseUrl}/Carteira/CreateFundo");
            Thread.Sleep(1000);

            PreencherComDelay(By.Name("Nome"), "Fundo Selenium");
            PreencherComDelay(By.Name("Montanteinvestido"), "2000");
            PreencherComDelay(By.Name("Taxajuropdefeito"), "4");
            PreencherComDelay(By.Name("ativo.Duracaomeses"), "24");

            var selectBanco = new SelectElement(driver.FindElement(By.Name("BancoId")));
            selectBanco.SelectByIndex(1);
            Thread.Sleep(800);

            var form = driver.FindElement(By.CssSelector("form[action*='CreateFundo']"));
            var token = form.FindElement(By.Name("__RequestVerificationToken")).GetAttribute("value");
            Console.WriteLine("Token CSRF do fundo: " + token);

            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].submit()", form);
            Thread.Sleep(2500);

            driver.Navigate().GoToUrl($"{BaseUrl}/Carteira");
            Thread.Sleep(2000);
            Assert.That(driver.PageSource, Does.Contain("Fundo Selenium"), "Fundo não aparece na carteira.");
        }

        [Test]
        public void EditarFundoNaPaginaCatalogo()
        {
            driver.Navigate().GoToUrl($"{BaseUrl}/Carteira/GestaoFundos");
            Thread.Sleep(1000);

            var editarLinks = driver.FindElements(By.CssSelector("a[href*='EditFundo']"));
            Assert.That(editarLinks.Count, Is.GreaterThan(0), "Nenhum botão de edição encontrado para fundos.");

            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", editarLinks[0]);
            Thread.Sleep(500);
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", editarLinks[0]);
            Thread.Sleep(1000);

            Assert.That(driver.Url, Does.Contain("EditFundo"));

            // Altera o nome do fundo (verificação mais robusta)
            var nomeInput = driver.FindElement(By.Id("Nome"));
            nomeInput.Clear();
            nomeInput.SendKeys("Fundo Selenium Editado");
            Thread.Sleep(500);

            var form = driver.FindElement(By.CssSelector("form[action*='EditFundo']"));
            var token = form.FindElement(By.Name("__RequestVerificationToken")).GetAttribute("value");
            Console.WriteLine("Token CSRF de edição do fundo: " + token);

            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].submit()", form);
            Thread.Sleep(2000);

            driver.Navigate().GoToUrl($"{BaseUrl}/Carteira");
            Thread.Sleep(1500);

            string pageSource = driver.PageSource;
            Assert.That(pageSource, Does.Contain("Fundo Selenium Editado"), "O nome atualizado do fundo não foi encontrado.");
        }

        [Test]
        public void RemoverFundoNaCarteira()
        {
            driver.Navigate().GoToUrl($"{BaseUrl}/Carteira/GestaoFundos");
            Thread.Sleep(1500);

            var removerLinks = driver.FindElements(By.CssSelector("a[href*='DeleteFundo']"));
            Assert.That(removerLinks.Count, Is.GreaterThan(0), "Nenhum botão de remoção encontrado para fundos.");

            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", removerLinks[0]);
            Thread.Sleep(400);
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", removerLinks[0]);
            Thread.Sleep(1000);

            Assert.That(driver.Url, Does.Contain("DeleteFundo"));

            var form = driver.FindElement(By.CssSelector("form[action*='DeleteFundo']"));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].submit()", form);
            Thread.Sleep(2000);

            driver.Navigate().GoToUrl($"{BaseUrl}/Carteira");
            Thread.Sleep(2000);
            Assert.That(driver.PageSource, Does.Not.Contain("Fundo Selenium Editado"), "O fundo ainda aparece após a remoção.");
        }

        [TearDown]
        public void TearDown()
        {
            driver.Quit();
            driver.Dispose();
        }
    }
}
