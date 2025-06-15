

using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Threading;

namespace TrabalhoES2.Tests.UI
{
    [TestFixture]
    public class ImovelUITests
    {
        private IWebDriver driver;

        private void PreencherComDelay(By by, string valor, int delay = 800)
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
        public void CriarImovelEVerificarNaCarteira()
        {
            driver.Navigate().GoToUrl("http://localhost:5168/Carteira/AtivosCatalogo");
            Thread.Sleep(1000);

            var novoImovelBtn = driver.FindElement(By.LinkText("+ Novo Imóvel Arrendado"));
            novoImovelBtn.Click();
            Thread.Sleep(1000);

            PreencherComDelay(By.Name("Imovel.Designacao"), "Casa Selenium");
            PreencherComDelay(By.Name("Imovel.Localizacao"), "Rua Teste 123");
            PreencherComDelay(By.Name("Imovel.Valorimovel"), "200000");
            PreencherComDelay(By.Name("Imovel.Valorrenda"), "850");
            PreencherComDelay(By.Name("Imovel.Valormensalcondo"), "100");
            PreencherComDelay(By.Name("Imovel.Valoranualdespesas"), "300");

            PreencherComDelay(By.Name("Ativo.Duracaomeses"), "36");
            PreencherComDelay(By.Name("Ativo.Percimposto"), "15");

            var selectBanco = new SelectElement(driver.FindElement(By.Name("Imovel.BancoId")));
            selectBanco.SelectByIndex(1);

            Thread.Sleep(800);

            var form = driver.FindElement(By.CssSelector("form[action$='CreateImovel']"));
            var token = form.FindElement(By.Name("__RequestVerificationToken")).GetAttribute("value");
            Console.WriteLine("🛡 CSRF Token correto: " + token);

            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].submit()", form);
            Thread.Sleep(2500);

            Assert.That(driver.Url, Does.Not.Contain("/Login"), " Redirecionado para login!");

            driver.Navigate().GoToUrl("http://localhost:5168/Carteira");
            Thread.Sleep(2000);
            Assert.That(driver.PageSource, Does.Contain("Casa Selenium"), " O imóvel não aparece na carteira.");
        }
       
        [Test]
        public void EditarImovelNaPaginaCatalogo()
        {
            driver.Navigate().GoToUrl("http://localhost:5168/Carteira/AtivosCatalogo");
            Thread.Sleep(1000);

            var editarLinks = driver.FindElements(By.LinkText("Editar"));
            Assert.That(editarLinks.Count, Is.GreaterThan(0), " Nenhum botão de edição encontrado.");

            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", editarLinks[0]);
            Thread.Sleep(400);
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", editarLinks[0]);
            Thread.Sleep(1000);

            Assert.That(driver.Url, Does.Contain("/Edit"));

            var designacaoInput = driver.FindElement(By.Name("Designacao"));
            designacaoInput.Clear();
            designacaoInput.SendKeys("Casa Selenium Editada");
            Thread.Sleep(400);

            var rendaInput = driver.FindElement(By.Name("Valorrenda"));
            rendaInput.Clear();
            rendaInput.SendKeys("1000");
            Thread.Sleep(400);

            var form = driver.FindElement(By.CssSelector("form[action*='EditImovel']"));
            var token = form.FindElement(By.Name("__RequestVerificationToken")).GetAttribute("value");
            Console.WriteLine("🛡 Token CSRF de edição: " + token);

            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].submit()", form);
            Thread.Sleep(2000);

            Assert.That(driver.Url, Does.Not.Contain("/Login"), " Redirecionado para login!");

            driver.Navigate().GoToUrl("http://localhost:5168/Carteira");
            Thread.Sleep(1500);

            Assert.That(driver.PageSource, Does.Contain("Casa Selenium Editada"), " O imóvel editado não aparece.");
        }


               [Test]
        public void RemoverImovelDaCarteira()
        {
            // Ir para a página da carteira
            driver.Navigate().GoToUrl("http://localhost:5168/Carteira");
            Thread.Sleep(1000);

            // Clicar no botão "Ver Ativos Disponíveis"
            var verAtivosBtn = driver.FindElement(By.LinkText("Ver Ativos Disponíveis"));
            verAtivosBtn.Click();
            Thread.Sleep(1000);

            // Verificar que está na página dos ativos
            Assert.That(driver.Url, Does.Contain("/Carteira/AtivosCatalogo"));

            // Procurar botão "Eliminar" num card
            var eliminarLinks = driver.FindElements(By.LinkText("Eliminar"));
            Assert.That(eliminarLinks.Count, Is.GreaterThan(0), " Nenhum botão 'Eliminar' encontrado.");

            // Scroll até ao botão e clicar via JS
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", eliminarLinks[0]);
            Thread.Sleep(400);
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", eliminarLinks[0]);
            Thread.Sleep(1000);

            // Verifica que chegou à página DeleteImovel
            Assert.That(driver.Url, Does.Contain("/DeleteImovel"));

            // Captura o formulário de eliminação e o token CSRF
            var form = driver.FindElement(By.CssSelector("form[action*='DeleteImovel']"));
            var token = form.FindElement(By.Name("__RequestVerificationToken")).GetAttribute("value");
            Console.WriteLine("🛡 Token CSRF de eliminação: " + token);

            // Submete via JavaScript
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].submit()", form);
            Thread.Sleep(2000);

            // Verifica que não foi redirecionado para login
            Assert.That(driver.Url, Does.Not.Contain("/Login"), " Redirecionado para login — sessão perdida!");

            // Voltar à carteira e confirmar remoção
            driver.Navigate().GoToUrl("http://localhost:5168/Carteira");
            Thread.Sleep(1500);

            // Confirmar que o imóvel não aparece mais (ex: "Casa Selenium Editada")
            Assert.That(driver.PageSource, Does.Not.Contain("Casa Selenium Editada"), " O imóvel ainda aparece após eliminação.");
        }


        [TearDown]
        public void TearDown()
        {
            driver.Quit();
            driver.Dispose();
        }
    }
}

