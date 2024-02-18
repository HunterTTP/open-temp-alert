﻿using open_dust_monitor.models;

namespace open_dust_monitor.repositories
{
    public class TemperatureRepository
    {
        private static readonly string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string _pathToTemperatureHistoryCsv = Path.Combine(baseDirectory, "temperature_history.csv");
        private List<TemperatureSnapshot> loadedIdleSnapshots = [];
        private List<TemperatureSnapshot> loadedLowSnapshots = [];
        private List<TemperatureSnapshot> loadedMediumSnapshots = [];
        private List<TemperatureSnapshot> loadedHighSnapshots = [];
        private List<TemperatureSnapshot> loadedMaxSnapshots = [];

        public TemperatureRepository()
        {
            EnsureTemperatureHistoryCsvExists();
            LoadTemperatureSnapshots(GetAllTemperatureSnapshotsFromCsv());
        }

        private static void EnsureTemperatureHistoryCsvExists()
        {
            if (!File.Exists(_pathToTemperatureHistoryCsv))
                try
                {
                    var newTemperatureHistoryCsv = File.Create(_pathToTemperatureHistoryCsv);
                    var historyCsvWriter = new StreamWriter(newTemperatureHistoryCsv);
                    historyCsvWriter.WriteLine(TemperatureSnapshot.GetCsvRowHeaders());
                    historyCsvWriter.Close();
                }
                catch (Exception ex)
                {
                    throw new FileLoadException("Could not create TemperatureHistoryCsv: " + ex);
                }
        }

        public void LoadTemperatureSnapshots(List<TemperatureSnapshot> snapshots)
        {
            loadedIdleSnapshots = snapshots.Where(snapshot => snapshot.CpuLoadRange == "idle").ToList();
            loadedLowSnapshots = snapshots.Where(snapshot => snapshot.CpuLoadRange == "low").ToList();
            loadedMediumSnapshots = snapshots.Where(snapshot => snapshot.CpuLoadRange == "medium").ToList();
            loadedHighSnapshots = snapshots.Where(snapshot => snapshot.CpuLoadRange == "high").ToList();
            loadedMaxSnapshots = snapshots.Where(snapshot => snapshot.CpuLoadRange == "max").ToList();
        }

        public void LoadTemperatureSnapshot(TemperatureSnapshot snapshot)
        {
            if (snapshot.CpuLoadRange.Equals("idle"))
            {
                loadedIdleSnapshots.Add(snapshot);
            }
            else if (snapshot.CpuLoadRange.Equals("low"))
            {
                loadedLowSnapshots.Add(snapshot);
            }
            else if (snapshot.CpuLoadRange.Equals("medium"))
            {
                loadedMediumSnapshots.Add(snapshot);
            }
            else if (snapshot.CpuLoadRange.Equals("high"))
            {
                loadedHighSnapshots.Add(snapshot);
            }
            else if (snapshot.CpuLoadRange.Equals("max"))
            {
                loadedMaxSnapshots.Add(snapshot);
            }
        }

        public List<TemperatureSnapshot> GetLoadedIdleSnapshots()
        {
            return loadedIdleSnapshots;
        }

        public List<TemperatureSnapshot> GetLoadedLowSnapshots()
        {
            return loadedLowSnapshots;
        }

        public List<TemperatureSnapshot> GetLoadedMediumSnapshots()
        {
            return loadedMediumSnapshots;
        }

        public List<TemperatureSnapshot> GetLoadedHighSnapshots()
        {
            return loadedHighSnapshots;
        }

        public List<TemperatureSnapshot> GetLoadedMaxSnapshots()
        {
            return loadedMaxSnapshots;
        }

        public void SaveTemperatureSnapshot(TemperatureSnapshot snapshot)
        {
            LoadTemperatureSnapshot(snapshot);
            using (var csvAppender = File.AppendText(_pathToTemperatureHistoryCsv))
            {
                csvAppender.WriteLine(snapshot.GetAsCsvRow());
            }
        }

        public List<TemperatureSnapshot> GetAllTemperatureSnapshotsFromCsv()
        {
            var temperatureSnapshots = new List<TemperatureSnapshot>();
            using (var reader = new StreamReader(_pathToTemperatureHistoryCsv))
            {
                reader.ReadLine(); //skip header
                string csvRow;
                while ((csvRow = reader.ReadLine()) != null)
                {
                    var csvRowValues = csvRow.Split(',');
                    var temperatureSnapshot = MapCsvRowToTemperatureSnapshot(csvRowValues);
                    temperatureSnapshots.Add(temperatureSnapshot);
                }
            }

            return temperatureSnapshots;
        }

        private static TemperatureSnapshot MapCsvRowToTemperatureSnapshot(string[] csvRowValues)
        {
            var dateTime = DateTime.Parse(csvRowValues[0]);
            var cpuName = csvRowValues[1];
            var cpuTemperature = float.Parse(csvRowValues[2]);
            var cpuLoad = float.Parse(csvRowValues[3]);
            var cpuLoadRange = csvRowValues[4];
            return new TemperatureSnapshot(dateTime, cpuName, cpuTemperature, cpuLoad, cpuLoadRange);
        }

        public List<TemperatureSnapshot> GetLoadedTemperatureSnapshots()
        {
            return loadedIdleSnapshots
                .Concat(loadedLowSnapshots)
                .Concat(loadedMediumSnapshots)
                .Concat(loadedHighSnapshots)
                .Concat(loadedMaxSnapshots)
                .ToList();
        }

        public int GetLoadedTemperatureSnapshotsCount()
        {
            return loadedIdleSnapshots.Count
                + loadedLowSnapshots.Count
                + loadedMediumSnapshots.Count
                + loadedHighSnapshots.Count
                + loadedMaxSnapshots.Count;
        }
    }
}