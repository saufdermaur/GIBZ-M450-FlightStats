using Backend.Models;
using Microsoft.IdentityModel.Tokens;
using OpenQA.Selenium;
using Shared.DTOs;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Backend.Selenium
{
    public class SeleniumFlights : ISeleniumFlights
    {
        private readonly IWebDriver _webDriver;
        private bool _quit = true; 

        public SeleniumFlights(IWebDriver webDriver)
        {
            _webDriver = webDriver;

            _webDriver.Manage().Window.Maximize();
            _webDriver.Navigate().GoToUrl("https://www.google.com/travel/flights");
            _webDriver.FindElement(By.XPath("(//span[@jsname='V67aGc'][contains(.,'Alle ablehnen')])[1]")).Click();
        }

        public void SearchForFlights(Airport originAirport, Airport destinationAirport, DateTime flightDate)
        {
            // Set "One Way" trip
            IWebElement flightWayField = _webDriver.FindElement(By.XPath("//DIV[@class='VfPpkd-aPP78e']/self::DIV"));
            flightWayField.Click();
            IWebElement flightOneWayField = _webDriver.FindElement(By.XPath("(//li[@class='MCs1Pd UbEQCe VfPpkd-OkbHre VfPpkd-OkbHre-SfQLQb-M1Soyc-bN97Pc VfPpkd-aJasdd-RWgCYc-wQNmvb  ib1Udf VfPpkd-rymPhb-ibnC6b VfPpkd-rymPhb-ibnC6b-OWXEXe-SfQLQb-M1Soyc-Bz112c VfPpkd-rymPhb-ibnC6b-OWXEXe-SfQLQb-Woal0c-RWgCYc'][contains(.,'Nur Hinreise')])[1]"));
            flightOneWayField.Click();

            // Enter the origin airport
            _webDriver.FindElement(By.XPath("(//input[contains(@aria-label,'Von wo?')])[1]")).Click();
            IWebElement originAirportSelenium = _webDriver.FindElement(By.XPath("//input[contains(@aria-describedby,'i24')]"));
            originAirportSelenium.SendKeys(originAirport.IATA);
            originAirportSelenium.SendKeys(Keys.Enter);

            // Enter the destination airport
            _webDriver.FindElement(By.XPath("(//input[contains(@aria-label,'Wohin?')])[1]")).Click();
            IWebElement destinationAirportSelenium = _webDriver.FindElement(By.XPath("(//input[contains(@tabindex,'0')])[2]"));
            destinationAirportSelenium.SendKeys(destinationAirport.IATA);
            destinationAirportSelenium.SendKeys(Keys.Enter);

            // Enter the departure date
            IWebElement departureDateField = _webDriver.FindElement(By.XPath("(//input[@placeholder='Abflug'])[1]"));
            departureDateField.Click();
            departureDateField.SendKeys(flightDate.ToString("dd-MM-yyyy"));
            _webDriver.FindElement(By.XPath("(//span[@jsname='V67aGc'][contains(.,'Fertig')])[2]")).Click();

            // Click on the "Search" button
            IWebElement searchButton = _webDriver.FindElement(By.XPath("//span[contains(text(), 'Suche')]"));
            searchButton.Click();

            Thread.Sleep(500);

            // Open the "Stops" filter
            IWebElement stopsFilterButton = _webDriver.FindElement(By.XPath("//button[contains(@aria-label, 'Stopps, Nicht ausgewählt')]"));
            stopsFilterButton.Click();

            // Select "Nonstop flights only"
            IWebElement nonstopFlightsButton = _webDriver.FindElement(By.XPath("//label[contains(.,'Nur Nonstop-Flüge')]"));
            nonstopFlightsButton.Click();

            Thread.Sleep(500);

            // Close the "Stops" filter
            stopsFilterButton.Click();
            _webDriver.FindElement(By.XPath("(//span[@jscontroller='rV7Ljf'][contains(.,'Es können optionale Gebühren und Gepäckgebühren anfallen. Informationen zur Passagierbetreuung.')])[1]")).Click();

            Thread.Sleep(500);

            // Filter nach abflugzeit
            IWebElement sortierButton = _webDriver.FindElement(By.XPath("//button[@aria-label='Nach beliebtesten Flügen sortiert, Sortierreihenfolge ändern.']"));
            sortierButton.Click();
            Thread.Sleep(500);
            IWebElement abflugzeit = _webDriver.FindElement(By.XPath("(//span[contains(@class,'VfPpkd-StrnGf-rymPhb-b9t22c')])[3]"));

            abflugzeit.Click();
            _webDriver.FindElement(By.XPath("(//span[@jscontroller='rV7Ljf'][contains(.,'Es können optionale Gebühren und Gepäckgebühren anfallen. Informationen zur Passagierbetreuung.')])[1]")).Click();

            Thread.Sleep(1000);
        }

        public List<FlightDTO> GetAllFlights(Airport originAirport, Airport destinationAirport, DateTime flightDate)
        {
            List<FlightDTO> FetchedFlights = [];

            try
            {
                SearchForFlights(originAirport, destinationAirport, flightDate);

                ReadOnlyCollection<IWebElement> listOfFlights = _webDriver.FindElements(By.XPath("(//div[contains(@class,'gQ6yfe m7VU8c')])"));

                foreach (IWebElement flightObj in listOfFlights)
                {

                    if (flightObj.Text.IsNullOrEmpty())
                        continue;

                    IWebElement detailButton = flightObj.FindElement(By.XPath(".//button[contains(@class, 'VfPpkd-LgbsSe VfPpkd-LgbsSe-OWXEXe-k8QpJ VfPpkd-LgbsSe-OWXEXe-Bz112c-M1Soyc VfPpkd-LgbsSe-OWXEXe-dgl2Hf nCP5yc AjY5Oe LQeN7 nJawce OTelKf')]"));
                    detailButton.Click();

                    Thread.Sleep(500);

                    // departure time
                    string fullTextDepart = flightObj.FindElement(By.XPath(".//div[contains(@class, 'dPzsIb AdWm1c y52p7d QS0io')]")).Text;
                    string departureTimeElement = Regex.Match(fullTextDepart, @"^\d{2}:\d{2}").Value;
                    DateTime departureTime = DateTime.ParseExact(departureTimeElement.Trim(), "HH:mm", null);

                    // arrival time
                    string fullTextArrival = flightObj.FindElement(By.XPath(".//div[contains(@class, 'SWFQlc AdWm1c y52p7d QS0io')]")).Text;
                    string arrivalTimeElement = Regex.Match(fullTextArrival, @"^\d{2}:\d{2}").Value;
                    DateTime arrivalTime = DateTime.ParseExact(arrivalTimeElement.Trim(), "HH:mm", null);

                    // flight number
                    IWebElement flightNumberElement = flightObj.FindElement(By.XPath(".//span[contains(@class, 'Xsgmwe QS0io')]"));
                    string flightNumber = flightNumberElement.Text.Trim();

                    // price 
                    string price = flightObj.FindElement(By.XPath(".//div[contains(@class, 'BVAVmf I11szd Qr8X4d')]//div[contains(@class, 'YMlIz FpEdX')]/span")).Text;
                    int number = int.Parse(Regex.Match(price, @"\d+").Value);

                    FlightDTO flight = new FlightDTO
                    {
                        Origin = new AirportDTO() { Code = originAirport.IATA, Name = originAirport.Name },
                        Destination = new AirportDTO() { Code = destinationAirport.IATA, Name = destinationAirport.Name },
                        FlightDepartureTime = new DateTime(flightDate.Year, flightDate.Month, flightDate.Day, departureTime.Hour, departureTime.Minute, 0),
                        FlightArrivalTime = new DateTime(flightDate.Year, flightDate.Month, flightDate.Day, arrivalTime.Hour, arrivalTime.Minute, 0),
                        FlightNumber = flightNumber,
                        Price = number
                    };

                    FetchedFlights.Add(flight);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (_quit)
                    _webDriver.Quit();
            }
            return FetchedFlights;
        }

        public FlightDTO GetSpecificFlight(Airport originAirport, Airport destinationAirport, DateTime flightDate, string flightNumber)
        {
            FlightDTO flightDTO = new FlightDTO()
            {
                Origin = new AirportDTO()
                {
                    Code = originAirport.IATA,
                    Name = originAirport.Name
                },
                Destination = new AirportDTO()
                {
                    Code = destinationAirport.IATA,
                    Name = destinationAirport.Name
                },
            };

            try
            {
                List<FlightDTO> allFlights = GetAllFlights(originAirport, destinationAirport, flightDate);

                FlightDTO? findFLight = allFlights.Find(_ => _.FlightNumber.Equals(flightNumber));

                if (findFLight != null)
                {
                    return findFLight;
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (_quit)
                    _webDriver.Quit();
            }
            return flightDTO;
        }

        public List<DayPrice> GetSpeGetCheapestMostExpensiveDateWithFlexibilitycificFlight(Airport originAirport, Airport destinationAirport, DateTime flightDate, string flightNumber, int flexibility)
        {
            List<DayPrice> dayPrices = new List<DayPrice>();

            try
            {
                for (int i = -flexibility; i <= flexibility; i++)
                {
                    DateTime date = flightDate.AddDays(i);
                    if (date > DateTime.Now)
                    {
                        _quit = false;
                        FlightDTO flightDetails = GetSpecificFlight(originAirport, destinationAirport, date, flightNumber);
                        dayPrices.Add(FlightDataToDayPrice(flightDetails));
                        _webDriver.Navigate().GoToUrl("https://www.google.com/travel/flights");
                    }
                }
                return dayPrices;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _quit = true;
                _webDriver.Quit();
            }
        }

        #region HelperMethods
        private static DayPrice FlightDataToDayPrice(FlightDTO flightData)
        {
            return new DayPrice
            {
                Day = flightData.FlightDepartureTime,
                Avg = flightData.Price
            };
        }

        #endregion
    }
}
