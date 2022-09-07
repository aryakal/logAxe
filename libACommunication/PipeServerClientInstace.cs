//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using libALogger;
using Newtonsoft.Json;


namespace libACommunication
{
   class PipeServerClientInstace : IPLCommunication
   {
      ILibALogger _logger;
      IProtoProcessorCommand _processor;
      PipeStream _stream;
      StreamWriter _writer;
      SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
      public IClientInfo ID { get; private set; }
      public PipeServerClientInstace(ILibALogger logger, IClientInfo id, PipeStream stream, IProtoProcessorCommand processor)
      {
         ID = id;
         _logger = logger;
         _stream = stream;
         _processor = processor;
      }
      public async Task Send(string msg)
      {
         await SendOpCode(1, msg);
      }
      byte[] _packetHeader = new byte[10];
      public async Task SendOpCode(int command, string payload)
      {

         try
         {

            _lock.Wait();
            {

               var buf = Encoding.UTF8.GetBytes(payload);
               byte[] packetSize = BitConverter.GetBytes(buf.Length);

               if (!_stream.IsConnected)
               {
                  _logger?.Error("Sending on disconnected");
               }

               _packetHeader[0] = 0xf;
               _packetHeader[1] = 0xe;

               _packetHeader[2] = (byte)(command / 256);
               _packetHeader[3] = (byte)(command & 255);

               _packetHeader[4] = 0xf;


               _packetHeader[5] = packetSize[0];
               _packetHeader[6] = packetSize[1];
               _packetHeader[7] = packetSize[2];
               _packetHeader[8] = packetSize[3];

               _packetHeader[9] = 0xf;


               await _stream.WriteAsync(_packetHeader, 0, _packetHeader.Length);
               await _stream.WriteAsync(buf, 0, buf.Length);
               //LogBuffer(">", command, payload);


            }
         }
         finally
         {
            _lock.Release();
         }
      }
      public async Task Send(UnitMsg msg)
      {
         if (msg == null)
         {
            return;
         }
         await Send(JsonConvert.SerializeObject(msg, Formatting.None));
      }
      private async Task ProcessResponse(int command, string payload)
      {

         try
         {
            //LogBuffer("<", command, payload);

            var message = JsonConvert.DeserializeObject<UnitMsg>(payload);
            await Send(_processor.ProcessUnitCmd(LibCommProtoMsgType.Msg, ID, message));
         }
         catch (Exception ex)
         {
            _logger?.Error("Error in processing request, " + ex.ToString());
         }
      }
      public Task Run(CancellationToken token)
      {
         _logger?.Info($"starting");
         return Task.Run(async () =>
         {
            try
            {
               _processor?.ProcessUnitCmd(LibCommProtoMsgType.Connected, ID, null);
               var payload = new byte[1024 * 1024];
               var header = new byte[100];
               int packetLen = 0;
               int readData = 0;
               int transportCommand = 0;
               _logger?.Debug("Starting incomming queue");
               while (_stream.IsConnected)
               {

                  readData = _stream.Read(header, 0, 10);
                  if (readData != 10)
                  {
                     _logger?.Error("Rejecting packet");
                     continue;
                  }
                  transportCommand = BitConverter.ToUInt16(header, 2);
                  packetLen = BitConverter.ToInt32(header, 5);

                  readData = _stream.Read(payload, 0, payload.Length);
                  if (readData != packetLen)
                  {
                     _logger?.Error($"Rejecting packet {transportCommand}, {packetLen}, {readData}");
                     continue;
                  }

                  await ProcessResponse(transportCommand, Encoding.UTF8.GetString(payload, 0, readData));
               }
               _stream.Close();

            }
            catch (Exception ex)
            {
               _logger?.Error(ex.ToString());
            }
            finally
            {
               _processor?.ProcessUnitCmd(LibCommProtoMsgType.Disconnected, ID, null);
            }
         });
      }
      public void RunForever(CancellationToken token)
      {
         Run(token).Wait();
      }
      private void LogBuffer(string dir, int command, string payload)
      {
         if (string.IsNullOrEmpty(payload))
         {
            return;
         }
         if (payload.Length >= 150)
         {
            _logger?.Debug($"{dir} , {command}, {payload.Substring(0, 100)}");
         }
         else
         {
            _logger?.Debug($"{dir} , {command}, {payload}");
         }
      }
   }
}
