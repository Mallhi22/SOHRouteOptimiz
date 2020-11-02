using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Mars.Common.Logging;
using Mars.Common.Logging.Enums;
using Mars.Components.Starter;
using Mars.Core.Model.Entities;
using Mars.Core.Simulation;
using Mars.Interfaces;
using SOHCarModel.Model;
using SOHCarModel.Parking;
using SOHMultimodalModel.Model;
using SOHMultimodalModel.Output.Trips;
using SOHMultimodalModel.Layers.TrafficLight;


namespace SOHRouteOptimiz
{
    /// <summary>
    /// Here we are trying to simulate our RouteOptimiz project with external default simulation configuration with CSV output and trips.
    /// </summary>
    ///
    /// 
    internal static class Program
    {
        private static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("EN-US");
            LoggerFactory.SetLogLevel(LogLevel.Off);

            var description = new ModelDescription();
           
            description.AddLayer<CarLayer>();
            description.AddLayer<CarParkingLayer>();
            description.AddLayer<TrafficLightLayer>();
            description.AddLayer<TravelerLayer>(); 


            description.AddAgent<Traveler,TravelerLayer >();
            description.AddEntity<Car>();


            ISimulationContainer application;

            if (args != null && args.Any())
            {
                application = SimulationStarter.BuildApplication(description, args);
            }
            else
            {
                var config = CreateDefaultConfig();
                application = SimulationStarter.BuildApplication(description, config);
            }

            var simulation = application.Resolve<ISimulation>();

            var watch = Stopwatch.StartNew();
            var state = simulation.StartSimulation();

            var layers = state.Model.Layers;


            foreach (var layer in layers)
            {
                if (layer.Value is TravelerLayer travelerLayer)
                {
                    TripsOutputAdapter.PrintTripResult(travelerLayer.Travelers.Values);
                }
            }

            watch.Stop();

            Console.WriteLine($"Executed iterations {state.Iterations} lasted {watch.Elapsed}");
            application.Dispose();
        }
    


    private static SimulationConfig CreateDefaultConfig()
        {
            var startPoint = DateTime.Parse("2020-01-01T04:00:00");
            var suffix = DateTime.Now.ToString("yyyyMMddHHmm");
            var config = new SimulationConfig
            {
                SimulationIdentifier = "RouteOptimiz",
                Globals =
                {
                    StartPoint = startPoint,
                    EndPoint = startPoint + TimeSpan.FromHours(3),
                    DeltaTUnit = TimeSpanUnit.Seconds,
                    ShowConsoleProgress = true,
                    OutputTarget = OutputTargetType.None
                },
                LayerMappings =
                {
                    new LayerMapping
                    {
                        Name = nameof(CarLayer),
                        File = Path.Combine("resources", "harburg_zentrum_drive_graph.geojson")
                    },
                    new LayerMapping
                    {
                        Name = nameof(TrafficLightLayer),
                        File = Path.Combine("resources", "traffic_lights_harburg_zentrum.geojson")
                    },
                    new LayerMapping
                    {
                        Name = nameof(CarParkingLayer),
                        File = Path.Combine("resources", "Parking_Harburg_zentrum.geojson")
                    },
                    new LayerMapping
                    {
                        Name = nameof(TravelerLayer),
                        File = Path.Combine("resources", "OneCar.csv")
                    }
                },
                AgentMappings =
                {
                    new AgentMapping
                    {
                        Name = nameof(Traveler),
                        OutputTarget = OutputTargetType.None,
                        IndividualMapping =
                        {
                            new IndividualMapping {Name = "start", Value = (10.025595724582672,53.56711503998041)},
                            new IndividualMapping {Name = "Goal", Value = (10.027395486831665,53.58051568068171)},
                            new IndividualMapping {Name = "TravelCapabilities", Value = true},
                            
                        }
                    }
                },
                EntityMappings =
                {
                    new EntityMapping
                    {
                        Name = nameof(Car),
                        File = Path.Combine("resources", "car.csv")
                    }
                }
            };
            return config;
        }
    }

    
}