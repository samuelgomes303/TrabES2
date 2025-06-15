
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Threading;

namespace TrabalhoES2.Tests.UI
{
    [TestFixture]
    public class DepositoUITests
    {
        private IWebDriver driver;

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
            driver.Navigate().GoToUrl("http://localhost:5168/Identity/Account/Login");

            Thread.Sleep(1000);
            driver.FindElement(By.Id("Input_Email")).SendKeys("teste@gmail.com");
            Thread.Sleep(800);
            driver.FindElement(By.Id("Input_Password")).SendKeys("Teste#1");
            Thread.Sleep(800);
            driver.FindElement(By.CssSelector("button[type='submit']")).Click();
            Thread.Sleep(2000);
        }

        [Test]
        public void CriarDepositoEVerificarNaCarteira()
        {
            // Ir para AtivosCatalogo
            driver.Navigate().GoToUrl("http://localhost:5168/Carteira/AtivosCatalogo");
            Thread.Sleep(1500);

            // Clicar no botão para criar depósito
            var novoDepositoBtn = driver.FindElement(By.LinkText("+ Novo Depósito a Prazo"));
            novoDepositoBtn.Click();
            Thread.Sleep(1000);

            // Preencher formulário
            PreencherComDelay(By.Name("deposito.Titular"), "Selenium");
            PreencherComDelay(By.Name("deposito.Valorinicial"), "1500");
            PreencherComDelay(By.Name("deposito.Taxajuroanual"), "5");
            PreencherComDelay(By.Name("ativo.Duracaomeses"), "12");

            var selectBanco = new SelectElement(driver.FindElement(By.Name("deposito.BancoId")));
            selectBanco.SelectByIndex(1);
            Thread.Sleep(800);

            // Encontrar formulário correto e token
            var form = driver.FindElement(By.CssSelector("form[action*='CriarDeposito']"));
            var token = form.FindElement(By.Name("__RequestVerificationToken")).GetAttribute("value");
            Console.WriteLine("🛡️ CSRF Token correto: " + token);

            // Submeter o formulário via JavaScript
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].submit()", form);
            Thread.Sleep(2500);

            // Verificar se a sessão foi mantida
            string currentUrl = driver.Url;
            Console.WriteLine("🌍 URL atual após submit: " + currentUrl);
            Assert.That(currentUrl, Does.Not.Contain("/Login"), " Redirecionado para login — sessão foi perdida.");

            // Verificar se depósito está visível
            driver.Navigate().GoToUrl("http://localhost:5168/Carteira");
            Thread.Sleep(2500);
            Assert.That(driver.PageSource, Does.Contain("Selenium"), " O depósito não foi encontrado na carteira.");
        }
        
        [Test]
        public void EditarDepositoNaPaginaCatalogo()
        {
            driver.Navigate().GoToUrl("http://localhost:5168/Carteira/AtivosCatalogo");
            Thread.Sleep(1000);

            var editarLinks = driver.FindElements(By.LinkText("Editar"));
            Assert.That(editarLinks.Count, Is.GreaterThan(0), " Nenhum botão de edição encontrado.");

            // Scroll até o botão e clicar via JS
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", editarLinks[0]);
            Thread.Sleep(500); // dar tempo ao scroll
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", editarLinks[0]);
            Thread.Sleep(1000);
            
            // Verifica que está na página certa
            Assert.That(driver.Url, Does.Contain("/Edit"));

            // Edita Valor Inicial
            var valorInput = driver.FindElement(By.Id("valor-inicial"));
            valorInput.Clear();
            valorInput.SendKeys("500");
            Thread.Sleep(500);

            // Captura token CSRF e form
            var form = driver.FindElement(By.CssSelector("form[action*='Edit']"));
            var token = form.FindElement(By.Name("__RequestVerificationToken")).GetAttribute("value");
            Console.WriteLine("🛡 Token CSRF de edição: " + token);

            // Submete via JS
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].submit()", form);
            Thread.Sleep(2000);

            // Verifica se não foi redirecionado para login
            var currentUrl = driver.Url;
            Console.WriteLine("🌍 URL após edição: " + currentUrl);
            Assert.That(currentUrl, Does.Not.Contain("/Login"), " Redirecionado para login — sessão foi perdida.");

            // Volta à carteira para confirmar
            driver.Navigate().GoToUrl("http://localhost:5168/Carteira");
            Thread.Sleep(1500);

            Assert.That(driver.PageSource, Does.Contain("500"), " Valor não atualizado na carteira.");
        }
        
        [Test]
        public void RemoverDepositoNaCarteira()
        {
            // Ir para página da carteira
            driver.Navigate().GoToUrl("http://localhost:5168/Carteira");
            Thread.Sleep(1500);

            // Procurar botão "Remover"
            var removerLinks = driver.FindElements(By.CssSelector("a[href*='/Remover/']"));
            Assert.That(removerLinks.Count, Is.GreaterThan(0), " Nenhum botão de remoção encontrado.");

            // Scroll até o botão e clicar via JS
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", removerLinks[0]);
            Thread.Sleep(400);
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", removerLinks[0]);
            Thread.Sleep(1000);

            // Confirma que chegou à página de remoção
            Assert.That(driver.Url, Does.Contain("/Remover"), " Não redirecionou para página de confirmação.");

            // Captura token CSRF e form
            var removerForm = driver.FindElement(By.CssSelector("form[action*='Remover']"));
            var token = removerForm.FindElement(By.Name("__RequestVerificationToken")).GetAttribute("value");
            Console.WriteLine("🛡 Token CSRF de remoção: " + token);

            // Submete via JS com sessão mantida
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].submit()", removerForm);
            Thread.Sleep(2000);

            // Verifica que não foi redirecionado para login
            var currentUrl = driver.Url;
            Assert.That(currentUrl, Does.Not.Contain("/Login"), " Redirecionado para login — sessão foi perdida.");

            // Volta à carteira
            driver.Navigate().GoToUrl("http://localhost:5168/Carteira");
            Thread.Sleep(1500);

            // Confirma que o valor antigo do depósito já não aparece (exemplo: "500")
            Assert.That(driver.PageSource, Does.Not.Contain("Selenium"), " O ativo ainda está presente após a remoção.");
        }




        [TearDown]
        public void TearDown()
        {
            driver.Quit();
            driver.Dispose();
        }
    }
}


