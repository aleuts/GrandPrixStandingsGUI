﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GPStandingsGUI.Controllers
{
    class APIController
    {
        private Models.ConstructorModel constructorModel;
        private Models.DriverModel driverModel;

        private readonly Models.APIHelper apiHelper;
        
        private readonly string apiURL;

        public APIController(int year, string table)
        {
            this.constructorModel = new Models.ConstructorModel();
            this.driverModel = new Models.DriverModel();

            this.apiHelper = new Models.APIHelper();
            this.apiHelper.InitializeClient();

            this.apiURL = $"https://ergast.com/api/f1/{year}/{table}.json";
        }

        public async Task<Tuple<string, ObservableCollection<Models.IStandingsCollection>>> ConstructorsStandings()
        {
            ObservableCollection<Models.IStandingsCollection> standingsCollection = new ObservableCollection<Models.IStandingsCollection>();
            
            this.constructorModel = await this.GetModelData(this.constructorModel, this.apiURL);

            string heading =
                $"{this.constructorModel.MRData.StandingsTable.StandingsLists[0].season} Formula 1 Season - {this.constructorModel.MRData.StandingsTable.StandingsLists[0].round} Rounds";

            foreach (Models.ConstructorModel.Constructorstanding constructor in this.constructorModel.MRData.StandingsTable.StandingsLists[0].ConstructorStandings)
            {
                standingsCollection.Add(new Models.ConstructorsStandingsCollection()
                {
                    Position = int.Parse(constructor.position),
                    Constructor = constructor.Constructor.name,
                    Points = float.Parse(constructor.points),
                    Wins = int.Parse(constructor.wins),
                    Nationality = constructor.Constructor.nationality
                });
            }

            return new Tuple<string, ObservableCollection<Models.IStandingsCollection>>(heading, standingsCollection);

        }

        public async Task<Tuple<string, ObservableCollection<Models.IStandingsCollection>>> DriversStandings()
        {
            ObservableCollection<Models.IStandingsCollection> standingsCollection = new ObservableCollection<Models.IStandingsCollection>();
            
            this.driverModel = await this.GetModelData(this.driverModel, this.apiURL);

            string heading =
                $"{this.driverModel.MRData.StandingsTable.StandingsLists[0].season} Formula 1 Season - {driverModel.MRData.StandingsTable.StandingsLists[0].round} Rounds";

            foreach (Models.DriverModel.Driverstanding driver in this.driverModel.MRData.StandingsTable.StandingsLists[0].DriverStandings)
            {
                standingsCollection.Add(new Models.DriversStandingsCollection()
                {
                    Position = int.Parse(driver.position),
                    Driver =
                        $"{driver.Driver.givenName[0]}. {driver.Driver.familyName} ({(driver.Driver.permanentNumber != null ? driver.Driver.permanentNumber : "--")})",
                    Points = float.Parse(driver.points),
                    Wins = int.Parse(driver.wins),
                    Constructor = driver.Constructors[0].name,
                    Nationality = driver.Driver.nationality
                });
            }

            return new Tuple<string, ObservableCollection<Models.IStandingsCollection>>(heading, standingsCollection);

        }

        // async stops app freezing by running process similanteously.
        private async Task<T> GetModelData<T>(T type, string url)
        {
            // make a new request from api client and wait for reponse then dispose.
            using (HttpResponseMessage apiResponse = await this.apiHelper.ApiClient.GetAsync(url))
            {
                if (apiResponse.IsSuccessStatusCode)
                {
                    // Using newton converter to take specified json data and convert to specified type (DriverStandings properies)
                    // By calling .Result you are synchronously reading the result,
                    T modelData = await apiResponse.Content.ReadAsAsync<T>();

                    return modelData;
                }
                else
                {
                    throw new Exception(apiResponse.ReasonPhrase);
                }
            }
        }
    }
}
