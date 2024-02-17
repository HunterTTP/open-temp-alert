﻿using open_dust_monitor.models;
using open_dust_monitor.repositories;
using open_dust_monitor.src.Handler;

namespace open_dust_monitor.services
{
    public class TemperatureService
    {
        private readonly HardwareService _hardwareService = InstanceHandler.GetHardwareService();
        private readonly TemperatureRepository _temperatureRepository = InstanceHandler.GetTemperatureRepository();

        public TemperatureSnapshot GetLatestTemperatureSnapshot()
        {
            var dateTime = DateTime.Now;
            var cpuName = _hardwareService.GetCpuName();
            var cpuTemperature = _hardwareService.GetCurrentCpuTemperature();
            var cpuLoad = _hardwareService.GetCurrentCpuLoad();
            var cpuLoadRange = _hardwareService.GetCurrentCpuLoadRange(cpuLoad);
            var snapshot = new TemperatureSnapshot(dateTime, cpuName, cpuTemperature, cpuLoad, cpuLoadRange);
            _temperatureRepository.SaveTemperatureSnapshot(snapshot);
            return snapshot;
        }

        public bool IsRecentAverageTemperatureWithinThreshold()
        {
            return GetRecentAverageTemperature() <= GetAlertThresholdTemperature();
        }

        public float GetRecentAverageTemperature()
        {
            var temperatureSnapshots = _temperatureRepository.GetLoadedTemperatureSnapshots();
            var endDate = temperatureSnapshots.Max(snapshot => snapshot.Timestamp).AddDays(-7);
            var recentAverageTemperature = temperatureSnapshots
                .Where(snapshot => snapshot.Timestamp >= endDate)
                .Select(snapshot => snapshot.CpuTemperature)
                .DefaultIfEmpty(0)
                .Average();
            return (float)Math.Round(recentAverageTemperature);
        }

        public float GetAlertThresholdTemperature()
        {
            return (float)Math.Round(GetBaselineTemperature() + 5f);
        }

        private float GetBaselineTemperature()
        {
            var temperatureSnapshots = _temperatureRepository.GetLoadedTemperatureSnapshots();
            var endDate = temperatureSnapshots.Min(snapshot => snapshot.Timestamp).AddDays(7);
            return temperatureSnapshots
                .Where(snapshot => snapshot.Timestamp <= endDate)
                .Select(snapshot => snapshot.CpuTemperature)
                .DefaultIfEmpty(0)
                .Average();
        }

        internal string GetTemperatureSnapshotLabel(TemperatureSnapshot snapshot, int timerInterval)
        {
            return
                "Latest Snapshot:" +
                "\n Timestamp: " + snapshot.Timestamp +
                "\n CPU: " + snapshot.CpuName +
                "\n Temperature: " + snapshot.CpuTemperature + "°C" +
                "\n Utilization: " + snapshot.CpuLoad + "%" +
                "\n" +
                "\nKey Variables:" +
                "\n alertThresholdTemperature: " + GetAlertThresholdTemperature() + "°C" +
                "\n recentAverageTemperature: " + GetRecentAverageTemperature() + "°C" +
                "\n recentAverageIsOk: " + IsRecentAverageTemperatureWithinThreshold().ToString() +
                "\n idleSnapshotCount: " + _temperatureRepository.GetLoadedIdleSnapshots().Count() +
                "\n lowSnapshotCount: " + _temperatureRepository.GetLoadedLowSnapshots().Count() +
                "\n mediumSnapshotCount: " + _temperatureRepository.GetLoadedMediumSnapshots().Count() +
                "\n highSnapshotCount: " + _temperatureRepository.GetLoadedHighSnapshots().Count() +
                "\n maxSnapshotCount: " + _temperatureRepository.GetLoadedHighSnapshots().Count() +
                "\n totalSnapshotCount: " + _temperatureRepository.GetLoadedTemperatureSnapshots().Count() +
                "\n snapshotFrequency: " + timerInterval / 1000 + " seconds";
        }

        public void StopTemperatureMonitoring()
        {
            _hardwareService.StopHardwareMonitoring();
        }
    }
}