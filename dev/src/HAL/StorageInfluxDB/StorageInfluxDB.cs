using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HAL.Plugin;
using InfluxDB.LineProtocol.Client;
using InfluxDB.LineProtocol.Payload;
using Newtonsoft.Json;

namespace HAL.Storage
{
    public class StorageInfluxDB : IStoragePlugin, IDatabaseStorage
    {
        private LineProtocolClient client;
        private string influxdbUri;

        public void Init(string connectionString)
        {
            influxdbUri = connectionString;
            client = new LineProtocolClient(new Uri(influxdbUri), "hal_data");
        }

        public async Task<StorageCode> Save<T>(APlugin plugin, T obj)
        {
            var resultProtocol = new LineProtocolPoint(
                plugin.Infos.Name,
                new Dictionary<string, object>
                {
                    {"result", JsonConvert.DeserializeObject<dynamic>(obj.ToString())}
                }
            );

            var payload = new LineProtocolPayload();
            payload.Add(resultProtocol);

            var influxResult = await client.WriteAsync(payload);

            if (influxResult.Success)
                return StorageCode.Success;
            return StorageCode.Failed;
        }

        public void Dispose()
        {
        }
    }
}