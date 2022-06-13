using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;
using mmisharp;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.IO;
using System.Collections.Generic;

namespace AppGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MmiCommunication mmiC;
        private IWebDriver driver;

        //  new 16 april 2020
        private MmiCommunication mmiSender;
        private LifeCycleEvents lce;
        private MmiCommunication mmic;

        public int letter;
        public int number;
        public String currentGame = null;

        public MainWindow()
        {
            InitializeComponent();


            mmiC = new MmiCommunication("localhost",8000, "User1", "GUI");
            mmiC.Message += MmiC_Message;
            mmiC.Start();

            // NEW 16 april 2020
            //init LifeCycleEvents..
            lce = new LifeCycleEvents("APP", "TTS", "User1", "na", "command"); // LifeCycleEvents(string source, string target, string id, string medium, string mode
            // MmiCommunication(string IMhost, int portIM, string UserOD, string thisModalityName)
            mmic = new MmiCommunication("localhost", 8000, "User1", "GUI");


            driver = new ChromeDriver(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName);
            driver.Navigate().GoToUrl("https://cardgames.io/");

            letter = 1;
            number = 1;
        }

        private void MmiC_Message(object sender, MmiEventArgs e)
        {
            Console.WriteLine(e.Message);
            var doc = XDocument.Parse(e.Message);
            var com = doc.Descendants("command").FirstOrDefault().Value;
            dynamic json = JsonConvert.DeserializeObject(com);

            Random r = null;
            string[] repeat = {"Desculpe, não percebi, pode repetir?", "Não o consegui ouvir, pode repetir por favor?", "Poderia repetir se faz favor? Não percebi bem" };
            r = new Random(DateTime.Now.Day);

            if (float.Parse(json.recognized[2].ToString()) < 0.50)
            {
                //repeat
            }
            else
            {
                if (currentGame == null)
                {
                    switch ((string)json.recognized[1].ToString())
                    {
                        case "SEA":
                            driver.Navigate().GoToUrl("https://cardgames.io/seabattle/");
                            currentGame = "sea";
                            break;
                    }
                } else if (currentGame == "sea")
                {
                    switch ((string)json.recognized[1].ToString())
                    {
                        case "START":
                            if (driver.FindElement(By.Id("ready-to-start")).Displayed)
                            {
                                driver.FindElement(By.Id("ready-to-start")).Click();
                                driver.FindElement(By.Id("play-silently")).Click();
                            }
                            break;
                    
                        case "SHUFFLE":
                            if (driver.FindElement(By.Id("randomize")).Displayed)
                            {
                                driver.FindElement(By.Id("randomize")).Click();
                            }
                            break;

                        case "SHOOT":
                            driver.FindElement(By.XPath("//*[@id='their-ships']/div/div["+number+"]/div["+letter+"]")).Click();
                            letter = 1;
                            number = 1;
                            break;

                        case "LETTER":
                            if (letter < 10)
                            {
                                letter += 1;
                            }
                            break;

                        case "NUMBER":
                            if (number < 10)
                            {
                                number += 1;
                            }
                            break;

                        case "STOP":
                            driver.Navigate().GoToUrl("https://cardgames.io/seabattle/");
                            break;
                    
                    }
                }
            }
        }

        public void sendJson(String content)
        {
            mmic.Send(lce.NewContextRequest());

            string json2 = "";
            json2 += content;
            var exNot = lce.ExtensionNotification(0 + "", 0 + "", 1, json2);
            mmic.Send(exNot);
        }
    }
}
